/*
Copyright 2019 - 2023 Inetum
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
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration.dto.emotes;

namespace umi3d.cdk.collaboration.emotes
{
    /// <summary>
    /// Loader for <see cref="UMI3DEmotesConfigDto"/>.
    /// </summary>
    public class UMI3DEmotesConfigLoader : AbstractLoader
    {
        private UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        #region DependencyInjection

        private IEmoteService emoteManagementService;
        private IEnvironmentManager environmentManager;

        public UMI3DEmotesConfigLoader()
        {
            emoteManagementService = EmoteManager.Instance;
            environmentManager = UMI3DEnvironmentLoader.Instance;
        }

        public UMI3DEmotesConfigLoader(IEmoteService emoteManager, IEnvironmentManager environmentManager)
        {
            emoteManagementService = emoteManager;
            this.environmentManager = environmentManager;
        }

        #endregion DependencyInjection

        /// <inheritdoc/>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DEmotesConfigDto;
        }

        /// <inheritdoc/>
        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as UMI3DEmotesConfigDto;
            environmentManager.RegisterEntity(dto.id, dto, null).NotifyLoaded();

            foreach (UMI3DEmoteDto emoteDto in dto.emotes)
                environmentManager.RegisterEntity(emoteDto.id, emoteDto, null).NotifyLoaded();

            emoteManagementService.UpdateEmoteConfig(dto);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (value.entity.dto is not UMI3DEmotesConfigDto dto)
                return await Task.FromResult(false);

            switch (value.property.property)
            {
                case UMI3DPropertyKeys.ChangeEmoteConfig:
                    dto.emotes = value.property.value as List<UMI3DEmoteDto>;
                    emoteManagementService.UpdateEmoteConfig(dto);
                    break;

                default:
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (value.entity.dto is not UMI3DEmotesConfigDto dto)
                return await Task.FromResult(false);

            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.ChangeEmoteConfig:
                    dto.emotes = UMI3DSerializer.Read<List<UMI3DEmoteDto>>(value.container);
                    emoteManagementService.UpdateEmoteConfig(dto);
                    break;

                default:
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        /// <inheritdoc/>
        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.ChangeEmoteConfig:
                    data.result = UMI3DSerializer.Read<UMI3DEmotesConfigDto>(data.container);
                    emoteManagementService.UpdateEmoteConfig(data.result as UMI3DEmotesConfigDto);
                    break;

                default:
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }
    }
}