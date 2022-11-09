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

namespace umi3d.common.collaboration
{
    /// <summary>
    /// DTO describing an identity sent to the UMI3D server.
    /// </summary>
    /// Same than <see cref="IdentityDto"/> but with customizable metadata.
    [Serializable]
    public class RegisterIdentityDto : IdentityDto
    {
        /// <summary>
        /// Customizable data as a byte array. 
        /// </summary>
        /// The server should know how to parse them.
        public byte[] metaData;
    }
}