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
    /// Class to describe a transaction the 3D media is asking a browser to execute.
    /// </summary>
    [Serializable]
    public class TransactionDto : UMI3DDto
    {
        /// <summary>
        /// The list of operations to be executed by the device
        /// </summary>
        public List<AbstractOperationDto> operations = new List<AbstractOperationDto>();
    }
}