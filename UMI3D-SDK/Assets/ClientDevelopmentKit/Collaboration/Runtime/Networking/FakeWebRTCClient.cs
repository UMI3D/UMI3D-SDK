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
using System.Collections.Generic;
using System.Linq;

namespace umi3d.cdk.collaboration
{
    public class FakeWebRTCClient : IWebRTCClient
    {
        UMI3DCollaborationClientServer client;
        protected WebSocket wsReliable;
        protected WebSocket wsUnReliable;
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
            _Init(wsReliable,true);
            _Init(wsUnReliable,false);
        }

        void _Init(WebSocket ws, bool reliable)
        {
            ws.OnOpen += (sender, e) =>
            {
                ws?.Send(UMI3DCollaborationClientServer.Identity.ToBson());
                //UnityMainThreadDispatcher.Instance().Enqueue(onOpen());
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
        /// Call when a Message is received
        /// </summary>
        /// <param name="obj">message</param>
        /// <returns></returns>
        protected IEnumerator onMessage(UMI3DDto obj,bool reliable)
        {
            var fake = obj as FakeWebrtcMessageDto;
            
            var user = UMI3DCollaborationEnvironmentLoader.Instance?.UserList?.FirstOrDefault(u => u.id == fake.sourceId);
            if (fake.dataType == DataType.Audio)
                AudioManager.Instance.Read(user, fake.content, null);
            else
                UMI3DCollaborationClientServer.OnRtcMessage(user, fake.content, null);
            yield return null;
        }

        /// <summary>
        /// Call when an error is raised
        /// </summary>
        /// <param name="err">error</param>
        /// <returns></returns>
        protected IEnumerator onError(string err,bool reliable)
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

        /// <summary>
        /// Call when an RtcMessage is received.
        /// </summary>
        /// <param name="id">peer id.</param>
        /// <param name="bytes">message as byte[].</param>
        /// <param name="channel">Datachannel from which this message was received.</param>
        protected void OnRtcMessage(string id, byte[] bytes, DataChannel channel)
        {
            var user = UMI3DCollaborationEnvironmentLoader.Instance?.UserList?.FirstOrDefault(u => u.id == id);
            if (channel.type == DataType.Audio)
                AudioManager.Instance.Read(user, bytes, channel);
            else
                UMI3DCollaborationClientServer.OnRtcMessage(user, bytes, channel);
        }


        public List<DataChannel> Add(DataChannel dataBase)
        {
            return null;
        }

        public DataChannel Add(string uid, DataChannel dataBase)
        {
            return null;
        }

        public void Clear()
        {
            reconnect = false;
            if (wsReliable != null)
            {
                wsReliable.Close();
            }
            if (wsUnReliable != null)
            {
                wsUnReliable.Close();
            }
        }

        public bool Exist(bool reliable, DataType dataType, out List<DataChannel> dataChannels, string peerId = null)
        {
            dataChannels = null;
            return false;
        }

        public bool ExistServer(bool reliable, DataType dataType, out List<DataChannel> dataChannels)
        {
            dataChannels = null;
            return false;
        }

        public void HandleMessage(RTCDto dto)
        {
            Debug.Log(dto);
        }

        public void Remove(DataChannel dataChannel)
        {
            
        }

        public void Send(UMI3DDto dto, bool reliable, string peerId = null)
        {
            List<string> target = peerId == null ? new List<string>() { peerId } : UMI3DCollaborationEnvironmentLoader.Instance.UserList.Select(u => u.id).ToList();
            target.Add(UMI3DGlobalID.ServerId);
            Send(dto, target, DataType.Data, reliable);        
        }

        public void Send(UMI3DDto dto, bool reliable, DataType dataType, string peerId = null)
        {
            List<string> target = peerId == null ? new List<string>() { peerId } : UMI3DCollaborationEnvironmentLoader.Instance.UserList.Select(u => u.id).ToList();
            target.Add(UMI3DGlobalID.ServerId);
            Send(dto, target, dataType, reliable);
        }

        public void sendAudio(AudioDto dto)
        {
            List<string> target = UMI3DCollaborationEnvironmentLoader.Instance.UserList.Select(u => u.id).ToList();
            target.Add(UMI3DGlobalID.ServerId);
            Send(dto, target, DataType.Audio, false);
        }

        public void sendAudio(List<DataChannel> channels, AudioDto dto)
        {
            throw new System.NotImplementedException();
        }

        public void SendServer(UMI3DDto dto, bool reliable)
        {
            List<string> target = new List<string>() { UMI3DGlobalID.ServerId };

            Send(dto, target, DataType.Audio, false);
        }

        void Send(UMI3DDto content, List<string> target, DataType dataType, bool reliable, bool useWebrtc = true)
        {

            var dto = new FakeWebrtcMessageDto()
            {
                sourceId = UMI3DCollaborationClientServer.Identity.userId,
                content = content.ToBson(),
                targetId = target,
                dataType = dataType,
                reliable = reliable
            };
            var ws = reliable ? wsReliable : wsUnReliable;
            ws?.Send(dto.ToBson());
        }


        public void Stop()
        {
            Clear();
        }
    }
}