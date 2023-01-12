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
    /// <summary>
    /// Request from the server to get a locally stored data on a browser.
    /// </summary>
    /// It is similar to get a cookie on a traditional browser.
    public class GetLocalInfoRequest : Operation
    {
        /// <summary>
        /// Key of the locally stored data to access.
        /// </summary>
        public string key;

        public GetLocalInfoRequest(string key)
        {
            this.key = key;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.GetLocalInfoRequest)
                + UMI3DSerializer.Write(key);
        }

        protected virtual GetLocalInfoRequestDto CreateDto() { return new GetLocalInfoRequestDto(); }
        protected virtual void WriteProperties(GetLocalInfoRequestDto dto) { dto.key = key; }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            GetLocalInfoRequestDto dto = CreateDto();
            WriteProperties(dto);
            return dto;
        }
    }
}