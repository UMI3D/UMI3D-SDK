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
using umi3d.edk.interaction;
using umi3d.edk.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.collaboration
{
    public class UMI3DWebRTC : AbstractWebRtcClient
    {
        UMI3DServer server;

        protected struct bridge
        {
            public string peerA;
            public string peerB;

            public List<RTCDataChannelDto> channel;

            public bridge(string peerA, string peerB)
            {
                this.peerA = peerA;
                this.peerB = peerB;
                channel = new List<RTCDataChannelDto>();
            }

            public bool Contain(string peer) { return peerA == peer || peerB == peer; }
            public bool Contain(string A,string B) { return (peerA == A && peerB == B) || (peerA == B && peerB == A); }
            public string GetOther(string peer) { 
                if (peer == peerA) return peerB; 
                if (peer == peerB) return peerA;
                throw new Exception($"bridge does not contain peer[{peer}]");
            }
        }
        protected List<bridge> peerMap; 

        public class NewPeerListener : UnityEvent<string> { };
        public NewPeerListener onNewPeer = new NewPeerListener();

        /// <summary>
        /// Initialization of the WebrtcClient
        /// </summary>
        /// <param name="server">The server</param>
        public UMI3DWebRTC(UMI3DServer server) : base (server)
        {
            this.server = server;
            peerMap = new List<bridge>();
        }

        /// <summary>
        /// Handle RTCDto Message received by the server
        /// </summary>
        /// <param name="dto"></param>
        public override void HandleMessage(RTCDto dto) {
            if (dto.targetUser != UMI3DGlobalID.ServerId)
            {
                if (dto is OfferDto)
                {
                    var offer = dto as OfferDto;
                    UMI3DCollaborationUser otherUser = UMI3DCollaborationServer.Collaboration.GetUser(offer.targetUser);
                    if (otherUser != null)
                        otherUser.connection.SendData(offer);
                }
                else if (dto is AnswerDto)
                {
                    var answer = dto as AnswerDto;
                    UMI3DCollaborationUser otherUser = UMI3DCollaborationServer.Collaboration.GetUser(answer.targetUser);
                    if (otherUser != null)
                        otherUser.connection.SendData(answer);
                }
                else if (dto is CandidateDto)
                {
                    var candidate = dto as CandidateDto;
                    UMI3DCollaborationUser otherUser = UMI3DCollaborationServer.Collaboration.GetUser(candidate.targetUser);
                    if (otherUser != null)
                        otherUser.connection.SendData(candidate);
                }
                else if (dto is RTCDataChannelDto)
                {

                    var dcDto = dto as RTCDataChannelDto;
                    Debug.Log($"serveur {dcDto.Label}  {dcDto.sourceUser}->{dcDto.targetUser}");
                    UMI3DCollaborationUser otherUser = UMI3DCollaborationServer.Collaboration.GetUser(dcDto.targetUser);

                    var bridge = peerMap.FirstOrDefault(b => b.Contain(dto.sourceUser, dto.targetUser));
                    if (!bridge.Equals(default) && bridge.channel != null && !bridge.channel.Any(d => d.Label == dcDto.Label))
                        bridge.channel.Add(dcDto);
                    if (otherUser != null)
                        otherUser.connection.SendData(dcDto);
                }
            }
            else
            {
                base.HandleMessage(dto);
            }
        }

        public bool ContainsChannel(UMI3DUser userA, UMI3DUser userB, string label)
        {
            if (!(userA == null || userB == null || label == null) && peerMap.Any(b => b.Contain(userA.Id(), userB.Id())))
            {
                return peerMap.FirstOrDefault(b => b.Contain(userA.Id(), userB.Id())).channel.Any(dto => dto.Label == label);
            }
            return false;
        }

        public bool ContainsChannel(UMI3DUser user, string label)
        {
            if(peers.ContainsKey(user.Id()))
                return peers[user.Id()].channels.Any(d => d.Label == label);
            return false;
        }

        public RTCDataChannelDto GetChannel(UMI3DUser userA, UMI3DUser userB, string label)
        {
            if (!(userA == null || userB == null || label == null) && peerMap.Any(b => b.Contain(userA.Id(), userB.Id())))
            {
                return peerMap.FirstOrDefault(b => b.Contain(userA.Id(), userB.Id())).channel.FirstOrDefault(dto => dto.Label == label);
            }
            return null;
        }

        public DataChannel GetChannel(UMI3DUser user, string label)
        {
            if (peers.ContainsKey(user.Id()))
                return peers[user.Id()].channels.FirstOrDefault(d => d.Label == label);
            return null;
        }

        public DataChannel GetChannel(UMI3DUser user, DataType dataType, bool reliable)
        {
            if (peers.ContainsKey(user.Id()))
                return peers[user.Id()].channels.FirstOrDefault(d => d.type == dataType && d.reliable == reliable);
            return null;
        }


        public void OpenChannel(UMI3DUser userA, UMI3DUser userB, string label, DataType type, bool reliable)
        {
            if (!(userA == null || userB == null || label == null) && peerMap.Any(b => b.Contain(userA.Id(), userB.Id()))) {
                var bridge = peerMap.FirstOrDefault(b => b.Contain(userA.Id(), userB.Id()));
                if (!bridge.channel.Any(dto => dto.Label == label))
                {
                    var rtc = new RTCDataChannelDto()
                    {
                        Label = label,
                        type = type,
                        reliable = reliable,
                        sourceUser = userA.Id(),
                        targetUser = userB.Id()
                    };
                    WebSocketSend(rtc, userB.Id());
                    WebSocketSend(rtc, userA.Id());
                    bridge.channel.Add(rtc);
                }
            }
        }
        public DataChannel OpenChannel(UMI3DUser user, string label, DataType type, bool reliable)
        {
            var connection = peers[user.Id()];
            var datachannel = connection.channels.FirstOrDefault(d => d.Label == label);
            if (datachannel != default)
            {
                if (datachannel.type == type && datachannel.reliable == reliable) return datachannel;
                else throw new Exception("A datachannel with this label already exist");
            }
            datachannel = Add(user.Id(), new DataChannel(label, reliable, type));
            return datachannel;
        }

        public void CloseChannel(UMI3DUser userA, UMI3DUser userB, string label)
        {
            if (!(userA == null || userB == null || label == null) && peerMap.Any(b => b.Contain(userA.Id(), userB.Id())))
            {
                var bridge = peerMap.FirstOrDefault(b => b.Contain(userA.Id(), userB.Id()));
                var dto = bridge.channel.First(d => d.Label == label);
                var rtc = new RTCCloseDataChannelDto()
                {
                    Label = label,
                    sourceUser = userA.Id(),
                    targetUser = userB.Id()
                };
                WebSocketSend(rtc, userB.Id());
                bridge.channel.Remove(dto);
            }
        }
        
        public void CloseChannel(UMI3DUser user, string label)
        {
            var connection = peers[user.Id()];
            var datachannel = connection.channels.FirstOrDefault(d => d.Label == label);
            if (datachannel != default)
            {
                datachannel.Close();
                connection.channels.Remove(datachannel);
            }
        }

        /// <summary>
        /// Create bridge between peers and the bridge between peers and server
        /// </summary>
        /// <param name="user"></param>
        public void newUser(UMI3DCollaborationUser user)
        {
            if (!peers.ContainsKey(user.Id()))
            {
                WebRTCconnection rtc = CreateWebRtcConnection(user.Id(),true);
                peers[user.Id()] = rtc;
                rtc.Offer();
            }
            foreach(var u in UMI3DCollaborationServer.Collaboration.Users)
            {
                if (u != user)
                {
                    if (peerMap.FindAll((b) => { return b.Contain(user.Id(),u.Id()); }).Count == 0)
                    {
                        peerMap.Add(new bridge(user.Id(), u.Id()));
                        u.connection.SendData(new RTCConnectionDTO
                        {
                            sourceUser = user.Id(),
                            targetUser = u.Id(),
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Clear peer bridge when a user leave the server.
        /// </summary>
        /// <param name="user"></param>
        public void UserLeave(UMI3DUser user)
        {
            string id = user.Id();
            if (peers.ContainsKey(id))
            {
                foreach (var bridge in peerMap.FindAll((b) => { return b.Contain(id); }).ToList())
                {
                    peerMap.Remove(bridge);
                    UMI3DCollaborationUser u = UMI3DCollaborationServer.Collaboration.GetUser(bridge.GetOther(id));
                    if (u?.connection != null)
                        u.connection.SendData(new RTCCloseConnectionDto
                        {
                            sourceUser = user.Id(),
                            targetUser = u.Id(),
                        });
                }

                peers[id].Close();
                peers.Remove(id);
            }
        }

        /// <summary>
        /// Invoke onRtcMessage.
        /// </summary>
        /// <seealso cref="AbstractWebRtcClient.OnRtcMessage(string, byte[], DataChannel)"/>
        protected override void OnRtcMessage(string id, byte[] bytes, DataChannel channel)
        {
            var user = UMI3DCollaborationServer.Collaboration.GetUser(id);
            if (channel.type == DataType.Tracking)
            {
                var data = UMI3DDto.FromBson(bytes);
                if (data is common.userCapture.UserTrackingFrameDto)
                    UMI3DEmbodimentManager.Instance.UserTrackingReception(data as common.userCapture.UserTrackingFrameDto);

                else if (data is common.userCapture.UserCameraPropertiesDto)
                    UMI3DEmbodimentManager.Instance.UserCameraReception(data as common.userCapture.UserCameraPropertiesDto, user);
            }
            else if(channel.type == DataType.Data)
            {
                var data = UMI3DDto.FromBson(bytes);
                UMI3DBrowserRequestDispatcher.DispatchBrowserRequest(user, data);
            }
            else
            {
                Debug.Log("new radiovideo message");
                //RadioVideo
            }
        }

        /// <summary>
        /// Add defaultPeerToServerChannels
        /// </summary>
        /// <seealso cref="AbstractWebRtcClient.ChannelsToAddCreation(string, WebRTCconnection)"/>
        /// <seealso cref="WebRtcChannels.defaultPeerToServerChannels"/>
        protected override void ChannelsToAddCreation(string uid, WebRTCconnection connection)
        {
            base.ChannelsToAddCreation(uid, connection);
            foreach (var channel in WebRtcChannels.defaultPeerToServerChannels)
                connection.channels.Add(new DataChannel(channel));
        }

        /// <inheritdoc cref="AbstractWebRtcClient.OnRtcDataChannelOpen(DataChannel)"/>
        protected override void OnRtcDataChannelOpen(DataChannel channel)
        {
            //Debug.Log($"TODO: Data Channel Opened! {channel.Label}");
        }

        /// <inheritdoc cref="AbstractWebRtcClient.OnRtcDataChannelClose(DataChannel)"/>
        protected override void OnRtcDataChannelClose(DataChannel channel)
        {
            //Debug.Log($"TODO: Data Channel Closed! {channel.Label}");
        }

        /// <inheritdoc cref="AbstractWebRtcClient.GetLogPrefix()"/>
        protected override string GetLogPrefix()
        {
            return $"WebRTC Server";
        }

        /// <inheritdoc cref="AbstractWebRtcClient.GetUID"/>
        protected override string GetUID()
        {
            return UMI3DGlobalID.ServerId;
        }

        /// <inheritdoc cref="AbstractWebRtcClient.WebSocketSend(RTCDto, string)"/>
        protected override void WebSocketSend(RTCDto dto, string targetId)
        {
            UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUser(targetId);
            user.connection.SendData(dto);
        }
    }
}