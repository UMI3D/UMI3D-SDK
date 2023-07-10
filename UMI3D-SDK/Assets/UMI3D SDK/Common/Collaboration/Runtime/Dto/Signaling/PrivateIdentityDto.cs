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

namespace umi3d.common.collaboration.dto.signaling
{
    /// <summary>
    /// DTO describing idnetifiers that are known only from the user and the media.
    /// </summary>
    [Serializable]
    public class PrivateIdentityDto : IdentityDto
    {
        /// <summary>
        /// Global token of the user.
        /// </summary>
        public string globalToken { get; set; }

        /// <summary>
        /// Essential data to enable the connection to an environment using a Forge server.
        /// </summary>
        public EnvironmentConnectionDto connectionDto { get; set; }

        /// <summary>
        /// Libraries possessed by the user.
        /// </summary>
        public List<LibrariesDto> libraries { get; set; }
    }
}