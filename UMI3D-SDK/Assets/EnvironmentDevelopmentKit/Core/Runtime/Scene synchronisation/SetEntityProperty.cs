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
    /// <summary>
    /// An operation to inform users the value of an entity's property changed.
    /// </summary>
    public class SetEntityProperty : Operation
    {
        /// <summary>
        /// The unique identifier of the entity
        /// </summary>
        public ulong entityId;

        /// <summary>
        /// The name of the modified property
        /// </summary>
        public uint property;

        /// <summary>
        /// The new value for the property
        /// </summary>
        public object value;

        public override (int, Func<byte[], int, int, (int, int)>) ToBytes(int baseSize, UMI3DUser user)
        {
            int size = baseSize + sizeof(uint) + sizeof(ulong) + sizeof(uint) + UMI3DNetworkingHelper.GetSize(value);
            Func<byte[], int, int, (int, int)> func = (b, i, bs) => {
                bs += UMI3DNetworkingHelper.Write(UMI3DOperationKeys.SetEntityProperty, b, ref i);
                bs += UMI3DNetworkingHelper.Write(entityId, b, ref i);
                bs += UMI3DNetworkingHelper.Write(property, b, ref i);
                bs += UMI3DNetworkingHelper.Write(value, b, ref i);
                return (i,bs);
            };
            return (size, func);
        }

        ///<inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            var setEntity = new SetEntityPropertyDto();
            setEntity.property = property;
            setEntity.entityId = entityId;
            setEntity.value = value;
            return setEntity;
        }

        public static SetEntityProperty operator +(SetEntityProperty a, IEnumerable<UMI3DUser> b)
        {
            a.users = new HashSet<UMI3DUser>(a.users.Concat(b));
            return a;
        }
        public static SetEntityProperty operator -(SetEntityProperty a, IEnumerable<UMI3DUser> b)
        {
            foreach (var u in b)
            {
                if (a.users.Contains(u)) a.users.Remove(u);
            }
            return a;
        }

    }
}