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

using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// Request to open or close a client microphone.
    /// The open request can be ignored according to user setting.
    /// </summary>
    public class MicrophoneStateRequest : Operation
    {
        public bool microphoneState = false;

        /// <summary>
        /// Create a MicrophoneStateRequest to open a microphone for a given user.
        /// Use with caution.
        /// This message can be ignore according to client setting
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static MicrophoneStateRequest OpenMicrophone(UMI3DUser user)
        {
            MicrophoneStateRequest request = new(true);
            request.users = new() { user };
            return request;
        }

        /// <summary>
        /// Create a MicrophoneStateRequest to close a microphone for a given user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static MicrophoneStateRequest CloseMicrophone(UMI3DUser user)
        {
            MicrophoneStateRequest request = new(false);
            request.users = new() { user };
            return request;
        }

        public static MicrophoneStateRequest CloseAllMicrophone()
        {
            MicrophoneStateRequest request = new(false);
            request.users = UMI3DServer.Instance.UserSetWhenHasJoined();
            return request;
        }

        public MicrophoneStateRequest(bool microphoneState)
        {
            this.microphoneState = microphoneState;
        }

        /// <inheritdoc/>
        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.MicrophoneRequest;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey())
                + UMI3DSerializer.Write(microphoneState);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            MicrophoneStatusRequestDto dto = CreateDto();
            WriteProperties(dto);
            return dto;
        }

        protected virtual MicrophoneStatusRequestDto CreateDto() { return new MicrophoneStatusRequestDto(); }
        protected virtual void WriteProperties(MicrophoneStatusRequestDto dto) { dto.status = microphoneState; }

    }
}


