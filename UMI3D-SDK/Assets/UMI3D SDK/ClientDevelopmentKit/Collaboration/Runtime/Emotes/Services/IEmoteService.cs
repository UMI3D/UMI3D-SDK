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
using umi3d.common.collaboration.dto.emotes;
using UnityEngine;

namespace umi3d.cdk.collaboration.emotes
{
    public interface IEmoteService
    {
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

        #region Emote Playing

        /// <summary>
        /// Is the EmoteManager currently playing an emote?
        /// </summary>
        public bool IsPlaying { get; }

        /// <summary>
        /// Play the emote if possible.
        /// </summary>
        /// <param name="emote"></param>
        /// <returns></returns>
        public void PlayEmote(Emote emote);

        /// <summary>
        /// Stop the currently playing emote.
        /// </summary>
        /// <param name="emote"></param>
        /// <returns></returns>
        public void StopEmote();

        #endregion Emote Playing

        #region Emote Changes

        /// <summary>
        /// Change the availability of an emote based on the received <see cref="UMI3DEmoteDto"/>.
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateEmote(UMI3DEmoteDto emoteDto);

        /// <summary>
        /// Load and configure emotes from an <see cref="UMI3DEmotesConfigDto"/>
        /// and try to get the animations.
        /// </summary>
        /// <param name="dto"></param>
        public void UpdateEmoteConfig(UMI3DEmotesConfigDto dto);

        /// <summary>
        /// Default icon used when no corresponding emote is found.
        /// </summary>
        public Sprite DefaultIcon { get; set; }

        /// <summary>
        /// Emotes attributed to the user.
        /// </summary>
        public IReadOnlyList<Emote> Emotes { get; }

        #endregion Emote Changes
    }
}