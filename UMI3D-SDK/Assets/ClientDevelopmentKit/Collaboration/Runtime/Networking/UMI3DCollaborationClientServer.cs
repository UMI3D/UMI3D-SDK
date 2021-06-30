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
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Collaboration Extension of the UMI3DClientServer
    /// </summary>
    public class UMI3DCollaborationClientServer : UMI3DClientServer
    {
        public static new UMI3DCollaborationClientServer Instance { get { return UMI3DClientServer.Instance as UMI3DCollaborationClientServer; } set { UMI3DClientServer.Instance = value; } }

        public static bool useDto { protected set; get; } = false;

        static public DateTime lastTokenUpdate { get; private set; }
        public HttpClient HttpClient { get; private set; }
        public UMI3DForgeClient ForgeClient { get; private set; }

        static public IdentityDto Identity = new IdentityDto();
        static public UserConnectionDto UserDto = new UserConnectionDto();

        public UnityEvent OnNewToken = new UnityEvent();
        public UnityEvent OnConnectionLost = new UnityEvent();

        public ClientIdentifierApi Identifier;


        static bool connected = false;


        private void Start()
        {
            lastTokenUpdate = default;
            HttpClient = new HttpClient(this);
            connected = false;
            joinning = false;
        }

        public void Init()
        {
            ForgeClient = UMI3DForgeClient.Create();
        }

        /// <summary>
        /// State if the Client is connected to a Server.
        /// </summary>
        /// <returns>True if the client is connected.</returns>
        public static bool Connected()
        {
            return Exists && Instance?.ForgeClient != null ? Instance.ForgeClient.IsConnected && connected : false;
        }

        /// <summary>
        /// Start the connection workflow to the Environement defined by the Media variable in UMI3DBrowser.
        /// </summary>
        /// <seealso cref="UMI3DCollaborationClientServer.Media"/>
        static public void Connect()
        {
            Instance.Init();
            if (UMI3DCollaborationClientServer.Media.connection is ForgeConnectionDto connection)
            {
                Instance.ForgeClient.ip = connection.host;
                Instance.ForgeClient.port = connection.forgeServerPort;
                Instance.ForgeClient.masterServerHost = connection.forgeMasterServerHost;
                Instance.ForgeClient.masterServerPort = connection.forgeMasterServerPort;
                Instance.ForgeClient.natServerHost = connection.forgeNatServerHost;
                Instance.ForgeClient.natServerPort = connection.forgeNatServerPort;

                UMI3DCollaborationClientServer.Instance.Identifier.GetIdentity((Auth) =>
                {
                    UMI3DCollaborationClientServer.Identity.login = "";
                    Auth.LoginSet = (s) =>
                    {
                        UMI3DCollaborationClientServer.Identity.login = s;
                        Auth.LoginSet = null;
                    };
                    Instance.ForgeClient.Join(Auth);
                });
            }
        }

        /// <summary>
        /// Logout of the current server
        /// </summary>
        static public void Logout(Action success, Action<string> failled)
        {
            if (Exists)
                Instance._Logout(success, failled);
        }
        void _Logout(Action success, Action<string> failled)
        {
            if (Connected())
                HttpClient.SendPostLogout(() =>
                {
                    ForgeClient.Stop();
                    Start();
                    success?.Invoke();
                    Identity = new IdentityDto();
                },
                (error) => { failled.Invoke(error); Identity = new IdentityDto(); });
            else
                Identity = new IdentityDto();
        }


        /// <summary>
        /// Notify that the connection with the server was lost.
        /// </summary>
        public void ConnectionLost()
        {
            UMI3DCollaborationClientServer.Logout(null, null);

            OnConnectionLost.Invoke();
        }


        /// <summary>
        /// Retry a failed http request
        /// </summary>
        /// <param name="argument">failed request argument</param>
        /// <returns></returns>
        public bool TryAgainOnHttpFail(HttpClient.RequestFailedArgument argument)
        {
            if (argument.ShouldTryAgain(argument))
            {
                StartCoroutine(TryAgain(argument));
                return true;
            }
            return false;
        }

        /// <summary>
        /// launch a new request
        /// </summary>
        /// <param name="argument">argument used in the request</param>
        /// <returns></returns>
        IEnumerator TryAgain(HttpClient.RequestFailedArgument argument)
        {
            bool newToken = argument.request.responseCode != 401 || (lastTokenUpdate - argument.date).TotalMilliseconds > 0;
            if (!newToken)
            {
                UnityAction a = () => newToken = true;
                OnNewToken.AddListener(a);
                yield return new WaitUntil(() => newToken);
                OnNewToken.RemoveListener(a);
            }
            argument.TryAgain();
        }


        /// <summary>
        /// Get a media dto at a raw url using a get http request.
        /// The result is store in UMI3DClientServer.Media.
        /// </summary>
        /// <param name="url">Url used for the get request.</param>
        /// <seealso cref="UMI3DCollaborationClientServer.Media"/>
        static public void GetMedia(string url, Action<MediaDto> callback = null, Action<string> failback = null, Func<HttpClient.RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DCollaborationClientServer.Instance.HttpClient.SendGetMedia(url, (media) =>
            {
                Media = media; Instance._setMedia(); callback?.Invoke(media);
            }, failback, shouldTryAgain);
        }

        void _setMedia()
        {

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        static public void OnStatusChanged(StatusDto statusDto)
        {
            switch (statusDto.status)
            {
                case StatusType.CREATED:
                    UMI3DCollaborationClientServer.Instance.HttpClient.SendGetIdentity((user) =>
                    {
                        Instance.StartCoroutine(Instance.UpdateIdentity(user));
                    }, (error) => { Debug.Log("error on get id :" + error); });
                    break;
                case StatusType.READY:
                    if (Identity.userId == null)
                        Instance.HttpClient.SendGetIdentity((user) =>
                        {
                            UserDto = user;
                            Identity.userId = user.id;
                            Instance.Join();

                        }, (error) => { Debug.Log("error on get id :" + error); });
                    else
                        Instance.Join();
                    break;
            }
        }


        /// <summary>
        /// Set the token used to communicate to the server.
        /// </summary>
        /// <param name="token"></param>
        static public void SetToken(string token)
        {
            if (Exists)
            {
                //Debug.Log($"<color=magenta> new token { token}</color>");
                lastTokenUpdate = DateTime.UtcNow;
                Instance?.HttpClient?.SetToken(token);
                BeardedManStudios.Forge.Networking.Unity.MainThreadManager.Run(() =>
                {
                    Instance?.OnNewToken?.Invoke();
                });
            }
        }

        /// <summary>
        /// Send a BrowserRequestDto on a RTC
        /// </summary>
        /// <param name="dto">Dto to send</param>
        /// <param name="reliable">is the data channel used reliable</param>
        protected override void _Send(AbstractBrowserRequestDto dto, bool reliable)
        {
            ForgeClient.SendBrowserRequest(dto, reliable);
        }

        /// <summary>
        /// Send Tracking BrowserRequest
        /// </summary>
        /// <param name="dto">Dto to send</param>
        /// <param name="reliable">is the data channel used reliable</param>
        protected override void _SendTracking(AbstractBrowserRequestDto dto)
        {
            ForgeClient.SendTrackingFrame(dto);
        }

        /// <summary>
        /// Handles the message comming from the websockekt server.
        /// </summary>
        /// <param name="message"></param>
        static public void OnMessage(object message)
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
                            Instance.HttpClient.SendGetIdentity((user) =>
                            {
                                Instance.StartCoroutine(Instance.UpdateIdentity(user));
                            }, (error) => { Debug.Log("error on get id :" + error); });
                            break;
                        case StatusType.READY:
                            if (Identity.userId == null)
                                Instance.HttpClient.SendGetIdentity((user) =>
                                {
                                    UserDto = user;
                                    Identity.userId = user.id;
                                    Instance.Join();

                                }, (error) => { Debug.Log("error on get id :" + error); });
                            else
                                Instance.Join();
                            break;
                    }
                    break;
                case StatusRequestDto statusRequestDto:
                    Instance.HttpClient.SendPostUpdateStatus(null, null);
                    break;
            }
        }

        bool joinning;
        void Join()
        {
            if (joinning || connected) return;
            joinning = true;

            JoinDto joinDto = new JoinDto()
            {
                trackedBonetypes = UMI3DClientUserTrackingBone.instances.Values.Select(trackingBone => new KeyValuePair<string, bool>(trackingBone.boneType, trackingBone.isTracked)).ToDictionary(x => x.Key, x => x.Value),
                userSize = UMI3DClientUserTracking.Instance.skeletonContainer.localScale,
            };

            Instance.HttpClient.SendPostJoin(
                joinDto,
                (enter) => { joinning = false; connected = true; Instance.EnterScene(enter); },
                (error) => { joinning = false; Debug.Log("error on get id :" + error); });
        }

        /// <summary>
        /// Coroutine to handle identity.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        IEnumerator UpdateIdentity(UserConnectionDto user)
        {
            UserDto = user;
            Identity.userId = user.id;
            bool Ok = true;
            bool librariesUpdated = UserDto.librariesUpdated;

            if (!UserDto.librariesUpdated)
            {
                HttpClient.SendGetLibraries(
                    (LibrariesDto) =>
                    {
                        Instance.Identifier.ShouldDownloadLibraries(
                            UMI3DResourcesManager.LibrariesToDownload(LibrariesDto),
                            b =>
                            {
                                if (!b)
                                {
                                    Ok = false;
                                }
                                else
                                    UMI3DResourcesManager.DownloadLibraries(LibrariesDto,
                                        Media.name,
                                        () =>
                                        {
                                            librariesUpdated = true;
                                        },
                                        (error) => { Ok = false; Debug.Log("error on download Libraries :" + error); }
                                        );
                            });
                    },
                    (error) => { Ok = false; Debug.Log("error on get Libraries: " + error); }
                    );

                yield return new WaitUntil(() => { return librariesUpdated || !Ok; });
                UserDto.librariesUpdated = librariesUpdated;
            }
            if (Ok)
                Instance.Identifier.GetParameterDtos(user.parameters, (param) =>
                {
                    user.parameters = param;
                    Instance.HttpClient.SendPostUpdateIdentity(() => { }, (error) => { Debug.Log("error on post id :" + error); });
                });
            else
                Logout(null, null);
        }

        void EnterScene(EnterDto enter)
        {
            useDto = enter.usedDto;
            HttpClient.SendGetEnvironment(
                (environement) =>
                {
                    Action setStatus = () =>
                    {
                        UMI3DNavigation.Instance.currentNav.Teleport(new TeleportDto() { position = enter.userPosition, rotation = enter.userRotation });
                        UserDto.status = StatusType.ACTIVE;
                        HttpClient.SendPostUpdateIdentity(null, null);
                    };
                    StartCoroutine(UMI3DEnvironmentLoader.Instance.Load(environement, setStatus, null));
                },
                (error) => { Debug.Log("error on get Environement :" + error); });
        }

        ///<inheritdoc/>
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        ///<inheritdoc/>
        protected override void _GetFile(string url, Action<byte[]> callback, Action<string> onError)
        {
            HttpClient.SendGetPrivate(url, callback, onError);
        }

        ///<inheritdoc/>
        public override string GetId() { return Identity.userId; }

        ///<inheritdoc/>
        public override ulong GetTime()
        {
            return ForgeClient.GetNetWorker().Time.Timestep;
        }

        protected override string _getAuthorization() { return HttpClient.ComputedToken; }

        /// <summary>
        /// return HTTPClient if the server is a collaboration server.
        /// </summary>
        public override object GetHttpClient() { return HttpClient; }

    }
}