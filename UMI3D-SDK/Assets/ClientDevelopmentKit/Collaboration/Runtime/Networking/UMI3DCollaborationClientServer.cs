/*
Copyright 2019 Gfi Informatique

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

using MainThreadDispatcher;
using System;
using System.Collections;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.userCapture;
using Unity.WebRTC;
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

        static public DateTime lastTokenUpdate { get; private set; }
        public HttpClient HttpClient { get; private set; }
        public WebSocketClient WebSocketClient { get; private set; }
        public WebRTCClient WebRTCClient { get; private set; }
        static public IdentityDto Identity = new IdentityDto();
        static public UserConnectionDto UserDto = new UserConnectionDto();

        public UnityEvent OnNewToken = new UnityEvent();
        public UnityEvent OnConnectionLost = new UnityEvent();

        int tryCount = 0;
        int maxTryCount = 10;

        //public CameraDisplayer cameraDisplayer;
        //public RectTransform TextureContainer;
        //List<RawImage> images = new List<RawImage>();
        //List<Texture2D> textures = new List<Texture2D>();


        //public Camera cam;
        //public RawImage image;

        public ClientIdentifierApi Identifier;


        static bool connected = false;





        private void Start()
        {
            lastTokenUpdate = default;
            HttpClient = new HttpClient(this);
            WebSocketClient = new WebSocketClient(this);
            WebRTCClient = new WebRTCClient(this);
            //WebRTCClient.audio = Audio.CaptureStream();
            //WebRTCClient.video = cam.CaptureStream(1280, 720, 1000000);
            //image.texture = cam.targetTexture;
            if (Identity.login == default)
            {
                Identity.login = "Default";
            }
            connected = false;
            joinning = false;
            //cameraDisplayer.Play();
        }

        //public Texture2D GetStreamTexture2D()
        //{
        //    return cameraDisplayer.texture;
        //}
        //public Texture2D GetReceivedTexture2D()
        //{
        //    GameObject g = new GameObject();
        //    g.transform.SetParent(TextureContainer);

        //    RawImage image = g.AddComponent<RawImage>();
        //    images.Add(image);
        //    var texture = new Texture2D(0, 0);
        //    textures.Add(texture);
        //    return texture;
        //}

        //public void Update()
        //{
        //    for (int i = 0; i < images.Count; i++)
        //        images[i].texture = textures[i];
        //}


        private void OnAudioFilterRead(float[] data, int channels)
        {
            Audio.Update(data, data.Length);
        }

        /// <summary>
        /// State if the Client is connected to a Server.
        /// </summary>
        /// <returns>True if the client is connected.</returns>
        public static bool Connected()
        {
            return Exists && Instance?.WebSocketClient != null ? Instance.WebSocketClient.Connected() && connected : false;
        }

        /// <summary>
        /// Start the connection workflow to the Environement defined by the Media variable in UMI3DBrowser.
        /// </summary>
        /// <seealso cref="UMI3DCollaborationClientServer.Media"/>
        static public void Connect()
        {
            Instance.WebSocketClient.Init();
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
                    WebRTCClient.Stop();
                    WebSocketClient.Close();
                    Start();
                    success?.Invoke();
                },
                (error) => { failled.Invoke(error); });
        }

        public void onOpen()
        {
            tryCount = 0;
        }

        /// <summary>
        /// Should The websocket connection try to reconnect
        /// </summary>
        /// <param name="code">error code</param>
        /// <returns></returns>
        public bool shouldReconnectWebsocket(ushort code)
        {
            tryCount++;
            if (code.Equals(1006) || tryCount >= maxTryCount)
            {
                OnConnectionLost.Invoke();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retry a failed http request
        /// </summary>
        /// <param name="argument">failed request argument</param>
        /// <returns></returns>
        public bool TryAgainOnHttpFail(HttpClient.RequestFailedArgument argument)
        {
            if (argument.count < 3)
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
        static public void GetMedia(string url, Action<MediaDto> callback = null, Action<string> failback = null)
        {
            UMI3DCollaborationClientServer.Instance.HttpClient.SendGetMedia(url, (media) => { Media = media; callback?.Invoke(media); }, failback);
        }

        /// <summary>
        /// Set the token used to communicate to the server.
        /// </summary>
        /// <param name="token"></param>
        static public void SetToken(string token)
        {
            if (Exists)
            {
                //Debug.Log("new token " + token);
                lastTokenUpdate = DateTime.UtcNow;
                Instance?.HttpClient?.SetToken(token);
                Instance?.OnNewToken?.Invoke();
            }
        }

        /// <summary>
        /// Send a RTCDto message via the websocket connection
        /// </summary>
        /// <param name="dto"></param>
        public void Send(RTCDto dto)
        {
            WebSocketClient.Send(dto);
        }

        /// <summary>
        /// Send a BrowserRequestDto on a RTC
        /// </summary>
        /// <param name="dto">Dto to send</param>
        /// <param name="reliable">is the data channel used reliable</param>
        protected override void _Send(AbstractBrowserRequestDto dto, bool reliable)
        {
            if (Exists)
                Instance.WebRTCClient?.SendServer(dto, reliable);
        }

        /// <summary>
        /// Send Tracking BrowserRequest
        /// </summary>
        /// <param name="dto">Dto to send</param>
        /// <param name="reliable">is the data channel used reliable</param>
        protected override void _SendTracking(AbstractBrowserRequestDto dto, bool reliable)
        {
            if (Exists)
                Instance.WebRTCClient.Send(dto, reliable, DataType.Tracking);
        }

        /// <summary>
        /// Handles the message comming from the websockekt server.
        /// </summary>
        /// <param name="message"></param>
        static public void OnMessage(object message)
        {
            if (message is TokenDto)
            {
                var req = message as TokenDto;
                SetToken(req.token);
            }
            if (message is StatusDto)
            {
                var req = message as StatusDto;
                if (req.status == StatusType.CREATED)
                {
                    Instance.HttpClient.SendGetIdentity((user) =>
                    {
                        Instance.StartCoroutine(Instance.UpdateIdentity(user));
                    }, (error) => { Debug.Log("error on get id :" + error); });
                }
                else if (req.status == StatusType.READY)
                {
                    if (Identity.userId == null)
                        Instance.HttpClient.SendGetIdentity((user) =>
                        {
                            UserDto = user;
                            Identity.userId = user.id;
                            Instance.Join();

                        }, (error) => { Debug.Log("error on get id :" + error); });
                    else
                        Instance.Join();
                }
            }
            if (message is RTCDto)
            {
                Instance.WebRTCClient.HandleMessage(message as RTCDto);
            }
        }

        bool joinning;
        void Join()
        {
            if (joinning || connected) return;
            joinning = true;
            Instance.HttpClient.SendPostJoin(
                new JoinDto(),
                (enter) => { joinning = false; connected = true; Instance.EnterScene(); },
                (error) => { joinning = false; Debug.Log("error on get id :" + error); });
        }

        /// <summary>
        /// Handle Rtc Message
        /// </summary>
        /// <param name="bytes">Message to handle</param>
        /// <param name="channel">Channel from which the message was received</param>
        static public void OnRtcMessage(byte[] bytes, DataChannel channel)
        {
            var dto = UMI3DDto.FromBson(bytes);
            switch (dto)
            {
                case TransactionDto transaction:
                    UnityMainThreadDispatcher.Instance().Enqueue(UMI3DTransactionDispatcher.PerformTransaction(transaction));
                    break;
                case NavigateDto navigate:
                    UnityMainThreadDispatcher.Instance().Enqueue(UMI3DNavigation.Navigate(navigate));
                    break;
                case UserTrackingFrameDto trackingFrame:
                    if (UMI3DClientUserTracking.Instance.embodimentDict.TryGetValue(trackingFrame.userId, out UserAvatar userAvatar))
                        userAvatar.UpdateBonePosition(trackingFrame);
                    else
                        throw new Exception("User Avatar not found.");
                    break;
                default:
                    Debug.Log($"Type not catch {dto.GetType()}");
                    break;
            }
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

        void EnterScene()
        {
            HttpClient.SendGetEnvironment(
                (environement) =>
                {
                    Action setStatus = () =>
                    {
                        UserDto.status = StatusType.ACTIVE;
                        HttpClient.SendPostUpdateIdentity(null, null);
                    };
                    StartCoroutine(UMI3DEnvironmentLoader.Instance.Load(environement, setStatus, null));
                },
                (error) => { Debug.Log("error on get Environement :" + error); });
        }

        protected override void OnDestroy()
        {
            WebRTCClient?.Clear();
            base.OnDestroy();
        }

        protected override void _GetFile(string url, Action<byte[]> callback, Action<string> onError) {
            HttpClient.SendGetPrivate(url, callback, onError);
        }

        public override string GetId() { return Identity.userId; }
    }
}