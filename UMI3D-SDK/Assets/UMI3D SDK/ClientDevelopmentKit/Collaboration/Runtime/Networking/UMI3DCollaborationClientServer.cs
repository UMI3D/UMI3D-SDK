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

using inetum.unityUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Collaboration Extension of the UMI3DClientServer
    /// </summary>
    public class UMI3DCollaborationClientServer : UMI3DClientServer
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Networking;

        public static new UMI3DCollaborationClientServer Instance { get => UMI3DClientServer.Instance as UMI3DCollaborationClientServer; set => UMI3DClientServer.Instance = value; }
        public static bool useDto => environmentClient?.useDto ?? false;

        private static UMI3DWorldControllerClient worldControllerClient;
        private static UMI3DEnvironmentClient environmentClient;

        public static PublicIdentityDto PublicIdentity => worldControllerClient?.PublicIdentity;

        override protected ForgeConnectionDto connectionDto => environmentClient?.connectionDto;

        public UnityEvent OnLeaving = new UnityEvent();
        public UnityEvent OnLeavingEnvironment = new UnityEvent();

        public UnityEvent OnNewToken = new UnityEvent();
        public UnityEvent OnConnectionLost = new UnityEvent();
        public UnityEvent OnRedirection = new UnityEvent();

        public ClientIdentifierApi Identifier;

        public StatusType status
        {
            get => environmentClient?.status ?? StatusType.NONE;
            set
            {
                if (environmentClient != null)
                    environmentClient.status = value;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Clear();
        }

        public async void Clear()
        {
            if (Exists)
            {
                worldControllerClient?.Clear();
                await environmentClient?.Clear();
            }
        }

        private void Start()
        {
            UMI3DNetworkingHelper.AddModule(new UMI3DCollaborationNetworkingModule());
            UMI3DNetworkingHelper.AddModule(new common.collaboration.UMI3DCollaborationNetworkingModule());
        }


        /// <summary>
        /// State if the Client is connected to a Server.
        /// </summary>
        /// <returns>True if the client is connected.</returns>
        public static bool Connected()
        {
            return environmentClient?.IsConnected() ?? false;
        }

        /// <summary>
        /// Start the connection to a Master Server.
        /// </summary>
        public static async void Connect(RedirectionDto redirection)
        {
            if (Exists)
            {
                Instance.status = StatusType.AWAY;
                UMI3DWorldControllerClient wc = worldControllerClient?.Redirection(redirection) ?? new UMI3DWorldControllerClient(redirection);
                if (await wc.Connect())
                {
                    Instance.OnRedirection.Invoke();

                    var env = environmentClient;
                    environmentClient = null;
                    UMI3DEnvironmentLoader.Clear();

                    if (env != null)
                        await env.Logout();
                    if (worldControllerClient != null)
                        worldControllerClient.Logout();

                    //Connection will not restart without this...
                    await Task.Yield();

                    worldControllerClient = wc;
                    environmentClient = wc.ConnectToEnvironment();
                    environmentClient.status = StatusType.ACTIVE;
                }
            }
        }

        public static void Connect(MediaDto dto)
        {
            Connect(new RedirectionDto()
            {
                media = dto,
                gate = null
            });
        }

        public static async void Logout()
        {
            if (environmentClient != null)
                await environmentClient.Logout();
            if (worldControllerClient != null)
                worldControllerClient.Logout();
            if (Exists)
            {
                Instance.OnLeavingEnvironment.Invoke();
                Instance.OnLeaving.Invoke();
            }
        }

        /// <summary>
        /// Logout of the current server
        /// </summary>
        public static void EnvironmentLogout(Action success, Action<string> failled)
        {
            if (Exists)
                Instance._EnvironmentLogout(success, failled);
        }

        private async void _EnvironmentLogout(Action success, Action<string> failled)
        {
            if (await environmentClient.Logout())
                success?.Invoke();
            else
                failled?.Invoke("Failled to Logout");
            Instance.OnLeavingEnvironment.Invoke();
        }


        /// <summary>
        /// Notify that the connection with the server was lost.
        /// </summary>
        public void ConnectionLost(UMI3DEnvironmentClient client)
        {
            if (environmentClient == client)
            {
                UMI3DCollaborationClientServer.EnvironmentLogout(null, null);

                OnConnectionLost.Invoke();
            }
        }


        /// <summary>
        /// Retry a failed http request
        /// </summary>
        /// <param name="argument">failed request argument</param>
        /// <returns></returns>
        public override async Task<bool> TryAgainOnHttpFail(RequestFailedArgument argument)
        {
            if (argument.ShouldTryAgain(argument))
            {
                UMI3DLogger.LogWarning($"Http request failed [{argument}], try again", scope | DebugScope.Connection);
                return await TryAgain(argument);
            }
            UMI3DLogger.LogError($"Http request failed [{argument}], abort", scope | DebugScope.Connection);
            return false;
        }

        /// <summary>
        /// launch a new request
        /// </summary>
        /// <param name="argument">argument used in the request</param>
        /// <returns></returns>
        private async Task<bool> TryAgain(RequestFailedArgument argument)
        {
            bool needNewToken = environmentClient != null && environmentClient.IsConnected() && argument.GetRespondCode() == 401 && (environmentClient.lastTokenUpdate - argument.date).TotalMilliseconds < 0;
            if (needNewToken)
            {
                UnityAction a = () => needNewToken = false;
                UMI3DCollaborationClientServer.Instance.OnNewToken.AddListener(a);
                while (environmentClient != null && environmentClient.IsConnected() && needNewToken && !((DateTime.UtcNow - argument.date).TotalMilliseconds > environmentClient.maxMillisecondToWait))
                    await UMI3DAsyncManager.Yield();
                UMI3DCollaborationClientServer.Instance.OnNewToken.RemoveListener(a);
                return environmentClient != null && environmentClient.IsConnected();
            }
            return false;
        }

        /// <summary>
        /// Get a media dto at a raw url using a get http request.
        /// The result is store in UMI3DClientServer.Media.
        /// </summary>
        /// <param name="url">Url used for the get request.</param>
        /// <seealso cref="UMI3DCollaborationClientServer.Media"/>
        public static async Task<MediaDto> GetMedia(string url, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Get media at {url}", scope | DebugScope.Connection);
            return await HttpClient.SendGetMedia(url, shouldTryAgain);
        }

        /// <summary>
        /// Send a BrowserRequestDto on a RTC
        /// </summary>
        /// <param name="dto">Dto to send</param>
        /// <param name="reliable">is the data channel used reliable</param>
        protected override void _Send(AbstractBrowserRequestDto dto, bool reliable)
        {
            environmentClient?.Send(dto, reliable);
        }

        /// <summary>
        /// Send Tracking BrowserRequest
        /// </summary>
        /// <param name="dto">Dto to send</param>
        /// <param name="reliable">is the data channel used reliable</param>
        protected override void _SendTracking(AbstractBrowserRequestDto dto)
        {
            environmentClient?.SendTracking(dto);
        }


        public static void SendVOIP(int length, byte[] sample)
        {
            if (Exists && Connected()
                && Instance.status == StatusType.ACTIVE)
                environmentClient?.SendVOIP(length, sample);
        }



        ///<inheritdoc/>
        protected override async Task<byte[]> _GetFile(string url)
        {
            UMI3DLogger.Log($"GetFile {url}", scope);
            return await environmentClient?.GetFile(url);
        }

        ///<inheritdoc/>
        protected override async Task<LoadEntityDto> _GetEntity(List<ulong> ids)
        {
            UMI3DLogger.Log($"GetEntity {ids.ToString<ulong>()}", scope);
            return await environmentClient?.GetEntity(ids);
        }

        ///<inheritdoc/>
        public override ulong GetUserId() { return worldControllerClient?.GetUserID() ?? 0; }

        ///<inheritdoc/>
        public override ulong GetTime()
        {
            return environmentClient?.TimeStep ?? 0;
        }

        ///<inheritdoc/>
        public override double GetRoundTripLAtency() { return environmentClient?.ForgeClient?.RoundTripLatency ?? 0; }

        ///<inheritdoc/>
        protected override string _getAuthorization() { return environmentClient?.HttpClient.HeaderToken; }

        /// <summary>
        /// return HTTPClient if the server is a collaboration server.
        /// </summary>
        public override object GetHttpClient() { return environmentClient?.HttpClient; }

    }
}