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
        UMI3DCollaborationServer server;

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
            public bool Contain(string A, string B) { return (peerA == A && peerB == B) || (peerA == B && peerB == A); }
            public string GetOther(string peer)
            {
                if (peer == peerA) return peerB;
                if (peer == peerB) return peerA;
                throw new Exception($"bridge does not contain peer[{peer}]");
            }
        }
        protected List<bridge> peerMap;

        public class NewPeerListener : UnityEvent<string> { };
        public NewPeerListener onNewPeer = new NewPeerListener();
        public UMI3DFakeWebRTC fakeWebRTC;

        public override AbstractWebsocketRtc websocketRtc => fakeWebRTC;

        /// <summary>
        /// Initialization of the WebrtcClient
        /// </summary>
        /// <param name="server">The server</param>
        public UMI3DWebRTC(UMI3DCollaborationServer server, bool useSoftware) : base(server, useSoftware)
        {
            this.server = server;
            peerMap = new List<bridge>();
             fakeWebRTC = new UMI3DFakeWebRTC(this, OnFakeRtcMessage);
        }

        /// <summary>
        /// Handle RTCDto Message received by the server
        /// </summary>
        /// <param name="dto"></param>
        public override void HandleMessage(RTCDto dto)
        {
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
                    //Debug.Log($"serveur {dcDto.Label} {dcDto.type} {dcDto.reliable} {dcDto.sourceUser}->{dcDto.targetUser}");
                    UMI3DCollaborationUser otherUser = UMI3DCollaborationServer.Collaboration.GetUser(dcDto.targetUser);
                    if (!otherUser.useWebrtc) Debug.LogError("Should Not try to established peer connection with this peer");
                    else
                    {
                        var bridge = peerMap.FirstOrDefault(b => b.Contain(dto.sourceUser, dto.targetUser));
                        if (!bridge.Equals(default) && bridge.channel != null && !bridge.channel.Any(d => d.Label == dcDto.Label))
                            bridge.channel.Add(dcDto);
                        if (otherUser != null)
                            otherUser.connection.SendData(dcDto);
                    }
                }
            }
            else
            {
                base.HandleMessage(dto);
            }
        }

        /// <summary>
        /// Send a Message to all peers
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="reliable"></param>
        public void SendRTC(UMI3DDto dto, bool reliable, string peerId = null, bool useWebrtc = true)
        {
            Send(dto, reliable, peerId);
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
            if (peers.ContainsKey(user.Id()))
                return peers[user.Id()].Any(d => d.Label == label);
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
                return peers[user.Id()].Find(d => d.Label == label);
            return null;
        }

        public DataChannel GetChannel(UMI3DUser user, DataType dataType, bool reliable)
        {
            if (peers.ContainsKey(user.Id()))
                return peers[user.Id()].Find(d => d.type == dataType && d.reliable == reliable);
            return null;
        }


        public void OpenChannel(UMI3DUser userA, UMI3DUser userB, string label, DataType type, bool reliable)
        {
            if (!(userA == null || userB == null || label == null) && peerMap.Any(b => b.Contain(userA.Id(), userB.Id())))
            {
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
            var datachannel = connection.Find(d => d.Label == label);
            if (datachannel != default)
            {
                if (datachannel.type == type && datachannel.reliable == reliable) return datachannel;
                else throw new Exception("A datachannel with this label already exist");
            }
            datachannel = Add(user.Id(), new WebRTCDataChannel(GetUID(),user.Id(), label, reliable, type));
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
            DataChannel datachannel;
            if (connection.Find(d => d.Label == label,out datachannel))
            {
                connection.RemoveDataChannel(datachannel);
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
                IWebRTCconnection rtc = CreateWebRtcConnection(user.Id(), true);
                peers[user.Id()] = rtc;
                rtc.Offer();
            }
            if (user.useWebrtc)
                foreach (var u in UMI3DCollaborationServer.Collaboration.Users)
                {
                    if (u != user && u.useWebrtc)
                    {
                        if (peerMap.FindAll((b) => { return b.Contain(user.Id(), u.Id()); }).Count == 0)
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
            else if (channel.type == DataType.Data)
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

        protected void OnFakeRtcMessage(string id, DataType dataType, bool reliable, List<string> ids, byte[] _data)
        {
            var data = UMI3DDto.FromBson(_data);
            var user = UMI3DCollaborationServer.Collaboration.GetUser(id);
            foreach (var target in ids)
            {
                if (target == UMI3DGlobalID.ServerId)
                {
                    if (dataType == DataType.Tracking)
                    {
                        if (data is common.userCapture.UserTrackingFrameDto tracking)
                            UMI3DEmbodimentManager.Instance.UserTrackingReception(tracking);

                        else if (data is common.userCapture.UserCameraPropertiesDto userCamera)
                            UMI3DEmbodimentManager.Instance.UserCameraReception(userCamera, user);
                    }
                    else if (dataType == DataType.Data)
                    {
                        UMI3DBrowserRequestDispatcher.DispatchBrowserRequest(user, data);
                    }
                    else
                    {
                        Debug.Log($"new radiovideo message {dataType}");
                        //RadioVideo
                    }
                }
                else
                    Debug.Log($"transfer message to {target}");
            }
        }


        /// <summary>
        /// Add defaultPeerToServerChannels
        /// </summary>
        /// <seealso cref="AbstractWebRtcClient.ChannelsToAddCreation(string, IWebRTCconnection)"/>
        /// <seealso cref="WebRtcChannels.defaultPeerToServerChannels"/>
        protected override void ChannelsToAddCreation(string uid, IWebRTCconnection connection)
        {
            base.ChannelsToAddCreation(uid, connection);
            foreach (var channel in WebRtcChannels.defaultPeerToServerChannels)
                connection.AddDataChannel(new WebRTCDataChannel(GetUID(),uid,channel), false);
        }

        ///<inheritdoc/>
        protected override void OnRtcDataChannelOpen(DataChannel channel)
        {
            //Debug.Log($"TODO: Data Channel Opened! {channel.Label}");
        }

        ///<inheritdoc/>
        protected override void OnRtcDataChannelClose(DataChannel channel)
        {
            //Debug.Log($"TODO: Data Channel Closed! {channel.Label}");
        }

        ///<inheritdoc/>
        protected override string GetLogPrefix()
        {
            return $"WebRTC Server";
        }

        ///<inheritdoc/>
        protected override string GetUID()
        {
            return UMI3DGlobalID.ServerId;
        }

        ///<inheritdoc/>
        protected override void WebSocketSend(RTCDto dto, string targetId)
        {
            UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUser(targetId);
            user.connection.SendData(dto);
        }

        /// <summary>
        /// Create and setup WebrtcConnection
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        protected override IWebRTCconnection CreateWebRtcConnection(string uid, bool instanciateChannel = false)
        {
            var user = UMI3DCollaborationServer.Collaboration.GetUser(uid);
            if (user.useWebrtc)
                return base.CreateWebRtcConnection(uid, instanciateChannel);
            UMI3DFakeRTCClient connection = new UMI3DFakeRTCClient(uid);
            ChannelsToAddCreation(uid, connection);
            connection.Init(GetUID(),uid, instanciateChannel);
            return connection;
        }

        ///<inheritdoc/>
        protected override void OnConnectionDisconnected(string id)
        {
            var user = UMI3DCollaborationServer.Collaboration.GetUser(id);
            user.SetStatus(StatusType.MISSING);
        }
    }
}