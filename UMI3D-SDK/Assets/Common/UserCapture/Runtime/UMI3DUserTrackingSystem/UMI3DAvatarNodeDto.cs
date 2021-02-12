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

namespace umi3d.common.userCapture
{
    /// <summary>
    /// Abstract class to represent the root node of one user's representation.
    /// </summary>
    [Serializable]
    public class UMI3DAvatarNodeDto : UMI3DNodeDto
    {
        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        public string userId;

        /// <summary>
        /// A bool to enable or disable bindings
        /// </summary>
        public bool activeBindings;

        /// <summary>
        /// A list of bindings between the user's bones and their representations.
        /// </summary>
        public List<BoneBindingDto> bindings;
    }
}
