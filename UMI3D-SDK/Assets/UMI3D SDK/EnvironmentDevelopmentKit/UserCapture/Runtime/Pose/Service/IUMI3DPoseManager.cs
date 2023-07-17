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

namespace umi3d.edk.userCapture.pose
{
    public interface IUMI3DPoseManager
    {
        /// <summary>
        /// Returns all the pose containers of the scene 
        /// </summary>
        IList<UMI3DPoseOverriderContainerDto> PoseOverriderContainers { get; }

        /// <summary>
        /// Returns all the pose stored for every users in the experience
        /// </summary>
        IDictionary<ulong, IList<PoseDto>> Poses { get; }

        void RegisterEnvironmentPoses(IPosesRegister register);

        void RegisterPoseOverriders(IPoseOverridersRegister register);

        /// <summary>
        /// Sets the new poses of a specific user based on his id,
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="poseDtos"></param>
        void RegisterUserCustomPose(ulong userId, IEnumerable<PoseDto> poseDtos);
    }
}