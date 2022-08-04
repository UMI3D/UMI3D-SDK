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
        // Basic mumble audio player
        public GameObject MyMumbleAudioPlayerPrefab;
        // Mumble audio player that also receives position commands
        public GameObject MyMumbleAudioPlayerPositionedPrefab;

        bool micIsOn => MyMumbleMic?.isRecording ?? false;

        public bool UseLocalLoopback
        {
            get => DebuggingVariables?.UseLocalLoopback ?? false;
            set { if (DebuggingVariables != null) DebuggingVariables.UseLocalLoopback = value; }
        }

        public static bool IsMute
        {
            get { return Exists ? !Instance.useMumble || muted : true; }

            set { muted = value; if (Exists) Instance.handleMute(); }
        }

        public float MinAmplitude
        {
            get => MyMumbleMic?.MinAmplitude ?? 0; set
            {
                if (MyMumbleMic != null && value >= 0f && value <= 1f)
                    MyMumbleMic.MinAmplitude = value;
            }
        }
        public float VoiceHoldSeconds
        {
            get => MyMumbleMic?.VoiceHoldSeconds ?? 0; set
            {
                if (MyMumbleMic != null && value >= 0f)
                    MyMumbleMic.VoiceHoldSeconds = value;
            }
        }
        public KeyCode PushToTalkKeycode
        {
            get => MyMumbleMic?.PushToTalkKeycode ?? KeyCode.M; set
            {
                if (MyMumbleMic != null)
                    MyMumbleMic.PushToTalkKeycode = value;
            }
        }

        #endregion
        #region private field
        MumbleClient mumbleClient;
        MumbleMicrophone MyMumbleMic;
        DebugValues DebuggingVariables;

        const bool connectAsyncronously = true;
        const bool sendPosition = false;

        static bool muted;

        string hostName = "1.2.3.4";
        int port = 64738;
        string username = "ExampleUser";
        string password = "1passwordHere!";
        string channelToJoin = "";

        bool useMumble = false;
        bool playing = false;
        bool playingInit = false;
        bool IdentityUpdateOnce = false;
        #endregion

        #region Init
        protected void Start()
        {
            MyMumbleMic = gameObject.GetOrAddComponent<MumbleMicrophone>();
            gameObject.GetOrAddComponent<EventProcessor>();

            DebuggingVariables = new DebugValues()
            {
                EnableEditorIOGraph = false,
                UseLocalLoopback = false,
                UseRandomUsername = false,
                UseSyntheticSource = false
            };

            QuittingManager.OnApplicationIsQuitting.AddListener(_OnApplicationQuit);

            UMI3DUser.OnUserMicrophoneIdentityUpdated.AddListener(IdentityUpdate);
            UMI3DUser.OnUserMicrophoneChannelUpdated.AddListener(ChannelUpdate);
            UMI3DUser.OnUserMicrophoneServerUpdated.AddListener(ServerUpdate);
            UMI3DUser.OnUserMicrophoneUseMumbleUpdated.AddListener(UseMumbleUpdate);

            PushToTalkKeycode = KeyCode.M;
        }

        void _OnApplicationQuit()
        {
            StopMicrophone();
        }
        #endregion

        public async void StartMicrophone()
        {
            if (!playingInit)
            {
                playingInit = true;
                await _StartMicrophone();
                playingInit = false;
            }
        }

        public async void StopMicrophone()
        {
            if (await IsPLaying() && mumbleClient != null)
            {
                MyMumbleMic.OnMicDisconnect -= OnMicDisconnected;
                MyMumbleMic.StopSendingAudio();
                mumbleClient.Close();
            }
            playing = false;
        }

        private void OnMicDisconnected()
        {
            string disconnectedMicName = MyMumbleMic.GetCurrentMicName();
            StartCoroutine(ExampleMicReconnect(disconnectedMicName));
        }


        public async void ToggleSendingSound(bool? on = null)
        {
            if (on == null)
                on = !MyMumbleMic.isRecording;

            if (await IsPLaying() && MyMumbleMic != null && GetCurrentMicrophoneMode() == MicrophoneMode.MethodBased)
            {
                if (on == MyMumbleMic.isRecording)
                    return;

                if (MyMumbleMic.isRecording)
                    MyMumbleMic.StopSendingAudio();
                else
                    MyMumbleMic.StartSendingAudio();
            }
        }

        #region private method

        private async Task _StartMicrophone()
        {
            Debug.LogError($"Start {!useMumble}");
            if (!useMumble) return;

            if (hostName == "1.2.3.4")
            {
                Debug.LogError("Please set the mumble host name to your mumble server");
                return;
            }

            if (UMI3DCollaborationEnvironmentLoader.Exists)
            {
                Debug.LogError($"Exist");
                var user = UMI3DCollaborationEnvironmentLoader.Instance.GetClientUser();
                if (user != null)
                {
                    Debug.LogError($"user {username} {password} {hostName} {port} {channelToJoin}");
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
                        SpeakerCreationMode.ALL, DebuggingVariables, posLength);

                    if (connectAsyncronously)
                        while (!mumbleClient.ReadyToConnect)
                            await UMI3DAsyncManager.Yield();

                    mumbleClient.Connect(username, password);

                    if (connectAsyncronously)
                        await UMI3DAsyncManager.Yield();

                    if (MyMumbleMic != null)
                    {
                        MyMumbleMic.SendAudioOnStart = false;
                        mumbleClient.AddMumbleMic(MyMumbleMic);
                        mumbleClient.SetSelfMute(IsMute);
                        if (sendPosition)
                            MyMumbleMic.SetPositionalDataFunction(WritePositionalData);
                        MyMumbleMic.OnMicDisconnect += OnMicDisconnected;

                        await JoinChannel();

                        if (MyMumbleMic.VoiceSendingType == MumbleMicrophone.MicType.AlwaysSend
                                || MyMumbleMic.VoiceSendingType == MumbleMicrophone.MicType.Amplitude)
                            MyMumbleMic.StartSendingAudio();

                        playing = true;

                        return;
                    }
                }
            }
            Debug.LogError($"playing false");
            playing = false;
        }

        async Task JoinChannel(int trycount = 0)
        {
            if (trycount < 3)
            {
                await UMI3DAsyncManager.Delay(5000);
                if (!mumbleClient.JoinChannel(channelToJoin))
                {
                    await JoinChannel(trycount++);
                }
            }
        }

        async Task<bool> IsPLaying()
        {
            if (playing)
                return true;
            while (playingInit)
                await UMI3DAsyncManager.Yield();
            return playing;
        }

        void handleMute()
        {
            if (useMumble && mumbleClient != null)
                mumbleClient.SetSelfMute(IsMute);
        }

        void SetMumbleUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;
            var s = url.Split(':');

            hostName = s[0];
            if (s.Length > 1 && int.TryParse(s[1], out int port))
                this.port = port;
        }

        async void IdentityUpdate(UMI3DUser user)
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
                    StopMicrophone();
                    StartMicrophone();
                }
                IdentityUpdateOnce = false;
            }
        }

        async void ChannelUpdate(UMI3DUser user)
        {
            channelToJoin = user.audioChannel;
            if (await IsPLaying())
            {
                await JoinChannel();
            }
        }

        async void ServerUpdate(UMI3DUser user)
        {

            Debug.Log($"server {hostName}:{port} -> {user.audioServer}");
            SetMumbleUrl(user.audioServer);
            if (await IsPLaying())
            {
                StopMicrophone();
                StartMicrophone();
            }
        }

        void UseMumbleUpdate(UMI3DUser user)
        {
            if (useMumble != user.useMumble)
            {
                useMumble = user.useMumble;

                Debug.Log(useMumble);
                if (useMumble)
                    StartMicrophone();
                else
                    StopMicrophone();
            }
        }

        IEnumerator ExampleMicReconnect(string micToConnect)
        {
            while (playing)
            {
                string[] micNames = Microphone.devices;
                // try to see if the desired mic is connected
                for (int i = 0; i < micNames.Length; i++)
                {
                    if (micNames[i] == micToConnect)
                    {
                        Debug.Log("Desired mic reconnected");
                        MyMumbleMic.MicNumberToUse = i;
                        MyMumbleMic.StartSendingAudio();
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
            Debug.LogError("New player");
            // Depending on your use case, you might want to add the prefab to an existing object (like someone's head)
            // If you have users entering and leaving frequently, you might want to implement an object pool
            GameObject newObj = sendPosition
                ? GameObject.Instantiate(MyMumbleAudioPlayerPositionedPrefab)
                : GameObject.Instantiate(MyMumbleAudioPlayerPrefab);

            newObj.name = username + "_MumbleAudioPlayer";
            MumbleAudioPlayer newPlayer = newObj.GetComponent<MumbleAudioPlayer>();
            Debug.Log("Adding audio player for: " + username);
            return newPlayer;
        }
        private void OnOtherUserStateChange(uint session, MumbleProto.UserState updatedDeltaState, MumbleProto.UserState fullUserState)
        {
            print("User #" + session + " had their user state change");

            // Here we can do stuff like update a UI with users' current channel/mute etc.
        }
        private void DestroyMumbleAudioPlayer(uint session, MumbleAudioPlayer playerToDestroy)
        {
            UnityEngine.GameObject.Destroy(playerToDestroy.gameObject);
        }
        #endregion

        #region Data and settings


        private string[] GetMicrophonesNames() { return Microphone.devices; }
        public string GetCurrentMicrophoneName() => MyMumbleMic?.GetCurrentMicName();
        public async Task<bool> SetCurrentMicrophoneName(string value)
        {
            if (MyMumbleMic == null || value == GetCurrentMicrophoneName())
                return false;

            var mics = GetMicrophonesNames();
            var count = mics.Length;
            int i = 0;

            for (; i < count; i++)
                if (mics[i] == value)
                {
                    MyMumbleMic.MicNumberToUse = i;
                    break;
                }

            if (await IsPLaying())
            {
                StopMicrophone();
                StartMicrophone();
            }
            return true;
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

        public MicrophoneMode GetCurrentMicrophoneMode() => MicTypeToMode(MyMumbleMic?.VoiceSendingType);
        public bool SetCurrentMicrophoneMode(MicrophoneMode value)
        {
            if (MyMumbleMic == null || value == GetCurrentMicrophoneMode())
                return false;
            MyMumbleMic.VoiceSendingType = MicModeToType(value);
            return true;
        }

        public partial class PacketData
        {
            private static bool running = false;
            private static bool stop = false;

            MicrophoneListener microphone;
            int timeStepMilliseconds;

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

            async void Update(int timeStepMilliseconds)
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


        #endregion

        #endregion

        public List<DebugInfo> GetInfos()
        {
            return new List<DebugInfo>();
        }

        public string GetLogName()
        {
            return "Microphone";
        }

    }
}