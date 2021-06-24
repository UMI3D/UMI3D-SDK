using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;

namespace umi3d.edk.volume
{
    public class Box : AbstractPrimitive
    {
        public Bounds bounds;

        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return new BoxDto()
            {
                id = Id(),
                center = this.transform.TransformPoint(bounds.center),
                size = this.transform.TransformVector(bounds.size)
            };
        }
    }
}