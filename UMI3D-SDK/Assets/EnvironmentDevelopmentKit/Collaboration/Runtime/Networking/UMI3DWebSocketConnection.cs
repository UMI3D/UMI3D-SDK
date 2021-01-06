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
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;
using WebSocketSharp;

namespace umi3d.edk.collaboration
{

    public class UMI3DWebSocketConnection : UMI3DAbstractWebSocketConnection
    {
        ///<inheritdoc/>
        public UMI3DWebSocketConnection() : base()
        {}

        ///<inheritdoc/>
        protected override void OnUserCreated(UMI3DCollaborationUser user, bool reconnection)
        {
            base.OnUserCreated(user, reconnection);
            user.InitConnection(this);
            SendData(user.ToStatusDto());
            Debug.Log($"<color=yellow>open {_id}</color>");
        }

        ///<inheritdoc/>
        protected override void OnOpen()
        {
            base.OnOpen();
        }

        ///<inheritdoc/>
        protected override void OnClose(CloseEventArgs e)
        {
            Debug.Log($"<color=orange>onClose {_id}</color>");
            UnityMainThreadDispatcher.Instance().Enqueue(UMI3DCollaborationServer.Collaboration.ConnectionClose(_id));
        }

        ///<inheritdoc/>
        protected override void HandleMessage(UMI3DDto dto)
        {
            base.HandleMessage(dto);
            if (dto is StatusDto req)
            {
                Debug.Log(req.status);
                UMI3DCollaborationServer.Collaboration.OnStatusUpdate(_id, req.status);
            }
            else if (dto is RTCDto rtc)
            {
                UMI3DCollaborationServer.Instance.WebRtcMessage(_id, rtc);
            }
            else
                UMI3DCollaborationServer.WebRTC.OnMessage(dto);
        }
    }
}