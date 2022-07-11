/*
Copyright 2019 - 2022 Inetum

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
    /// Emote config file to send to users
    /// </summary>
    [CreateAssetMenu(fileName = "UMI3DEmotesConfig", menuName = "UMI3D/Emotes Config")]
    public class UMI3DEmotesConfig : ScriptableObject, UMI3DLoadableEntity
    {
        void Awake()
        {
            id = default;
            registered = false;
            if (IncludedEmotes?.Count > 0)
            {
                foreach (var emote in IncludedEmotes)
                {
                    emote.id = default;
                    emote.registered = false;
                }
            }
        }  
        /// <summary>
        /// Should the emotes be available by default to users ?
        /// </summary>
        public bool allAvailableAtStartByDefault = false;

        /// <summary>
        /// Entity id
        /// </summary>
        [HideInInspector]
        private ulong id;

        /// <summary>
        /// List of included emotes
        /// </summary>
        public List<UMI3DEmote> IncludedEmotes;

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

        /// <inheritdoc/>
        public IEntity ToEntityDto(UMI3DUser user)
        {
            return new UMI3DEmotesConfigDto()
            {
                emotes = this.IncludedEmotes.Select(x => (UMI3DEmoteDto)x.ToEntityDto(user)).ToList(),
                allAvailableByDefault = this.allAvailableAtStartByDefault
            };
        }

        /// <inheritdoc/>
        public Bytable ToBytes(UMI3DUser user)
        {
            Bytable bytable = UMI3DNetworkingHelper.Write(allAvailableAtStartByDefault);

            UMI3DNetworkingHelper.Write(IncludedEmotes.Count);
            foreach (var emote in IncludedEmotes)
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
                id = UMI3DEnvironment.Register(this);
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
