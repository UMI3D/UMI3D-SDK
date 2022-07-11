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
    [System.Serializable]
    public class UMI3DEmote : UMI3DLoadableEntity
    {
        /// <summary>
        /// Emote name
        /// </summary>
        public string name;

        /// <summary>
        /// Emote entity id
        /// </summary>
        [HideInInspector]
        public ulong id;

        /// <summary>
        /// If  the user can see and play the emote
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
            private set { _available = value; }
        }
        public UMI3DAsyncProperty<bool> _available;

        /// <summary>
        /// If  the user can see and play the emote at the start
        /// </summary>
        public bool availableAtStart;

        /// <summary>
        /// Icon ressource details
        /// </summary>
        [Header("Icon")]
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
                name = this.name,
                id = this.Id(),
                available = this.availableAtStart,
                iconResource = this.iconResource.ToDto()
            };
        }


        /// <inheritdoc/>
        public Bytable ToBytes(UMI3DUser user)
        {
            Bytable bytable = UMI3DNetworkingHelper.Write<ulong>(id)
                            + UMI3DNetworkingHelper.Write<string>(name)
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
