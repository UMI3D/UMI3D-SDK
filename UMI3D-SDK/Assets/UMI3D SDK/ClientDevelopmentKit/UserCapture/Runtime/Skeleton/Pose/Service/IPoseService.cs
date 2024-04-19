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
    /// Manager that handles pose animators requests.
    /// </summary>
    public interface IPoseService
    {
        /// <summary>
        /// Validate/Invalidate an <see cref="EnvironmentPoseCondition"/>.
        /// </summary>
        /// <param name="requestDto"></param>
        void ChangeEnvironmentPoseCondition(ulong environmentId, ulong poseConditionId, bool shouldBeValidated);

        /// <summary>
        /// Request to try to activate a pose animator now.
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="poseAnimatorId"></param>
        /// <returns></returns>
        bool TryActivatePoseAnimator(ulong environmentId, ulong poseAnimatorId);

        /// <summary>
        /// Request to try to deactivate a pose animator now.
        /// </summary>
        /// <param name="environmentId"></param>
        /// <param name="poseAnimatorId"></param>
        /// <returns></returns>
        bool TryDeactivatePoseAnimator(ulong environmentId, ulong poseAnimatorId);

        /// <summary>
        /// Sets the within the pose clip on the poseSkeleton
        /// </summary>
        void PlayPoseClip(PoseClip poseClip, ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = null, bool shouldOverride = false);

        /// <summary>
        /// Stops the related pose in the poseSkeleton
        /// </summary>
        void StopPoseClip(PoseClip poseClip);

        /// <summary>
        /// Stops all poses
        /// </summary>
        void StopAllPoses();

        /// <summary>
        /// Swap a playing pose for another pose.
        /// </summary>
        /// <param name="playingPoseClip"></param>
        /// <param name="newPoseClip"></param>
        /// <param name="transitionDuration"></param>
        /// <param name="parameters"></param>
        void SwitchPose(PoseClip playingPoseClip, PoseClip newPoseClip, float transitionDuration, ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = null);
    }
}