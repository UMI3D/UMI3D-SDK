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
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using umi3d.common.userCapture;
using umi3d.common.userCapture.description;

using UnityEngine;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Compute a subskeleton pose from a subskeleton descriptor.
    /// </summary>
    /// Mainly used for interpolations.
    public class SubskeletonDescriptionInterpolationPlayer : ISubskeletonDescriptionInterpolationPlayer
    {
        /// <summary>
        /// Complete skeleton affected by the player.
        /// </summary>
        public ISkeleton Skeleton { get; protected set; }

        /// <summary>
        /// True if the player is currently applying a pose.
        /// </summary>
        /// Note that this value is still true after some time 
        /// after calling <see cref="End(bool)"/> because of end of application handling.
        public bool IsPlaying { get; protected set; } = false;

        /// <summary>
        /// True when the player is in the ending interpolation phase.
        /// </summary>
        public bool IsEnding { get; protected set; } = false;

        public bool ShouldInterpolate => shouldInterpolate;

        protected bool shouldInterpolate;

        public ISubskeletonDescriptor Descriptor { get; protected set; }

        protected PlayingData playingData;

        protected struct PlayingData
        {
            public readonly float startTime;
            public readonly ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters;
            public float endAskedTime;

            public PlayingData(float startTime, ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters)
            {
                this.startTime = startTime;
                this.parameters = parameters;
                endAskedTime = -1f;
            }
        }

        #region DI

        private readonly ICoroutineService coroutineService;


        public SubskeletonDescriptionInterpolationPlayer(ISubskeletonDescriptor descriptor, bool shouldInterpolate, ISkeleton parentSkeleton, ICoroutineService coroutineService)
        {
            Descriptor = descriptor ?? throw new System.ArgumentNullException(nameof(descriptor));
            this.shouldInterpolate = shouldInterpolate;
            this.Skeleton = parentSkeleton ?? throw new System.ArgumentNullException(nameof(parentSkeleton));
            this.coroutineService = coroutineService;
        }

        public SubskeletonDescriptionInterpolationPlayer(ISubskeletonDescriptor descriptor, bool shouldInterpolate, ISkeleton parentSkeleton) : this(descriptor, shouldInterpolate, parentSkeleton, CoroutineManager.Instance)
        {
        }

        #endregion DI

        public void Play()
        {
            Play(null);
        }

        /// <inheritdoc/>
        public virtual void Play(ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = null)
        {
            if (IsPlaying)
                return;

            IsPlaying = true;

            playingData = new(UnityEngine.Time.time, parameters ?? new());
        }

        /// <inheritdoc/>
        public virtual void End(bool shouldStopImmediate = false)
        {
            if (!IsPlaying)
                return;

            if (!shouldStopImmediate && ShouldInterpolate)
            {
                IsEnding = true;
                playingData.endAskedTime = Time.time;
                endRoutine = coroutineService.AttachCoroutine(EndRoutine());
            }
            else
            {
                if (IsEnding) // force end while ending
                {
                    IsEnding = false;
                    coroutineService.DetachCoroutine(endRoutine);
                    endRoutine = null;
                }
                Stop();
            }
        }

        private Coroutine endRoutine;

        private SubSkeletonPoseDto InterpolateSwitchTransitionTo(UMI3DSkeletonHierarchy hierarchy, ISubskeletonDescriptor oldDescriptor, ISubskeletonDescriptor newDescriptor)
        {
            SubSkeletonPoseDto newDescription = newDescriptor.GetPose(hierarchy);

            SubSkeletonPoseDto currentDescription = oldDescriptor.GetPose(hierarchy);

            Dictionary<uint, SubSkeletonBoneDto> currentBonePoses = currentDescription.bones.ToDictionary(x => x.boneType, y => y);

            Dictionary<uint, SubSkeletonBoneDto> bonePoses = new ();

            float normalizedTransitionTime = transitionData.duration <= 0 ? 1 : (Time.time - transitionData.startTime) / transitionData.duration;
            foreach (SubSkeletonBoneDto newDescriptionBonePose in newDescription.bones)
            {
                uint boneType = newDescriptionBonePose.boneType;
                if (bonePoses.ContainsKey(boneType))
                    continue;

                if (currentBonePoses.ContainsKey(boneType)) // not defined in current pose so go on
                {
                    newDescriptionBonePose.localRotation = Quaternion.Slerp(currentBonePoses[boneType].localRotation.Quaternion(), newDescriptionBonePose.localRotation.Quaternion(), normalizedTransitionTime).Dto();
                }
                else
                {
                    if (ShouldInterpolate && boneType != BoneType.Hips && boneType != newDescription.boneAnchor?.bone)
                    {
                        if (!IsEnding) // start transition
                            newDescriptionBonePose.localRotation = Quaternion.Slerp(Skeleton.Bones[boneType].LocalRotation, newDescriptionBonePose.localRotation.Quaternion(), normalizedTransitionTime).Dto();
                        else // end transition
                            newDescriptionBonePose.localRotation = Quaternion.Slerp(newDescriptionBonePose.localRotation.Quaternion(), Skeleton.Bones[boneType].LocalRotation, normalizedTransitionTime).Dto();
                    }
                   
                }
                bonePoses[boneType] = newDescriptionBonePose;
            }

            return new SubSkeletonPoseDto()
            {
                boneAnchor = newDescription.boneAnchor,
                bones = bonePoses.Values.ToList()
            };
        }

        private bool IsInTransition;
        private Coroutine transitionCoroutine;
        private TransitionData transitionData;

        public event System.Action SwitchTransitionFinished;

        private record TransitionData
        {
            public ISubskeletonDescriptor newDescriptor;
            public float startTime;
            public float duration = 0.25f;
        }

        public void SwitchTo(ISubskeletonDescriptor newDescriptor, float transitionDuration = 0.25f)
        {
            IsInTransition = true;
            transitionData = new()
            {
                newDescriptor = newDescriptor,
                duration = transitionDuration,
                startTime = Time.time
            };
            transitionCoroutine = coroutineService.AttachCoroutine(TransitionRoutine(transitionDuration));
        }

        private IEnumerator TransitionRoutine(float transitionDuration)
        {
            yield return new WaitForSeconds(transitionDuration);
            IsInTransition = false;
            if (transitionCoroutine != null)
                coroutineService.DetachCoroutine(transitionCoroutine);
            transitionCoroutine = null;
            End(true);
            SwitchTransitionFinished?.Invoke();
        }

        /// <summary>
        /// Coroutine handling the waiting for the ending phase.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator EndRoutine()
        {
            yield return new WaitForSeconds(playingData.parameters.endTransitionDuration);
            IsEnding = false;
            Stop();
            coroutineService.DetachCoroutine(endRoutine);
            endRoutine = null;
        }

        protected virtual void Stop()
        {
            IsPlaying = false;
            playingData = default;
        }

        /// <inheritdoc/>
        public virtual SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            if (!IsPlaying)
                return null;

            SubSkeletonPoseDto pose = Descriptor.GetPose(hierarchy);

            if (IsInTransition)
                return InterpolateSwitchTransitionTo(hierarchy, Descriptor, transitionData.newDescriptor);

            if (pose == null
                || !ShouldInterpolate 
                || (!IsEnding && (Time.time - playingData.startTime) > playingData.parameters.startTransitionDuration)) // no interpolation needed
                return pose;

            return InterpolateTransitionPose(pose);
        }

        /// <summary>
        /// Interpolate pose with the current pose of the parent skeleton.
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        protected SubSkeletonPoseDto InterpolateTransitionPose(SubSkeletonPoseDto pose)
        {
            PoseAnchorDto anchor = pose.boneAnchor;

            float tEnterPhase = playingData.parameters.startTransitionDuration <= 0 ? 1 : (Time.time - playingData.startTime) / playingData.parameters.startTransitionDuration;
            float tEndPhase = playingData.parameters.endTransitionDuration <= 0 ? 1 : (Time.time - playingData.endAskedTime) / playingData.parameters.endTransitionDuration;

            // manages Hips position interpolation if it is the anchor
            if (anchor != null && anchor.bone == BoneType.Hips)
            {
                Vector3 skeletonHipsPosition = new(0, Skeleton.Bones[BoneType.Hips].Position.y, 0);
                if (!IsEnding) // start transition
                {
                    anchor.position = Vector3.Lerp(skeletonHipsPosition, anchor.position.Struct(), tEnterPhase).Dto();
                    anchor.rotation = Quaternion.Slerp(Skeleton.Bones[BoneType.Hips].Rotation, anchor.rotation.Quaternion(), tEnterPhase).Dto();
                }
                else // end transition
                {
                    anchor.position = Vector3.Lerp(anchor.position.Struct(), skeletonHipsPosition, tEndPhase).Dto();
                    anchor.rotation = Quaternion.Slerp(anchor.rotation.Quaternion(), Skeleton.Bones[BoneType.Hips].Rotation, tEndPhase).Dto();
                }
            }

            // manages local rotation interpolation for all bones of the poses
            Dictionary<uint, SubSkeletonBoneDto> bonePoses = new();
            foreach (SubSkeletonBoneDto bonePose in pose.bones)
            {
                if (bonePoses.ContainsKey(bonePose.boneType))
                    continue;

                if (ShouldInterpolate && bonePose.boneType != BoneType.Hips && bonePose.boneType != pose.boneAnchor?.bone)
                {
                    if (!IsEnding) // start transition
                        bonePose.localRotation = Quaternion.Slerp(Skeleton.Bones[bonePose.boneType].LocalRotation, bonePose.localRotation.Quaternion(), tEnterPhase).Dto();
                    else // end transition
                        bonePose.localRotation = Quaternion.Slerp(bonePose.localRotation.Quaternion(), Skeleton.Bones[bonePose.boneType].LocalRotation, tEndPhase).Dto();
                }

                bonePoses[bonePose.boneType] = bonePose;
            }

            return new SubSkeletonPoseDto()
            {
                boneAnchor = anchor,
                bones = bonePoses.Values.ToList()
            };
        }
    }
}

