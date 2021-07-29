﻿/*
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
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.interaction;
using UnityEngine;
using umi3d.edk.interaction;

namespace umi3d.edk.collaboration
{
    public abstract class IdentifierApi : ScriptableObject
    {

        Dictionary<ulong, bool> librariesUpdateStatus;

        public virtual UMI3DAuthenticator GetAuthenticator(ref AuthenticationType type) { return null; }

        /// <summary>
        /// Update a client status acording to a userconnectionDto
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="identity">Identity Dto send by the user.</param>
        /// <returns></returns>
        public virtual StatusType UpdateIdentity(UMI3DCollaborationUser user, UserConnectionDto identity)
        {
            if (librariesUpdateStatus == null) librariesUpdateStatus = new Dictionary<ulong, bool>();
            librariesUpdateStatus[user.Id()] = identity.librariesUpdated;
            SetUserLocalInfoAuthorization(user, identity.parameters);
            return librariesUpdateStatus[user.Id()] ? ((identity.status > StatusType.READY) ? identity.status : StatusType.READY) : StatusType.CREATED;
        }


        void SetUserLocalInfoAuthorization(UMI3DCollaborationUser user, FormDto param)
        {
            if (param != null)
                foreach (var dto in param.fields)
                    if (dto is LocalInfoRequestParameterDto)
                        LocalInfoParameter.ChageUserLocalInfo(user, dto as LocalInfoRequestParameterDto);
        }

        /// <summary>
        /// Get a Form Dto for a login.
        /// </summary>
        /// <param name="login">Login of the user.</param>
        /// <returns></returns>
        public virtual FormDto GetParameterDtosFor(UMI3DCollaborationUser user)
        {
            return null;
        }

        /// <summary>
        /// Should a user update has updataed its libraries.
        /// </summary>
        /// <param name="login">Login of the user.</param>
        /// <returns></returns>
        public virtual bool getLibrariesUpdateSatus(UMI3DCollaborationUser user)
        {
            if (librariesUpdateStatus == null) librariesUpdateStatus = new Dictionary<ulong, bool>();
            return librariesUpdateStatus.ContainsKey(user.Id()) ? librariesUpdateStatus[user.Id()] : false;
        }
    }
}