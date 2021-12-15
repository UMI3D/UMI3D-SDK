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
        const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Networking;

        internal string ComputedToken;

        private string httpUrl => UMI3DCollaborationClientServer.Media.connection.httpUrl;


        ThreadDeserializer deserializer;
        /// <summary>
        /// Init HttpClient.
        /// </summary>
        /// <param name="UMI3DClientServer"></param>
        public HttpClient(UMI3DCollaborationClientServer UMI3DClientServer)
        {
            UMI3DLogger.Log($"Init HttpClient", scope | DebugScope.Connection);
            deserializer = new ThreadDeserializer();
        }

        public void Stop()
        {
            deserializer?.Stop();
        }

        /// <summary>
        /// Renew token.
        /// </summary>
        /// <param name="token"></param>
        public void SetToken(string token)
        {
            UMI3DLogger.Log($"SetToken {token}", scope | DebugScope.Connection);
            ComputedToken = UMI3DNetworkingKeys.bearer + token;
        }

        private bool DefaultShouldTryAgain(RequestFailedArgument argument)
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
            UMI3DLogger.Log($"Send Get Identity", scope | DebugScope.Connection);
            Action<UMI3DDto> action2 = (dto) =>
            {
                var user = dto as UserConnectionDto;
                callback.Invoke(user);
            };
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"Received Get Identity", scope | DebugScope.Connection);
                byte[] res = uwr.downloadHandler.data;
                deserializer.FromBson(res, action2);
            }; 
            UMI3DClientServer.StartCoroutine(_GetRequest(httpUrl + UMI3DNetworkingKeys.identity, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using POST method to update user Identity.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendPostUpdateIdentity(Action callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostUpdateIdentity", scope | DebugScope.Connection);
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"Received PostUpdateIdentity", scope | DebugScope.Connection);
                //res ?
                callback?.Invoke();
            };
            UMI3DClientServer.StartCoroutine(_PostRequest(httpUrl + UMI3DNetworkingKeys.identity_update, UMI3DCollaborationClientServer.UserDto.dto.ToBson(), action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using POST method to update user Identity.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendPostUpdateStatus(Action callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostUpdateStatus", scope | DebugScope.Connection);
            Action<UnityWebRequest> action = (uwr) =>
            {
                //res ?
                UMI3DLogger.Log($"Received PostUpdateStatus", scope | DebugScope.Connection);
                callback?.Invoke();
            };
            UMI3DClientServer.StartCoroutine(_PostRequest(httpUrl + UMI3DNetworkingKeys.status_update, new StatusDto() { status = UMI3DCollaborationClientServer.UserDto.dto.status }.ToBson(), action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using POST method to logout of the server.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendPostLogout(Action callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostLogout", scope | DebugScope.Connection);
            Action<UnityWebRequest> action = (uwr) =>
            {
                //res ?
                UMI3DLogger.Log($"Received PostLogout", scope | DebugScope.Connection);
                callback.Invoke();
            };
            UMI3DClientServer.StartCoroutine(_PostRequest(httpUrl + UMI3DNetworkingKeys.logout, new UMI3DDto().ToBson(), action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
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
            UMI3DLogger.Log($"Send GetMedia", scope | DebugScope.Connection);
            Action<UMI3DDto> action2 = (dto) =>
            {
                var media = dto as MediaDto;
                callback.Invoke(media);
            };
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"Received GetMedia", scope | DebugScope.Connection);
                byte[] res = uwr.downloadHandler.data;
                deserializer.FromBson(res, action2); 
            };
            UMI3DClientServer.StartCoroutine(_GetRequest(url, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e)));
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
            UMI3DLogger.Log($"Send GetLibraries", scope | DebugScope.Connection);
            Action<UMI3DDto> action2 = (dto) =>
            {
                var res = dto as LibrariesDto;
                callback.Invoke(res);
            };
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"Received GetLibraries", scope | DebugScope.Connection);
                deserializer.FromBson(uwr.downloadHandler.data, action2);
            };
            UMI3DClientServer.StartCoroutine(_GetRequest(httpUrl + UMI3DNetworkingKeys.libraries, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Get a LoadEntityDto
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="onError"></param>
        /// <param name="shouldTryAgain"></param>
        public void SendPostEntity(EntityRequestDto id, Action<LoadEntityDto> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostEntity", scope | DebugScope.Connection);
            Action<UMI3DDto> action2 = (dto) =>
            {
                var res = dto as LoadEntityDto;
                callback.Invoke(res);
            };
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"Received PostEntity", scope | DebugScope.Connection);
                deserializer.FromBson(uwr.downloadHandler.data, action2);
            };

            UMI3DClientServer.StartCoroutine(_PostRequest(httpUrl + UMI3DNetworkingKeys.entity, id.ToBson(), action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using GET
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendGetPublic(string url, Action<byte[]> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send GetPublic {url}", scope | DebugScope.Connection);
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"received getPublic {url}", scope | DebugScope.Connection);
                byte[] res = uwr.downloadHandler.data;
                callback.Invoke(res);
            };
            UMI3DClientServer.StartCoroutine(_GetRequest(url, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), false));
        }

        /// <summary>
        /// Send request using GET method to get the a private file.
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendGetPrivate(string url, Action<byte[]> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send GetPrivate {url}", scope | DebugScope.Connection);
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"Received GetPrivate {url}", scope | DebugScope.Connection);
                byte[] res = uwr.downloadHandler.data;
                callback.Invoke(res);
            };
            UMI3DClientServer.StartCoroutine(_GetRequest(url, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
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
            UMI3DLogger.Log($"Send GetEnvironment", scope | DebugScope.Connection);
            Action<UMI3DDto> action2 = (dto) =>
            {
                var env = dto as GlTFEnvironmentDto;
                callback.Invoke(env);
            };
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"Received GetEnvironment", scope | DebugScope.Connection);
                byte[] res = uwr.downloadHandler.data;
                deserializer.FromBson(res, action2);
            };
            UMI3DClientServer.StartCoroutine(_GetRequest(httpUrl + UMI3DNetworkingKeys.environment, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using POST method to Join server.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendPostJoin(JoinDto join, Action<EnterDto> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostJoin", scope | DebugScope.Connection);
            Action<UMI3DDto> action2 = (dto) =>
            {
                var enter = dto as EnterDto;
                callback.Invoke(enter);
            };
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"Received PostJoin", scope | DebugScope.Connection);
                byte[] res = uwr.downloadHandler.data;
                deserializer.FromBson(res,action2);
            };
            UMI3DClientServer.StartCoroutine(_PostRequest(httpUrl + UMI3DNetworkingKeys.join, join.ToBson(), action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using POST method to request the server to send a Scene.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendPostSceneRequest(Action callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostSceneRequest", scope | DebugScope.Connection);
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"Received PostSceneRequest", scope | DebugScope.Connection);
                callback.Invoke();
            };
            UMI3DClientServer.StartCoroutine(_PostRequest(httpUrl + UMI3DNetworkingKeys.scene, null, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        #endregion

        #region Local Info
        /// <summary>
        /// Send request using POST method to send to the server Local Info.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        /// <param name="key">Local data file key.</param>
        public void SendPostLocalInfo(Action callback, Action<string> onError, string key, byte[] bytes, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostLocalInfo {key}", scope | DebugScope.Connection);
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"Received PostLocalInfo {key}", scope | DebugScope.Connection);
                callback.Invoke();
            };
            string url = System.Text.RegularExpressions.Regex.Replace(httpUrl + UMI3DNetworkingKeys.localData, ":param", key);
            UMI3DClientServer.StartCoroutine(_PostRequest(url, bytes, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        /// <summary>
        /// Send request using GET method to get datas from server then save its in local file.
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public void SendGetLocalInfo(string key, Action<byte[]> callback, Action<string> onError, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send GetLocalInfo {key}", scope | DebugScope.Connection);
            Action<UnityWebRequest> action = (uwr) =>
            {
                UMI3DLogger.Log($"Received GetLocalInfo {key}", scope | DebugScope.Connection);
                byte[] bytes = uwr.downloadHandler.data;
                callback.Invoke(bytes);
            };
            string url = System.Text.RegularExpressions.Regex.Replace(httpUrl + UMI3DNetworkingKeys.localData, ":param", key);
            UMI3DClientServer.StartCoroutine(_GetRequest(url, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true));
        }

        #endregion

        #region upload
        /// <summary>
        /// Send request using POST method to send file to the server.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        /// <param name="token">Authorization token, given by the server.</param>
        /// <param name="fileName">Name of the uploaded file.</param>
        /// <param name="bytes">the file in bytes.</param>
        /// <param name="shouldTryAgain"></param>
        public void SendPostFile(Action callback, Action<string> onError, string token, string fileName, byte[] bytes, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            Action<UnityWebRequest> action = (uwr) =>
            {
                if (callback != null)
                    callback.Invoke();
            };
            string url = System.Text.RegularExpressions.Regex.Replace(httpUrl + UMI3DNetworkingKeys.uploadFile, ":param", token);
            //Header
            var headers = new List<(string, string)>
            {
                (UMI3DNetworkingKeys.contentHeader, fileName)
            };
            UMI3DClientServer.StartCoroutine(_PostRequest(url, bytes, action, onError, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true, headers));
        }
        #endregion

        #region utils
        /// <summary>
        /// Ienumerator to send GET request.
        /// </summary>
        /// <param name="url">Url to send the request at.</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        /// <returns></returns>
        private IEnumerator _GetRequest(string url, Action<UnityWebRequest> callback, Action<string> onError, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, List<(string, string)> headers = null, int tryCount = 0)
        {
            var www = UnityWebRequest.Get(url);
            if (UseCredential) www.SetRequestHeader(UMI3DNetworkingKeys.Authorization, ComputedToken);
            if (headers != null)
            {
                foreach ((string, string) item in headers)
                {
                    www.SetRequestHeader(item.Item1, item.Item2);
                }
            }
            DateTime date = DateTime.UtcNow;
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                if (!UMI3DClientServer.Exists || !UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(www, () => UMI3DClientServer.StartCoroutine(_GetRequest(url, callback, onError, ShouldTryAgain, UseCredential, headers, tryCount + 1)), tryCount, date, ShouldTryAgain)))
                {
                    if (onError != null)
                    {
                        onError.Invoke(www.error + " Failed to get " + www.url);
                    }
                    else
                    {
                        UMI3DLogger.LogError(www.error,scope);
                        UMI3DLogger.LogError("Failed to get " + www.url,scope);
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
        private IEnumerator _PostRequest(string url, byte[] bytes, Action<UnityWebRequest> callback, Action<string> onError, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, List<(string, string)> headers = null, int tryCount = 0)
        {
            UnityWebRequest www = CreatePostRequest(url, bytes, true);
            if (UseCredential) www.SetRequestHeader(UMI3DNetworkingKeys.Authorization, ComputedToken);
            if (headers != null)
            {
                foreach ((string, string) item in headers)
                {
                    www.SetRequestHeader(item.Item1, item.Item2);
                }
            }
            DateTime date = DateTime.UtcNow;
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                if (!UMI3DClientServer.Exists || !UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(www, () => UMI3DClientServer.StartCoroutine(_PostRequest(url, bytes, callback, onError, ShouldTryAgain, UseCredential, headers, tryCount + 1)), tryCount, date, ShouldTryAgain)))
                {
                    if (onError != null)
                    {
                        onError.Invoke(www.error + " Failed to post " + www.url);
                    }
                    else
                    {
                        UMI3DLogger.LogError(www.error,scope);
                        UMI3DLogger.LogError("Failed to post " + www.url,scope);
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
        private UnityWebRequest CreatePostRequest(string url, byte[] bytes, bool withResult = false)
        {
            var requestU = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            var uH = new UploadHandlerRaw(bytes);
            requestU.uploadHandler = uH;
            if (withResult)
                requestU.downloadHandler = new DownloadHandlerBuffer();
            //requestU.SetRequestHeader("access_token", UMI3DClientServer.GetToken(null));
            return requestU;
        }
        #endregion
    }
}