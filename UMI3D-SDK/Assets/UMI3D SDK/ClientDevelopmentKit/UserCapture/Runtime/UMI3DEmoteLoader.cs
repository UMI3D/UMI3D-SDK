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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Loader for <see cref="UMI3DEmoteDto"/>. Manages the dtos from the client-side.
    /// </summary>
    public class UMI3DEmoteLoader : AbstractLoader
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DEmoteDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as UMI3DEmoteDto;
            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, null).NotifyLoaded();
            UMI3DClientUserTracking.Instance.EmoteChangedEvent.Invoke(dto);
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            var dto = value.entity.dto as UMI3DEmoteDto;
            if (dto == null) return false;
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.ActiveEmote:
                    dto.available = (bool)value.property.value;
                    UMI3DClientUserTracking.Instance.EmoteChangedEvent.Invoke(dto);
                    break;

                default:
                    return false;
            }
            return true;
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            var dto = value.entity.dto as UMI3DEmoteDto;
            if (dto == null) return false;
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.ActiveEmote:
                    dto.available = UMI3DSerializer.Read<bool>(value.container);
                    UMI3DClientUserTracking.Instance.EmoteChangedEvent.Invoke(dto);
                    break;

                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Load an emote
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyKey"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.ActiveEmote:
                    data.result = UMI3DSerializer.Read<UMI3DEmoteDto>(data.container);

                    UMI3DClientUserTracking.Instance.EmoteChangedEvent.Invoke(data.result as UMI3DEmoteDto);
                    break;

                default:
                    return false;
            }
            return true;
        }
    }
}