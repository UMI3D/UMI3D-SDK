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
    /// An operation to inform a browser the value of an entity's property changed.
    /// </summary>
    [Serializable]
    public class MultiSetEntityPropertyDto : AbstractOperationDto
    {
        /// <summary>
        /// The identifiers list of the entities
        /// </summary>
        public List<string> entityIds;

        /// <summary>
        /// The name of the modified property
        /// </summary>
        public string property;

        /// <summary>
        /// The new value for the property
        /// </summary>
        public object value;
    }
}
