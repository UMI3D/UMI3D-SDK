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

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// Cubic Bezier spline link constraint
    /// </summary>
    public class BezierLerpLink : LerpLink
    {
        public Vector3 TangeantA;
        public Vector3 TangeantB;

        /// <summary>
        /// if set to true the tangeant position is 'nodeA.rotation * TangeantA + nodeA.position'
        /// </summary>
        public bool isTangeantALocal;

        /// <summary>
        /// if set to true the tangeant position is 'nodeB.rotation * TangeantB + nodeB.position'
        /// </summary>
        public bool isTangeantBLocal;

        public BezierLerpLink(ISkeletonMappingLink nodeA, ISkeletonMappingLink nodeB) : base(nodeA, nodeB)
        {
        }

        /// <inheritdoc/>
        public override (Vector3 position, Quaternion rotation) Compute()
        {
            var cA = nodeA.Compute();
            var cB = nodeB.Compute();

            var tA = isTangeantALocal ? cA.position + cA.rotation * TangeantA : TangeantA;
            var tB = isTangeantBLocal ? cB.position + cB.rotation * TangeantB : TangeantB;

            // B(f) = (1 - t) ^ 3 A + 3(1 - t) ^ 2 t AT +3(1 - t) t ^ 2 BT + t ^ 3 B
            var p = Mathf.Pow((1 - factor), 3) * cA.position + 3 * Mathf.Pow((1 - factor), 2) * factor * tA + 3 * (1 - factor) * Mathf.Pow(factor, 2) * tB + Mathf.Pow((factor), 3) * cB.position;
            var r = Quaternion.Slerp(cA.rotation, cB.rotation, factor);

            return (p, r);
        }
    }
}