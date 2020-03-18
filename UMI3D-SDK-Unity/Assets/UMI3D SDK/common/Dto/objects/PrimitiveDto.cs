/*
Copyright 2019 Gfi Informatique

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    [System.Serializable]
    public class PrimitiveDto : EmptyObject3DDto
    {
        /// <summary>
        /// Primitive type.
        /// </summary>
        public int Primitive = -1;

        /// <summary>
        /// Material to override to (if any).
        /// </summary>
        public MaterialDto material = new MaterialDto();

        /// <summary>
        /// Video ressource (if any).
        /// </summary>
        public VideoDto video = null;

        /// <summary>
        /// Type of the collider generated in front end.
        /// </summary>
        public ColliderType colliderType = ColliderType.Auto;

        /// <summary>
        /// In case of a mesh collider, should it be convex ?
        /// </summary>
        public bool convex = false;

        public PrimitiveDto() : base() { }

    }
}
