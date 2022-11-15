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
    /// DTO describing an identity, a set of identifying info of the user.
    /// </summary>
    [Serializable]
    public class IdentityDto : PublicIdentityDto
    {
        /// <summary>
        /// Local token of the user.
        /// </summary>
        public string localToken;
        public string headerToken;
        public string key;
        public string guid;
    }
}