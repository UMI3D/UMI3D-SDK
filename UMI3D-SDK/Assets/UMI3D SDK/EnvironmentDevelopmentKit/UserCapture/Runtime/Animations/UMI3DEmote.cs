/*
Copyright 2019 - 2021 Inetum
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
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// Emote data, including a reference to the icon resource
    /// </summary>
    /// An emote is a short animation that is played to convey a specific communication, often an emotion.
    /// Emotes are used on non-immersive devices to allow the user to communicate non-verbally.
    [System.Serializable]
    public class UMI3DEmote : UMI3DLoadableEntity
    {
        /// <summary>
        /// Emote entity id
        /// </summary>
        [HideInInspector]
        public ulong id;

        /// <summary>
        /// Emote state name in Animator
        /// </summary>
        [Tooltip("Emote state name in Animator. Make sure that it is the same name used in AnimationClips.")]
        public string stateName;

        /// <summary>
        /// Emote name displayed to players
        /// </summary>
        [Tooltip("Emote name displayed to players. If let empty, take the value of the state name.")]
        public string label;


        /// <summary>
        /// If the user can see and play the emote.
        /// </summary>
        public UMI3DAsyncProperty<bool> Available
        {
            get
            {
                if (_available == null)
                {
                    if (id == default)
                        Id();
                    _available = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.ActiveEmote, availableAtStart);

                }
                return _available;
            }
            private set => _available = value;
        }
        public UMI3DAsyncProperty<bool> _available;

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
        public UMI3DResourceFile iconResource;

        /// <summary>
        /// True when the entity is registered into the environment
        /// </summary>
        [HideInInspector]
        public bool registered = false;

        /// <inheritdoc/>
        public ulong Id()
        {
            if (!registered)
            {
                id = UMI3DEnvironment.Register(this);
                _available = new UMI3DAsyncProperty<bool>(id, UMI3DPropertyKeys.ActiveEmote, availableAtStart);
                registered = true;
            }
            return id;
        }

        /// <summary>
        /// Export the <see cref="UMI3DEmote"/> to a <see cref="UMI3DEmoteDto"/> for transfer
        /// </summary>
        /// <returns></returns>
        public IEntity ToEntityDto(UMI3DUser user)
        {
            return new UMI3DEmoteDto()
            {
                label = string.IsNullOrEmpty(this.label) ? this.stateName : this.label,
                stateName = this.stateName,
                id = this.Id(),
                available = this.availableAtStart,
                iconResource = this.iconResource.ToDto(),
            };
        }


        /// <inheritdoc/>
        public Bytable ToBytes(UMI3DUser user)
        {
            var label = string.IsNullOrEmpty(this.label) ? this.stateName : this.label;
            Bytable bytable = UMI3DNetworkingHelper.Write<ulong>(id)
                            + UMI3DNetworkingHelper.Write<string>(label)
                            + UMI3DNetworkingHelper.Write<string>(stateName)
                            + UMI3DNetworkingHelper.Write<bool>(availableAtStart)
                            + UMI3DNetworkingHelper.Write<FileDto>(iconResource.ToDto());

            return bytable;
        }

        #region filter
        private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        /// <inheritdoc/>
        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        /// <inheritdoc/>
        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        /// <inheritdoc/>
        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }

        #endregion filter

        /// <inheritdoc/>
        public LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSetWhenHasJoined()
            };

            return operation;
        }

        /// <inheritdoc/>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSet()
            };
            return operation;
        }
    }
}
