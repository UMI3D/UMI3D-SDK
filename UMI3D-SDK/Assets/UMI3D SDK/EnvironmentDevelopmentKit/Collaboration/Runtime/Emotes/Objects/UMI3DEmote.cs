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
using umi3d.common;
using umi3d.common.collaboration.emotes;
using umi3d.edk.core;
using UnityEngine;

namespace umi3d.edk.collaboration.emotes
{
    /// <summary>
    /// Emote data, including a reference to the icon resource
    /// </summary>
    /// An emote is a short animation that is played to convey a specific communication, often an emotion.
    /// Emotes are used on non-immersive devices to allow the user to communicate non-verbally.
    [System.Serializable]
    public class UMI3DEmote : AbstractLoadableEntity
    {
        /// <summary>
        /// Emote name displayed to players
        /// </summary>
        [Tooltip("Emote name displayed to players.")]
        public string label;

        /// <summary>
        /// Emote animation
        /// </summary>
        public UMI3DAsyncProperty<ulong> AnimationId
        {
            get
            {
                _animationId ??= new UMI3DAsyncProperty<ulong>(Id(), UMI3DPropertyKeys.AnimationEmote, default);
                return _animationId;
            }
            private set => _animationId = value;
        }

        protected UMI3DAsyncProperty<ulong> _animationId;

        /// <summary>
        /// If the user can see and play the emote.
        /// </summary>
        public UMI3DAsyncProperty<bool> Available
        {
            get
            {
                _available ??= new UMI3DAsyncProperty<bool>(Id(), UMI3DPropertyKeys.ActiveEmote, availableAtStart);
                return _available;
            }
            private set => _available = value;
        }

        protected UMI3DAsyncProperty<bool> _available;

        /// <summary>
        /// True if the user can see and play the emote at the start.
        /// </summary>
        [Tooltip("True if the user can see and play the emote at the start.")]
        public bool availableAtStart;

        /// <summary>
        /// Icon illustrating the emote ressource details.
        /// </summary>
        /// It illustrates the emote to be displayed on the client side.
        [Header("Icon"), Tooltip("Icon illustrating the emote to be displayed on the client side.")]
        public UMI3DResource iconResource;

        #region UMI3DLoadableEntity

        #region Serialization

        /// <summary>
        /// Export the <see cref="UMI3DEmote"/> to a <see cref="UMI3DEmoteDto"/> for transfer
        /// </summary>
        /// <returns></returns>
        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return new UMI3DEmoteDto()
            {
                id = this.Id(),
                label = this.label,
                animationId = this.AnimationId.GetValue(user), // create a DTO
                available = this.availableAtStart,
                iconResource = this.iconResource.ToDto(),
            };
        }

        #endregion Serialization

        #region Loading

        /// <inheritdoc/>
        public override LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };

            return operation;
        }

        /// <inheritdoc/>
        public override DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSet()
            };
            return operation;
        }

        #endregion Loading

        #endregion UMI3DLoadableEntity
    }
}