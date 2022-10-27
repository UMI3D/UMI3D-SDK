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
    /// <summary>
    /// DTO to describe a material based on textures.
    /// </summary>
    /// See alos <see cref="PBRMaterialDto"/>.
    [System.Serializable]
    public class UMI3DMaterialDto : AbstractEntityDto, IMaterialDto
    {
        //glTF PBR Texture
        /// <summary>
        /// Base color in PBR as a texture.
        /// </summary>
        public TextureDto baseColorTexture;

        /// <summary>
        /// Metallic roughness in PBR as a texture.
        /// </summary>
        public TextureDto metallicRoughnessTexture;

        //glTF additional maps
        /// <summary>
        /// Normal map as a texture.
        /// </summary>
        public ScalableTextureDto normalTexture;

        /// <summary>
        /// Emission map as a texture.
        /// </summary>
        public TextureDto emissiveTexture;

        /// <summary>
        /// Occlusion map as a texture.
        /// </summary>
        public TextureDto occlusionTexture;

        //UMI3D additional maps
        /// <summary>
        /// Metallic map as a texture.
        /// </summary>
        public TextureDto metallicTexture;

        /// <summary>
        /// Roughness map as a texture.
        /// </summary>
        public TextureDto roughnessTexture;

        /// <summary>
        /// Height map as a texture.
        /// </summary>
        public ScalableTextureDto heightTexture;

        /// <summary>
        /// Map channel as a texture.
        /// </summary>
        public TextureDto channelTexture;

        /// <summary>
        /// Modified properties in the shader as a key-value collection;
        /// </summary>
        public Dictionary<string, object> shaderProperties { get; set; }
    }
}