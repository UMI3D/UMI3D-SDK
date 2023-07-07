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
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// A UMI3D resource. A same resource could have several variants.
    /// </summary>
    [System.Serializable]
    public class UMI3DResource
    {
        /// <summary>
        /// Variants of the resource from different files.
        /// </summary>
        [SerializeField, Tooltip("Variants of the resource from different files.")]
        public List<UMI3DResourceFile> variants = new List<UMI3DResourceFile>();

        /// <inheritdoc/>
        public ResourceDto ToDto()
        {
            var dto = new ResourceDto();
            dto.variants.AddRange(variants.Select(v => v.ToDto()));
            return dto;
        }

        /// <inheritdoc/>
        public Bytable ToByte()
        {
            return UMI3DSerializer.WriteCollection(variants);
        }
    }
}