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

namespace umi3d.common.collaboration.dto.signaling
{
    /// <summary>
    /// DTO describing a user as a collaborative entity.
    /// </summary>
    [Serializable]
    public class UserDto : AbstractEntityDto
    {
        /// <summary>
        /// String that state the langue of a user.
        /// </summary>
        public string language { get; set; }

        /// <summary>
        /// Current attributed status of the user.
        /// </summary>
        public StatusType status { get; set; }

        /// <summary>
        /// Forge id of the user.
        /// </summary>
        public uint networkId { get; set; }

        #region audio

        /// <summary>
        /// UMI3D id of the audio source where the user's voice comes from.
        /// </summary>
        public ulong audioSourceId { get; set; }

        /// <summary>
        /// Voice recording frequency.
        /// </summary>
        public int audioFrequency { get; set; }

        /// <summary>
        /// To stream video. Not used at the moment.
        /// </summary>
        public ulong videoSourceId { get; set; }

        #endregion audio

        /// <summary>
        /// Name of the user to display.
        /// </summary>
        public string login { get; set; }

        /// <summary>
        /// Is the user's microphone activated?
        /// </summary>
        public bool microphoneStatus { get; set; }

        /// <summary>
        /// Has the user an avatar?
        /// </summary>
        public bool avatarStatus { get; set; }

        /// <summary>
        /// Size of the user.
        /// </summary>
        public Vector3Dto userSize { get; set; } 

        /// <summary>
        /// Is the user indicating they require special attention?
        /// </summary>
        /// It is the equivalent of a "raise your end" feature in videochat.
        public bool attentionRequired { get; set; }

        #region audioNetworking

        /// <summary>
        /// Name of the channel the user belongs to, on the audio server.
        /// </summary>
        public string audioChannel { get; set; }

        /// <summary>
        /// URL of the audio server the user belongs to.
        /// </summary>
        public string audioServerUrl { get; set; }

        /// <summary>
        /// Login of the user to connect to the audio server.
        /// </summary>
        public string audioLogin { get; set; }

        /// <summary>
        /// Is the audio server a Murmur server?
        /// </summary>
        /// A Murmur server is used by Mumble clients.
        public bool audioUseMumble { get; set; }

        #endregion audioNetworking

        #region audioAnimation 

        /// <summary>
        /// UMI3D id of the <see cref="UMI3DAnimationDto"/> to play when the user starts to talk.
        /// </summary>
        public ulong onStartSpeakingAnimationId { get; set; }

        /// <summary>
        /// UMI3D id of the <see cref="UMI3DAnimationDto"/> to play when the user stops to talk.
        /// </summary>
        public ulong onStopSpeakingAnimationId { get; set; }

        #endregion audioAnimation 
    }
}