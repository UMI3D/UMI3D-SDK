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

namespace umi3d.common
{
    /// <summary>
    /// DTO describing a list of files using their path.
    /// </summary>
    /// Typically used for listing files in a library.
    [System.Serializable]
    public class FileListDto : UMI3DDto
    {
        /// <summary>
        /// Base URL of all files in the list.
        /// </summary>
        public string baseUrl;

        /// <summary>
        /// Relative URLs of files in the list to <see cref="baseUrl"/>.
        /// </summary>
        public List<string> files = new List<string>();
    }
}