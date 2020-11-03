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
#if UNITY_WEBRTC
using Unity.WebRTC;
#endif

namespace umi3d.common.collaboration
{
    public class DataChannel
    {
        public string Label;
        public bool reliable;
        public DataType type;
        public Action OnOpen;
        public Action OnClose;
        public Action OnCreated;
        public Action<byte[]> OnMessage;
        public List<byte[]> MessageNotSend = new List<byte[]>();
        protected bool isOpen;

        public virtual bool IsOpen { get => isOpen; }

        public DataChannel(string label, bool reliable, DataType type, Action onCreated = null, Action onOpen = null, Action onClose = null)
        {
            Label = label;
            this.reliable = reliable;
            this.type = type;
            isOpen = false;
            OnOpen = onOpen;
            OnClose = onClose;
        }

        public DataChannel(DataChannel channel)
        {
            Label = channel.Label;
            reliable = channel.reliable;
            type = channel.type;
            isOpen = false;
        }

        public void Open() { OnOpen?.Invoke(); isOpen = true; }
        public void SendStack()
        {
            foreach (byte[] msg in MessageNotSend) Send(msg);
            MessageNotSend.Clear();
        }

        public virtual void Send(byte[] msg) { }

        public virtual void Close() { }
        public void Closed() { OnClose?.Invoke(); isOpen = false; }
        public void Created() { OnCreated?.Invoke(); }
        public void Messaged(byte[] data) { OnMessage?.Invoke(data); }
    }



    public class WebRTCDataChannel : DataChannel
    {
#if UNITY_WEBRTC
        public RTCDataChannel dataChannel;
        public override bool IsOpen { get => isOpen && dataChannel.ReadyState == RTCDataChannelState.Open; }
#endif
        public WebRTCDataChannel(DataChannel channel) : base(channel)
        {
        }

        public WebRTCDataChannel(string label, bool reliable, DataType type, Action onCreated = null, Action onOpen = null, Action onClose = null) : base(label, reliable, type, onCreated, onOpen, onClose)
        {
        }
#if UNITY_WEBRTC
        public override void Send(byte[] msg) { dataChannel.Send(msg); }
        public override void Close() { dataChannel.Close(); }
#endif
    }
}