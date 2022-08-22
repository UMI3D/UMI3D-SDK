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

namespace umi3d.common
{
    public class ConferenceBrowserRequest : AbstractBrowserRequestDto
    {
        public uint operation;
        public bool value;
        public ulong id;

        protected ConferenceBrowserRequest(uint operation, ulong id, bool value)
        {
            this.operation = operation;
            this.value = value;
            this.id = id;
        }
        protected ConferenceBrowserRequest(uint operation)
        {
            this.operation = operation;
            this.value = false;
            this.id = 0;
        }

        public static ConferenceBrowserRequest GetChangeMicrophoneStatusRequest(ulong userId, bool value)
        {
            return new ConferenceBrowserRequest(UMI3DOperationKeys.UserMicrophoneStatus, userId, value);
        }
        public static ConferenceBrowserRequest GetChangeAvatarStatusRequest(ulong userId, bool value)
        {
            return new ConferenceBrowserRequest(UMI3DOperationKeys.UserAvatarStatus, userId, value);
        }
        public static ConferenceBrowserRequest GetChangeAttentionStatusRequest(ulong userId, bool value)
        {
            return new ConferenceBrowserRequest(UMI3DOperationKeys.UserAttentionStatus, userId, value);
        }

        public static ConferenceBrowserRequest GetMuteAllMicrophoneRequest()
        {
            return new ConferenceBrowserRequest(UMI3DOperationKeys.MuteAllMicrophoneStatus);
        }
        public static ConferenceBrowserRequest GetMuteAllAvatarRequest()
        {
            return new ConferenceBrowserRequest(UMI3DOperationKeys.MuteAllAvatarStatus);
        }
        public static ConferenceBrowserRequest GetMuteAllAttentionRequest()
        {
            return new ConferenceBrowserRequest(UMI3DOperationKeys.MuteAllAvatarStatus);
        }


        protected override uint GetOperationId() { return operation; }

        public override Bytable ToBytableArray(params object[] parameters)
        {
            if (operation != UMI3DOperationKeys.MuteAllAttentionStatus && operation != UMI3DOperationKeys.MuteAllAvatarStatus && operation != UMI3DOperationKeys.MuteAllMicrophoneStatus)
                return base.ToBytableArray(parameters)
                    + UMI3DNetworkingHelper.Write(id)
                    + UMI3DNetworkingHelper.Write(value);
            return base.ToBytableArray(parameters);
        }
    }
}