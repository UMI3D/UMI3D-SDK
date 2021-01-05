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
using umi3d.edk.interaction;
using umi3d.edk.userCapture;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace umi3d.edk.collaboration
{

    public class UMI3DWebsocketRTCFactory : AbstractWebsocketRtcFactory, IWebRTCServer
    {
        const string ReliableName = "Reliable";
        const string UnreliableName = "Unreliable";

        public DataChannel CreateChannel(UMI3DCollaborationUser user, DataChannel dataBase)
        {
            return CreateChannel(user, dataBase.reliable, dataBase.type);
        }

        public DataChannel CreateChannel(UMI3DCollaborationUser user, bool reliable, DataChannelTypes dataType)
        {
            var dc = new WebsocketDataChannel(UMI3DGlobalID.ServerId, user.Id(), $"{(reliable ? ReliableName : UnreliableName)}_{dataType}", reliable, dataType);
            dc.Socket = user.connection;
            var dto = new RTCDataChannelDto() { reliable = reliable, sourceUser = UMI3DGlobalID.ServerId, targetUser = user.Id(), type = dataType };
            SendWebsocket(user, dto);
            return dc;
        }

        public void OnMessage(UMI3DDto dto)
        {
            if (dto is FakeWebrtcMessageDto fake)
            {
                foreach (string targetId in fake.targetId)
                    if (targetId == UMI3DGlobalID.ServerId)
                    {
                        var user = UMI3DCollaborationServer.Collaboration.GetUser(fake.sourceId);
                        switch (fake.dataType)
                        {
                            case DataChannelTypes.Tracking:
                                var trackingData = UMI3DDto.FromBson(fake.content);
                                if (trackingData is common.userCapture.UserTrackingFrameDto frame)
                                    UMI3DEmbodimentManager.Instance.UserTrackingReception(frame);
                                else if (trackingData is common.userCapture.UserCameraPropertiesDto camera)
                                    UMI3DEmbodimentManager.Instance.UserCameraReception(camera, user);

                                UMI3DCollaborationServer.Collaboration.Users
                                .Where(u => u.Id() != fake.sourceId)
                                .Select(u => u.dataChannels
                                .FirstOrDefault(d => d.reliable == fake.reliable && d.type == fake.dataType))
                                .Where(d => d != default)
                                .ForEach(d => d.Send(fake.content));

                                break;

                            case DataChannelTypes.Data:
                                var data2 = UMI3DDto.FromBson(fake.content);
                                UMI3DBrowserRequestDispatcher.DispatchBrowserRequest(user, data2);
                                break;
                            case DataChannelTypes.VoIP:
                                UMI3DCollaborationServer.Collaboration.Users
                                    .Where(u => u.Id() != fake.sourceId)
                                    .Select(u => u.dataChannels
                                    .FirstOrDefault(d => d.reliable == false && d.type == DataChannelTypes.VoIP))
                                    .Where(d => d != default)
                                    .ForEach(d => d.Send(fake.content));
                                break;
                            case DataChannelTypes.Video:
                                break;
                        }
                    }
                    else
                    {
                        var u = UMI3DCollaborationServer.Collaboration.GetUser(targetId);
                        var dc = u.dataChannels.FirstOrDefault(d => d is WebsocketDataChannel wd && wd.type == fake.dataType && wd.reliable == fake.reliable);
                        if (dc != default)
                            dc.Send(fake.content);
                    }
            }
        }

        public DataChannel CreateChannel(UMI3DCollaborationUser userA, UMI3DCollaborationUser userB, DataChannel dataBase)
        {
            return CreateChannel(userA, userB, dataBase.reliable, dataBase.type);
        }

        public DataChannel CreateChannel(UMI3DCollaborationUser userA, UMI3DCollaborationUser userB, bool reliable, DataChannelTypes dataType)
        {
            var dto = new RTCDataChannelDto() { reliable = reliable, sourceUser = userA.Id(), targetUser = userB.Id(), type = dataType };
            Debug.Log(dto.reliable);
            SendWebsocket(userA, dto);
            var dc = new BridgeChannel(userA, userB, $"{userA.Id()} - {userB.Id()} : {(reliable ? ReliableName : UnreliableName)}_{dataType} ", reliable, dataType);
            return dc;
        }

        public DataChannel CreateChannel(string userA, string userB, bool reliable, DataChannelTypes dataType)
        {
            if (userA == UMI3DGlobalID.ServerId) return CreateChannel(userB, reliable, dataType);
            else if (userB == UMI3DGlobalID.ServerId) return CreateChannel(userA, reliable, dataType);
            else
            {
                var uA = UMI3DCollaborationServer.Collaboration.GetUser(userA);
                var uB = UMI3DCollaborationServer.Collaboration.GetUser(userB);
                return CreateChannel(uA, uB, reliable, dataType);
            }
        }

        public override DataChannel CreateChannel(string user, bool reliable, DataChannelTypes dataType)
        {
            var u = UMI3DCollaborationServer.Collaboration.GetUser(user);
            return CreateChannel(u, reliable, dataType);
        }


        public override void DeleteChannel(DataChannel dataChannel)
        {
            dataChannel.Close();
        }

        public override void HandleMessage(RTCDto dto)
        {
            if (dto.targetUser == UMI3DGlobalID.ServerId || dto.sourceUser == UMI3DGlobalID.ServerId)
                base.HandleMessage(dto);
            else
            {
                var u = UMI3DCollaborationServer.Collaboration.GetUser(dto.targetUser);
                switch (dto)
                {
                    case OfferDto offer:
                        break;
                    case AnswerDto answer:
                        break;
                    case CandidateDto candidate:
                        break;
                    case LeaveDto _:
                        break;
                    case RTCConnectionDTO _:
                        break;
                    case RTCCloseConnectionDto _:
                        break;
                    case RTCDataChannelDto dcDto:
                        CreateChannel(dcDto.targetUser, dto.sourceUser, dcDto.reliable, dcDto.type);
                        break;
                    default:
                        Debug.LogError("other :" + dto);
                        break;
                }
                SendWebsocket(u, dto);
            }
        }

        void SendWebsocket(UMI3DCollaborationUser user, RTCDto dto)
        {
            user.connection.SendData(dto);
        }
    }
}