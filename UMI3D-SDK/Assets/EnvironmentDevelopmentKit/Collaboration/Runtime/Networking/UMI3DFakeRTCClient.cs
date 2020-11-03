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
using System.Linq;
using umi3d.edk.collaboration;
using UnityEngine;

namespace umi3d.common.collaboration
{
    public class UMI3DFakeRTCClient : IWebRTCconnection
    {
        UMI3DFakeRTCConnection reliable = null;
        UMI3DFakeRTCConnection unreliable = null;
        List<FakeWebrtcMessageDto> StackReliable = new List<FakeWebrtcMessageDto>();
        List<FakeWebrtcMessageDto> StackUnreliable = new List<FakeWebrtcMessageDto>();
        public string name { get; private set; }
        public UMI3DFakeRTCConnection Reliable
        {
            get => reliable; set {
                reliable = value;
                foreach (var data in StackReliable)
                    Send(data);
                StackReliable.Clear();
            }
        }
        public UMI3DFakeRTCConnection Unreliable
        {
            get => unreliable; set {
                unreliable = value;
                foreach (var data in StackUnreliable)
                    Send(data);
                StackUnreliable.Clear();
            }
        }

        List<FakeDataChannel> channels = new List<FakeDataChannel>();

        public UMI3DFakeRTCClient(string name)
        {
            this.name = name;
        }

        public void AddDataChannel(DataChannel channel, bool instanciateChannel = true)
        {
            var ws = channel.reliable ? Reliable : Unreliable;
            channels.Add(new FakeDataChannel(channel, ws));
        }

        public bool Any(Func<DataChannel, bool> predicate)
        {
            return channels.Any(predicate);
        }

        public void Close()
        {
        }

        public DataChannel Find(Func<DataChannel, bool> predicate)
        {
            return channels.Find((c)=> predicate(c));
        }

        public bool Find(bool reliable, DataType dataType, out DataChannel channel)
        {
            channel = channels.FirstOrDefault((c) => c.reliable == reliable && dataType == c.type);
            return channel == null;
        }

        public DataChannel FirstOrDefault(Func<DataChannel, bool> predicate)
        {
            return channels.FirstOrDefault((c) => predicate(c));
        }

        public void Init(string name, bool instanciateChannel)
        {
            this.name = name;
        }

        public void Offer()
        {
            
        }

        public void RemoveDataChannel(DataChannel channel)
        {
            if(channel is FakeDataChannel fake)
                channels.Remove(fake);
        }

        public void Send(byte[] data, bool reliable, bool tryToSendAgain = true)
        {
            Send(data, reliable, DataType.Data, tryToSendAgain);
        }

        public void Send(byte[] data, bool reliable, DataType dataType, bool tryToSendAgain = true)
        {
            var dto = new FakeWebrtcMessageDto()
            {
                sourceId = UMI3DGlobalID.ServerId,
                content = data,
                dataType = dataType,
                reliable = reliable,
                targetId = new List<string>() { name }
            };
            Send(dto, tryToSendAgain);
        }

        public void Send(byte[] data, DataChannel channel, bool tryToSendAgain = true)
        {
            Send(data, channel.reliable, channel.type, tryToSendAgain);
        }

        public void Send(FakeWebrtcMessageDto dto, bool tryToSendAgain = true)
        {
            var ws = dto.reliable ? Reliable : Unreliable;
            if (ws != null)
                ws.SendData(dto.ToBson());
            else if(tryToSendAgain)
            {
                if (dto.reliable)
                    StackReliable.Add(dto);
                else
                    StackUnreliable.Add(dto);
            }
        }

    }
}