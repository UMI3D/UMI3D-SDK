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
        private readonly Dictionary<string, MumbleAudioPlayer> PendingMumbleAudioPlayer = new Dictionary<string, MumbleAudioPlayer>();

        private readonly Dictionary<ulong, MumbleAudioPlayer> GlobalReader = new Dictionary<ulong, MumbleAudioPlayer>();
        private readonly Dictionary<ulong, MumbleAudioPlayer> SpacialReader = new Dictionary<ulong, MumbleAudioPlayer>();
        private readonly Dictionary<ulong, Coroutine> WaitCoroutine = new Dictionary<ulong, Coroutine>();

        public bool SetGainForUser(UMI3DUser user, float gain)
        {
            if(user == null || gain <= 0)
                return false;
            var player = MumbleAudioPlayerContain(user.id);
            if (player == null)
                return false;

            player.Gain = gain;

            return true;
        }

        public float? GetGainForUser(UMI3DUser user)
        {
            if (user == null)
                return null;
            var player = MumbleAudioPlayerContain(user.id);
            if (player == null)
                return null;

            return player.Gain;
        }

        public bool SetVolumeForUser(UMI3DUser user, float volume)
        {
            if (user == null || volume < 0 || volume > 1)
                return false;
            var player = MumbleAudioPlayerContain(user.id);
            if (player == null)
                return false;

            player.SetVolume(volume);

            return true;
        }

        public float? GetVolumeForUser(UMI3DUser user)
        {
            if (user == null)
                return null;
            var player = MumbleAudioPlayerContain(user.id);
            if (player == null)
                return null;

            return player.GetVolume();
        }

        private void Start()
        {
            UMI3DUser.OnNewUser.AddListener(OnAudioChanged);
            UMI3DUser.OnRemoveUser.AddListener(OnUserDisconected);
            UMI3DUser.OnUserAudioUpdated.AddListener(OnAudioChanged);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UMI3DUser.OnNewUser.RemoveListener(OnAudioChanged);
            UMI3DUser.OnRemoveUser.RemoveListener(OnUserDisconected);
            UMI3DUser.OnUserAudioUpdated.RemoveListener(OnAudioChanged);
        }

        MumbleAudioPlayer MumbleAudioPlayerContain(ulong id)
        {
            if(SpacialReader.ContainsKey(id))
                return SpacialReader[id];
            if (GlobalReader.ContainsKey(id))
                return GlobalReader[id];
            return null;
        }


        public MumbleAudioPlayer GetMumbleAudioPlayer(UMI3DUser user)
        {
            if (user == null) return null;

            var userId = user.id;

            if (!string.IsNullOrEmpty(user.audioLogin) && PendingMumbleAudioPlayer.ContainsKey(user.audioLogin))
            {
                return PendingMumbleAudioPlayer[user.audioLogin];
            }

            return MumbleAudioPlayerContain(userId);
        }

        public MumbleAudioPlayer GetMumbleAudioPlayer(string username, uint session)
        {
            var user = UMI3DCollaborationEnvironmentLoader.Instance.UserList.FirstOrDefault(u => u.audioLogin == username);
            if (user != null)
            {
                MumbleAudioPlayer newPlayer = GetMumbleAudioPlayer(user);
                return newPlayer;
            }

            return CreatePrending(username);
        }

        MumbleAudioPlayer CreatePrending(string username) {
            var g = new GameObject
            {
                name = $"pending_audio_{username}"
            };
            PendingMumbleAudioPlayer[username] = g.AddComponent<MumbleAudioPlayer>();
            var audio = g.GetComponent<AudioSource>();
            audio.rolloffMode = AudioRolloffMode.Linear;
            audio.spatialBlend = 0;

            return PendingMumbleAudioPlayer[username];
        }
        void CleanPrending(UMI3DUser user) {
            if (!string.IsNullOrEmpty(user.audioLogin) && PendingMumbleAudioPlayer.ContainsKey(user.audioLogin))
            {
               PendingMumbleAudioPlayer[user.audioLogin].Reset();
               GameObject.Destroy(PendingMumbleAudioPlayer[user.audioLogin].gameObject);
               PendingMumbleAudioPlayer.Remove(user.audioLogin);
            }
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
            var oldReader = GetMumbleAudioPlayer(user);

            var audioPlayer = user?.audioplayer?.audioSource;
            if (audioPlayer != null)
            {
                var reader = audioPlayer.gameObject.GetOrAddComponent<MumbleAudioPlayer>();

                SpacialReader[user.id] = reader;

                if (oldReader != null && oldReader != reader)
                {
                    reader.Setup(oldReader);
                    oldReader.Reset();
                }
                if (GlobalReader.ContainsKey(user.id))
                {
                    Destroy(GlobalReader[user.id].gameObject);
                    GlobalReader.Remove(user.id);
                }
                CleanPrending(user);
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
                        GlobalReader[user.id] = g.AddComponent<MumbleAudioPlayer>();
                        var audio = g.GetComponent<AudioSource>();
                        audio.rolloffMode = AudioRolloffMode.Linear;
                        audio.spatialBlend = 0;
                        if (oldReader != null && oldReader != GlobalReader[user.id])
                        {
                            GlobalReader[user.id].Setup(oldReader);
                            oldReader.Reset();
                        }
                        CleanPrending(user);
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