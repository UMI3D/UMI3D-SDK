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
using umi3d.common.dto.binding;
using umi3d.common.userCapture.binding;

namespace umi3d.edk.userCapture.binding
{
    /// <summary>
    /// Operation binding a rig under a node to a user's skeleton bone.
    /// </summary>
    public class RigBoneBinding : BoneBinding
    {
        /// <summary>
        /// Name of the rig to bind under the declared bound node.
        /// </summary>
        public string rigName = "";

        public RigBoneBinding(ulong boundNodeId, string rigName, ulong userId, uint boneType) : base(boundNodeId, userId, boneType)
        {
            this.rigName = rigName;
        }

        /// <inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            AbstractBindingDataDto bindingDataDto;

            bindingDataDto = new RigBoneBindingDataDto()
            {
                userId = userId,
                boneType = boneType,
                rigName = rigName,

                offSetPosition = offsetPosition.Dto(),
                offSetRotation = offsetRotation.Dto(),
                offSetScale = offsetScale.Dto(),

                anchorPosition = anchor.Dto(),

                partialFit = partialFit,
                priority = priority,

                syncPosition = syncPosition,
                syncRotation = syncRotation,
                syncScale = syncScale
            };

            BindingDto bindingDto = new BindingDto()
            {
                id = Id(),
                boundNodeId = boundNodeId,
                data = bindingDataDto
            };

            return bindingDto;
        }
    }
}