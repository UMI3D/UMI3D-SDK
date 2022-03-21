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
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace umi3d.edk.collaboration
{
    public class UMI3DHttp : inetum.unityUtils.Singleton<UMI3DHttp>
    {

        private readonly HttpServer httpsv;

        public HttpRoutingUtil rootMapGET;
        public HttpRoutingUtil rootMapPOST;

        public void AddRoot(IHttpApi Object)
        {
            rootMapGET.AddRoot(Object);
            rootMapPOST.AddRoot(Object);
        }

        public UMI3DHttp() : base() { }

        public UMI3DHttp(ushort port) : this()
        {

            httpsv = new HttpServer(port);

            rootMapGET = new HttpRoutingUtil(typeof(HttpGet));
            rootMapPOST = new HttpRoutingUtil(typeof(HttpPost));

            httpsv.OnGet += (object sender, HttpRequestEventArgs e) =>
            {
                if (!rootMapGET.TryProccessRequest(sender, e))
                {
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
            Stop();
        }
    }
}