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

namespace umi3d.cdk.collaboration
{
    public interface IUMI3DWorldConnection : IUMI3DMasterServerConnection
    {
        
    }

    public interface IUMI3DMasterServerConnection
    {
        /// <summary>
        /// Try to connect to a master server asynchronously.
        /// 
        /// <para>
        /// The connection is established in another thread. The <paramref name="connectSucceeded"/> and <paramref name="connectFailed"/> actions are raised in the main-thread.
        /// </para>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="connectSucceeded">Action raise when the connection succeeded.</param>
        /// <param name="connectFailed">Action raise when the connection failed.</param>
        /// <returns></returns>
        UMI3DAsyncOperation Connect_MSC(string url, Action connectSucceeded, Action connectFailed);

        /// <summary>
        /// Get the requested information about this master server asynchronously.
        /// 
        /// <para>
        /// The request is performed in another thread.
        /// </para>
        /// </summary>
        /// <param name="requestServerInfSucceeded">Action raise when a request info has succeeded.</param>
        /// <param name="requestFailed">Action raise when a request info has failed.</param>
        public void RequestServerInfo_MSC(Action<(string serverName, string icon)> requestServerInfSucceeded, Action requestFailed);

        /// <summary>
        /// Get the requested information about this master server's <paramref name="sessionId"/> asynchronously.
        /// </summary>
        /// <param name="sessionId">Id of the session.</param>
        /// <param name="requestSessionInfSucceeded">Action raise when a request info has succeeded.</param>
        /// <param name="requestFailed">Action raise when a request info has failed.</param>
        public void RequestSessionInfo_MSC(string sessionId, Action<MasterServerResponse.Server> requestSessionInfSucceeded, Action requestFailed);
    }
}
