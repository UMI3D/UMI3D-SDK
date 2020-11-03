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
using System.Collections;
using System.Collections.Generic;
using umi3d.edk.collaboration;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class UMI3DFakeRTCClient : IWebRTCconnection
    {
        public UMI3DFakeRTCConnection Reliable = null;
        public UMI3DFakeRTCConnection Unreliable = null;
        public string name { get; private set; }

        public UMI3DFakeRTCClient(string name)
        {
            this.name = name;
        }

        public void AddDataChannel(DataChannel channel, bool instanciateChannel = true)
        {

        }

        public bool Any(Func<DataChannel, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {

        }

        public DataChannel Find(Func<DataChannel, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public bool Find(bool reliable, DataType dataType, out DataChannel channel)
        {
            throw new NotImplementedException();
        }

        public DataChannel FirstOrDefault(Func<DataChannel, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public void Init(string name, bool instanciateChannel)
        {
           
        }

        public void Offer()
        {
            
        }

        public void RemoveDataChannel(DataChannel channel)
        {
            
        }

        public void Send(byte[] data, bool reliable, bool tryToSendAgain = true)
        {
            Send(data, reliable, DataType.Data, tryToSendAgain);
        }

        public void Send(byte[] data, bool reliable, DataType dataType, bool tryToSendAgain = true)
        {
            var ws = reliable ? Reliable : Unreliable;
            if(ws != null)
                ws.SendData(data);
        }

        public void Send(byte[] data, DataChannel channel, bool tryToSendAgain = true)
        {
            Send(data, channel.reliable, channel.type, tryToSendAgain);
        }
    }
}