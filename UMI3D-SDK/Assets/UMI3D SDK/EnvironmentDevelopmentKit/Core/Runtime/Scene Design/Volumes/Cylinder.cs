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
    public class Cylinder : AbstractPrimitive
    {
        [SerializeField]
        [EditorReadOnly]
        public float radius_inspector = 1;

        [SerializeField]
        [EditorReadOnly]
        public float height_inspector = 3;

        public UMI3DAsyncProperty<float> radius;
        public UMI3DAsyncProperty<float> height;


        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return new CylinderDto()
            {
                id = Id(),
                height = height.GetValue(),
                radius = radius.GetValue(),
                rootNodeId = GetRootNode().Id(),
                isTraversable = IsTraversable()
            };
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Mesh cylinder = GeometryTools.GetCylinder(this.transform.position, this.transform.rotation, this.transform.localScale, radius_inspector, height_inspector);
            Gizmos.DrawWireMesh(cylinder);
            if (Application.isPlaying)
                Destroy(cylinder);
        }

        protected override void Awake()
        {
            base.Awake();

            radius = new UMI3DAsyncProperty<float>(Id(), UMI3DPropertyKeys.VolumePrimitive_Cylinder_Radius, radius_inspector);
            height = new UMI3DAsyncProperty<float>(Id(), UMI3DPropertyKeys.VolumePrimitive_Cylinder_Height, height_inspector);

            radius.OnValueChanged += r => radius_inspector = r;
            height.OnValueChanged += h => height_inspector = h;
        }
    }
}