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
using umi3d.common.userCapture;
using umi3d.common.userCapture.binding;
using umi3d.edk.binding;

namespace umi3d.edk.userCapture.binding
{
    /// <summary>
    /// Operation binding a node to a user's skeleton bone.
    /// </summary>
    public class BoneBinding : AbstractSingleBinding
    {
        /// <summary>
        /// Bone type of the anchor bone as referenced in <see cref="BoneType"/>.
        /// </summary>
        public uint boneType;

        /// <summary>
        /// User owning the bone.
        /// </summary>
        public ulong userId;

        /// <summary>
        /// Specifying if the object is binded to the computed bone or to the controller
        /// </summary>
        public bool bindToController;

        /// <summary>
        ///
        /// </summary>
        /// <param name="boundNodeId"></param>
        /// <param name="userId">User owning the bone.</param>
        /// <param name="boneType">one type of the anchor bone as referenced in <see cref="BoneType"/></param>
        public BoneBinding(ulong boundNodeId, ulong userId, uint boneType) : base(boundNodeId)
        {
            this.boneType = boneType;
            this.userId = userId;
        }

        /// <inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            AbstractBindingDataDto bindingDataDto;

            bindingDataDto = new BoneBindingDataDto()
            {
                userId = userId,
                boneType = boneType,
                bindToController = bindToController,

                offSetPosition = offsetPosition.Dto(),
                offSetRotation = offsetRotation.Dto(),
                offSetScale = offsetScale.Dto(),

                anchorPosition = anchor.Dto(),

                partialFit = partialFit,
                priority = priority,
                resetWhenRemoved = resetWhenRemoved,

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