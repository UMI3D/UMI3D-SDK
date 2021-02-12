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
using umi3d.common;
using umi3d.common.interaction;

namespace umi3d.cdk.interaction
{
    public static class UMI3DAbstractToolLoader
    {

        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var dto = (entity.dto as AbstractToolDto);
            if (dto == null) return false;
            var tool = entity.Object as AbstractTool;
            switch (property.property)
            {
                case UMI3DPropertyKeys.AbstractToolName:
                    dto.name = (string)property.value;
                    break;
                case UMI3DPropertyKeys.AbstractToolDescription:
                    dto.description = (string)property.value;
                    break;
                case UMI3DPropertyKeys.AbstractToolIcon2D:
                    dto.icon2D = (ResourceDto)property.value;
                    break;
                case UMI3DPropertyKeys.AbstractToolIcon3D:
                    dto.icon3D = (ResourceDto)property.value;
                    break;
                case UMI3DPropertyKeys.AbstractToolInteractions:
                    return SetInteractions(dto, tool, property);
                default:
                    return false;
            }
            return true;
        }

        static bool SetInteractions(AbstractToolDto dto, AbstractTool tool, SetEntityPropertyDto property)
        {
            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    dto.interactions.Add((AbstractInteractionDto)add.value);
                    break;
                case SetEntityListRemovePropertyDto rem:
                    if ((int)(Int64)rem.index < dto.interactions.Count)
                        dto.interactions.RemoveAt((int)(Int64)rem.index);
                    else return false;
                    break;
                case SetEntityListPropertyDto set:
                    if ((int)(Int64)set.index < dto.interactions.Count)
                        dto.interactions[(int)(Int64)set.index] = (AbstractInteractionDto)set.value;
                    else return false;
                    break;
                default:
                    dto.interactions = (List<AbstractInteractionDto>)property.value;
                    break;
            }
            tool.Updated();
            return true;
        }

    }
}