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
    /// <summary>
    /// UMI3D user representation.
    /// </summary>
    [Serializable]
    public class UMI3DUser
    {
        /// <summary>
        /// DTO local copy.
        /// </summary>
        private UserDto dto;

        /// <summary>
        /// See <see cref="UserDto.id"/>.
        /// </summary>
        public ulong id => dto.id;
        /// <summary>
        /// See <see cref="UserDto.networkId"/>.
        /// </summary>
        public uint networkId => dto.networkId;
        /// <summary>
        /// See <see cref="UserDto.status"/>.
        /// </summary>
        public StatusType status => dto.status;

        /// <summary>
        /// See <see cref="UserDto.audioSourceId"/>.
        /// </summary>
        public ulong audioPlayerId => dto.audioSourceId;
        /// <summary>
        /// See <see cref="UserDto.audioFrequency"/>.
        /// </summary>
        public int audioFrequency => dto.audioFrequency;
        /// <summary>
        /// Audio player attached to the user.
        /// </summary>
        public UMI3DAudioPlayer audioplayer => UMI3DAudioPlayer.Get(dto.audioSourceId);
        /// <summary>
        /// See <see cref="UserDto.videoSourceId"/>.
        /// </summary>
        public ulong videoPlayerId => dto.videoSourceId;
        /// <summary>
        /// Video player attached to the user.
        /// </summary>
        public UMI3DVideoPlayer videoPlayer => UMI3DVideoPlayer.Get(dto.videoSourceId);

        /// <summary>
        /// Virtual representation of the user in the media.
        /// </summary>
        public UserAvatar avatar => UMI3DEnvironmentLoader.GetEntity(dto.id)?.Object as UserAvatar;

        /// <summary>
        /// See <see cref="UserDto.microphoneStatus"/>.
        /// </summary>
        public bool microphoneStatus => dto.microphoneStatus;
        /// <summary>
        /// See <see cref="UserDto.avatarStatus"/>.
        /// </summary>
        public bool avatarStatus => dto.avatarStatus;
        /// <summary>
        /// See <see cref="UserDto.attentionRequired"/>.
        /// </summary>
        public bool attentionRequired => dto.attentionRequired;

        /// <summary>
        /// See <see cref="UserDto.audioUseMumble"/>.
        /// </summary>
        public bool useMumble => dto.audioUseMumble;
        /// <summary>
        /// See <see cref="UserDto.audioLogin"/>.
        /// </summary>
        public string audioLogin => dto.audioLogin;
        /// <summary>
        /// See <see cref="UMI3DEnvironmentClient.UserInfo.AudioPassword"/>.
        /// </summary>
        public string audioPassword
        {
            get
            {
                if (isClient && UMI3DCollaborationClientServer.Exists)
                {
                    UMI3DEnvironmentClient.UserInfo user = UMI3DCollaborationClientServer.Instance.GetUser();
                    if (user != null)
                        return user.AudioPassword;
                }
                return null;
            }
            private set
            {
                if (isClient && UMI3DCollaborationClientServer.Exists)
                {
                    UMI3DEnvironmentClient.UserInfo user = UMI3DCollaborationClientServer.Instance.GetUser();
                    if (user != null)
                        user.AudioPassword = value;
                }
            }
        }
        /// <summary>
        /// See <see cref="UserDto.audioChannel"/>.
        /// </summary>
        public string audioChannel => dto.audioChannel;
        /// <summary>
        /// See <see cref="UserDto.audioServerUrl"/>.
        /// </summary>
        public string audioServer => dto.audioServerUrl;

        /// <summary>
        /// See <see cref="UserDto.onStartSpeakingAnimationId"/>.
        /// </summary>
        public ulong onStartSpeakingAnimationId => dto.onStartSpeakingAnimationId;
        /// <summary>
        /// See <see cref="UserDto.onStopSpeakingAnimationId"/>.
        /// </summary>
        public ulong onStopSpeakingAnimationId => dto.onStopSpeakingAnimationId;

        /// <summary>
        /// See <see cref="UserDto.login"/>.
        /// </summary>
        public string login => dto?.login;

        public bool isClient => id == UMI3DCollaborationClientServer.Instance.GetUserId();

        public UMI3DUser(UserDto user)
        {
            dto = user;
            UMI3DEnvironmentLoader.RegisterEntityInstance(dto.id, dto, null).NotifyLoaded();
            OnNewUser.Invoke(this);
        }

        public void Destroy()
        {
            UMI3DEnvironmentLoader.DeleteEntity(dto.id);
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
            bool audioFrequencyUpdate = dto.audioFrequency != user.audioFrequency;

            bool microphoneStatusUpdate = dto.microphoneStatus != user.microphoneStatus;
            bool avatarStatusUpdate = dto.avatarStatus != user.avatarStatus;
            bool attentionStatusUpdate = dto.attentionRequired != user.attentionRequired;

            bool useMumbleUpdate = dto.audioUseMumble != user.audioUseMumble;
            bool channelUpdate = dto.audioChannel != user.audioChannel;
            bool serverUpdate = dto.audioServerUrl != user.audioServerUrl;

            bool loginUpdate = dto.audioLogin != user.audioLogin;
            bool pswUpdate = false;

            dto = user;

            if (isClient && user is UserConnectionDto connectionDto)
            {

                pswUpdate = connectionDto.audioPassword != audioPassword;

                audioPassword = connectionDto.audioPassword;
            }

            if (statusUpdate) OnUserStatusUpdated.Invoke(this);
            if (avatarUpdate) OnUserAvatarUpdated.Invoke(this);
            if (audioUpdate) OnUserAudioUpdated.Invoke(this);
            if (videoUpdate) OnUserVideoUpdated.Invoke(this);
            if (audioFrequencyUpdate) OnUserAudioFrequencyUpdated.Invoke(this);
            if (attentionStatusUpdate) OnUserAttentionStatusUpdated.Invoke(this);
            if (microphoneStatusUpdate) OnUserMicrophoneStatusUpdated.Invoke(this);
            if (avatarStatusUpdate) OnUserAvatarStatusUpdated.Invoke(this);
            if (useMumbleUpdate) OnUserMicrophoneUseMumbleUpdated.Invoke(this);
            if (channelUpdate) OnUserMicrophoneChannelUpdated.Invoke(this);
            if (serverUpdate) OnUserMicrophoneServerUpdated.Invoke(this);
            if (loginUpdate || pswUpdate) OnUserMicrophoneIdentityUpdated.Invoke(this);
        }

        public bool UpdateUser(ulong property, object value)
        {
            switch (property)
            {
                case UMI3DPropertyKeys.UserMicrophoneStatus:
                    dto.microphoneStatus = (bool)value;
                    OnUserMicrophoneStatusUpdated.Invoke(this);
                    return true;

                case UMI3DPropertyKeys.UserAttentionRequired:
                    dto.attentionRequired = (bool)value;
                    OnUserAttentionStatusUpdated.Invoke(this);
                    return true;

                case UMI3DPropertyKeys.UserAvatarStatus:
                    dto.avatarStatus = (bool)value;
                    OnUserAvatarStatusUpdated.Invoke(this);
                    return true;

                case UMI3DPropertyKeys.UserAudioFrequency:
                    dto.audioFrequency = (int)value;
                    OnUserAudioFrequencyUpdated.Invoke(this);
                    return true;

                case UMI3DPropertyKeys.UserAudioUseMumble:
                    dto.audioUseMumble = (bool)value;
                    OnUserMicrophoneUseMumbleUpdated.Invoke(this);
                    return true;


                case UMI3DPropertyKeys.UserAudioPassword:
                    audioPassword = (string)value;
                    OnUserMicrophoneIdentityUpdated.Invoke(this);
                    return true;

                case UMI3DPropertyKeys.UserAudioLogin:
                    dto.audioLogin = (string)value;
                    OnUserMicrophoneIdentityUpdated.Invoke(this);
                    return true;

                case UMI3DPropertyKeys.UserAudioServer:
                    dto.audioServerUrl = (string)value;
                    OnUserMicrophoneServerUpdated.Invoke(this);
                    return true;

                case UMI3DPropertyKeys.UserAudioChannel:
                    dto.audioChannel = (string)value;
                    OnUserMicrophoneChannelUpdated.Invoke(this);
                    return true;

                case UMI3DPropertyKeys.UserOnStartSpeakingAnimationId:
                    dto.onStartSpeakingAnimationId = (ulong)value;
                    return true;

                case UMI3DPropertyKeys.UserOnStopSpeakingAnimationId:
                    dto.onStopSpeakingAnimationId = (ulong)value;
                    return true;
            }
            return false;
        }


        public void SetMicrophoneStatus(bool microphoneStatus)
        {
            if (dto.microphoneStatus != microphoneStatus)
            {
                UMI3DClientServer.SendData(ConferenceBrowserRequest.GetChangeMicrophoneStatusRequest(id, microphoneStatus), true);
            }
        }
        public void SetAvatarStatus(bool avatarStatus)
        {
            if (dto.avatarStatus != avatarStatus)
            {
                UMI3DClientServer.SendData(ConferenceBrowserRequest.GetChangeAvatarStatusRequest(id, avatarStatus), true);
            }
        }
        public void SetAttentionStatus(bool attentionStatus)
        {
            if (dto.attentionRequired != attentionStatus)
            {
                UMI3DClientServer.SendData(ConferenceBrowserRequest.GetChangeAttentionStatusRequest(id, attentionStatus), true);
            }
        }

        public static void MuteAllMicrophone()
        {
            UMI3DClientServer.SendData(ConferenceBrowserRequest.GetMuteAllMicrophoneRequest(), true);
        }
        public static void MuteAllAvatar()
        {
            UMI3DClientServer.SendData(ConferenceBrowserRequest.GetMuteAllAvatarRequest(), true);
        }
        public static void MuteAllAttention()
        {
            UMI3DClientServer.SendData(ConferenceBrowserRequest.GetMuteAllAttentionRequest(), true);
        }

        public override string ToString()
        {
            return $"[User {id} : login : {login}, audioLogin : {audioLogin}, status : {status}]";
        }

        /// <summary>
        /// Event raising an UMI3DUser instance.
        /// </summary>
        [Serializable]
        public class UMI3DUserEvent : UnityEvent<UMI3DUser> { }

        public static UMI3DUserEvent OnNewUser = new UMI3DUserEvent();
        public static UMI3DUserEvent OnRemoveUser = new UMI3DUserEvent();

        public static UMI3DUserEvent OnUserAvatarUpdated = new UMI3DUserEvent();
        public static UMI3DUserEvent OnUserAudioUpdated = new UMI3DUserEvent();
        public static UMI3DUserEvent OnUserVideoUpdated = new UMI3DUserEvent();
        public static UMI3DUserEvent OnUserStatusUpdated = new UMI3DUserEvent();

        public static UMI3DUserEvent OnUserAudioFrequencyUpdated = new UMI3DUserEvent();
        public static UMI3DUserEvent OnUserMicrophoneStatusUpdated = new UMI3DUserEvent();
        public static UMI3DUserEvent OnUserAvatarStatusUpdated = new UMI3DUserEvent();
        public static UMI3DUserEvent OnUserAttentionStatusUpdated = new UMI3DUserEvent();

        public static UMI3DUserEvent OnUserMicrophoneIdentityUpdated = new UMI3DUserEvent();
        public static UMI3DUserEvent OnUserMicrophoneServerUpdated = new UMI3DUserEvent();
        public static UMI3DUserEvent OnUserMicrophoneChannelUpdated = new UMI3DUserEvent();
        public static UMI3DUserEvent OnUserMicrophoneUseMumbleUpdated = new UMI3DUserEvent();

    }
}