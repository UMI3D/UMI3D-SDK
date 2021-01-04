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
using System.Collections.Generic;
#if UNITY_WEBRTC
using Unity.WebRTC;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.common.collaboration
{

    public interface IWebsocket
    {
        void Send(byte[] content);
    }

    public abstract class AbstractWebsocketRtcFactory : IAbstractWebRtcClient
    {

        public virtual void DeleteChannel(DataChannel dataChannel)
        {
            dataChannel.Close();
        }

        public virtual void HandleMessage(RTCDto dto)
        {
            switch (dto)
            {
                case OfferDto offer:
                    break;
                case AnswerDto answer:
                    break;
                case CandidateDto candidate:
                    break;
                case LeaveDto _:
                    break;
                case RTCConnectionDTO _:
                    break;
                case RTCCloseConnectionDto _:
                    break;
                case RTCDataChannelDto dcDto:
                    CreateChannel(dcDto.sourceUser, dcDto.reliable, dcDto.type);
                    break;
                default:
                    Debug.LogError("other :" + dto);
                    break;
            }
        }

        public abstract DataChannel CreateChannel(string user, bool reliable, DataType dataType);

        public void Init()
        {

        }

        public virtual void Clear()
        {

        }

        public virtual void Stop()
        {

        }
    }
}