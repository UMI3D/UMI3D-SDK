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
    /// Unidirectional distance link constraint.
    /// </summary>
    public class LineDistanceConstraintLink : ISkeletonMappingLink
    {
        public ISkeletonMappingLink node;
        public float distance;
        public Vector3 direction;

        public LineDistanceConstraintLink(ISkeletonMappingLink node, float distance, Vector3 direction)
        {
            this.node = node;
            this.distance = distance;
            this.direction = direction.normalized;
        }

        /// <inheritdoc/>
        public virtual (Vector3 position, Quaternion rotation) Compute()
        {
            var c = node.Compute();

            c.position += direction * distance;
            
            return c;
        }
    }
}