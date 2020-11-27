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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;
using WebSocketSharp.Server;

namespace umi3d.edk.collaboration
{
    public class UMI3DFakeWebRTC : AbstractWebsocketRtc
    {
        public HttpServer wsReliable;
        public HttpServer wsUnreliable;

        Action<string, DataType, bool, List<string>, byte[]> messageAction;
        Dictionary<string, (WSContent, WSContent)> websockets = new Dictionary<string, (WSContent, WSContent)>();
        UMI3DWebRTC client;

        public UMI3DFakeWebRTC(UMI3DWebRTC client, Action<string, DataType, bool, List<string>, byte[]> messageAction)
        {
            this.client = client;
            this.messageAction = messageAction;
            wsReliable = new HttpServer(UMI3DCollaborationServer.Instance.fakeRTCReliablePort);
            wsUnreliable = new HttpServer(UMI3DCollaborationServer.Instance.fakeRTCUnreliablePort);
            _Init(true, wsReliable);
            _Init(false, wsUnreliable);
        }

        void _Init(bool reliable, HttpServer ws)
        {
            // Add the WebSocket services
            ws.AddWebSocketService<UMI3DFakeRTCConnection>(
                UMI3DNetworkingKeys.websocket,
                () =>
                    new UMI3DFakeRTCConnection(reliable, OnMessage, OnClose, OnIdentify)
                    {
                        IgnoreExtensions = true,
                        Protocol = UMI3DNetworkingKeys.websocketProtocol,
                    }
            );
            ws.AuthenticationSchemes = AuthenticationType.Anonymous.Convert();//UMI3DCollaborationServer.GetAuthentication().Convert();
            ws.Realm = "UMI3D";
            ws.UserCredentialsFinder = id =>
            {
                var name = id.Name;
                return new WebSocketSharp.Net.NetworkCredential(id.Name, "pwd");
            };

            ws.Start();
            if (ws.IsListening)
            {
                Console.WriteLine("Listening on port {0}, and providing WebSocket services:", ws.Port);
                foreach (var path in ws.WebSocketServices.Paths)
                    Console.WriteLine("- {0}", path);
            }
        }

        void OnMessage(UMI3DFakeRTCConnection connection, UMI3DDto dto)
        {
            var fake = dto as FakeWebrtcMessageDto;
            if (connection._id == null) connection._id = fake.sourceId;
            messageAction?.Invoke(fake.sourceId, fake.dataType, fake.reliable, fake.targetId, fake.content);
        }

        void OnClose(UMI3DFakeRTCConnection connection) { }

        public class WSContent : AbstractWebsocket
        {

            UMI3DFakeRTCConnection connection;

            public WSContent(UMI3DFakeRTCConnection connection)
            {
                this.connection = connection;
            }

            public override void Send(byte[] content)
            {
                connection.SendData(content);
            }
        }


        void OnIdentify(UMI3DFakeRTCConnection connection, IdentityDto id)
        {
            Debug.Log($"open {id.login}");
            WSContent ws = new WSContent(connection);
            (WSContent, WSContent) t;
            var uid = id.userId;
            if (websockets.ContainsKey(uid)) t = websockets[uid];
            else t = (null, null);

            if (connection.reliable)
                t = (ws, t.Item2);
            else
                t = (t.Item1, ws);
            websockets[id.userId] = t;

            if (client.peers.Select(p => p.Value).FirstOrDefault((p) => p.targetId == id.userId) is WebRTCconnection peer)
            {
                foreach(var c in peer.channels.Cast<WebRTCDataChannel>().Where(c=>c.reliable = connection.reliable))
                {
                    c.socket =  ws;
                }
            }
        }

        /// <summary>
        /// Stop the websocket
        /// </summary>
        public void Stop()
        {
            if (wsReliable != null)
                wsReliable.Stop();
            if (wsUnreliable != null)
                wsUnreliable.Stop();
        }

        public override void SetUpd(WebRTCDataChannel channels)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(_setup(channels));
        }

        IEnumerator _setup(WebRTCDataChannel channels)
        {
            yield return new WaitForFixedUpdate();
            if (websockets.ContainsKey(channels.id))
                channels.socket = channels.reliable ? websockets[channels.id].Item1 : websockets[channels.id].Item2;
        }

        public override void Send(byte[] content, DataType type, bool reliable, List<IWebRTCconnection> connection)
        {
            throw new Exception("Should not end up here");
        }
    }
}