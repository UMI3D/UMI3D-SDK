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

using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
using umi3d.edk.userCapture.tracking;

namespace umi3d.edk.collaboration
{
    public abstract class UMI3DCollaborationAbstractUser : UMI3DTrackedUser
    {
        protected const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.User;

        public RegisterIdentityDto identityDto { get; set; }

        /// <inheritdoc/>
        protected override ulong userId { get => identityDto.userId; set => identityDto.userId = value; }
        /// <summary>
        /// The unique user login.
        /// </summary>
        public string login => identityDto.login;

        /// <summary>
        /// The unique user display Name.
        /// </summary>
        public string displayName => identityDto.displayName ?? login ?? userId.ToString();

        /// <summary>
        /// The unique user login.
        /// </summary>
        public string guid => identityDto.guid;

        /// <summary>
        /// The unique user login.
        /// </summary>
        public byte[] metadata => identityDto.metaData;

        /// <summary>
        /// Language used by user.
        /// </summary>
        public string language = string.Empty;

        /// <summary>
        /// The user token
        /// </summary>
        public string token => identityDto.localToken;

        /// <summary>
        /// If true, user's visible controllers (device representation or hands from hand tracking) are visible.
        /// </summary>
        /// Typically set this value to false if you already have an avatar 
        /// for the user that is enough for visual feedback.
        public UMI3DAsyncProperty<bool> AreTrackedControllersVisible;


        public UMI3DCollaborationAbstractUser(RegisterIdentityDto identity) : base()
        {
            this.identityDto = identity ?? new RegisterIdentityDto();

            userId = identity is not null && identity.userId != 0 ? UMI3DEnvironment.Register(this, identity.userId) : Id();

            AreTrackedControllersVisible = new UMI3DAsyncProperty<bool>(Id(), UMI3DPropertyKeys.AreTrackedControllersVisible, true);

            status = StatusType.CREATED;

            UMI3DLogger.Log($"<color=magenta>new User {Id()} {login} [{this.GetType()}]</color>", scope);
        }
    }
}