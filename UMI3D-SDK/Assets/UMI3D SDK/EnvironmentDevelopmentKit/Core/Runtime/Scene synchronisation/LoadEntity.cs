﻿/*
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
using System.Linq;
using umi3d.common;

namespace umi3d.edk
{
    public class LoadEntity : Operation
    {

        public List<UMI3DLoadableEntity> entities;

        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(UMI3DOperationKeys.LoadEntity)
                + UMI3DNetworkingHelper.Write(entities.Select((e) => e.Id()));
        }

        ///<inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new LoadEntityDto()
            {
                entities = entities.Select((e) => e.ToEntityDto(user)).ToList(),
            };
        }

        public static LoadEntity operator +(LoadEntity a, IEnumerable<UMI3DUser> b)
        {
            a.users = new HashSet<UMI3DUser>(a.users.Concat(b));
            return a;
        }

        public static LoadEntity operator +(LoadEntity a, LoadEntity b)
        {
            a.entities.AddRange(b.entities);
            return a + b.users;
        }

        public static LoadEntity operator -(LoadEntity a, LoadEntity b)
        {
            foreach (UMI3DLoadableEntity i in b.entities)
            {
                if (a.entities.Contains(i))
                    a.entities.Remove(i);
            }
            return a - b.users;
        }

        public static LoadEntity operator -(LoadEntity a, IEnumerable<UMI3DUser> b)
        {
            foreach (UMI3DUser u in b)
            {
                if (a.users.Contains(u)) a.users.Remove(u);
            }
            return a;
        }
    }
}