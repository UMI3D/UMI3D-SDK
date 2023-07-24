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
    /// <summary>
    /// Service that handle poses from the environment side.
    /// </summary>
    public interface IPoseManager
    {
        /// <summary>
        /// Returns all the pose containers of the scene 
        /// </summary>
        IList<UMI3DPoseOverridersContainerDto> PoseOverriderContainers { get; }

        /// <summary>
        /// Returns all the pose stored for every users in the experience
        /// </summary>
        IDictionary<ulong, IList<PoseDto>> Poses { get; }

        /// <summary>
        /// Register poses that are designed for the environment.
        /// Put them in standard format.
        /// </summary>
        /// <param name="register">Register that has the poses.</param>
        void RegisterEnvironmentPoses(IPosesRegister register);

        /// <summary>
        /// Register pose overriders that are designed for the environment.
        /// Generate all the needed pose overriders containers.
        /// </summary>
        /// <param name="register">Register that has the pose overriders.</param>
        void RegisterPoseOverriders(IPoseOverridersRegister register);

        /// <summary>
        /// Register poses that are designed for each browser.
        /// </summary>
        /// <param name="userId">User id of the browser sending custom poses.</param>
        /// <param name="poseDtos">Poses to register.</param>
        void RegisterUserCustomPose(ulong userId, IEnumerable<PoseDto> poseDtos);
    }
}