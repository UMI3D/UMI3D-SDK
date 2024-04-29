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

using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
namespace umi3d.edk
{
    /// <summary>
    /// A user in a UMI3D context
    /// </summary>
    public class UMI3DUser : UMI3DMediaEntity
    {

        #region identity

        /// <summary>
        /// The unique user identifier.
        /// </summary>
        protected virtual ulong userId { get; set; } = 0;

        /// <summary>
        /// The public Getter for objectId.
        /// </summary>
        public virtual ulong Id()
        {
            if (userId == 0 && UMI3DEnvironment.Exists)
                userId = UMI3DEnvironment.Register(this);
            return userId;
        }

        public bool HasAlreadyGotTheEnvironmentDtoOnce = false;

        #endregion

        #region session
        /// <summary>
        /// True if the user's browser is using purely virtual immersion and not any form of Mixed Reality.
        /// </summary>
        public bool HasImmersiveDevice { get; protected set; } = true;

        /// <summary>
        /// Does the user have an immersive display on their device.
        /// </summary>
        public bool HasHeadMountedDisplay { get; protected set; } = true;

        /// <summary>
        /// UMI3D status of the object. 
        /// </summary>
        /// See <see cref="StatusType"/>.
        public StatusType status { get; protected set; } = StatusType.CREATED;

        public bool IsReadyToGetResources = true;

        /// <summary>
        /// Has the user joined the environment?
        /// </summary>
        public bool hasJoined = false;

        /// <summary>
        /// Setter for the <see cref="status"/>
        /// </summary>
        /// <param name="status"></param>
        public virtual void SetStatus(StatusType status)
        {
            this.status = status;
        }

        /// <inheritdoc/>
        public IEntity ToEntityDto(UMI3DUser user)
        {
            throw new NotImplementedException();
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
        #endregion

        #endregion
    }
}