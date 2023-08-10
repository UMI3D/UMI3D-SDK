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

namespace umi3d.common.interaction
{
    /// <summary>
    /// DTO describing a connection form.
    /// </summary>
    [System.Serializable]
    public class ConnectionFormDto : AbstractInteractionDto
    {
        /// <summary>
        /// Fields of the form that are themselves DTOs as <see cref="AbstractParameterDto"/>.
        /// </summary>
        public List<AbstractParameterDto> fields { get; set; } = new List<AbstractParameterDto>();

        /// <summary>
        /// Globaltoken previously used in the media the client want to connect to.
        /// </summary>
        public string globalToken { get; set; }

        /// <summary>
        /// array that can be use to store data.
        /// </summary>
        public byte[] metadata { get; set; }

        public ConnectionFormDto() : base() { }
    }
}
