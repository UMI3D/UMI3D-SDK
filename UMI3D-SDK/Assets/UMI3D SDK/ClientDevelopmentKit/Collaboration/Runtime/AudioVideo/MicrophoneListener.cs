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
using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using umi3d.common;
using UnityEngine;
using UnityOpus;

namespace umi3d.cdk.collaboration
{
    [Serializable]
    public class MicrophoneEvent : UnityEngine.Events.UnityEvent<bool>
    {
    }

    [RequireComponent(typeof(AudioSource))]
    public class MicrophoneListener : SingleBehaviour<MicrophoneListener>, ILoggable
    {

        #region const

        /// <summary>
        /// Is the length of the AudioClip produced by the recording.
        /// </summary>
        private const int lengthSeconds = 1;

        /// <summary>
        ///  RMS value for 0 dB
        /// </summary>
        private const float refValue = 1f;

        #endregion

        #region static properties 

        public static MicrophoneEvent OnSaturated => Exists ? Instance._OnSaturated : null;
        public static MicrophoneEvent OnSendingData => Exists ? Instance._OnSending : null;
        /// <summary>
        /// Whether the microphone is running
        /// </summary>
        public static bool IsMute
        {
            get => Exists ? Instance.muted : false;
            set
            {
                if (Exists)
                {
                    if (Instance.muted != value)
                    {
                        if (value) Instance.StopRecording();
                        else Instance.StartRecording();
                    }

                    Instance.muted = value;
                }
            }
        }

        public static bool LoopBack
        {
            get => Exists ? Instance.loopback : false;
            set
            {
                if (Exists && Instance.loopback != value)
                {
                    Instance.loopback = value;
                    if (Instance.reading)
                    {
                        Instance._LoopBack();
                    }
                }
            }
        }


        public static string CurrentMicrophone
        {
            get => Exists ? Instance.microphoneLabel : "";
            set => SetDevices(value);
        }

        public static float NoiseThreshold
        {
            get => Exists ? Instance._MinRMSToSend : -1;
            set
            {
                if (Exists)
                    Instance._MinRMSToSend = value;
            }
        }

        public static float TimeToTurnOff
        {
            get => Exists ? Instance.timeToTurnOff : -1;
            set
            {
                if (Exists)
                    Instance.timeToTurnOff = value > 0 ? value : 0;
            }
        }

        public static int Bitrate
        {
            get => Exists ? Instance.bitrate : -1;
            set
            {
                if (Exists)
                    Instance.bitrate = value;
            }
        }

        public static float Gain
        {
            get => Exists ? Instance._Gain : -1;
            set
            {
                if (Exists)
                    Instance._Gain = value;
            }
        }

        #endregion

        #region static method

        public static void UpdateFrequency(int frequency)
        {
            if (Exists) Instance._UpdateFrequency(frequency);
        }

        public static void ChangeThreshold(bool up)
        {
            if (Exists) Instance._ChangeThreshold(up);
        }

        public static void ChangeBitrate(bool up)
        {
            if (Exists) Instance._ChangeBitrate(up);
        }

        public static void ChangeTimeToTurnOff(bool up)
        {
            if (Exists) Instance._ChangeTimeToTurnOff(up);
        }

        public static string[] getDevices()
        {
            return Exists ? Instance._getDevices() : null;
        }

        public static void NextDevices()
        {
            if (Exists) Instance._NextDevices();
        }

        public static bool SetDevices(string name)
        {
            return Exists ? Instance._SetDevices(name) : false;
        }

        public static bool IsAValidDevices(string name)
        {
            return Exists ? Instance._IsAValidDevices(name) : false;
        }

        #endregion

        public MicrophoneEvent _OnSaturated = new MicrophoneEvent();
        public MicrophoneEvent _OnSending = new MicrophoneEvent();
        private AudioSource audioSource;

        private void Start()
        {
            IsMute = IsMute;
            audioSource = GetComponent<AudioSource>();
            UMI3DLogger.Register(this);
        }

        private void _UpdateFrequency(int frequency)
        {
            samplingFrequency = frequency;
            if (Reading)
            {
                OnDisable();
                OnEnable();
                StopRecording();
                StartRecording();
            }
            else
            {
                OnDisable();
                OnEnable();
            }
        }

