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
    /// Request to a client for uploading a file.
    /// </summary>
    /// This request is used to allow a browser to upload a file to the server.
    public class UploadFileRequest : Operation
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Core | DebugScope.Networking;

        /// <summary>
        /// Token for the upload.
        /// </summary>
        public string token;
        /// <summary>
        /// File to upload id.
        /// </summary>
        public string fileId;

        public UploadFileRequest(string fileId)
        {
            this.token = System.Guid.NewGuid().ToString();//.Replace('-','0');
            UMI3DLogger.LogWarning("token : " + this.token, scope);
            this.fileId = fileId;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.UploadFileRequest)
                + UMI3DSerializer.Write(token) + UMI3DSerializer.Write(fileId);
        }

        protected virtual RequestHttpUploadDto CreateDto() { return new RequestHttpUploadDto(); }
        protected virtual void WriteProperties(RequestHttpUploadDto dto) { dto.uploadToken = token; dto.fileId = fileId; }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            RequestHttpUploadDto dto = CreateDto();
            WriteProperties(dto);
            return dto;
        }
    }
}