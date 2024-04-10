/*
Copyright 2019 - 2023 Inetum

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
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.tracking;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Subskeleton that receive poses on its bones.
    /// </summary>
    public class PoseSubskeleton : IPoseSubskeleton
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.UserCapture;

        #region Dependency Injection

        private readonly IEnvironmentManager environmentManagerService;
        private readonly ISkeleton parentSkeleton;

        public PoseSubskeleton(ulong environmentId, ISkeleton parentSkeleton) : this(environmentId: environmentId,
                                                                                                               parentSkeleton: parentSkeleton,
                                                                                                               environmentManagerService: UMI3DEnvironmentLoader.Instance)
        {
        }

        public PoseSubskeleton(ulong environmentId, ISkeleton parentSkeleton, IEnvironmentManager environmentManagerService)
        {
            this.environmentManagerService = environmentManagerService;
            this.parentSkeleton = parentSkeleton;
            EnvironmentId = environmentId;
        }

        #endregion Dependency Injection

        /// <inheritdoc/>
        public IReadOnlyList<PoseClip> AppliedPoses => appliedPoses;

        protected List<PoseClip> appliedPoses = new();

        public int Priority => PRIORITY;

        public ulong EnvironmentId { get; set; }

        private const int PRIORITY = 100;

        private readonly Dictionary<PoseClip, ISubskeletonDescriptionInterpolationPlayer> posePlayers = new();

        private SubskeletonDescriptionInterpolationPlayer CreatePoseClipPlayer(PoseClip poseClip)
        {
            SubskeletonDescriptionInterpolationPlayer posePlayer = new (poseClip, poseClip.IsInterpolable, parentSkeleton);

            if (posePlayers.ContainsKey(poseClip))
                RemovePoseClipPlayer(poseClip);

            posePlayers.Add(poseClip, posePlayer);
            return posePlayer;
        }

        private void RemovePoseClipPlayer(PoseClip poseClip)
        {
            if (!posePlayers.TryGetValue(poseClip, out ISubskeletonDescriptionInterpolationPlayer posePlayer))
                return;

            if (posePlayer.IsPlaying)
                posePlayer.End(true);

            posePlayers.Remove(poseClip);
        }

        /// <inheritdoc/>
        public void StartPose(IEnumerable<PoseClip> posesToAdd, bool isOverriding = false, ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = null)
        {
            if (posesToAdd == null)
                throw new ArgumentNullException(nameof(posesToAdd), $"Cannot start poses.");

            if (isOverriding)
                StopAllPoses();

            foreach (PoseClip poseClip in posesToAdd)
                StartPose(poseClip, parameters: parameters);
        }

        /// <inheritdoc/>
        public void StartPose(PoseClip poseToAdd, bool isOverriding = false, ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = null)
        {
            if (poseToAdd == null)
                throw new ArgumentNullException(nameof(poseToAdd), $"Cannot start pose.");

            if (appliedPoses.Contains(poseToAdd))
            {
                UMI3DLogger.LogWarning($"Pose clip {poseToAdd.Id} is already playing.", DEBUG_SCOPE);
                return;
            }

            if (isOverriding)
                StopAllPoses();

            appliedPoses.Add(poseToAdd);

            SubskeletonDescriptionInterpolationPlayer player = CreatePoseClipPlayer(poseToAdd);

            player.Play(parameters);
        }

        /// <inheritdoc/>
        public void StopPose(IEnumerable<PoseClip> posesToStop)
        {
            if (posesToStop == null)
                return;

            posesToStop.ForEach(StopPose);
        }

        /// <inheritdoc/>
        public void StopPose(PoseClip poseToStop)
        {
            if (poseToStop == null)
                return;

            if (!posePlayers.TryGetValue(poseToStop, out ISubskeletonDescriptionInterpolationPlayer posePlayer))
                return;

            posePlayer.End();
            appliedPoses.Remove(poseToStop);
        }

        /// <inheritdoc/>
        public void StopPose(IEnumerable<ulong> posesToStopIds)
        {
            StopPose(posesToStopIds.Select(poseId => appliedPoses.Find(x => x.Id == poseId)));
        }

        /// <inheritdoc/>
        public void StopPose(ulong poseToStopId)
        {
            StopPose(appliedPoses.Find(x => x.Id == poseToStopId));
        }

        /// <inheritdoc/>
        public void StopAllPoses()
        {
            appliedPoses.Clear();
        }

        /// <inheritdoc/>
        public SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            Dictionary<uint, SubSkeletonBoneDto> bonePoses = new();

            // merge poses from pose players
            foreach (var (poseClip, posePlayer) in posePlayers)
            {
                if (!posePlayer.IsPlaying)
                    continue;

                SubSkeletonPoseDto subSkeletonPose = posePlayer.GetPose(hierarchy);
                Dictionary<uint, SubSkeletonBoneDto> subskeletonBonePose = subSkeletonPose.bones.ToDictionary(x => x.boneType);

                foreach (BoneDto bone in poseClip.Bones)
                {
                    if (bonePoses.ContainsKey(bone.boneType) && posePlayer.IsEnding) // priority to non-ending poses
                        continue;

                    bonePoses[bone.boneType] = subskeletonBonePose[bone.boneType];
                }

                if (!poseClip.IsComposable)
                    break;
            }

            return new SubSkeletonPoseDto()
            {
                bones = bonePoses.Values.ToList()
            };
        }

        /// <inheritdoc/>
        public void UpdateBones(UserTrackingFrameDto trackingFrame)
        {
            if (trackingFrame is null)
                throw new ArgumentNullException(nameof(trackingFrame));

            StartPoses(trackingFrame);

            // remove not activated poses
            int nbObjToRemove = appliedPoses.Count - trackingFrame.poses.Count;
            if (nbObjToRemove > 0)
            {
                Queue<PoseClip> posesToRemove = new Queue<PoseClip>(nbObjToRemove);
                appliedPoses.ForEach(pose =>
                {
                    if (!trackingFrame.poses.Contains(pose.Id))
                        posesToRemove.Enqueue(pose);
                });
                StopPose(posesToRemove);
            }
        }

        private void StartPoses(UserTrackingFrameDto trackingFrame)
        {
            foreach (ulong poseId in trackingFrame.poses)
            {
                //at load, could receive tracking frame without having the pose
                if (environmentManagerService.TryGetEntity(EnvironmentId, poseId, out PoseClip poseClip) && !appliedPoses.Contains(poseClip))
                    StartPose(poseClip);
            }
        }

        /// <inheritdoc/>
        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            if (trackingFrame == null)
                throw new ArgumentNullException(nameof(trackingFrame));

            trackingFrame.poses ??= new(appliedPoses.Count);

            appliedPoses.ForEach((pose) =>
            {
                trackingFrame.poses.Add(pose.Id);
            });
        }
    }
}