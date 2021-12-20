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
        public bool extendFromBottom = false;

        [SerializeField]
        [EditorReadOnly]
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
        public UMI3DAsyncProperty<Vector3> size;
        public UMI3DAsyncProperty<Vector3> center;


        protected override void Awake()
        {
            base.Awake();

            center = new UMI3DAsyncProperty<Vector3>(Id(), UMI3DPropertyKeys.VolumePrimitive_Box_Center, bounds.center);
            size = new UMI3DAsyncProperty<Vector3>(Id(), UMI3DPropertyKeys.VolumePrimitive_Box_Size, bounds.size);

            center.OnValueChanged += c => bounds.center = c;
            size.OnValueChanged += s => bounds.size = s;
        }

        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return new BoxDto()
            {
                id = Id(),
                center = center.GetValue() + (extendFromBottom ? bounds.extents.y * Vector3.up : Vector3.zero),
                size = size.GetValue(),
                rootNodeId = GetRootNode().Id(),
                rootNodeToLocalMatrix = GetRootNodeToLocalMatrix(),
                isTraversable = IsTraversable()
            };
        }

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