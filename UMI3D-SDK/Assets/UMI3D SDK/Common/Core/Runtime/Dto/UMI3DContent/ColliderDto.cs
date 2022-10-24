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

using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// DTO describing a collider.
    /// </summary>
    [System.Serializable]
    public class ColliderDto : UMI3DDto
    {
        /// <summary>
        /// Type of the collider generated in front end.
        /// </summary>
        public ColliderType colliderType = ColliderType.Mesh;

        /// <summary>
        /// In case of a mesh collider, should it be convex ?
        /// </summary>
        public bool convex = false;

        /// <summary>
        /// Center of the collider for box, sphere and capsule collider
        /// </summary>
        public SerializableVector3 colliderCenter;

        /// <summary>
        /// The radius for sphere and capsule collider
        /// </summary>
        public float colliderRadius = 1f;

        /// <summary>
        /// The box scale for boxCollider
        /// </summary>
        public SerializableVector3 colliderBoxSize = Vector3.one;

        /// <summary>
        /// The height of le collider for capsule collider
        /// </summary>
        public float colliderHeight = 1f;

        /// <summary>
        /// The collider direction for capsule collider
        /// </summary>
        public DirectionalType colliderDirection = DirectionalType.Y_Axis;

        /// <summary>
        /// true if un custom mesh is used for the MeshCollider
        /// </summary>
        public bool isMeshCustom = false;

        /// <summary>
        /// Custom MeshCollider used if isMeshCustom
        /// </summary>
        public ResourceDto customMeshCollider;

    }
}
