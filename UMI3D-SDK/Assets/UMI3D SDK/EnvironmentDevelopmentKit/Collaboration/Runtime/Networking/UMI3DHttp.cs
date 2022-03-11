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
using System.Collections.Generic;
using System.Text;
using umi3d.common;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace umi3d.edk.collaboration
{
    public class UMI3DHttp
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.Networking;

        private readonly HttpServer httpsv;

        public HttpRoutingUtil rootMapGET = null;
        public HttpRoutingUtil rootMapPOST = null;
        private readonly UMI3DApi root;

        public UMI3DHttp(UMI3DApi api)
        {

            httpsv = new HttpServer(UMI3DCollaborationServer.Instance.httpPort);

            // Set the document root path.

            root = api;
            var rootMapGET = new HttpRoutingUtil(new List<object>() { root }, typeof(HttpGet));
            var rootMapPOST = new HttpRoutingUtil(new List<object>() { root }, typeof(HttpPost));

            httpsv.OnGet += (object sender, HttpRequestEventArgs e) =>
            {
                if (!rootMapGET.TryProccessRequest(sender, e))
                {
                    UMI3DLogger.Log("get environement", scope);
                    string path = e.Request.RawUrl;
                    WebSocketSharp.Net.HttpListenerResponse res = e.Response;
                    if (path == "/")
                        path += "index.html";

                    byte[] content = httpsv.GetFile(path);
                    if (content == null)
                    {
                        res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.NotFound;
                        return;
                    }
                    if (path.EndsWith(".html"))
                    {
                        res.ContentType = "text/html";
                        res.ContentEncoding = Encoding.UTF8;
                    }
                    else if (path.EndsWith(".js"))
                    {
                        res.ContentType = "application/javascript";
                        res.ContentEncoding = Encoding.UTF8;
                    }
                    res.WriteContent(content);
                }
            };

            httpsv.OnPost += (object sender, HttpRequestEventArgs e) =>
            {
                if (!rootMapPOST.TryProccessRequest(sender, e))
                {
                    UMI3DLogger.Log("post error " + e.Request.RawUrl, scope);
                }
            };

            httpsv.Start();
            if (httpsv.IsListening)
            {
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", httpsv.Port);
                foreach (string path in httpsv.WebSocketServices.Paths)
                    Console.WriteLine("- {0}", path);
            }
        }

        /// <summary>
        /// Stop the http server.
        /// </summary>
        public void Stop()
        {
            if (httpsv != null)
                httpsv.Stop();
        }

        public void Destroy()
        {
            root.Stop();
        }
    }
}