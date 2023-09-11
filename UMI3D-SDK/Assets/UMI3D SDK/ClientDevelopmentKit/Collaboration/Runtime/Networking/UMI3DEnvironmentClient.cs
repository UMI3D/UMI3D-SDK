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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Client for the UMI3D environment server, handles the connection to environments.
    /// </summary>
    /// The Environment Client singlely handles all that is connection and creates 
    /// an <see cref="umi3d.cdk.collaboration.HttpClient"/> and a <see cref="UMI3DForgeClient"/> to handle other messages.
    public class UMI3DEnvironmentClient
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Networking;

        private bool isJoinning, isConnecting, isConnected, needToGetFirstConnectionInfo, disconected;

        /// <summary>
        /// Is the client connected to the environment server?
        /// </summary>
        /// Require that the Forge client is connected.
        /// <returns></returns>
        public bool IsConnected()
        {
            return ForgeClient != null && isConnected && ForgeClient.IsConnected && !disconected;
        }

        public class ConnectionStateEvent : UnityEvent<string> { };
        public static ConnectionStateEvent ConnectionState = new ConnectionStateEvent();

        /// <summary>
        /// Max waiting time before attempting again a request that failed.
        /// </summary>
        public readonly double maxMillisecondToWait = 10000;

        /// <summary>
        /// Data for connection through the Forge server.
        /// </summary>
        public readonly EnvironmentConnectionDto connectionDto;

        /// <summary>
        /// Computed version of the environment
        /// </summary>
        public readonly UMI3DVersion.Version version;

        private readonly UMI3DWorldControllerClient worldControllerClient;

        static public UnityEvent EnvironementJoinned = new UnityEvent();
        static public UnityEvent EnvironementLoaded = new UnityEvent();

        /// <summary>
        /// Get current user UMI3D id.
        /// </summary>
        /// <returns></returns>
        public ulong GetUserID() { return UserDto.answerDto.id; }

        public DateTime lastTokenUpdate { get; private set; }

        #region clients

        /// <summary>
        /// Handles HTTP requests before connection or to retrieve DTOs.
        /// </summary>
        public HttpClient HttpClient { get; private set; }

        /// <summary>
        /// Handles most of the transaction-related message after connection.
        /// </summary>
        public UMI3DForgeClient ForgeClient { get; private set; }

        public ulong TimeStep => ForgeClient.GetNetWorker().Time.Timestep;

        #endregion clients

        /// <summary>
        /// Is the server using json serialization rather than byte containers?
        /// </summary>
        public bool useDto { get; private set; } = false;

        /// <summary>
        /// Status to set on the server.
        /// </summary>
        public StatusType _statusToBeSet = StatusType.ACTIVE;
        public StatusType statusToBeSet
        {
            get => _statusToBeSet;
            set
            {
                if (value == StatusType.AWAY || value == StatusType.ACTIVE)
                {
                    _statusToBeSet = value;
                }
            }
        }

        /// <summary>
        /// Status of the user attributed by the environmet.
        /// </summary>
        public StatusType status
        {
            get => IsConnected() ? UserDto.answerDto.status : StatusType.NONE;
            set
            {
                if (value == StatusType.AWAY || value == StatusType.ACTIVE)
                {
                    if (isConnected)
                    {
                        if (UserDto.answerDto.status != value)
                        {
                            UserDto.answerDto.status = value;
                            HttpClient.SendPostUpdateStatusAsync(UserDto.answerDto.status);
                        }
                    }
                    else
                        statusToBeSet = value;
                }
            }
        }

        #region loadingProgress

        MultiProgress progress;

        /// <summary>
        /// Progress of the joining process.
        /// </summary>
        MultiProgress joinProgress;

        /// <summary>
        /// Progress of the library downloading.
        /// </summary>
        MultiProgress libraryProgress;

        #endregion loadingProgress

        /// <summary>
        /// User specific information related to the environment.
        /// </summary>
        public class UserInfo
        {
            public ConnectionFormDto formdto;
            public UserConnectionAnswerDto answerDto;

            public string AudioPassword;

            public UserInfo()
            {
                formdto = new ConnectionFormDto();
                answerDto = new UserConnectionAnswerDto();
                AudioPassword = null;

            }

            public void Set(UserConnectionDto dto)
            {
                FormAnswerDto param = this.answerDto.parameters;
                this.answerDto = new UserConnectionAnswerDto()
                {
                    id = dto.id,
                    login = dto.login,
                    status = dto.status,

                    audioSourceId = dto.audioSourceId,
                    audioFrequency = dto.audioFrequency,
                    videoSourceId = dto.videoSourceId,
                    networkId = dto.networkId,

                    microphoneStatus = dto.microphoneStatus,
                    avatarStatus = dto.avatarStatus,
                    attentionRequired = dto.attentionRequired,

                    audioChannel = dto.audioChannel,
                    audioServerUrl = dto.audioServerUrl,
                    audioLogin = dto.audioLogin,
                    audioUseMumble = dto.audioUseMumble,

                    onStartSpeakingAnimationId = dto.onStartSpeakingAnimationId,
                    onStopSpeakingAnimationId = dto.onStopSpeakingAnimationId,
                    language = dto.language,

                    librariesUpdated = dto.librariesUpdated,

                    parameters = param
                };
                this.formdto = dto.parameters;
                this.AudioPassword = dto.audioPassword;
            }
        }

        /// <summary>
        /// Local copy of the user description.
        /// </summary>
        public UserInfo UserDto = new UserInfo();


        public UMI3DEnvironmentClient(EnvironmentConnectionDto connectionDto, UMI3DWorldControllerClient worldControllerClient, MultiProgress progress)
        {
            this.isJoinning = false;
            this.isConnecting = false;
            this.isConnected = false;
            this.disconected = false;
            this.connectionDto = connectionDto;
            this.version = new UMI3DVersion.Version(connectionDto.version);
            this.worldControllerClient = worldControllerClient;

            this.progress = progress;
            this.libraryProgress = new MultiProgress("Download libraries");
            this.joinProgress = new MultiProgress("Joining Environment");
            progress.Add(libraryProgress);
            progress.Add(joinProgress);

            lastTokenUpdate = default;
            HttpClient = new HttpClient(this);
            needToGetFirstConnectionInfo = true;
        }

        /// <summary>
        /// Connect to the environment.
        /// </summary>
        /// <returns>False if already connected or connecting. True otherwise.</returns>
        public bool Connect()
        {
            ConnectionState.Invoke("Connecting to the Environment");
            if (IsConnected())
                return false;

            if (isConnecting)
                return false;

            isConnecting = true;
            disconected = false;

            ForgeClient = UMI3DForgeClient.Create(this);
            ForgeClient.ip = connectionDto.forgeHost;
            ForgeClient.port = connectionDto.forgeServerPort;
            ForgeClient.masterServerHost = connectionDto.forgeMasterServerHost;
            ForgeClient.masterServerPort = connectionDto.forgeMasterServerPort;
            ForgeClient.natServerHost = connectionDto.forgeNatServerHost;
            ForgeClient.natServerPort = connectionDto.forgeNatServerPort;

            var Auth = new common.collaboration.UMI3DAuthenticator(GetLocalToken);
            SetToken(worldControllerClient.Identity.localToken);
            JoinForge(Auth);


            return true;
        }

        private async void JoinForge(BeardedManStudios.Forge.Networking.IUserAuthenticator authenticator)
        {
            ForgeClient.Join(authenticator);
            await UMI3DAsyncManager.Delay(4500);
            if (ForgeClient != null && !ForgeClient.IsConnected && !disconected)
            {
                ConnectionState.Invoke("Connection Failed");
                isConnecting = false;
                isConnected = false;
                ForgeClient.Stop();
                GameObject.Destroy(ForgeClient);
                await UMI3DAsyncManager.Delay(500);
                Connect();
            }
            else
            {
                EnvironementJoinned.Invoke();
            }
        }

        public void ConnectionStatus(bool lost)
        {
            if (UMI3DCollaborationClientServer.Exists)
                UMI3DCollaborationClientServer.Instance.ConnectionStatus(this, lost);
        }

        public async void ConnectionDisconnected()
        {
            UMI3DLogger.Log($"Connection lost with environment [Was Connected: {IsConnected()}]", scope);

            if (UMI3DCollaborationClientServer.Exists)
                UMI3DCollaborationClientServer.Instance.ConnectionLost(this);
            await Task.Yield();

            isConnecting = false;
            isConnected = false;
        }

        /// <summary>
        /// Log out from the environment.
        /// </summary>
        /// <param name="notify"></param>
        /// <returns></returns>
        public async Task<bool> Logout(bool notify = true)
        {
            bool ok = false;

            if (IsConnected())
            {
                try
                {
                    if (notify)
                        await HttpClient.SendPostLogout();
                }
                finally { };

                ForgeClient.Stop();
                ok = true;
            }
            disconected = true;
            isConnected = false;
            if (ForgeClient != null)
            {
                GameObject.Destroy(ForgeClient.gameObject);
                ForgeClient = null;
            }
            return ok;
        }

        /// <summary>
        /// Log out and destroy the client.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Clear()
        {
            if (IsConnected())
            {
                await Logout();
            }

            if (ForgeClient != null)
            {
                GameObject.Destroy(ForgeClient.gameObject);
                ForgeClient = null;
            }
            return true;
        }

        private void GetLocalToken(Action<string> callback)
        {
            callback?.Invoke(worldControllerClient.Identity.localToken);
        }


        /// <summary>
        /// Set the token used to communicate to the server.
        /// </summary>
        /// <param name="token"></param>
        public void SetToken(string token)
        {
            lastTokenUpdate = DateTime.UtcNow;
            HttpClient?.SetToken(token);
            BeardedManStudios.Forge.Networking.Unity.MainThreadManager.Run(
                () => UMI3DClientServer.StartCoroutine(OnNewTokenNextFrame())
                );
        }

        /// <summary>
        /// Handles the message comming from the websockekt server.
        /// </summary>
        /// <param name="message"></param>
        public async void OnMessage(object message)
        {
            try
            {
                switch (message)
                {
                    case TokenDto tokenDto:
                        SetToken(tokenDto.token);
                        break;
                    case StatusDto statusDto:
                        switch (statusDto.status)
                        {
                            case StatusType.CREATED:
                                needToGetFirstConnectionInfo = false;
                                UserConnectionDto user = await HttpClient.SendGetIdentity();
                                UpdateIdentity(user);
                                break;
                            case StatusType.READY:
                                if (needToGetFirstConnectionInfo)
                                {
                                    needToGetFirstConnectionInfo = false;
                                    UserConnectionDto _user = await HttpClient.SendGetIdentity();
                                    UserDto.Set(_user);
                                    Join(joinProgress);
                                }
                                else
                                {
                                    Join(joinProgress);
                                }

                                break;
                        }
                        break;
                    case StatusRequestDto statusRequestDto:
                        HttpClient.SendPostUpdateStatusAsync(UserDto.answerDto.status, true);
                        break;
                }
            }
            catch (Exception e)
            {
                UMI3DLogger.LogWarning($"Error on OnMessage({message})", scope);
                UMI3DLogger.LogException(e, scope);
                ConnectionDisconnected();
            }
        }

        /// <summary>
        /// Called when the client status is required to change.
        /// </summary>
        /// <param name="status">New status description.</param>
        public async void OnStatusChanged(StatusDto statusDto)
        {
            try
            {
                switch (statusDto.status)
                {
                    case StatusType.CREATED:
                        needToGetFirstConnectionInfo = false;
                        UserConnectionDto user = await HttpClient.SendGetIdentity();
                        UpdateIdentity(user);
                        break;
                    case StatusType.READY:
                        if (needToGetFirstConnectionInfo)
                        {
                            needToGetFirstConnectionInfo = false;
                            UserConnectionDto _user = await HttpClient.SendGetIdentity();
                            UserDto.Set(_user);
                            Join(joinProgress);
                        }
                        else
                        {
                            Join(joinProgress);
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                UMI3DLogger.LogWarning($"Error on OnStatusChanged({statusDto})", scope);
                UMI3DLogger.LogException(e, scope);
                ConnectionDisconnected();
            }
        }

        /// <summary>
        /// Coroutine to handle identity.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async void UpdateIdentity(UserConnectionDto user)
        {
            try
            {
                //UMI3DLogger.Log($"UpdateIdentity {user.id}", scope | DebugScope.Connection);
                UserDto.Set(user);
                //Identity.userId = user.id;
                bool Ok = true;
                bool librariesUpdated = UserDto.answerDto.librariesUpdated;

                //UMI3DLogger.Log($"Somthing to update {UserDto.formdto != null} {!UserDto.answerDto.librariesUpdated} ", scope | DebugScope.Connection);

                if (!UserDto.answerDto.librariesUpdated)
                {
                    LibrariesDto LibrariesDto = await HttpClient.SendGetLibraries();

                    // UMI3DLogger.Log($"Ask to download Libraries", scope | DebugScope.Connection);
                    bool b = await UMI3DCollaborationClientServer.Instance.Identifier.ShouldDownloadLibraries(
                        UMI3DResourcesManager.LibrariesToDownload(LibrariesDto)
                        );

                    if (!b)
                    {
                        Ok = false;
                        //UMI3DLogger.Log($"libraries Dowload aborted", scope | DebugScope.Connection);
                    }
                    else
                    {
                        libraryProgress.SetStatus("Downloading Libraries");
                        try
                        {
                            await UMI3DResourcesManager.DownloadLibraries(LibrariesDto, worldControllerClient.name, libraryProgress);
                            librariesUpdated = true;
                        }
                        catch (Exception e)
                        {
                            UMI3DLogger.LogException(e, scope);
                            Ok = false;
                        }
                    }
                    while (!librariesUpdated && Ok)
                        await UMI3DAsyncManager.Yield();
                    UserDto.answerDto.librariesUpdated = librariesUpdated;
                }
                else
                    libraryProgress.SetAsCompleted();

                if (Ok)
                {
                    //UMI3DLogger.Log($"Update Identity parameters {UserDto.formdto} ", scope | DebugScope.Connection);
                    if (UserDto.formdto != null)
                    {
                        FormAnswerDto param = await UMI3DCollaborationClientServer.Instance.Identifier.GetParameterDtos(UserDto.formdto);
                        UserDto.answerDto.parameters = param;
                        await HttpClient.SendPostUpdateIdentity(UserDto.answerDto);
                    }
                    else
                    {
                        await HttpClient.SendPostUpdateIdentity(UserDto.answerDto);
                    }
                }
                else
                {
                    UMI3DCollaborationClientServer.Logout();
                }
            }
            catch (UMI3DAsyncManagerException)
            {
                //This exeception is thrown only when app is stopping.
            }
            catch (Exception e)
            {
                if (e is Umi3dNetworkingException n && n.errorCode == 401)
                    UMI3DCollaborationClientServer.ReceivedLogoutMessage("You are not authorized to proceed further.");
                else
                    UMI3DCollaborationClientServer.Logout();
            }
        }

        private IEnumerator OnNewTokenNextFrame()
        {
            yield return new WaitForFixedUpdate();
            UMI3DCollaborationClientServer.Instance.OnNewToken?.Invoke();
        }

        private async void Join(MultiProgress progress)
        {
            //UMI3DLogger.Log($"Join {joinning} {connected}", scope | DebugScope.Connection);
            libraryProgress.SetAsCompleted();
            Progress PostJoinProgress = new Progress(2, "Joinning Environment");
            MultiProgress EnterProgress = new MultiProgress("Entering Environment");
            progress.Add(PostJoinProgress);
            progress.Add(EnterProgress);

            if (isJoinning || isConnected) return;
            UMI3DLogger.Log($"Join", scope | DebugScope.Connection);
            isJoinning = true;

            PoseManager.Instance.InitLocalPoses();
            var joinDto = new JoinDto()
            {
                clientLocalPoses = PoseManager.Instance.localPoses.ToList(),
                userSize = PersonalSkeletonManager.Instance.PersonalSkeleton.Transform.localScale.Dto(),
            };
            try
            {
                PostJoinProgress.AddComplete();
                EnterDto enter = await HttpClient.SendPostJoin(joinDto);
                PostJoinProgress.AddAndSetStatus("Joinned Environment");
                isConnecting = false;
                isConnected = true;
                await EnterScene(enter, EnterProgress);
            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
                UMI3DCollaborationClientServer.Logout();
            }
            finally
            {
                progress.SetAsFailed();
                isJoinning = false;
            }
        }

        private async Task EnterScene(EnterDto enter, MultiProgress progress)
        {
            Progress PostJoinProgress = new Progress(1, "Joinning Environment");
            MultiProgress LoadProgress = new MultiProgress("Loading Environment");
            Progress UpdateProgress = new Progress(2, "Instanciating Environment");
            progress.Add(PostJoinProgress);
            progress.Add(LoadProgress);
            progress.Add(UpdateProgress);

            PostJoinProgress.AddComplete();
            UMI3DLogger.Log($"Enter scene", scope | DebugScope.Connection);
            useDto = enter.usedDto;
            GlTFEnvironmentDto environement = await HttpClient.SendGetEnvironment();
            UMI3DLogger.Log($"get environment completed", scope | DebugScope.Connection);
            await (UMI3DEnvironmentLoader.Instance.Load(environement, LoadProgress));
            UpdateProgress.AddComplete();
            UMI3DLogger.Log($"Load ended, Teleport and set status to active", scope | DebugScope.Connection);
            UMI3DNavigation.Instance.currentNav.Teleport(new TeleportDto() { position = enter.userPosition, rotation = enter.userRotation });
            EnvironementLoaded.Invoke();
            UserDto.answerDto.status = statusToBeSet;
            UMI3DCollaborationClientServer.transactionPending = await HttpClient.SendPostUpdateIdentity(UserDto.answerDto, null);
            UpdateProgress.AddComplete();
        }

        /// <summary>
        /// Send a BrowserRequestDto on a RTC
        /// </summary>
        /// <param name="dto">Dto to send</param>
        /// <param name="reliable">is the data channel used reliable</param>
        public void Send(AbstractBrowserRequestDto dto, bool reliable)
        {
            ForgeClient.SendBrowserRequest(dto, reliable);
        }

        /// <summary>
        /// Send Tracking BrowserRequest
        /// </summary>
        /// <param name="dto">Dto to send</param>
        /// <param name="reliable">is the data channel used reliable</param>
        public void SendTracking(AbstractBrowserRequestDto dto)
        {
            ForgeClient.SendTrackingFrame(dto);
        }

        /// <summary>
        /// Send audio <paramref name="sample"/> using Voice over IP.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="sample"></param>
        public void SendVOIP(int length, byte[] sample)
        {
            ForgeClient?.SendVOIP(length, sample);
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetFile(string url, bool useParameterInsteadOfHeader)
        {
            //UMI3DLogger.Log($"GetFile {url}", scope);
            return await HttpClient.SendGetPrivate(url, useParameterInsteadOfHeader);
        }

        /// <inheritdoc/>
        public async Task<LoadEntityDto> GetEntity(List<ulong> ids)
        {
            //UMI3DLogger.Log($"GetEntity {ids.ToString<ulong>()}", scope);
            var dto = new EntityRequestDto() { entitiesId = ids };
            return await HttpClient.SendPostEntity(dto);
        }
    }
}