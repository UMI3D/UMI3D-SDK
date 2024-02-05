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
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.collaboration
{
    [Serializable]
    public class MicrophoneEvent : UnityEngine.Events.UnityEvent<MicrophoneStatus>
    {
    }
    [Serializable]
    public class MumbleEvent : UnityEngine.Events.UnityEvent<MumbleStatus>
    {
    }
    [Serializable]
    public class SaturatedEvent : UnityEngine.Events.UnityEvent<bool>
    {
    }

    public enum MicrophoneMode
    {
        AlwaysSend,
        Amplitude,
        PushToTalk,
        MethodBased
    }

    public enum MumbleStatus
    {
        NotConnected,
        Connecting,
        Connected,
        Disconnecting
    }

    public enum MicrophoneStatus
    {
        NoMicrophone,
        MicrophoneConnecting,
        MicrophoneReady,
        RemovingMicrophone,
    }

    public enum MicrophoneInputType
    {
        Unity,
        NAudio
    }

    [RequireComponent(typeof(AudioSource))]
    public abstract class AbstractMicrophoneListener<T> : SingleBehaviour<T>, common.IPublisher<float[]> where T : AbstractMicrophoneListener<T>
    {
        class EventUpdater<L>
        {
            L value;
            UnityEngine.Events.UnityEvent<L> @event;

            public EventUpdater(L value, UnityEvent<L> @event)
            {
                this.value = value;
                this.@event = @event;
            }

            public void SetValue(L value)
            {
                if (!(value?.Equals(this.value) ?? this.value != null))
                {
                    this.value = value;
                    @event?.Invoke(value);
                }
            }

        }

        public static SaturatedEvent OnSaturated = new SaturatedEvent();
        EventUpdater<bool> OnSaturatedUpdater;

        public static MicrophoneEvent OnMicrophoneStatusUpdate = new MicrophoneEvent();
        EventUpdater<MicrophoneStatus> OnMicrophoneStatusUpdateUpdater;

        public static MumbleEvent OnMumbleStatusUpdate = new MumbleEvent();
        EventUpdater<MumbleStatus> OnMumbleStatusUpdateUpdater;

        List<Action<float[]>> subscribed = new List<Action<float[]>>();

        protected class Identity
        {
            Func<(string, int, string, string)> GetIdentity;

            public Identity(Func<(string, int, string, string)> getIdentity)
            {
                GetIdentity = getIdentity;
                Clear();
            }

            public string hostName { get; set; } = null;
            public int port { get; set; } = 0;
            public string username { get; set; } = null;
            public string password { get; set; } = null;

            public void Update()
            {
                var id = GetIdentity();

                this.hostName = id.Item1;
                this.port = id.Item2;
                this.username = id.Item3;
                this.password = id.Item4;
            }

            public void Clear()
            {
                this.hostName = null;
                this.port = 0;
                this.username = null;
                this.password = null;
            }

            public bool Ready => hostName != null && port != 0 && username != null && password != null;

        }
        protected Identity identity = null;

        public MumbleStatus mumbleStatus
        {
            get => _mumbleStatus;
            private set
            {
                _mumbleStatus = value;
                OnMumbleStatusUpdateUpdater.SetValue(value);
            }
        }
        public MicrophoneStatus microphoneStatus
        {
            get => _microphoneStatus;
            private set
            {
                _microphoneStatus = value;
                OnMicrophoneStatusUpdateUpdater.SetValue(value);
            }
        }

        public MicrophoneInputType inputType;

        protected string channel { get; private set; } = null;
        protected string pendingChannel { get; private set; } = null;

        protected MumbleClient mumbleClient { get; private set; } = null;
        protected MumbleMicrophone mumbleMic { get; private set; } = null;

        private bool sendPosition = false;

        protected bool debugGui;
        protected DebugValues debuggingVariables;

        public float lastPingDelta { get; private set; } = 0;
        public float lastPing { get; private set; } = -1;

        private string _pendingMic;

        protected bool isMute = true;

        public bool debugSampling = false;

        public float rms { get; private set; }
        public float db { get; private set; }

        public bool saturated
        {
            get => _saturated; private set
            {
                _saturated = value;
                OnSaturatedUpdater.SetValue(value);
            }
        }

        float refValue = 1f;

        int line = 0;
        private bool _saturated;


        static public bool mute
        {
            get
            {
                if (Exists)
                    return Instance._mute;
                return false;
            }
            set
            {
                if (Exists)
                    Instance._mute = value;
            }
        }

        bool _mute
        {
            get => IsMute() ?? isMute;
            set => Mute(value);
        }


        public bool useLocalLoopback
        {
            get => debuggingVariables?.UseLocalLoopback ?? false;
            set { if (debuggingVariables != null) debuggingVariables.UseLocalLoopback = value; }
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

        #region Init
        protected virtual void Start()
        {
            OnSaturatedUpdater = new EventUpdater<bool>(saturated, OnSaturated);
            OnMumbleStatusUpdateUpdater = new EventUpdater<MumbleStatus>(mumbleStatus, OnMumbleStatusUpdate);
            OnMicrophoneStatusUpdateUpdater = new EventUpdater<MicrophoneStatus>(microphoneStatus, OnMicrophoneStatusUpdate);

            switch (inputType)
            {
                case MicrophoneInputType.Unity:
                    mumbleMic = gameObject.AddComponent<MumbleMicrophone>();
                    break;
                case MicrophoneInputType.NAudio:
                    mumbleMic = gameObject.AddComponent<NAudioMicrophone>();
                    break;
                default:
                    break;
            }

            SetMicrophone();
            gameObject.GetOrAddComponent<EventProcessor>();

            debuggingVariables = new DebugValues()
            {
                EnableEditorIOGraph = false,
                UseLocalLoopback = false,
                UseRandomUsername = false,
                UseSyntheticSource = false
            };

            running = false;
            channel = null;
            pendingChannel = null;
            identity?.Clear();

            QuittingManager.OnApplicationIsQuitting.AddListener(_OnApplicationQuit);
        }

        protected virtual void _OnApplicationQuit()
        {
            running = false;
            Reset();
        }

        protected bool running { get; private set; } = false;
        int millisecondsHeartBeat = 3000;
        private MumbleStatus _mumbleStatus = MumbleStatus.NotConnected;
        private MicrophoneStatus _microphoneStatus = MicrophoneStatus.NoMicrophone;

        protected async void Heartbeat()
        {
            if (running)
                return;
            running = true;

            await Delay(millisecondsHeartBeat);
            while (running && Exists && UMI3DCollaborationClientServer.Exists)
            {
                switch (mumbleStatus)
                {
                    case MumbleStatus.NotConnected:
                        if (await Connect())
                            await UpdateChanel();
                        else
                            await Delay(6000);
                        break;
                    case MumbleStatus.Connected:
                        {
                            if (pendingChannel != null)
                            {
                                if (!await UpdateChanel())
                                    await Delay(3000);
                            }
                            if (microphoneStatus == MicrophoneStatus.NoMicrophone)
                                StartMicrophone();
                        }
                        break;
                }
                await Delay(millisecondsHeartBeat);
            }
            Reset();
        }

        protected void Reset()
        {
            running = false;
            Disconnect();
            identity?.Clear();
            channel = null;
            pendingChannel = null;
        }

        #endregion


        protected void SetChannelToJoin(string value)
        {
            pendingChannel = value;
        }

        private async Task<bool> UpdateChanel()
        {
            if (mumbleStatus == MumbleStatus.Connected && pendingChannel != null && pendingChannel != channel)
            {
                if (pendingChannel == channel)
                {
                    pendingChannel = null;
                    return false;
                }
                if (!mumbleClient.JoinChannel(pendingChannel))
                {
                    await Delay(1000);
                    return false;
                }
                channel = pendingChannel;
                pendingChannel = null;
                return true;
            }
            return false;
        }

        private async Task<bool> Connect()
        {
            //Only One Connection at a time
            if (mumbleStatus > MumbleStatus.NotConnected)
                return false;
            mumbleStatus = MumbleStatus.Connecting;

            //Update Identity
            identity.Update();
            if (!identity.Ready)
            {
                mumbleStatus = MumbleStatus.NotConnected;
                return false;
            }

            int posLength = sendPosition ? 3 * sizeof(float) : 0;

            if (mumbleClient != null)
            {
                LogError("client should be null");
                mumbleClient.Close();
                mumbleClient = null;
            }

            mumbleClient = new MumbleClient(identity.hostName, identity.port, CreateMumbleAudioPlayerFromPrefab,
                   DestroyMumbleAudioPlayer, OnOtherUserStateChange, true,
                   SpeakerCreationMode.ALL, debuggingVariables, posLength);

            mumbleClient.ConnectionError.AddListener(Failed);

            while (!(mumbleClient?.ReadyToConnect ?? true) && await YieldConnected()) { }
            if (mumbleClient == null || mumbleStatus != MumbleStatus.Connecting)
            {
                mumbleStatus = MumbleStatus.NotConnected;
                return false;
            }

            mumbleClient.connectionFailed.AddListener(Failed);
            mumbleClient.OnDisconnected += OnDisconected;
            mumbleClient.OnPingReceived += OnPingReceived;

            if (!await YieldConnected())
            {
                mumbleStatus = MumbleStatus.NotConnected;
                return false;
            }

            mumbleClient.Connect(identity.username, identity.password);

            lastPing = -1;

            while (lastPing == -1 && await YieldConnected()) { }
            if (mumbleStatus != MumbleStatus.Connecting)
            {
                mumbleStatus = MumbleStatus.NotConnected;
                return false;
            }

            mumbleStatus = MumbleStatus.Connected;

            return true;
        }

        private void Disconnect()
        {
            StopMicrophone();

            if (mumbleStatus > MumbleStatus.Connecting && mumbleClient != null)
            {
                mumbleStatus = MumbleStatus.Disconnecting;
                mumbleClient.ConnectionError.RemoveListener(Failed);
                mumbleClient.connectionFailed.RemoveListener(Failed);
                mumbleClient.OnDisconnected -= OnDisconected;
                mumbleClient.OnPingReceived -= OnPingReceived;
                mumbleClient.Close();
                mumbleClient = null;
                mumbleStatus = MumbleStatus.NotConnected;
            }
        }

        void StartMicrophone()
        {
            if (mumbleStatus == MumbleStatus.Connected && microphoneStatus == MicrophoneStatus.NoMicrophone)
            {
                microphoneStatus = MicrophoneStatus.MicrophoneConnecting;



                if (mumbleMic == null)
                {
                    microphoneStatus = MicrophoneStatus.NoMicrophone;
                    return;
                }

                mumbleMic.OnMicData += DebugSample;

                SetMicrophone();

                if (!microphoneIsValid())
                {
                    microphoneStatus = MicrophoneStatus.NoMicrophone;
                    return;
                }

                mumbleMic.SendAudioOnStart = false;
                mumbleClient.AddMumbleMic(mumbleMic);

                mumbleClient.SetSelfMute(isMute);

                if (sendPosition)
                    mumbleMic.SetPositionalDataFunction(WritePositionalData);

                mumbleMic.OnMicDisconnect += OnMicDisconnected;

                if (mumbleMic.VoiceSendingType == MumbleMicrophone.MicType.AlwaysSend
                        || mumbleMic.VoiceSendingType == MumbleMicrophone.MicType.Amplitude)
                    mumbleMic.StartSendingAudio();

                microphoneStatus = MicrophoneStatus.MicrophoneReady;
            }
        }

        protected void Mute(bool? mute)
        {
            var isMute = mute ?? !this.isMute;
            if (this.isMute != isMute)
            {
                this.isMute = isMute;

                UMI3DUser user = UMI3DCollaborationEnvironmentLoader.Instance.GetClientUser();

                if (user.microphoneStatus == isMute)
                    user.SetMicrophoneStatus(!isMute);

                if (microphoneStatus == MicrophoneStatus.MicrophoneReady)
                    mumbleClient.SetSelfMute(isMute);
            }
        }

        protected bool? IsMute()
        {
            if (microphoneStatus == MicrophoneStatus.MicrophoneReady)
                return mumbleClient?.IsSelfMuted();
            return null;
        }

        void StopMicrophone()
        {
            if (microphoneStatus == MicrophoneStatus.MicrophoneReady)
            {
                microphoneStatus = MicrophoneStatus.RemovingMicrophone;
                try
                {
                    mumbleMic.StopSendingAudio();
                    mumbleMic.OnMicDisconnect -= OnMicDisconnected;
                    mumbleMic.OnMicData -= DebugSample;
                }
                catch { };
                microphoneStatus = MicrophoneStatus.NoMicrophone;
            }
        }

        private void OnMicDisconnected()
        {
            //LogError("On Mic Disconnected");
            StopMicrophone();
        }

        /// <summary>
        /// An example of how to serialize the positional data that you're interested in. <br/>
        /// NOTE: this function, in the current implementation, is called regardless
        /// of if the user is speaking
        /// </summary>
        /// <param name="posData"></param>
        protected virtual void WritePositionalData(ref byte[] posData, ref int posDataLength)
        {
            // Get the XYZ position of the camera
            Vector3 pos = Camera.main.transform.position;
            //Log("Sending pos: " + pos);
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

        void SetMicrophone()
        {
            if (_pendingMic == null || mumbleMic == null)
                return;

            string[] mics = GetMicrophonesNames();
            int count = mics.Length;
            int i = 0;

            for (; i < count; i++)
                if (mics[i] == _pendingMic)
                {
                    mumbleMic.MicNumberToUse = i;
                    _pendingMic = null;
                    return;
                }
            LogError($"Microphone [{_pendingMic}] not found, set to first mic if any [{mics.FirstOrDefault()}]");
            mumbleMic.MicNumberToUse = 0;
        }

        public async Task<bool> SetCurrentMicrophoneName(int value)
        {
            var mics = GetMicrophonesNames();
            if (mics.Length > value && value >= 0)
            {
                return await SetCurrentMicrophoneName(mics[value]);
            }
            return false;
        }

        public async Task<bool> SetCurrentMicrophoneName(string value)
        {
            if (value == GetCurrentMicrophoneName() && value != _pendingMic)
                return false;

            _pendingMic = value;
            if (mumbleMic == null)
            {
                return false;
            }

            SetMicrophone();

            if (await IsMicrophoneConnected())
            {
                StopMicrophone();
                StartMicrophone();
            }
            else
            {
                _pendingMic = value;
            }

            return true;
        }

        bool microphoneIsValid()
        {
            return mumbleMic != null && mumbleMic.MicNumberToUse < GetMicrophonesNames().Length;
        }

        async Task<bool> IsMicrophoneConnected()
        {
            while (microphoneStatus == MicrophoneStatus.MicrophoneConnecting || microphoneStatus == MicrophoneStatus.RemovingMicrophone)
            {
                await Yield();
            }
            return microphoneStatus == MicrophoneStatus.MicrophoneReady;
        }

        public static string[] GetMicrophonesNames() { return Microphone.devices; }
        public string GetCurrentMicrophoneName()
        {
            if (mumbleMic == null || !mumbleMic.HasMic())
                return _pendingMic;
            return mumbleMic.GetCurrentMicName();
        }

        protected virtual async Task Yield()
        {
            await Task.Yield();
        }

        protected virtual async Task Delay(int millisecondsDelay)
        {
            await Task.Delay(millisecondsDelay);
        }

        async Task<bool> YieldConnected()
        {
            await Task.Yield();
            return mumbleStatus == MumbleStatus.Connecting;
        }

        void Failed(Exception e)
        {
            LogError("Failed with exception");
            LogException(e);
            //Failed();
        }
        void Failed()
        {
            LogError("Failed");
            if (mumbleClient != null)
            {
                mumbleClient.ConnectionError.RemoveListener(Failed);
                mumbleClient.connectionFailed.RemoveListener(Failed);
                mumbleClient.OnDisconnected -= OnDisconected;
                mumbleClient.OnPingReceived -= OnPingReceived;
                mumbleClient.Close();
                mumbleClient = null;
            }
            mumbleStatus = MumbleStatus.NotConnected;
        }

        void OnDisconected()
        {
            LogError("Disconected");
            Failed();
        }

        void OnPingReceived()
        {
            var lp = lastPing;

            lastPing = Time.time;
            if (lp != -1)
                lastPingDelta = lastPing - lp;
        }

        #region UserLife
        private MumbleAudioPlayer CreateMumbleAudioPlayerFromPrefab(string username, uint session)
        {
            return AudioManager.Instance.GetMumbleAudioPlayer(username, session);
        }
        private void OnOtherUserStateChange(uint session, MumbleProto.UserState updatedDeltaState, MumbleProto.UserState fullUserState)
        {
            string debug =
                 "------Mumble User State Change------\n" +
                $" Name     : {fullUserState?.Name} \n" +
                $" Session  : {fullUserState?.Session} \n" +
                $" Channel  : {fullUserState?.ChannelId} \n" +
                $" Deaf     : {fullUserState?.Deaf} \n" +
                $" Mute     : {fullUserState?.Mute} \n" +
                $" SelfDeaf : {fullUserState?.SelfDeaf} \n" +
                $" SelfMute : {fullUserState?.SelfMute} \n" +
                $"------------------------------------";

            Log(debug);
        }
        private void DestroyMumbleAudioPlayer(uint session, MumbleAudioPlayer playerToDestroy)
        {
            string debug =
                 "--------Mumble User Destroyed-------\n" +
                $" Session  : {session} \n" +
                $"------------------------------------";

            Log(debug);

            if (!AudioManager.Instance.DeletePending(playerToDestroy?.GetUsername(), session))
            {
                playerToDestroy?.Reset();
            }
        }
        #endregion

        private void DebugSample(PcmArray array)
        {
            foreach (var action in subscribed)
                action?.Invoke(array.Pcm);

            if (debugSampling)
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

        private void WriteLabel(Rect rect, string message)
        {
            GUI.color = Color.black;
            GUI.Label(rect, message);
            // Do the same thing as above but make the above UI look like a solid
            // shadow so that the text is readable on any contrast screen
            GUI.color = Color.white;
            GUI.Label(rect, message);
        }

        private void WriteLabel(string message, int? set = null)
        {
            WriteLabel(new Rect(14, GetLine(set), 256, 25), message);
        }

        int GetLine(int? set = null)
        {
            if (set == null)
                line = line + 14;
            else
                line = set ?? 0;
            return line;
        }

        private void OnGUI()
        {
            if (!debugGui)
                return;

            WriteLabel($"------------------------------", 14);
            WriteLabel($"HeartBeat          : {running}");
            WriteLabel($"Mumble Status      : {mumbleStatus}");
            WriteLabel($"Microphone Status  : {microphoneStatus}");
            WriteLabel($"Current Microphone : {GetCurrentMicrophoneName()}");
            WriteLabel($"Current Channel    : {channel}");
            WriteLabel($"Saturated          : {saturated}");
            WriteLabel($"Current Channel    : {channel}");
            WriteLabel($"------------------------------");
            WriteLabel($"Sampeling   : {debugSampling}");
            WriteLabel($"RMS         : {rms}");
            WriteLabel($"DB          : {db}");
            WriteLabel($"------------------------------");
        }

        protected virtual void Log(object log)
        {
            Debug.Log(log);
        }

        protected virtual void LogError(object log)
        {
            Debug.LogError(log);
        }

        protected virtual void LogException(Exception log)
        {
            Debug.LogException(log);
        }

        public bool Subscribe(Action<float[]> callback)
        {
            if (!subscribed.Contains(callback))
            {
                subscribed.Add(callback);
                return true;
            }
            return false;
        }

        public bool UnSubscribe(Action<float[]> callback)
        {
            return subscribed.Remove(callback);
        }

    }
}