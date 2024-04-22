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
using System.Linq;
using System.Threading.Tasks;
using umi3d.common.collaboration.dto.emotes;

namespace umi3d.cdk.collaboration.emotes
{
    /// <summary>
    /// Loader for <see cref="UMI3DEmotesConfigDto"/>.
    /// </summary>
    public class UMI3DEmotesConfigLoader : AbstractLoader<UMI3DEmotesConfigDto, EmotesConfig>
    {
        private UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        #region DependencyInjection

        private readonly IEmoteService emoteManagementService;
        private readonly IEnvironmentManager environmentManager;
        private readonly ILoader<UMI3DEmoteDto, Emote> emoteLoader;

        public UMI3DEmotesConfigLoader() : this(emoteManagementService: EmoteManager.Instance,
                                                environmentManager: UMI3DCollaborationEnvironmentLoader.Instance,
                                                emoteLoader: new UMI3DEmoteLoader())
        {
        }

        public UMI3DEmotesConfigLoader(IEmoteService emoteManagementService, IEnvironmentManager environmentManager, ILoader<UMI3DEmoteDto, Emote> emoteLoader)
        {
            this.emoteManagementService = emoteManagementService;
            this.environmentManager = environmentManager;
            this.emoteLoader = emoteLoader;
        }


        #endregion DependencyInjection

        public override async Task<EmotesConfig> Load(ulong environmentId, UMI3DEmotesConfigDto dto)
        {
            IEnumerable<Emote> emotes = await Task.WhenAll(dto.emotes.Select(async (emoteDto) => await emoteLoader.Load(environmentId, emoteDto)));

            EmotesConfig emoteConfig = new(dto, emotes);

            environmentManager.RegisterEntity(environmentId, dto.id, dto, emoteConfig).NotifyLoaded();

            emoteManagementService.AddEmoteConfig(emoteConfig);

            return emoteConfig;
        }

        public override void Delete(ulong environmentId, ulong id)
        {
            // nothing to do
        }
    }
}