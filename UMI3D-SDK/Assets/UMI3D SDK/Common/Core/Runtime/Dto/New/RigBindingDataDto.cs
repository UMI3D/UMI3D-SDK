using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class RigBindingDataDto : SimpleBoneBindingDto
    {
        public RigBindingDataDto() { }

        public RigBindingDataDto(string rigName, ulong userId, uint boneType,
                SimpleBindingDto[] simpleBindings,
                bool syncRotation, bool syncScale, bool syncPosition,
                Vector3 offSetPosition, Vector4 offSetRotation, Vector3 offSetScale,
                int priority, bool partialFit) : base(userId, boneType, simpleBindings,
                                                        syncRotation, syncScale, syncPosition,
                                                        offSetPosition, offSetRotation, offSetScale,
                                                        priority, partialFit)
        {
            this.rigName= rigName;
        }


        public string rigName { get; private set; }
    }
}
