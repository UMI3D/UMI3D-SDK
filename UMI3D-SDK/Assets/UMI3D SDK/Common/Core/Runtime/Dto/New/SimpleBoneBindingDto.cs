using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class SimpleBoneBindingDto : SimpleBindingDto
    {
        public SimpleBoneBindingDto() { }

        public SimpleBoneBindingDto(ulong userId, uint boneType,
                        SimpleBindingDto[] simpleBindings,
                        bool syncRotation, bool syncScale, bool syncPosition,
                        Vector3 offSetPosition, Vector4 offSetRotation, Vector3 offSetScale,
                        int priority, bool partialFit) : base(simpleBindings, syncRotation, syncScale, syncPosition,
                                                                offSetPosition, offSetRotation, offSetScale,
                                                                priority, partialFit)
        {
            this.userId = userId;  
            this.boneType = boneType;
        }

        public ulong userId { get; private set; }
        public uint boneType { get; private set; }  
     }
}
