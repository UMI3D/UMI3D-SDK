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
using umi3d.common.collaboration.emotes;
using UnityEngine;

namespace umi3d.edk.collaboration.emotes
{
    /// <summary>
    /// Emote config file to send to users
    /// </summary>
    /// The emote configuration is used asynchronously to describe all the available emotes in an environment and explicit
    /// which ones are allow ed to be used for each user.
    [CreateAssetMenu(fileName = "EmotesConfig", menuName = "UMI3D/Collaboration/Emotes Config")]
    public class UMI3DEmotesConfig : ScriptableObject, UMI3DLoadableEntity
    {
        /// <summary>
        /// Should the emotes be available by default to users ?
        /// </summary>
        [Tooltip("Should the emotes be available by default to users? This setting will override all availability settings in the emotes list.")]
        public bool allAvailableAtStartByDefault = false;

        /// <summary>
        /// List of included emotes
        /// </summary>
        [Tooltip("List of included emotes.")]
        public List<UMI3DEmote> IncludedEmotes = new();

        #region Registration

        /// <summary>
        /// Entity id
        /// </summary>
        [NonSerialized]
        private ulong id;

        /// <summary>
        /// True when the entity is registered into the environment
        /// </summary>
        [NonSerialized]
        private bool registered = false;

        /// <inheritdoc/>
        public ulong Id()
        {
            if (!registered)
            {
                id = UMI3DEnvironment.Instance.RegisterEntity(this);
                registered = true;
            }
            return id;
        }

        #endregion Registration

        #region UMI3DLoadableEntity

        #region Serialization

        /// <inheritdoc/>
        public IEntity ToEntityDto(UMI3DUser user)
        {
            foreach (var emote in IncludedEmotes)
                emote.Available.SetValue(user, emote.availableAtStart || allAvailableAtStartByDefault);
            return new UMI3DEmotesConfigDto()
            {
                id = this.Id(),
                emotes = this.IncludedEmotes.Select(x => (UMI3DEmoteDto)x.ToEntityDto(user)).ToList(),
                allAvailableByDefault = this.allAvailableAtStartByDefault
            };
        }

        /// <inheritdoc/>
        public Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DSerializer.Write(ToEntityDto(user));
        }

        #endregion Serialization

        #region Loading

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

        /// <inheritdoc/>
        public LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users ?? UMI3DServer.Instance.UserSetWhenHasJoined()
            };

            return operation;
        }

        #endregion Loading

        #region Filters

        private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        /// <inheritdoc/>
        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        /// <inheritdoc/>
        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        /// <inheritdoc/>
        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }

        #endregion Filters

        #endregion UMI3DLoadableEntity
    }
}