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

        public static void ReadUMI3DExtension(ToolboxDto dto, GameObject node, Action finished, Action<Umi3dException> failed)
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
            var tool = entity?.Object as Toolbox;
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
                case UMI3DPropertyKeys.ToolActive:
                    dto.Active = (bool)property.value;
                    break;
                default:
                    return false;
            }
            return true;
        }

        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            var dto = (entity.dto as ToolboxDto);
            if (dto == null) return false;
            var tool = entity?.Object as Toolbox;
            if (tool == null) return false;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.ToolboxName:
                    dto.name = UMI3DNetworkingHelper.Read<string>(container);
                    break;
                case UMI3DPropertyKeys.ToolboxDescription:
                    dto.description = UMI3DNetworkingHelper.Read<string>(container);
                    break;
                case UMI3DPropertyKeys.ToolboxIcon2D:
                    dto.icon2D = UMI3DNetworkingHelper.Read<ResourceDto>(container);
                    break;
                case UMI3DPropertyKeys.ToolboxIcon3D:
                    dto.icon3D = UMI3DNetworkingHelper.Read<ResourceDto>(container);
                    break;
                case UMI3DPropertyKeys.ToolboxTools:
                    return SetTools(dto, tool, operationId, propertyKey, container);
                case UMI3DPropertyKeys.ToolActive:
                    dto.Active = UMI3DNetworkingHelper.Read<bool>(container); ;
                    break;
                default:
                    return false;
            }
            return true;
        }

        static public bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.ToolboxName:
                    value = UMI3DNetworkingHelper.Read<string>(container);
                    break;
                case UMI3DPropertyKeys.ToolboxDescription:
                    value = UMI3DNetworkingHelper.Read<string>(container);
                    break;
                case UMI3DPropertyKeys.ToolboxIcon2D:
                    value = UMI3DNetworkingHelper.Read<ResourceDto>(container);
                    break;
                case UMI3DPropertyKeys.ToolboxIcon3D:
                    value = UMI3DNetworkingHelper.Read<ResourceDto>(container);
                    break;
                case UMI3DPropertyKeys.ToolboxTools:
                    return SetTools(ref value, propertyKey, container);
                case UMI3DPropertyKeys.ToolActive:
                    value = UMI3DNetworkingHelper.Read<bool>(container); ;
                    break;
                default:
                    return false;
            }
            return true;
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

        static bool SetTools(ToolboxDto dto, Toolbox tool, uint operationId, uint propertyKey, ByteContainer container)
        {
            int index;
            ToolDto value;

            switch (operationId)
            {
                case UMI3DOperationKeys.SetEntityListAddProperty:
                    index = UMI3DNetworkingHelper.Read<int>(container);
                    value = UMI3DNetworkingHelper.Read<ToolDto>(container);
                    dto.tools.Add(value);
                    break;
                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    index = UMI3DNetworkingHelper.Read<int>(container);
                    if (index < dto.tools.Count)
                        dto.tools.RemoveAt(index);
                    else return false;
                    break;
                case UMI3DOperationKeys.SetEntityListProperty:
                    index = UMI3DNetworkingHelper.Read<int>(container);
                    value = UMI3DNetworkingHelper.Read<ToolDto>(container);
                    if (index < dto.tools.Count)
                        dto.tools[index] = (ToolDto)value;
                    else return false;
                    break;
                default:
                    dto.tools = UMI3DNetworkingHelper.ReadList<ToolDto>(container);
                    break;
            }
            return true;
        }

        static bool SetTools(ref object value, uint propertyKey, ByteContainer container)
        {
            value = UMI3DNetworkingHelper.ReadList<ToolDto>(container);
            return true;
        }

    }
}