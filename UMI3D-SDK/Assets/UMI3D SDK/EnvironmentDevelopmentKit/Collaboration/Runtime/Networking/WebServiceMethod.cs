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
using System.Reflection;
using System.Text;
using umi3d.common;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace umi3d.edk.collaboration
{
    public class WebServiceMethod
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.Networking;

        public MethodInfo methodInfo;
        public object source;
        public WebServiceMethodAttribute attribute;

        public WebServiceMethod(object source, MethodInfo methodInfo, WebServiceMethodAttribute attribute)
        {
            this.methodInfo = methodInfo;
            this.source = source;
            this.attribute = attribute;
        }

        public void ProcessRequest(string uri, object sender, HttpRequestEventArgs e)
        {
            try
            {
                if (attribute.security == WebServiceMethodAttribute.Security.Private)
                {
                    if (UMI3DCollaborationServer.IsAuthenticated(e.Request))
                    {
                        methodInfo.Invoke(source, new object[] { sender, e, attribute.GetParametersFrom(uri) });
                    }
                    else
                    {
                        HttpListenerResponse res = e.Response;
                        res.ContentType = "text/html";
                        res.ContentEncoding = Encoding.UTF8;
                        res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.Unauthorized;
                        res.StatusDescription = "You should be authenticated to access files here :(";
                        res.WriteContent(Encoding.UTF8.GetBytes("401 You should be authenticated to access files here :("));

                    }
                }
                else
                {
                    methodInfo.Invoke(source, new object[] { sender, e, attribute.GetParametersFrom(uri) });
                }
            }
            catch (Exception ext)
            {
                UMI3DLogger.LogError(ext, scope);

                HttpListenerResponse res = e.Response;
                res.ContentType = "text/html";
                res.ContentEncoding = Encoding.UTF8;
                res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.InternalServerError;
                res.StatusDescription = "Something went bad";
                res.WriteContent(Encoding.UTF8.GetBytes("500 This wasn't handled as expected"));
            }
        }

    }
}