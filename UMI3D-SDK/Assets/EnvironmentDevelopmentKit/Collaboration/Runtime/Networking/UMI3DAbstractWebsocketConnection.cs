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

    public abstract class UMI3DAbstractWebSocketConnection : WebSocketBehavior, IWebsocket
    {
        public string _id = null;
        private static int _number = 0;
        private string _prefix;

        public UMI3DAbstractWebSocketConnection()
            : this(null)
        {
        }

        public UMI3DAbstractWebSocketConnection(string prefix)
        {
            _prefix = !prefix.IsNullOrEmpty() ? prefix : "connection_";
        }

        private string genId()
        {
            var id = Context.QueryString["id"];
            return !id.IsNullOrEmpty() ? id : _prefix + getNumber();
        }

        private static int getNumber()
        {
            return Interlocked.Increment(ref _number);
        }

        ///<inheritdoc/>
        protected override void OnClose(CloseEventArgs e)
        {
        }

        ///<inheritdoc/>
        protected override void OnMessage(MessageEventArgs e)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(_OnMessage(e));
        }

        IEnumerator _OnMessage(MessageEventArgs e)
        {
            var res = UMI3DDto.FromBson(e.RawData);
            if (res is IdentityDto)
            {
                var req = res as IdentityDto;
                _id = req.userId;
                if (_id == null || _id == "") _id = genId();
                UMI3DCollaborationServer.Collaboration.CreateUser(req.login, OnUserCreated);
            }
            if (_id != null)
            {
                HandleMessage(res);
            }
            yield break;
        }


        protected virtual void OnUserCreated(UMI3DCollaborationUser user, bool reconnection)
        {
            _id = user.Id();
        }

        protected virtual void HandleMessage(UMI3DDto dto)
        {
        }

        ///<inheritdoc/>
        protected override void OnOpen()
        {
        }

        public void SendData(UMI3DDto obj, Action<bool> callback = null)
        {
            if (obj != null && this.Context.WebSocket.IsConnected)
            {
                var data = obj.ToBson();
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

        public string GetId()
        {
            return _id;
        }

        void IWebsocket.Send(byte[] content)
        {
            if(this.Context.WebSocket.IsConnected)
                SendAsync(content, (b) => { });
        }
    }
}