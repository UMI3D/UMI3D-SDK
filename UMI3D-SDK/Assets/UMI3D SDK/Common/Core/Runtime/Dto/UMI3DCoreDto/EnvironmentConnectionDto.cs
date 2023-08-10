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

namespace umi3d.common
{
    /// <summary>
    /// Essential data to enable the connection to an environment using a Forge server.
    /// </summary>
    [System.Serializable]
    public class EnvironmentConnectionDto : UMI3DDto
    {
        /// <summary>
        /// Name of the server
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Http url
        /// </summary>
        public string httpUrl { get; set; }

        /// <summary>
        /// Url of the resources server. Resources at this url will be concidered as from the media itself.
        /// </summary>
        public string resourcesUrl { get; set; }

        /// <summary>
        /// Should the authorization token should be in the url or in the header
        /// </summary>
        public bool authorizationInHeader { get; set; }

        /// <summary>
        /// Url of the forge server for websocket.
        /// </summary>
        public string forgeHost { get; set; }

        /// <summary>
        /// Url of the Master server if any
        /// </summary>
        public string forgeMasterServerHost { get; set; }

        /// <summary>
        /// Url of the nat server if any
        /// </summary>
        public string forgeNatServerHost { get; set; }

        /// <summary>
        /// Port of the forge server
        /// </summary>
        public ushort forgeServerPort { get; set; }

        /// <summary>
        /// Port of the master server
        /// </summary>
        public ushort forgeMasterServerPort { get; set; }

        /// <summary>
        /// Port of the nat server
        /// </summary>
        public ushort forgeNatServerPort { get; set; }

        /// <summary>
        /// Umi3d version of the environment.
        /// </summary>
        /// Versions are Major.Minor.Status.Date
        public string version { get; set; }

        public EnvironmentConnectionDto() : base() { }
    }
}
