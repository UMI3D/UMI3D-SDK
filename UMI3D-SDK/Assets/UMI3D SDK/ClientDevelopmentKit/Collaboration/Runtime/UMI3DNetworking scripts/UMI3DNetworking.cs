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
using umi3d.cdk.collaboration;
using umi3d.common;
using umi3d.debug;
using UnityEngine;
using UnityEngine.Networking;

namespace umi3d.cdk
{
    /// <summary>
    /// Allow the browser to communicate with a distant server.
    /// 
    /// <para>
    /// With this class the user can:
    /// <list type="bullet">
    /// <item>Send Web request (WR).</item>
    /// <item>Try to connect to a Master Server (LoMS).</item>
    /// <item>Try to connect to a World Controller (LoWC).</item>
    /// </list>
    /// </para>
    /// </summary>
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
        public static UMI3DAsyncOperation Connect_LoMS(string url, Action connectSucceeded, Action connectFailed)
        {
            return LauncherOnMasterServer.Connect(url, connectSucceeded, connectFailed);
        }

        /// <summary>
        /// Disconnect a master server asynchronously.
        /// </summary>
        public static UMI3DAsyncOperation Disconnect_LoMS()
        {
            return LauncherOnMasterServer.Disconnect();
        }

        /// <summary>
        /// Get the requested information about this master server asyncronously.
        /// 
        /// <para>
        /// The request is performed in another thread.
        /// </para>
        /// </summary>
        /// <param name="requestServerInfSucceeded">Action raise when a request info has succeeded.</param>
        /// <param name="requestFailed">Action raise when a request info has failed.</param>
        public static void RequestServerInfo_LoMS(Action<(string serverName, string icon)> requestServerInfSucceeded, Action requestFailed)
        {
            LauncherOnMasterServer.RequestServerInfo(requestServerInfSucceeded, requestFailed);
        }

        /// <summary>
        /// Get the requested information about this master server's <paramref name="sessionId"/> asyncronously.
        /// </summary>
        /// <param name="sessionId">Id of the session.</param>
        /// <param name="requestSessionInfSucceeded">Action raise when a request info has succeeded.</param>
        /// <param name="requestFailed">Action raise when a request info has failed.</param>
        public static void RequestSessionInfo_LoMS(string sessionId, Action<MasterServerResponse.Server> requestSessionInfSucceeded, Action requestFailed)
        {
            LauncherOnMasterServer.RequestSessionInfo(sessionId, requestSessionInfSucceeded, requestFailed);
        }

        #endregion

        #region LauncherOnWorldController

        /// <summary>
        /// Send a request to get a <see cref="MediaDto"/>.
        /// </summary>
        /// <param name="RawURL">A simplified version of the url where a media dto can be requested.</param>
        /// <param name="requestSucceeded">Action raised when a media dto has been found.</param>
        /// <param name="requestFailed">Action raised when the request failed.</param>
        /// <param name="shouldCleanAbort">Whether or not the request has been interrupted.</param>
        /// <param name="tryCount">The number of try.</param>
        /// <param name="maxTryCount">The maximum number of try before giving up.</param>
        /// <param name="report">A log report.</param>
        /// <returns></returns>
        public static Coroutine RequestMediaDto_LoWC(
            string RawURL,
            Action<MediaDto> requestSucceeded, Action<int> requestFailed, Func<bool> shouldCleanAbort,
            int tryCount = 1, int maxTryCount = 3, UMI3DLogReport report = null
        )
        {
            return LauncherOnWorldController.RequestMediaDto(RawURL, requestSucceeded, requestFailed, shouldCleanAbort, tryCount, maxTryCount, report);
        }

        #endregion
    }
}