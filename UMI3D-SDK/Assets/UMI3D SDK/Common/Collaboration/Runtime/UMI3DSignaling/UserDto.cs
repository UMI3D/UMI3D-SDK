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

namespace umi3d.common.collaboration
{
    /// <summary>
    /// DTO describing a user as a collaborative entity.
    /// </summary>
    [Serializable]
    public class UserDto : AbstractEntityDto
    {
        /// <summary>
        /// Current attributed status of the user.
        /// </summary>
        public StatusType status;

        /// <summary>
        /// User's avatar UMI3D id.
        /// </summary>
        public ulong avatarId;

        /// <summary>
        /// Forge id of the user.
        /// </summary>
        public uint networkId;

        #region audio
        /// <summary>
        /// UMI3D id of the audio source where the user's voice comes from.
        /// </summary>
        public ulong audioSourceId;

        /// <summary>
        /// Voice recording frequency.
        /// </summary>
        public int audioFrequency;

        /// <summary>
        /// To stream video. Not used at the moment.
        /// </summary>
        public ulong videoSourceId;
        #endregion audio

        /// <summary>
        /// Name of the user to display.
        /// </summary>
        public string login;

        /// <summary>
        /// Is the user's microphone activated?
        /// </summary>
        public bool microphoneStatus;

        /// <summary>
        /// Has the user an avatar?
        /// </summary>
        public bool avatarStatus;

        /// <summary>
        /// Is the user indicating they require special attention?
        /// </summary>
        /// It is the equivalent of a "raise your end" feature in videochat.
        public bool attentionRequired;

        #region audioNetworking

        /// <summary>
        /// Name of the channel the user belongs to, on the audio server.
        /// </summary>
        public string audioChannel;

        /// <summary>
        /// URL of the audio server the user belongs to.
        /// </summary>
        public string audioServerUrl;

        /// <summary>
        /// Login of the user to connect to the audio server.
        /// </summary>
        public string audioLogin;

        /// <summary>
        /// Is the audio server a Murmur server?
        /// </summary>
        /// A Murmur server is used by Mumble clients.
        public bool audioUseMumble;

        #endregion audioNetworking

        #region audioAnimation 

        /// <summary>
        /// UMI3D id of the <see cref="UMI3DAnimationDto"/> to play when the user starts to talk.
        /// </summary>
        public ulong onStartSpeakingAnimationId;

        /// <summary>
        /// UMI3D id of the <see cref="UMI3DAnimationDto"/> to play when the user stops to talk.
        /// </summary>
        public ulong onStopSpeakingAnimationId;

        #endregion audioAnimation 

        public UserDto(UserDto source)
        {
            this.status = source.status;
            this.avatarId = source.avatarId;
            this.audioSourceId = source.audioSourceId;
            this.audioFrequency = source.audioFrequency;
            this.videoSourceId = source.videoSourceId;
            this.networkId = source.networkId;
            this.id = source.id;
            this.login = source.login;

            this.microphoneStatus = source.microphoneStatus;
            this.avatarStatus = source.avatarStatus;
            this.attentionRequired = source.attentionRequired;

            this.audioChannel = source.audioChannel;
            this.audioServerUrl = source.audioServerUrl;
            this.audioLogin = source.audioLogin;
            this.audioUseMumble = source.audioUseMumble;
        }

        public UserDto() { }
    }
}