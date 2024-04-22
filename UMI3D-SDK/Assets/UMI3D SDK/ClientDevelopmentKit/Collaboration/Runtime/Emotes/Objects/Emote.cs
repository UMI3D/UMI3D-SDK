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

using umi3d.common;
using umi3d.common.collaboration.dto.emotes;
using UnityEngine;

namespace umi3d.cdk.collaboration.emotes
{
    /// <summary>
    /// Describes an emote from the client side
    /// </summary>
    [System.Serializable]
    public class Emote : IBrowserEntity
    {
        public Emote(UMI3DEmoteDto dto, Sprite icon, ulong environmentId = UMI3DGlobalID.EnvironmentId)
        {
            this.dto = dto ?? throw new System.ArgumentNullException(nameof(dto));
            this.icon = icon;
            this.available = dto.available;
            this.AnimationId = dto.animationId;
            this.EnvironmentId = environmentId;
        }

        /// <summary>
        /// Emote dto
        /// </summary>
        private readonly UMI3DEmoteDto dto;

        public ulong Id => dto.id;

        public ulong EnvironmentId { get; private set; }

        /// <summary>
        /// Emote's label
        /// </summary>
        public virtual string Label => dto.label;

        /// <summary>
        /// Emote's animation
        /// </summary>
        public virtual ulong AnimationId { get; internal set; }

        /// <summary>
        /// Icon of the emote in the UI
        /// </summary>
        public Sprite icon { get; private set; }

        /// <summary>
        /// Should the emote be available or not
        /// </summary>
        public bool available { get; internal set; }
    }
}