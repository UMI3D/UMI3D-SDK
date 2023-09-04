/*
Copyright 2019 - 2023 Inetum

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
using BeardedManStudios.Forge.Networking;
using inetum.unityUtils;
using System;
using umi3d.common;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;

namespace umi3d.cdk.collaboration
{
    public interface IUMI3DWorldInformation : IUMI3DMasterServerInformation
    {

    }

    public interface IUMI3DMasterServerInformation
    {
        /// <summary>
        /// Whether or not the client is connected.
        /// </summary>
        bool isConnected { get; }

        /// <summary>
        /// Get the requested information about this master server asynchronously.
        /// 
        /// <para>
        /// The request is performed in another thread.
        /// </para>
        /// </summary>
        /// <param name="requestServerInfSucceeded">Action raise when a request info has succeeded.</param>
        /// <param name="requestFailed">Action raise when a request info has failed.</param>
        public void RequestServerInfo(Action<(string serverName, string icon)> requestServerInfSucceeded, Action requestFailed);

        /// <summary>
        /// Get the requested information about this master server's <paramref name="sessionId"/> asynchronously.
        /// </summary>
        /// <param name="sessionId">Id of the session.</param>
        /// <param name="requestSessionInfSucceeded">Action raise when a request info has succeeded.</param>
        /// <param name="requestFailed">Action raise when a request info has failed.</param>
        public void RequestSessionInfo(string sessionId, Action<MasterServerResponse.Server> requestSessionInfSucceeded, Action requestFailed);
    }

    public interface IUMI3DWorldControllerInformation
    {
        /// <summary>
        /// Whether or not a connection or redirection is in progress.
        /// </summary>
        bool isConnectingOrRedirecting { get; }

        /// <summary>
        /// The status of the user in the server.
        /// </summary>
        StatusType status { get; }

        #region Shortcut data

        /// <summary>
        /// The current url to the media dto.
        /// </summary>
        public string MediaDtoUrl { get; }

        /// <summary>
        /// The current url where the connection dto is sent.
        /// </summary>
        public string ConnectionDtoUrl { get; }

        /// <summary>
        /// Name of the world.
        /// </summary>
        public string WorldName { get; }

        /// <summary>
        /// Name of the environment.
        /// </summary>
        public string EnvironmentName { get; }

        /// <summary>
        /// Create a new UMI3DVersion.Version corresponding to the version of the environment.
        /// </summary>
        public UMI3DVersion.Version EnvironmentUMI3DVersion { get; }

        #endregion

        #region Dtos

        /// <summary>
        /// Called to create a new Public Identity for this client.
        /// </summary>
        public PublicIdentityDto PublicIdentity { get; }

        /// <summary>
        /// Called to create a new Identity for this client.
        /// </summary>
        public IdentityDto Identity { get; }

        /// <summary>
        /// Create a new media dto from the one that is currently used.
        /// </summary>
        public MediaDto MediaDto { get; }

        /// <summary>
        /// Create a new gate dto from the one that is currently used.
        /// </summary>
        public GateDto GateDto { get; }

        /// <summary>
        /// Create a new environment connection dto from the one that is currently used.
        /// </summary>
        public EnvironmentConnectionDto EnvironmentConnectionDto { get; }

        #endregion
    }
}
