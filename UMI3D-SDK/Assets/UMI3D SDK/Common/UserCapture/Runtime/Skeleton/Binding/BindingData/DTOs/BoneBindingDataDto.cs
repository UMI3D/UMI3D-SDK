/*
Copyright 2019 - 2023 Inetum

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

using umi3d.common.dto.binding;

namespace umi3d.common.userCapture.binding
{
    /// <summary>
    /// Bone binding data dto, required to load a bone binding.
    /// </summary>
    [System.Serializable]
    public class BoneBindingDataDto : AbstractSimpleBindingDataDto
    {
        /// <summary>
        /// The user to which the object i going to be binded ID
        /// </summary>
        public ulong userId { get; set; }

        /// <summary>
        /// The bone to which the object is going to be binded
        /// </summary>
        public uint boneType { get; set; }
    }
}