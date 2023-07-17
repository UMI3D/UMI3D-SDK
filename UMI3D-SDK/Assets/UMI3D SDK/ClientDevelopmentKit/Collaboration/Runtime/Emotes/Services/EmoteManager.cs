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

using System;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.collaboration.dto.emotes;
using UnityEngine;

namespace umi3d.cdk.collaboration.emotes
{
    /// <summary>
    /// Manager that handles emotes
    /// </summary>
    /// This manager is optionnal and should be added to a browser only if it plans to supports emotes.
    public class EmoteManager : inetum.unityUtils.Singleton<EmoteManager>, IEmoteService
    {
        #region Fields

        private const DebugScope DEBUG_SCOPE = DebugScope.CDK | DebugScope.Collaboration;

        /// <summary>
        /// Emotes attributed to the user
        /// </summary>
        public virtual IReadOnlyList<Emote> Emotes => emotes;
        protected List<Emote> emotes = new();

        /// <summary>
        /// Last received Emote Configuration dto reference
        /// </summary>
        private UMI3DEmotesConfigDto emoteConfigDto;

        /// <summary>
        /// Default icon used when no corresponding emote is found
        /// </summary>
        public Sprite DefaultIcon
        {
            get
            {
                if (_defaultIcon is null) //instantiate an empty texture if no default icon is set
                {
                    Texture2D emptyTexture = new Texture2D(30, 30);
                    _defaultIcon = Sprite.Create(emptyTexture, new Rect(0.0f, 0.0f, emptyTexture.width, emptyTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
                }
                return _defaultIcon;
            }
            set
            {
                _defaultIcon = value;
            }
        }

        private Sprite _defaultIcon;

        /// <summary>
        /// True when a bundle with emotes has been loaded
        /// </summary>
        private bool hasReceivedEmotes = false;

        /// <summary>
        /// Currently playing emote if one.
        /// </summary>
        private Emote playingEmote;

        /// <summary>
        /// Animaton of the currently played emote
        /// </summary>
        private UMI3DAbstractAnimation playingEmoteAnimation;

        /// <summary>
        /// Is the EmoteManager currently playing an emote?
        /// </summary>
        public bool IsPlaying => playingEmote is not null;

        #endregion Fields

        #region Dependencies Injection

        private readonly ILoadingManager environmentLoaderService;
        private readonly IEnvironmentManager environmentManager;
        private readonly IUMI3DCollaborationClientServer collabClientServerService;

        public EmoteManager() : base()
        {
            environmentLoaderService = UMI3DCollaborationEnvironmentLoader.Instance;
            environmentManager = UMI3DCollaborationEnvironmentLoader.Instance;
            collabClientServerService = UMI3DCollaborationClientServer.Instance;
        }

        public EmoteManager(ILoadingManager environmentLoader,
                            IEnvironmentManager environmentManager,
                            IUMI3DCollaborationClientServer collabClientServerService)
                            : base()
        {
            environmentLoaderService = environmentLoader;
            this.environmentManager = environmentManager;
            this.collabClientServerService = collabClientServerService;
        }

        #endregion Dependencies Injection

        #region Events

        /// <summary>
        /// Triggered when emotes are loaded and made available.
        /// </summary>
        public event Action<IReadOnlyList<Emote>> EmotesLoaded;

        /// <summary>
        /// Triggered when an emote is modified.
        /// </summary>
        public event Action<Emote> EmoteUpdated;

        /// <summary>
        /// Triggered when an emote is played by the user.
        /// </summary>
        public event Action<Emote> EmoteStarted;

        /// <summary>
        /// Triggered when an emote is finished to be played by the user.
        /// </summary>
        public event Action<Emote> EmoteEnded;

        #endregion Events

        #region Emote Changes

        /// <summary>
        /// Load and configure emotes from an <see cref="UMI3DEmotesConfigDto"/>
        /// and try to get the animations.
        /// </summary>
        /// <param name="dto"></param>
        public virtual void UpdateEmoteConfig(UMI3DEmotesConfigDto dto)
        {
            if (emoteConfigDto is not null)
                ResetEmoteSystem();

            emoteConfigDto = dto;

            if (!environmentManager.loaded)
                environmentLoaderService.onEnvironmentLoaded.AddListener(LoadEmotes);
            else //sometimes the environment is already loaded when loading emotes
                LoadEmotes();

            collabClientServerService.OnRedirection.AddListener(ResetEmoteSystem); //? most of this work (e.g. cleaning the animation, should be handled by the server.
            collabClientServerService.OnLeaving.AddListener(ResetEmoteSystem);
        }

        /// <summary>
        /// Instanciate emotes serialized in the EmoteConfig.
        /// </summary>
        private void LoadEmotes()
        {
            foreach (UMI3DEmoteDto emoteDtoInConfig in emoteConfigDto.emotes)
            {
                if (emoteDtoInConfig is not null)
                {
                    var emote = new Emote()
                    {
                        available = emoteConfigDto.allAvailableByDefault || emoteDtoInConfig.available,
                        icon = DefaultIcon,
                        dto = emoteDtoInConfig
                    };
                    if (emoteDtoInConfig.iconResource is not null
                        && emoteDtoInConfig.iconResource.variants.Count > 0
                        && emoteDtoInConfig.iconResource.variants[0].metrics.size != 0)
                        LoadIcon(emoteDtoInConfig, emote);
                    emotes.Add(emote);
                }
            }
            hasReceivedEmotes = true;

            EmotesLoaded?.Invoke(Emotes);
        }

        /// <summary>
        /// Change the availability of an emote based on the received <see cref="UMI3DEmoteDto"/>
        /// </summary>
        /// <param name="dto"></param>
        public virtual void UpdateEmote(UMI3DEmoteDto dto)
        {
            if (!hasReceivedEmotes)
            {
                UMI3DLogger.LogWarning($"Trying to update the emote {dto.label} when no EmoteConfig was received", DEBUG_SCOPE);
                return;
            }

            var emote = emotes.Find(x => x.dto.id == dto.id);
            emote.available = dto.available;
            emote.dto = dto;
            EmoteUpdated?.Invoke(emote);
        }

        private async void LoadIcon(UMI3DEmoteDto emoteRefInConfig, Emote emote)
        {
            try
            {
                var iconResourceFile = UMI3DEnvironmentLoader.AbstractParameters.ChooseVariant(emoteRefInConfig.iconResource.variants);
                IResourcesLoader loader = UMI3DEnvironmentLoader.AbstractParameters.SelectLoader(iconResourceFile.extension);
                Texture2D texture = (Texture2D)await UMI3DResourcesManager.LoadFile(emoteConfigDto.id,
                                                                                     iconResourceFile,
                                                                                     loader);
                emote.icon = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
            catch (Umi3dException e)
            {
                // in that case, we use the default icon.
                UMI3DLogger.LogWarning($"Unable to load emote \"{emoteRefInConfig?.label}\" icon ressource.\n${e.Message}", DEBUG_SCOPE);
            }
            return;
        }

        /// <summary>
        /// Reset all variables and disable the system
        /// </summary>
        private void ResetEmoteSystem()
        {
            if (IsPlaying)
                StopEmote();

            if (!hasReceivedEmotes) return;

            EmotesLoaded?.Invoke(null); // used to empty the UI
            emotes.Clear();
            emoteConfigDto = null;
            hasReceivedEmotes = false;

            environmentLoaderService.onEnvironmentLoaded.RemoveListener(LoadEmotes);
            collabClientServerService.OnRedirection.RemoveListener(ResetEmoteSystem);
        }

        #endregion Emote Changes

        #region Emote Playing

        /// <summary>
        /// Play the emote
        /// </summary>
        /// <param name="emote"></param>
        public virtual void PlayEmote(Emote emote)
        {
            if (!emote.available)
            {
                UMI3DLogger.LogWarning($"Could not play emote {emote.Label} as it is not available for this user", DEBUG_SCOPE);
                return;
            }

            if (IsPlaying)
            {
                if (emote == playingEmote) // spam is not allowed
                    return;
                else
                    StopEmote();
            }

            // emote play mode
            StartPlayMode(emote);

            EmoteStarted?.Invoke(emote);

            // send the emote triggerring text to other browsers through the server
            var emoteRequest = new EmoteRequestDto()
            {
                emoteId = emote.dto.id,
                shouldTrigger = true
            };

            collabClientServerService._SendRequest(emoteRequest, true);
        }

        private void StartPlayMode(Emote emote)
        {
            playingEmote = emote;
            playingEmoteAnimation = environmentManager.GetEntityObject<UMI3DAbstractAnimation>(playingEmote.AnimationId);
            playingEmoteAnimation.AnimationEnded += StopEmote;
        }

        private void StopPlayMode()
        {
            playingEmoteAnimation.AnimationEnded -= StopEmote;
            playingEmoteAnimation = null;
            playingEmote = null;
        }

        /// <summary>
        /// Stop the emote playing process.
        /// </summary>
        /// <param name="emote"></param>
        public virtual void StopEmote()
        {
            if (IsPlaying)
            {
                // send the emote interruption text to other browsers through the server
                var emoteRequest = new EmoteRequestDto()
                {
                    emoteId = playingEmote.dto.id,
                    shouldTrigger = false
                };

                collabClientServerService._SendRequest(emoteRequest, true);

                EmoteEnded?.Invoke(playingEmote);

                StopPlayMode();
            }
        }

        #endregion Emote Playing
    }
}