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
using System.Collections.Generic;
using System.Threading;
using umi3d.common;
using UnityEngine;
using UnityOpus;
using System.Linq;
using System;
using System.Collections;
using MainThreadDispatcher;

namespace umi3d.cdk.collaboration
{
    [RequireComponent(typeof(AudioSource))]
    public class MicrophoneListener : Singleton<MicrophoneListener>
    {
        #region const

        /// <summary>
        /// Is the length of the AudioClip produced by the recording.
        /// </summary>
        const int lengthSeconds = 1;

        /// <summary>
        ///  RMS value for 0 dB
        /// </summary>
        const float refValue = 1f;

        #endregion

        #region static properties 

        /// <summary>
        /// Whether the microphone is running
        /// </summary>
        public static bool IsMute
        {
            get { return Exists ? Instance.muted : false; }
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

        public static string[] getDevices() => Exists ? Instance._getDevices() : null;

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

        private void Start()
        {
            IsMute = IsMute;
        }

        void _UpdateFrequency(int frequency)
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
        void StartRecording()
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
        void StopRecording()
        {
            Reading = false;
            Destroy(clip);
            Microphone.End(microphoneLabel);
        }

        #region ReadMicrophone

        /// <summary>
        /// 
        /// </summary>
        [SerializeField, EditorReadOnly]
        bool muted = false;

        float _gain = 1f;
        object gainLocker = new object();
        float _Gain
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

        object readingLocker = new object();
        bool reading = false;
        bool Reading
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

        string microphoneLabel;

        int samplingFrequency = 12000;



        AudioClip clip;
        int head = 0;
        float[] microphoneBuffer;

        private Thread thread;
        int sleepTimeMiliseconde = 5;

        float db;
        object dbLocker = new object();
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

        float rms;
        object RMSLocker = new object();
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


        object minRMSToSendLocker = new object();
        float _minRMSToSend = 0.1f;
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

        bool IslowerThanThreshold
        {
            get
            {
                var rms = RMS;
                var threshold = NoiseThreshold;
                return rms < threshold;
            }
        }

        bool shouldSend;
        object shouldSendLocker = new object();
        bool TurnMicOffRunning;
        public bool ShouldSend
        {
            get
            {
                var highRMS = !IslowerThanThreshold;
                lock (shouldSendLocker)
                {
                    shouldSend |= highRMS;
                    return shouldSend;
                }
            }
            private set
            {
                lock (shouldSendLocker)
                {
                    shouldSend = value;
                }
            }
        }

        float timeToTurnOff = 1f;
        IEnumerator TurnMicOff()
        {
            if (TurnMicOffRunning)
                yield break;
            TurnMicOffRunning = true;
            var time = Time.time + TimeToTurnOff;

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

        void _ChangeThreshold(bool up)
        {
            if (up)
                NoiseThreshold += 0.05f;
            else
                NoiseThreshold -= 0.05f;
        }

        void _ChangeBitrate(bool up)
        {
            if (up)
                Bitrate += 500;
            else
                Bitrate -= 500;
            if (encoder != null)
                encoder.Bitrate = Bitrate;
        }

        void _ChangeTimeToTurnOff(bool up)
        {
            if (up)
                TimeToTurnOff += 0.5f;
            else
                TimeToTurnOff -= 0.5f;
        }

        string[] _getDevices() => Microphone.devices;

        void _NextDevices()
        {
            var devices = _getDevices();
            var i = Array.IndexOf(devices, microphoneLabel) + 1;
            if (i < 0 || i >= devices.Length)
                i = 0;
            _SetDevices(devices[i]);
        }

        bool _SetDevices(string name)
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
                    microphoneLabel = name;
                return true;
            }
            return false;
        }

        bool _IsAValidDevices(string name)
        {
            if (name == null) return false;
            return getDevices().Contains(name);
        }

        void Update()
        {
            if (!Reading) return;

            var position = Microphone.GetPosition(null);
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

        int bitrate = 96000;
        int frameSize; //at least frequency/100
        int outputBufferSize; // at least frameSize * sizeof(float)

        Encoder encoder;
        Queue<float> pcmQueue;
        float[] frameBuffer;
        byte[] outputBuffer;

        void OnEnable()
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

        void OnDisable()
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

        void ThreadUpdate()
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
                    lock (pcmQueue)
                    {
                        for (int i = 0; i < frameSize; i++)
                        {
                            var v = pcmQueue.Dequeue() * gain;
                            if (v > 1) v = 1;
                            else if (v < -1) v = -1;
                            frameBuffer[i] = v;
                            sum += v * v;
                        }
                    }

                    RMS = Mathf.Sqrt(sum / frameSize);
                    DB = 20 * Mathf.Log10(RMS / refValue);

                    if (ShouldSend)
                    {
                        var encodedLength = encoder.Encode(frameBuffer, outputBuffer);
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
        #endregion
    }
}
