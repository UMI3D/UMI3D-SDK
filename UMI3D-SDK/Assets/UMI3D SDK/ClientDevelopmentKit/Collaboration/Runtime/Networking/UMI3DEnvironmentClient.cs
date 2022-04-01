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
    public class UMI3DEnvironmentClient
    {
        bool isJoinning, isConnecting, isConnected, needToGetFirstConnectionInfo;
        public bool IsConnected() => ForgeClient != null ? isConnected && ForgeClient.IsConnected : false;

        public readonly double maxMillisecondToWait = 10000;

        public readonly ForgeConnectionDto connectionDto;
        UMI3DWorldControllerClient worldControllerClient;

        public DateTime lastTokenUpdate { get; private set; }
        public HttpClient HttpClient { get; private set; }
        public UMI3DForgeClient ForgeClient { get; private set; }
        public ulong TimeStep => ForgeClient.GetNetWorker().Time.Timestep;
        public bool useDto { get; private set; } = false;

        public class UserInfo
        {
            public FormDto formdto;
            public UserConnectionAnswerDto answerDto;

            public UserInfo()
            {
                formdto = new FormDto();
                answerDto = new UserConnectionAnswerDto();
            }

            public void Set(UserConnectionDto dto)
            {
                FormAnswerDto param = this.answerDto.parameters;
                this.answerDto = new UserConnectionAnswerDto(dto)
                {
                    parameters = param
                };
                this.formdto = dto.parameters;
            }
        }

        public UserInfo UserDto = new UserInfo();


        public UMI3DEnvironmentClient(ForgeConnectionDto connectionDto, UMI3DWorldControllerClient worldControllerClient)
        {
            this.isJoinning = false;
            this.isConnecting = false;
            this.isConnected = false;
            this.connectionDto = connectionDto;
            this.worldControllerClient = worldControllerClient;

            lastTokenUpdate = default;
            HttpClient = new HttpClient(this);
            needToGetFirstConnectionInfo = true;
        }

        public bool Connect() 
        {
            if (IsConnected())
                return false;

            if (isConnecting)
                return false;

            isConnecting = true;

            ForgeClient = UMI3DForgeClient.Create(this);
            ForgeClient.ip = connectionDto.host;
            ForgeClient.port = connectionDto.forgeServerPort;
            ForgeClient.masterServerHost = connectionDto.forgeMasterServerHost;
            ForgeClient.masterServerPort = connectionDto.forgeMasterServerPort;
            ForgeClient.natServerHost = connectionDto.forgeNatServerHost;
            ForgeClient.natServerPort = connectionDto.forgeNatServerPort;

            var Auth = new common.collaboration.UMI3DAuthenticator(GetLocalToken);
            SetToken(worldControllerClient.Identity.localToken);

            ForgeClient.Join(Auth);

            return true;
        }

        public async void ConnectionLost()
        {
            if (IsConnected())
            {
                Debug.Log($"Connection lost for {worldControllerClient.Identity.localToken}");
                if (UMI3DCollaborationClientServer.Exists)
                    UMI3DCollaborationClientServer.Instance.ConnectionLost(this);
            }
            await Task.Yield();

            isConnecting = false;
            isConnected = false;
        }

        public async Task<bool> Logout()
        {
            bool ok = false;
            if (IsConnected())
            {
                try
                {
                    await HttpClient.SendPostLogout();
                }
                finally { };

                ForgeClient.Stop();
                ok = true;
            }
            isConnected = false;
            return ok;
        }

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

        void GetLocalToken(Action<string> callback) { 
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
                    ()=>UMI3DClientServer.StartCoroutine(OnNewTokenNextFrame())
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
                                var user = await HttpClient.SendGetIdentity();
                                UpdateIdentity(user);
                                break;
                            case StatusType.READY:
                                if (needToGetFirstConnectionInfo)
                                {
                                    needToGetFirstConnectionInfo = false;
                                    var _user = await HttpClient.SendGetIdentity();
                                    UserDto.Set(_user);
                                    Join();
                                }
                                else
                                {
                                    Join();
                                }

                                break;
                        }
                        break;
                    case StatusRequestDto statusRequestDto:
                        HttpClient.SendPostUpdateStatusAsync(UserDto.answerDto.status, null);
                        break;
                }
            }
            catch
            {
                Debug.LogWarning($"Error on OnMessage({message})");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        public async void OnStatusChanged(StatusDto statusDto)
        {
            try
            {
                switch (statusDto.status)
                {
                    case StatusType.CREATED:
                        needToGetFirstConnectionInfo = false;
                        var user = await HttpClient.SendGetIdentity();
                        UpdateIdentity(user);
                        break;
                    case StatusType.READY:
                        if (needToGetFirstConnectionInfo)
                        {
                            needToGetFirstConnectionInfo = false;
                            var _user = await HttpClient.SendGetIdentity();
                            UserDto.Set(_user);
                            Join();
                        }
                        else
                        {
                            Join();
                        }

                        break;
                }
            }
            catch
            {
                Debug.LogWarning($"Error on OnStatusChanged({statusDto})");
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
                    var b = await UMI3DCollaborationClientServer.Instance.Identifier.ShouldDownloadLibraries(
                        UMI3DResourcesManager.LibrariesToDownload(LibrariesDto)
                        );

                    if (!b)
                    {
                        Ok = false;
                        //UMI3DLogger.Log($"libraries Dowload aborted", scope | DebugScope.Connection);
                    }
                    else
                    {
                        UMI3DResourcesManager.DownloadLibraries(LibrariesDto,
                            worldControllerClient.name,
                            () =>
                            {
                                librariesUpdated = true;
                            },
                            (error) => { Ok = false;/* UMI3DLogger.Log("error on download Libraries :" + error, scope);*/ }
                            );
                    }

                    while (!librariesUpdated && Ok)
                        await UMI3DAsyncManager.Yield();
                    UserDto.answerDto.librariesUpdated = librariesUpdated;
                }

                if (Ok)
                {
                    //UMI3DLogger.Log($"Update Identity parameters {UserDto.formdto} ", scope | DebugScope.Connection);
                    if (UserDto.formdto != null)
                    {
                        var param = await UMI3DCollaborationClientServer.Instance.Identifier.GetParameterDtos(UserDto.formdto);
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
                    await Logout();
                }
            }
            catch (UMI3DAsyncManagerException e)
            {
                //This exeception is thrown only when app is stopping.
            }
            catch
            {
                await Logout();
                throw;
            }
        }

        private IEnumerator OnNewTokenNextFrame()
        {
            yield return new WaitForFixedUpdate();
            UMI3DCollaborationClientServer.Instance.OnNewToken?.Invoke();
        }

        private async void Join()
        {
            //UMI3DLogger.Log($"Join {joinning} {connected}", scope | DebugScope.Connection);
            if (isJoinning || isConnected) return;
            //UMI3DLogger.Log($"Join", scope | DebugScope.Connection);
            isJoinning = true;

            var joinDto = new JoinDto()
            {
                trackedBonetypes = UMI3DClientUserTrackingBone.instances.Values.Select(trackingBone => new KeyValuePair<uint, bool>(trackingBone.boneType, trackingBone.isTracked)).ToDictionary(x => x.Key, x => x.Value),
                userSize = UMI3DClientUserTracking.Instance.skeletonContainer.localScale,
            };
            try
            {
                var enter = await HttpClient.SendPostJoin(joinDto);
                isConnecting = false; isConnected = true; EnterScene(enter);
            }
            finally
            {
                isJoinning = false;
            }
        }

        private async void EnterScene(EnterDto enter)
        {
            Debug.Log($"Enter scene");
            try
            {
                //UMI3DLogger.Log($"Enter scene", scope | DebugScope.Connection);
                useDto = enter.usedDto;
                UMI3DEnvironmentLoader.Instance.NotifyLoad();
                var environement = await HttpClient.SendGetEnvironment();
                //UMI3DLogger.Log($"get environment completed", scope | DebugScope.Connection);
                Action setStatus = () =>
                {
                    async void set()
                    {
                    //UMI3DLogger.Log($"Load ended, Teleport and set status to active", scope | DebugScope.Connection);
                    UMI3DNavigation.Instance.currentNav.Teleport(new TeleportDto() { position = enter.userPosition, rotation = enter.userRotation });
                        UserDto.answerDto.status = StatusType.ACTIVE;
                        await HttpClient.SendPostUpdateIdentity(UserDto.answerDto, null);
                    }
                    set();
                };
                UMI3DEnvironmentLoader.StartCoroutine(UMI3DEnvironmentLoader.Instance.Load(environement, setStatus, null));
            }
            catch { }
        }

        /// <summary>
        /// Send a BrowserRequestDto on a RTC
        /// </summary>
        /// <param name="dto">Dto to send</param>
        /// <param name="reliable">is the data channel used reliable</param>
        public void Send(AbstractBrowserRequestDto dto, bool reliable)
        {
            Debug.Log(worldControllerClient.Identity.localToken);
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

        public void SendVOIP(int length, byte[] sample)
        {
            ForgeClient?.SendVOIP(length, sample);
        }

        ///<inheritdoc/>
        public async Task<byte[]> GetFile(string url)
        {
            //UMI3DLogger.Log($"GetFile {url}", scope);
            return await HttpClient.SendGetPrivate(url);
        }

        ///<inheritdoc/>
        public async Task<LoadEntityDto> GetEntity(List<ulong> ids)
        {
            //UMI3DLogger.Log($"GetEntity {ids.ToString<ulong>()}", scope);
            var dto = new EntityRequestDto() { entitiesId = ids };
            return await HttpClient.SendPostEntity(dto);
        }
    }
}