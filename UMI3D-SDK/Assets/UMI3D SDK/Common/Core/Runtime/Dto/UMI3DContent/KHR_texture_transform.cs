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
    /// glTF 2.0 extension for texture tilling and offset.
    /// </summary>
    /// See <a href="https://github.com/KhronosGroup/glTF/blob/master/extensions/2.0/Khronos/KHR_texture_transform/README.md"/>
    [System.Serializable]
    public class KHR_texture_transform
    {
        public SerializableVector2 scale = Vector2.one;
        public SerializableVector2 offset = Vector2.zero;
        public float rotation = 0f;
    }
}
