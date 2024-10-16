/*
Copyright 2019 - 2021 Inetum

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
using umi3d.common.collaboration.dto.signaling;

namespace umi3d.edk.collaboration
{
    public class UMI3DServerUser : UMI3DCollaborationAbstractContentUser
    {
        public static event Action<UMI3DServerUser, string> OnMessage;

        public UMI3DServerUser(RegisterIdentityDto identity) : base(identity)
        {

        }


        public override void InitConnection(UMI3DForgeServer connection)
        {
            base.InitConnection(connection);
            SetStatus(common.StatusType.READY);
        }

        public virtual void SendMessageToThisServer(string message)
        {
            var request = new ServerMessageRequest(message, this);
            request.ToTransaction(true).Dispatch();
            UnityEngine.Debug.Log("Dispatch");
        }

        public void ReceivedMessage(string message)
        {
            OnMessage?.Invoke(this, message);
        }
    }
}