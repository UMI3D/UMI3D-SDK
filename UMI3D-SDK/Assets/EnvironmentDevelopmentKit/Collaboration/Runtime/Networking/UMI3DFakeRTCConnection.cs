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
        public string _id = null;
        private static int _number = 0;
        private string _prefix;
        public bool reliable { get; private set; }

        Action<UMI3DFakeRTCConnection, UMI3DDto> messageAction;
        Action<UMI3DFakeRTCConnection> closeAction;


        public UMI3DFakeRTCConnection(bool reliable, Action<UMI3DFakeRTCConnection, UMI3DDto> messageAction, Action<UMI3DFakeRTCConnection> closeAction) :this(null,reliable, messageAction, closeAction)
        {
        }

        public UMI3DFakeRTCConnection(string prefix,bool reliable, Action<UMI3DFakeRTCConnection, UMI3DDto> messageAction, Action<UMI3DFakeRTCConnection> closeAction)
        {
            _prefix = !prefix.IsNullOrEmpty() ? prefix : "fakeRTCconnection_";
            this.reliable = reliable;
        }

        private static int getNumber()
        {
            return Interlocked.Increment(ref _number);
        }

        //on user quit
        protected override void OnClose(CloseEventArgs e)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(_OnClose(e));
        }

        //on user send message
        protected override void OnMessage(MessageEventArgs e)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(_OnMessage(e));
        }

        IEnumerator _OnMessage(MessageEventArgs e)
        {
            messageAction?.Invoke(this,UMI3DDto.FromBson(e.RawData));
            yield break;
        }

        IEnumerator _OnClose(CloseEventArgs e)
        {
            closeAction?.Invoke(this);
            yield break;
        }

        //on user connect
        protected override void OnOpen()
        {
            Debug.Log("open");
        }

        public void SendData(UMI3DDto obj,Action<bool> callback = null)
        {
            if (obj != null && this.Context.WebSocket.IsConnected)
            {
                var data = obj.ToBson();
                try
                {
                    if(callback == null) callback = (b) => { };
                    SendAsync(data, callback);
                }
                catch (InvalidOperationException exp)
                {
                    Debug.LogWarning(exp);
                    return;
                }
            }
        }

        public string GetId()
        {
            return _id;
        }

    }
}