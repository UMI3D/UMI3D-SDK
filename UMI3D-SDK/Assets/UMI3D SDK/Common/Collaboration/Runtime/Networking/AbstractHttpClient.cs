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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;
using UnityEngine.Networking;

namespace umi3d.common.collaboration
{




    /// <summary>
    /// Send HTTP requests to the environment server.
    /// </summary>
    /// Usually used before connection or to retrieve DTOs.
    public abstract partial class AbstractHttpClient<T>
    {
        protected const DebugScope scope = DebugScope.Common | DebugScope.Collaboration | DebugScope.Networking;

        protected string _HeaderToken;

        abstract protected string httpUrl { get; }

        private readonly ThreadDeserializer deserializer;
        protected readonly T environmentClient;

        /// <summary>
        /// Init HttpClient.
        /// </summary>
        /// <param name="UMI3DClientServer"></param>
        public AbstractHttpClient(T environmentClient)
        {
            this.environmentClient = environmentClient;
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
            _HeaderToken = UMI3DNetworkingKeys.bearer + token;
        }

        protected static bool DefaultShouldTryAgain(RequestFailedArgument argument)
        {
            return argument.count < 3;
        }

        #region user

        /// <summary>
        /// Connect to a media
        /// </summary>
        /// <param name="connectionDto"></param>
        public static async Task<UMI3DDto> Connect(ConnectionDto connectionDto, string MasterUrl, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(connectionDto.ToJson(Newtonsoft.Json.TypeNameHandling.None));

            using (UnityWebRequest uwr = await _PostRequest(null, null, MasterUrl + UMI3DNetworkingKeys.connect, "application/json", bytes, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), false))
            {
                UMI3DLogger.Log($"Received answer to Connect : \n " + uwr?.downloadHandler?.text, scope | DebugScope.Connection);

                UMI3DDto dto = uwr?.downloadHandler.data != null ? ReadConnectAnswer(System.Text.Encoding.UTF8.GetString(uwr?.downloadHandler.data)) : null;
                return dto;
            }
        }

        private static UMI3DDto ReadConnectAnswer(string text)
        {
            PrivateIdentityDto dto1 = null;
            FakePrivateIdentityDto dto2 = null;

            try
            {
                dto1 = UMI3DDtoSerializer.FromJson<PrivateIdentityDto>(text, Newtonsoft.Json.TypeNameHandling.None);
            }
            catch (Exception)
            {
                dto2 = UMI3DDtoSerializer.FromJson<FakePrivateIdentityDto>(text, Newtonsoft.Json.TypeNameHandling.None);
            }

            ConnectionFormDto dto3 = UMI3DDtoSerializer.FromJson<ConnectionFormDto>(text, Newtonsoft.Json.TypeNameHandling.None, new List<JsonConverter>() { new Old_ParameterConverter() });
            interaction.form.ConnectionFormDto dto4 = UMI3DDtoSerializer.FromJson<interaction.form.ConnectionFormDto>(text, Newtonsoft.Json.TypeNameHandling.None, new List<JsonConverter>() { new ParameterConverter() });

            if (dto1 != null && dto1?.globalToken != null && dto1?.connectionDto != null)
                return dto1;
            else if (dto2 != null && dto2?.GlobalToken != null && dto2?.connectionDto != null)
                return dto2.ToPrivateIdentity();
            else
                return (UMI3DDto)dto3 ?? dto4;

        }

        private class FakePrivateIdentityDto : IdentityDto
        {
            public string GlobalToken;
            public string connectionDto;
            public List<AssetLibraryDto> libraries;

            public PrivateIdentityDto ToPrivateIdentity()
            {
                return new PrivateIdentityDto()
                {
                    globalToken = GlobalToken,
                    connectionDto = UMI3DDtoSerializer.FromJson<EnvironmentConnectionDto>(connectionDto, Newtonsoft.Json.TypeNameHandling.None),
                    libraries = libraries,
                    localToken = localToken,
                    headerToken = headerToken,
                    guid = guid,
                    displayName = displayName,
                    key = key,
                    login = login,
                    userId = userId
                };
            }
        }

        /// <summary>
        /// Send request using GET method to get the user Identity.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task<UserConnectionDto> SendGetIdentity(Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send Get Identity", scope | DebugScope.Connection);

