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
    /// Linear interpolation link constraint.
    /// </summary>
    public class LerpLink : ISkeletonMappingLink
    {
        public ISkeletonMappingLink nodeA;
        public ISkeletonMappingLink nodeB;

        /// <summary>
        /// Linear inteprolaiton factor between 0 and 1. 0 is using completely node A values, and 1 is using ones from node B.
        /// </summary>
        public float factor;

        public LerpLink(ISkeletonMappingLink nodeA, ISkeletonMappingLink nodeB)
        {
            this.nodeA = nodeA ?? throw new System.ArgumentNullException("SkeletonMapping link A is null");
            this.nodeB = nodeB ?? throw new System.ArgumentNullException("SkeletonMapping link B is null");
        }

        /// <inheritdoc/>
        public virtual (Vector3 position, Quaternion rotation) Compute()
        {
            var cA = nodeA.Compute();
            var cB = nodeB.Compute();

            var p = Vector3.Lerp(cA.position, cB.position, factor);
            var r = Quaternion.Slerp(cA.rotation, cB.rotation, factor);

            return (p, r);
        }
    }
}