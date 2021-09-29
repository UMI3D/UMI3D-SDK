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
    public class GlTFEnvironmentDto : AbstractEntityDto
    {

        public GlTFAsset asset = new GlTFAsset();

        public List<GlTFSceneDto> scenes = new List<GlTFSceneDto>();

        public GlTFEnvironmentExtensions extensions = new GlTFEnvironmentExtensions();

        public bool ShouldSerializescenes()
        {
            return scenes.Count > 0;
        }

        public int scene = -1;
        public bool ShouldSerializescene()
        {
            return ShouldSerializescenes() && scene >= 0 && scene < scenes.Count;
        }

    }
}
