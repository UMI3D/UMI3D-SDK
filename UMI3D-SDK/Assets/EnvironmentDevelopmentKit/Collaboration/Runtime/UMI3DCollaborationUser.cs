﻿/*
Copyright 2019 Gfi Informatique

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
using umi3d.common.collaboration;
using umi3d.edk.userCapture;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class UMI3DCollaborationUser : UMI3DTrackedUser
    {
        public UMI3DCollaborationUser(string login)
        {
            this.login = login;
            Debug.Log($"<color=magenta>new User {Id()}</color>");
        }

        public void InitConnection(UMI3DWebSocketConnection connection)
        {
            this.connection = connection;
            UserConnectionDto ucDto = new UserConnectionDto(ToUserDto());
            ucDto.librariesUpdated = !UMI3DEnvironment.UseLibrary();
            RenewToken();
            SetStatus(UMI3DCollaborationServer.Instance.Identifier.UpdateIdentity(this, ucDto));
        }

        public void Logout()
        {
            UMI3DEnvironment.Remove(this);
        }

        /// <summary>
        /// Does the user have a devise compatible with webrtc
        /// </summary>
        public bool useWebrtc { get; set; }

        /// <summary>
        /// The user token
        /// </summary>
        public string token { get; private set; }
        

        /// <summary>
        /// The unique user login.
        /// </summary>
        public string login;

        public UMI3DAbstractWebSocketConnection connection;
        public UMI3DAbstractWebSocketConnection reliableData;
        public UMI3DAbstractWebSocketConnection unreliableData;
        public UMI3DAbstractWebSocketConnection reliableTracking;
        public UMI3DAbstractWebSocketConnection unreliableTracking;
        public UMI3DAbstractWebSocketConnection audio;
        public UMI3DAbstractWebSocketConnection video;

        public UMI3DAudioPlayer audioPlayer;
        public UMI3DAudioPlayer videoPlayer;

        public List<DataChannel> dataChannels = new List<DataChannel>();

        public string RenewToken()
        {
            DateTime date = DateTime.UtcNow;
            date = date.AddSeconds(UMI3DCollaborationServer.Instance.tokenLifeTime);
            byte[] time = BitConverter.GetBytes(date.ToBinary());
            byte[] key = Guid.NewGuid().ToByteArray();
            string token = Convert.ToBase64String(time.Concat(key).ToArray());
            this.token = token;
            connection.SendData(ToTokenDto());
            return token;
        }

        public virtual TokenDto ToTokenDto()
        {
            TokenDto token = new TokenDto();
            token.token = this.token;
            return token;
        }

        ///<inheritdoc/>
        public override void SetStatus(StatusType status)
        {
            base.SetStatus(status);
            UMI3DCollaborationServer.Instance.NotifyUserStatusChanged(this, status);
        }


        public virtual UserDto ToUserDto()
        {
            UserDto user = new UserDto();
            user.id = Id();
            user.status = status;
            user.avatarId = Avatar == null ? null : Avatar.Id();
            return user;
        }

        public virtual StatusDto ToStatusDto()
        {
            StatusDto status = new StatusDto();
            status.status = this.status;
            return status;
        }

    }
}