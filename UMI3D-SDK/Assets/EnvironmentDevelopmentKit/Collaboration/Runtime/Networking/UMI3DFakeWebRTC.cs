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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.collaboration;
using WebSocketSharp.Server;

namespace umi3d.edk.collaboration
{
    public class UMI3DFakeWebRTC
    {
        public HttpServer wsReliable;
        public HttpServer wsUnreliable;

        Action<string, DataType, bool, List<string>, byte[]> messageAction;
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

        void OnIdentify(UMI3DFakeRTCConnection connection, IdentityDto id)
        {
            var peer = client.peers.Select(p => p.Value).FirstOrDefault((p) => p.name == id.userId) as UMI3DFakeRTCClient;
            if (peer != null)
            {
                if (connection.reliable)
                    peer.Reliable = connection;
                else
                    peer.Unreliable = connection;
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
    }
}