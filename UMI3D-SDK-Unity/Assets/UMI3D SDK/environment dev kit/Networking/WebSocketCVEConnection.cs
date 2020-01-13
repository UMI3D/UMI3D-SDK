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
using System.Threading;
using umi3d.common;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;


namespace umi3d.edk {

    public class WebSocketCVEConnection : WebSocketBehavior, IUMI3DRealtimeConnection
    {
        public string _id = null;
        private static int _number = 0;
        private string _prefix;

        public WebSocketCVEConnection()
            : this(null)
        {
        }

        public WebSocketCVEConnection(string prefix)
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

        //on user quit
        protected override void OnClose(CloseEventArgs e)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(UMI3D.UserManager.OnRealtimeConnectionClose(_id));
        }

        //on user send message
        protected override void OnMessage(MessageEventArgs e)
        {
            var res = DtoUtility.Deserialize(e.Data);
            if(res is RealtimeConnectionRequestDto)
            {
                var req = res as RealtimeConnectionRequestDto;
                _id = req.UserId;
                UMI3DMainThreadDispatcher.Instance.Enqueue(UMI3D.UserManager.OnConnection(_id, this));
            } 
            if( _id != null)
                UMI3DMainThreadDispatcher.Instance.Enqueue(UMI3D.UserManager.OnMessage(_id, res));
        }
        
        //on user connect
        protected override void OnOpen()
        {
            //_id = genId();
            //UnityMainThreadDispatcher.Instance().Enqueue(UMI3D.UserManager.OnConnection(this));
        }

        public void SendData(UMI3DDto obj)
        {
            if (obj != null && this.Context.WebSocket.IsConnected)
            {
                var data = DtoUtility.Serialize(obj);
                try
                {
                    SendAsync(data, (bool completed) => { });
                }
                catch(InvalidOperationException exp)
                {
                    Debug.LogWarning(exp);
                    UnityMainThreadDispatcher.Instance().Enqueue(UMI3D.UserManager.OnRealtimeConnectionClose(_id));
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