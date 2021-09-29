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
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine.Events;

namespace umi3d.cdk.collaboration
{
    [Serializable]
    public class UMI3DUser
    {
        UserDto dto;

        public ulong id;
        public uint networkId;
        public StatusType status;
        public UMI3DAudioPlayer audioplayer { get => UMI3DAudioPlayer.Get(dto.audioSourceId); }
        public UMI3DVideoPlayer videoPlayer { get => UMI3DVideoPlayer.Get(dto.videoSourceId); }
        public UserAvatar avatar { get => UMI3DEnvironmentLoader.GetEntity(dto.id)?.Object as UserAvatar; }

        public UMI3DUser(UserDto user)
        {
            dto = user;
            id = user.id;
            networkId = user.networkId;
            status = user.status;
            OnNewUser.Invoke(this);
        }

        public void Destroy()
        {
            OnRemoveUser.Invoke(this);
        }

        /// <summary>
        /// Update user with a dto
        /// </summary>
        /// <param name="user"></param>
        public void Update(UserDto user)
        {
            bool statusUpdate = dto.status != user.status;
            bool avatarUpdate = dto.avatarId != user.avatarId;
            bool audioUpdate = dto.audioSourceId != user.audioSourceId;
            bool videoUpdate = dto.videoSourceId != user.videoSourceId;
            dto = user;
            status = user.status;
            if (statusUpdate) OnUserStatusUpdated.Invoke(this);
            if (avatarUpdate) OnUserAvatarUpdated.Invoke(this);
            if (audioUpdate) OnUserAudioUpdated.Invoke(this);
            if (videoUpdate) OnUserVideoUpdated.Invoke(this);
        }

        /// <summary>
        /// Event raising an UMI3DUser instance.
        /// </summary>
        [Serializable]
        public class UMI3DUserEvent : UnityEvent<UMI3DUser> { }

        static public UMI3DUserEvent OnNewUser = new UMI3DUserEvent();
        static public UMI3DUserEvent OnRemoveUser = new UMI3DUserEvent();
        static public UMI3DUserEvent OnUserAvatarUpdated = new UMI3DUserEvent();
        static public UMI3DUserEvent OnUserAudioUpdated = new UMI3DUserEvent();
        static public UMI3DUserEvent OnUserVideoUpdated = new UMI3DUserEvent();
        static public UMI3DUserEvent OnUserStatusUpdated = new UMI3DUserEvent();
    }
}