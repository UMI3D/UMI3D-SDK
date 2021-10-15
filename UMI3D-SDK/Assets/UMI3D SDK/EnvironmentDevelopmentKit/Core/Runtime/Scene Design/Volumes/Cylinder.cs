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

    public class Cylinder : AbstractPrimitive
    {
        public float radius = 1;
        public float height = 3;

        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return new CylinderDto()
            {
                id = Id(),
                height = height,
                radius = radius,
                rootNodeId = GetRootNode().Id(),
                rootNodeToLocalMatrix = GetRootNodeToLocalMatrix(),
                isTraversable = IsTraversable()
            };
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireMesh(GeometryTools.GetCylinder(this.transform.position, this.transform.rotation, this.transform.localScale, radius, height));
        }
    }
}