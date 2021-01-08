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
using System.Threading;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;


namespace umi3d.edk.collaboration
{

    public class UMI3DFakeRTCConnection : UMI3DAbstractWebSocketConnection
    {
        public DataType type { get; }
        public bool reliable { get; }

        Action<UMI3DDto> messageAction { get; }

        ///<inheritdoc/>
        public UMI3DFakeRTCConnection(DataType type, bool reliable, Action<UMI3DDto> messageAction) : base()
        {
            this.type = type;
            this.reliable = reliable;
            this.messageAction = messageAction;
        }

        public UMI3DFakeRTCConnection(DataType type, Action<UMI3DDto> messageAction) : this(type, false, messageAction)
        { }

        ///<inheritdoc/>
        protected override void OnUserCreated(UMI3DCollaborationUser user, bool reconnection)
        {
            base.OnUserCreated(user, reconnection);
            Debug.Log($"<color=yellow>open {type} [reliable:{reliable}] {_id}</color>");
        }

        ///<inheritdoc/>
        protected override void OnOpen()
        {
            base.OnOpen();
        }

        ///<inheritdoc/>
        protected override void OnClose(CloseEventArgs e)
        {
            Debug.Log($"<color=orange>onClose {type} [reliable:{reliable}] {_id}</color>");
            UnityMainThreadDispatcher.Instance().Enqueue(UMI3DCollaborationServer.Collaboration.ConnectionClose(_id));
        }

        ///<inheritdoc/>
        protected override void HandleMessage(UMI3DDto dto)
        {
            base.HandleMessage(dto);
            messageAction?.Invoke(dto);
        }
    }
}