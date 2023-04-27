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
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// Operation binding a rig under a node to a user's skeleton bone.
    /// </summary>
    public class RiggedBoneBinding : BoneBinding
    {
        /// <summary>
        /// Name of the rig to bind under the declared bound node.
        /// </summary>
        public string rigName = "";

        public RiggedBoneBinding(ulong boundNodeId, uint boneType, ulong userId) : base(boundNodeId, boneType, userId)
        {
        }



        /// <inheritdoc/>
        public override BindingDto ToDto()
        {
            AbstractBindingDataDto bindingDataDto;

            bindingDataDto = new RiggedBoneBindingDataDto(
                userId: userId,
                boneType: boneType,
                rigName: rigName,

                offSetPosition: offsetPosition,
                offSetRotation: offsetRotation,
                offSetScale: offsetScale,

                anchorPosition: anchor,

                partialFit: partialFit,
                priority: priority,

                syncPosition: syncPosition,
                syncRotation: syncRotation,
                syncScale: syncScale
            );


            BindingDto bindingDto = new BindingDto(
                boundNodeId: boundNodeId,
                data: bindingDataDto
            );

            return bindingDto;
        }
    }
}
