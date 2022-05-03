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
using umi3d.common.collaboration;

namespace umi3d.edk.collaboration
{
    public class RedirectionRequest : DispatchableRequest
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Core | DebugScope.Networking;

        public RedirectionDto redirection;

        public RedirectionRequest(bool reliable, RedirectionDto redirection, HashSet<UMI3DUser> users = null) : base(reliable, users)
        {
            this.redirection = redirection;
        }

        protected virtual Bytable ToBytable()
        {
            return UMI3DNetworkingHelper.Write(UMI3DOperationKeys.RedirectionRequest)
                + UMI3DNetworkingHelper.Write(redirection);
        }

        public override byte[] ToBytes()
        {
            return ToBytable().ToBytes();
        }

        public override byte[] ToBson()
        {
            RedirectionDto dto = CreateDto();
            WriteProperties(dto);
            return dto.ToBson();
        }

        protected virtual RedirectionDto CreateDto() { return new RedirectionDto(); }
        protected virtual void WriteProperties(RedirectionDto dto) { dto.gate = redirection.gate; dto.media = redirection.media; }
    }
}