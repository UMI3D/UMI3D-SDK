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
using System.Linq;
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// Request to ask a user to select one or more file and upload it to a given url
    /// </summary>
    public class UploadFileToServerRequest : Operation
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Core | DebugScope.Networking;

        /// <summary>
        /// url for the upload.
        /// </summary>
        public string url;
        public List<string> extensions;
        public bool allowMultipleFiles;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">upload url</param>
        /// <param name="extensions">extensions filters</param>
        /// <param name="allowMultipleFile">can the user upload multiple files to the url</param>
        public UploadFileToServerRequest(string url, IEnumerable<string> extensions, bool allowMultipleFile)
        {
            this.url = url;
            this.extensions = extensions.ToList();
            this.allowMultipleFiles = allowMultipleFile;
        }

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.UploadFileToUrlRequest)
                + UMI3DSerializer.Write(url) 
                + UMI3DSerializer.Write(extensions) 
                + UMI3DSerializer.Write(allowMultipleFiles);
        }

        protected virtual RequestHttpUploadToUrlDto CreateDto() { return new RequestHttpUploadToUrlDto(); }
        protected virtual void WriteProperties(RequestHttpUploadToUrlDto dto) {
            dto.url = url;
            dto.extensions = extensions;
            dto.allowMultipleFile = allowMultipleFiles;
        }

        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            RequestHttpUploadToUrlDto dto = CreateDto();
            WriteProperties(dto);
            return dto;
        }
    }
}