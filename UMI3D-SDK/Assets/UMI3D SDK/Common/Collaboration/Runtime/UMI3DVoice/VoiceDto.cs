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

namespace umi3d.common.collaboration
{
    /// <summary>
    /// DTO describing a vocal server configuration.
    /// </summary>
    /// This server can be a Murmur server, for example.
    public class VoiceDto
    {
        /// <summary>
        /// URL of the vocal server.
        /// </summary>
        public string url;

        /// <summary>
        /// Login credential to connect to the vocal server.
        /// </summary>
        public string login;

        /// <summary>
        /// Password credential to connect to the vocal server.
        /// </summary>
        public string password;

        /// <summary>
        /// Name of the channel to join in the server.
        /// </summary>
        public string channelName;
    }
}