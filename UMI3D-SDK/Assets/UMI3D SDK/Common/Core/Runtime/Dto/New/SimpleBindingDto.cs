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

namespace umi3d.common.collaboration
{
    public class SimpleBindingDto : BindingDataDto
    {
        public SimpleBindingDto() { }

        public SimpleBindingDto(SimpleBindingDto[] simpleBindings, bool syncRotation, bool syncScale, bool syncPosition,
                                Vector3 offSetPosition, Vector4 offSetRotation, Vector3 offSetScale,
                                int priority, bool partialFit) : base(priority, partialFit)
        {
            this.syncRotation = syncRotation;
            this.syncScale = syncScale;
            this.syncPosition = syncPosition;
            this.offSetPosition = offSetPosition;
            this.offSetRotation = offSetRotation;          
            this.offSetScale = offSetScale;   
            this.simpleBindings = simpleBindings;
        }

        public SimpleBindingDto(bool syncRotation, bool syncScale, bool syncPosition,
                                Vector3 offSetPosition, Vector4 offSetRotation, Vector3 offSetScale,
                                BindingDataDto bindingDataDto) : base(bindingDataDto.priority, bindingDataDto.partialFit)
        {
            this.syncRotation = syncRotation;
            this.syncScale = syncScale;
            this.syncPosition = syncPosition;
            this.offSetPosition = offSetPosition;
            this.offSetRotation = offSetRotation;
            this.offSetScale = offSetScale;
        }

        public bool syncRotation { get; private set; }
        public bool syncScale { get; private set; }
        public bool syncPosition { get; private set; }

        public Vector3 offSetPosition { get; private set; }
        public Vector4 offSetRotation { get; private set; }
        public Vector3 offSetScale { get; private set; }

        public SimpleBindingDto[] simpleBindings { get; private set; }
    }
}
