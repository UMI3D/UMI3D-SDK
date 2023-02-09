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
