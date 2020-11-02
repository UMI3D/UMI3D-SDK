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
using Unity.WebRTC;

namespace umi3d.common.collaboration
{
    public interface IWebRTCconnection
    {
        void AddDataChannel(DataChannel channel, bool instanciateChannel = true);
        void RemoveDataChannel(DataChannel channel);
        void AddIceCandidate(RTCIceCandidate candidate);
        void AddTracks();
        void Close();
        IEnumerator CreateAnswer(RTCSessionDescription description);

        void Init(string name, bool instanciateChannel);
        void Offer();
        void RemoveTracks();
        void Send(byte[] data, bool reliable, bool tryToSendAgain = true);
        void Send(byte[] data, bool reliable, DataType dataType, bool tryToSendAgain = true);
        void Send(byte[] data, DataChannel channel, bool tryToSendAgain = true);
        void Send(string text, bool reliable, bool tryToSendAgain = true);
        void SetRemoteSession(string sdp);
        bool Any(Func<DataChannel, bool> predicate);
        DataChannel Find(Func<DataChannel, bool> predicate);
        bool Find(bool reliable, DataType dataType, out DataChannel channel);
        DataChannel FirstOrDefault(Func<DataChannel, bool> predicate);
    }
}