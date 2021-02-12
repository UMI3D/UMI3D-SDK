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

using System.Collections.Generic;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.cdk.collaboration
{

    /// <summary>
    /// Singleton use to read AudioDto.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        Dictionary<string, AudioReader> GlobalReader = new Dictionary<string, AudioReader>();
        Dictionary<string, AudioReader> SpacialReader = new Dictionary<string, AudioReader>();

        private void Start()
        {
            UMI3DUser.OnNewUser.AddListener(OnAudioChanged);
            UMI3DUser.OnRemoveUser.AddListener(OnUserDisconected);
            UMI3DUser.OnUserAudioUpdated.AddListener(OnAudioChanged);

            if (MicrophoneListener.Exists)
                MicrophoneListener.Instance.StartRecording();
        }

        /// <summary>
        /// Read a Voice Dto and dispatched it in the right audioSource.
        /// </summary>
        /// <param name="userId"> the speaking user</param>
        /// <param name="dto"> the voice dto</param>
        public void Read(string userId, VoiceDto dto)
        {
            if (SpacialReader.ContainsKey(userId))
                SpacialReader[userId].Read(dto);
            else if (GlobalReader.ContainsKey(userId))
                GlobalReader[userId].Read(dto);
        }



        /// <summary>
        /// MAnage user update
        /// </summary>
        /// <param name="user"></param>
        void OnUserDisconected(UMI3DUser user)
        {
            if (GlobalReader.ContainsKey(user.id))
            {
                Destroy(GlobalReader[user.id].gameObject);
                GlobalReader.Remove(user.id);
            }
            if (SpacialReader.ContainsKey(user.id))
                SpacialReader.Remove(user.id);
        }

        /// <summary>
        /// MAnage user update
        /// </summary>
        /// <param name="user"></param>
        void OnAudioChanged(UMI3DUser user)
        {
            var audioPlayer = user.audioplayer;
            if (audioPlayer != null)
            {
                var reader = audioPlayer.audioSource.gameObject.GetOrAddComponent<AudioReader>();
                SpacialReader[user.id] = reader;
                if (GlobalReader.ContainsKey(user.id))
                {
                    Destroy(GlobalReader[user.id].gameObject);
                    GlobalReader.Remove(user.id);
                }
            }
            else
            {
                if (SpacialReader.ContainsKey(user.id))
                    SpacialReader.Remove(user.id);
                if (!GlobalReader.ContainsKey(user.id))
                {
                    var g = new GameObject($"Audio Reader {user.id}");
                    g.name = user.id;
                    GlobalReader[user.id] = g.AddComponent<AudioReader>();
                }
            }
        }

    }
}