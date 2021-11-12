﻿/*
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
    /// Upload file parameter dto
    /// </summary>
    public class UploadFileParameterDto : AbstractParameterDto<string>
    {
        public UploadFileParameterDto() : base() { }

        /// <summary>
        /// Only these extensions could be upload by the client. Enpty list = allow all, the extensions contain a dot (".obj" for exemple)
        /// </summary>
        public List<string> authorizedExtensions = new List<string>();
    }
}
