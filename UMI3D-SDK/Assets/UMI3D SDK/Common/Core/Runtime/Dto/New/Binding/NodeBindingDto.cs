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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.userCapture
{
    public class NodeBindingDto : SimpleBindingDto
    {
        public NodeBindingDto() { }

        public NodeBindingDto(ulong objectID,
                        bool syncRotation, bool syncScale, bool syncPosition,
                        Vector3 offSetPosition, Vector4 offSetRotation, Vector3 offSetScale,
                        int priority, bool partialFit) : base( syncRotation, syncScale, syncPosition,
                                                                offSetPosition, offSetRotation, offSetScale,
                                                                priority, partialFit)
        {
            this.objectId = objectID;
        }

        public NodeBindingDto(SimpleBindingDto simpleBinding, uint objectID) : base (simpleBinding.syncRotation, simpleBinding.syncScale, simpleBinding.syncPosition,
                                                                simpleBinding.offSetPosition, simpleBinding.offSetRotation, simpleBinding.offSetScale,
                                                                simpleBinding.priority, simpleBinding.partialFit)
        {
            this.objectId = objectID;
        }

        public ulong objectId { get; private set; }
    }
}