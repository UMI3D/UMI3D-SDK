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
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.interaction;
using umi3d.edk.interaction;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    [CreateAssetMenu(fileName = "DefaultIdentifierApi", menuName = "UMI3D/Default Identifier")]
    public class IdentifierApi : ScriptableObject
    {
        private Dictionary<ulong, bool> librariesUpdateStatus;

        /// <summary>
        /// Update a client status acording to a userconnectionDto
        /// </summary>
        /// <param name="user">User.</param>
        /// <param name="identity">Identity Dto send by the user.</param>
        /// <returns></returns>
        public virtual StatusType UpdateIdentity(UMI3DCollaborationUser user, UserConnectionAnswerDto identity)
        {
            if (librariesUpdateStatus == null) librariesUpdateStatus = new Dictionary<ulong, bool>();
            librariesUpdateStatus[user.Id()] = identity.librariesUpdated;
            SetUserLocalInfoAuthorization(user, identity.parameters);
            return librariesUpdateStatus[user.Id()] ? ((identity.status > StatusType.READY) ? identity.status : StatusType.READY) : StatusType.CREATED;
        }

        private void SetUserLocalInfoAuthorization(UMI3DCollaborationUser user, FormAnswerDto param)
        {
            if (param != null)
            {
                foreach (ParameterSettingRequestDto dto in param.answers)
                {
                    AbstractInteraction interaction = UMI3DEnvironment.GetEntity<AbstractInteraction>(dto.id);
                    if (interaction is LocalInfoParameter local)
                        local.ChageUserLocalInfo(user, dto.parameter as LocalInfoRequestParameterValue);
                }
            }
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