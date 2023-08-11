/*
Copyright 2019 - 2021 Inetum

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


using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Anchor of a node in the real world, if any, for AR scene design.
    /// </summary>
    /// Objects that posses an Anchor are placed according to their anchor in the real world, instaed of its position in the scene.
    public class UMI3DAnchor : MonoBehaviour
    {
        /// <summary>
        /// Position offset of the anchor relative to the object.
        /// </summary>
        public Vector3 PositionOffset;
        /// <summary>
        /// Rotation offset of the anchor relative to the object.
        /// </summary>
        public Quaternion RotationOffset;
        /// <summary>
        /// Scale offset of the anchor relative to the object.
        /// </summary>
        public Vector3 ScaleOffset;

        /// <inheritdoc/>
        public UMI3DAnchorDto ToDto()
        {
            return new UMI3DAnchorDto()
            {
                positionOffset = PositionOffset.Dto(),
                rotationOffset = RotationOffset.Dto(),
                scaleOffset = ScaleOffset.Dto()
            };
        }
    }
}