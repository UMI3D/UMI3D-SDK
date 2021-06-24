using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common.volume
{
    public class BoxDto : AbstractPrimitiveDto
    {
        public SerializableVector3 center;
        public SerializableVector3 size;
    }
}