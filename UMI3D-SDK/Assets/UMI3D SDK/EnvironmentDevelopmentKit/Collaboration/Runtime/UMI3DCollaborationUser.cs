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
using umi3d.common.collaboration;
using umi3d.edk.userCapture;

namespace umi3d.edk.collaboration
{
    public class UMI3DCollaborationUser : UMI3DTrackedUser
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.User;

        private RegisterIdentityDto identityDto;

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

        private static ulong lastGivenUserId = 1;

        /// <summary>
        /// Current id for ForgeNetworkingRemastered
        /// </summary>
        public NetworkingPlayer networkPlayer { get; set; }

        /// <summary>
        /// The user token
        /// </summary>
        public string token => identityDto.localToken;

        public UMI3DForgeServer forgeServer;

        public UMI3DAudioPlayer audioPlayer;

        public UMI3DAudioPlayer videoPlayer;

        public UMI3DAsyncProperty<int> audioFrequency;
        public UMI3DAsyncProperty<bool> microphoneStatus;
        public UMI3DAsyncProperty<bool> avatarStatus;
        public UMI3DAsyncProperty<bool> attentionRequired;

        public UMI3DAsyncProperty<string> audioChannel;
        public UMI3DAsyncProperty<string> audioServerUrl;
        public UMI3DAsyncProperty<bool> audioUseMumble;
        public UMI3DAsyncProperty<string> audioPassword;
        public UMI3DAsyncProperty<string> audioLogin;

        public UMI3DAsyncProperty<UMI3DAbstractAnimation> onStartSpeakingAnimationId;
        public UMI3DAsyncProperty<UMI3DAbstractAnimation> onStopSpeakingAnimationId;

        public UMI3DCollaborationUser(RegisterIdentityDto identity)
        {
            this.identityDto = identity ?? new RegisterIdentityDto();
            userId = UMI3DEnvironment.Register(this, lastGivenUserId++);

            audioFrequency = new UMI3DAsyncProperty<int>(userId, UMI3DPropertyKeys.UserAudioFrequency, 12000);
            microphoneStatus = new UMI3DAsyncProperty<bool>(userId, UMI3DPropertyKeys.UserMicrophoneStatus, false);
            avatarStatus = new UMI3DAsyncProperty<bool>(userId, UMI3DPropertyKeys.UserAvatarStatus, true);
            attentionRequired = new UMI3DAsyncProperty<bool>(userId, UMI3DPropertyKeys.UserAttentionRequired, false);

            audioChannel = new UMI3DAsyncProperty<string>(userId, UMI3DPropertyKeys.UserAudioChannel, null);
            audioServerUrl = new UMI3DAsyncProperty<string>(userId, UMI3DPropertyKeys.UserAudioServer, null);
            audioUseMumble = new UMI3DAsyncProperty<bool>(userId, UMI3DPropertyKeys.UserAudioUseMumble, false);
            audioPassword = new UMI3DAsyncProperty<string>(userId, UMI3DPropertyKeys.UserAudioPassword, null);
            audioLogin = new UMI3DAsyncProperty<string>(userId, UMI3DPropertyKeys.UserAudioLogin, null);

            onStartSpeakingAnimationId = new UMI3DAsyncProperty<UMI3DAbstractAnimation>(userId, UMI3DPropertyKeys.UserOnStartSpeakingAnimationId, null, (v, u) => v?.Id());
            onStopSpeakingAnimationId = new UMI3DAsyncProperty<UMI3DAbstractAnimation>(userId, UMI3DPropertyKeys.UserOnStopSpeakingAnimationId, null, (v, u) => v?.Id());

            status = StatusType.CREATED;
            UMI3DLogger.Log($"<color=magenta>new User {Id()} {login}</color>", scope);
        }

        public void Update(RegisterIdentityDto identity)
        {
            ulong id = Id();
            this.identityDto = identity ?? new RegisterIdentityDto();
            userId = id;
        }

        public void InitConnection(UMI3DForgeServer connection)
        {
            this.forgeServer = connection;
            var ucDto = new UserConnectionAnswerDto(ToUserDto(this))
            {
                librariesUpdated = !UMI3DEnvironment.UseLibrary()
            };
            //RenewToken();
            SetStatus(UMI3DCollaborationServer.Instance.Identifier.UpdateIdentity(this, ucDto));
        }

        public void Logout()
        {
            UMI3DEnvironment.Remove(this);
        }

        public void NotifyUpdate()
        {
            UMI3DCollaborationServer.Collaboration.NotifyUserStatusChanged(this);
        }

        ///<inheritdoc/>
        public override void SetStatus(StatusType status)
        {
            bool isSame = status == this.status;
            UMI3DLogger.Log($"Set Status {Id()} {status} {isSame}", scope);
            base.SetStatus(status);
            if (!isSame)
                UMI3DCollaborationServer.Instance.NotifyUserStatusChanged(this, status);
        }


        public virtual UserConnectionDto ToUserConnectionDto()
        {
            var connectionInformation = new UserConnectionDto(ToUserDto(this))
            {
                audioPassword = audioPassword.GetValue(),

                parameters = UMI3DCollaborationServer.Instance.Identifier.GetParameterDtosFor(this),
                librariesUpdated = UMI3DCollaborationServer.Instance.Identifier.getLibrariesUpdateSatus(this)
            };

            return connectionInformation;
        }

        public virtual UserDto ToUserDto(UMI3DUser user)
        {
            var _user = new UserDto
            {
                id = Id(),
                status = status,
                avatarId = Avatar == null ? 0 : Avatar.Id(),
                networkId = networkPlayer?.NetworkId ?? 0,
                audioSourceId = audioPlayer?.Id() ?? 0,
                audioFrequency = audioFrequency.GetValue(),
                videoSourceId = videoPlayer?.Id() ?? 0,
                login = string.IsNullOrEmpty(displayName) ? (string.IsNullOrEmpty(login) ? Id().ToString() : login) : displayName,

                microphoneStatus = microphoneStatus.GetValue(user),
                avatarStatus = avatarStatus.GetValue(user),
                attentionRequired = attentionRequired.GetValue(user),

                audioChannel = audioChannel.GetValue(user),
                audioServerUrl = audioServerUrl.GetValue(user),
                audioUseMumble = audioUseMumble.GetValue(user),
                audioLogin = audioLogin.GetValue(user),

                onStartSpeakingAnimationId = onStartSpeakingAnimationId.GetValue(user)?.Id() ?? 0,
                onStopSpeakingAnimationId = onStopSpeakingAnimationId.GetValue(user)?.Id() ?? 0,
            };
            return _user;
        }

        public virtual StatusDto ToStatusDto()
        {
            var status = new StatusDto
            {
                status = this.status
            };
            return status;
        }
    }
}