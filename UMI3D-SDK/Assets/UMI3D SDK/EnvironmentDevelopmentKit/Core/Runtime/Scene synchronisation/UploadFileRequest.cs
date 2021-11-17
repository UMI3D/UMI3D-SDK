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
    public class UploadFileRequest : DispatchableRequest
    {
        const DebugScope scope = DebugScope.EDK | DebugScope.Core | DebugScope.Networking;

        public string token;
        public string fileId;

        public UploadFileRequest(bool reliable, string fileId, HashSet<UMI3DUser> users = null) : base(reliable, users)
        {
            this.token = System.Guid.NewGuid().ToString();//.Replace('-','0');
            UMI3DLogger.LogWarning("token : " + this.token,scope);
            this.fileId = fileId;
        }

        protected virtual Bytable ToBytable()
        {
            return UMI3DNetworkingHelper.Write(UMI3DOperationKeys.UploadFileRequest)
                + UMI3DNetworkingHelper.Write(token) + UMI3DNetworkingHelper.Write(fileId);
        }

        public override byte[] ToBytes()
        {
            return ToBytable().ToBytes();
        }

        public override byte[] ToBson()
        {
            RequestHttpUploadDto dto = CreateDto();
            WriteProperties(dto);
            return dto.ToBson();
        }

        protected virtual RequestHttpUploadDto CreateDto() { return new RequestHttpUploadDto(); }
        protected virtual void WriteProperties(RequestHttpUploadDto dto) { dto.uploadToken = token; dto.fileId = fileId; }
    }
}