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

using umi3d.common;
using umi3d.common.collaboration;
using umi3d.edk.userCapture;
using System.Collections.Generic;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// Root node of any collaborative environment.
    /// </summary>
    public class UMI3DCollaborationEnvironment : UMI3DEnvironment
    {
        /// <inheritdoc/>
        protected override UMI3DEnvironmentDto CreateDto()
        {
            return new UMI3DCollaborationEnvironmentDto();
        }

        /// <inheritdoc/>
        protected override void WriteProperties(UMI3DEnvironmentDto _dto, UMI3DUser user)
        {
            base.WriteProperties(_dto, user);
            if (_dto is UMI3DCollaborationEnvironmentDto dto)
            {
                dto.userList = UMI3DCollaborationServer.Collaboration.ToDto(user);

                dto.allPoses = UMI3DPoseManager.Instance.allPoses;
                dto.allPoseOverriderContainer = UMI3DPoseManager.Instance.GetOverriders();
            }
        }
    }
}