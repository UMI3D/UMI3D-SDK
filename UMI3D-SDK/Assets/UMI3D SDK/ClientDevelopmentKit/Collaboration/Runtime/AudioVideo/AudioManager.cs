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

using inetum.unityUtils;
using Mumble;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace umi3d.cdk.collaboration
{

    /// <summary>
    /// Singleton use to read AudioDto.
    /// </summary>
    public class AudioManager : SingleBehaviour<AudioManager>
    {
        private readonly Dictionary<ulong, MumbleAudioPlayer> MumbleAudioPlayer = new Dictionary<ulong, MumbleAudioPlayer>();
        private readonly Dictionary<string, MumbleAudioPlayer> PendingMumbleAudioPlayer = new Dictionary<string, MumbleAudioPlayer>();

        private readonly Dictionary<ulong, AudioSource> GlobalReader = new Dictionary<ulong, AudioSource>();
        private readonly Dictionary<ulong, AudioSource> SpacialReader = new Dictionary<ulong, AudioSource>();
        private readonly Dictionary<ulong, Coroutine> WaitCoroutine = new Dictionary<ulong, Coroutine>();

        private void Start()
        {
            UMI3DUser.OnNewUser.AddListener(OnAudioChanged);
            UMI3DUser.OnRemoveUser.AddListener(OnUserDisconected);
            UMI3DUser.OnUserAudioUpdated.AddListener(OnAudioChanged);
        }

        public MumbleAudioPlayer GetMumbleAudioPlayer(UMI3DUser user)
        {
            var userId = user.id;

            if (!string.IsNullOrEmpty(user.audioLogin) && PendingMumbleAudioPlayer.ContainsKey(user.audioLogin))
            {
                if (MumbleAudioPlayer.ContainsKey(userId))
                    MumbleAudioPlayer[userId].Reset();

                MumbleAudioPlayer[userId] = PendingMumbleAudioPlayer[user.audioLogin];
                PendingMumbleAudioPlayer.Remove(user.audioLogin);
            }

            if (!MumbleAudioPlayer.ContainsKey(userId))
            {
                MumbleAudioPlayer[userId] = new MumbleAudioPlayer();
            }

            return MumbleAudioPlayer[userId];
        }

        public MumbleAudioPlayer GetMumbleAudioPlayer(string username, uint session)
        {
            var user = UMI3DCollaborationEnvironmentLoader.Instance.UserList.FirstOrDefault(u => u.audioLogin == username);
            if (user != null)
            {
                MumbleAudioPlayer newPlayer = GetMumbleAudioPlayer(user);
                return newPlayer;
            }
            PendingMumbleAudioPlayer[username] = new MumbleAudioPlayer();
            return PendingMumbleAudioPlayer[username];
        }


        /// <summary>
        /// MAnage user update
        /// </summary>
        /// <param name="user"></param>
        private void OnUserDisconected(UMI3DUser user)
        {
            if (WaitCoroutine.ContainsKey(user.id))
            {
                StopCoroutine(WaitCoroutine[user.id]);
                WaitCoroutine.Remove(user.id);
            }
            if (GlobalReader.ContainsKey(user.id))
            {
                Destroy(GlobalReader[user.id].gameObject);
                GlobalReader.Remove(user.id);
            }
            if (SpacialReader.ContainsKey(user.id))
                SpacialReader.Remove(user.id);
        }

        /// <summary>
        /// Manage user update
        /// </summary>
        /// <param name="user"></param>
        private void OnAudioChanged(UMI3DUser user)
        {
            if (WaitCoroutine.ContainsKey(user.id))
            {
                StopCoroutine(WaitCoroutine[user.id]);
                WaitCoroutine.Remove(user.id);
            }

            MumbleAudioPlayer reader = GetMumbleAudioPlayer(user);

            var audioPlayer = user?.audioplayer?.audioSource;
            if (audioPlayer != null)
            {
                SpacialReader[user.id] = audioPlayer;
                reader.Setup(audioPlayer);
                if (GlobalReader.ContainsKey(user.id))
                {
                    Destroy(GlobalReader[user.id].gameObject);
                    GlobalReader.Remove(user.id);
                }
            }
            else
            {
                if (user.audioPlayerId != 0)
                {
                    WaitCoroutine[user.id] = StartCoroutine(WaitForAudioCreation(user));
                }
                else
                {
                    if (SpacialReader.ContainsKey(user.id))
                        SpacialReader.Remove(user.id);
                    if (!GlobalReader.ContainsKey(user.id))
                    {
                        var g = new GameObject
                        {
                            name = $"user_{user.id}_audio_reader"
                        };
                        GlobalReader[user.id] = g.AddComponent<AudioSource>();
                        reader.Setup(GlobalReader[user.id]);
                    }
                }
            }
        }

        private IEnumerator WaitForAudioCreation(UMI3DUser user)
        {
            yield return new WaitUntil(() => user?.audioplayer?.audioSource?.gameObject != null);
            OnAudioChanged(user);
        }
    }
}