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

    public class UMI3DFakeRTCConnection : WebSocketBehavior
    {
        internal string _id = null;
        private static int _number = 0;
        private string _prefix;
        public bool reliable { get; private set; }

        Action<UMI3DFakeRTCConnection, UMI3DDto> messageAction;
        Action<UMI3DFakeRTCConnection> closeAction;
        Action<UMI3DFakeRTCConnection, IdentityDto> identifyAction;

        public UMI3DFakeRTCConnection(bool reliable, Action<UMI3DFakeRTCConnection, UMI3DDto> messageAction, Action<UMI3DFakeRTCConnection> closeAction, Action<UMI3DFakeRTCConnection, IdentityDto> identifyAction) : this(null, reliable, messageAction, closeAction, identifyAction)
        {
        }

        public UMI3DFakeRTCConnection(string prefix, bool reliable, Action<UMI3DFakeRTCConnection, UMI3DDto> messageAction, Action<UMI3DFakeRTCConnection> closeAction, Action<UMI3DFakeRTCConnection, IdentityDto> identifyAction)
        {
            _prefix = !prefix.IsNullOrEmpty() ? prefix : "fakeRTCconnection_";
            this.reliable = reliable;
            this.messageAction = messageAction;
            this.closeAction = closeAction;
            this.identifyAction = identifyAction;
        }

        private static int getNumber()
        {
            return Interlocked.Increment(ref _number);
        }

        ///<inheritdoc/>
        protected override void OnClose(CloseEventArgs e)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(_OnClose(e));
        }

        ///<inheritdoc/>
        protected override void OnMessage(MessageEventArgs e)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(_OnMessage(e));
        }

        IEnumerator _OnMessage(MessageEventArgs e)
        {
            var data = UMI3DDto.FromBson(e.RawData);
            if (data is IdentityDto id)
                identifyAction?.Invoke(this, id);
            else
                messageAction?.Invoke(this, data);
            yield break;
        }

        IEnumerator _OnClose(CloseEventArgs e)
        {
            closeAction?.Invoke(this);
            yield break;
        }

        ///<inheritdoc/>
        protected override void OnOpen()
        {
            Debug.Log("open");
        }

        /// <summary>
        /// Send Byte message.
        /// </summary>
        /// <param name="data">message to send</param>
        /// <param name="callback">callback function</param>
        public void SendData(byte[] data, Action<bool> callback = null)
        {
            if (data != null && this.Context.WebSocket.IsConnected)
            {
                try
                {
                    if (callback == null) callback = (b) => { };
                    SendAsync(data, callback);
                }
                catch (InvalidOperationException exp)
                {
                    Debug.LogWarning(exp);
                    return;
                }
            }
        }

        /// <summary>
        /// det the id of the connection
        /// </summary>
        /// <returns></returns>
        public string GetId()
        {
            return _id;
        }
    }
}