            using (UnityWebRequest uwr = await _GetRequest(this, _HeaderToken, httpUrl + UMI3DNetworkingKeys.connectionInfo, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
            {
                UMI3DLogger.Log($"Received Get Identity", scope | DebugScope.Connection);
                UMI3DDto dto = await deserializer.FromBson(uwr?.downloadHandler.data);
                return dto as UserConnectionDto;
            }
        }

        /// <summary>
        /// Send request using POST method to update user Identity.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task<PendingTransactionDto> SendPostUpdateIdentity(UserConnectionAnswerDto answer, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            PendingTransactionDto result = null;
            UMI3DLogger.Log($"Send PostUpdateIdentity", scope | DebugScope.Connection);
            using (UnityWebRequest uwr = await _PostRequest(this, _HeaderToken, httpUrl + UMI3DNetworkingKeys.connection_information_update, null, answer.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
            {
                try
                {
                    var b = uwr?.downloadHandler.data;
                    if (b != null)
                    {
                        result = UMI3DDtoSerializer.FromBson<PendingTransactionDto>(b);
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
            UMI3DLogger.Log($"Received PostUpdateIdentity", scope | DebugScope.Connection);
            return result;
        }

        /// <summary>
        /// Send request using POST method to update user Identity.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async void SendPostUpdateStatusAsync(StatusType status, bool throwError = false, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            try
            {
                await SendPostUpdateStatus(status, shouldTryAgain);
            }
            catch (UMI3DAsyncManagerException)
            {

            }
            catch
            {
                if (throwError)
                    throw;
            }
        }

        /// <summary>
        /// Send request using POST method to update user Identity.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task SendPostUpdateStatus(StatusType status, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostUpdateStatus", scope | DebugScope.Connection);
            UnityWebRequest uwr = await _PostRequest(this, _HeaderToken, httpUrl + UMI3DNetworkingKeys.status_update, null, new StatusDto() { status = status }.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            uwr.Dispose();
            UMI3DLogger.Log($"Received PostUpdateStatus", scope | DebugScope.Connection);
        }

        /// <summary>
        /// Send request using POST method to logout of the server.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task SendPostLogout(Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostLogout", scope | DebugScope.Connection);
            UnityWebRequest uwr = await _PostRequest(this, _HeaderToken, httpUrl + UMI3DNetworkingKeys.logout, null, new UMI3DDto().ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            uwr.Dispose();
            UMI3DLogger.Log($"Received PostLogout", scope | DebugScope.Connection);
        }
        #endregion

        #region media
        /// <summary>
        /// Send request using GET method to get the server Media.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task<MediaDto> SendGetMedia(Func<RequestFailedArgument, bool> shouldTryAgain)
        {
            return await SendGetMedia(httpUrl + UMI3DNetworkingKeys.media, shouldTryAgain);
        }

        /// <summary>
        /// Send request using GET method to get a Media at a specified url.
        /// </summary>
        /// <param name="url">Url to send the resquest to. For a vanilla server add '/media' at the end of the server url.</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public static async Task<MediaDto> SendGetMedia(string url, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send GetMedia", scope | DebugScope.Connection);

            using (UnityWebRequest uwr = await _GetRequest(null, null, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e)))
            {
                UMI3DLogger.Log($"Received GetMedia", scope | DebugScope.Connection);
                if (uwr?.downloadHandler.data == null) return null;
                string json = System.Text.Encoding.UTF8.GetString(uwr.downloadHandler.data);
                return UMI3DDtoSerializer.FromJson<MediaDto>(json, Newtonsoft.Json.TypeNameHandling.None);
            }
        }

        #endregion

        #region resources

        /// <summary>
        /// Send request using GET.
        /// </summary>
        /// <param name="url">Url.</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task<LibrariesDto> SendGetLibraries(Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send GetLibraries", scope | DebugScope.Connection);
            using (UnityWebRequest uwr = await _GetRequest(this, _HeaderToken, httpUrl + UMI3DNetworkingKeys.libraries, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
            {
                UMI3DLogger.Log($"Received GetLibraries", scope | DebugScope.Connection);
                UMI3DDto dto = await deserializer.FromBson(uwr?.downloadHandler.data);
                return dto as LibrariesDto;
            }
        }

        /// <summary>
        /// Get a LoadEntityDto
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="onError"></param>
        /// <param name="shouldTryAgain"></param>
        public async Task<LoadEntityDto> SendPostEntity(EntityRequestDto id, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostEntity", scope | DebugScope.Connection);
            using (UnityWebRequest uwr = await _PostRequest(this, _HeaderToken, httpUrl + UMI3DNetworkingKeys.entity, null, id.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
            {
                UMI3DLogger.Log($"Received PostEntity", scope | DebugScope.Connection);
                UMI3DDto dto = await deserializer.FromBson(uwr?.downloadHandler.data);
                return dto as LoadEntityDto;
            }
        }

        /// <summary>
        /// Send request using GET
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task<byte[]> SendGetPublic(string url, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send GetPublic {url}", scope | DebugScope.Connection);
            using (UnityWebRequest uwr = await _GetRequest(this, _HeaderToken, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), false))
            {
                UMI3DLogger.Log($"received getPublic {url}", scope | DebugScope.Connection);
                return uwr?.downloadHandler.data;
            }
        }

        /// <summary>
        /// Send request using GET method to get the a private file.
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        /// <param name="useParameterInsteadOfHeader">If true, sets authorization via parameters instead of header</param>
        public async Task<byte[]> SendGetPrivate(string url, bool useParameterInsteadOfHeader, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send GetPrivate {url}", scope | DebugScope.Connection);

            if (useParameterInsteadOfHeader)
            {
                url = SendGetPrivate(url);
            }
            int i = 0;
            while (i < 10)
            {
                i++;
                using (UnityWebRequest uwr = await _GetRequest(this, _HeaderToken, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), !useParameterInsteadOfHeader))
                {
                    UMI3DLogger.Log($"Received GetPrivate {url}\n{uwr?.responseCode}\n{uwr?.url}", scope | DebugScope.Connection);
                    if (uwr?.responseCode != 204)
                        return uwr?.downloadHandler.data;
                    UMI3DLogger.Log($"Resend GetPrivate Because responce code was 204 {url}", scope | DebugScope.Connection);
                    await UMI3DAsyncManager.Delay(1000);
                }
            }
            return null;
        }

        public virtual string SendGetPrivate(string url)
        {
            return url;
        }


        #endregion

        #region environement
        /// <summary>
        /// Send request using GET method to get the Environement.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task<GlTFEnvironmentDto> SendGetEnvironment(Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send GetEnvironment", scope | DebugScope.Connection);
            using (UnityWebRequest uwr = await _GetRequest(this, _HeaderToken, httpUrl + UMI3DNetworkingKeys.environment, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
            {
                UMI3DLogger.Log($"Received GetEnvironment", scope | DebugScope.Connection);
                UMI3DDto dto = await deserializer.FromBson(uwr?.downloadHandler.data);
                return dto as GlTFEnvironmentDto;
            }
        }

        /// <summary>
        /// Send request using POST method to Join server.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task<EnterDto> SendPostJoin(JoinDto join, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostJoin", scope | DebugScope.Connection);

            using (UnityWebRequest uwr = await _PostRequest(this, _HeaderToken, httpUrl + UMI3DNetworkingKeys.join, null, join.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
            {
                UMI3DLogger.Log($"Received PostJoin", scope | DebugScope.Connection);
                UMI3DDto dto = await deserializer.FromBson(uwr?.downloadHandler.data);
                return dto as EnterDto;
            }
        }

        /// <summary>
        /// Send request using POST method to request the server to send a Scene.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task SendPostSceneRequest(Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostSceneRequest", scope | DebugScope.Connection);
            UnityWebRequest uwr = await _PostRequest(this, _HeaderToken, httpUrl + UMI3DNetworkingKeys.scene, null, null, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            uwr.Dispose();
            UMI3DLogger.Log($"Received PostSceneRequest", scope | DebugScope.Connection);
        }

        #endregion

        #region Local Info
        /// <summary>
        /// Send request using POST method to send to the server Local Info.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        /// <param name="key">Local data file key.</param>
        public async Task SendPostLocalInfo(string key, byte[] bytes, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostLocalInfo {key}", scope | DebugScope.Connection);
            string url = System.Text.RegularExpressions.Regex.Replace(httpUrl + UMI3DNetworkingKeys.localData, ":param", key);
            UnityWebRequest uwr = await _PostRequest(this, _HeaderToken, url, null, bytes, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            uwr.Dispose();
            UMI3DLogger.Log($"Received PostLocalInfo {key}", scope | DebugScope.Connection);
        }

        /// <summary>
        /// Send request using GET method to get datas from server then save its in local file.
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task<byte[]> SendGetLocalInfo(string key, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send GetLocalInfo {key}", scope | DebugScope.Connection);
            string url = System.Text.RegularExpressions.Regex.Replace(httpUrl + UMI3DNetworkingKeys.localData, ":param", key);

            using (UnityWebRequest uwr = await _GetRequest(this, _HeaderToken, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
            {
                UMI3DLogger.Log($"Received GetLocalInfo {key}", scope | DebugScope.Connection);
                return uwr?.downloadHandler.data;
            }
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
        public async Task SendPostFile(string token, string fileName, byte[] bytes, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            string url = System.Text.RegularExpressions.Regex.Replace(httpUrl + UMI3DNetworkingKeys.uploadFile, ":param", token);
            var headers = new List<(string, string)>
            {
                (UMI3DNetworkingKeys.contentHeader, fileName)
            };
            UnityWebRequest uwr = await _PostRequest(this, _HeaderToken, url, null, bytes, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true, headers);
            uwr.Dispose();
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
        protected static async Task<UnityWebRequest> _GetRequest(AbstractHttpClient<T> instance, string HeaderToken, string url, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, List<(string, string)> headers = null, int tryCount = 0)
        {
            var www = UnityWebRequest.Get(url);
            if (UseCredential) www.SetRequestHeader(UMI3DNetworkingKeys.Authorization, HeaderToken);
            if (headers != null)
            {
                foreach ((string, string) item in headers)
                {
                    www.SetRequestHeader(item.Item1, item.Item2);
                }
            }
            DateTime date = DateTime.UtcNow;
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();
            while (!operation.isDone)
                await UMI3DAsyncManager.Yield();

#if UNITY_2020_1_OR_NEWER
            if (www.result > UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                return
                    await (instance?.Sub__GetRequest(www, date, HeaderToken, url, ShouldTryAgain, UseCredential, headers, tryCount)
                    ?? throw new Umi3dNetworkingException(www, "Failed to get "));

            }
            return www;
        }

        protected virtual async Task<UnityWebRequest> Sub__GetRequest(UnityWebRequest www, DateTime date, string HeaderToken, string url, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, List<(string, string)> headers = null, int tryCount = 0)
        {
            throw new Umi3dNetworkingException(www, "Failed to get ");
        }

        /// <summary>
        /// Ienumerator to send POST Request.
        /// </summary>
        /// <param name="url">Url to send the request at.</param>
        /// <param name="bytes">Data send via post method.</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        /// <returns></returns>
        protected static async Task<UnityWebRequest> _PostRequest(AbstractHttpClient<T> instance, string HeaderToken, string url, string contentType, byte[] bytes, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, List<(string, string)> headers = null, int tryCount = 0)
        {

            UnityWebRequest www = CreatePostRequest(url, bytes, contentType, true);
            if (UseCredential) www.SetRequestHeader(UMI3DNetworkingKeys.Authorization, HeaderToken);
            if (headers != null)
            {
                foreach ((string, string) item in headers)
                {
                    www.SetRequestHeader(item.Item1, item.Item2);
                }
            }
            DateTime date = DateTime.UtcNow;

            UnityWebRequestAsyncOperation operation = www.SendWebRequest();
            while (!operation.isDone)
                await UMI3DAsyncManager.Yield();

#if UNITY_2020_1_OR_NEWER
            if (www.result > UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                return
                    await (instance?.Sub_PostRequest(www, date, HeaderToken, url, contentType, bytes, ShouldTryAgain, UseCredential, headers, tryCount)
                    ?? throw new Umi3dNetworkingException(www, "Failed to get "));

            }
            return www;
        }

        protected virtual async Task<UnityWebRequest> Sub_PostRequest(UnityWebRequest www, DateTime date, string HeaderToken, string url, string contentType, byte[] bytes, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, List<(string, string)> headers = null, int tryCount = 0)
        {
            UnityEngine.Debug.Log(System.Text.Encoding.ASCII.GetString(bytes));
            throw new Umi3dNetworkingException(www, " Failed to post\n" + www.downloadHandler.text);
        }


        /// <summary>
        /// Util function to create POST request.
        /// </summary>
        /// <param name="url">Url to send the request at.</param>
        /// <param name="bytes">Data send via post method.</param>
        /// <param name="withResult">require a result</param>
        /// <returns></returns>
        private static UnityWebRequest CreatePostRequest(string url, byte[] bytes, string contentType = null, bool withResult = false)
        {
            var requestU = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            var uH = new UploadHandlerRaw(bytes);
            if (contentType != null)
                uH.contentType = contentType;
            requestU.uploadHandler = uH;
            if (withResult)
                requestU.downloadHandler = new DownloadHandlerBuffer();
            //requestU.SetRequestHeader("access_token", UMI3DClientServer.GetToken(null));
            return requestU;
        }
        #endregion
    }
}