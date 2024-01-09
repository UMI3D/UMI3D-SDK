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

using BeardedManStudios.Forge.Networking;
using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
using umi3d.edk.userCapture.tracking;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// <see cref="UMI3DTrackedUser"/> in a collaborative context, with authentication, sound and video streaming etc...
    /// </summary>
    public abstract class UMI3DCollaborationAbstractUser : UMI3DTrackedUser
    {
        protected const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.User;

        public RegisterIdentityDto identityDto { get; set; }

        public UMI3DAsyncProperty<string> audioChannel;
        public UMI3DAsyncProperty<string> audioServerUrl;
        public UMI3DAsyncProperty<bool> audioUseMumble;

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
        /// Current id for ForgeNetworkingRemastered
        /// </summary>
        public NetworkingPlayer networkPlayer { get; set; }

        /// <summary>
        /// The user token
        /// </summary>
        public string token => identityDto.localToken;


        public UMI3DForgeServer forgeServer;

        /// <summary>
        /// Room object used to relay data
        /// </summary>
        public ICollaborationRoom RelayRoom;

        public UMI3DCollaborationAbstractUser(RegisterIdentityDto identity)
        {
            this.identityDto = identity ?? new RegisterIdentityDto();


            userId = identity is not null && identity.userId != 0 ? UMI3DEnvironment.Register(this, identity.userId) : Id();

            status = StatusType.CREATED;

            audioChannel = new UMI3DAsyncProperty<string>(userId, UMI3DPropertyKeys.UserAudioChannel, null);
            audioServerUrl = new UMI3DAsyncProperty<string>(userId, UMI3DPropertyKeys.UserAudioServer, null);
            audioUseMumble = new UMI3DAsyncProperty<bool>(userId, UMI3DPropertyKeys.UserAudioUseMumble, false);

            UMI3DLogger.Log($"<color=magenta>new User {Id()} {login}</color>", scope);
        }

        /// <summary>
        /// Update the identity of the user.
        /// </summary>
        /// <param name="identity"></param>
        public void Update(RegisterIdentityDto identity)
        {
            ulong id = Id();
            this.identityDto = identity ?? new RegisterIdentityDto();
            userId = id;
        }

        public virtual void InitConnection(UMI3DForgeServer connection)
        {
            this.forgeServer = connection;
        }


        /// <summary>
        /// Remove a user fom the environment.
        /// </summary>
        public void Logout()
        {
            UMI3DEnvironment.Remove(this);
        }

        /// <inheritdoc/>
        public override void SetStatus(StatusType status)
        {
            bool isSame = status == this.status;
            UMI3DLogger.Log($"Set Status {Id()} {status} {isSame}", scope);
            base.SetStatus(status);
            if (!isSame)
                UMI3DCollaborationServer.Instance.NotifyUserStatusChanged(this, status);
        }

        public virtual StatusDto ToStatusDto()
        {
            var status = new StatusDto
            {
                status = this.status
            };
            return status;
        }

        public virtual UserConnectionDto ToUserConnectionDto()
        {
            var source = ToUserDto(this);
            var connectionInformation = new UserConnectionDto()
            {
                id = source.id,
                login = source.login,
                status = source.status,

                audioSourceId = source.audioSourceId,
                audioFrequency = source.audioFrequency,
                videoSourceId = source.videoSourceId,
                networkId = source.networkId,

                microphoneStatus = source.microphoneStatus,
                avatarStatus = source.avatarStatus,
                attentionRequired = source.attentionRequired,

                audioChannel = source.audioChannel,
                audioServerUrl = source.audioServerUrl,
                audioLogin = source.audioLogin,
                audioUseMumble = source.audioUseMumble,

                onStartSpeakingAnimationId = source.onStartSpeakingAnimationId,
                onStopSpeakingAnimationId = source.onStopSpeakingAnimationId,
                language = source.language,
            };

            return connectionInformation;
        }
        public virtual UserDto ToUserDto(UMI3DUser user)
        {
            var _user = new UserDto
            {
                id = Id(),
                status = status,

                audioChannel = audioChannel.GetValue(user),
                audioServerUrl = audioServerUrl.GetValue(user),
                audioUseMumble = audioUseMumble.GetValue(user),
            };
            return _user;
        }
    }
}