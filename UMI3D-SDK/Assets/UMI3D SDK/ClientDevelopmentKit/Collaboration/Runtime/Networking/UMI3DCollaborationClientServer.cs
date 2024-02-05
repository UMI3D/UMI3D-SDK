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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.userCapture;
using UnityEngine.Events;

namespace umi3d.cdk.collaboration
{

    public class OnForceLogoutEvent : UnityEvent<string> { }

    public class OnProgressEvent : UnityEvent<Progress> { }

    /// <summary>
    /// UMI3D server on the browser, in a collaborative context.
    /// </summary>
    public class UMI3DCollaborationClientServer : UMI3DClientServer, IUMI3DCollaborationClientServer
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Networking;

        public static new UMI3DCollaborationClientServer Instance { get => UMI3DClientServer.Instance as UMI3DCollaborationClientServer; set => UMI3DClientServer.Instance = value; }
        /// <summary>
        /// Should serialization be done using json DTOs rather than byte containers?
        /// </summary>
        public static bool useDto => environmentClient?.useDto ?? false;

        public static PendingTransactionDto transactionPending = null;

        private static UMI3DWorldControllerClient worldControllerClient;
        private static UMI3DEnvironmentClient environmentClient;

        public static PublicIdentityDto PublicIdentity => worldControllerClient?.PublicIdentity;

        protected override EnvironmentConnectionDto connectionDto => environmentClient?.connectionDto;
        public override UMI3DVersion.Version version => environmentClient?.version;

        public static Func<MultiProgress> EnvironmentProgress = null;

        static public OnProgressEvent onProgress = new OnProgressEvent();

        public OnForceLogoutEvent OnForceLogoutMessage { get; } = new OnForceLogoutEvent();

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

        public string environementName => environmentClient?.connectionDto?.name;
        public string environementHttpUrl => environmentClient?.connectionDto?.httpUrl;
        public string worldName => worldControllerClient?.name;

