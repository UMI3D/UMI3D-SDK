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
            this.poseAggregator = new PoseClipAggregator(parentSkeleton);
            EnvironmentId = environmentId;
            Init();
        }

        #endregion Dependency Injection

        /// <summary>
        /// Aggregates all played poses into a final skeleton for the pose subskeleton.
        /// </summary>
        private readonly IPoseClipAggregator poseAggregator;

        /// <inheritdoc/>
        public IReadOnlyList<PoseClip> AppliedPoses => poseAggregator.ActivePoses;

        public int Priority => PRIORITY;

        public ulong EnvironmentId { get; set; }

        private const int PRIORITY = 100;

        /// <summary>
        /// Poses to play next when possible.
        /// </summary>
        private readonly List<PlayPoseRequest> queuedPlayPoseRequests = new();

        private record PlayPoseRequest
        {
            public PoseClip poseClip;
            public ISubskeletonDescriptionInterpolationPlayer.PlayingParameters playingParameters = new();
        }

        public void Init()
        {
            poseAggregator.PoseStopped += (_) => CheckPendingPlayingPoseRequests();
        }

        #region Pose Playing

        #region Start

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

            // pose already playing on a channel.
            if (poseAggregator.IsPlaying(poseToAdd))
            {
                UMI3DLogger.LogWarning($"Pose clip {poseToAdd.Id} is already playing.", DEBUG_SCOPE);
                return;
            }

            if (isOverriding)
                StopAllPoses();

            // pose already requested to be played.
            if (queuedPlayPoseRequests.Select(x => x.poseClip).Contains(poseToAdd))
            {
                UMI3DLogger.LogWarning($"Pose clip {poseToAdd.Id} is already queued for playing.", DEBUG_SCOPE);
                return;
            }

            // if the pose could not be executed right now, store and wait for a channel to free itself.
            if (!poseAggregator.CanPlay(poseToAdd))
            {
                PlayPoseRequest request = new()
                {
                    poseClip = poseToAdd,
                    playingParameters = parameters
                };

                queuedPlayPoseRequests.Add(request);
                CheckPendingPlayingPoseRequests();
                return;
            }

            poseAggregator.Play(poseToAdd, parameters);
        }

        #endregion Start

        #region Pending Requests Management

        /// <summary>
        /// Try to apply pending pose playing requests
        /// </summary>
        private void CheckPendingPlayingPoseRequests()
        {
            if (poseAggregator.IsLocked) // a non-composable pose is preventing others to be played.
                return;

            foreach (PlayPoseRequest playPoseRequest in queuedPlayPoseRequests.ToArray())
            {
                if (poseAggregator.CanPlay(playPoseRequest.poseClip)) // all poses are compatibles, play the pose
                {
                    queuedPlayPoseRequests.Remove(playPoseRequest);

                    poseAggregator.Play(playPoseRequest.poseClip, playPoseRequest.playingParameters ?? new());

                    if (!playPoseRequest.poseClip.IsComposable)
                        return;
                }
            }
        }

        #endregion Pending Requests Management

        #region Stop

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

            if (queuedPlayPoseRequests.Select(x => x.poseClip).Contains(poseToStop))
            {
                queuedPlayPoseRequests.Remove(queuedPlayPoseRequests.First(x => x.poseClip == poseToStop));
                return;
            }

            if (!poseAggregator.IsPlaying(poseToStop))
                return;

            poseAggregator.End(poseToStop);
        }

        /// <inheritdoc/>
        public void StopPose(IEnumerable<ulong> posesToStopIds)
        {
            foreach (ulong poseToStopId in posesToStopIds)
            {
                StopPose(poseAggregator.PlayingPoses.FirstOrDefault(x => x.Id == poseToStopId));
            }
        }

        /// <inheritdoc/>
        public void StopPose(ulong poseToStopId)
        {
            StopPose(poseAggregator.PlayingPoses.Where(x => x.Id == poseToStopId));
        }

        /// <inheritdoc/>
        public void StopAllPoses()
        {
            queuedPlayPoseRequests.Clear();
            poseAggregator.StopAll();
        }

        #endregion Stop

        #endregion Pose Playing

        /// <inheritdoc/>
        public SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            return poseAggregator.GetPose(hierarchy);
        }

        #region TrackingFrames

        private IEnumerable<PoseClip> posesToStop; // optimization for allocation

        /// <inheritdoc/>
        public void UpdateBones(UserTrackingFrameDto trackingFrame)
        {
            if (trackingFrame is null)
                throw new ArgumentNullException(nameof(trackingFrame));

            // stop poses
            posesToStop = AppliedPoses.Where(pose => !trackingFrame.poses.Contains(pose.Id)).ToList();
            foreach (PoseClip poseToStop in posesToStop)
            {
                StopPose(poseToStop);
            }

            // play poses
            foreach (ulong poseId in trackingFrame.poses)
            {
                //at load, could receive tracking frame without having the pose
                if (!environmentManagerService.TryGetEntity(EnvironmentId, poseId, out PoseClip poseClip))
                    continue;

                if (!poseAggregator.IsPlaying(poseClip))
                    StartPose(poseClip);
            }
        }

        /// <inheritdoc/>
        public void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option)
        {
            if (trackingFrame == null)
                throw new ArgumentNullException(nameof(trackingFrame));

            trackingFrame.poses ??= new(AppliedPoses.Count);

            AppliedPoses.ForEach((pose) =>
            {
                trackingFrame.poses.Add(pose.Id);
            });
        }

        #endregion TrackingFrames
    }
}