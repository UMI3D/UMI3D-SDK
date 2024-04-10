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
using System.IO;
using System.Text;
using umi3d.common;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace umi3d.edk.collaboration
{
    public abstract class UMI3DAbstractEnvironmentApi : IHttpApi
    {
        protected const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.Networking;


        #region utils
        protected void ReadDto(HttpListenerRequest request, Action<UMI3DDto> action)
        {
            byte[] bytes = default(byte[]);
            using (var memstream = new MemoryStream())
            {
                byte[] buffer = new byte[512];
                int bytesRead = default(int);
                while ((bytesRead = request.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                    memstream.Write(buffer, 0, bytesRead);
                bytes = memstream.ToArray();
            }
            action.Invoke(UMI3DDtoSerializer.FromBson(bytes));
        }

        protected byte[] ReadObject(HttpListenerRequest request)
        {
            byte[] bytes = default(byte[]);
            using (var memstream = new MemoryStream())
            {
                byte[] buffer = new byte[512];
                int bytesRead = default(int);
                while ((bytesRead = request.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                    memstream.Write(buffer, 0, bytesRead);
                bytes = memstream.ToArray();
                return bytes;
            }
        }

        protected void Return404(HttpListenerResponse response, string description = "This file does not exist :(")
        {
            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            response.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.NotFound;
            response.StatusDescription = description;
            response.WriteContent(Encoding.UTF8.GetBytes("404 :("));
            UMI3DLogger.LogError($"404 {description}", scope);
        }

        protected void ReturnNotImplemented(HttpListenerResponse response, string description = "This method isn't implemented now :(")
        {
            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            response.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.NotImplemented;
            response.StatusDescription = description;
            UMI3DLogger.LogError($"501 {description}", scope);
        }

        public virtual bool isAuthenticated(HttpListenerRequest request, bool allowOldToken, bool allowResourceOnly)
        {
            return UMI3DCollaborationServer.IsAuthenticated(request, allowOldToken, allowResourceOnly);
        }
        #endregion
    }
}