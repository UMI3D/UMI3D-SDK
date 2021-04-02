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
using UnityEngine;

namespace umi3d.cdk.interaction
{
    static public class UMI3DToolBoxLoader
    {

        public static void ReadUMI3DExtension(ToolboxDto dto, GameObject node, Action finished, Action<string> failed)
        {
            Toolbox toolbox = new Toolbox(dto);
            AbstractInteractionMapper.Instance?.CreateToolbox(toolbox);
            foreach (var tool in dto.tools)
            {
                UMI3DToolLoader.ReadUMI3DExtension(tool, toolbox);
            }
            finished?.Invoke();
        }

        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var dto = (entity.dto as ToolboxDto);
            if (dto == null) return false;
            var tool = UMI3DEnvironmentLoader.GetEntity(dto.id)?.Object as Toolbox;
            if (tool == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.ToolboxName:
                    dto.name = (string)property.value;
                    break;
                case UMI3DPropertyKeys.ToolboxDescription:
                    dto.description = (string)property.value;
                    break;
                case UMI3DPropertyKeys.ToolboxIcon2D:
                    dto.icon2D = (ResourceDto)property.value;
                    break;
                case UMI3DPropertyKeys.ToolboxIcon3D:
                    dto.icon3D = (ResourceDto)property.value;
                    break;
                case UMI3DPropertyKeys.ToolboxTools:
                    return SetTools(dto, tool, property);
                case UMI3DPropertyKeys.ToolBoxActive:
                    dto.Active = (bool)property.value;
                    break;
                default:
                    return false;
            }
            return false;
        }


        static bool SetTools(ToolboxDto dto, Toolbox tool, SetEntityPropertyDto property)
        {
            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    dto.tools.Add((ToolDto)add.value);
                    break;
                case SetEntityListRemovePropertyDto rem:
                    if ((int)(Int64)rem.index < dto.tools.Count)
                        dto.tools.RemoveAt((int)(Int64)rem.index);
                    else return false;
                    break;
                case SetEntityListPropertyDto set:
                    if ((int)(Int64)set.index < dto.tools.Count)
                        dto.tools[(int)(Int64)set.index] = (ToolDto)set.value;
                    else return false;
                    break;
                default:
                    dto.tools = (List<ToolDto>)property.value;
                    break;
            }
            return true;
        }

    }
}