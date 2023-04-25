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
using UnityEngine;
using UnityEngine.UIElements;

namespace umi3d.edk
{
    /// <summary>
    /// Request relative to vehicle boarding.
    /// </summary>
    public class FrameRequest : Operation
    {
        /// <summary>
        /// Node Id of the Frame.
        /// </summary>
        public ulong FrameId = 0;

        public FrameRequest(ulong FrameId = 0)
        {
            this.FrameId = FrameId;
        }

        /// <inheritdoc/>
        protected virtual uint GetOperationKey()
        {
            return UMI3DOperationKeys.FrameRequest;
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            FrameRequestDto dto = CreateDto();
            WriteProperties(dto);
            return dto;
        }

        protected virtual FrameRequestDto CreateDto() { return new FrameRequestDto(); }
        protected virtual void WriteProperties(FrameRequestDto dto) { dto.FrameId = FrameId; }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey())
                + UMI3DSerializer.Write(FrameId);
        }
    }
}


