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
    /// <see cref="AbstractWebsocketRtcFactory"/>
    public class WebRTCClientFactory : AbstractWebsocketRtcFactory, IWebRTCClient
    {
        const string ReliableName = "Reliable";
        const string UnreliableName = "Unreliable";

        public DataChannel CreateChannel(UMI3DUser user, DataChannel dataBase)
        {
            throw new System.NotImplementedException();
        }

        public DataChannel CreateChannel(UMI3DUser user, bool reliable, DataType dataType)
        {

            var dataChannels = user == null ? UMI3DCollaborationClientServer.dataChannels : user.dataChannels;
            DataChannel dataChannel;
            if((dataChannel = dataChannels.FirstOrDefault(d=>d is WebsocketDataChannel wd && d.reliable == reliable && d.type == dataType)) != default)
            {
                return dataChannel;
            }

            var id = user == null ? UMI3DGlobalID.ServerId : user.id;
            var dc = new WebsocketDataChannel(UMI3DCollaborationClientServer.Identity.userId, id, $"{(reliable ? ReliableName : UnreliableName)}_{dataType}", reliable, dataType);
            dc.Socket = UMI3DCollaborationClientServer.Instance.WebSocketClient;
            dataChannels.Add(dc);
            var dto = new RTCDataChannelDto() { reliable = reliable, sourceUser = UMI3DCollaborationClientServer.Identity.userId, targetUser = id, type = dataType };
            UMI3DCollaborationClientServer.Instance.WebSocketClient.Send(dto);
            return dc;
        }

        public override DataChannel CreateChannel(string userId, bool reliable, DataType dataType)
        {
            //Debug.Log($"<color=purple>{userId} {reliable} {dataType}<color>");
            var user = UMI3DCollaborationEnvironmentLoader.Instance.UserList.FirstOrDefault(u => u.id == userId);
            return CreateChannel(user,reliable,dataType);
        }

        public void OnMessage(UMI3DDto dto)
        {
            if(dto is FakeWebrtcMessageDto fake)
            {
                switch (fake.dataType)
                {
                    case DataType.Tracking:
                    case DataType.Data:
                        var data = UMI3DDto.FromBson(fake.content);
                        UMI3DCollaborationClientServer.OnRtcMessage(null, data, null);
                        break;
                    case DataType.Audio:
                        AudioManager.Instance.Read(fake.content);
                        break;
                    case DataType.Video:
                        break;
                }
                
            }
        }
    }
}