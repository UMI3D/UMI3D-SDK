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
using umi3d.common.userCapture;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    public interface IPoseManager
    {
        PoseDto GetPose(ulong key, int index);

        /// <summary>
        /// Activated if the Hover Enter is triggered
        /// </summary>
        void OnHoverEnter(ulong id);

        /// <summary>
        /// Activated if the Hover Exit is triggered
        /// </summary>
        void OnHoverExit(ulong id);

        /// <summary>
        /// Activated if the Release Enter is triggered
        /// </summary>
        void OnRelease(ulong id);

        /// <summary>
        /// Activated if the Trigger is triggered
        /// </summary>
        void OnTrigger(ulong id);

        void SetPoses(Dictionary<ulong, List<PoseDto>> allPoses);

        /// <summary>
        /// Inits all the pose overrider containers
        /// </summary>
        /// <param name="allPoseOverriderContainer"></param>
        void SetPosesOverriders(List<UMI3DPoseOverriderContainerDto> allPoseOverriderContainer);

        /// <summary>
        /// Sets the related pose to the overrider Dto, in the poseSkeleton
        /// </summary>
        /// <param name="poseOverriderDto"></param>
        void SetTargetPose(PoseOverriderDto poseOverriderDto, bool isSeverPose = true);

        /// <summary>
        /// Stops all poses
        /// </summary>
        /// <param name="poseOverriderDto"></param>
        void StopAllPoses();

        /// <summary>
        /// Stops the related pose to the overriderDto, in the poseSkeleton
        /// </summary>
        /// <param name="poseOverriderDto"></param>
        void StopTargetPose(PoseOverriderDto poseOverriderDto, bool isServerPose = true);

        /// <summary>
        /// Allows to add a pose handler unit at runtime
        /// </summary>
        /// <param name="overrider"></param>
        /// <param name="unit"></param>
        void SubscribeNewPoseHandlerUnit(UMI3DPoseOverriderContainerDto overrider);

        /// <summary>
        /// Allows to remove a pose handler unit at runtime
        /// </summary>
        /// <param name="overrider"></param>
        void UnSubscribePoseHandlerUnit(UMI3DPoseOverriderContainerDto overrider);
    }
}