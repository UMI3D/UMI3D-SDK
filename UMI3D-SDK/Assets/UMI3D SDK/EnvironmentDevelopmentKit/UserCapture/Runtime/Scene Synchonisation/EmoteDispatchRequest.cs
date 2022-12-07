/*
Copyright 2019 - 2021 Inetum

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
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.userCapture;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// <see cref="DispatchableRequest"/> to trigger or interrupt emotes on other avatars.
    /// </summary>
    public class EmoteDispatchRequest : DispatchableRequest
    {
        /// <summary>
        /// UMI3D id of the emote to trigger/interrupt.
        /// </summary>
        public ulong emoteId;
        /// <summary>
        /// If true, the emote sould be triggered. Otherwise, it should be interrupted.
        /// </summary>
        public bool shouldTrigger;
        /// <summary>
        /// User id of the user planning to trigger an emote.
        /// </summary>
        public ulong sendingUserId;

        public EmoteDispatchRequest(bool reliable, HashSet<UMI3DUser> users) : base(reliable, users)
        {
        }

        /// <inheritdoc/>
        public override byte[] ToBson()
        {
            EmoteRequest dto = new EmoteRequest();
            dto.emoteId = emoteId;
            dto.shouldTrigger = shouldTrigger;
            dto.sendingUserId = sendingUserId;
            return dto.ToBson();
        }

        /// <inheritdoc/>
        public override byte[] ToBytes()
        {
            return (UMI3DNetworkingHelper.Write(UMI3DOperationKeys.EmoteRequest)
                + UMI3DNetworkingHelper.Write(emoteId)
                + UMI3DNetworkingHelper.Write(shouldTrigger)
                + UMI3DNetworkingHelper.Write(sendingUserId)).ToBytes();
        }
    }
}
