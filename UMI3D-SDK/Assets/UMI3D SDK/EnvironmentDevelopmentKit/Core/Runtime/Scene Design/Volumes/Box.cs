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

namespace umi3d.edk.volume
{
    /// <summary>
    /// Box shaped volume cell.
    /// </summary>
    public class Box : AbstractPrimitive
    {
        public Bounds bounds;

        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return new BoxDto()
            {
                id = Id(),
                center = this.transform.TransformPoint(bounds.center),
                size = this.transform.TransformVector(bounds.size)
            };
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(this.transform.TransformPoint(bounds.center), this.transform.TransformVector(bounds.size));
        }
    }
}