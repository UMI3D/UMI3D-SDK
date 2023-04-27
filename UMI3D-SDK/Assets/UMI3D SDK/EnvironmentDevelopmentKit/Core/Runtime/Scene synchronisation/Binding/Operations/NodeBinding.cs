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

namespace umi3d.edk
{
    /// <summary>
    /// Operation binding a node and another node.
    /// </summary>
    public class NodeBinding : AbstractSingleBinding
    {
        /// <summary>
        /// Node to which the object is attached upon.
        /// </summary>
        public ulong nodeId;

        public NodeBinding(ulong boundNodeId, ulong nodeId) : base(boundNodeId)
        {
            this.nodeId = nodeId;
        }

        /// <inheritdoc/>
        public override BindingDto ToDto()
        {
            AbstractBindingDataDto bindingDataDto;

            bindingDataDto = new NodeBindingDataDto(
                nodeId: nodeId,
                offSetPosition: offsetPosition,
                offSetRotation: offsetRotation,
                offSetScale: offsetScale,
                partialFit: partialFit,
                priority: priority,
                syncPosition: syncPosition,
                syncRotation: syncRotation,
                syncScale: syncScale,
                anchorPosition: anchor
            );

            var bindingDto = new BindingDto(
                boundNodeId: base.boundNodeId,
                data: bindingDataDto
            );

            return bindingDto;
        }
    }
}