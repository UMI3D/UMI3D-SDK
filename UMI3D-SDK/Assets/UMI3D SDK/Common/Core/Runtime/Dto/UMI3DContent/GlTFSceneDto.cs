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
    /// DTO to describe a scene as the scene root in the glTF 2.0 standard.
    /// </summary>
    [System.Serializable]
    public class GlTFSceneDto : UMI3DDto, IEntity
    {
        /// <summary>
        /// Name describing the scene.
        /// </summary>
        public string name;
        public List<GlTFNodeDto> nodes = new List<GlTFNodeDto>();
        public List<GlTFMaterialDto> materials = new List<GlTFMaterialDto>();
        public GlTFSceneExtensions extensions = new GlTFSceneExtensions();
    }
}
