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

using inetum.unityUtils;
using System;
using System.Threading.Tasks;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Loader for <see cref="UMI3DAvatarNodeDto"/> in a collaborative context.
    /// </summary>
    public class UMI3DCollabAvatarNodeLoader : UMI3DAvatarNodeLoader
    {
        /// <summary>
        /// Load an avatar node for collaborative user.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the abstract node will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override async Task ReadUMI3DExtension(UMI3DDto dto, GameObject node)
        {
            var nodeDto = dto as UMI3DAbstractNodeDto;
            if (node == null)
            {
                throw (new Umi3dException("dto should be an  UMI3DAbstractNodeDto"));
            }

            await base.ReadUMI3DExtension(dto, node);

            if (!(dto as UMI3DAvatarNodeDto).userId.Equals(UMI3DClientServer.Instance.GetUserId()))
            {
                UserAvatar ua = node.GetOrAddComponent<UMI3DCollaborativeUserAvatar>();
                ua.Set(dto as UMI3DAvatarNodeDto);
                UMI3DClientUserTracking.Instance.RegisterEmbd((nodeDto as UMI3DAvatarNodeDto).userId, ua);
            }
        }
    }
}
