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
using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
using umi3d.edk.interaction;
using UnityEngine.Events;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace umi3d.edk.collaboration
{

    /// <summary>
    /// Environment API to handle HTTP requests.
    /// </summary>
    public class UMI3DEnvironmentApi : UMI3DAbstractEnvironmentApi
    {      
        public UMI3DEnvironmentApi()
        { }

        public void Stop()
        { }

        private UMI3DCollaborationUser GetUserFor(HttpListenerRequest request)
        {
            (UMI3DCollaborationUser user, bool oldToken) c = UMI3DCollaborationServer.GetUserFor(request);
            return c.user;
        }

        #region users

        /// <summary>
        /// GET "/me"
        /// Get the user's information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpGet(UMI3DNetworkingKeys.connectionInfo, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void GetConnectionInformation(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DCollaborationUser user = GetUserFor(e.Request);
            UMI3DLogger.Log($"Get Connection Information {user?.Id()}", scope);
            try
            {
                var connectionInformation = user.ToUserConnectionDto();
                e.Response.WriteContent(connectionInformation.ToBson());
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }

        /// <summary>
        /// POST "/me/status"
        /// Updates the user's information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpPost(UMI3DNetworkingKeys.status_update, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void UpdateStatus(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DCollaborationUser user = GetUserFor(e.Request);
            UMI3DLogger.Log($"Update Status {user?.Id()}", scope);
            bool finished = false;

            ReadDto(e.Request,
                (dto) =>
                {
                    try
                    {
                        var status = dto as StatusDto;
                        UMI3DLogger.Log($"Update Status {user?.Id()} {status}", scope);
                        UnityMainThreadDispatcher.Instance().Enqueue(_updateStatus(user, status));
                    }
                    catch (Exception ex)
                    {
                        UMI3DLogger.LogException(ex, scope);
                    }

                    finished = true;
                });
            while (!finished) System.Threading.Thread.Sleep(1);
        }

        private IEnumerator _updateStatus(UMI3DCollaborationUser user, StatusDto dto)
        {
            UMI3DCollaborationServer.Instance.UpdateStatus(user, dto);
            yield break;
        }

        /// <summary>
        /// POST "/me/update"
        /// Updates the user's information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpPost(UMI3DNetworkingKeys.connection_information_update, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void UpdateConnectionInformation(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DCollaborationUser user = GetUserFor(e.Request);
            UMI3DLogger.Log($"Update Connection Information {user?.Id()}", scope);
            bool finished = false;
            ReadDto(e.Request, (dto) =>
            {
                try
                {
                    var anw = dto as UserConnectionAnswerDto;
                    UnityMainThreadDispatcher.Instance().Enqueue(_updateConnectionInformation(user, anw));
                    var pt = UMI3DCollaborationServer.Instance.IsThereTransactionPending(user);
                    e.Response.WriteContent(pt.ToBson());
                }
                catch (Exception ex)
                {
                    UMI3DLogger.LogException(ex, scope);
                }

                finished = true;
            });
            while (!finished) System.Threading.Thread.Sleep(1);
        }

        private IEnumerator _updateConnectionInformation(UMI3DCollaborationUser user, UserConnectionAnswerDto dto)
        {
            UMI3DLogger.Log($"Update Connection Information {user?.Id()} {dto?.status} {dto?.librariesUpdated} {dto?.parameters?.answers?.Select((a) => a.parameter.ToString()).ToString<string>()}", scope);
            user.SetStatus(UMI3DCollaborationServer.Instance.Identifier.UpdateIdentity(user, dto));
            user.forgeServer.SendSignalingMessage(user.networkPlayer, user.ToStatusDto());
            yield break;
        }

        /// <summary>
        /// POST "/logout"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpPost(UMI3DNetworkingKeys.logout, WebServiceMethodAttribute.Security.PrivateAllowOldToken, WebServiceMethodAttribute.Type.Method)]
        public void Logout(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            (UMI3DCollaborationUser user, bool oldToken) c = UMI3DCollaborationServer.GetUserFor(e.Request);
            if (c.user != null)
            {
                UMI3DLogger.Log($"Logout {c.user.Id()}", scope);
                if (c.user != null)
                {
                    UMI3DCollaborationServer.Logout(c.user);
                }
            }
            else if (c.oldToken)
            {
                UMI3DLogger.Log($"Logout ignore for old token", scope);
            }
        }

        #endregion

        #region resources
        /// <summary>
        /// GET Libraries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpGet(UMI3DNetworkingKeys.libraries, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void GetLibraries(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DUser user = GetUserFor(e.Request);
            UMI3DLogger.Log($"Get Libraries {user?.Id()}", scope);
            e.Response.WriteContent(UMI3DEnvironment.Instance.ToLibrariesDto(user).ToBson());
        }

        /// <summary>
        /// GET "/file/public/"
        /// Handles the GET public file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpGet(UMI3DNetworkingKeys.publicFiles, WebServiceMethodAttribute.Security.Public, WebServiceMethodAttribute.Type.Directory)]
        public void GetPublicFile(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {

            string file = e.Request.RawUrl.Substring(UMI3DNetworkingKeys.publicFiles.Length);
            file = inetum.unityUtils.Path.Combine(
                UMI3DServer.publicRepository, file);
            file = System.Uri.UnescapeDataString(file);
            UMI3DLogger.Log($"Get public file {file}", scope);
            //Validate url.
            HttpListenerResponse res = e.Response;
            if (UMI3DServer.IsInPublicRepository(file))
            {
                GetFile(file, res);
            }
            else
            {
                UMI3DLogger.LogError($"Get public file forbiden {file}", scope);
                res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.Forbidden;
                res.StatusDescription = "Requested file is located in a forbidden area !";
                res.WriteContent(null);
            }
        }

        /// <summary>
        /// GET "/file/private/"
        /// Handles the GET private file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpGet(UMI3DNetworkingKeys.privateFiles, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Directory)]
        public void GetPrivateFile(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            string file = e.Request.RawUrl.Substring(UMI3DNetworkingKeys.privateFiles.Length);
            file = inetum.unityUtils.Path.Combine(UMI3DServer.privateRepository, file);
            file = System.Uri.UnescapeDataString(file);
            UMI3DLogger.Log($"Get private file{file}", scope);
            //Validate url.
            HttpListenerResponse res = e.Response;
            if (UMI3DServer.IsInPrivateRepository(file) || UMI3DServer.IsInPublicRepository(file))
            {
                GetFile(file, res);
            }
            else
            {
                UMI3DLogger.LogError($"Get private file failed {file}", scope);
                res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.Forbidden;
                res.StatusDescription = "Requested file is located in a forbidden area !";
                res.WriteContent(null);
            }
        }

        /// <summary>
        /// GET "/directory/{path}/"
        /// Handles the file list in a directory.
        /// Return a list of local paths
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpGet(UMI3DNetworkingKeys.directory, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Directory)]
        public void GetDirectory(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            string rawDirectory = e.Request.RawUrl.Substring(UMI3DNetworkingKeys.directory.Length);
            rawDirectory = System.Uri.UnescapeDataString(rawDirectory);
            string directory = inetum.unityUtils.Path.Combine(UMI3DServer.dataRepository, rawDirectory);
            UMI3DLogger.Log($"Get Directory {directory}", scope);
            //Validate url.
            HttpListenerResponse res = e.Response;
            if (UMI3DServer.IsInDataRepository(directory))
            {
                if (Directory.Exists(directory))
                {
                    var dto = new FileListDto()
                    {
                        files = GetDir(directory).Select(f => System.Uri.EscapeUriString(f)).ToList(),
                        baseUrl = System.Uri.EscapeUriString(inetum.unityUtils.Path.Combine(UMI3DServer.GetResourcesUrl(), UMI3DNetworkingKeys.files, rawDirectory))
                    };

                    res.WriteContent(dto.ToBson());
                }
                else
                {
                    UMI3DLogger.LogError($"Get directory not found {directory}", scope);
                    Return404(res, "This directory doesn't exists!");
                }
            }
            else
            {
                UMI3DLogger.LogError($"Get directory forbiden {directory}", scope);
                res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.Forbidden;
                res.StatusDescription = "Requested directory is located in a forbidden area !";
                res.WriteContent(null);
            }
        }

        /// <summary>
        /// GET "/zip/{path}/"
        /// download a directory as zip.
        /// Return a list of local paths
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpGet(UMI3DNetworkingKeys.directory_zip, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Directory)]
        public void GetDirectoryAsZip(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            string directory = e.Request.RawUrl.Substring(UMI3DNetworkingKeys.directory_zip.Length);
            directory = inetum.unityUtils.Path.Combine(UMI3DServer.dataRepository, directory);
            directory = System.Uri.UnescapeDataString(directory);
            UMI3DLogger.Log($"Get directory as zip {directory}", scope);
            //Validate url.
            HttpListenerResponse res = e.Response;
            if (UMI3DServer.IsInDataRepository(directory))
            {
                if (Directory.Exists(directory))
                {
                    ReturnNotImplemented(res);
                }
                else
                {
                    UMI3DLogger.LogError($"Get directory as zip not found {directory}", scope);
                    Return404(res, "This directory doesn't exists!");
                }
            }
            else
            {
                UMI3DLogger.LogError($"Get directory as zip forbiden {directory}", scope);
                res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.Forbidden;
                res.StatusDescription = "Requested directory is located in a forbidden area !";
                res.WriteContent(null);
            }
        }

        /// <summary>
        /// Handles an authorized file access.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        private void GetFile(string file, HttpListenerResponse response)
        {
            //file = file.Replace("/", "\\");
            byte[] content = File.Exists(file) ? File.ReadAllBytes(file) : null;
            if (content == null)
            {
                Return404(response);
                UMI3DLogger.LogError(file, scope);
                return;
            }
            if (file.EndsWith(".html"))
            {
                response.ContentType = "text/html";
                response.ContentEncoding = Encoding.UTF8;
            }
            else if (file.EndsWith(".js"))
            {
                response.ContentType = "application/javascript";
                response.ContentEncoding = Encoding.UTF8;
            }
            response.WriteContent(content);
        }

        /// <summary>
        /// Handles an authorized directory access.
        /// </summary>
        private List<string> GetDir(string directory, string localpath = "/")
        {
            var files = new List<string>();
            IEnumerable<string> localFiles = Directory.GetFiles(directory).Select(full => System.IO.Path.GetFileName(full));
            IEnumerable<string> uris = localFiles.Select(f => inetum.unityUtils.Path.Combine(localpath, f));
            files.AddRange(uris);
            foreach (string susdir in Directory.GetDirectories(directory))
            {
                files.AddRange(GetDir(susdir, inetum.unityUtils.Path.Combine(localpath, System.IO.Path.GetFileName(System.IO.Path.GetFileName(susdir)))));
            }

            return files;
        }

        #endregion

        #region environment

        /// <summary>
        /// GET "/environment"
        /// Get the environment description.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpGet(UMI3DNetworkingKeys.environment, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void GetEnvironment(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DUser user = GetUserFor(e.Request);
            UMI3DLogger.Log($"Get Environment {user?.Id()}", scope);
            UMI3DEnvironment environment = UMI3DEnvironment.Instance;
            if (environment == null)
            {
                UMI3DLogger.LogError($"UMI3DEnvironment is missing ! {user?.Id()}", scope);
                Return404(e.Response, "UMI3DEnvironment is missing !");
            }
            else if (user == null)
            {
                UMI3DLogger.LogError($"UMI3DUser is missing !", scope);
                Return404(e.Response, "UMI3DUser is missing !");
            }
            else
            {
                while (!user.IsReadyToGetResources)
                    System.Threading.Thread.Sleep(1);
                GlTFEnvironmentDto result = null;
                bool finished = false;
                UnityMainThreadDispatcher.Instance().Enqueue(
                    _GetEnvironment(
                        environment, user,
                        (res) => { result = res; finished = true; },
                        () => { finished = true; }
                    ));

                while (!finished) System.Threading.Thread.Sleep(1);

                e.Response.WriteContent(result?.ToBson() ?? new byte[0]);
            }
            UMI3DLogger.Log($"End Get Environment {user?.Id()}", scope);
        }

        private IEnumerator _GetEnvironment(UMI3DEnvironment environment, UMI3DUser user, Action<GlTFEnvironmentDto> callback, Action error)
        {
            try
            {
                callback.Invoke(environment.ToDto(user));
            }
            catch (Exception ex)
            {
                UMI3DLogger.LogError("Error while getting environment", scope);
                UMI3DLogger.LogException(ex, scope);

                error?.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// POST "/environment/join"
        /// Join the UMI3DEnvironment
        /// join dto TBD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpPost(UMI3DNetworkingKeys.join, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void JoinEnvironment(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DCollaborationUser user = GetUserFor(e.Request);
            UMI3DLogger.Log($"Join environment {user?.Id()}", scope);
            bool finished = false;

            ReadDto(e.Request, (dto) =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(async () =>
                {
                    try
                    {
                        JoinDto join = dto as JoinDto;
                        await user.JoinDtoReception(join);

                        e.Response.WriteContent(UMI3DEnvironment.ToEnterDto(user).ToBson());
                        await UMI3DCollaborationServer.NotifyUserJoin(user);
                    }
                    catch (Exception ex)
                    {
                        UMI3DLogger.LogException(ex, scope);
                    }

                    finished = true;
                });
            });
            while (!finished) System.Threading.Thread.Sleep(1);

            UMI3DLogger.Log($"End Join environment {user?.Id()}", scope);
        }

        /// <summary>
        /// GET "/environment"
        /// Get the environment description.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpPost(UMI3DNetworkingKeys.entity, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void PostEntity(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DCollaborationUser user = GetUserFor(e.Request);
            UMI3DLogger.Log($"Post Entity {user?.Id()}", scope);
            bool finished2 = false;
            ReadDto(e.Request, (dto) =>
            {

                var entityDto = dto as EntityRequestDto;

                IEnumerable<(ulong id, (UMI3DLoadableEntity entity, bool exist, bool found))> Allentities = entityDto.entitiesId.Select(id => (id, UMI3DEnvironment.GetEntityIfExist<UMI3DLoadableEntity>(id)));
                IEnumerable<(ulong id, UMI3DLoadableEntity entity)> entities = Allentities.Where(el => el.Item2.found && el.Item2.exist)?.Select(el2 => (el2.id, el2.Item2.entity)) ?? new List<(ulong id, UMI3DLoadableEntity entity)>();
                IEnumerable<ulong> oldentities = Allentities.Where(el => el.Item2.found && !el.Item2.exist)?.Select(el2 => el2.id) ?? new List<ulong>();
                IEnumerable<ulong> entitiesNotFound = Allentities.Where(el => !el.Item2.found)?.Select(el2 => el2.id) ?? new List<ulong>();

                if (entities != null)
                {
                    LoadEntityDto result = null;
                    bool finished = false;
                    bool ok = true;
                    UnityMainThreadDispatcher.Instance().Enqueue(
                        _GetEnvironment(
                            entities, user,
                            (res) =>
                            {
                                result = res;
                                result.entities.AddRange(oldentities.Select(el => new MissingEntityDto() { id = el, reason = MissingEntityDtoReason.Unregistered }));
                                result.entities.AddRange(entitiesNotFound.Select(el => new MissingEntityDto() { id = el, reason = MissingEntityDtoReason.NotFound }));
                                finished = true;
                            },
                            () => { ok = false; finished = true; }
                        ));
                    while (!finished) System.Threading.Thread.Sleep(1);
                    if (ok)
                    {
                        e.Response.WriteContent(result.ToBson());
                    }
                    else
                    {
                        Return404(e.Response, "Internal Error");
                    }
                }
                else
                {
                    Return404(e.Response, "Internal Error");
                }
                finished2 = true;
            });
            while (!finished2) System.Threading.Thread.Sleep(1);
        }

        private IEnumerator _GetEnvironment((ulong, UMI3DLoadableEntity) entity, UMI3DUser user, Action<LoadEntityDto> callback, Action error)
        {
            return _GetEnvironment(new List<(ulong, UMI3DLoadableEntity)>() { entity }, user, callback, error);
        }

        private IEnumerator _GetEnvironment(IEnumerable<(ulong, UMI3DLoadableEntity)> entities, UMI3DUser user, Action<LoadEntityDto> callback, Action error)
        {
            try
            {
                var load = new LoadEntityDto()
                {
                    entities = entities.Select((e) =>
                    {
                        try
                        {
                            return e.Item2?.ToEntityDto(user) ?? new MissingEntityDto() { id = e.Item1, reason = MissingEntityDtoReason.ServerInternalError };
                        }
                        catch (Exception ex)
                        {
                            UMI3DLogger.LogWarning($"An error occured while writting the entityDto [{e.Item1}] type : {e.Item2?.GetType()} {ex}", scope);
                            return new MissingEntityDto() { id = e.Item1, reason = MissingEntityDtoReason.ServerInternalError };
                        }
                    }).ToList(),
                };
                callback?.Invoke(load);
            }
            catch (Exception ex)
            {
                UMI3DLogger.LogException(ex, scope);
                error?.Invoke();
            }

            yield return null;
        }

        /// <summary>
        /// GET "/environment/scene/:id"
        /// Get a scene description.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpGet(UMI3DNetworkingKeys.scene, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void GetScene(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            if (ulong.TryParse(uriparam["id"], out ulong id))
            {
                UMI3DUser user = GetUserFor(e.Request);
                UMI3DLogger.Log($"Get Scene {user?.Id()}", scope);
                UMI3DScene scene = UMI3DEnvironment.GetEntityInstance<UMI3DScene>(id);
                if (scene == null)
                {
                    Return404(e.Response, "UMI3DScene is missing !");
                }
                else if (user == null)
                {
                    Return404(e.Response, "UMI3DUser is missing !");
                }
                else
                {
                    e.Response.WriteContent(scene.ToGlTFNodeDto(user).ToBson());
                }
            }
            else
            {
                Return404(e.Response, "UMI3DScene is missing ! SceneId parsing failed");
            }
        }



        /// <summary>
        /// GET "/environment/player_count"
        /// get the player count
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="uriparam"></param>
        [HttpGet(UMI3DNetworkingKeys.playerCount, WebServiceMethodAttribute.Security.Public, WebServiceMethodAttribute.Type.Method)]
        public void GetPlayerCount(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DLogger.Log($"Get Player count", scope);
            e.Response.WriteContent(UMI3DCollaborationServer.Collaboration.GetPlayerCount().ToBson());
        }

        #endregion

        #region UploadFile

        /// <summary>
        /// POST "uploadFile/:param"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        /// <param name="uriparam"></param>
        [HttpPost(UMI3DNetworkingKeys.uploadFile, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void PostUploadFile(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DCollaborationUser user = GetUserFor(e.Request);
            UMI3DLogger.Log($"Post Upload File {user?.Id()}", scope);
            if (!uriparam.ContainsKey("param"))
            {
                UMI3DLogger.LogWarning("unvalide upload request, wrong networking key", scope);
                return;
            }
            string token = uriparam["param"];
            if (!UploadFileParameter.uploadTokens.ContainsKey(token))
            {
                UMI3DLogger.LogWarning("unvalide token (upload request)", scope);
                return;
            }
            if (!e.Request.Headers.Contains(UMI3DNetworkingKeys.contentHeader))
            {
                UMI3DLogger.LogWarning("unvalide header (upload request)", scope);
                return;
            }
            string fileName = e.Request.Headers[UMI3DNetworkingKeys.contentHeader];
            UploadFileParameter uploadParam = UploadFileParameter.uploadTokens[token];
            if (uploadParam.authorizedExtensions.Contains(System.IO.Path.GetExtension(fileName)) || uploadParam.authorizedExtensions.Count == 0)
            {
                UploadFileParameter.RemoveToken(token);
                uploadParam.onReceive.Invoke(token, fileName, ReadObject(e.Request));

            }
            else
            {
                UMI3DLogger.LogWarning("unauthorized extension : " + fileName + " (upload request)", scope);
                return;
            }
        }
        #endregion

        #region LocalData

        /// <summary>
        /// POST "LocalData/key/:param"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        /// <param name="uriparam"></param>
        [HttpPost(UMI3DNetworkingKeys.localData, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void PostPlayerLocalInfo(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DCollaborationUser user = GetUserFor(e.Request);
            UMI3DLogger.Log($"PostPlayerLocalInfo {user?.Id()}", scope);
            //UMI3DLogger.Log("Receive local data from : " + user,scope);
            if (receiveLocalInfoListener != null)
            {
                receiveLocalInfoListener.Invoke(uriparam["param"], user, ReadObject(e.Request));
            }
        }

        /// <summary>
        /// GET "LocalData/key/:param"
        /// get the cookies for client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="uriparam"></param>
        [HttpGet(UMI3DNetworkingKeys.localData, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void GetPlayerLocalInfo(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DCollaborationUser user = GetUserFor(e.Request);
            UMI3DLogger.Log($"GetPlayerLocalInfo {user?.Id()}", scope);
            UMI3DLogger.Log(user + " wants to get datas from : " + uriparam["param"], scope);
            if (sendLocalInfoListener != null)
            {
                sendLocalInfoListener.Invoke(uriparam["param"], user, e.Response);
            }
        }


        [Serializable]
        public class ReceiveLocalInfoEvent : UnityEvent<string, UMI3DUser, byte[]> // key, user, data. 
        {
        }
        public static ReceiveLocalInfoEvent receiveLocalInfoListener = new ReceiveLocalInfoEvent();

        [Serializable]
        public class SendLocalinfoEvent : UnityEvent<string, UMI3DUser, HttpListenerResponse> // key, user, response to send. 
        {
        }
        public static SendLocalinfoEvent sendLocalInfoListener = new SendLocalinfoEvent();



        #endregion
    }
}