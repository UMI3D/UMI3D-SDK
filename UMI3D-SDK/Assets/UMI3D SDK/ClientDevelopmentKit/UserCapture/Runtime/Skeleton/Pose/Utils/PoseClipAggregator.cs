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
using umi3d.cdk.userCapture.description;
using umi3d.common.userCapture.description;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Combine several playing poses into a final pose subskeleton.
    /// </summary>
    internal class PoseClipAggregator : IPoseClipAggregator
    {
        private readonly ISkeleton skeleton;

        public PoseClipAggregator(ISkeleton skeleton)
        {
            this.skeleton = skeleton;
        }

        /// <summary>
        /// Container that reserve a part of the aggregator to play a pose.
        /// </summary>
        private record PoseChannel
        {
            public PoseClip poseClip;
            public ISubskeletonDescriptionInterpolationPlayer.PlayingParameters playingParameters = new();
            public ISubskeletonDescriptionInterpolationPlayer posePlayer;
        }

        private readonly Dictionary<PoseClip, PoseChannel> poseChannels = new();

        /// <summary>
        /// Poses that are currently being aggregated, including stopping transition.
        /// </summary>
        public IReadOnlyList<PoseClip> PlayingPoses => poseChannels.Values.Where(x => x.posePlayer.IsPlaying)
                                                                    .Select(x => x.poseClip).ToList();

        /// <summary>
        /// Poses that are currently being aggregated, excluding stopping transition.
        /// </summary>
        public IReadOnlyList<PoseClip> ActivePoses => poseChannels.Values.Where(x => x.posePlayer.IsPlaying && !x.posePlayer.IsEnding)
                                                                    .Select(x => x.poseClip).ToList();

        /// <summary>
        /// If true, cannot receive any other pose.
        /// </summary>
        public bool IsLocked => poseChannels.Keys.Any(currentlyPlayingPose => !currentlyPlayingPose.IsComposable);

        /// <summary>
        /// True if the pose clip is currently being aggregated in the aggregator.
        /// </summary>
        /// <param name="poseClip"></param>
        /// <returns></returns>
        public bool IsPlaying(PoseClip poseClip)
        {
            return poseChannels.ContainsKey(poseClip);
        }

        /// <summary>
        /// True if the aggregator can play this pose directly or by transition.
        /// </summary>
        /// <param name="poseClip"></param>
        /// <returns></returns>
        public bool CanPlay(PoseClip poseClip)
        {
            return !IsLocked
                    && (poseChannels.Keys.All(currentlyPlayingPose => ArePoseClipsCompatible(skeleton.SkeletonHierarchy, currentlyPlayingPose, poseClip)) || CanTransiteTo(poseClip));
        }

        /// <summary>
        /// True if the aggregator can play this pose by transition.
        /// </summary>
        /// <param name="poseClip"></param>
        /// <returns></returns>
        public bool CanTransiteTo(PoseClip poseClip)
        {
            PoseChannel[] incompatiblePoseChannels = poseChannels.Values.Where(channel => !ArePoseClipsCompatible(skeleton.SkeletonHierarchy, channel.poseClip, poseClip)).ToArray();

            return poseClip.IsInterpolable && incompatiblePoseChannels.All(x => x.posePlayer.IsEnding);
        }

        /// <summary>
        /// Checks whether two poses would not superpose on a same subskeleton.
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="poseClip1"></param>
        /// <param name="poseClip2"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static bool ArePoseClipsCompatible(UMI3DSkeletonHierarchy hierarchy, PoseClip poseClip1, PoseClip poseClip2)
        {
            if (poseClip1 == null)
                throw new System.ArgumentNullException(nameof(poseClip1));

            if (poseClip2 == null)
                throw new System.ArgumentNullException(nameof(poseClip2));

            if (poseClip1 == poseClip2)
                return false;

            uint highestBonePose1 = hierarchy.GetHighestBone(poseClip1.Pose.bones.Select(x => x.boneType));
            uint highestBonePose2 = hierarchy.GetHighestBone(poseClip2.Pose.bones.Select(x => x.boneType));

            if (highestBonePose1 == highestBonePose2)
                return false;

            return hierarchy.CompareBones(highestBonePose1, highestBonePose2) == 0;
        }

        /// <summary>
        /// Attribute a channel to the pose and start playing.
        /// </summary>
        /// <param name="poseToPlay"></param>
        /// <param name="parameters"></param>
        public void Play(PoseClip poseToPlay, ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = null)
        {
            if (!CanPlay(poseToPlay))
                return;

            PoseChannel[] incompatiblePoseChannels = poseChannels.Values.Where(channel => !ArePoseClipsCompatible(skeleton.SkeletonHierarchy, channel.poseClip, poseToPlay)).ToArray();

            if (incompatiblePoseChannels.Length == 0)
            {
                if (parameters == null && poseToPlay.IsInterpolable == false)
                {
                    parameters = new()
                    {
                        startTransitionDuration = 0,
                        endTransitionDuration = 0
                    };
                }
            }
            else if (poseToPlay.IsInterpolable && incompatiblePoseChannels.All(x => x.posePlayer.IsEnding)) // all incompatible poses are ending, transite to new pose
            {
                // keep a snapshot of the ending state of the incompatible poses
                SubskeletonPoseSnapshot endingPoseChannelsMergedDescription = new(AggregateFromChannels(skeleton.SkeletonHierarchy, incompatiblePoseChannels.Select(x => x.posePlayer)));
                incompatiblePoseChannels.ForEach(x => RemoveChannel(x.poseClip));

                parameters ??= new();
                parameters.previousDescriptor = endingPoseChannelsMergedDescription; // add the snapshot as the old descriptor for interpolation
            }

            PoseChannel channel = CreateChannel(poseToPlay);
            channel.posePlayer.Play(parameters);
        }

        /// <summary>
        /// Request the end of a pose.
        /// </summary>
        /// <param name="poseToStop"></param>
        /// <param name="isImmediateStop">True to stop without transition is one is required.</param>
        public void End(PoseClip poseToStop, bool isImmediateStop = false)
        {
            if (!poseChannels.TryGetValue(poseToStop, out PoseChannel poseChannel))
                return;

            poseChannel.posePlayer.Stopped += () =>
            {
                if (poseChannels.ContainsKey(poseToStop)) // could have been removed by a transition
                    poseChannels.Remove(poseToStop);

                PoseStopped?.Invoke(poseToStop);
            };

            poseChannel.posePlayer.End(!poseToStop.IsInterpolable || isImmediateStop);
        }

        /// <summary>
        /// Raised each time a pose is no longer considered by the pose aggregator. I.e. when stop transition is finished.
        /// </summary>
        public event System.Action<PoseClip> PoseStopped;

        /// <summary>
        /// Immediately interrupts all poses.
        /// </summary>
        public void StopAll()
        {
            foreach (var poseChannel in poseChannels.Values)
            {
                poseChannel.posePlayer.End(true);
            }
            poseChannels.Clear();
        }

        /// <summary>
        /// Snapshot of description of a subskeleton pose.
        /// </summary>
        private class SubskeletonPoseSnapshot : ISubskeletonDescriptor
        {
            private SubSkeletonPoseDto dto;

            public SubskeletonPoseSnapshot(SubSkeletonPoseDto dto)
            {
                this.dto = dto;
            }

            public SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
            {
                return dto;
            }
        }

        /// <summary>
        /// Create a Pose Playing Channel for the pose clip.
        /// </summary>
        /// <param name="poseClip"></param>
        /// <returns></returns>
        private PoseChannel CreateChannel(PoseClip poseClip)
        {
            if (poseChannels.ContainsKey(poseClip))
                RemoveChannel(poseClip);

            SubskeletonDescriptionInterpolationPlayer posePlayer = new(poseClip, poseClip.IsInterpolable, skeleton);

            PoseChannel channel = new()
            {
                poseClip = poseClip,
                posePlayer = posePlayer
            };

            poseChannels.Add(poseClip, channel);
            return channel;
        }

        /// <summary>
        /// Remove a channel that is playing a pose clip.
        /// </summary>
        /// <param name="poseClip"></param>
        private void RemoveChannel(PoseClip poseClip)
        {
            if (!poseChannels.TryGetValue(poseClip, out var channel))
                return;

            if (channel.posePlayer.IsPlaying)
                channel.posePlayer.End(true);

            poseChannels.Remove(poseClip);
        }

        /// <inheritdoc/>
        public SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            return AggregateFromChannels(hierarchy, poseChannels.Values.Select(x => x.posePlayer));
        }

        /// <summary>
        /// Aggregate the poses from several players.
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="posePlayers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private SubSkeletonPoseDto AggregateFromChannels(UMI3DSkeletonHierarchy hierarchy, IEnumerable<ISubskeletonDescriptionInterpolationPlayer> posePlayers)
        {
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            Dictionary<uint, SubSkeletonBoneDto> bonePoses = new();

            // merge poses from pose players
            foreach (var posePlayer in posePlayers)
            {
                if (!posePlayer.IsPlaying)
                    continue;

                SubSkeletonPoseDto subSkeletonPose = posePlayer.GetPose(hierarchy);
                Dictionary<uint, SubSkeletonBoneDto> subskeletonBonePose = subSkeletonPose.bones.ToDictionary(x => x.boneType);

                foreach (var bone in subSkeletonPose.bones)
                {
                    if (bonePoses.ContainsKey(bone.boneType) && posePlayer.IsEnding) // priority to non-ending poses
                        continue;

                    bonePoses[bone.boneType] = subskeletonBonePose[bone.boneType];
                }
            }

            return new SubSkeletonPoseDto()
            {
                bones = bonePoses.Values.ToList()
            };
        }
    }
}