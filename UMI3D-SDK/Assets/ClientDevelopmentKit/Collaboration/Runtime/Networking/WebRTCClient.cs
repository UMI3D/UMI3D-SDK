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
using umi3d.edk.collaboration;
using Unity.WebRTC;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// AbstractWebRtcClient implementation for a client
    /// </summary>
    /// <see cref="AbstractWebRtcClient"/>
    public class WebRTCClient : AbstractWebRtcClient
    {
        UMI3DCollaborationClientServer client;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client">a reference to the server.</param>
        public WebRTCClient(UMI3DCollaborationClientServer client) : base(client)
        {
            this.client = client;
        }

        /// <summary>
        /// Send an audioDto to a list of DataChannels.
        /// </summary>
        /// <param name="channels">list of data channel.</param>
        /// <param name="dto">AudioDto.</param>
        public void sendAudio(List<DataChannel> channels,AudioDto dto)
        {
            foreach (var c in channels)
                c.dataChannel.Send(dto.ToBson());
        }

        /// <summary>
        /// Send an audioDto to all peers.
        /// </summary>
        /// <param name="dto">AudioDto.</param>
        public void sendAudio(AudioDto dto)
        {
            foreach (var peer in peers.Values)
            {
                foreach(var channel in peer.channels)
                    if(channel.type == DataType.Audio)
                    {
                        if (channel?.dataChannel != null && channel.IsOpen && channel.dataChannel.ReadyState == RTCDataChannelState.Open)
                        {
                            //Debug.Log($"Send via [{channel.IsOpen && channel.dataChannel.ReadyState == RTCDataChannelState.Open}] {channel?.Label}:{channel?.dataChannel}");
                            channel?.dataChannel?.Send(dto.ToBson());
                        }
                        //else
                        //    Debug.Log($"Send via [False] {channel?.Label}:{channel?.dataChannel}");
                        break;
                    }
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
            var user = UMI3DCollaborationEnvironmentLoader.Instance.UserList.FirstOrDefault(u => u.id == id);
            if (channel.type == DataType.Audio)
                AudioManager.Instance.Read(user, bytes, channel);
            else
                UMI3DCollaborationClientServer.OnRtcMessage(user, bytes, channel);
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
        protected override void ChannelsToAddCreation(string uid, WebRTCconnection connection)
        {
            base.ChannelsToAddCreation(uid, connection);
            if(uid == UMI3DGlobalID.ServerId)
                foreach (var channel in WebRtcChannels.defaultPeerToServerChannels)
                    if(!connection.channels.Any(c => c.Label == channel.Label))
                        connection.channels.Add(CreateDataChannel(channel, uid));
        }

        /// <summary>
        /// Send a Webrtc message on a Data channel
        /// </summary>
        /// <param name="dto">message to send</param>
        /// <param name="reliable">should the data channel be reliable or not</param>
        public void SendServer(UMI3DDto dto, bool reliable)
        {
            peers[UMI3DGlobalID.ServerId].Send(dto.ToBson(),reliable);
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
    }
}