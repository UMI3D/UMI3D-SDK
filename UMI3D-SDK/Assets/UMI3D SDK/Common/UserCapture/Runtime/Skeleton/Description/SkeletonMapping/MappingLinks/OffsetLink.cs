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
    /// Fixed distance link constraint.
    /// </summary>
    public class OffsetLink : ISkeletonMappingLink
    {
        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        public ISkeletonMappingLink node;

        public OffsetLink(ISkeletonMappingLink node)
        {
            this.node = node ?? throw new System.ArgumentNullException("SkeletonMappingLink is null");
        }

        /// <inheritdoc/>
        public virtual (Vector3 position, Quaternion rotation) Compute()
        {
            var computed = node.Compute();

            computed.position += positionOffset;
            computed.rotation *= Quaternion.Euler(rotationOffset);

            return computed;
        }
    }
}