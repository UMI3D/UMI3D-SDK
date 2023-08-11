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

using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration.dto.emotes;

namespace umi3d.cdk.collaboration.emotes
{
    /// <summary>
    /// Loader for <see cref="UMI3DEmoteDto"/>. Manages the dtos from the client-side.
    /// </summary>
    public class UMI3DEmoteLoader : AbstractLoader
    {
        private UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        #region DependencyInjection

        private IEmoteService emoteManagementService;
        private IEnvironmentManager environmentManager;

        public UMI3DEmoteLoader()
        {
            emoteManagementService = EmoteManager.Instance;
            environmentManager = UMI3DCollaborationEnvironmentLoader.Instance;
        }

        public UMI3DEmoteLoader(IEmoteService emoteManager, IEnvironmentManager environmentManager)
        {
            emoteManagementService = emoteManager;
            this.environmentManager = environmentManager;
        }

        #endregion DependencyInjection

        /// <inheritdoc/>
        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DEmoteDto;
        }

        /// <inheritdoc/>
        public override Task ReadUMI3DExtension(ReadUMI3DExtensionData value)
        {
            var dto = value.dto as UMI3DEmoteDto;
            environmentManager.RegisterEntity(dto.id, dto, null).NotifyLoaded();
            emoteManagementService.UpdateEmote(dto);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (value.entity.dto is not UMI3DEmoteDto dto)
                return await Task.FromResult(false);

            switch (value.property.property)
            {
                case UMI3DPropertyKeys.ActiveEmote:
                    dto.available = (bool)value.property.value;
                    emoteManagementService.UpdateEmote(dto);
                    break;

                case UMI3DPropertyKeys.AnimationEmote:
                    dto.animationId = (ulong)value.property.value;
                    emoteManagementService.UpdateEmote(dto);
                    break;

                default:
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (value.entity.dto is not UMI3DEmoteDto dto)
                return await Task.FromResult(false);

            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.ActiveEmote:
                    dto.available = UMI3DSerializer.Read<bool>(value.container);
                    emoteManagementService.UpdateEmote(dto);
                    break;

                case UMI3DPropertyKeys.AnimationEmote:
                    dto.animationId = UMI3DSerializer.Read<ulong>(value.container);
                    emoteManagementService.UpdateEmote(dto);
                    break;

                default:
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
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
                case UMI3DPropertyKeys.AnimationEmote:
                    data.result = UMI3DSerializer.Read<UMI3DEmoteDto>(data.container);
                    emoteManagementService.UpdateEmote(data.result as UMI3DEmoteDto);
                    break;

                default:
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }
    }
}