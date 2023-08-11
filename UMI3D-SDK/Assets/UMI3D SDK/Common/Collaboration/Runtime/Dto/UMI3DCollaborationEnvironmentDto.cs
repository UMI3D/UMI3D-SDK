/*
Copyright 2019 - 2021 Inetum

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
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.userCapture.pose;

namespace umi3d.common.collaboration.dto
{
    public class UMI3DCollaborationEnvironmentDto : UMI3DEnvironmentDto
    {
        /// <summary>
        /// List of users connected to the environment, whatever the status they have.
        /// </summary>
        /// Users initialized through this list are added/deleted through transactions
        /// when they connect/disconnect.
        public List<UserDto> userList { get; set; }

        /// <summary>
        /// A dictionary that contains all the poses already loaded in the environment
        /// key : user Id 
        /// value : list of all the poses of this user
        /// </summary>
        public Dictionary<ulong, List<PoseDto>> poses { get; set; }

        /// <summary>
        /// A dictionary that contains all the poses already loaded in the environment
        /// key : user Id 
        /// value : list of all the poses of this user
        /// </summary>
        public List<UMI3DPoseOverridersContainerDto> poseOverriderContainers { get; set; } = new();
    }
}