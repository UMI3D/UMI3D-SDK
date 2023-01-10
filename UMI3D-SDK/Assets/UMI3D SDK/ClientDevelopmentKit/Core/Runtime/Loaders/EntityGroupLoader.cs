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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using umi3d.common;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="EntityGroupDto"/>.
    /// </summary>
    public class EntityGroupLoader : AbstractLoader
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is EntityGroupDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as EntityGroupDto;
            if(dto == null)
                throw (new Umi3dException("dto should be an EntityGroupDto"));
            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, null).NotifyLoaded();
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (value.entity != null && value.entity.dto is EntityGroupDto groupDto)
            {
                switch (value.property.property)
                {
                    case UMI3DPropertyKeys.EntityGroupIds:
                        UpdateEntities(value, groupDto);
                        break;
                    default:
                        foreach (ulong e in groupDto.entitiesId)
                        {
                            SetEntityPropertyDto np = value.property.Copy();
                            np.entityId = e;
                            await UMI3DEnvironmentLoader.SetEntity(np);
                        }
                        break;
                }
                return true;
            }
            return false;
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (value.entity != null && value.entity.dto is EntityGroupDto groupDto)
            {
                switch (value.propertyKey)
                {
                    case UMI3DPropertyKeys.EntityGroupIds:
                        UpdateEntities(value.entity, groupDto, value.operationId, value.propertyKey, value.container);
                        break;
                    default:
                        foreach (ulong e in groupDto.entitiesId)
                        {
                            await UMI3DEnvironmentLoader.SetEntity(value.operationId, e, value.propertyKey, new ByteContainer(value.container));
                        }
                        break;
                }
                return true;
            }
            return false;
        }

        private static void UpdateEntities(SetUMI3DPropertyData value, EntityGroupDto groupDto)
        {
            List<ulong> list = groupDto.entitiesId;
            switch (value.property)
            {
                case SetEntityListAddPropertyDto add:
                    if (add.index == list.Count())
                        list.Add((ulong)(long)add.value);
                    else if (add.index < list.Count() && add.index >= 0)
                        list.Insert(add.index, (ulong)(long)add.value);
                    else
                        UMI3DLogger.LogWarning($"Add value ignore for {add.index} in collection of size {list.Count}", scope);
                    break;
                case SetEntityListRemovePropertyDto rem:
                    if (rem.index < list.Count && rem.index >= 0)
                        list.RemoveAt(rem.index);
                    else
                        UMI3DLogger.LogWarning($"Remove value ignore for {rem.index} in collection of size {list.Count}", scope);
                    break;
                case SetEntityListPropertyDto set:
                    if (set.index < list.Count() && set.index >= 0)
                        list[set.index] = (ulong)(long)set.value;
                    else
                        UMI3DLogger.LogWarning($"Set value ignore for {set.index} in collection of size {list.Count}", scope);
                    break;
                default:
                    groupDto.entitiesId = (value.property.value as List<object>).Select(o => (ulong)(long)o).ToList();
                    break;
            }
        }

        private static void UpdateEntities(UMI3DEntityInstance entity, EntityGroupDto groupDto, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (groupDto.entitiesId == null)
                groupDto.entitiesId = new List<ulong>();
            UMI3DSerializer.ReadList(operationId, container, groupDto.entitiesId);
        }
    }
}