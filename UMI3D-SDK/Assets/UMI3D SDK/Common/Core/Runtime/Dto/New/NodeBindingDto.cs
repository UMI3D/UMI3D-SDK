using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class NodeBindingDto : SimpleBindingDto
    {
        public NodeBindingDto() { }

        public NodeBindingDto(ulong objectID,
                        SimpleBindingDto[] simpleBindings,
                        bool syncRotation, bool syncScale, bool syncPosition,
                        Vector3 offSetPosition, Vector4 offSetRotation, Vector3 offSetScale,
                        int priority, bool partialFit) : base(simpleBindings, syncRotation, syncScale, syncPosition,
                                                                offSetPosition, offSetRotation, offSetScale,
                                                                priority, partialFit)
        {
            this.objectId = objectID;
        }

        public ulong objectId { get; private set; }
    }
}
