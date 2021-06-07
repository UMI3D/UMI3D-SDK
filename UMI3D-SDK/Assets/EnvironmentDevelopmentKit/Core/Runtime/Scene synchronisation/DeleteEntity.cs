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
    public class DeleteEntity : Operation
    {

        public ulong entityId;

        public override (int, Func<byte[], int, int, (int, int)>) ToBytes(int baseSize, UMI3DUser user)
        {
            int size = baseSize + sizeof(uint) + sizeof(ulong);
            Func<byte[], int, int, (int, int)> func = (b, i, bs) => {
                bs += UMI3DNetworkingHelper.Write(UMI3DOperationKeys.DeleteEntity, b, ref i);
                bs += UMI3DNetworkingHelper.Write(entityId, b, ref i);
                return (i,bs);
            };
            return (size, func);
        }

        ///<inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new DeleteEntityDto() { entityId = entityId };
        }

        public static DeleteEntity operator +(DeleteEntity a, IEnumerable<UMI3DUser> b)
        {
            a.users = new HashSet<UMI3DUser>(a.users.Concat(b));
            return a;
        }

        public static DeleteEntity operator -(DeleteEntity a, IEnumerable<UMI3DUser> b)
        {
            foreach (var u in b)
            {
                if (a.users.Contains(u)) a.users.Remove(u);
            }
            return a;
        }

    }
}