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

using umi3d.common;
using umi3d.common.interaction;

namespace umi3d.cdk.interaction
{
    static public class UMI3DToolLoader
    {
        static public void ReadUMI3DExtension(ToolDto dto, Toolbox toolbox)
        {
            Tool tool = new Tool(dto, toolbox);
            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, tool, tool.Destroy);
            AbstractInteractionMapper.Instance.CreateTool(tool);
        }

        static public void ReadUMI3DExtension(ToolDto dto)
        {
            Tool tool = new Tool(dto, null);
            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, tool, tool.Destroy);
            AbstractInteractionMapper.Instance.CreateTool(tool);
        }


        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            return (UMI3DAbstractToolLoader.SetUMI3DProperty(entity, property));
        }

        static public bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            return (UMI3DAbstractToolLoader.SetUMI3DProperty(entity, operationId, propertyKey, container));
        }

        static public bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            return (UMI3DAbstractToolLoader.ReadUMI3DProperty(ref value, propertyKey, container));
        }
    }
}