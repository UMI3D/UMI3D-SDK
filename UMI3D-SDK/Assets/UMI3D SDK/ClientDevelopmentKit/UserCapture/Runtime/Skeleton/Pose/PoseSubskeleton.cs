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
        private readonly ITrackerSimulator trackerSimulator;

        public PoseSubskeleton(ulong environmentId, ISkeleton parentSkeleton) : this(environmentId: environmentId,
                                                                                                               parentSkeleton: parentSkeleton,
                                                                                                               environmentManagerService: UMI3DEnvironmentLoader.Instance,
                                                                                                               trackerSimulator: TrackerSimulationManager.Instance.GetTrackerSimulator(parentSkeleton))
        {
        }

        public PoseSubskeleton(ulong environmentId, ISkeleton parentSkeleton, IEnvironmentManager environmentManagerService, ITrackerSimulator trackerSimulator)
        {
            this.environmentManagerService = environmentManagerService;
            this.parentSkeleton = parentSkeleton;
            this.trackerSimulator = trackerSimulator;
            EnvironmentId = environmentId;
        }

        #endregion Dependency Injection

        /// <inheritdoc/>
        public IReadOnlyList<PoseClip> AppliedPoses => appliedPoses;

        protected List<PoseClip> appliedPoses = new();

        public int Priority => PRIORITY;

        public ulong EnvironmentId { get; set; }

        private const int PRIORITY = 100;

        private readonly Dictionary<PoseClip, PosePlayingControllers> posePlayingControllers = new();

        private class PosePlayingControllers
        {
            public ISubskeletonDescriptionInterpolationPlayer Player;
            public PoseAnchorDto Anchor;
        }

        private SubskeletonDescriptionInterpolationPlayer AddPoseClipPlayer(PoseClip poseClip, PoseAnchorDto anchor = null)
        {
            SubskeletonDescriptionInterpolationPlayer posePlayer = new (poseClip, poseClip.IsInterpolable, parentSkeleton);
            PosePlayingControllers posePlayingData = new()
            {
                Player = posePlayer,
                Anchor = anchor
            };

            if (posePlayingControllers.ContainsKey(poseClip))
                RemovePoseClipPlayer(poseClip);

            posePlayingControllers.Add(poseClip, posePlayingData);
            return posePlayer;
        }

        private void RemovePoseClipPlayer(PoseClip poseClip)
        {
            if (!posePlayingControllers.TryGetValue(poseClip, out PosePlayingControllers playingControllers))
                return;

            if (playingControllers.Player.IsPlaying)
                playingControllers.Player.End(true);

            posePlayingControllers.Remove(poseClip);
        }

        /// <inheritdoc/>
        public void StartPose(IEnumerable<PoseClip> posesToAdd, bool isOverriding = false, ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = null, PoseAnchorDto anchorToForce = null)
        {
            if (posesToAdd == null)
                throw new ArgumentNullException(nameof(posesToAdd), $"Cannot start poses.");

            if (isOverriding)
                StopAllPoses();

            foreach (PoseClip poseClip in posesToAdd)
                StartPose(poseClip, parameters: parameters, anchorToForce: anchorToForce);
        }

        /// <inheritdoc/>
        public void StartPose(PoseClip poseToAdd, bool isOverriding = false, ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = null, PoseAnchorDto anchorToForce = null)
        {
            if (poseToAdd == null)
                throw new ArgumentNullException(nameof(poseToAdd), $"Cannot start pose.");

            if (appliedPoses.Contains(poseToAdd))
            {
                UMI3DLogger.LogWarning($"Pose clip {poseToAdd.Id} is already playing.", DebugScope.CDK | DebugScope.UserCapture);
                return;
            }

            if (isOverriding)
                StopAllPoses();

            appliedPoses.Add(poseToAdd);

            if (posePlayingControllers.TryGetValue(poseToAdd, out PosePlayingControllers existingPosePlayingController)) // reset the pose player when chaining the same pose clip
            {
                if (existingPosePlayingController.Anchor != null)
                    trackerSimulator.StopTrackerSimulation(existingPosePlayingController.Anchor);
                RemovePoseClipPlayer(poseToAdd);
            }

            SubskeletonDescriptionInterpolationPlayer player = AddPoseClipPlayer(poseToAdd, anchorToForce ?? poseToAdd.Pose.anchor);

            PoseAnchorDto anchor = posePlayingControllers[poseToAdd].Anchor;
            if (anchor != null)
                trackerSimulator.StartTrackerSimulation(anchor);

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

            if (!posePlayingControllers.TryGetValue(poseToStop, out PosePlayingControllers posePlayer))
                return;

            posePlayer.Player.End();
            appliedPoses.Remove(poseToStop);

            if (posePlayer.Anchor != null)
                trackerSimulator.StopTrackerSimulation(posePlayer.Anchor);
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
            foreach (var (poseClip, posePlayingController) in posePlayingControllers)
            {
                if (!posePlayingController.Player.IsPlaying)
                    continue;

                SubSkeletonPoseDto subSkeletonPose = posePlayingController.Player.GetPose(hierarchy);
                Dictionary<uint, SubSkeletonBoneDto> subskeletonBonePose = subSkeletonPose.bones.ToDictionary(x => x.boneType);

                foreach (BoneDto bone in poseClip.Bones)
                {
                    if (bonePoses.ContainsKey(bone.boneType) && posePlayingController.Player.IsEnding) // priority to non-ending poses
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
                UMI3DEntityInstance poseClipEntityInstance = environmentManagerService.TryGetEntityInstance(EnvironmentId, poseId);
                PoseClip poseClip = poseClipEntityInstance?.Object as PoseClip;

                if (poseClipEntityInstance is not null && !appliedPoses.Contains(poseClip))
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