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
    public class LoadEntity : Operation
    {

        public UMI3DLoadableEntity entity;

        public override (int, Func<byte[], int, int>) ToBytes(UMI3DUser user)
        {
            //var entityFunc = entity.Id(user);
            var id = entity.Id();
            int size = sizeof(uint) + sizeof(ulong) /*entityFunc.Item1*/;
            Func<byte[], int, int> func = (b, i) => {
                i += UMI3DNetworkingHelper.Write(UMI3DOperationKeys.LoadEntity, b, i);
                i += UMI3DNetworkingHelper.Write(id, b, i);
                //i += entityFunc.Item2(b, i);
                return size;
            };
            return (size, func);
        }

        ///<inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new LoadEntityDto()
            {
                entity = entity.ToEntityDto(user),
            };
        }

        public static LoadEntity operator +(LoadEntity a, IEnumerable<UMI3DUser> b)
        {
            a.users = new HashSet<UMI3DUser>(a.users.Concat(b));
            return a;
        }

        public static LoadEntity operator -(LoadEntity a, IEnumerable<UMI3DUser> b)
        {
            foreach (var u in b)
            {
                if (a.users.Contains(u)) a.users.Remove(u);
            }
            return a;
        }
    }
}