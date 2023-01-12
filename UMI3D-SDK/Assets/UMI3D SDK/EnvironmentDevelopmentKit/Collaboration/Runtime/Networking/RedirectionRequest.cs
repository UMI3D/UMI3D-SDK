﻿/*
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
using umi3d.common.collaboration;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// Request to disconnect from an environment and connect to another.
    /// </summary>
    public class RedirectionRequest : Operation
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Core | DebugScope.Networking;

        public RedirectionDto redirection;

        public RedirectionRequest(RedirectionDto redirection)
        {
            this.redirection = redirection;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.RedirectionRequest)
                + UMI3DSerializer.Write(redirection);
        }



        protected virtual RedirectionDto CreateDto() { return new RedirectionDto(); }
        protected virtual void WriteProperties(RedirectionDto dto) { dto.gate = redirection.gate; dto.media = redirection.media; }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            RedirectionDto dto = CreateDto();
            WriteProperties(dto);
            return dto;
        }
    }
}