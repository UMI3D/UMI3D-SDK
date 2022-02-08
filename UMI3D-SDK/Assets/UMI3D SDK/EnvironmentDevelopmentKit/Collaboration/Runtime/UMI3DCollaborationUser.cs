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
using System;
using System.Linq;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.edk.userCapture;

namespace umi3d.edk.collaboration
{
    public class UMI3DCollaborationUser : UMI3DTrackedUser
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.User;
        public UMI3DCollaborationUser(string login)
        {
            this.login = login;
            status = StatusType.CREATED;
            UMI3DLogger.Log($"<color=magenta>new User {Id()} {login}</color>", scope);
        }

        public void InitConnection(UMI3DForgeServer connection)
        {
            this.forgeServer = connection;
            var ucDto = new UserConnectionAnswerDto(ToUserDto())
            {
                librariesUpdated = !UMI3DEnvironment.UseLibrary()
            };
            RenewToken();
            SetStatus(UMI3DCollaborationServer.Instance.Identifier.UpdateIdentity(this, ucDto));
        }

        public void Logout()
        {
            UMI3DEnvironment.Remove(this);
        }

        /// <summary>
        /// Current id for ForgeNetworkingRemastered
        /// </summary>
        public NetworkingPlayer networkPlayer { get; set; }

        /// <summary>
        /// The user token
        /// </summary>
        public string token { get; private set; }


        /// <summary>
        /// The unique user login.
        /// </summary>
        public string login;

        public UMI3DForgeServer forgeServer;

        public UMI3DAudioPlayer audioPlayer;
        public int audioFrequency = 12000;
        public UMI3DAudioPlayer videoPlayer;

        public void NotifyUpdate()
        {
            UMI3DCollaborationServer.Collaboration.NotifyUserStatusChanged(this);
        }


        public string RenewToken()
        {
            DateTime date = DateTime.UtcNow;
            date = date.AddSeconds(UMI3DCollaborationServer.Instance.tokenLifeTime);
            byte[] time = BitConverter.GetBytes(date.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());
            this.token = token;
            forgeServer.SendSignalingMessage(networkPlayer, ToTokenDto());
            return token;
        }

        public virtual TokenDto ToTokenDto()
        {
            var token = new TokenDto
            {
                token = this.token
            };
            return token;
        }

        ///<inheritdoc/>
        public override void SetStatus(StatusType status)
        {
            bool isSame = status == this.status;
            base.SetStatus(status);
            if (!isSame)
                UMI3DCollaborationServer.Instance.NotifyUserStatusChanged(this, status);
        }


        public virtual UserDto ToUserDto()
        {
            var user = new UserDto
            {
                id = Id(),
                status = status,
                avatarId = Avatar == null ? 0 : Avatar.Id(),
                networkId = networkPlayer?.NetworkId ?? 0,
                audioSourceId = audioPlayer?.Id() ?? 0,
                audioFrequency = audioFrequency,
                videoSourceId = videoPlayer?.Id() ?? 0
            };
            return user;
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