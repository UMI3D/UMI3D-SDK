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

using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Behaviur of a piece of skeleton position to merge to have the whole skeleton position.
    /// </summary>
    public interface ISubSkeleton
    {
        /// <summary>
        /// Update the position of this subskeleton according to the received <paramref name="trackingFrame"/>.
        /// </summary>
        /// <param name="trackingFrame"></param>
        void UpdateFrame(UserTrackingFrameDto trackingFrame);

        /// <summary>
        /// Get the skeleton pose based on the position of this subskeleton.
        /// </summary>
        /// <returns></returns>
        PoseDto GetPose();

        /// <summary>
        /// Fill out the <paramref name="trackingFrame"/> with data from the subskeleton.
        /// </summary>
        /// <param name="trackingFrame"></param>
        /// <param name="option"></param>
        void WriteTrackingFrame(UserTrackingFrameDto trackingFrame, TrackingOption option);

        /// <summary>
        /// Get subskeleton camera parameters.
        /// </summary>
        /// <returns></returns>
        UserCameraPropertiesDto GetCameraDto(); 
    }
}