        /// <summary>
        /// Starts to stream the input of the current Mic device
        /// </summary>
        private void StartRecording()
        {
            Reading = true;

            frameSize = samplingFrequency / 100; //at least frequency/100
            outputBufferSize = frameSize * sizeof(float); // at least frameSize * sizeof(float)
            pcmQueue = new Queue<float>();
            frameBuffer = new float[frameSize];
            outputBuffer = new byte[outputBufferSize];
            microphoneBuffer = new float[lengthSeconds * samplingFrequency];

            if (!IsAValidDevices(microphoneLabel))
                microphoneLabel = Microphone.devices[0];

            clip = Microphone.Start(microphoneLabel, true, lengthSeconds, samplingFrequency);

            _LoopBack();

            lock (pcmQueue)
                pcmQueue.Clear();
            if (thread == null)
                thread = new Thread(ThreadUpdate);
            if (!thread.IsAlive)
                thread.Start();
        }

        /// <summary>
        /// Ends the Mic stream.
        /// </summary>
        private void StopRecording()
        {
            Reading = false;
            Destroy(clip);
            Microphone.End(microphoneLabel);
        }

        private void _LoopBack()
        {
            if (loopback)
            {
                audioSource.clip = clip;
                audioSource.loop = true;
                audioSource.Play();
            }
            else
            {
                audioSource.Stop();
                audioSource.clip = null;
            }
        }

        #region ReadMicrophone

        /// <summary>
        /// 
        /// </summary>
        [SerializeField, EditorReadOnly]
        private bool muted = false;
        private float _gain = 1f;
        private readonly object gainLocker = new object();

        private float _Gain
        {
            get
            {
                lock (gainLocker)
                    return _gain;
            }
            set
            {
                lock (gainLocker)
                    _gain = value > 0 ? value : 0;
            }
        }

        private readonly object readingLocker = new object();
        private bool reading = false;

        private bool Reading
        {
            get
            {
                lock (readingLocker)
                    return reading;
            }
            set
            {
                lock (readingLocker)
                    reading = value;
            }
        }

        private string microphoneLabel;
        private int samplingFrequency = 12000;
        private AudioClip clip;
        private int head = 0;
        private float[] microphoneBuffer;

        private Thread thread;
        private readonly int sleepTimeMiliseconde = 5;
        private float db;
        private readonly object dbLocker = new object();
        public float DB
        {
            get
            {
                lock (dbLocker)
                    return db;
            }
            private set
            {
                lock (dbLocker)
                    db = value;
            }
        }

        private float rms;
        private readonly object RMSLocker = new object();
        public float RMS
        {
            get
            {
                lock (RMSLocker)
                    return rms;
            }
            private set
            {
                lock (RMSLocker)
                    rms = value;
                if (IslowerThanThreshold)
                    UnityMainThreadDispatcher.Instance().Enqueue(TurnMicOff());
            }
        }

        private bool currentSaturated;
        private bool displayedSaturated;
        private readonly object SaturatedLocker = new object();
        private readonly object displayedSaturatedLocker = new object();

        public bool DisplayedSaturated
        {
            get
            {
                lock (displayedSaturatedLocker)
                    return displayedSaturated;
            }
            set
            {
                bool v;
                lock (displayedSaturatedLocker)
                    v = displayedSaturated;
                if (v != value)
                {
                    lock (displayedSaturatedLocker)
                        displayedSaturated = value;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _OnSaturated.Invoke(value));
                }
            }
        }

        public bool Saturated
        {
            get
            {
                lock (SaturatedLocker)
                    return displayedSaturated;
            }
            private set
            {
                DisplayedSaturated |= value;
                lock (SaturatedLocker)
                    currentSaturated = value;
                if (value)
                    UnityMainThreadDispatcher.Instance().Enqueue(StaySaturated());
            }
        }

        private readonly object minRMSToSendLocker = new object();
        private float _minRMSToSend = 0f;
        public float _MinRMSToSend
        {
            get
            {
                lock (minRMSToSendLocker)
                    return _minRMSToSend;
            }
            set
            {
                lock (minRMSToSendLocker)
                    _minRMSToSend = Mathf.Clamp01(value);
            }
        }

