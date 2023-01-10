/*
Copyright 2019 - 2022 Inetum
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
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.userCapture;
using static umi3d.cdk.UMI3DResourcesManager;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Loader for <see cref="UMI3DEmotesConfigDto"/>.
    /// </summary>
    public class UMI3DEmotesConfigLoader : AbstractLoader
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DEmotesConfigDto;
        }

        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as UMI3DEmotesConfigDto;
            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, null).NotifyLoaded();
            foreach (UMI3DEmoteDto emoteDto in dto.emotes)
                UMI3DEnvironmentLoader.RegisterEntityInstance(emoteDto.id, emoteDto, null).NotifyLoaded();
            UMI3DClientUserTracking.Instance.EmotesLoadedEvent.Invoke(dto);
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            var dto = value.entity.dto as UMI3DEmotesConfigDto;
            if (dto == null) return false;
            switch (value.property.property)
            {
                case UMI3DPropertyKeys.ChangeEmoteConfig:
                    dto.emotes = value.property.value as List<UMI3DEmoteDto>;
                    UMI3DClientUserTracking.Instance.EmotesLoadedEvent.Invoke(dto);
                    break;

                default:
                    return false;
            }
            return true;
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            var dto = value.entity.dto as UMI3DEmotesConfigDto;
            if (dto == null) return false;
            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.ChangeEmoteConfig:
                    dto.emotes = UMI3DSerializer.Read<List<UMI3DEmoteDto>>(value.container);
                    UMI3DClientUserTracking.Instance.EmotesLoadedEvent.Invoke(dto);
                    break;

                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Load an emote config
        /// </summary>
        /// <returns></returns>
        public override async Task< bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.ChangeEmoteConfig:
                    data.result = UMI3DSerializer.Read<UMI3DEmotesConfigDto>(data.container);

                    UMI3DClientUserTracking.Instance.EmotesLoadedEvent.Invoke(data.result as UMI3DEmotesConfigDto);
                    break;

                default:
                    return false;
            }
            return true;
        }
    }
}