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

namespace umi3d.common
{
    /// <summary>
    /// DTO for Physics-Based Rendering (PBR) material 
    /// </summary>
    [System.Serializable]
    public class PBRMaterialDto
    {
        /// <summary>
        /// Base color of the material, define the diffuse albedo for non-metals, and the specular color for metals.
        /// </summary>
        /// Default is white.
        public ColorDto baseColorFactor { get; set; }

        /// <summary>
        /// Metallic behaviour of the surface. 
        /// </summary>
        /// Usually either 0 for non-metallic surfaces or 1 for totally metallic ones. A value between 0 and 1 will result in an interpolated behaviour.
        public float metallicFactor { get; set; }

        /// <summary>
        /// Roughness of the surface.
        /// </summary>
        /// Rougher surfaces tends to have more blurried reflections.
        public float roughnessFactor { get; set; }

    }
}
