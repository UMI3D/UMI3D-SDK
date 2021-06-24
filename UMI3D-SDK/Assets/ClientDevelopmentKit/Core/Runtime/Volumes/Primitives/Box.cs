using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.cdk.volumes
{
    public class Box : AbstractPrimitive
    {
        public Bounds bounds;

        public override void Delete() { }

        public override bool IsInside(Vector3 point)
        {
            return bounds.Contains(point);
        }
    }
}