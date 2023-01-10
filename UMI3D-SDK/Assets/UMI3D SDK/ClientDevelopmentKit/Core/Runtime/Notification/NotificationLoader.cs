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
using System.ComponentModel;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="NotificationDto"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultNotificationLoader", menuName = "UMI3D/Default Notification Loader")]
    public class NotificationLoader : ScriptableObject
    {
        public virtual AbstractLoader GetNotificationLoader()
        {
            return new InternalNotificationLoader();
        }
    }

    public class InternalNotificationLoader : AbstractLoader
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is NotificationDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as NotificationDto;
            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, null).NotifyLoaded();
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            var dto = value.entity.dto as NotificationDto;
            if (dto == null) return false;
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.NotificationTitle:
                    dto.title = (string)value.property.value;
                    break;
                case UMI3DPropertyKeys.NotificationContent:
                    dto.content = (string)value.property.value;
                    break;
                case UMI3DPropertyKeys.NotificationDuration:
                    dto.duration = (float)(Double)value.property.value;
                    break;
                case UMI3DPropertyKeys.NotificationIcon2D:
                    dto.icon2D = (ResourceDto)value.property.value;
                    break;
                case UMI3DPropertyKeys.NotificationIcon3D:
                    dto.icon3D = (ResourceDto)value.property.value;
                    break;
                case UMI3DPropertyKeys.NotificationObjectId:
                    var Odto = dto as NotificationOnObjectDto;
                    if (Odto == null) return false;
                    Odto.objectId = (ulong)(long)value.property.value;
                    break;
                default:
                    return false;
            }
            return true;
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            var dto = value.entity.dto as NotificationDto;
            if (dto == null) return false;
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.NotificationTitle:
                    dto.title = UMI3DSerializer.Read<string>(value.container);
                    break;
                case UMI3DPropertyKeys.NotificationContent:
                    dto.content = UMI3DSerializer.Read<string>(value.container);
                    break;
                case UMI3DPropertyKeys.NotificationDuration:
                    dto.duration = UMI3DSerializer.Read<float>(value.container);
                    break;
                case UMI3DPropertyKeys.NotificationIcon2D:
                    dto.icon2D = UMI3DSerializer.Read<ResourceDto>(value.container);
                    break;
                case UMI3DPropertyKeys.NotificationIcon3D:
                    dto.icon3D = UMI3DSerializer.Read<ResourceDto>(value.container);
                    break;
                case UMI3DPropertyKeys.NotificationObjectId:
                    var Odto = dto as NotificationOnObjectDto;
                    if (Odto == null) return false;
                    Odto.objectId = UMI3DSerializer.Read<ulong>(value.container);
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Update a property directly.
        /// </summary>
        /// <param name="value">Value extracted as output.</param>
        /// <param name="propertyKey">UMI3D key of the property to update.</param>
        /// <param name="container">New value in a container.</param>
        /// <returns></returns>
        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData value)
        {
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.NotificationTitle:
                    value.result = UMI3DSerializer.Read<string>(value.container);
                    break;
                case UMI3DPropertyKeys.NotificationContent:
                    value.result = UMI3DSerializer.Read<string>(value.container);
                    break;
                case UMI3DPropertyKeys.NotificationDuration:
                    value.result = UMI3DSerializer.Read<float>(value.container);
                    break;
                case UMI3DPropertyKeys.NotificationIcon2D:
                    value.result = UMI3DSerializer.Read<ResourceDto>(value.container);
                    break;
                case UMI3DPropertyKeys.NotificationIcon3D:
                    value.result = UMI3DSerializer.Read<ResourceDto>(value.container);
                    break;
                case UMI3DPropertyKeys.NotificationObjectId:
                    value.result = UMI3DSerializer.Read<ulong>(value.container);
                    break;
                default:
                    return false;
            }
            return true;
        }
    }
}