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

namespace umi3d.edk.core
{
    public abstract class AbstractLoadableEntity : UMI3DLoadableEntity
    {
        /// <summary>
        /// UMI3D id.
        /// </summary>
        protected ulong id;

        protected bool registered;

        private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        #region Dependencies Injection

        private readonly UMI3DServer umi3dServerService;
        private readonly UMI3DEnvironment umi3dEnvironmentService;

        public AbstractLoadableEntity()
        {
            umi3dServerService = UMI3DServer.Instance;
            umi3dEnvironmentService = UMI3DEnvironment.Instance;
        }

        protected AbstractLoadableEntity(UMI3DServer umi3dServerService, UMI3DEnvironment umi3dEnvironmentService)
        {
            this.umi3dServerService = umi3dServerService;
            this.umi3dEnvironmentService = umi3dEnvironmentService;
        }

        #endregion Dependencies Injection

        public virtual ulong Id()
        {
            if (!registered)
            {
                id = umi3dEnvironmentService.RegisterEntity(this);
                registered = true;
            }
            return id;
        }

        #region Serialization

        /// <summary>
        /// Export the <see cref="UMI3DEmote"/> to a <see cref="UMI3DEmoteDto"/> for transfer
        /// </summary>
        /// <returns></returns>
        public abstract IEntity ToEntityDto(UMI3DUser user);

        /// <inheritdoc/>
        public virtual Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DSerializer.Write(ToEntityDto(user));
        }

        #endregion Serialization

        #region Loading

        /// <inheritdoc/>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : umi3dServerService.UserSetWhenHasJoined()
            };

            return operation;
        }

        /// <inheritdoc/>
        public virtual DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : umi3dServerService.UserSet()
            };
            return operation;
        }

        #endregion Loading

        #region Filters

        /// <inheritdoc/>
        public virtual bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        /// <inheritdoc/>
        public virtual bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        /// <inheritdoc/>
        public virtual bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }

        #endregion Filters
    }
}