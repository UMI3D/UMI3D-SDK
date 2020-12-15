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

using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// AbstractWebRtcClient implementation for a client
    /// </summary>
    /// <see cref="AbstractWebRtcClient"/>
    public class WebRTCClient : AbstractWebRtcClient, IWebRTCClient
    {
        UMI3DCollaborationClientServer client;
        FakeWebRTCClient FakeWebRTC;

        public override AbstractWebsocketRtc websocketRtc => FakeWebRTC;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client">a reference to the server.</param>
        public WebRTCClient(UMI3DCollaborationClientServer client, bool useSoftware) : base(client, useSoftware)
        {
            this.client = client;
            FakeWebRTC = new FakeWebRTCClient(client);
        }

        /// <summary>
        /// Send an audioDto to a list of DataChannels.
        /// </summary>
        /// <param name="channels">list of data channel.</param>
        /// <param name="dto">AudioDto.</param>
        public void sendAudio(List<DataChannel> channels, AudioDto dto)
        {
            foreach (var c in channels)
                c.Send(dto.ToBson());
        }

        /// <summary>
        /// Send an audioDto to all peers.
        /// </summary>
        /// <param name="dto">AudioDto.</param>
        public void sendAudio(AudioDto dto)
        {
            foreach (var peer in peers.Values)
            {
                peer.Send(dto.ToBson(), false, DataType.Audio);
            }
        }


        /// <summary>
        /// Call when an RtcMessage is received.
        /// </summary>
        /// <param name="id">peer id.</param>
        /// <param name="bytes">message as byte[].</param>
        /// <param name="channel">Datachannel from which this message was received.</param>
        protected override void OnRtcMessage(string id, byte[] bytes, DataChannel channel)
        {
            var user = UMI3DCollaborationEnvironmentLoader.Instance?.UserList?.FirstOrDefault(u => u.id == id);
            if (channel.type == DataType.Audio)
                AudioManager.Instance.Read(user, bytes, channel);
            else
            {
                var dto = UMI3DDto.FromBson(bytes);
                if (dto is FakeWebrtcMessageDto fake)
                {
                    var user2 = UMI3DCollaborationEnvironmentLoader.Instance?.UserList?.FirstOrDefault(u => u.id == fake.sourceId);
                    if (fake.dataType == DataType.Audio)
                        AudioManager.Instance.Read(user2, fake.content, null);
                    else
                    {
                        var dto2 = UMI3DDto.FromBson(fake.content);
                        UMI3DCollaborationClientServer.OnRtcMessage(user2, dto2, null);
                    }
                }
                else
                    UMI3DCollaborationClientServer.OnRtcMessage(user, dto, channel);
            }
        }

        /// <summary>
        /// Called when a dataChannel is opened
        /// </summary>
        /// <param name="channel"></param>
        protected override void OnRtcDataChannelOpen(DataChannel channel)
        {
            //Debug.Log($"TODO: Data Channel Opened! {channel.Label}");
        }

        /// <summary>
        /// Called when a dataChannel is closed
        /// </summary>
        /// <param name="channel"></param>
        protected override void OnRtcDataChannelClose(DataChannel channel)
        {
            //Debug.Log($"TODO: Data Channel Closed! {channel.Label}");
        }

        /// <summary>
        /// Called to add default Channels to a connection
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="connection"></param>
        protected override void ChannelsToAddCreation(string uid, IWebRTCconnection connection)
        {
            base.ChannelsToAddCreation(uid, connection);
            if (uid == UMI3DGlobalID.ServerId)
                foreach (var channel in WebRtcChannels.defaultPeerToServerChannels)
                    if (!connection.Any(c => c.Label == channel.Label))
                        connection.AddDataChannel(CreateDataChannel(channel, uid), false);
        }

        /// <summary>
        /// Send a Webrtc message on a Data channel
        /// </summary>
        /// <param name="dto">message to send</param>
        /// <param name="reliable">should the data channel be reliable or not</param>
        public void SendServer(UMI3DDto dto, bool reliable)
        {
            peers[UMI3DGlobalID.ServerId].Send(dto.ToBson(), reliable);
        }

        /// <summary>
        /// Find matching channel with the server. 
        /// </summary>
        /// <param name="reliable">should this channel be reliable.</param>
        /// <param name="dataType">datatype of the channel.</param>
        /// <param name="dataChannels">First matching DataChannels.</param>
        /// <returns></returns>
        public virtual bool ExistServer(bool reliable, DataType dataType, out List<DataChannel> dataChannels)
        {
            return Exist(reliable, dataType, out dataChannels, UMI3DGlobalID.ServerId);
        }

        /// <summary>
        /// USe For debug purpose only.
        /// this will add a formated message before webrtc log message.
        /// </summary>
        /// <returns></returns>
        protected override string GetLogPrefix()
        {
            return $"WebRtc Client {UMI3DCollaborationClientServer.Identity.login}";
        }

        /// <summary>
        /// Get the UID of the peer.
        /// </summary>
        /// <returns></returns>
        protected override string GetUID()
        {
            return UMI3DCollaborationClientServer.Identity.userId;
        }

        /// <summary>
        /// Send a websocketMessage.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="targetId"></param>
        protected override void WebSocketSend(RTCDto dto, string targetId)
        {
            client.Send(dto);
        }

        ///<inheritdoc/>
        protected override void OnConnectionDisconnected(string id)
        {
            Debug.Log($"client connection lost {id}");
        }

#if !UNITY_WEBRTC
        protected override IWebRTCconnection CreateWebRtcConnection(string uid, bool instanciateChannel = false)
        {
            return base.CreateWebRtcConnection(uid, true);
        }
#endif
    }
}