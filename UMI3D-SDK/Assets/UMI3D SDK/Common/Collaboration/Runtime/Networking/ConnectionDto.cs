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
    /// BaseConnection message to connect to a media.
    /// </summary>
    public class ConnectionDto : UMI3DDto
    {
        /// <summary>
        /// Globaltoken previously used in the media the client want to connect to.
        /// </summary>
        public string GlobalToken;

        /// <summary>
        /// Gate data to help the media where to redirect the user.
        /// </summary>
        public GateDto gate;

        /// <summary>
        /// State if the client want only to download library.
        /// if false : the client to connect normaly.
        /// if true : the client ask only for the library it will need in a later connection.
        /// </summary>
        public bool LibraryPreloading;
    }
}