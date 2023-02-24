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
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// Emote config file to send to users
    /// </summary>
    /// The emote configuration is used asynchronously to describe all the available emotes in an environment and explicit
    /// which ones are allow ed to be used for each user.

    [CreateAssetMenu(fileName = "UMI3DEmotesConfigTemplate", menuName = "UMI3D/Emotes Config")]
    public class UMI3DEmotesConfig : MonoBehaviour, UMI3DLoadableEntity
    {
        /// <summary>
        /// Entity id
        /// </summary>
        private ulong id;

        /// <summary>
        /// Should the emotes be available by default to users ?
        /// </summary>
        [Tooltip("Should the emotes be available by default to users? This setting will override all availability settings in the emotes list.")]
        public bool allAvailableAtStartByDefault = false;

        /// <summary>
        /// List of included emotes
        /// </summary>
        [Tooltip("List of included emotes.")]
        public List<UMI3DEmote> IncludedEmotes;

        private void Awake()
        {
            id = default;
            registered = false;
            if (IncludedEmotes?.Count > 0)
            {
                foreach (UMI3DEmote emote in IncludedEmotes)
                {
                    emote.id = default;
                    emote.registered = false;
                }
            }
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
        public IEntity ToEntityDto(UMI3DUser user)
        {
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
            Bytable bytable = UMI3DSerializer.Write(this.Id());
            bytable += UMI3DSerializer.Write(allAvailableAtStartByDefault);
            UMI3DSerializer.Write(IncludedEmotes.Count);
            foreach (UMI3DEmote emote in IncludedEmotes)
            {
                bytable += emote.ToBytes(user);
            }
            return bytable;
        }

        /// <summary>
        /// True when the entity is registered into the environment
        /// </summary>
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
    }
}