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

namespace umi3d.edk.userCapture
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
        ///
        /// </summary>
        /// <param name="boundNodeId"></param>
        /// <param name="boneType">one type of the anchor bone as referenced in <see cref="BoneType"/></param>
        /// <param name="userId">User owning the bone.</param>
        public BoneBinding(ulong boundNodeId, uint boneType, ulong userId) : base(boundNodeId)
        {
            this.boneType = boneType;
            this.userId = userId;
        }

        /// <inheritdoc/>
        public override BindingDto ToDto()
        {
            AbstractBindingDataDto bindingDataDto;

            bindingDataDto = new BoneBindingDataDto()
            {
                userId = userId,
                boneType = boneType,

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
                boundNodeId = boundNodeId,
                data = bindingDataDto
            };

            return bindingDto;
        }
    }
}