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

using inetum.unityUtils;
using umi3d.common;
using umi3d.common.volume;
using UnityEngine;

namespace umi3d.edk.volume
{
    /// <summary>
    /// Box shaped volume cell.
    /// </summary>
    public class Box : AbstractPrimitive
    {
        /// <summary>
        /// If true, the extents vector start from a center that is on the bottom face of the box.
        /// </summary>
        /// This means that the center require to be put up of the extent on the y-axis.
        public bool extendFromBottom = false;

        /// <summary>
        /// Bounds of the box.
        /// </summary>
        /// Bounds are axis-aligned bounding boxes.
        [SerializeField]
        [EditorReadOnly, Tooltip("Bounds of the box.")]
        private Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        /// <summary>
        /// Size of the box.
        /// </summary>
        [Tooltip("Size of the box.")]
        public UMI3DAsyncProperty<Vector3> size;
        /// <summary>
        /// Center of the box.
        /// </summary>
        /// Located at the center of mass.
        [Tooltip("Center of the box.")]
        public UMI3DAsyncProperty<Vector3> center;


        protected override void Awake()
        {
            base.Awake();

            center = new UMI3DAsyncProperty<Vector3>(Id(), UMI3DPropertyKeys.VolumePrimitive_Box_Center, bounds.center);
            size = new UMI3DAsyncProperty<Vector3>(Id(), UMI3DPropertyKeys.VolumePrimitive_Box_Size, bounds.size);

            center.OnValueChanged += c => bounds.center = c;
            size.OnValueChanged += s => bounds.size = s;
        }

        /// <inheritdoc/>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return new BoxDto()
            {
                id = Id(),
                center =( center.GetValue() + (extendFromBottom ? bounds.extents.y * Vector3.up : Vector3.zero)).Dto(),
                size = size.GetValue().Dto(),
                rootNodeId = GetRootNode().Id(),
                isTraversable = IsTraversable()
            };
        }

        /// <summary>
        /// Unity's OnDrawGizmos function
        /// </summary>
        public void OnDrawGizmos()
        {
            Bounds displayBound = bounds;
            if (extendFromBottom)
                displayBound.center += bounds.extents.y * Vector3.up;

            Gizmos.matrix = this.transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(displayBound.center, displayBound.size);
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawCube(displayBound.center, displayBound.size);
        }
    }
}