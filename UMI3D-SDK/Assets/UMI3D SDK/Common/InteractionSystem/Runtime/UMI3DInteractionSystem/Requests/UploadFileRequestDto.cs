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

namespace umi3d.common.interaction
{
    /// <summary>
    /// A class to describe the response from the client to the server when it wants to upload a file.
    /// </summary>
    [System.Serializable]
    public class UploadFileRequestDto : ParameterSettingRequestDto
    {
        /// <summary>
        /// File ID.
        /// </summary>
        public string fileId;

        /// <inheritdoc/>
        public override Bytable ToBytableArray(params object[] parameters)
        {
            return base.ToBytableArray(parameters)
                + UMI3DSerializer.Write(fileId);
        }
    }
}