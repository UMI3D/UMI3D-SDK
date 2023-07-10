﻿/*
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
using umi3d.common.binding;
using umi3d.common.dto.binding;

namespace umi3d.edk.binding
{
    /// <summary>
    /// Operation binding a node and another node.
    /// </summary>
    public class NodeBinding : AbstractSingleBinding
    {
        /// <summary>
        /// Node to which the object is attached upon.
        /// </summary>
        public ulong parentNodeId;

        public NodeBinding(ulong boundNodeId, ulong parentNodeId) : base(boundNodeId)
        {
            this.parentNodeId = parentNodeId;
        }

        /// <inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            AbstractBindingDataDto bindingDataDto;

            bindingDataDto = new NodeBindingDataDto
            {
                parentNodeId = parentNodeId,
                offSetPosition = offsetPosition.Dto(),
                offSetRotation = offsetRotation.Dto(),
                offSetScale = offsetScale.Dto(),
                partialFit = partialFit,
                priority = priority,
                syncPosition = syncPosition,
                syncRotation = syncRotation,
                syncScale = syncScale,
                anchorPosition = anchor.Dto()
            };

            var bindingDto = new BindingDto
            {
                id = Id(),
                boundNodeId = base.boundNodeId,
                data = bindingDataDto
            };

            return bindingDto;
        }
    }
}