        /// <inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Clear();
        }

        /// <summary>
        /// Makes all client log out and delete references to them.
        /// </summary>
        public async void Clear()
        {
            if (Exists)
            {
                worldControllerClient?.Clear();
                if (environmentClient != null) await environmentClient?.Clear();
                worldControllerClient = null;
                environmentClient = null;
                IsRedirectionInProgress = false;
            }
        }

        private void Start()
        {
            _ = UMI3DCollaborationEnvironmentLoader.Instance; // force right service instanciation
            UMI3DSerializer.AddModule(UMI3DSerializerModuleUtils.GetModules().ToList());

            //UMI3DSerializer.AddModule(new UMI3DSerializerBasicModules());
            //UMI3DSerializer.AddModule(new UMI3DSerializerStringModules());
            //UMI3DSerializer.AddModule(new UMI3DSerializerVectorModules());
            //UMI3DSerializer.AddModule(new UMI3DSerializerAnimationModules());
            //UMI3DSerializer.AddModule(new UMI3DSerializerShaderModules());
            //UMI3DSerializer.AddModule(new UMI3DUserCaptureBindingSerializerModule());
            //UMI3DSerializer.AddModule(new UMI3DEmotesSerializerModule());
            //UMI3DSerializer.AddModule(new UMI3DCollaborationSerializerModule());
            //UMI3DSerializer.AddModule(new common.collaboration.UMI3DCollaborationSerializerModule());
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
        /// Reconnect to the last environment
        /// </summary>
        public static async void Reconnect()
        {
            if (worldControllerClient != null)
            {
                Instance.OnReconnect.Invoke();
                UMI3DEnvironmentLoader.Clear(false);

                MultiProgress progress = new MultiProgress("Reconnect");
                onProgress.Invoke(progress);

                environmentClient = await worldControllerClient.ConnectToEnvironment(progress);
                environmentClient.status = StatusType.CREATED;
            }
        }

        /// <summary>
        /// Start the connection to a Master Server.
        /// </summary>
        public static async void Connect(RedirectionDto redirection, Action<string> failed = null)
        {
            if (!Exists)
            {
                failed?.Invoke("No Intance of UMI3DCollaborationServer");
                return;
            }

            if (UMI3DCollaborationClientServer.Instance.IsRedirectionInProgress)
            {
                failed?.Invoke("Redirection already in progress");
                return;
            }
            bool aborted = false;
            UMI3DCollaborationClientServer.Instance.IsRedirectionInProgress = true;
            Instance.OnRedirectionStarted.Invoke();

            try
            {
                if (Exists)
                {
                    Instance.status = StatusType.AWAY;
                    UMI3DWorldControllerClient wc = worldControllerClient?.Redirection(redirection) ?? new UMI3DWorldControllerClient(redirection);
                    if (await wc.Connect())
                    {
                        Instance.OnRedirection.Invoke();
                        loadingEntities.Clear();
                        UMI3DEnvironmentLoader.Clear(false);

                        UMI3DEnvironmentClient env = environmentClient;
                        environmentClient = null;
                        //UMI3DEnvironmentLoader.Clear();

                        if (env != null)
                            await env.Logout();
                        if (worldControllerClient != null)
                            worldControllerClient.Logout();

                        //Connection will not restart without this...
                        await Task.Yield();

                        MultiProgress progress = EnvironmentProgress?.Invoke() ?? new MultiProgress("Joinning Environment");
                        onProgress.Invoke(progress);

                        worldControllerClient = wc;
                        environmentClient = await wc.ConnectToEnvironment(progress);
                        environmentClient.status = StatusType.CREATED;
                    }
                }
                else
                {
                    failed?.Invoke("Client Server do not exist");
                    aborted = true;
                }
            }
            catch (Exception e)
            {
                UMI3DLogger.Log($"Error in connection process", scope);
                UMI3DLogger.LogException(e, scope);
                failed?.Invoke(e.Message);
                aborted = true;
            }
            UMI3DCollaborationClientServer.Instance.IsRedirectionInProgress = false;
            if (aborted)
                Instance.OnRedirectionAborted.Invoke();
        }

        public static void Connect(MediaDto dto, Action<string> failed = null)
        {
            Connect(new RedirectionDto()
            {
                media = dto,
                gate = null
            }, failed);
        }

        public static async void Logout()
        {
            if (environmentClient != null)
            {
                await environmentClient.Logout();
                environmentClient = null;
            }

            if (worldControllerClient != null)
                worldControllerClient.Logout();
            if (Exists)
            {
                Instance.OnLeavingEnvironment.Invoke();
                Instance.OnLeaving.Invoke();
                Instance.IsRedirectionInProgress = false;
            }

        }


        public static void ReceivedLogoutMessage(string message)
        {
            if (Exists)
                Instance.OnForceLogoutMessage.Invoke(message);
            Logout();
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
            environmentClient = null;
            Instance.OnLeavingEnvironment.Invoke();
        }

        public void ConnectionStatus(UMI3DEnvironmentClient client, bool lost)
        {
            if (client == environmentClient)
                try
                {
                    if (lost)
                        OnConnectionCheck.Invoke();
                    else
                        OnConnectionRetreived.Invoke();
                }
                catch (Exception e)
                {
                    UMI3DLogger.LogError($"Error in ConnectionStatus event : {e.Message} \n {e.StackTrace}", scope);
                }
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
        public override void SendTracking(AbstractBrowserRequestDto dto)
        {
            environmentClient?.SendTracking(dto);
        }

        /// <summary>
        /// Send a vocal message through VoIP.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="sample"></param>
        public static void SendVOIP(int length, byte[] sample)
        {
            if (Exists && Connected()
                && Instance.status == StatusType.ACTIVE)
                environmentClient?.SendVOIP(length, sample);
        }



        /// <inheritdoc/>
        protected override async Task<byte[]> _GetFile(string url, bool useParameterInsteadOfHeader)
        {
            UMI3DLogger.Log($"GetFile {url}", scope);
            return await (environmentClient?.GetFile(url, useParameterInsteadOfHeader) ?? Task.FromResult<byte[]>(null));
        }

        /// <inheritdoc/>
        protected override async Task<LoadEntityDto> _GetEntity(List<ulong> ids)
        {
            List<ulong> idsToSend = new List<ulong>();
            foreach (ulong id in ids)
            {
                if (loadingEntities.Add(id))
                {
                    idsToSend.Add(id);
                }
                else
                {
                    UMI3DLogger.Log($"Cancel GetEntity {id}", scope);
                }

            }

            UMI3DLogger.Log($"GetEntity {idsToSend.ToString<ulong>()}", scope);
            LoadEntityDto result = null;
            try
            {
                result = idsToSend.Count > 0 ?
                    await (environmentClient?.GetEntity(idsToSend) ?? Task.FromResult<LoadEntityDto>(null))
                    : new LoadEntityDto() { entities = new List<IEntity>() };
            }
            finally
            {

                foreach (var id in idsToSend)
                {
                    loadingEntities.Remove(id);
                }
                UMI3DLogger.Log($"Remove GetEntity {idsToSend.ToString<ulong>()} {loadingEntities.ToString<ulong>()}", scope);
            }


            return result;
        }

        private static SortedSet<ulong> loadingEntities = new SortedSet<ulong>();


        /// <inheritdoc/>
        public override ulong GetUserId() { return worldControllerClient?.GetUserID() ?? 0; }

        /// <inheritdoc/>
        public UMI3DEnvironmentClient.UserInfo GetUser() { return environmentClient?.UserDto; }

        /// <inheritdoc/>
        public override ulong GetTime()
        {
            return environmentClient?.TimeStep ?? 0;
        }

        /// <inheritdoc/>
        public override double GetRoundTripLAtency() { return environmentClient?.ForgeClient?.RoundTripLatency ?? 0; }

        /// <inheritdoc/>
        protected override string _getAuthorization() { return environmentClient?.HttpClient.HeaderToken; }

        /// <summary>
        /// return HTTPClient if the server is a collaboration server.
        /// </summary>
        public override object GetHttpClient() { return environmentClient?.HttpClient; }
    }
}