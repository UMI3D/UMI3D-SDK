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
using UnityEngine;

namespace umi3d.cdk.collaboration.emotes
{
    /// <summary>
    /// Loader for <see cref="UMI3DEmoteDto"/>. Manages the dtos from the client-side.
    /// </summary>
    public class UMI3DEmoteLoader : AbstractLoader<UMI3DEmoteDto, Emote>
    {
        private UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Loading;

        #region DependencyInjection

        private readonly IEmoteService emoteManagementService;
        private readonly IEnvironmentManager environmentManager;

        public UMI3DEmoteLoader() : this(emoteManagementService : EmoteManager.Instance,
                                        environmentManager : UMI3DCollaborationEnvironmentLoader.Instance)
        {
        }

        public UMI3DEmoteLoader(IEmoteService emoteManagementService, IEnvironmentManager environmentManager)
        {
            this.emoteManagementService = emoteManagementService;
            this.environmentManager = environmentManager;
        }

        #endregion DependencyInjection

        public override async Task<Emote> Load(ulong environmnetId, UMI3DEmoteDto dto)
        {
            Sprite emoteIcon;

            if (dto.iconResource is not null
                && dto.iconResource.variants.Count > 0
                && dto.iconResource.variants[0].metrics.size != 0)
            {
                emoteIcon = await LoadIcon(dto);
            }
            else
            {
                emoteIcon = emoteManagementService.DefaultIcon;
            }

            Emote emote = new Emote(dto, emoteIcon);

            environmentManager.RegisterEntity(environmnetId, dto.id, dto, emote).NotifyLoaded();

            return emote;
        }

        private async Task<Sprite> LoadIcon(UMI3DEmoteDto dto)
        {
            Sprite icon;
            try
            {
                var iconResourceFile = UMI3DEnvironmentLoader.AbstractParameters.ChooseVariant(dto.iconResource.variants);
                IResourcesLoader loader = UMI3DEnvironmentLoader.AbstractParameters.SelectLoader(iconResourceFile.extension);
                Texture2D texture = (Texture2D)await UMI3DResourcesManager.LoadFile(dto.id, iconResourceFile, loader);
                icon = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
            catch (Umi3dException e)
            {
                // in that case, we use the default icon.
                UMI3DLogger.LogWarning($"Unable to load emote \"{dto.label}\" icon ressource.\n${e.Message}", DEBUG_SCOPE);
                icon = emoteManagementService.DefaultIcon;
            }
            return icon;
        }

        public override void Delete(ulong environmentId, ulong id)
        {
            // free engine memory from sprite
            Emote emote = environmentManager.GetEntityObject<Emote>(environmentId, id);
            if (emote.icon != emoteManagementService.DefaultIcon)
                UnityEngine.Object.Destroy(emote.icon);
        }

        #region Properties

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (value.entity.dto is not UMI3DEmoteDto dto)
                return await Task.FromResult(false);

            if (!environmentManager.TryGetEntity(value.environmentId, dto.id, out Emote emote))
                return await Task.FromResult(false);

            switch (value.property.property)
            {
                case UMI3DPropertyKeys.ActiveEmote:
                    emote.available = (bool)value.property.value;
                    emoteManagementService.UpdateEmote(emote);
                    break;

                case UMI3DPropertyKeys.AnimationEmote:
                    emote.AnimationId = (ulong)value.property.value;
                    emoteManagementService.UpdateEmote(emote);
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

            if (!environmentManager.TryGetEntity(value.environmentId, dto.id, out Emote emote))
                return await Task.FromResult(false);

            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.ActiveEmote:
                    emote.available = UMI3DSerializer.Read<bool>(value.container);
                    emoteManagementService.UpdateEmote(emote);
                    break;

                case UMI3DPropertyKeys.AnimationEmote:
                    emote.AnimationId = UMI3DSerializer.Read<ulong>(value.container);
                    emoteManagementService.UpdateEmote(emote);
                    break;

                default:
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.ActiveEmote:
                case UMI3DPropertyKeys.AnimationEmote:
                    {
                        UMI3DEmoteDto emoteDto = UMI3DSerializer.Read<UMI3DEmoteDto>(data.container);
                        data.result = emoteDto;

                        if (!environmentManager.TryGetEntity(data.environmentId, emoteDto.id, out Emote emote))
                            return await Task.FromResult(false);

                        emote.available = emoteDto.available;
                        emote.AnimationId = emoteDto.animationId;
                        emoteManagementService.UpdateEmote(emote);
                        break;
                    }

                default:
                    return await Task.FromResult(false);
            }
            return await Task.FromResult(true);
        }

        #endregion Properties
    }
}