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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.interaction;
using UnityEngine.Networking;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Class to send Http Request.
    /// </summary>
    public class HttpClient
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Networking;

        internal string HeaderToken;

        private string httpUrl => environmentClient.connectionDto.httpUrl;

        private readonly ThreadDeserializer deserializer;

        UMI3DEnvironmentClient environmentClient;

        /// <summary>
        /// Init HttpClient.
        /// </summary>
        /// <param name="UMI3DClientServer"></param>
        public HttpClient(UMI3DEnvironmentClient environmentClient)
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
            HeaderToken = UMI3DNetworkingKeys.bearer + token;
        }

        private static bool DefaultShouldTryAgain(RequestFailedArgument argument)
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
            var bytes = System.Text.Encoding.UTF8.GetBytes(connectionDto.ToJson(Newtonsoft.Json.TypeNameHandling.None));
            var uwr = await _PostRequest(null, MasterUrl + UMI3DNetworkingKeys.connect, "application/json", bytes, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), false);
            UMI3DLogger.Log($"Received answer to Connect", scope | DebugScope.Connection);
            var dto = uwr?.downloadHandler.data != null ? ReadConnectAnswer(System.Text.Encoding.UTF8.GetString(uwr?.downloadHandler.data)) : null;
            return dto;
        }

        static UMI3DDto ReadConnectAnswer(string text)
        {
            var dto1 = UMI3DDto.FromJson<PrivateIdentityDto>(text, Newtonsoft.Json.TypeNameHandling.None);
            var dto2 = UMI3DDto.FromJson<ConnectionFormDto>(text, Newtonsoft.Json.TypeNameHandling.None, new List<JsonConverter>() { new FooConverter() });

            if (dto1?.GlobalToken != null && dto1?.connectionDto != null)
                return dto1;
            else
                return dto2;
        }

        public class FooConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanRead => true;

            public override bool CanWrite => false;

            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(AbstractParameterDto));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JObject jo = JObject.Load(reader);
                AbstractParameterDto dto = null;
                if (jo.TryGetValue("possibleValues", out JToken tokenA))
                {
                    UnityEngine.Debug.Log(tokenA.Type.ToString());
                }
                if (jo.TryGetValue("value", out JToken token))
                {
                    var obj = token.Value<object>();

                    switch (token.Type)
                    {
                        case JTokenType.String:
                            dto = new StringParameterDto()
                            {
                                value = token.ToObject<string>()
                            };
                            break;
                        case JTokenType.Boolean:
                            dto = new BooleanParameterDto()
                            {
                                value = token.ToObject<bool>()
                            };
                            break;
                        case JTokenType.Float:
                            dto = new FloatParameterDto()
                            {
                                value = token.ToObject<float>()
                            };
                            break;
                        case JTokenType.Integer:
                            dto = new IntegerParameterDto()
                            {
                                value = token.ToObject<int>()
                            };
                            break;
                    }
                }

                if (jo.TryGetValue("description", out JToken tokend))
                    dto.description = tokend.ToObject<string>();
                if (jo.TryGetValue("id", out JToken tokeni))
                    dto.id = (ulong)tokeni.ToObject<int>();
                if (jo.TryGetValue("name", out JToken tokenn))
                    dto.name = tokenn.ToObject<string>();
                if (jo.TryGetValue("icon2D", out JToken tokenI2))
                    dto.icon2D = tokenI2.ToObject<ResourceDto>();
                if (jo.TryGetValue("icon3D", out JToken tokenI3))
                    dto.icon3D = tokenI3.ToObject<ResourceDto>();

                return dto;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
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
            var uwr = await _GetRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.connectionInfo, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            UMI3DLogger.Log($"Received Get Identity", scope | DebugScope.Connection);
            var dto = await deserializer.FromBson(uwr?.downloadHandler.data);
            return dto as UserConnectionDto;
        }


        /// <summary>
        /// Send request using POST method to update user Identity.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async void SendPostUpdateIdentityAsync(UserConnectionAnswerDto answer, bool throwError = false, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            try
            {
                await SendPostUpdateIdentity(answer, shouldTryAgain);
            }
            catch (UMI3DAsyncManagerException e)
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
        public async Task SendPostUpdateIdentity(UserConnectionAnswerDto answer, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostUpdateIdentity", scope | DebugScope.Connection);
            await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.connection_information_update, null, answer.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            UMI3DLogger.Log($"Received PostUpdateIdentity", scope | DebugScope.Connection);
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
            catch (UMI3DAsyncManagerException e)
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
            await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.status_update, null, new StatusDto() { status = status }.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
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
            await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.logout, null, new UMI3DDto().ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
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
            var uwr = await _GetRequest(null, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e));
            UMI3DLogger.Log($"Received GetMedia", scope | DebugScope.Connection);
            if (uwr?.downloadHandler.data == null) return null;
            var json = System.Text.Encoding.UTF8.GetString(uwr.downloadHandler.data);
            return UMI3DDto.FromJson<MediaDto>(json,Newtonsoft.Json.TypeNameHandling.None) as MediaDto;
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
            var uwr = await _GetRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.libraries, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            UMI3DLogger.Log($"Received GetLibraries", scope | DebugScope.Connection);
            var dto = await deserializer.FromBson(uwr?.downloadHandler.data);
            return dto as LibrariesDto;
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
            var uwr = await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.entity, null, id.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            UMI3DLogger.Log($"Received PostEntity", scope | DebugScope.Connection);
            var dto = await deserializer.FromBson(uwr?.downloadHandler.data);
            return dto as LoadEntityDto;
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
            var uwr = await _GetRequest(HeaderToken, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), false);
            UMI3DLogger.Log($"received getPublic {url}", scope | DebugScope.Connection);
            return uwr?.downloadHandler.data;
        }

        /// <summary>
        /// Send request using GET method to get the a private file.
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task<byte[]> SendGetPrivate(string url, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send GetPrivate {url}", scope | DebugScope.Connection);
            var uwr = await _GetRequest(HeaderToken, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            UMI3DLogger.Log($"Received GetPrivate {url}", scope | DebugScope.Connection);
            return uwr?.downloadHandler.data;
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
            var uwr = await _GetRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.environment, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            UMI3DLogger.Log($"Received GetEnvironment", scope | DebugScope.Connection);
            var dto = await deserializer.FromBson(uwr?.downloadHandler.data);
            return dto as GlTFEnvironmentDto;
        }

        /// <summary>
        /// Send request using POST method to Join server.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task<EnterDto> SendPostJoin(JoinDto join, Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostJoin", scope | DebugScope.Connection);
            var uwr = await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.join, null, join.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            UMI3DLogger.Log($"Received PostJoin", scope | DebugScope.Connection);
            var dto = await deserializer.FromBson(uwr?.downloadHandler.data);
            return dto as EnterDto;
        }

        /// <summary>
        /// Send request using POST method to request the server to send a Scene.
        /// </summary>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        public async Task SendPostSceneRequest(Func<RequestFailedArgument, bool> shouldTryAgain = null)
        {
            UMI3DLogger.Log($"Send PostSceneRequest", scope | DebugScope.Connection);
            await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.scene, null, null, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
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
            await _PostRequest(HeaderToken, url, null, bytes, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
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
            var uwr = await _GetRequest(HeaderToken, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
            UMI3DLogger.Log($"Received GetLocalInfo {key}", scope | DebugScope.Connection);
            return uwr?.downloadHandler.data;
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
            await _PostRequest(HeaderToken, url, null, bytes, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true, headers);
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
        private static async Task<UnityWebRequest> _GetRequest(string HeaderToken, string url, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, List<(string, string)> headers = null, int tryCount = 0)
        {
            if (!UMI3DClientServer.Exists)
                return null;

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
            var operation = www.SendWebRequest();
            while (!operation.isDone)
                await UMI3DAsyncManager.Yield();

#if UNITY_2020_1_OR_NEWER
            if (www.result > UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {

                if (UMI3DClientServer.Exists && await UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(www, tryCount, date, ShouldTryAgain)))
                    return await _GetRequest(HeaderToken, url, ShouldTryAgain, UseCredential, headers, tryCount + 1);
                else
                    throw new Umi3dException(www.responseCode, www.error + " Failed to get " + www.url);
            }
            return www;
        }

        /// <summary>
        /// Ienumerator to send POST Request.
        /// </summary>
        /// <param name="url">Url to send the request at.</param>
        /// <param name="bytes">Data send via post method.</param>
        /// <param name="callback">Action to be call when the request succeed.</param>
        /// <param name="onError">Action to be call when the request fail.</param>
        /// <returns></returns>
        private static async Task<UnityWebRequest> _PostRequest(string HeaderToken, string url, string contentType, byte[] bytes, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, List<(string, string)> headers = null, int tryCount = 0)
        {
            if (!UMI3DClientServer.Exists)
                return null;

            UnityWebRequest www = CreatePostRequest(url, bytes, true);
            if (UseCredential) www.SetRequestHeader(UMI3DNetworkingKeys.Authorization, HeaderToken);
            if (headers != null)
            {
                foreach ((string, string) item in headers)
                {
                    www.SetRequestHeader(item.Item1, item.Item2);
                }
            }
            DateTime date = DateTime.UtcNow;

            var operation = www.SendWebRequest();
            while (!operation.isDone)
                await UMI3DAsyncManager.Yield();

#if UNITY_2020_1_OR_NEWER
            if (www.result > UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {

                if (UMI3DClientServer.Exists && await UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(www, tryCount, date, ShouldTryAgain)))
                    return await _PostRequest(HeaderToken, url, contentType, bytes, ShouldTryAgain, UseCredential, headers, tryCount + 1);
                else
                    throw new Umi3dException(www.responseCode, www.error + " Failed to post " + www.url);
            }
            return www;
        }

        /// <summary>
        /// Util function to create POST request.
        /// </summary>
        /// <param name="url">Url to send the request at.</param>
        /// <param name="bytes">Data send via post method.</param>
        /// <param name="withResult">require a result</param>
        /// <returns></returns>
        private static UnityWebRequest CreatePostRequest(string url, byte[] bytes, bool withResult = false)
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