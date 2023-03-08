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
using UnityEngine;

namespace umi3d.common.userCapture
{
    [SerializeField]
    public abstract class SkeletonMappingLink
    {
        public abstract (Vector3 position, Quaternion rotation) Compute();
    }

    public class GameNodeLink : SkeletonMappingLink
    {
        public Transform transform;
        public override (Vector3 position, Quaternion rotation) Compute()
        {
            return (transform.position, transform.rotation);
        }
    }

    public class OffsetLink : SkeletonMappingLink
    {
        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        public bool isPositionOffsetLocal;
        public bool isRotationOffsetLocal;

        public SkeletonMappingLink node;

        public override (Vector3 position, Quaternion rotation) Compute()
        {
            var computed = node.Compute();

            computed.position += positionOffset;
            computed.rotation *= Quaternion.Euler(rotationOffset);

            return computed;
        }
    }

    public class LerpLink : SkeletonMappingLink
    {
        public SkeletonMappingLink nodeA;
        public SkeletonMappingLink nodeB;
        [Range(0,1)]
        public float factor;

        public override (Vector3 position, Quaternion rotation) Compute()
        {
            var cA = nodeA.Compute();
            var cB = nodeB.Compute();
            
            var p = Vector3.Lerp(cA.position, cB.position, factor);
            var r = Quaternion.Slerp(cA.rotation, cB.rotation, factor);

            return (p, r);
        }
    }

    public class BezierLerpLink : LerpLink
    {
        public Vector3 TangeantA;
        public Vector3 TangeantB;

        /// <summary>
        /// if set to true the tangeant poisition is 'nodeA.rotation * TangeantA + nodeA.position'
        /// </summary>
        public bool isTangeantALocal;

        /// <summary>
        /// if set to true the tangeant poisition is 'nodeB.rotation * TangeantB + nodeB.position'
        /// </summary>
        public bool isTangeantBLocal;

        //B(f) = (1-t)^3 A + 3(1-t)^2 t AT + 3(1-t) t^2 BT + t^3 B
        public override (Vector3 position, Quaternion rotation) Compute()
        {
            var cA = nodeA.Compute();
            var cB = nodeB.Compute();

            var tA = isTangeantALocal ? cA.position + cA.rotation * TangeantA : TangeantA;
            var tB = isTangeantBLocal ? cB.position + cB.rotation * TangeantB : TangeantB;

            var p = Mathf.Pow((1-factor),3) * cA.position + 3 * Mathf.Pow((1 - factor), 2) * factor * tA + 3 * (1 - factor) * Mathf.Pow(factor,2) * tB + Mathf.Pow((factor), 3) * cB.position;
            var r = Quaternion.Slerp(cA.rotation, cB.rotation, factor);

            return (p, r);
        }
    }

    public class LineDistanceConstraint : SkeletonMappingLink
    {
        public SkeletonMappingLink node;
        public SkeletonMappingLink contraintNode;
        public float distance;
        public Vector3 direction;
        public bool isLocal;

        public override (Vector3 position, Quaternion rotation) Compute()
        {
            var c = node.Compute();
            var cContraint = contraintNode.Compute();
            //todo
            return c;
        }
    }

}