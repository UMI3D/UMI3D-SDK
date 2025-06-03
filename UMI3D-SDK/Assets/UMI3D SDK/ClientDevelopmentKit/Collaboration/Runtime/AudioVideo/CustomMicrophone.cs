/*
Copyright 2019 - 2025 Inetum

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

using Mumble;
using System;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.Profiling;
using CSCore.SoundIn;
using CSCore;
using System.Linq;
using System.Threading.Tasks;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Custom microphone for <see cref="MumbleClient"/> not based on Unity interface for mics 
    /// but on <see cref="https://github.com/filoe/cscore/"/>. 
    /// </summary>
    public class CustomMicrophone : MumbleMicrophone
    {
        #region Fields

        #region Mic data

        /// <summary>
        /// Index of the current input device chosen to record audio.
        /// </summary>
        private int currentMicIndex = -1;

        /// <summary>
        /// Current sample rate used to record audio. Value forced to 48000 to be able to use noise reduction.
        /// </summary>
        private readonly int currentMicSampleRate = 48000;

        /// <summary>
        /// Two channels are recorded by default because some microphones only record data in one channel.
        /// (But a mono audio input is sent to murmure)
        /// </summary>
        private int numberOfChannel = 2;

        /// <summary>
        /// Input audio channel which is sent to the server. 0 = right channel, 1 = left channel.
        /// </summary>
        private int channelChoosen = 0;

        /// <summary>
        /// Is <see cref="channelChoosen"/> set up ?
        /// </summary>
        private bool isChannelChoosen = false;

        /// <summary>
        /// Audio data to process;
        /// </summary>
        private PcmArray newData;

        #endregion

        #region Record data

        /// <summary>
        /// Sends audio to Murmure server ?
        /// </summary>
        private bool shouldSendAudioToServer = false;

        /// <summary>
        /// Does microphone needs to record data.
        /// </summary>
        private bool needToRecord = false;

        /// <summary>
        /// Voice samples recorded from microphone.
        /// </summary>
        private List<float> data = new List<float>();

        /// <summary>
        /// Microphone recorder.
        /// </summary>
        private WaveIn waveIn;

        /// <summary>
        /// Last <see cref="MicType"/> used by the microphone last frame.
        /// </summary>
        private MicType lastMicrophoneMode;

        #endregion

        #region Audio Processing

        static readonly CustomSampler audioProcessingProfilerMarker = CustomSampler.Create("CustomMicrophone.Filters");

        private bool filterInit = false;

        private List<IMicrophoneFilter> filters = new();

        private bool useAudioEnhancement = true;

        public bool UseAudioEnhancement
        {
            get => useAudioEnhancement;
            set
            {
                useAudioEnhancement = value;

                foreach (IMicrophoneFilter filter in filters)
                {
                    filter.Enable = value;
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override int InitializeMic()
        {
            StopRecording();

            if (MicNumberToUse == currentMicIndex)
            {
                Debug.Log("Mic already init " + GetCurrentMicName());
                return currentMicSampleRate;
            }

            //Make sure there are are microphones connected.
            int nbDevices = WaveInDevice.EnumerateDevices().Count();

            if (nbDevices <= 0)
            {
                UMI3DLogger.LogError($"{nameof(CustomMicrophone)}.{nameof(InitializeMic)} : No microphone connected !", DebugScope.Collaboration);
                return -1;
            }
            else if (MicNumberToUse >= nbDevices)
            {
                UMI3DLogger.LogError($"{nameof(CustomMicrophone)}.{nameof(InitializeMic)} : Impossible to use mic nb {MicNumberToUse}, there is only {nbDevices} mics available.", DebugScope.Collaboration);
                return -1;
            }

            currentMicIndex = MicNumberToUse;
            isChannelChoosen = false;
            NumSamplesPerOutgoingPacket = MumbleConstants.NUM_FRAMES_PER_OUTGOING_PACKET * currentMicSampleRate / 100;

            InitMicrophoneAsync();

            if (!this.filterInit)
            {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX

                var settings = new AudioProcessingWebRTCWrapper.AudioProcessingSettings()
                {
                    sampleRate = currentMicSampleRate,
                    nbChannels = 1,
                    useNoiseReduction = true,
                    noiseReductionLevel = AudioProcessingWebRTCWrapper.NoiseReductionLevel.Moderate,
                    useEchoCanceller = true
                };

                this.filters.Add(new AECAndNoiseReductionMicrophoneFilter(settings));
#endif

                foreach (IMicrophoneFilter filter in filters)
                    filter.Enable = false;
                this.filterInit = true;
            }

            return currentMicSampleRate;
        }

        private async void InitMicrophoneAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    waveIn?.Dispose();
                    waveIn = new WaveIn(new WaveFormat(this.currentMicSampleRate, 16, numberOfChannel))
                    {
                        Device = WaveInDevice.EnumerateDevices().ElementAt(MicNumberToUse),
                        Latency = 100 // Delay, to be sure echo samples are recorded before mic samples
                    };

                    lock (waveIn)
                    {
                        waveIn.Initialize();// CPU Intensive
                        waveIn.DataAvailable += ProcessAudio;
                    }

                    UMI3DLogger.Log($"{nameof(CustomMicrophone)} : init with {waveIn.Device.Name}", DebugScope.Collaboration);
                });
            }
            catch (Exception ex)
            {
                UMI3DLogger.LogException(ex, DebugScope.Collaboration);
            }
        }

        /// <summary>
        /// Processes audio and add filters if enabled.
        /// </summary>
        /// <param name="micSampleRate"></param>
        protected void ProcessAudio(object o, DataAvailableEventArgs a)
        {
            if (!shouldSendAudioToServer)
                return;

            Profiler.BeginThreadProfiling("Others", "Custom Audio Thread");
            audioProcessingProfilerMarker.Begin();

            float[] tmpSamples = ConvertAudioInputToData(a.ByteCount, a.Data);

            bool useLoopBack = (MicrophoneListener.Exists && MicrophoneListener.Instance.useLocalLoopback);

            if (!useLoopBack)
            {
                foreach (IMicrophoneFilter filter in filters)
                {
                    if (!filter.Enable)
                        continue;

                    try
                    {
                        filter.ProcessAudio(tmpSamples);
                    }
                    catch (Exception e)
                    {
                        UMI3DLogger.LogError("Impossible to apply microphone filter " + filter.GetType().Name, DebugScope.Collaboration);
                        UMI3DLogger.LogException(e, DebugScope.Collaboration);
                    }
                }
            }

            lock (data)
                data.AddRange(tmpSamples);

            if (!isChannelChoosen)
                ChooseChannel(a.Data, a.ByteCount);

            audioProcessingProfilerMarker.End();
            Profiler.EndThreadProfiling();
        }

        float[] tmpSamples;

        private float[] ConvertAudioInputToData(int nbBytes, byte[] buffer)
        {
            try
            {
                int sampleCount = (int)(nbBytes / (2f * numberOfChannel));

                if (tmpSamples == null || tmpSamples.Length == sampleCount)
                    tmpSamples = new float[sampleCount];

                int j = 0;
                for (int index = 0; index < nbBytes; index += 2 * numberOfChannel)
                {
                    tmpSamples[j] = ((short)((buffer[channelChoosen + index + 1] << 8) | buffer[channelChoosen + index])) / (float)short.MaxValue;
                    j++;
                }

                return tmpSamples;
            }
            catch (Exception e)
            {
                UMI3DLogger.LogError($"{nameof(CustomMicrophone)}.{nameof(ConvertAudioInputToData)} error ", DebugScope.Mumble);
                UMI3DLogger.LogException(e, DebugScope.Mumble);

                return new float[0];
            }
        }

        /// <summary>
        /// Sets up <see cref="channelChoosen"/>, meaning choose the audio input channel to send to the server.
        /// </summary>
        /// <returns></returns>
        private void ChooseChannel(byte[] buffer, int nbByte)
        {
            if (!isChannelChoosen)
            {
                int right = 0;
                int left = 0;

                for (int i = 0; i < nbByte; i += 4)
                {
                    right = right + Mathf.Abs((short)((buffer[i + 1] << 8) | buffer[i]));
                    left = left + Mathf.Abs((short)((buffer[i + 3] << 8) | buffer[i + 2]));
                }

                var diff = Mathf.Abs(right - left);

                if (diff > 100000)
                {
                    isChannelChoosen = true;
                    channelChoosen = (right - left > 0) ? 0 : 2;
                    Debug.Log("Mic canal choosen " + channelChoosen);
                }
                else if (diff < 1000)
                {
                    isChannelChoosen = true;
                    channelChoosen = 0;
                    Debug.Log("Mic canal choosen " + channelChoosen);
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void SendVoiceIfReady()
        {
            lock (data)
            {
                bool sendData = VoiceSendingType == MicType.AlwaysSend;

                while (data.Count >= NumSamplesPerOutgoingPacket)
                {
                    newData = _mumbleClient.GetAvailablePcmArray();
                    newData.Pcm = data.GetRange(0, NumSamplesPerOutgoingPacket).ToArray();

                    if (VoiceSendingType == MicType.Amplitude && !AmplitudeHigherThan(MinAmplitude, newData.Pcm))
                    {
                        sendData = false;

                        //Drop data with low amplitude
                        if (data.Count > NumSamplesPerOutgoingPacket)
                            data = data.GetRange(NumSamplesPerOutgoingPacket + 1, data.Count - NumSamplesPerOutgoingPacket - 1);
                        else
                            data.Clear();
                    }
                    else
                    {
                        sendData = true;
                    }

                    if (sendData)
                    {
                        if (data.Count > NumSamplesPerOutgoingPacket)
                            data = data.GetRange(NumSamplesPerOutgoingPacket + 1, data.Count - NumSamplesPerOutgoingPacket - 1);

                        OnMicData?.Invoke(newData);

                        if (_writePositionalDataFunc != null)
                            _writePositionalDataFunc(ref newData.PositionalData, ref newData.PositionalDataLength);
                        else
                            newData.PositionalDataLength = 0;

                        _mumbleClient.SendVoicePacket(newData);
                    }
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="sampleRate"></param>
        public override void StartSendingAudio(int sampleRate)
        {
            data.Clear();

            shouldSendAudioToServer = true;
        }

        /// <summary>
        /// Starts recording user's voice.
        /// </summary>
        private void StartRecording()
        {
            if (!needToRecord && this.waveIn != null)
            {
                try
                {
                    foreach (IMicrophoneFilter filter in filters)
                        filter.Enable = UseAudioEnhancement;

                    waveIn.Start();
                    needToRecord = true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error, cannot start recording audio with : {GetCurrentMicName()} \n" + ex.Message);

                    needToRecord = false;
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void StopSendingAudio()
        {
            data.Clear();

            shouldSendAudioToServer = false;

            foreach (IMicrophoneFilter filter in filters)
                filter.Enable = false;
        }

        /// <summary>
        /// Stops recording user's voice.
        /// </summary>
        private void StopRecording()
        {
            if (needToRecord)
            {
                needToRecord = false;
                waveIn.Stop();

                foreach (IMicrophoneFilter filter in filters)
                    filter.Enable = false;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Update()
        {
            if (_mumbleClient == null || !_mumbleClient.ConnectionSetupFinished)
                return;

            if (VoiceSendingType == MicType.PushToTalk)
            {
                if (lastMicrophoneMode != MicType.PushToTalk)
                    StopRecording();

                if (PushToTalkInputDown)
                    StartRecording();
                else
                    StopRecording();
            }
            else
            {
                if (!_mumbleClient.IsSelfMuted())
                {
                    StartRecording();
                }
                else if (needToRecord)
                {
                    StopRecording();
                }
            }

            if (needToRecord && shouldSendAudioToServer)
                SendVoiceIfReady();

            lastMicrophoneMode = VoiceSendingType;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override bool HasMic()
        {
            return currentMicIndex != -1;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string GetCurrentMicName()
        {
            if (HasMic())
                return WaveInDevice.EnumerateDevices().ElementAt(currentMicIndex).Name;
            else
                return string.Empty;
        }

        protected void OnDestroy()
        {
            StopRecording();

            foreach (IMicrophoneFilter filter in filters)
            {
                if (filter is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            waveIn?.Dispose();
        }

        #endregion     
    }
}

