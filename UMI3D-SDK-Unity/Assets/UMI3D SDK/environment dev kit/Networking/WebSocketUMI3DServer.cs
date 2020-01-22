/*
Copyright 2019 Gfi Informatique

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
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Reflection;
using System.IO;
using umi3d.common;
using MainThreadDispatcher;

namespace umi3d.edk
{
    public class WebSocketUMI3DServer : MonoBehaviour, IUMI3DServer
    {
        HttpServer httpsv;

        private RootMap rootMapGET = null;
        private RootMap rootMapPOST = null;

        public string ip = "localhost";

        public string socketPath = "/socket";

        public bool UseRandomPort;
        public int port;

        public string resourcesPath = "/../Public/";

        public string GetEnvironmentUrl()
        {
            return "http://" + ip + ":" + port;
        }

        [Obsolete("ToDto should be used instead.")]
        public string GetProtocol()
        {
            return "websocket";
        }

        [Obsolete("ToDto should be used instead.")]
        public string GetIP()
        {
            return ip;
        }

        [Obsolete("ToDto should be used instead.")]
        public int GetPort()
        {
            return port;
        }

        [Obsolete("ToDto should be used instead.")]
        public string GetSocketPath()
        {
            return socketPath;
        }

        public UMI3DDto ToDto()
        {
            var dto = new WebsocketConnectionDto();
            dto.IP = ip;
            dto.Port = port;
            dto.Postfix = socketPath;
            dto.Url = "ws://" + ip + ":" + port + socketPath;
            return dto;
        }

        public void Init()
        {
            ip = GetLocalIPAddress();
            if (UseRandomPort)
                port = FreeTcpPort();
            else
                port = FreeTcpPort(port);
            httpsv = new HttpServer(port);
            // Set the document root path.
            httpsv.RootPath = Application.dataPath + resourcesPath;

            rootMapGET = CreateRootMap(typeof(Get));
            rootMapPOST = CreateRootMap(typeof(Post));

            httpsv.OnGet += (object sender, HttpRequestEventArgs e) => DispatchRequest(rootMapGET, sender, e);
            httpsv.OnPost += (object sender, HttpRequestEventArgs e) => DispatchRequest(rootMapPOST, sender, e);

            // Add the WebSocket services.
            httpsv.AddWebSocketService<WebSocketCVEConnection>(
                socketPath,
                () =>
                new WebSocketCVEConnection()
                {
                    IgnoreExtensions = true,
                    Protocol = "echo-protocol"
                }
            );

            httpsv.Start();
            if (httpsv.IsListening)
            {
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", httpsv.Port);
                foreach (var path in httpsv.WebSocketServices.Paths)
                    Console.WriteLine("- {0}", path);
            }
        }


        #region GET
        /// <summary>
        /// GET/load?user=[string]&pid=[string]
        /// HTTP Get root to load the children of a UMI3D Object for a given user.
        /// user: the user identifier provided by the UMI3D environment
        /// pid: the UMI3D object identifier (no value -> load the scene root nodes)
        /// </summary>
        /// <param name="sender">The origin of the request.</param>
        /// <param name="e">The request's arguments.</param>
        [Get("/load")]
        public void GetChildren(object sender, HttpRequestEventArgs e)
        {
            var req = e.Request;
            var res = e.Response;
            var user = req.QueryString.Get("user");
            var pid = req.QueryString.Get("pid");
            string message = null;
            bool finished = false;
            //need to be done in the main thread due to Transform access
            UnityMainThreadDispatcher.Instance().Enqueue(GetChildren(
                   user, pid,
                   //success callback
                   (LoadDto response) => {
                       message = DtoUtility.Serialize(response);
                       finished = true;
                   },
                   //error callback
                   () => {
                       res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.NotFound;
                       finished = true;
                   }));

            //wait the main thread answer
            while (!finished)
                System.Threading.Thread.Sleep(1);
            //send the result to the UMI3D Browser
            res.WriteContent(Encoding.ASCII.GetBytes(message));
        }

        IEnumerator GetChildren(string user, string pid, Action<LoadDto> callback, Action error)
        {
            var usr = UMI3D.UserManager.Get(user);
            if (usr == null)
            {
                error.Invoke();
                yield return null;
            }
            else
            {
                callback.Invoke(usr.LoadSubObjects(pid));
            }

        }

        [Get("/update")]
        public void GetUpdates(object sender, HttpRequestEventArgs e)
        {
            var req = e.Request;
            var res = e.Response;
            var user = req.QueryString.Get("user");
            string message = null;
            bool finished = false;
            UnityMainThreadDispatcher.Instance().Enqueue(GetUpdates(
                   user,
                   (UpdateDto response) => {
                       message = DtoUtility.Serialize(response);
                       finished = true;
                   },
                   () => {
                       res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.NotFound;
                       finished = true;
                   }));
            while (!finished)
            {
                System.Threading.Thread.Sleep(1);
            }
            res.WriteContent(Encoding.ASCII.GetBytes(message));
        }

        IEnumerator GetUpdates(string user, Action<UpdateDto> callback, Action error)
        {
            var usr = UMI3D.UserManager.Get(user);
            if (usr == null)
            {
                error.Invoke();
                yield return null;
            }
            else
            {
                callback.Invoke(usr.GetUpdates());
            }

        }

        [Get("/logout")]
        public void Logout(object sender, HttpRequestEventArgs e)
        {
            var req = e.Request;
            var user = req.QueryString.Get("user");
            UnityMainThreadDispatcher.Instance().Enqueue(UMI3D.UserManager.OnConnectionClose(user));
        }

        [Get("/")]
        public void GetFile(object sender, HttpRequestEventArgs e)
        {
            var path = e.Request.RawUrl;
            var res = e.Response;
            if (path == "/")
                path += "index.html";

            var content = httpsv.GetFile(path);
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


        [Get("/media")]
        public void GetMedia(object sender, HttpRequestEventArgs e)
        {
            var req = e.Request;
            var res = e.Response;
            string message = null;
            bool finished = false;
            UnityMainThreadDispatcher.Instance().Enqueue(GetMedia(
                   (MediaDto response) => {
                       message = DtoUtility.Serialize(response);
                       finished = true;
                   },
                   () => {
                       res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.NotFound;
                       finished = true;
                   }));
            while (!finished)
            {
                System.Threading.Thread.Sleep(1);
            }
            res.WriteContent(Encoding.ASCII.GetBytes(message));
        }

        IEnumerator GetMedia(Action<MediaDto> callback, Action error)
        {
            var scene = UMI3D.Scene;
            if (scene == null)
            {
                error.Invoke();
                yield return null;
            }
            else
            {
                callback.Invoke(scene.ToDto());
            }
        }
        #endregion

        #region POST
        [Post("/interact")]
        public void Interact(object sender, HttpRequestEventArgs e)
        {
            string jsonString = null;
            var req = e.Request;
            var user = req.QueryString.Get("user");
            var usr = UMI3D.UserManager.Get(user);
            if (usr != null)
            {
                using (StreamReader inputStream = new StreamReader(e.Request.InputStream))
                {
                    jsonString = inputStream.ReadToEnd();
                    var request = DtoUtility.Deserialize(jsonString);
                    if (request != null)
                        UnityMainThreadDispatcher.Instance().Enqueue(() => usr.onMessage(request));
                }
            }
        }

        [Post("/login")]
        public void Login(object sender, HttpRequestEventArgs e)
        {
            string jsonString = null;
            var req = e.Request;
            var res = e.Response;
            using (StreamReader inputStream = new StreamReader(e.Request.InputStream))
            {
                jsonString = inputStream.ReadToEnd();
                var request = DtoUtility.Deserialize(jsonString);
                string message = null;
                bool finished = false;
                UnityMainThreadDispatcher.Instance().Enqueue(CreateUser(
                    request as ConnectionRequestDto,
                   (EnterDto response) => {
                       message = DtoUtility.Serialize(response);
                       finished = true;
                   },
                   () => {
                       res.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.NotFound;
                       finished = true;
                   }));
                while (!finished)
                {
                    System.Threading.Thread.Sleep(1);
                }
                res.WriteContent(Encoding.ASCII.GetBytes(message));
            }
        }



        IEnumerator CreateUser(ConnectionRequestDto connection, Action<EnterDto> callback, Action error)
        {
            if (connection == null)
            {
                error.Invoke();
                yield return null;
            }
            else
            {
                callback.Invoke(UMI3D.UserManager.Login(connection));
            }

        }
        #endregion

        #region automated rooting
        public abstract class RootAttribute : System.Attribute
        {
            public string path = null;

            public RootAttribute(string name)
            {
                this.path = name;
            }
        }

        [System.AttributeUsage(System.AttributeTargets.Method)]
        public class Get : RootAttribute
        {
            public Get(string name) : base(name) { }
        }

        [System.AttributeUsage(System.AttributeTargets.Method)]
        public class Post : RootAttribute
        {
            public Post(string name) : base(name) { }
        }

        public class RootMap
        {
            public MethodInfo defaultRoot;
            public Dictionary<string, MethodInfo> others = new Dictionary<string, MethodInfo>();

            public void Clear()
            {
                defaultRoot = null;
                others.Clear();
            }
        }

        private RootMap CreateRootMap(Type attributeType)
        {
            RootMap map = new RootMap();
            map.Clear();
            MethodInfo[] methods = this.GetType().GetMethods();
            foreach (var method in methods)
            {
                var attrArray = method.GetCustomAttributes(attributeType, false);
                RootAttribute attribute = (attrArray == null || attrArray.Length == 0) ? null : (RootAttribute)attrArray[0];
                if (attribute != null)
                {
                    if (attribute.path == null || attribute.path == "/")
                        map.defaultRoot = method;
                    else
                        map.others.Add(attribute.path, method);
                }
            }
            return map;
        }

        void DispatchRequest(RootMap map, object sender, HttpRequestEventArgs e)
        {
            var path = e.Request.RawUrl;
            foreach (var entry in map.others)
            {
                if (path.StartsWith(entry.Key))
                {
                    entry.Value.Invoke(this, new object[] { sender, e });
                    return;
                }
            }
            if (map.defaultRoot != null)
                map.defaultRoot.Invoke(this, new object[] { sender, e });
            else
                e.Response.StatusCode = (int)WebSocketSharp.Net.HttpStatusCode.NotFound;
        }
        #endregion



        public static string GetLocalIPAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ip.ToString().EndsWith(".1"))
                {
                    return ip.ToString();
                }
            }
            //if offline. 
            Debug.LogWarning("No public IP found. This computer seems to be offline.");
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        void OnApplicationQuit()
        {
            if (httpsv != null)
                httpsv.Stop();
        }

        static int FreeTcpPort(int port = 0)
        {
            try
            {
                TcpListener l = new TcpListener(IPAddress.Loopback, port);
                l.Start();
                port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                return port;
            }
            catch (Exception)
            {
                TcpListener l = new TcpListener(IPAddress.Loopback, 0);
                l.Start();
                port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                return port;
            }
        }
    }
}
