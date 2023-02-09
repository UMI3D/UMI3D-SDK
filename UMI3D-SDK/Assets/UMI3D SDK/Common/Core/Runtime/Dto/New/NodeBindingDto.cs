using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class NodeBindingDto : SimpleBindingDto
    {
        public NodeBindingDto() { }

        public NodeBindingDto(ulong objectID,
                        bool syncRotation, bool syncScale, bool syncPosition,
                        Vector3 offSetPosition, Vector4 offSetRotation, Vector3 offSetScale,
                        int priority, bool partialFit) : base(syncRotation, syncScale, syncPosition,
                                                                offSetPosition, offSetRotation, offSetScale,
                                                                priority, partialFit)
        {

        }

        public ulong objectId { get; private set; }
    }
}
