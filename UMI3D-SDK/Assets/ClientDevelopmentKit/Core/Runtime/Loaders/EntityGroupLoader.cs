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
    static public class EntityGroupLoader
    {
        static public void ReadUMI3DExtension(EntityGroupDto groupDto)
        {
            groupDto.entitiesId = groupDto.entitiesId.ToList();
            UMI3DEnvironmentLoader.RegisterEntityInstance(groupDto.id, groupDto, null);
        }

        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (entity != null && entity.dto is EntityGroupDto groupDto)
            {
                switch (property.property)
                {
                    case UMI3DPropertyKeys.EntityGroupIds:
                        UpdateEntities(entity, groupDto, property);
                        break;
                    default:
                        foreach (var e in groupDto.entitiesId)
                        {
                            var np = property.Copy();
                            np.entityId = e;
                            UMI3DEnvironmentLoader.SetEntity(np);
                        }
                        break;
                }
                return true;
            }
            return false;
        }

        static void UpdateEntities(UMI3DEntityInstance entity, EntityGroupDto groupDto, SetEntityPropertyDto property)
        {
            var list = groupDto.entitiesId;
            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    if (add.index == list.Count())
                        list.Add(add.value as string);
                    else if (add.index < list.Count() && add.index >= 0)
                        list.Insert(add.index, add.value as string);
                    else
                        Debug.LogWarning($"Add value ignore for {add.index} in collection of size {list.Count}");
                    break;
                case SetEntityListRemovePropertyDto rem:
                    if (rem.index < list.Count && rem.index >= 0)
                        list.RemoveAt(rem.index);
                    else
                        Debug.LogWarning($"Remove value ignore for {rem.index} in collection of size {list.Count}");
                    break;
                case SetEntityListPropertyDto set:
                    if (set.index < list.Count() && set.index >= 0)
                        list[set.index] = set.value as string;
                    else
                        Debug.LogWarning($"Set value ignore for {set.index} in collection of size {list.Count}");
                    break;
                default:
                    groupDto.entitiesId = property.value as List<string>;
                    break;
            }
        }

    }
}