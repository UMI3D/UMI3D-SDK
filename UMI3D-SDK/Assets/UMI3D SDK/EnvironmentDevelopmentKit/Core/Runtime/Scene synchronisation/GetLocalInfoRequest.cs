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

namespace umi3d.edk
{
    public class GetLocalInfoRequest : DispatchableRequest
    {
        public string key;

        public GetLocalInfoRequest(string key, bool reliable, HashSet<UMI3DUser> users = null) : base(reliable, users)
        {
            this.key = key;
        }

        protected virtual Bytable ToBytable()
        {
            return UMI3DNetworkingHelper.Write(UMI3DOperationKeys.GetLocalInfoRequest)
                + UMI3DNetworkingHelper.Write(key);
        }

        public override byte[] ToBytes()
        {
            return ToBytable().ToBytes();
        }

        public override byte[] ToBson()
        {
            var dto = CreateDto();
            WriteProperties(dto);
            return dto.ToBson();
        }

        protected virtual GetLocalInfoRequestDto CreateDto() { return new GetLocalInfoRequestDto(); }
        protected virtual void WriteProperties(GetLocalInfoRequestDto dto) { dto.key = key; }
    }
}