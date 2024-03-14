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

using BeardedManStudios.Forge.Networking;
using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
using umi3d.edk.userCapture.tracking;

namespace umi3d.edk.collaboration
{

    /// <summary>
    /// <see cref="UMI3DTrackedUser"/> in a collaborative context, with authentication, sound and video streaming etc...
    /// </summary>
    public abstract class UMI3DCollaborationAbstractContentUser : UMI3DCollaborationAbstractUser
    {


        public UMI3DAsyncProperty<string> audioChannel;
        public UMI3DAsyncProperty<string> audioServerUrl;
        public UMI3DAsyncProperty<bool> audioUseMumble;


        /// <summary>
        /// Current id for ForgeNetworkingRemastered
        /// </summary>
        public NetworkingPlayer networkPlayer { get; set; }


        public UMI3DForgeServer forgeServer;

        /// <summary>
        /// Room object used to relay data
        /// </summary>
        public ICollaborationRoom RelayRoom;

        public UMI3DCollaborationAbstractContentUser(RegisterIdentityDto identity) : base(identity)
        {
            audioChannel = new UMI3DAsyncProperty<string>(userId, UMI3DPropertyKeys.UserAudioChannel, null);
            audioServerUrl = new UMI3DAsyncProperty<string>(userId, UMI3DPropertyKeys.UserAudioServer, null);
            audioUseMumble = new UMI3DAsyncProperty<bool>(userId, UMI3DPropertyKeys.UserAudioUseMumble, false);
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