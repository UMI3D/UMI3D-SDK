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

using System;
using System.Collections.Generic;

namespace umi3d.common
{
    /// <summary>
    /// Describe a transaction from the 3D media to the browser, to ask one or more specific operation(s) to execute.
    /// </summary>
    [Serializable]
    public class TransactionDto : UMI3DDto
    {
        /// <summary>
        /// Operations to be executed by the device.
        /// </summary>
        public List<AbstractOperationDto> operations { get; set; } = new List<AbstractOperationDto>();
    }
}