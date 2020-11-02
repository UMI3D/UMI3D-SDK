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
using System.Collections.Generic;
using Unity.WebRTC;

namespace umi3d.common.collaboration
{
    public class DataChannel
    {
        public RTCDataChannel dataChannel;
        public string Label;
        public bool reliable;
        public DataType type;
        public Action OnOpen;
        public Action OnClose;
        public Action OnCreated;
        public Action<byte[]> OnMessage;
        public List<byte[]> MessageNotSend = new List<byte[]>();
        public bool IsOpen { get; private set; }

        public DataChannel(string label, bool reliable, DataType type, Action onCreated = null, Action onOpen = null, Action onClose = null)
        {
            Label = label;
            this.reliable = reliable;
            this.type = type;
            IsOpen = false;
            OnOpen = onOpen;
            OnClose = onClose;
        }

        public DataChannel(DataChannel channel)
        {
            Label = channel.Label;
            reliable = channel.reliable;
            type = channel.type;
            IsOpen = false;
        }

        public void Open() { OnOpen?.Invoke(); IsOpen = true; }
        public void SendStack()
        {
            foreach (byte[] msg in MessageNotSend) dataChannel.Send(msg);
            MessageNotSend.Clear();
        }

        public void Close() { OnClose?.Invoke(); IsOpen = false; }
        public void Created() { OnCreated?.Invoke(); }
        public void Message(byte[] data) { OnMessage?.Invoke(data); }
    }
}