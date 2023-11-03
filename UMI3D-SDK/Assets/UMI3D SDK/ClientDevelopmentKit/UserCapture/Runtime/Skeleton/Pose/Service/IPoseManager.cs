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

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Manager that handles poses.
    /// </summary>
    public interface IPoseManager
    {
        /// <summary>
        /// Sets the related pose in the poseSkeleton
        /// </summary>
        /// <param name="poseClip"></param>
        void PlayPoseClip(PoseClip poseClip);

        /// <summary>
        /// Stops all poses
        /// </summary>
        void StopAllPoses();

        /// <summary>
        /// Stops the related pose in the poseSkeleton
        /// </summary>
        void StopPoseClip(PoseClip poseClip);

        /// <summary>
        /// Validate/Invalidate an <see cref="EnvironmentPoseCondition"/>.
        /// </summary>
        /// <param name="requestDto"></param>
        void ChangeEnvironmentPoseCondition(ulong poseConditionId, bool shouldValidate);
    }
}