        private bool IslowerThanThreshold
        {
            get
            {
                float rms = RMS;
                float threshold = NoiseThreshold;
                return rms < threshold;
            }
        }

        private bool shouldSend;
        private readonly object shouldSendLocker = new object();
        private bool TurnMicOffRunning;
        public bool ShouldSend
        {
            get
            {
                if (loopback) return false;

                bool highRMS = !IslowerThanThreshold;
                lock (shouldSendLocker)
                {
                    shouldSend |= highRMS;
                    return shouldSend;
                }
            }
            private set
            {
                bool ok;
                lock (shouldSendLocker)
                    ok = shouldSend;
                if (ok != value)
                {
                    lock (shouldSendLocker)
                        shouldSend = value;
                    UnityMainThreadDispatcher.Instance().Enqueue(() => _OnSending.Invoke(value));
                }
            }
        }

        private float timeToTurnOff = 1f;

        private IEnumerator TurnMicOff()
        {
            if (TurnMicOffRunning)
                yield break;
            TurnMicOffRunning = true;
            float time = Time.time + TimeToTurnOff;

            while (IslowerThanThreshold)
            {
                if (time <= Time.time)
                {
                    ShouldSend = false;
                    TurnMicOffRunning = false;
                    yield break;
                }
                yield return null;
            }
            ShouldSend = true;
            TurnMicOffRunning = false;
        }

        private bool StaySaturatedRunning;
        private readonly float timeStayingSaturated = 0.3f;

        private IEnumerator StaySaturated()
        {
            if (StaySaturatedRunning)
                yield break;
            StaySaturatedRunning = true;
            DisplayedSaturated = true;
            float time = 0;

            do
            {
                if (currentSaturated)
                {
                    time = Time.time + timeStayingSaturated;
                    currentSaturated = false;
                }
                yield return null;
            }
            while (time > Time.time);

            DisplayedSaturated = false;
            StaySaturatedRunning = false;
        }

        private void _ChangeThreshold(bool up)
        {
            if (up)
                NoiseThreshold += 0.05f;
            else
                NoiseThreshold -= 0.05f;
        }

        private void _ChangeBitrate(bool up)
        {
            if (up)
                Bitrate += 500;
            else
                Bitrate -= 500;
            if (encoder != null)
                encoder.Bitrate = Bitrate;
        }

        private void _ChangeTimeToTurnOff(bool up)
        {
            if (up)
                TimeToTurnOff += 0.5f;
            else
                TimeToTurnOff -= 0.5f;
        }

        private string[] _getDevices()
        {
            return Microphone.devices;
        }

        private void _NextDevices()
        {
            string[] devices = _getDevices();
            int i = Array.IndexOf(devices, microphoneLabel) + 1;
            if (i < 0 || i >= devices.Length)
                i = 0;
            _SetDevices(devices[i]);
        }

        private bool _SetDevices(string name)
        {
            if (_IsAValidDevices(name))
            {
                if (Reading)
                {
                    StopRecording();
                    microphoneLabel = name;
                    StartRecording();
                }
                else
                {
                    microphoneLabel = name;
                }

                return true;
            }
            return false;
        }

        private bool _IsAValidDevices(string name)
        {
            if (name == null) return false;
            return getDevices().Contains(name);
        }

        private void Update()
        {
            if (!Reading) return;

            int position = Microphone.GetPosition(microphoneLabel);
            if (position < 0 || head == position)
            {
                return;
            }

            clip.GetData(microphoneBuffer, 0);
            if (!muted)
            {
                if (head < position)
                {
                    lock (pcmQueue)
                    {
                        for (int i = head; i < position; i++)
                        {
                            pcmQueue.Enqueue(microphoneBuffer[i]);
                        }
                    }
                }
                else
                {
                    lock (pcmQueue)
                    {
                        //head -> length
                        for (int i = head; i < microphoneBuffer.Length; i++)
                        {
                            pcmQueue.Enqueue(microphoneBuffer[i]);
                        }
                        //0->position
                        for (int i = 0; i < position; i++)
                        {
                            pcmQueue.Enqueue(microphoneBuffer[i]);
                        }
                    }
                }
            }
            head = position;
        }

