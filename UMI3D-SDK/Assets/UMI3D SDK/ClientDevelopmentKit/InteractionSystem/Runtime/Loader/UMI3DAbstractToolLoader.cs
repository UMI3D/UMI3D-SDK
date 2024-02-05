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
using System.ComponentModel;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    /// <summary>
    /// Helper class that manages the loading of <see cref="AbstractTool"/> entities.
    /// </summary>
    public class UMI3DAbstractToolLoader : AbstractLoader
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return false;
        }

        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            var dto = value.entity.dto as AbstractToolDto;
            if (dto == null) return false;
            var tool = value.entity.Object as AbstractTool;
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.AbstractToolName:
                    dto.name = (string)value.property.value;
                    break;
                case UMI3DPropertyKeys.AbstractToolDescription:
                    dto.description = (string)value.property.value;
                    break;
                case UMI3DPropertyKeys.AbstractToolIcon2D:
                    dto.icon2D = (ResourceDto)value.property.value;
                    break;
                case UMI3DPropertyKeys.AbstractToolIcon3D:
                    dto.icon3D = (ResourceDto)value.property.value;
                    break;
                case UMI3DPropertyKeys.AbstractToolInteractions:
                    return SetInteractions(dto, tool, value.property);
                case UMI3DPropertyKeys.AbstractToolActive:
                    dto.active = (bool)value.property.value;
                    break;
                default:
                    return false;
            }
            return true;
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            var dto = value.entity.dto as AbstractToolDto;
            if (dto == null) return false;
            var tool = value.entity.Object as AbstractTool;
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.AbstractToolName:
                    dto.name = UMI3DSerializer.Read<string>(value.container);
                    break;
                case UMI3DPropertyKeys.AbstractToolDescription:
                    dto.description = UMI3DSerializer.Read<string>(value.container);
                    break;
                case UMI3DPropertyKeys.AbstractToolIcon2D:
                    dto.icon2D = UMI3DSerializer.Read<ResourceDto>(value.container);
                    break;
                case UMI3DPropertyKeys.AbstractToolIcon3D:
                    dto.icon3D = UMI3DSerializer.Read<ResourceDto>(value.container);
                    break;
                case UMI3DPropertyKeys.AbstractToolInteractions:
                    return SetInteractions(dto, tool, value.operationId, value.propertyKey, value.container);
                case UMI3DPropertyKeys.AbstractToolActive:
                    dto.active = UMI3DSerializer.Read<bool>(value.container);
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Reads the value of an unknown <see cref="object"/> based on a received <see cref="ByteContainer"/> and updates it.
        /// <br/> Part of the bytes networking workflow.
        /// </summary>
        /// <param name="value">Unknown object</param>
        /// <param name="propertyKey">Property to update key in <see cref="UMI3DPropertyKeys"/></param>
        /// <param name="container">Received byte container</param>
        /// <returns>True if property setting was successful</returns>
        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.AbstractToolName:
                    data.result = UMI3DSerializer.Read<string>(data.container);
                    break;
                case UMI3DPropertyKeys.AbstractToolDescription:
                    data.result = UMI3DSerializer.Read<string>(data.container);
                    break;
                case UMI3DPropertyKeys.AbstractToolIcon2D:
                    data.result = UMI3DSerializer.Read<ResourceDto>(data.container);
                    break;
                case UMI3DPropertyKeys.AbstractToolIcon3D:
                    data.result = UMI3DSerializer.Read<ResourceDto>(data.container);
                    break;
                case UMI3DPropertyKeys.AbstractToolInteractions:
                    return ReadInteractions(data);
                case UMI3DPropertyKeys.AbstractToolActive:
                    data.result = UMI3DSerializer.Read<bool>(data.container);
                    break;
                default:
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Set the value of an <see cref="AbstractTool"/> based on a received <see cref="ByteContainer"/> and update it.
        /// <br/> Part of the bytes networking workflow.
        /// </summary>
        /// <param name="dto">Tool dto</param>
        /// <param name="tool">Tool</param>
        /// <param name="operationId">Operation id in <see cref="UMI3DOperationKeys"/></param>
        /// <param name="propertyKey">Property to update key in <see cref="UMI3DPropertyKeys"/></param>
        /// <param name="container">Received byte container</param>
        /// <returns>True if property setting was successful</returns>
        private static bool SetInteractions(AbstractToolDto dto, AbstractTool tool, uint operationId, uint propertyKey, ByteContainer container)
        {
            int index;
            ulong value;

            switch (operationId)
            {
                case UMI3DOperationKeys.SetEntityListAddProperty:
                    index = UMI3DSerializer.Read<int>(container);
                    value = UMI3DSerializer.Read<ulong>(container);
                    if (index == dto.interactions.Count)
                        dto.interactions.Add(value);
                    else if (index < dto.interactions.Count)
                        dto.interactions.Insert(index, value);
                    else return false;
                    tool.Added(UMI3DEnvironmentLoader.GetEntity(value).dto as AbstractInteractionDto); break;
                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    index = UMI3DSerializer.Read<int>(container);
                    if (index < dto.interactions.Count)
                    {
                        AbstractInteractionDto removed = UMI3DEnvironmentLoader.GetEntity(dto.interactions[index]).dto as AbstractInteractionDto;
                        dto.interactions.RemoveAt(index);
                        tool.Removed(removed);
                    }
                    else return false;
                    break;
                case UMI3DOperationKeys.SetEntityListProperty:
                    index = UMI3DSerializer.Read<int>(container);
                    value = UMI3DSerializer.Read<ulong>(container);
                    if (index < dto.interactions.Count)
                        dto.interactions[index] = value;
                    else return false;
                    tool.Updated();
                    break;
                default:
                    dto.interactions = UMI3DSerializer.ReadList<ulong>(container);
                    tool.Updated();
                    break;
            }
            return true;
        }

        /// <summary>
        /// Set the value of an <see cref="AbstractTool"/> based on a received <see cref="SetEntityPropertyDto"/> and update it.
        /// </summary>
        /// <param name="dto">Tool dto</param>
        /// <param name="tool">Tool</param>
        /// <param name="property">Received dto</param>
        /// <returns>True if property setting was successful</returns>
        private static bool SetInteractions(AbstractToolDto dto, AbstractTool tool, SetEntityPropertyDto property)
        {
            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    dto.interactions.Add((ulong)add.value);
                    break;
                case SetEntityListRemovePropertyDto rem:
                    if ((int)(Int64)rem.index < dto.interactions.Count)
                        dto.interactions.RemoveAt((int)(Int64)rem.index);
                    else return false;
                    break;
                case SetEntityListPropertyDto set:
                    if ((int)(Int64)set.index < dto.interactions.Count)
                        dto.interactions[(int)(Int64)set.index] = (ulong)set.value;
                    else return false;
                    break;
                default:
                    dto.interactions = (List<ulong>)property.value;
                    break;
            }
            tool.Updated();
            return true;
        }

        /// <summary>
        /// Read a received <see cref="ByteContainer"/>
        /// <br/> Part of the bytes networking workflow.
        /// </summary>
        /// <param name="value">Object</param>
        /// <param name="propertyKey">Property to update key in <see cref="UMI3DPropertyKeys"/></param>
        /// <param name="container">Received container</param>
        /// <returns>Always true</returns>
        private static bool ReadInteractions(ReadUMI3DPropertyData data)
        {
            data.result = UMI3DSerializer.ReadList<AbstractInteractionDto>(data.container);
            return true;
        }
    }
}