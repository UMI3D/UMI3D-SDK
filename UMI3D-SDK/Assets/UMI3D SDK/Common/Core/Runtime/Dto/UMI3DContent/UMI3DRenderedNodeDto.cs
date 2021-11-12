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

using System.Collections.Generic;

namespace umi3d.common
{
    public class UMI3DRenderedNodeDto : UMI3DNodeDto
    {
        /// <summary>
        /// Should some material be overrided
        /// </summary>
        public List<MaterialOverrideDto> overridedMaterials = null;

        /// <summary>
        /// Should apply the overriders list
        /// </summary>
        public bool applyCustomMaterial = false;

        /// <summary>
        /// State if object will be opaque to light.
        /// </summary>
        public bool castShadow;
        /// <summary>
        /// State if object display shadow of other object.
        /// </summary>
        public bool receiveShadow;

        /// <summary>
        /// Should some material be averrided
        /// </summary>
        [System.Serializable]
        public class MaterialOverrideDto : UMI3DDto
        {
            public List<string> overridedMaterialsId; // List of names of mat or "ANY_mat" or "DEFAULT_mat"
            public ulong newMaterialId;  // name of the umi3d entity (the id of the mat)
            public bool addMaterialIfNotExists;
        }
    }
}