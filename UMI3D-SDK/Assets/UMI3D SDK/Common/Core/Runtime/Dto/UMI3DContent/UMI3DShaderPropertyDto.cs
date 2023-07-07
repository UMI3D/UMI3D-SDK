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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace umi3d.common
{
    /// <summary>
    /// Representation of a shader's property.
    /// </summary>
    /// Shader property are of unkown types, thus types are identified in <see cref="UMI3DShaderPropertyType"/>.
    [System.Serializable]
    public class UMI3DShaderPropertyDto : UMI3DDto
    {
        /// <summary>
        /// Shader property collection type in <see cref="UMI3DShaderPropertyType"/> if the property is a collection.
        /// </summary>
        public byte collectionType { get; set; }
        /// <summary>
        /// Size of the colleciton if the property is a collection.
        /// </summary>
        public int size { get; set; }
        /// <summary>
        /// Shader property type in <see cref="UMI3DShaderPropertyType"/>.
        /// </summary>
        public byte type { get; set; }
        /// <summary>
        /// Shader property value.
        /// </summary>
        public object value { get; set; }
    }
}