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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.interaction;
using UnityEngine;
using UnityEngine.Networking;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Send HTTP requests to the environment server.
    /// </summary>
    /// Usually used before connection or to retrieve DTOs.
    public class HttpClient
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Networking;

        internal string HeaderToken;

        private string httpUrl => environmentClient.connectionDto.httpUrl;

        private readonly ThreadDeserializer deserializer;
        private readonly UMI3DEnvironmentClient environmentClient;

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
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(connectionDto.ToJson(Newtonsoft.Json.TypeNameHandling.None));

            using (UnityWebRequest uwr = await _PostRequest(null, MasterUrl + UMI3DNetworkingKeys.connect, "application/json", bytes, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), false))
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

            ConnectionFormDto dto3 = UMI3DDtoSerializer.FromJson<ConnectionFormDto>(text, Newtonsoft.Json.TypeNameHandling.None, new List<JsonConverter>() { new ParameterConverter() });

            if (dto1 != null && dto1?.globalToken != null && dto1?.connectionDto != null)
                return dto1;
            else if (dto2 != null && dto2?.GlobalToken != null && dto2?.connectionDto != null)
                return dto2.ToPrivateIdentity();
            else
                return dto3;

        }

        public class ParameterConverter : Newtonsoft.Json.JsonConverter
        {
            public override bool CanRead => true;

            public override bool CanWrite => false;

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(AbstractParameterDto);
            }


            public AbstractParameterDto ReadObjectArray(JObject obj, JToken tokenA)
            {
                switch (ReadObjectValue(obj))
                {
                    case Color col:
                        return new EnumParameterDto<Color>()
                        {
                            possibleValues = tokenA.Values<object>().Select(objA => (Color)ReadObjectValue(objA as JObject)).ToList(),
                            value = col
                        };
                    case Vector4 v4:
                        return new EnumParameterDto<Vector4>()
                        {
                            possibleValues = tokenA.Values<object>().Select(objA => (Vector4)ReadObjectValue(objA as JObject)).ToList(),
                            value = v4
                        };
                    case Vector3 v3:
                        return new EnumParameterDto<Vector3>()
                        {
                            possibleValues = tokenA.Values<object>().Select(objA => (Vector3)ReadObjectValue(objA as JObject)).ToList(),
                            value = v3
                        };
                    case Vector2 v2:
                        return new EnumParameterDto<Vector2>()
                        {
                            possibleValues = tokenA.Values<object>().Select(objA => (Vector2)ReadObjectValue(objA as JObject)).ToList(),
                            value = v2
                        };
                }
                UnityEngine.Debug.LogError($"Missing case. {obj}");
                return null;
            }

            public AbstractParameterDto ReadObject(JObject obj)
            {
                switch (ReadObjectValue(obj))
                {
                    case Color col:
                        return new ColorParameterDto
                        {
                            value = col.Dto()
                        };
                    case Vector4 v4:
                        return new Vector4ParameterDto
                        {
                            value = v4.Dto()
                        };
                    case Vector3 v3:
                        return new Vector3ParameterDto
                        {
                            value = v3.Dto()
                        };
                    case Vector2 v2:
                        return new Vector2ParameterDto
                        {
                            value = v2.Dto()
                        };
                }
                UnityEngine.Debug.LogError($"Missing case. {obj}");
                return null;
            }

            public object ReadObjectValue(JObject obj)
            {
                if (obj.TryGetValue("R", out JToken tokenR)
                    && obj.TryGetValue("G", out JToken tokenG)
                    && obj.TryGetValue("B", out JToken tokenB)
                    && obj.TryGetValue("A", out JToken tokenA))
                {
                    return  new Color(tokenR.ToObject<float>(), tokenG.ToObject<float>(), tokenB.ToObject<float>(), tokenA.ToObject<float>());
                }

                if (obj.TryGetValue("X", out JToken tokenX)
                    && obj.TryGetValue("Y", out JToken tokenY))
                {
                    if (obj.TryGetValue("Z", out JToken tokenZ))
                    {
                        if (obj.TryGetValue("W", out JToken tokenW))
                            return  new Vector4(tokenX.ToObject<float>(), tokenY.ToObject<float>(), tokenZ.ToObject<float>(), tokenW.ToObject<float>());
                        return  new Vector3(tokenX.ToObject<float>(), tokenY.ToObject<float>(), tokenZ.ToObject<float>());
                    }
                    return  new Vector2(tokenX.ToObject<float>(), tokenY.ToObject<float>());
                }
                UnityEngine.Debug.LogError($"Missing case. {obj}");
                return null;
            }


            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var jo = JObject.Load(reader);
                AbstractParameterDto dto = null;
                bool isArray = false;
                isArray = jo.TryGetValue("possibleValues", out JToken tokenA);

                if (jo.TryGetValue("value", out JToken token))
                {
                    switch (token.Type)
                    {
                        case JTokenType.String:
                            if (isArray)
                                dto = new EnumParameterDto<string>()
                                {
                                    possibleValues = tokenA.Values<string>().ToList(),
                                    value = token.ToObject<string>()
                                };
                            else
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
                            if (isArray)
                                dto = new EnumParameterDto<float>()
                                {
                                    possibleValues = tokenA.Values<float>().ToList(),
                                    value = token.ToObject<float>()
                                };
                            else
                                dto = new FloatParameterDto()
                                {
                                    value = token.ToObject<float>()
                                };
                            break;
                        case JTokenType.Integer:
                            if (isArray)
                                dto = new EnumParameterDto<int>()
                                {
                                    possibleValues = tokenA.Values<int>().ToList(),
                                    value = token.ToObject<int>()
                                };
                            else
                                dto = new IntegerParameterDto()
                                {
                                    value = token.ToObject<int>()
                                };
                            break;
                        case JTokenType.Object:
                            var obj = token.ToObject<object>() as JObject;
                            if (isArray)
                                dto = ReadObjectArray(obj, tokenA);
                            else
                                dto = ReadObject(obj);
                            break;
                        default:
                            UnityEngine.Debug.LogError($"TODO Add Case for Color, Range or Vector 2 3 4. {token.Type}");
                            break;
                    }
                }
                if (dto == null)
                    return null;

                if (jo.TryGetValue("privateParameter", out JToken tokenp))
                    dto.privateParameter = tokenp.ToObject<bool>();
                if (jo.TryGetValue("isDisplayer", out JToken tokendisp))
                    dto.isDisplayer = tokendisp.ToObject<bool>();
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

        private class FakePrivateIdentityDto : IdentityDto
        {
            public string GlobalToken;
            public string connectionDto;
            public List<LibrariesDto> libraries;

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

            using (UnityWebRequest uwr = await _GetRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.connectionInfo, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
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
            using (UnityWebRequest uwr = await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.connection_information_update, null, answer.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
            {
                try
                {
                    var b = uwr?.downloadHandler.data;
                    if(b != null)
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
            UnityWebRequest uwr = await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.status_update, null, new StatusDto() { status = status }.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
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
            UnityWebRequest uwr = await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.logout, null, new UMI3DDto().ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
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

            using (UnityWebRequest uwr = await _GetRequest(null, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e)))
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
            using (UnityWebRequest uwr = await _GetRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.libraries, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
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
            using (UnityWebRequest uwr = await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.entity, null, id.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
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
            using (UnityWebRequest uwr = await _GetRequest(HeaderToken, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), false))
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
                if (UMI3DResourcesManager.HasUrlGotParameters(url))
                    url += "&" + UMI3DNetworkingKeys.ResourceServerAuthorization + "=" + HeaderToken;
                else
                    url += "?" + UMI3DNetworkingKeys.ResourceServerAuthorization + "=" + HeaderToken;
            }
            int i = 0;
            while (i < 10)
            {
                i++;
                using (UnityWebRequest uwr = await _GetRequest(HeaderToken, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), !useParameterInsteadOfHeader))
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
            using (UnityWebRequest uwr = await _GetRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.environment, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
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

            using (UnityWebRequest uwr = await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.join, null, join.ToBson(), (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
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
            UnityWebRequest uwr = await _PostRequest(HeaderToken, httpUrl + UMI3DNetworkingKeys.scene, null, null, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
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
            UnityWebRequest uwr = await _PostRequest(HeaderToken, url, null, bytes, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true);
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

            using (UnityWebRequest uwr = await _GetRequest(HeaderToken, url, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true))
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
            UnityWebRequest uwr = await _PostRequest(HeaderToken, url, null, bytes, (e) => shouldTryAgain?.Invoke(e) ?? DefaultShouldTryAgain(e), true, headers);
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
        private static async Task<UnityWebRequest> _GetRequest(string HeaderToken, string url, Func<RequestFailedArgument, bool> ShouldTryAgain, bool UseCredential = false, List<(string, string)> headers = null, int tryCount = 0)
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

                if (UMI3DClientServer.Exists && await UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(www, tryCount, date, ShouldTryAgain)))
                    return await _GetRequest(HeaderToken, url, ShouldTryAgain, UseCredential, headers, tryCount + 1);
                else
                    throw new Umi3dNetworkingException(www, "Failed to get ");
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
                if (UMI3DClientServer.Exists && await UMI3DClientServer.Instance.TryAgainOnHttpFail(new RequestFailedArgument(www, tryCount, date, ShouldTryAgain)))
                    return await _PostRequest(HeaderToken, url, contentType, bytes, ShouldTryAgain, UseCredential, headers, tryCount + 1);
                else
                {
                    UnityEngine.Debug.Log(System.Text.Encoding.ASCII.GetString(bytes));
                    throw new Umi3dNetworkingException(www, " Failed to post\n" + www.downloadHandler.text);
                }
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