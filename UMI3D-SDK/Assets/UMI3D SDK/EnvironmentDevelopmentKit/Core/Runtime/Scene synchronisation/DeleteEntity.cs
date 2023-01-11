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
using System.Linq;
using umi3d.common;

namespace umi3d.edk
{
    /// <summary>
    /// <see cref="Operation"/> to send to request the deletion of an object on clients.
    /// </summary>
    public class DeleteEntity : Operation
    {
        /// <summary>
        /// UMI3D id of the entity to delete.
        /// </summary>
        public ulong entityId;

        /// <inheritdoc/>
        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DSerializer.Write(UMI3DOperationKeys.DeleteEntity)
                + UMI3DSerializer.Write(entityId);
        }

        /// <inheritdoc/>
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
            foreach (UMI3DUser u in b)
            {
                if (a.users.Contains(u)) a.users.Remove(u);
            }
            return a;
        }
    }
}