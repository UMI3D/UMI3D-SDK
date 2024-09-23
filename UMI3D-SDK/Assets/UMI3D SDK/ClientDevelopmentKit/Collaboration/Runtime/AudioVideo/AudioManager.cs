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
using inetum.unityUtils.audio;
using Mumble;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.collaboration
{

    public class AudioUserIsSpeaking : UnityEvent<UMI3DUser, bool> { }
    public class AudioUserData : UnityEvent<UMI3DUser, float[]> { }

    /// <summary>
    /// Manager for audio reading.
    /// </summary>
    public class AudioManager : SingleBehaviour<AudioManager>
    {
        private readonly Dictionary<string, MumbleAudioPlayer> PendingMumbleAudioPlayer = new Dictionary<string, MumbleAudioPlayer>();

        private readonly Dictionary<ulong, MumbleAudioPlayer> GlobalReader = new Dictionary<ulong, MumbleAudioPlayer>();
        private readonly Dictionary<ulong, MumbleAudioPlayer> SpacialReader = new Dictionary<ulong, MumbleAudioPlayer>();
        private readonly Dictionary<ulong, Coroutine> WaitCoroutine = new Dictionary<ulong, Coroutine>();

        public readonly Dictionary<string, float> volumeMemory = new Dictionary<string, float>();
        public readonly Dictionary<string, float> gainMemory = new Dictionary<string, float>();

        public AudioUserIsSpeaking OnUserSpeaking = new AudioUserIsSpeaking();
        public AudioUserData OnAudioUserData = new AudioUserData();

        private HashSet<Func<Task<bool>>> OpenMicrophoneValidators = new();

        public void Setup(Dictionary<string, float> volumeMemory, Dictionary<string, float> gainMemory)
        {
            this.volumeMemory.Clear();
            foreach (var k in volumeMemory)
            {
                this.volumeMemory[k.Key] = k.Value;
            }

            this.gainMemory.Clear();
            foreach (var k in gainMemory)
            {
                this.gainMemory[k.Key] = k.Value;
            }
        }

        public static async Task OnMicrophoneStatusRequest(bool status)
        {
            if(status == MicrophoneListener.mute)
            {
                if (status && Exists && Instance.OpenMicrophoneValidators.Count > 0)
                {
                    //check if possible
                    foreach(var task in Instance.OpenMicrophoneValidators.Select(f => f?.Invoke()))
                    {
                        if (!await task)
                            return;
                    }
                }
                MicrophoneListener.mute = !status;
            }
        }

        public static bool AddOpenMicrophoneValidators(Func<Task<bool>> validator) => Exists && Instance.OpenMicrophoneValidators.Add(validator);
        public static bool RemoveOpenMicrophoneValidators(Func<Task<bool>> validator) => Exists && Instance.OpenMicrophoneValidators.Remove(validator);


        public bool SetGainForUser(UMI3DUser user, float gain)
        {
            if (user == null || gain <= 0)
                return false;

            gainMemory[user.login] = gain;
            MumbleAudioPlayer player = MumbleAudioPlayerContain(user);

            if (player == null)
                return false;

            player.Gain = gain;

            return true;
        }

        public float? GetGainForUser(UMI3DUser user)
        {
            if (user == null)
                return null;
            MumbleAudioPlayer player = MumbleAudioPlayerContain(user);
            if (player == null)
            {
                if (gainMemory.ContainsKey(user.login))
                    return gainMemory[user.login];
                return null;
            }

            return player.Gain;
        }

        public bool SetVolumeForUser(UMI3DUser user, float volume)
        {
            if (user == null || volume < 0 || volume > 1)
                return false;
            volumeMemory[user.login] = volume;
            MumbleAudioPlayer player = MumbleAudioPlayerContain(user);
            if (player == null)
                return false;

            player.SetVolume(volume);

            return true;
        }

        public float? GetVolumeForUser(UMI3DUser user)
        {
            if (user == null)
                return null;
            MumbleAudioPlayer player = MumbleAudioPlayerContain(user);
            if (player == null)
            {
                if (volumeMemory.ContainsKey(user.login))
                    return volumeMemory[user.login];
                return null;
            }

            return player.GetVolume();
        }

        private void SetGainAndVolumeForUser(UMI3DUser user)
        {
            if (user == null)
                return;
            MumbleAudioPlayer player = MumbleAudioPlayerContain(user);
            if (player == null)
                return;

            if (gainMemory.ContainsKey(user.login))
                player.Gain = gainMemory[user.login];
            if (volumeMemory.ContainsKey(user.login))
                player.SetVolume(volumeMemory[user.login]);
            player.OnPlaying.AddListener(s => OnUserSpeaking.Invoke(user, s));
            player.OnAudioSample = ((data, u) => OnAudioUserData.Invoke(user, data));
        }

        private void Start()
        {
            UMI3DUser.OnNewUser.AddListener(OnAudioChanged);
            UMI3DUser.OnRemoveUser.AddListener(OnUserDisconnected);
            UMI3DUser.OnUserAudioUpdated.AddListener(OnAudioChanged);
            UMI3DUser.OnUserMicrophoneIdentityUpdated.AddListener(OnAudioChanged);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UMI3DUser.OnNewUser.RemoveListener(OnAudioChanged);
            UMI3DUser.OnRemoveUser.RemoveListener(OnUserDisconnected);
            UMI3DUser.OnUserAudioUpdated.RemoveListener(OnAudioChanged);
            UMI3DUser.OnUserMicrophoneIdentityUpdated.RemoveListener(OnAudioChanged);
        }

        private MumbleAudioPlayer MumbleAudioPlayerContain(UMI3DUser user)
        {
            if (user == null)
                return null;
            var player = MumbleAudioPlayerContain(user.id);

            return player;
        }

        private MumbleAudioPlayer MumbleAudioPlayerContain(ulong id)
        {
            if (SpacialReader.ContainsKey(id))
                return SpacialReader[id];

            if (GlobalReader.ContainsKey(id))
                return GlobalReader[id];

            return null;
        }

        public MumbleAudioPlayer GetMumbleAudioPlayer(UMI3DUser user)
        {
            if (user == null) return null;

            if (
                !string.IsNullOrEmpty(user.audioLogin) 
                && PendingMumbleAudioPlayer.ContainsKey(user.audioLogin) 
                && PendingMumbleAudioPlayer[user.audioLogin] != null)
                return PendingMumbleAudioPlayer[user.audioLogin];

            var tmp = MumbleAudioPlayerContain(user);
            return tmp;
        }

        public MumbleAudioPlayer GetMumbleAudioPlayer(string username, uint session)
        {
            UMI3DUser user = UMI3DCollaborationEnvironmentLoader.Instance.UserList.FirstOrDefault(u => u.audioLogin == username);
            if (user != null)
            {
                MumbleAudioPlayer newPlayer = GetMumbleAudioPlayer(user);
                if (newPlayer != null)
                    return newPlayer;
            }

            var tmp = CreatePending(username);
            Debug.Assert(tmp != null, "CreatePending return null for " + username + ", " + session);
            return tmp;
        }

        private MumbleAudioPlayer CreatePending(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            lock (PendingMumbleAudioPlayer)
                if (!PendingMumbleAudioPlayer.ContainsKey(username) || PendingMumbleAudioPlayer[username] == null)
                {
                    var g = new GameObject
                    {
                        name = $"pending_audio_{username}"
                    };
                    PendingMumbleAudioPlayer[username] = g.AddComponent<MumbleAudioPlayer>();
                    AudioSource audio = g.GetComponent<AudioSource>();
                    audio.rolloffMode = AudioRolloffMode.Linear;
                    audio.spatialBlend = 0;
                }
            return PendingMumbleAudioPlayer[username];
        }

        private void CleanPending(UMI3DUser user)
        {
            if (!string.IsNullOrEmpty(user.audioLogin) && PendingMumbleAudioPlayer.ContainsKey(user.audioLogin))
            {
                PendingMumbleAudioPlayer[user.audioLogin].Reset();
                GameObject.Destroy(PendingMumbleAudioPlayer[user.audioLogin].gameObject);
                PendingMumbleAudioPlayer.Remove(user.audioLogin);
            }
        }

        public bool DeletePending(string username, uint session)
        {
            MumbleAudioPlayer player = null;
            string name = username;

            if (!string.IsNullOrEmpty(username) && PendingMumbleAudioPlayer.ContainsKey(username))
                player = PendingMumbleAudioPlayer[username];
            else
            {
                var kp = PendingMumbleAudioPlayer.FirstOrDefault(pending => pending.Value != null && pending.Value.Session == session);
                player = kp.Value;
                name = kp.Key;
            }

            if (player != null && name != null)
            {
                player.Reset();
                GameObject.Destroy(player.gameObject);
                PendingMumbleAudioPlayer.Remove(name);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Manage user update
        /// </summary>
        /// <param name="user"></param>
        private void OnUserDisconnected(UMI3DUser user)
        {
            if (WaitCoroutine.ContainsKey(user.id))
            {
                StopCoroutine(WaitCoroutine[user.id]);
                WaitCoroutine.Remove(user.id);
            }

            var pending = CreatePending(user.audioLogin);

            if (GlobalReader.ContainsKey(user.id))
            {
                pending?.Setup(GlobalReader[user.id]);

                GlobalReader[user.id].Reset();
                Destroy(GlobalReader[user.id].gameObject);
                GlobalReader.Remove(user.id);
            }
            if (SpacialReader.ContainsKey(user.id))
            {
                pending?.Setup(SpacialReader[user.id]);

                SpacialReader[user.id].Reset();
                SpacialReader.Remove(user.id);
            }

            if (pending != null && !pending.IsMumbleClientSet())
            {
                CleanPending(user);
            }
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
            MumbleAudioPlayer oldReader = GetMumbleAudioPlayer(user);

            AudioSource audioPlayer = user?.audioplayer?.audioSource;
            if (audioPlayer != null)
            {
                MumbleAudioPlayer reader = audioPlayer.gameObject.GetOrAddComponent<MumbleAudioPlayer>();

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
                CleanPending(user);
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
                        AudioSource audio = g.GetComponent<AudioSource>();
                        audio.rolloffMode = AudioRolloffMode.Linear;
                        audio.spatialBlend = 0;
                        if (oldReader != null && oldReader != GlobalReader[user.id])
                        {
                            GlobalReader[user.id].Setup(oldReader);
                            oldReader.Reset();
                        }
                        CleanPending(user);
                    }
                }
            }
            SetGainAndVolumeForUser(user);
            if (audioPlayer is not null)
                AudioMixerControl.SetGroup(AudioMixerControl.Group.Conversation, audioPlayer);
        }

        private IEnumerator WaitForAudioCreation(UMI3DUser user)
        {
            yield return new WaitUntil(() => user?.audioplayer?.audioSource?.gameObject != null);
            OnAudioChanged(user);
        }

        public void ResetAudioConference()
        {
            MicrophoneListener.instance.ResetAudioConference();
            foreach (var wait in WaitCoroutine)
                if (wait.Value != null)
                    StopCoroutine(wait.Value);
            foreach (var pending in PendingMumbleAudioPlayer)
                if (pending.Value != null)
                {
                    pending.Value.Reset();
                    GameObject.Destroy(pending.Value.gameObject);
                }
            foreach (var global in GlobalReader)
                if (global.Value != null)
                {
                    global.Value.Reset();
                    GameObject.Destroy(global.Value);
                }
            foreach (var local in SpacialReader)
                if (local.Value != null)
                {
                    local.Value.Reset();
                    GameObject.Destroy(local.Value);
                }
            PendingMumbleAudioPlayer.Clear();
            GlobalReader.Clear();
            SpacialReader.Clear();
            WaitCoroutine.Clear();
        }
    }
}