        #endregion

        #region Encoder
        private readonly object loopbackLocker = new object();
        private bool _loopback;
        private bool loopback
        {
            get
            {
                lock (loopbackLocker)
                    return _loopback;
            }
            set
            {
                lock (loopbackLocker)
                    _loopback = value;
            }
        }

        private int bitrate = 96000;
        private int frameSize; //at least frequency/100
        private int outputBufferSize; // at least frameSize * sizeof(float)

        private Encoder encoder;
        private Queue<float> pcmQueue;
        private float[] frameBuffer;
        private byte[] outputBuffer;

        private void OnEnable()
        {
            var samp = (SamplingFrequency)samplingFrequency;
            encoder = new Encoder(
                samp,
                NumChannels.Mono,
                OpusApplication.Audio)
            {
                Bitrate = Bitrate,
                Complexity = 10,
                Signal = OpusSignal.Voice
            };
        }

        private void OnDisable()
        {
            encoder.Dispose();
            encoder = null;
            pcmQueue?.Clear();
            Reading = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Reading = false;
        }

        private void ThreadUpdate()
        {
            while (Reading)
            {
                bool ok = false;
                lock (pcmQueue)
                {
                    ok = pcmQueue.Count >= frameSize;
                }
                if (ok)
                {
                    float sum = 0;
                    float gain = Gain;
                    bool saturated = false;
                    lock (pcmQueue)
                    {
                        for (int i = 0; i < frameSize; i++)
                        {
                            float v = pcmQueue.Dequeue() * gain;
                            if (v > 1)
                            {
                                v = 1;
                                saturated = true;
                            }
                            else if (v < -1)
                            {
                                v = -1;
                                saturated = true;
                            }

                            frameBuffer[i] = v;
                            sum += v * v;
                        }
                    }

                    Saturated = saturated;
                    RMS = Mathf.Sqrt(sum / frameSize);
                    DB = 20 * Mathf.Log10(RMS / refValue);

                    if (ShouldSend)
                    {
                        int encodedLength = encoder.Encode(frameBuffer, outputBuffer);
                        if (UMI3DCollaborationClientServer.Exists
                            && UMI3DCollaborationClientServer.Instance?.ForgeClient != null
                            && UMI3DCollaborationClientServer.UserDto.dto.status == StatusType.ACTIVE)
                        {
                            UMI3DCollaborationClientServer.Instance.ForgeClient.SendVOIP(encodedLength, outputBuffer);
                        }
                    }
                }
                Thread.Sleep(sleepTimeMiliseconde);
            }
            thread = null;
        }

        private const string LogName = "Microphone Listener";
        string ILoggable.GetLogName()
        {
            return LogName;
        }

        List<DebugInfo> ILoggable.GetInfos()
        {
            return new List<DebugInfo>
            {
                new DebugInfo<string>("Current Microphone",()=>CurrentMicrophone),
                new DebugInfo<string[]>("Microphones",getDevices(),(l)=>l.ToString<string>()),
                new DebugInfo<int>("Sampling Frequency (Hz)",()=>samplingFrequency),
                new DebugInfo<int>("Bitrate (b/s)",()=>Bitrate),
                new DebugInfo<int>("Frame Size (float)",()=>frameSize),
                new DebugInfo<int>("Output Buffer Size (bytes)",()=>outputBufferSize),
                new DebugInfo<int>("PCM Queue Size",()=>
                    {
                        if(pcmQueue != null)
                            lock (pcmQueue)
                                return pcmQueue.Count;
                        return 0;
                    }
                ),
                new DebugInfo<(float,float,bool)>("RMS",()=>(RMS,NoiseThreshold,ShouldSend),(t)=>$"{t.Item1}[>{t.Item2}=>{t.Item3}]"),
                new DebugInfo<float>(" DB",()=>DB),
                new DebugInfo<float>(" Gain",()=>Gain),
                new DebugInfo<float>(" Time to turn off",()=>TimeToTurnOff),
            };
        }
        #endregion
    }
}