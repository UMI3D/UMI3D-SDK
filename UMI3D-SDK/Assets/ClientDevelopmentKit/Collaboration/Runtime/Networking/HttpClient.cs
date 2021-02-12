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
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;
using UnityEngine.Networking;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Class to send Http Request.
    /// </summary>
    public class HttpClient
    {
        UMI3DCollaborationClientServer client;
        internal string ComputedToken;

        private string httpUrl { get { return UMI3DCollaborationClientServer.Media.connection.httpUrl; } }

        /// <summary>
        /// Init HttpClient.
        /// </summary>
        /// <param name="client"></param>
        public HttpClient(UMI3DCollaborationClientServer client)
        {
            this.client = client;
        }

        /// <summary>
        /// Renew token.
        /// </summary>
        /// <param name="token"></param>
        public void SetToken(string token)
        {
            ComputedToken = UMI3DNetworkingKeys.bearer + token;
        }

        bool DefaultShouldTryAgain(RequestFailedArgument argument)
        {
            return argument.count < 3;
        }

        #region user
        /// <summary>
        /// Send request using GET method to get the user Identity.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendGetIdentity(Action<UserConnectionDto> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                var res = uwr.downloadHandler.data;
                UserConnectionDto user = UMI3DDto.FromBson(res) as UserConnectionDto;
                callback.Invoke(user);
            };
            client.StartCoroutine(_GetRequest(httpUrl + UMI3DNetworkingKeys.identity, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using POST method to update user Identity.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendPostUpdateIdentity(Action callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                //res ?
                callback?.Invoke();
            };
            client.StartCoroutine(_PostRequest(httpUrl + UMI3DNetworkingKeys.identity_update, UMI3DCollaborationClientServer.UserDto.ToBson(), action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using POST method to update user Identity.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendPostUpdateStatus(Action callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                //res ?
                callback?.Invoke();
            };
            client.StartCoroutine(_PostRequest(httpUrl + UMI3DNetworkingKeys.status_update, new StatusDto() { status = UMI3DCollaborationClientServer.UserDto.status }.ToBson(), action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using POST method to logout of the server.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendPostLogout(Action callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                //res ?
                callback.Invoke();
            };
            client.StartCoroutine(_PostRequest(httpUrl + UMI3DNetworkingKeys.logout, new UMI3DDto().ToBson(), action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }
        #endregion

        #region media
        /// <summary>
        /// Send request using GET method to get the server Media.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendGetMedia(Action<MediaDto> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain)
        {
            SendGetMedia(httpUrl + UMI3DNetworkingKeys.media, callback, onError, shouldTryAgain);
        }

        /// <summary>
        /// Send request using GET method to get a Media at a specified url.
        /// </summary>
        /// <param name="url">Url to send the resquest to. For a vanilla server add '/media' at the end of the server url.</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendGetMedia(string url, Action<MediaDto> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                var res = uwr.downloadHandler.data;
                MediaDto media = UMI3DDto.FromBson(res) as MediaDto;
                callback.Invoke(media);
            };
            client.StartCoroutine(_GetRequest(url, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e)));
        }
        #endregion

        #region resources

        /// <summary>
        /// Send request using GET.
        /// </summary>
        /// <param name="url">Url.</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendGetLibraries(Action<LibrariesDto> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                var res = UMI3DDto.FromBson(uwr.downloadHandler.data) as LibrariesDto;
                callback.Invoke(res);
            };
            client.StartCoroutine(_GetRequest(httpUrl + UMI3DNetworkingKeys.libraries, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using GET
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendGetPublic(string url, Action<byte[]> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                var res = uwr.downloadHandler.data;
                callback.Invoke(res);
            };
            client.StartCoroutine(_GetRequest(url, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), false));
        }

        /// <summary>
        /// Send request using GET method to get the a private file.
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendGetPrivate(string url, Action<byte[]> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                var res = uwr.downloadHandler.data;
                callback.Invoke(res);
            };
            client.StartCoroutine(_GetRequest(url, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }
        #endregion

        #region environement
        /// <summary>
        /// Send request using GET method to get the Environement.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendGetEnvironment(Action<GlTFEnvironmentDto> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                var res = uwr.downloadHandler.data;
                GlTFEnvironmentDto user = UMI3DDto.FromBson(res) as GlTFEnvironmentDto;
                callback.Invoke(user);
            };
            client.StartCoroutine(_GetRequest(httpUrl + UMI3DNetworkingKeys.environment, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using POST method to Join server.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendPostJoin(JoinDto join, Action<EnterDto> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                var res = uwr.downloadHandler.data;
                EnterDto enter = UMI3DDto.FromBson(res) as EnterDto;
                callback.Invoke(enter);
            };
            client.StartCoroutine(_PostRequest(httpUrl + UMI3DNetworkingKeys.join, join.ToBson(), action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using POST method to request the server to send a Scene.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendPostSceneRequest(Action callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                callback.Invoke();
            };
            client.StartCoroutine(_PostRequest(httpUrl + UMI3DNetworkingKeys.scene, null, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }
        #endregion

        /// <summary>
        /// Class to be send to try to send a request again.
        /// </summary>
        public class RequestFailedArgument
        {

            Action tryAgain;
            public DateTime date { get; private set; }
            public UnityWebRequest request { get; private set; }
            public Func<RequestFailedArgument, bool> ShouldTryAgain { get; private set; }
            public RequestFailedArgument(UnityWebRequest request, Action tryAgain, int count, DateTime date, Func<RequestFailedArgument, bool> ShouldTryAgain)
            {
                this.request = request;
                this.tryAgain = tryAgain;
                this.count = count;
                this.date = date;
                this.ShouldTryAgain = ShouldTryAgain;
            }

            public int count { get; private set; }

            public virtual void TryAgain()
            {
                tryAgain.Invoke();
            }

        }

        #region utils
        /// <summary>
        /// Ienumerator to send GET request.
        /// </summary>
        /// <param name="url">Url to send the request at.</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        /// <returns></returns>
        IEnumerator _GetRequest(string url, Action<UnityWebRequest> callback, Action<string> onError, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, int tryCount = 0)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            if (UseCredential) www.SetRequestHeader(UMI3DNetworkingKeys.Authorization, ComputedToken);
            DateTime date = DateTime.UtcNow;
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                if (!client.TryAgainOnHttpFail(new RequestFailedArgument(www, () => client.StartCoroutine(_GetRequest(url, callback, onError, ShouldTryAgain, UseCredential, tryCount + 1)), tryCount, date, ShouldTryAgain)))
                {
                    if (onError != null)
                    {
                        onError.Invoke(www.error + " Failed to get " + www.url);
                    }
                    else
                    {
                        Debug.LogError(www.error);
                        Debug.LogError("Failed to get " + www.url);
                    }
                }
                yield break;
            }
            callback.Invoke(www);
        }

        /// <summary>
        /// Ienumerator to send POST Request.
        /// </summary>
        /// <param name="url">Url to send the request at.</param>
        /// <param name="bytes">Data send via post method.</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        /// <returns></returns>
        IEnumerator _PostRequest(string url, byte[] bytes, Action<UnityWebRequest> callback, Action<string> onError, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, int tryCount = 0)
        {
            UnityWebRequest www = CreatePostRequest(url, bytes, true);
            if (UseCredential) www.SetRequestHeader(UMI3DNetworkingKeys.Authorization, ComputedToken);
            DateTime date = DateTime.UtcNow;
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (!client.TryAgainOnHttpFail(new RequestFailedArgument(www, () => client.StartCoroutine(_PostRequest(url, bytes, callback, onError, ShouldTryAgain, UseCredential, tryCount + 1)), tryCount, date, ShouldTryAgain)))
                {
                    if (onError != null)
                    {
                        onError.Invoke(www.error + " Failed to post " + www.url);
                    }
                    else
                    {
                        Debug.LogError(www.error);
                        Debug.LogError("Failed to post " + www.url);
                    }
                }
                yield break;
            }
            callback.Invoke(www);
        }

        /// <summary>
        /// Util function to create POST request.
        /// </summary>
        /// <param name="url">Url to send the request at.</param>
        /// <param name="bytes">Data send via post method.</param>
        /// <param name="withResult">require a result</param>
        /// <returns></returns>
        UnityWebRequest CreatePostRequest(string url, byte[] bytes, bool withResult = false)
        {
            UnityWebRequest requestU = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            UploadHandlerRaw uH = new UploadHandlerRaw(bytes);
            requestU.uploadHandler = uH;
            if (withResult)
                requestU.downloadHandler = new DownloadHandlerBuffer();
            //requestU.SetRequestHeader("access_token", client.GetToken(null));
            return requestU;
        }
        #endregion
    }
}