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

namespace umi3d.common.collaboration
{
    /// <summary>
    /// DTO describing a base connection message to connect to a media.
    /// </summary>
    public class ConnectionDto : UMI3DDto
    {
        /// <summary>
        /// String that state the langue of a user.
        /// </summary>
        public string language;

        /// <summary>
        /// Globaltoken previously used in the media the client want to connect to.
        /// </summary>
        public string globalToken;

        /// <summary>
        /// metaData previously used in the media the client want to connect to.
        /// </summary>
        public byte[] metadata;

        /// <summary>
        /// Gate data to help the environment where to redirect the user.
        /// </summary>
        public GateDto gate;

        /// <summary>
        /// Is the client only wanting to download a library rather than connecting right now?
        /// </summary>
        /// If false : the client to connect normaly. <br/>
        /// If true : the client ask only for the library it will need in a later connection.
        public bool libraryPreloading;
    }
}