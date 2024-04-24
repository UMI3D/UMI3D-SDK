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

using System;
using System.Collections.Generic;

using umi3d.common.userCapture.description;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Aggregates all played poses into a final subskeleton.
    /// </summary>
    internal interface IPoseClipAggregator : ISubskeletonDescriptor
    {
        /// <summary>
        /// Poses that are currently being aggregated, excluding stopping transition.
        /// </summary>
        IReadOnlyList<PoseClip> ActivePoses { get; }

        /// <summary>
        /// Poses that are currently being aggregated, including stopping transition.
        /// </summary>
        IReadOnlyList<PoseClip> PlayingPoses { get; }

        /// <summary>
        /// If true, cannot receive any other pose.
        /// </summary>
        bool IsLocked { get; }

        /// <summary>
        /// Raised each time a pose is no longer considered by the pose aggregator. I.e. when stop transition is finished.
        /// </summary>
        event Action<PoseClip> PoseStopped;

        /// <summary>
        /// True if the aggregator can play this pose directly or by transition.
        /// </summary>
        /// <param name="poseClip"></param>
        /// <returns></returns>
        bool CanPlay(PoseClip poseClip);

        /// <summary>
        /// True if the aggregator can play this pose by transition.
        /// </summary>
        /// <param name="poseClip"></param>
        /// <returns></returns>
        bool CanTransiteTo(PoseClip poseClip);

        /// <summary>
        /// Request the end of a pose.
        /// </summary>
        /// <param name="poseToStop"></param>
        /// <param name="isImmediateStop">True to stop without transition is one is required.</param>
        void End(PoseClip poseToStop, bool isImmediateStop = false);

        /// <summary>
        /// True if the pose clip is currently being aggregated in the aggregator.
        /// </summary>
        /// <param name="poseClip"></param>
        /// <returns></returns>
        bool IsPlaying(PoseClip poseClip);

        /// <summary>
        /// Attribute a channel to the pose and start playing.
        /// </summary>
        /// <param name="poseToPlay"></param>
        /// <param name="parameters"></param>
        void Play(PoseClip poseToPlay, ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = null);

        /// <summary>
        /// Immediately interrupts all poses.
        /// </summary>
        void StopAll();
    }
}