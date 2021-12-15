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
using UnityEngine;

namespace umi3d.cdk
{
    public static class EntityGroupLoader
    {
        const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        public static void ReadUMI3DExtension(EntityGroupDto groupDto)
        {
            groupDto.entitiesId = groupDto.entitiesId.ToList();
            UMI3DEnvironmentLoader.RegisterEntityInstance(groupDto.id, groupDto, null).NotifyLoaded();
        }

        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (entity != null && entity.dto is EntityGroupDto groupDto)
            {
                switch (property.property)
                {
                    case UMI3DPropertyKeys.EntityGroupIds:
                        UpdateEntities(entity, groupDto, property);
                        break;
                    default:
                        foreach (ulong e in groupDto.entitiesId)
                        {
                            SetEntityPropertyDto np = property.Copy();
                            np.entityId = e;
                            UMI3DEnvironmentLoader.SetEntity(np);
                        }
                        break;
                }
                return true;
            }
            return false;
        }


        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (entity != null && entity.dto is EntityGroupDto groupDto)
            {
                switch (propertyKey)
                {
                    case UMI3DPropertyKeys.EntityGroupIds:
                        UpdateEntities(entity, groupDto, operationId, propertyKey, container);
                        break;
                    default:
                        foreach (ulong e in groupDto.entitiesId)
                        {
                            UMI3DEnvironmentLoader.SetEntity(operationId, e, propertyKey, container);
                        }
                        break;
                }
                return true;
            }
            return false;
        }

        public static bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            return false;
        }

        private static void UpdateEntities(UMI3DEntityInstance entity, EntityGroupDto groupDto, SetEntityPropertyDto property)
        {
            List<ulong> list = groupDto.entitiesId;
            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    if (add.index == list.Count())
                        list.Add((ulong)(long)add.value);
                    else if (add.index < list.Count() && add.index >= 0)
                        list.Insert(add.index, (ulong)(long)add.value);
                    else
                        UMI3DLogger.LogWarning($"Add value ignore for {add.index} in collection of size {list.Count}",scope);
                    break;
                case SetEntityListRemovePropertyDto rem:
                    if (rem.index < list.Count && rem.index >= 0)
                        list.RemoveAt(rem.index);
                    else
                        UMI3DLogger.LogWarning($"Remove value ignore for {rem.index} in collection of size {list.Count}",scope);
                    break;
                case SetEntityListPropertyDto set:
                    if (set.index < list.Count() && set.index >= 0)
                        list[set.index] = (ulong)(long)set.value;
                    else
                        UMI3DLogger.LogWarning($"Set value ignore for {set.index} in collection of size {list.Count}",scope);
                    break;
                default:
                    groupDto.entitiesId = (property.value as List<object>).Select(o => (ulong)(long)o).ToList();
                    break;
            }
        }

        private static void UpdateEntities(UMI3DEntityInstance entity, EntityGroupDto groupDto, uint operationId, uint propertyKey, ByteContainer container)
        {
            int index;
            ulong value;

            List<ulong> list = groupDto.entitiesId;
            switch (operationId)
            {
                case UMI3DOperationKeys.SetEntityListAddProperty:
                    index = UMI3DNetworkingHelper.Read<int>(container);
                    value = UMI3DNetworkingHelper.Read<ulong>(container);

                    if (index == list.Count())
                        list.Add(value);
                    else if (index < list.Count() && index >= 0)
                        list.Insert(index, value);
                    else
                        UMI3DLogger.LogWarning($"Add value ignore for {index} in collection of size {list.Count}",scope);
                    break;
                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    index = UMI3DNetworkingHelper.Read<int>(container);

                    if (index < list.Count && index >= 0)
                        list.RemoveAt(index);
                    else
                        UMI3DLogger.LogWarning($"Remove value ignore for {index} in collection of size {list.Count}",scope);
                    break;
                case UMI3DOperationKeys.SetEntityListProperty:
                    index = UMI3DNetworkingHelper.Read<int>(container);
                    value = UMI3DNetworkingHelper.Read<ulong>(container);

                    if (index < list.Count() && index >= 0)
                        list[index] = value;
                    else
                        UMI3DLogger.LogWarning($"Set value ignore for {index} in collection of size {list.Count}",scope);
                    break;
                default:
                    groupDto.entitiesId = UMI3DNetworkingHelper.ReadList<ulong>(container);
                    break;
            }
        }

    }
}