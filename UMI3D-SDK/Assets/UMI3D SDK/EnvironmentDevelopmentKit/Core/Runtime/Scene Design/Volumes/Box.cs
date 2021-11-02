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
using umi3d.common.volume;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.volume
{
    /// <summary>
    /// Box shaped volume cell.
    /// </summary>
    public class Box : AbstractPrimitive
    {
        public ObservableProperty<bool> extendFromBottom = new ObservableProperty<bool>(false);
        public ObservableProperty<Bounds> bounds = new ObservableProperty<Bounds>(new Bounds(Vector3.zero, Vector3.one));

        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return new BoxDto()
            {
                id = Id(),
                center = bounds.GetValue().center + (extendFromBottom.GetValue() ? bounds.GetValue().extents.y * Vector3.up : Vector3.zero),
                size = bounds.GetValue().size,
                rootNodeId = GetRootNode().Id(),
                rootNodeToLocalMatrix = GetRootNodeToLocalMatrix(),
                isTraversable = IsTraversable()
            };
        }

        public void OnDrawGizmos()
        {
            Bounds displayBound = bounds.GetValue();
            if (extendFromBottom.GetValue())
                displayBound.center += bounds.GetValue().extents.y * Vector3.up;

            Gizmos.matrix = this.transform.localToWorldMatrix;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(displayBound.center, displayBound.size);
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawCube(displayBound.center, displayBound.size);
        }
    }
}