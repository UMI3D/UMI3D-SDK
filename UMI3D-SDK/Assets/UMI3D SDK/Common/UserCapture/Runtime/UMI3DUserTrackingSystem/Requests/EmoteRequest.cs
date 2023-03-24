/*
Copyright 2019 - 2022 Inetum

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

namespace umi3d.common.userCapture
{
    /// <summary>
    /// Request from a browser to trigger an emote for its user on user browsers.
    /// </summary>
    public class EmoteRequest : AbstractBrowserRequestDto
    {
        /// <summary>
        /// UMI3D id of the emote to trigger/interrupt.
        /// </summary>
        public ulong emoteId;
        /// <summary>
        /// If true, the emote sould be triggered. Otherwise, it should be interrupted.
        /// </summary>
        public bool shouldTrigger = true;
        /// <summary>
        /// User id of the user planning to trigger an emote.
        /// </summary>
        public ulong sendingUserId;
        protected override uint GetOperationId()
        {
            return UMI3DOperationKeys.EmoteRequest;
        }
        public override Bytable ToBytableArray(params object[] parameters)
        {
            return base.ToBytableArray(parameters)
            + UMI3DSerializer.Write(emoteId)
            + UMI3DSerializer.Write(shouldTrigger)
            + UMI3DSerializer.Write(sendingUserId);
        }
    }
}