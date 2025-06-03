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
namespace umi3d.common
{
    public class FileUploadProgressStatusRequestDto : AbstractBrowserRequestDto
    {

        public ulong requestId { get; set; }

        /// <summary>
        /// Entities to load id.
        /// </summary>
        public float progress { get; set; }

        /// <summary>
        /// Name of the file on the user os.
        /// </summary>
        public string fileName { get; set; }

        /// <summary>
        /// Human readable status of the upload
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// Size of the file uploaded in byte
        /// </summary>
        public int fileSize { get; set; }

        /// <summary>
        /// State if the upload is ether failed or succeeded.
        /// </summary>
        /// 
        public bool completed { get; set; }
        /// <summary>
        /// State, when completed is true, if the upload failed or succeeded.
        /// </summary>
        public bool succeeded { get; set; }

    }
}