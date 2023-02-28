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
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Manager that handles emotes
    /// </summary>
    /// This manager is optionnal and should be added to a browser only if it plans to supports emotes.
    public class EmoteManager : inetum.unityUtils.Singleton<EmoteManager>
    {
        #region Fields

        #region Events

        /// <summary>
        /// Triggered when emotes are loaded and made available.
        /// </summary>
        public event Action<List<Emote>> EmotesLoaded;

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

        #region EmotesConfigManagement

        /// <summary>
        /// Available emotes from bundle
        /// </summary>
        public List<Emote> Emotes = new();

        /// <summary>
        /// Last received Emote Configuration dto reference
        /// </summary>
        private UMI3DEmotesConfigDto emoteConfigDto;

        /// <summary>
        /// Default icon used when no corresponding emote is found
        /// </summary>
        public Sprite defaultIcon;

        /// <summary>
        /// True when a bundle with emotes has been loaded
        /// </summary>
        private bool hasReceivedEmotes = false;

        /// <summary>
        /// Currently playing emote if one.
        /// </summary>
        private Emote playingEmote;

        /// <summary>
        /// Is the EmoteManager currently playing an emote?
        /// </summary>
        public bool IsPlaying => playingEmote is not null;

        #endregion EmotesConfigManagement

        #endregion Fields

        #region DependenciesInjection

        private UMI3DEnvironmentLoader environmentLoaderService;

        public EmoteManager() : base()
        {
            environmentLoaderService = UMI3DEnvironmentLoader.Instance;
        }

        public EmoteManager(UMI3DEnvironmentLoader environmentLoader) : base()
        {
            environmentLoaderService = environmentLoader;
        }
        #endregion DependenciesInjection

        /// <summary>
        /// To call to initialize the manager.
        /// </summary>
        /// <param name="defaultIcon">Icon to display when no icon is set for an emote</param>
        public virtual void Initialize(Sprite defaultIcon = null)
        {
            if (defaultIcon is null) //instantiate an empty texture if no default icon is set
            {
                Texture2D emptyTexture = new Texture2D(30, 30);
                defaultIcon = Sprite.Create(emptyTexture, new Rect(0.0f, 0.0f, emptyTexture.width, emptyTexture.height),new Vector2(0.5f, 0.5f), 100.0f);
            }
            this.defaultIcon = defaultIcon;
            UMI3DCollaborationClientUserTracking.Instance.EmotesLoadedEvent += ConfigEmotes;
        }

        #region Emote Config

        /// <summary>
        /// Load and configure emotes from an <see cref="UMI3DEmotesConfigDto"/>
        /// and try to get the animations.
        /// </summary>
        /// <param name="dto"></param>
        private void ConfigEmotes(UMI3DEmotesConfigDto dto)
        {
            emoteConfigDto = dto;
            if (!UMI3DEnvironmentLoader.Instance.isEnvironmentLoaded)
                UMI3DEnvironmentLoader.Instance.onEnvironmentLoaded.AddListener(LoadEmotes);
            else //sometimes the environment is already loaded when loading emotes
                LoadEmotes();
            UMI3DCollaborationClientServer.Instance.OnRedirection.AddListener(ResetEmoteSystem); //? most of this work (e.g. cleaning the animation, should be handled by the server.
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
                        icon = defaultIcon,
                        dto = emoteDtoInConfig
                    };
                    if (emoteDtoInConfig.iconResource is not null && emoteDtoInConfig.iconResource.metrics.size != 0)
                        LoadFile(emoteDtoInConfig, emote);
                    Emotes.Add(emote);
                }
            }
            hasReceivedEmotes = true;
            UMI3DCollaborationClientUserTracking.Instance.EmoteUpdatedEvent += UpdateEmote; // register for any future change in emotes

            EmotesLoaded?.Invoke(Emotes);
        }

        /// <summary>
        /// Change the availability of an emote based on the received <see cref="UMI3DEmoteDto"/>
        /// </summary>
        /// <param name="dto"></param>
        private void UpdateEmote(UMI3DEmoteDto dto)
        {
            if (!hasReceivedEmotes)
                return;
            var emote = Emotes.Find(x => x.dto.id == dto.id);
            emote.available = dto.available;
            emote.dto = dto;
            EmoteUpdated?.Invoke(emote);
        }

        private async void LoadFile(UMI3DEmoteDto emoteRefInConfig, Emote emote)
        {
            try
            {
                IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(emoteRefInConfig.iconResource.extension);
                Texture2D texture = (Texture2D) await UMI3DResourcesManager.LoadFile(emoteConfigDto.id,
                                                                                     emoteRefInConfig.iconResource,
                                                                                     loader);
                emote.icon = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
            catch (Umi3dException e)
            {
                // in that case, we use the default icon.
                Debug.LogWarning($"Unable to load emote \"{emoteRefInConfig?.label}\" icon ressource.\n${e.Message}");
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
            Emotes.Clear();
            emoteConfigDto = null;
            hasReceivedEmotes = false;

            environmentLoaderService.onEnvironmentLoaded.RemoveListener(LoadEmotes);
            UMI3DCollaborationClientServer.Instance.OnRedirection.RemoveListener(ResetEmoteSystem);
        }

        #endregion Emote Config

        #region Emote Playing

        /// <summary>
        /// Play the emote
        /// </summary>
        /// <param name="emote"></param>
        /// <returns></returns>
        public virtual void PlayEmote(Emote emote)
        {
            if (!emote.available)
                throw new Umi3dException($"Could not play emote {emote.Label} as it is not available for this user");

            EmoteStarted?.Invoke(emote);

            // send the emote triggerring text to other browsers through the server
            var emoteRequest = new EmoteRequest()
            {
                emoteId = emote.dto.id,
                shouldTrigger = true
            };

            UMI3DClientServer.Instance.SendRequest(emoteRequest, true);

            // emote play mode
            playingEmote = emote;
            StartPlayMode();
        }

        /// <summary>
        /// Start emote play mode and listen to events to stop emote.
        /// </summary>
        private void StartPlayMode()
        {
            var animation = environmentLoaderService.GetEntityObject<UMI3DAbstractAnimation>(playingEmote.AnimationId);

            EmoteStarted += StopEmote; //used if another emote is played in the meanwhile //! now managed by the server
            animation.AnimationEnded += StopEmote;
        }

        /// <summary>
        /// End emote play mode and remove listeners for end of emote.
        /// </summary>
        private void EndPlayMode()
        {
            var animation = environmentLoaderService.GetEntityObject<UMI3DAbstractAnimation>(playingEmote.AnimationId);

            EmoteStarted -= StopEmote;
            animation.AnimationEnded -= StopEmote;
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
                var emoteRequest = new EmoteRequest()
                {
                    emoteId = playingEmote.dto.id,
                    shouldTrigger = false
                };
                UMI3DClientServer.Instance.SendRequest(emoteRequest, true);

                EndPlayMode();
                EmoteEnded?.Invoke(playingEmote);
                playingEmote = null;
            }
        }

        /// <summary>
        /// Call <see cref="StopEmote"/>.
        /// </summary>
        /// Exist to listen to events that require an Emote.
        /// <param name="emote">Not used parameter</param>
        private void StopEmote(Emote emote) => StopEmote();

        #endregion Emote Playing
    }
}