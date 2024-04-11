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
using System.Linq;
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
        public virtual IReadOnlyList<Emote> Emotes => emotesConfig.Emotes;

        /// <summary>
        /// Last received Emote Configuration dto reference
        /// </summary>
        private EmotesConfig emotesConfig;

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
        public virtual void AddEmoteConfig(EmotesConfig emotesConfig)
        {
            if (this.emotesConfig is not null)
                ResetEmoteSystem();

            this.emotesConfig = emotesConfig;

            foreach (Emote emote in this.Emotes)
            {
                emote.available = emote.available || emotesConfig.AllAvailableByDefault;
            }

            hasReceivedEmotes = true;

            EmotesLoaded?.Invoke(Emotes);

            collabClientServerService.OnRedirection.AddListener(ResetEmoteSystem); //? most of this work (e.g. cleaning the animation, should be handled by the server.
            collabClientServerService.OnLeaving.AddListener(ResetEmoteSystem);
        }

        /// <summary>
        /// Change the availability of an emote based on the received <see cref="UMI3DEmoteDto"/>
        /// </summary>
        /// <param name="dto"></param>
        public virtual void UpdateEmote(Emote emote)
        {
            EmoteUpdated?.Invoke(emote);
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
            emotesConfig = null;
            hasReceivedEmotes = false;

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
                emoteId = emote.Id,
                shouldTrigger = true
            };

            collabClientServerService._SendRequest(emoteRequest, true);

            if (emote.AnimationId == default) // immediately end emote in an emote without animation
                EmoteEnded?.Invoke(emote);
        }

        private void StartPlayMode(Emote emote)
        {
            if (emote.AnimationId == default)
                return;

            if (!environmentManager.TryGetEntity(emote.EnvironmentId, emote.AnimationId, out playingEmoteAnimation))
            {
                UMI3DLogger.Log($"Could not start emote. Associated animation {emote.AnimationId} does not exist", DEBUG_SCOPE);
                return;
            }

            playingEmote = emote;
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
            if (!IsPlaying)
                return;
            
            // send the emote interruption text to other browsers through the server
            var emoteRequest = new EmoteRequestDto()
            {
                emoteId = playingEmote.Id,
                shouldTrigger = false
            };

            collabClientServerService._SendRequest(emoteRequest, true);

            EmoteEnded?.Invoke(playingEmote);

            StopPlayMode();
        }

        #endregion Emote Playing
    }
}