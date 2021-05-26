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

using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.edk.userCapture;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace umi3d.edk.collaboration
{
    public class UMI3DApi
    {
        #region users

        /// <summary>
        /// GET "/me"
        /// Get the user's information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpGet(UMI3DNetworkingKeys.identity, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void GetIdentity(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            var user = UMI3DCollaborationServer.GetUserFor(e.Request);
            UserConnectionDto identity = new UserConnectionDto(user.ToUserDto());
            identity.parameters = UMI3DCollaborationServer.Instance.Identifier.GetParameterDtosFor(user);
            //UMI3DEnvironment.Instance.libraries== null || UMI3DEnvironment.Instance.libraries.Count == 0
            identity.librariesUpdated = UMI3DCollaborationServer.Instance.Identifier.getLibrariesUpdateSatus(user);
            e.Response.WriteContent(identity.ToBson());
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
            UMI3DCollaborationUser user = UMI3DCollaborationServer.GetUserFor(e.Request);
            StatusDto dto = ReadDto(e.Request) as StatusDto;
            UnityMainThreadDispatcher.Instance().Enqueue(_updateStatus(user, dto));
        }

        IEnumerator _updateStatus(UMI3DCollaborationUser user, StatusDto dto)
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
        [HttpPost(UMI3DNetworkingKeys.identity_update, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void UpdateIdentity(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DCollaborationUser user = UMI3DCollaborationServer.GetUserFor(e.Request);
            UserConnectionDto dto = ReadDto(e.Request) as UserConnectionDto;
            UnityMainThreadDispatcher.Instance().Enqueue(_updateIdentity(user, dto));
        }

        IEnumerator _updateIdentity(UMI3DCollaborationUser user, UserConnectionDto dto)
        {
            user.SetStatus(UMI3DCollaborationServer.Instance.Identifier.UpdateIdentity(user, dto));
            user.forgeServer.SendSignalingMessage(user.networkPlayer, user.ToStatusDto());
            yield break;
        }

        /// <summary>
        /// POST "/logout"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpPost(UMI3DNetworkingKeys.logout, WebServiceMethodAttribute.Security.Private, WebServiceMethodAttribute.Type.Method)]
        public void Logout(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            UMI3DCollaborationUser user = UMI3DCollaborationServer.GetUserFor(e.Request);
            if (user != null)
            {
                UMI3DCollaborationServer.Logout(user);
            }
        }

        #endregion

        #region media
        /// <summary>
        /// Handles the GET Media Request received by the HTTP Server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Represents the event data for the HTTP request event</param>
        [HttpGet(UMI3DNetworkingKeys.media, WebServiceMethodAttribute.Security.Public, WebServiceMethodAttribute.Type.Method)]
        public void MediaRequest(object sender, HttpRequestEventArgs e, Dictionary<string, string> uriparam)
        {
            var res = e.Response;
            byte[] message = null;
            if (UMI3DEnvironment.Exists)
            {
                message = UMI3DEnvironment.Instance.ToDto().ToBson();
            }
            res.WriteContent(message);
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
            UMI3DUser user = UMI3DCollaborationServer.GetUserFor(e.Request);
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
            file = common.Path.Combine(
                UMI3DServer.publicRepository, file);
            file = System.Uri.UnescapeDataString(file);
            //Validate url.
            var res = e.Response;
            if (UMI3DServer.IsInPublicRepository(file))
            {
                GetFile(file, res);
            }
            else
            {
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
            file = common.Path.Combine(UMI3DServer.privateRepository, file);
            file = System.Uri.UnescapeDataString(file);
            //Validate url.
            HttpListenerResponse res = e.Response;
            if (UMI3DServer.IsInPrivateRepository(file) || UMI3DServer.IsInPublicRepository(file))
            {
                GetFile(file, res);
            }
            else
            {
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
            string directory = common.Path.Combine(UMI3DServer.dataRepository, rawDirectory);
            //Validate url.
            HttpListenerResponse res = e.Response;
            if (UMI3DServer.IsInDataRepository(directory))
            {
                if (Directory.Exists(directory))
                {
                    FileListDto dto = new FileListDto()
                    {
                        files = GetDir(directory).Select(f => System.Uri.EscapeUriString(f)).ToList(),
                        baseUrl = System.Uri.EscapeUriString(common.Path.Combine(UMI3DServer.GetHttpUrl(), UMI3DNetworkingKeys.files, rawDirectory))
                    };

                    res.WriteContent(dto.ToBson());
                }
                else
                    Return404(res, "This directory doesn't exists!");
            }
            else
            {
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
            directory = common.Path.Combine(UMI3DServer.dataRepository, directory);
            directory = System.Uri.UnescapeDataString(directory);

            //Validate url.
            HttpListenerResponse res = e.Response;
            if (UMI3DServer.IsInDataRepository(directory))
            {
                if (Directory.Exists(directory))
                {
                    ReturnNotImplemented(res);
                }
                else
                    Return404(res, "This directory doesn't exists!");
            }
            else
            {
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
            List<string> files = new List<string>();
            IEnumerable<string> localFiles = Directory.GetFiles(directory).Select(full => System.IO.Path.GetFileName(full));
            IEnumerable<string> uris = localFiles.Select(f => common.Path.Combine(localpath, f));
            files.AddRange(uris);
            foreach (string susdir in Directory.GetDirectories(directory))
            {
                files.AddRange(GetDir(susdir, common.Path.Combine(localpath, System.IO.Path.GetFileName(System.IO.Path.GetFileName(susdir)))));
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
            UMI3DUser user = UMI3DCollaborationServer.GetUserFor(e.Request);
            UMI3DEnvironment environment = UMI3DEnvironment.Instance;
            if (environment == null)
            {
                Return404(e.Response, "UMI3DEnvironment is missing !");
            }
            else if (user == null)
            {
                Return404(e.Response, "UMI3DUser is missing !");
            }
            else
            {
                GlTFEnvironmentDto result = null;
                bool finished = false;
                UnityMainThreadDispatcher.Instance().Enqueue(
                    _GetEnvironment(
                        environment, user,
                        (res) => { result = res; finished = true; },
                        () => { finished = true; }
                    ));
                while (!finished) System.Threading.Thread.Sleep(1);
                e.Response.WriteContent(result.ToBson());
            }
        }

        IEnumerator _GetEnvironment(UMI3DEnvironment environment, UMI3DUser user, Action<GlTFEnvironmentDto> callback, Action error)
        {
            callback.Invoke(environment.ToDto(user));
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
            UMI3DCollaborationUser user = UMI3DCollaborationServer.GetUserFor(e.Request);
            JoinDto dto = ReadDto(e.Request) as JoinDto;
            UMI3DEmbodimentManager.Instance.JoinDtoReception(user.Id(), dto.userSize, dto.trackedBonetypes); 
            e.Response.WriteContent((UMI3DEnvironment.ToEnterDto(user)).ToBson());
            UMI3DCollaborationServer.NotifyUserJoin(user);
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
            UMI3DCollaborationUser user = UMI3DCollaborationServer.GetUserFor(e.Request);
            EntityRequestDto dto = ReadDto(e.Request) as EntityRequestDto;
            var entity = UMI3DEnvironment.GetEntity<UMI3DLoadableEntity>(dto.entityId);
            if (entity != null)
            {
                var load = new LoadEntityDto()
                {
                    entity = entity.ToEntityDto(user),
                };
                e.Response.WriteContent(load.ToBson());
            }
            else
                Return404(e.Response, "Unvalid Id");
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
            ulong id;
            if (ulong.TryParse(uriparam["id"], out id))
            {
                UMI3DUser user = UMI3DCollaborationServer.GetUserFor(e.Request);
                UMI3DScene scene = UMI3DEnvironment.GetEntity<UMI3DScene>(id);
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
            e.Response.WriteContent(UMI3DCollaborationServer.Collaboration.GetPlayerCount().ToBson());
        }

        #endregion

        #region utils
        UMI3DDto ReadDto(HttpListenerRequest request)
        {
            var bytes = default(byte[]);
            using (var memstream = new MemoryStream())
            {
                var buffer = new byte[512];
                var bytesRead = default(int);
                while ((bytesRead = request.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                    memstream.Write(buffer, 0, bytesRead);
                bytes = memstream.ToArray();
                return UMI3DDto.FromBson(bytes);
            }
        }

        void Return404(HttpListenerResponse response, string description = "This file does not exist :(")
        {
            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            response.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.NotFound;
            response.StatusDescription = description;
            response.WriteContent(Encoding.UTF8.GetBytes("404 :("));
        }

        void ReturnNotImplemented(HttpListenerResponse response, string description = "This method isn't implemented now :(")
        {
            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            response.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.NotImplemented;
            response.StatusDescription = description;
        }
        #endregion


    }
}