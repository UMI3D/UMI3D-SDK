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

using BeardedManStudios.Forge.Networking.Unity;
using inetum.unityUtils;
using Mumble;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    [Serializable]
    public class MicrophoneEvent : UnityEngine.Events.UnityEvent<bool>
    {
    }

    public enum MicrophoneMode
    {
        AlwaysSend,
        Amplitude,
        PushToTalk,
        MethodBased
    }

    [RequireComponent(typeof(AudioSource))]
    public class MicrophoneListener : SingleBehaviour<MicrophoneListener>, ILoggable
    {
        #region Mumble
        #region public field

        public static MicrophoneEvent OnSaturated = new MicrophoneEvent();

        private bool micIsOn => mumbleMic?.isRecording ?? false;

        public bool useLocalLoopback
        {
            get => debuggingVariables?.UseLocalLoopback ?? false;
            set { if (debuggingVariables != null) debuggingVariables.UseLocalLoopback = value; }
        }

        public static bool isMute
        {
            get => !Exists || !Instance.useMumble || muted;

            set { muted = value; if (Exists) Instance.handleMute(); }
        }

        public float minAmplitudeToSend
        {
            get => mumbleMic?.MinAmplitude ?? 0; set
            {
                if (mumbleMic != null && value >= 0f && value <= 1f)
                    mumbleMic.MinAmplitude = value;
            }
        }
        public float voiceStopingDelaySeconds
        {
            get => mumbleMic?.VoiceHoldSeconds ?? 0; set
            {
                if (mumbleMic != null && value >= 0f)
                    mumbleMic.VoiceHoldSeconds = value;
            }
        }
        public KeyCode pushToTalkKeycode
        {
            get => mumbleMic?.PushToTalkKeycode ?? KeyCode.M; set
            {
                if (mumbleMic != null)
                    mumbleMic.PushToTalkKeycode = value;
            }
        }

        public bool Debug
        {
            get => _debug;
            set
            {
                _debug = value;
                rms = 0;
                db = 0;
                saturated = false;
            }
        }

        public bool saturated
        {
            get => _saturated;
            private set
            {
                if (_saturated != value)
                {
                    _saturated = value;
                    OnSaturated.Invoke(value);
                }
            }
        }
        public float rms { get; private set; }
        public float db { get; private set; }

        #endregion
        #region private field

        private string _pendingMic = null;
        private MumbleClient mumbleClient;
        private MumbleMicrophone mumbleMic;
        private DebugValues debuggingVariables;
        private const bool connectAsyncronously = true;
        private const bool sendPosition = false;
        private static bool muted;

        /// <summary>
        ///  RMS value for 0 dB
        /// </summary>
        private const float refValue = 1f;
        private bool _saturated = false;
        private bool _debug = false;
        private string hostName = "1.2.3.4";
        private int port = 64738;
        private string username = "ExampleUser";
        private string password = "1passwordHere!";
        private string channelToJoin = "";
        private bool useMumble = false;
        private bool playing = false;
        private bool playingInit = false;
        private bool IdentityUpdateOnce = false;
        #endregion

        #region Init
        protected void Start()
        {
            mumbleMic = gameObject.GetOrAddComponent<MumbleMicrophone>();

            if (_pendingMic != null)
                SetCurrentMicrophoneNameAsync(_pendingMic);

            gameObject.GetOrAddComponent<EventProcessor>();

            debuggingVariables = new DebugValues()
            {
                EnableEditorIOGraph = false,
                UseLocalLoopback = false,
                UseRandomUsername = false,
                UseSyntheticSource = false
            };

            mumbleMic.OnMicData += DebugSample;

            QuittingManager.OnApplicationIsQuitting.AddListener(_OnApplicationQuit);

            UMI3DUser.OnUserMicrophoneIdentityUpdated.AddListener(IdentityUpdate);
            UMI3DUser.OnUserMicrophoneChannelUpdated.AddListener(ChannelUpdate);
            UMI3DUser.OnUserMicrophoneServerUpdated.AddListener(ServerUpdate);
            UMI3DUser.OnUserMicrophoneUseMumbleUpdated.AddListener(UseMumbleUpdate);
            UMI3DCollaborationClientServer.Instance.OnRedirection.AddListener(Reset);

            pushToTalkKeycode = KeyCode.M;
        }

        private void _OnApplicationQuit()
        {
            StopMicrophoneAsync();
            UMI3DUser.OnUserMicrophoneIdentityUpdated.RemoveListener(IdentityUpdate);
            UMI3DUser.OnUserMicrophoneChannelUpdated.RemoveListener(ChannelUpdate);
            UMI3DUser.OnUserMicrophoneServerUpdated.RemoveListener(ServerUpdate);
            UMI3DUser.OnUserMicrophoneUseMumbleUpdated.RemoveListener(UseMumbleUpdate);
            UMI3DCollaborationClientServer.Instance.OnRedirection.RemoveListener(Reset);
        }
        #endregion

        public async void StartMicrophoneAsync()
        {
            await StartMicrophone();
        }

        public async Task StartMicrophone()
        {
            if (!playingInit)
            {
                playingInit = true;
                await _StartMicrophone();
                playingInit = false;
            }
        }

        public async void StopMicrophoneAsync()
        {
            await StopMicrophone();
        }

        public async Task StopMicrophone()
        {
            await ForceStopMicrophone(false);
        }

        private async Task ForceStopMicrophone(bool force = false)
        {
            if (force || await IsPLaying())
            {
                try
                {
                    if (mumbleMic != null)
                    {
                        mumbleMic.OnMicDisconnect -= OnMicDisconnected;
                        MainThreadManager.Run(
                            mumbleMic.StopSendingAudio);
                    }
                    if (mumbleClient != null)
                    {
                        mumbleClient.ConnectionError.RemoveListener(Failed);
                        mumbleClient.connectionFailed.RemoveListener(Failed);
                        mumbleClient.OnDisconnected -= OnDisconected;

                        mumbleClient.Close();
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
            mumbleClient = null;
            playingInit = false;
            playing = false;
        }

        private void OnMicDisconnected()
        {
            string disconnectedMicName = mumbleMic.GetCurrentMicName();
            StartCoroutine(ExampleMicReconnect(disconnectedMicName));
        }


        public async void ToggleSendingSound(bool? on = null)
        {
            if (on == null)
                on = !mumbleMic.isRecording;

            if (await IsPLaying() && mumbleMic != null && GetCurrentMicrophoneMode() == MicrophoneMode.MethodBased)
            {
                if (on == mumbleMic.isRecording)
                    return;

                if (mumbleMic.isRecording)
                    mumbleMic.StopSendingAudio();
                else
                    mumbleMic.StartSendingAudio();
            }
        }

        #region private method

        void UpdateUser()
        {
            if (UMI3DCollaborationEnvironmentLoader.Exists)
            {
                UMI3DUser user = UMI3DCollaborationEnvironmentLoader.Instance.GetClientUser();
                if (user != null)
                {
                    username = user.audioLogin;
                    password = user.audioPassword;
                    SetMumbleUrl(user.audioServer);
                    channelToJoin = user.audioChannel;
                    useMumble = user.useMumble;
                }
            }
        }

        private async Task _StartMicrophone()
        {
            UpdateUser();
            try
            {
                if (!useMumble) return;

                if (hostName == "1.2.3.4")
                {
                    UnityEngine.Debug.LogError("Please set the mumble host name to your mumble server");
                    return;
                }

                if (UMI3DCollaborationEnvironmentLoader.Exists)
                {
                    UMI3DUser user = UMI3DCollaborationEnvironmentLoader.Instance.GetClientUser();
                    if (user != null)
                    {
                        username = user.audioLogin;
                        password = user.audioPassword;
                        SetMumbleUrl(user.audioServer);
                        channelToJoin = user.audioChannel;
                        useMumble = user.useMumble;

                        Application.runInBackground = true;
                        // If SendPosition, we'll send three floats.
                        // This is roughly the standard for Mumble, however it seems that
                        // Murmur supports more
                        int posLength = sendPosition ? 3 * sizeof(float) : 0;
                        mumbleClient = new MumbleClient(hostName, port, CreateMumbleAudioPlayerFromPrefab,
                            DestroyMumbleAudioPlayer, OnOtherUserStateChange, connectAsyncronously,
                            SpeakerCreationMode.ALL, debuggingVariables, posLength);
                        mumbleClient.ConnectionError.AddListener(Failed);

                        if (connectAsyncronously)
                            while (!mumbleClient.ReadyToConnect)
                                await UMI3DAsyncManager.Yield();

                        mumbleClient.Connect(username, password);
                        mumbleClient.connectionFailed.AddListener(Failed);
                        mumbleClient.OnDisconnected += OnDisconected;

                        if (connectAsyncronously)
                            await UMI3DAsyncManager.Yield();

                        if (mumbleMic != null)
                        {
                            if (_pendingMic != null)
                                SetMicrophone(_pendingMic);
                            mumbleMic.SendAudioOnStart = false;
                            mumbleClient.AddMumbleMic(mumbleMic);
                            _pendingMic = null;
                            mumbleClient.SetSelfMute(isMute);
                            if (sendPosition)
                                mumbleMic.SetPositionalDataFunction(WritePositionalData);
                            mumbleMic.OnMicDisconnect += OnMicDisconnected;

                            await JoinChannel();

                            if (mumbleMic.VoiceSendingType == MumbleMicrophone.MicType.AlwaysSend
                                    || mumbleMic.VoiceSendingType == MumbleMicrophone.MicType.Amplitude)
                                mumbleMic.StartSendingAudio();

                            playing = true;

                            return;
                        }
                    }
                }
            }
            catch
            {
                await ForceStopMicrophone(true);
            }
            playing = false;
        }

        async void Failed(Exception e)
        {
            await ForceStopMicrophone(true);
        }

        async void OnDisconected()
        {
            await ForceStopMicrophone(true);
        }

        async void Failed()
        {
            await ForceStopMicrophone(true);
        }

        private async Task JoinChannel(int trycount = 0)
        {
            if (trycount < 5 && !string.IsNullOrEmpty(channelToJoin))
            {
                await UMI3DAsyncManager.Delay(1000);
                if (mumbleClient != null && !mumbleClient.JoinChannel(channelToJoin))
                {
                    await JoinChannel(trycount + 1);
                }
            }
        }

        private async Task<bool> IsPLaying()
        {
            if (playing)
                return true;
            while (playingInit)
                await UMI3DAsyncManager.Yield();
            return playing;
        }

        private void handleMute()
        {
            if (useMumble && mumbleClient != null)
                mumbleClient.SetSelfMute(isMute);
            else
                StartMicrophoneAsync();
        }

        private void SetMumbleUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;
            string[] s = url.Split(':');

            hostName = s[0];
            if (s.Length > 1 && int.TryParse(s[1], out int port))
                this.port = port;
        }

        private async void IdentityUpdate(UMI3DUser user)
        {
            if (user != null && user.isClient && !IdentityUpdateOnce)
            {
                IdentityUpdateOnce = true;
                await UMI3DAsyncManager.Yield();
                await UMI3DAsyncManager.Yield();

                username = user.audioLogin;
                password = user.audioPassword;

                if (await IsPLaying())
                {
                    await StopMicrophone();
                    await UMI3DAsyncManager.Yield();
                    await StartMicrophone();
                }
                IdentityUpdateOnce = false;
            }
        }

        private async void ChannelUpdate(UMI3DUser user)
        {
            channelToJoin = user.audioChannel;
            if (!string.IsNullOrEmpty(channelToJoin) && await IsPLaying())
            {
                await JoinChannel();
            }
        }

        private async void ServerUpdate(UMI3DUser user)
        {
            SetMumbleUrl(user.audioServer);
            if (await IsPLaying())
            {
                await StopMicrophone();
                await UMI3DAsyncManager.Yield();
                await StartMicrophone();
            }
        }

        private async void Reset()
        {
            useMumble = false;
            if (await IsPLaying())
                StopMicrophoneAsync();
        }

        private async void UseMumbleUpdate(UMI3DUser user)
        {
            if (useMumble != user.useMumble)
            {
                useMumble = user.useMumble;

                if (useMumble)
                    StartMicrophoneAsync();
                else if (await IsPLaying())
                    StopMicrophoneAsync();
            }
        }

        private IEnumerator ExampleMicReconnect(string micToConnect)
        {
            while (playing)
            {
                string[] micNames = Microphone.devices;
                // try to see if the desired mic is connected
                for (int i = 0; i < micNames.Length; i++)
                {
                    if (micNames[i] == micToConnect)
                    {
                        UnityEngine.Debug.Log("Desired mic reconnected");
                        mumbleMic.MicNumberToUse = i;
                        mumbleMic.StartSendingAudio();
                        yield break;
                    }
                }
                yield return new WaitForSeconds(2f);
            }
        }
        #endregion

        /// <summary>
        /// An example of how to serialize the positional data that you're interested in
        /// NOTE: this function, in the current implementation, is called regardless
        /// of if the user is speaking
        /// </summary>
        /// <param name="posData"></param>
        private void WritePositionalData(ref byte[] posData, ref int posDataLength)
        {
            // Get the XYZ position of the camera
            Vector3 pos = Camera.main.transform.position;
            //Debug.Log("Sending pos: " + pos);
            // Copy the XYZ floats into our positional array
            int dstOffset = 0;
            Buffer.BlockCopy(BitConverter.GetBytes(pos.x), 0, posData, dstOffset, sizeof(float));
            dstOffset += sizeof(float);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.y), 0, posData, dstOffset, sizeof(float));
            dstOffset += sizeof(float);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.z), 0, posData, dstOffset, sizeof(float));

            posDataLength = 3 * sizeof(float);
            // The reverse method is in MumbleExamplePositionDisplay
        }

        #region UserLife

        private MumbleAudioPlayer CreateMumbleAudioPlayerFromPrefab(string username, uint session)
        {
            return AudioManager.Instance.GetMumbleAudioPlayer(username, session);
        }
        private void OnOtherUserStateChange(uint session, MumbleProto.UserState updatedDeltaState, MumbleProto.UserState fullUserState)
        {
            //print("User #" + session + " had their user state change");

            //// Here we can do stuff like update a UI with users' current channel/mute etc.
        }
        private void DestroyMumbleAudioPlayer(uint session, MumbleAudioPlayer playerToDestroy)
        {
            //UnityEngine.GameObject.Destroy(playerToDestroy.gameObject);
        }
        #endregion

        #region Data and settings


        public string[] GetMicrophonesNames() { return Microphone.devices; }
        public string GetCurrentMicrophoneName()
        {
            if (mumbleMic == null || !mumbleMic.HasMic())
                return _pendingMic;
            return mumbleMic.GetCurrentMicName();
        }

        public async Task<bool> SetCurrentMicrophoneName(string value)
        {
            if (value == GetCurrentMicrophoneName() && value != _pendingMic)
                return false;

            if (mumbleMic == null)
            {
                _pendingMic = value;
                return false;
            }

            SetMicrophone(value);

            if (await IsPLaying())
            {
                await StopMicrophone();
                await UMI3DAsyncManager.Yield();
                await StartMicrophone();
                await IsPLaying();
            }
            else
            {
                _pendingMic = value;
            }

            return true;
        }

        void SetMicrophone(string value)
        {
            string[] mics = GetMicrophonesNames();
            int count = mics.Length;
            int i = 0;

            for (; i < count; i++)
                if (mics[i] == value)
                {
                    mumbleMic.MicNumberToUse = i;
                    break;
                }
        }

        public async void SetCurrentMicrophoneNameAsync(string value) { await SetCurrentMicrophoneName(value); }

        private MicrophoneMode MicTypeToMode(MumbleMicrophone.MicType? type)
        {
            switch (type)
            {
                case MumbleMicrophone.MicType.AlwaysSend:
                    return MicrophoneMode.AlwaysSend;
                case MumbleMicrophone.MicType.Amplitude:
                    return MicrophoneMode.Amplitude;
                case MumbleMicrophone.MicType.PushToTalk:
                    return MicrophoneMode.PushToTalk;
                case MumbleMicrophone.MicType.MethodBased:
                    return MicrophoneMode.MethodBased;
            }
            return MicrophoneMode.AlwaysSend;
        }
        private MumbleMicrophone.MicType MicModeToType(MicrophoneMode? type)
        {
            switch (type)
            {
                case MicrophoneMode.AlwaysSend:
                    return MumbleMicrophone.MicType.AlwaysSend;
                case MicrophoneMode.Amplitude:
                    return MumbleMicrophone.MicType.Amplitude;
                case MicrophoneMode.PushToTalk:
                    return MumbleMicrophone.MicType.PushToTalk;
                case MicrophoneMode.MethodBased:
                    return MumbleMicrophone.MicType.MethodBased;
            }
            return MumbleMicrophone.MicType.AlwaysSend;
        }

        public MicrophoneMode GetCurrentMicrophoneMode()
        {
            return MicTypeToMode(mumbleMic?.VoiceSendingType);
        }

        public bool SetCurrentMicrophoneMode(MicrophoneMode value)
        {
            if (mumbleMic == null || value == GetCurrentMicrophoneMode())
                return false;
            mumbleMic.VoiceSendingType = MicModeToType(value);
            return true;
        }

        public partial class PacketData
        {
            private static bool running = false;
            private static bool stop = false;
            private MicrophoneListener microphone;
            private int timeStepMilliseconds;

            private PacketData(MicrophoneListener microphone) : this(microphone, 1000)
            { }

            private PacketData(MicrophoneListener microphone, int timeStepMilliseconds)
            {
                this.microphone = microphone;
                this.timeStepMilliseconds = timeStepMilliseconds;
            }

            partial void PacketsReceived(long count);
            partial void PacketsLost(long count);
            partial void PacketsSent(long count);

            public void Start()
            {
                Update(timeStepMilliseconds);
            }

            public void Stop()
            {
                stop = true;
            }

            private async void Update(int timeStepMilliseconds)
            {
                if (!running)
                {
                    running = true;
                    stop = false;

                    if (await microphone.IsPLaying())
                    {

                        long numPacketsReceived = microphone.mumbleClient.NumUDPPacketsSent;
                        long numPacketsSent = microphone.mumbleClient.NumUDPPacketsReceieved;
                        long numPacketsLost = microphone.mumbleClient.NumUDPPacketsLost;

                        while (true)
                        {
                            await UMI3DAsyncManager.Delay(timeStepMilliseconds);

                            if (stop && !await microphone.IsPLaying() && microphone.mumbleClient == null)
                                break;

                            long numSentThisSample = microphone.mumbleClient.NumUDPPacketsSent - numPacketsSent;
                            long numRecvThisSample = microphone.mumbleClient.NumUDPPacketsReceieved - numPacketsReceived;
                            long numLostThisSample = microphone.mumbleClient.NumUDPPacketsLost - numPacketsLost;

                            PacketsSent(-numSentThisSample);
                            PacketsReceived(-numRecvThisSample);
                            PacketsLost(-numLostThisSample);

                            numPacketsSent += numSentThisSample;
                            numPacketsReceived += numRecvThisSample;
                            numPacketsLost += numLostThisSample;
                        }
                    }

                    running = false;
                }
            }
        }

        private void DebugSample(PcmArray array)
        {
            if (Debug)
            {
                float sum = 0;
                bool saturated = false;

                foreach (float v in array.Pcm)
                {
                    if (v > 1)
                    {
                        saturated = true;
                    }
                    else if (v < -1)
                    {
                        saturated = true;
                    }
                    sum += v * v;
                }

                this.saturated = saturated;
                rms = Mathf.Sqrt(sum / array.Pcm.Length);
                db = 20 * Mathf.Log10(rms / refValue);
            }
        }

        #endregion

        #endregion

        public List<DebugInfo> GetInfos()
        {
            return new List<DebugInfo>()
            {
                new DebugInfo<string>("Microphone",()=>{ return GetCurrentMicrophoneName(); }),
                new DebugInfo<bool>("Is Connected",()=>{ return playing; }),
                new DebugInfo<string>("Mode",()=>{ return GetCurrentMicrophoneMode().ToString(); }),
                new DebugInfo<bool>("| Is Sending Audio",()=>{ return mumbleMic?.isRecording ?? false; }),
                new DebugInfo<string>("| Push To Talk Key",()=>{ return pushToTalkKeycode.ToString(); }),
                new DebugInfo<float>("| Min Amplitude",()=>{ return minAmplitudeToSend; }),
                new DebugInfo<float>("| stop delay",()=>{ return voiceStopingDelaySeconds; }),
            };
        }

        public string GetLogName()
        {
            return "Microphone";
        }
    }
}