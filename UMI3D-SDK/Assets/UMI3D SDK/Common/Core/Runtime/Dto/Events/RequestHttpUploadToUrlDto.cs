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

namespace umi3d.common
{
    public class RequestHttpUploadToUrlDto : AbstractOperationDto
    {
        /// <summary>
        /// url to upload the files to
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// Headers
        /// </summary>
        public List<HeaderContent> headers { get; set; }

        /// <summary>
        /// extensions filters
        /// </summary>
        public List<string> extensions { get; set; }

        /// <summary>
        /// Allow to upload multiple files
        /// </summary>
        public bool allowMultipleFile { get; set; }

    }

    public class HeaderContent
    {
        /// <summary>
        /// header name
        /// </summary>
        public string header { get; set; }

        /// <summary>
        /// header value
        /// </summary>
        public string content { get; set; }

    }

}