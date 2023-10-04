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

using System.Collections.Generic;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Manager that handles poses.
    /// </summary>
    public interface IPoseManager
    {
        /// <summary>
        /// Poses based on an user id (0 for environment) and their id.
        /// </summary>
        IDictionary<ulong, IList<SkeletonPose>> Poses { get; set; }

        /// <summary>
        /// Activate all poses that listen to this mode.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="poseActivationMode"></param>
        bool TryActivatePoseOverriders(ulong nodeId, PoseActivationMode poseActivationMode);

        /// <summary>
        /// Sets the related pose to the overrider Dto, in the poseSkeleton
        /// </summary>
        /// <param name="poseOverriderDto"></param>
        void ApplyPoseOverride(PoseOverrider poseOverrider);

        /// <summary>
        /// Stops all poses
        /// </summary>
        /// <param name="poseOverriderDto"></param>
        void StopAllPoses();

        /// <summary>
        /// Stops the related pose to the overriderDto, in the poseSkeleton
        /// </summary>
        /// <param name="poseOverriderDto"></param>
        void StopPoseOverride(PoseOverrider poseOverrider);

        /// <summary>
        /// Allows to add a pose handler unit at runtime
        /// </summary>
        /// <param name="overrider"></param>
        /// <param name="unit"></param>
        void AddPoseOverriders(PoseOverridersContainer overrider);

        /// <summary>
        /// Allows to remove a pose handler unit at runtime
        /// </summary>
        /// <param name="overrider"></param>
        void RemovePoseOverriders(PoseOverridersContainer overrider);
    }
}