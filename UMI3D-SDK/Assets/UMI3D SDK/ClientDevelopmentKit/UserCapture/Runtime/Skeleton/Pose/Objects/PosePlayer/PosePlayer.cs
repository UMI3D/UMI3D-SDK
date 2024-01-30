/*
Copyright 2019 - 2024 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/


using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Player that applies a pose and gives the values corresponding to it. Ensure the interpolation if needed.
    /// </summary>
    public class PosePlayer : IPosePlayer
    {
        /// <summary>
        /// Pose clip handled by the player.
        /// </summary>
        public PoseClip PoseClip { get; private set; }

        /// <summary>
        /// Complete skeleton affected by the player.
        /// </summary>
        public ISkeleton Skeleton { get; private set; }

        /// <summary>
        /// True if the player is currently applying a pose.
        /// </summary>
        /// Note that this value is still true after some time 
        /// after calling <see cref="EndPoseClip(bool)"/> because of end of application handling.
        public bool IsPlaying { get; private set; } = false;

        /// <summary>
        /// True when the player is in the ending interpolation phase.
        /// </summary>
        public bool IsEnding { get; private set; } = false;

        protected PosePlayingData playingData;

        /// <summary>
        /// Parameters used to request a specific application of the pose.
        /// </summary>
        public class PlayingParameters
        {
            /// <summary>
            /// True if pose is meant to be anchored.
            /// </summary>
            public bool isAnchored;

            /// <summary>
            /// Specified anchor when pose is anchored.
            /// </summary>
            public PoseAnchorDto anchor;

            /// <summary>
            /// Duration (in seconds) of the transition when pose is starting to be applied.
            /// </summary>
            public float startTransitionDuration = 0.25f;

            /// <summary>
            /// Duration (in seconds) of the transition when pose is ending to be applied.
            /// </summary>
            public float endTransitionDuration = 0.25f;
        }

        protected struct PosePlayingData
        {
            public readonly float startTime;
            public readonly PlayingParameters parameters;
            public float endAskedTime;

            public PosePlayingData(float startTime, PlayingParameters parameters)
            {
                this.startTime = startTime;
                this.parameters = parameters;
                endAskedTime = -1f;
            }
        }

        #region DI

        private readonly ICoroutineService coroutineService;

        public PosePlayer(PoseClip poseClip, ISkeleton parentSkeleton, ICoroutineService coroutineService)
        {
            PoseClip = poseClip ?? throw new System.ArgumentNullException(nameof(PoseClip));
            this.Skeleton = parentSkeleton ?? throw new System.ArgumentNullException(nameof(parentSkeleton));
            this.coroutineService = coroutineService;
        }

        public PosePlayer(PoseClip poseClip, ISkeleton parentSkeleton) : this(poseClip, parentSkeleton, CoroutineManager.Instance)
        {
        }

        #endregion DI

        /// <inheritdoc/>
        public void PlayPoseClip(PlayingParameters parameters = null)
        {
            if (IsPlaying)
                return;

            IsPlaying = true;

            playingData = new(UnityEngine.Time.time, parameters ?? new());

            if (!playingData.parameters.isAnchored && PoseClip.Pose.anchor != null)
            {
                playingData.parameters.isAnchored = true;
                playingData.parameters.anchor = PoseClip.Pose.anchor;
            }

            if (playingData.parameters.isAnchored)
                Skeleton.TrackedSubskeleton.StartTrackerSimulation(playingData.parameters.anchor);
        }

        /// <inheritdoc/>
        public void EndPoseClip(bool shouldStopImmediate = false)
        {
            if (!IsPlaying)
                return;

            if (!shouldStopImmediate && PoseClip.IsInterpolable)
            {
                IsEnding = true;
                playingData.endAskedTime = Time.time;
                endPoseClipRoutine = coroutineService.AttachCoroutine(EndPoseClipRoutine());
            }
            else
            {
                if (IsEnding) // force end while ending
                {
                    IsEnding = false;
                    coroutineService.DetachCoroutine(endPoseClipRoutine);
                    endPoseClipRoutine = null;
                }
                StopPoseClip();
            }
        }

        private Coroutine endPoseClipRoutine;

        /// <summary>
        /// Coroutine handling the waiting for the ending phase.
        /// </summary>
        /// <returns></returns>
        private IEnumerator EndPoseClipRoutine()
        {
            yield return new WaitForSeconds(playingData.parameters.endTransitionDuration);
            IsEnding = false;
            StopPoseClip();
            coroutineService.DetachCoroutine(endPoseClipRoutine);
            endPoseClipRoutine = null;
        }

        private void StopPoseClip()
        {
            if (playingData.parameters.isAnchored)
                Skeleton.TrackedSubskeleton.StopTrackerSimulation(playingData.parameters.anchor);

            IsPlaying = false;
            playingData = default;
        }

        /// <inheritdoc/>
        public SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            if (!IsPlaying)
                return null;

            Dictionary<uint, SubSkeletonBoneDto> bonePoses = new();

            foreach (var bone in PoseClip.Bones)
            {
                SubSkeletonBoneDto bonePose = GetBonePose(hierarchy, bone, PoseClip).subBone;

                if (PoseClip.IsInterpolable)
                {
                    if (!IsEnding)
                        bonePose.localRotation = Quaternion.Slerp(Skeleton.Bones[bone.boneType].LocalRotation, bonePose.localRotation.Quaternion(), (Time.time - playingData.startTime) / playingData.parameters.startTransitionDuration).Dto();
                    else
                        bonePose.localRotation = Quaternion.Slerp(bonePose.localRotation.Quaternion(), Skeleton.Bones[bone.boneType].LocalRotation, (Time.time - playingData.endAskedTime) / playingData.parameters.endTransitionDuration).Dto();
                }

                bonePoses[bone.boneType] = bonePose;
            }

            computedMap.Clear();

            return new SubSkeletonPoseDto()
            {
                bones = bonePoses.Values.ToList()
            };
        }

        private Dictionary<uint, (BoneDto bone, SubSkeletonBoneDto subBone)> computedMap = new();

        /// <summary>
        /// Recursively compute local rotation for a bone.
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="boneDto"></param>
        /// <param name="pose"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private (BoneDto bone, SubSkeletonBoneDto subBone) GetBonePose(UMI3DSkeletonHierarchy hierarchy, BoneDto boneDto, PoseClip pose)
        {
            if (boneDto == null)
                throw new ArgumentNullException(nameof(boneDto));

            uint boneType = boneDto.boneType;

            if (computedMap.ContainsKey(boneType))
                return computedMap[boneType];

            if (!hierarchy.Relations.ContainsKey(boneType))
                throw new ArgumentException($"Bone ({boneType}, \"{BoneTypeHelper.GetBoneName(boneType)}\") not defined in hierarchy.", nameof(boneDto));

            var relation = hierarchy.Relations[boneType];

            var parentBone = pose.Bones.Find(b => b.boneType == relation.boneTypeParent);

            SubSkeletonBoneDto subBone = new() { boneType = boneType };
            if (parentBone == default || parentBone.boneType == BoneType.None) // bone has no parent
            {
                subBone.localRotation = boneDto.rotation;
            }
            else // bone has a parent and thus its rotation depends on it
            {
                var parent = GetBonePose(hierarchy, parentBone, pose);
                subBone.localRotation = (Quaternion.Inverse(parent.bone.rotation.Quaternion()) * boneDto.rotation.Quaternion()).Dto();
            }

            computedMap[boneType] = new()
            {
                bone = boneDto,
                subBone = subBone
            };

            return computedMap[boneType];
        }

    }
}
