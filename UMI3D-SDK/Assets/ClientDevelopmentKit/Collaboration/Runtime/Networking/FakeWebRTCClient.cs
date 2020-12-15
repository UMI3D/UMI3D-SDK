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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;
using WebSocketSharp;

namespace umi3d.cdk.collaboration
{
    public class FakeWebRTCClient : AbstractWebsocketRtc
    {
        public class WSContent : AbstractWebsocket
        {
            WebSocket ws;

            public WSContent(WebSocket ws)
            {
                this.ws = ws;
            }

            public override void Send(byte[] content)
            {
                ws.Send(content);
            }
        }

        UMI3DCollaborationClientServer client;
        protected WebSocket wsReliable;
        protected WebSocket wsUnReliable;
        WSContent wsContentReliable;
        WSContent wsContentUnReliable;

        bool reconnect = true;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client">a reference to the server.</param>
        public FakeWebRTCClient(UMI3DCollaborationClientServer client)
        {
            this.client = client;
            client.StartCoroutine(Init());
        }

        #region ws
        /// <summary>
        /// Setup the client.
        /// </summary>
        /// 
        public IEnumerator Init()
        {
            var connection = UMI3DCollaborationClientServer.Media?.connection as WebsocketConnectionDto;

            while (connection == null)
            {
                yield return new WaitForFixedUpdate();
                connection = UMI3DCollaborationClientServer.Media?.connection as WebsocketConnectionDto;
            }
            var id = UMI3DCollaborationClientServer.Identity?.userId;
            while (id == null)
            {
                yield return new WaitForFixedUpdate();
                id = UMI3DCollaborationClientServer.Identity?.userId;
            }
            var ReliableUrl = connection.rtcReliableUrl;
            var UnreliableUrl = connection.rtcUnreliableUrl;
            ReliableUrl = ReliableUrl.Replace("http", "ws");
            wsReliable = new WebSocket(ReliableUrl, UMI3DNetworkingKeys.websocketProtocol);
            wsUnReliable = new WebSocket(UnreliableUrl, UMI3DNetworkingKeys.websocketProtocol);
            _Init(wsReliable, true);
            _Init(wsUnReliable, false);

            wsContentReliable = new WSContent(wsReliable);
            wsContentUnReliable = new WSContent(wsUnReliable);
        }

        void _Init(WebSocket ws, bool reliable)
        {
            ws.OnOpen += (sender, e) =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(onOpen(ws));
            };

            ws.OnMessage += (sender, e) =>
            {
                if (e == null)
                    return;
                var res = UMI3DDto.FromBson(e.RawData);
                UnityMainThreadDispatcher.Instance().Enqueue(onMessage(res, reliable));

            };

            ws.OnError += (sender, e) =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(onError("websocket error " + e.Message, reliable));
            };

            ws.OnClose += (sender, e) =>
            {

                UnityMainThreadDispatcher.Instance().Enqueue(onClosed("websocket close " + e.Code + " " + e.Reason, e.Code, reliable));
            };

            //ws.SetCredentials(UMI3DCollaborationClientServer.Identity.userId, "pwd", false);
            ws.Connect();
        }

        #endregion


        /// <summary>
        /// Call when a connection is open
        /// </summary>
        /// <param name="obj">message</param>
        /// <returns></returns>
        protected IEnumerator onOpen(WebSocket ws)
        {
            while (!UMI3DCollaborationClientServer.Connected())
                yield return new WaitForFixedUpdate();
            ws?.Send(UMI3DCollaborationClientServer.Identity.ToBson());
            yield return null;
        }

        /// <summary>
        /// Call when a Message is received
        /// </summary>
        /// <param name="obj">message</param>
        /// <returns></returns>
        protected IEnumerator onMessage(UMI3DDto obj, bool reliable)
        {
            var fake = obj as FakeWebrtcMessageDto;

            var user = UMI3DCollaborationEnvironmentLoader.Instance?.UserList?.FirstOrDefault(u => u.id == fake.sourceId);
            if (fake.dataType == DataType.Audio)
                AudioManager.Instance.Read(user, fake.content, null);
            else
            {
                var dto = UMI3DDto.FromBson(fake.content);
                if (dto is FakeWebrtcMessageDto fake2)
                    UnityMainThreadDispatcher.Instance().Enqueue(onMessage(fake2, reliable));
                else
                    UMI3DCollaborationClientServer.OnRtcMessage(user, dto, null);
            }
            yield return null;
        }

        /// <summary>
        /// Call when an error is raised
        /// </summary>
        /// <param name="err">error</param>
        /// <returns></returns>
        protected IEnumerator onError(string err, bool reliable)
        {
            yield return null;
        }

        /// <summary>
        /// Call when the connection is closed
        /// </summary>
        /// <param name="reason">Closure reason</param>
        /// <param name="code">Error code</param>
        /// <returns></returns>
        protected IEnumerator onClosed(string reason, ushort code, bool reliable)
        {
            if (reconnect && client.shouldReconnectWebsocket(code))
                client.StartCoroutine(Reconnect(reliable));
            Debug.Log("closed");
            yield return null;
        }


        /// <summary>
        /// State if the Client is connected.
        /// </summary>
        public bool Connected(bool reliable)
        {
            var ws = reliable ? wsReliable : wsUnReliable;
            return ws != null && ws.IsConnected;
        }

        /// <summary>
        /// Wait and reconnect the websocket connection if not connected
        /// </summary>
        /// <returns></returns>
        IEnumerator Reconnect(bool reliable)
        {
            yield return new WaitForSeconds(2f);
            var ws = reliable ? wsReliable : wsUnReliable;
            if (!Connected(reliable))
                ws?.Connect();
        }


        public override void SetUpd(WebRTCDataChannel channels)
        {
            var ws = channels.reliable ? wsContentReliable : wsContentUnReliable;
            if (ws == null) Debug.LogWarning($"Websocket {channels.reliable} not opened yet. Workflow might need a review.");
            Debug.Log(channels.Label);
            channels.socket = ws;
        }

        public override void Send(byte[] content, DataType type, bool reliable, List<IWebRTCconnection> connection)
        {
            var dto = new FakeWebrtcMessageDto()
            {
                sourceId = UMI3DCollaborationClientServer.Identity.userId,
                content = content,
                targetId = connection.Select(c => c.targetId).ToList(),
                dataType = type,
                reliable = reliable
            };
            var ws = reliable ? wsReliable : wsUnReliable;
            ws?.Send(dto.ToBson());
        }
    }
}