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
using System.Linq;
using umi3d.common;

namespace umi3d.edk
{
    public class MultiSetEntityProperty : Operation
    {

        /// <summary>
        /// The identifiers list of the entities
        /// </summary>
        public List<UMI3DMediaEntity> entityIds;

        /// <summary>
        /// The name of the modified property
        /// </summary>
        public ulong property;

        /// <summary>
        /// The new value for the property
        /// </summary>
        public object value;

        public override (int, Func<byte[], int, int>) ToBytes(UMI3DUser user)
        {
            var funcs = entityIds.Select(e => e.Id());
            uint count = (uint)entityIds.Count();


            int size =  (int) (3 * sizeof(uint) + count * sizeof(ulong) + UMI3DNetworkingHelper.GetSize(value));
            Func<byte[], int, int> func = (b, i) => {
                i += UMI3DNetworkingHelper.Write(UMI3DOperationKeys.MultiSetEntityProperty, b, i);
                i += UMI3DNetworkingHelper.Write(entityIds.Count(), b, i);
                i += UMI3DNetworkingHelper.WritteArray(funcs, b, i);
                i += UMI3DNetworkingHelper.Write(property, b, i);
                i += UMI3DNetworkingHelper.Write(value, b, i);
                return size;
            };
            return (size, func);
        }

        ///<inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new MultiSetEntityPropertyDto()
            {
                property = property,
                value = value,
                entityIds = entityIds.Select(e => e.Id()).ToList()
            };

        }
    }
}
