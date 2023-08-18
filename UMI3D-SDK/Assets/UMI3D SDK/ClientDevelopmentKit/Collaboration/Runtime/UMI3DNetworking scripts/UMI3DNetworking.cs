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
using System.Collections;
using System.Collections.Generic;
using umi3d.debug;
using UnityEngine.Networking;

namespace umi3d.cdk
{
    public static class UMI3DNetworking
    {
        #region WebRequest

        /// <summary>
        /// Send an HTTP Get Request.
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="url"></param>
        /// <param name="shouldCleanAbort"></param>
        /// <param name="onCompleteSuccess"></param>
        /// <param name="onCompleteFail"></param>
        /// <param name="report"></param>
        /// <returns></returns>
        public static IEnumerator Get_WR(
            (string token, List<(string, string)> headers) credentials,
            string url,
            Func<bool> shouldCleanAbort,
            Action<UnityWebRequestAsyncOperation> onCompleteSuccess,
            Action<UnityWebRequestAsyncOperation> onCompleteFail,
            UMI3DLogReport report = null
        )
        {
            return UMI3DWebRequest.Get(
                credentials, url,
                shouldCleanAbort, onCompleteSuccess, onCompleteFail,
                report
            );
        }

        #endregion

        #region LauncherOnMasterServer

        /// <summary>
        /// Event raise when the connection to a master server failed.
        /// </summary>
        public static event Action connectFailed_LoMS
        {
            add
            {
                LauncherOnMasterServer.connectFailed += value;
            }
            remove
            {
                LauncherOnMasterServer.connectFailed += value;
            }
        }

        /// <summary>
        /// Event raise when the connection to a master server succeeded.
        /// </summary>
        public static event Action connectSucceeded_LoMS
        {
            add
            {
                LauncherOnMasterServer.connectSucceeded += value;
            }
            remove
            {
                LauncherOnMasterServer.connectSucceeded += value;
            }
        }

        /// <summary>
        /// Event raise when a request info to a master server has succeeded.
        /// </summary>
        public static event Action<(string serverName, string icon)> requestServerInfSucceeded_LoMS
        {
            add
            {
                LauncherOnMasterServer.requestServerInfSucceeded += value;
            }
            remove
            {
                LauncherOnMasterServer.requestServerInfSucceeded += value;
            }
        }

        /// <summary>
        /// Event raise when a request info has succeeded.
        /// </summary>
        public static event Action<MasterServerResponse.Server> requestSessionInfSucceeded
        {
            add
            {
                LauncherOnMasterServer.requestSessionInfSucceeded += value;
            }
            remove
            {
                LauncherOnMasterServer.requestSessionInfSucceeded -= value;
            }
        }

        /// <summary>
        /// Try to connect to a master server asyncronously.
        /// 
        /// <para>
        /// The connection is established in another thread. The <see cref="connectFailed"/> and <see cref="connectSucceeded"/> events are raised in the main-thread.
        /// </para>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static AsyncOperation ConnectAsync_LoMS(string url)
        {
            return LauncherOnMasterServer.ConnectAsync(url);
        }

        /// <summary>
        /// Disconnect a master server.
        /// </summary>
        public static void Disconnect_LoMS()
        {
            LauncherOnMasterServer.Disconnect();
        }

        /// <summary>
        /// Get the requested information about this master server asyncronously.
        /// 
        /// <para>
        /// The request is performed in another thread.
        /// </para>
        /// </summary>
        /// <param name="requestFailed"></param>
        public static void RequestServerInfo_LoMS(Action requestFailed)
        {
            LauncherOnMasterServer.RequestServerInfo(requestFailed);
        }

        /// <summary>
        /// Get the requested information about this master server's <paramref name="sessionId"/> asyncronously.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="requestFailed"></param>
        public static void RequestSessionInfo_LoMS(string sessionId, Action requestFailed)
        {
            LauncherOnMasterServer.RequestSessionInfo(sessionId, requestFailed);
        }

        #endregion
    }
}