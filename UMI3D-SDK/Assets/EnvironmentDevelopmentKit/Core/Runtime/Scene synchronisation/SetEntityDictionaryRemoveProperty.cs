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
    /// An operation to inform users the value of an entity's property changed.
    /// </summary>
    public class SetEntityDictionaryRemoveProperty : SetEntityDictionaryProperty
    {
        ///<inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            var setEntity = new SetEntityDictionaryRemovePropertyDto();
            setEntity.property = property;
            setEntity.entityId = entityId;
            setEntity.value = value;
            setEntity.key = key;
            return setEntity;
        }

        public static SetEntityDictionaryRemoveProperty operator +(SetEntityDictionaryRemoveProperty a, IEnumerable<UMI3DUser> b)
        {
            a.users = new HashSet<UMI3DUser>(a.users.Concat(b));
            return a;
        }
        public static SetEntityDictionaryRemoveProperty operator -(SetEntityDictionaryRemoveProperty a, IEnumerable<UMI3DUser> b)
        {
            foreach (var u in b)
            {
                if (a.users.Contains(u)) a.users.Remove(u);
            }
            return a;
        }

    }
}