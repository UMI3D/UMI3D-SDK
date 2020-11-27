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
using UnityEngine;
using WebSocketSharp;
#if UNITY_WEBRTC
using Unity.WebRTC;
#endif

namespace umi3d.common.collaboration
{
    public enum ChannelState
    {
        Opening,
        Open,
        Close
    }

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
        protected ChannelState state;

        public ChannelState State
        {
            get {
                CheckState();
                return state;
            }
        }

        protected virtual void CheckState() { }

        public DataChannel(string label, bool reliable, DataType type, Action onCreated = null, Action onOpen = null, Action onClose = null)
        {
            Label = label;
            this.reliable = reliable;
            this.type = type;
            state = ChannelState.Opening;
            OnOpen = onOpen;
            OnClose = onClose;
        }

        public DataChannel(DataChannel channel)
        {
            Label = channel.Label;
            reliable = channel.reliable;
            type = channel.type;
            state = ChannelState.Opening;
        }

        public void Open() { OnOpen?.Invoke(); state = ChannelState.Open; }
        public void SendStack()
        {
            foreach (byte[] msg in MessageNotSend) Send(msg);
            MessageNotSend.Clear();
        }

        public virtual void Send(byte[] msg) { }

        public virtual void Close() { }
        public void Closed() { OnClose?.Invoke(); state = ChannelState.Close; }
        public void Created() { OnCreated?.Invoke(); }
        public void Messaged(byte[] data) { OnMessage?.Invoke(data); }
    }



    public class WebRTCDataChannel : DataChannel
    {
        public AbstractWebsocket socket;
        public string id;
        public List<string> target;
        public bool useWebrtc = true;

#if UNITY_WEBRTC
        public RTCDataChannel dataChannel;
#endif
        ///<inheritdoc/>
        protected override void CheckState()
        {
#if UNITY_WEBRTC
            if (useWebrtc)
                if (dataChannel != null)
                {
                    switch (dataChannel.ReadyState)
                    {
                        case RTCDataChannelState.Connecting:
                            state = ChannelState.Opening;
                            break;
                        case RTCDataChannelState.Open:
                            state = ChannelState.Open;
                            break;
                        case RTCDataChannelState.Closing:
                        case RTCDataChannelState.Closed:
                            if (state != ChannelState.Close)
                                Close();
                            state = ChannelState.Close;
                            break;
                    }
                }
                else
#endif
            if (socket != null)
                    state = ChannelState.Open;
            else state = ChannelState.Opening;
        }

        public WebRTCDataChannel(string id,string target, DataChannel channel) : base(channel)
        {
            this.id = id;
            this.target = new List<string>() { target };
        }

        public WebRTCDataChannel(string id, string target, string label, bool reliable, DataType type, Action onCreated = null, Action onOpen = null, Action onClose = null) : base(label, reliable, type, onCreated, onOpen, onClose)
        {
            this.id = id;
            this.target = new List<string>() { target };
        }


        ///<inheritdoc/>
        public override void Send(byte[] msg) {
            if(State == ChannelState.Open)
#if UNITY_WEBRTC
                if(useWebrtc)
                    dataChannel.Send(msg);
            else
#endif
                    socketSend(msg);

        }

        ///<inheritdoc/>
        public override void Close() {
#if UNITY_WEBRTC
            Debug.Log("close"); dataChannel.Close();
#endif
        }

        void socketSend(byte[] msg)
        {
            var fake = new FakeWebrtcMessageDto
            {
                content = msg,
                dataType = type,
                reliable = reliable,
                sourceId = id,
                targetId = target
            };
            socket?.Send(fake.ToBson());
        }


    }
}