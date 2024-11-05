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
    public class CanUnmuteMicrophoneRequest : Operation
    {
        public bool canUnmute = false;


        public CanUnmuteMicrophoneRequest(bool canUnmute)
        {
            this.canUnmute = canUnmute;
        }

        /// <inheritdoc/>
        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.CanUnmuteMicrophoneRequest;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey())
                + UMI3DSerializer.Write(canUnmute);
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            CanUnmuteMicrophoneRequestDto dto = CreateDto();
            WriteProperties(dto);
            return dto;
        }

        protected virtual CanUnmuteMicrophoneRequestDto CreateDto() { return new CanUnmuteMicrophoneRequestDto(); }
        protected virtual void WriteProperties(CanUnmuteMicrophoneRequestDto dto) { dto.canUnmute = canUnmute; }

    }
}


