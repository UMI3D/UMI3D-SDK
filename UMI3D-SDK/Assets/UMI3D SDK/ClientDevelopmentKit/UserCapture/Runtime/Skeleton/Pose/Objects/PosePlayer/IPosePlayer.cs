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


using umi3d.common.userCapture.description;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Player that applies a pose and gives the values corresponding to it. Ensure the interpolation if needed.
    /// </summary>
    public interface IPosePlayer
    {
        /// <summary>
        /// Pose clip handled by the player.
        /// </summary>
        PoseClip PoseClip { get; }

        /// <summary>
        /// True if the player is currently applying a pose.
        /// </summary>
        /// Note that this value is still true after some time 
        /// after calling <see cref="EndPoseClip(bool)"/> because of end of application handling.
        bool IsPlaying { get; }

        /// <summary>
        /// True when the player is in the ending interpolation phase.
        /// </summary>
        bool IsEnding { get; }

        /// <summary>
        /// Complete skeleton affected by the player.
        /// </summary>
        ISkeleton Skeleton { get; }

        /// <summary>
        /// Start to apply a pose on the affected skeleton.
        /// </summary>
        /// <param name="parameters">Optional parameters to control the pose playing.</param>
        void PlayPoseClip(PosePlayer.PlayingParameters parameters = null);

        /// <summary>
        /// Start to end a pose on the affected skeleton. 
        /// </summary>
        /// <param name="shouldStopImmediate">If true, the pose is ended immediately with no ending inteprolation phase.</param>
        void EndPoseClip(bool shouldStopImmediate = false);

        /// <summary>
        /// Get the output pose from the player.
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <returns></returns>
        SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy);
    }
}