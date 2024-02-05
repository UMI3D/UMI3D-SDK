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

namespace umi3d.common.collaboration.dto.signaling
{
    /// <summary>
    /// DTO describing a publicly visible identity. All browsers can see this identity.
    /// </summary>
    [Serializable]
    public class PublicIdentityDto : UMI3DDto
    {
        /// <summary>
        /// Id of the user
        /// </summary>
        public ulong userId { get; set; } = 0;

        /// <summary>
        /// Public login attributed to the user.
        /// </summary>
        public string login { get; set; } = null;

        /// <summary>
        /// Public name attributed to the user and that should be displayed on browsers.
        /// </summary>
        public string displayName { get; set; } = null;
    }
}