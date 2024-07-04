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
using umi3d.common.interaction;

namespace umi3d.cdk.menu
{
    /// <summary>
    /// <see cref="MenuItem"/> to specify details on the upload input
    /// </summary>
    public class UploadInputMenuItem : TextInputMenuItem
    {
        /// <summary>
        /// Only these extensions could be upload by the client. Empty list = allow all, the extensions contain a dot (".obj" for exemple)
        /// </summary>
        public List<string> authorizedExtensions
        {
            get
            {
                return (dto as UploadFileParameterDto).authorizedExtensions;
            }
        }
    }
}
