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

        /// <summary>
        /// Scale of the user in the new referentiel. 
        /// Not to be misunderstood with the height.
        /// A user should be at full height with a scale of one.
        /// </summary>
        public float scale = 1;

        public FrameRequest(ulong FrameId = 0)
        {
            this.FrameId = FrameId;
        }

        public FrameRequest(ulong FrameId, float scale)
        {
            this.FrameId = FrameId;
            this.scale = scale;
        }

        public FrameRequest(float scale)
        {
            this.scale = scale;
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
        protected virtual void WriteProperties(FrameRequestDto dto) { dto.FrameId = FrameId; dto.scale = scale; }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(GetOperationKey())
                + UMI3DSerializer.Write(FrameId)
                + UMI3DSerializer.Write(scale);
        }
    }
}


