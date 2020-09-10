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

using MainThreadDispatcher;
using System;
using System.Collections;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;
using WebSocketSharp;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Websocket client.
    /// </summary>
    public class WebSocketClient
    {

        protected WebSocket ws;
        protected UMI3DCollaborationClientServer client;
        bool reconnect = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client">a reference to the server.</param>
        public WebSocketClient(UMI3DCollaborationClientServer client)
        {
            this.client = client;
        }

        /// <summary>
        /// State if the Client is connected.
        /// </summary>
        public bool Connected()
        {
            return ws != null && ws.IsConnected;
        }

        /// <summary>
        /// Setup the client.
        /// </summary>
        public void Init()
        {
            var connection = UMI3DCollaborationClientServer.Media.connection as WebsocketConnectionDto;
            var socketUrl = connection.websocketUrl;// UMI3DClientServer.Media.connection;
            
            socketUrl = socketUrl.Replace("http", "ws");
            ws = new WebSocket(socketUrl, UMI3DNetworkingKeys.websocketProtocol);

            ws.OnOpen += (sender, e) =>
            {
                Send(UMI3DCollaborationClientServer.Identity);
                UnityMainThreadDispatcher.Instance().Enqueue(onOpen());
            };

            ws.OnMessage += (sender, e) =>
            {
                if (e == null)
                    return;
                var res = UMI3DDto.FromBson(e.RawData);
                UnityMainThreadDispatcher.Instance().Enqueue(onMessage(res));

            };

            ws.OnError += (sender, e) =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(onError("websocket error "+ e.Message));
            };

            ws.OnClose += (sender, e) =>
            {
                
                UnityMainThreadDispatcher.Instance().Enqueue(onClosed("websocket close "+ e.Code + " " + e.Reason,e.Code));
            };

            if (UMI3DCollaborationClientServer.Media.Authentication != AuthenticationType.Anonymous) {

                UMI3DCollaborationClientServer.Instance.Identifier.GetPassword((password) => {
                    ws.SetCredentials(UMI3DCollaborationClientServer.Identity.login, password, false);
                    ws.Connect();
                });
            }
            else
                ws.Connect();

        }

        /// <summary>
        /// Wait and reconnect the websocket connection if not connected
        /// </summary>
        /// <returns></returns>
        IEnumerator Reconnect()
        {
            yield return new WaitForSeconds(2f);
            if (!Connected())
                ws?.Connect();
        }

        /// <summary>
        /// Call when a Connection is opened
        /// </summary>
        /// <returns></returns>
        protected IEnumerator onOpen()
        {
            client.onOpen();
            yield return null;
        }

        /// <summary>
        /// Call when a Message is received
        /// </summary>
        /// <param name="obj">message</param>
        /// <returns></returns>
        protected IEnumerator onMessage(object obj)
        {
            UMI3DCollaborationClientServer.OnMessage(obj);
            yield return null;
        }

        /// <summary>
        /// Call when an error is raised
        /// </summary>
        /// <param name="err">error</param>
        /// <returns></returns>
        protected IEnumerator onError(string err)
        {
            yield return null;
        }

        /// <summary>
        /// Call when the connection is closed
        /// </summary>
        /// <param name="reason">Closure reason</param>
        /// <param name="code">Error code</param>
        /// <returns></returns>
        protected IEnumerator onClosed(string reason,ushort code)
        {
            if (reconnect && client.shouldReconnectWebsocket(code))
                client.StartCoroutine(Reconnect());
            else
                UMI3DCollaborationClientServer.Logout(null,null);
            yield return null;
        }

        /// <summary>
        /// Send a UMI3DDto.
        /// </summary>
        /// <param name="obj"></param>
        public void Send(UMI3DDto obj,Action<bool> MessageSendCallback = null)
        {
            if (Connected() && obj != null)
            {
                if (MessageSendCallback == null) MessageSendCallback = (bool completed) => { };
                var data = obj.ToBson();
                ws.SendAsync(data, MessageSendCallback);
            }
            else
                Debug.LogError($"try sending message while not connected [{obj}]");
        }

        /// <summary>
        /// Terminate connection.
        /// </summary>
        public void Close()
        {
            reconnect = false;
            if (ws != null)
            {
                ws.Close();
            }
        }

        protected void OnDestroy()
        {
            Close();
        }
    }
}
