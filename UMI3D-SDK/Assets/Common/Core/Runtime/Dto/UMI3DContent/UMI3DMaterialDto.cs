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
    [System.Serializable]
    public class UMI3DMaterialDto : AbstractEntityDto, IMaterialDto
    {
        //glTF PBR Texture
        public TextureDto baseColorTexture;
        public TextureDto metallicRoughnessTexture;

        //glTF additional maps
        public ScalableTextureDto normalTexture;
        public TextureDto emissiveTexture;
        public TextureDto occlusionTexture;

        //UMI3D additional maps
        public TextureDto metallicTexture;
        public TextureDto roughnessTexture;
        public ScalableTextureDto heightTexture;

        public TextureDto channelTexture;

        // Modified properties in the shader 
        public Dictionary<string, object> shaderProperties { get; set; }
    }

}