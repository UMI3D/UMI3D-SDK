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

using CSCore.SoundIn;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using umi3d.common;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// A filter which perfoms an acoustic echo cancellation and noise reduction based on WebRTC Voice Engine.
    /// </summary>
    public class AECAndNoiseReductionMicrophoneFilter : IMicrophoneFilter, IDisposable
    {
        private const int MAX_ECHO_QUEUE_SIZE = 4096;

        #region Fields

        private bool enable = true;

        /// <inheritdoc/>
        public bool Enable
        {
            get => this.enable;

            set
            {
                this.enable = value;
                this.echoSamples.Clear();

                if (this.capture == null)
                    return;

                if (value && !this.isCapturing)
                {
                    this.isCapturing = true;
                    this.capture.Start();
                }
                else if (!value && this.isCapturing)
                {
                    this.capture.Stop();

                    isCapturing = false;
                }
            }
        }

        /// <summary>
        /// Audio played by the OS.
        /// </summary>
        private Queue<float> echoSamples = new();

        /// <summary>
        /// Recorder for audio system.
        /// </summary>
        private WasapiLoopbackCapture capture;

        private bool isCapturing = false;

        /// <summary>
        /// Bytes per sample for <see cref="capture"/>
        /// </summary>
        private int bytesPerSample;

        /// <summary>
        /// Buffer used by <see cref="IMicrophoneFilter.ProcessAudio"/>.
        /// </summary>
        private short[] echoShortSamples;

        /// <summary>
        /// Buffer used by <see cref="IMicrophoneFilter.ProcessAudio"/>.
        /// </summary>
        private short[] micShortSamples;

        /// <summary>
        /// Buffer used by <see cref="IMicrophoneFilter.ProcessAudio"/>.
        /// </summary>
        private short[] outShortSamples;

        private AudioProcessingWebRTCWrapper.AudioProcessingSettings settings;

        #endregion

        public AECAndNoiseReductionMicrophoneFilter(AudioProcessingWebRTCWrapper.AudioProcessingSettings settings)
        {
            this.settings = settings;

            InitAsync();
        }

        private async void InitAsync()
        {
            try
            {
                AudioProcessingWebRTCWrapper.Init(settings);

                await Task.Run(() =>
                {
                    // we can't change WasapiLoopbackCapture format to mono sound, otherwise it can fail
                    this.capture = new WasapiLoopbackCapture(100, new(48000, 16, 2));
                    this.capture.Initialize(); // CPU Intensive
                    this.capture.DataAvailable += RecordSystemAudio;
                    this.bytesPerSample = capture.WaveFormat.BitsPerSample / 8;
                });

                this.Enable = this.enable;
            }
            catch (Exception ex)
            {
                UMI3DLogger.LogError($"{nameof(AECAndNoiseReductionMicrophoneFilter)} : cannot init system audio recording, AEC won't be performed", DebugScope.Collaboration);
                UMI3DLogger.LogException(ex, DebugScope.Collaboration);
                this.capture = null;
            }
        }

        private void RecordSystemAudio(object sender, DataAvailableEventArgs e)
        {
            int sampleCount = e.ByteCount / bytesPerSample;

            lock(this.echoSamples)
            {
                float sample = 0f;

                for (int i = 0; i < sampleCount; i += capture.WaveFormat.Channels)
                {
                    switch (this.bytesPerSample)
                    {
                        case 2:
                            short shortSample = BitConverter.ToInt16(e.Data, i * bytesPerSample);
                            sample = shortSample / (float)(short.MaxValue);
                            break;
                        case 4:
                            int intSample = BitConverter.ToInt32(e.Data, i * bytesPerSample);
                            sample = intSample / (float)(int.MaxValue);
                            break;
                        default:
                            break;
                    }

                    if (this.echoSamples.Count > MAX_ECHO_QUEUE_SIZE)
                        _ = this.echoSamples.Dequeue();

                    this.echoSamples.Enqueue(sample);
                }
            }
        }

        void IMicrophoneFilter.ProcessAudio(float[] samples)
        {
            if (!this.Enable || this.capture == null)
                return;

            // nothing to do
            if (!this.settings.useEchoCanceller && !this.settings.useNoiseReduction)
                return;

                int bufferSize = samples.Length;

            if (echoShortSamples == null || echoShortSamples.Length != bufferSize)
                echoShortSamples = new short[bufferSize];

            if (micShortSamples == null || micShortSamples.Length != bufferSize)
                micShortSamples = new short[bufferSize];

            if (outShortSamples == null || outShortSamples.Length != bufferSize)
                outShortSamples = new short[bufferSize];

            // 1. Convert samples to short samples and get echo samples.
            lock (this.echoSamples)
            {
                for (int i = 0; i < bufferSize; i++)
                {
                    micShortSamples[i] = (short)(samples[i] * short.MaxValue);

                    if (this.echoSamples.Count > 0)
                        echoShortSamples[i] = (short)(this.echoSamples.Dequeue() * short.MaxValue);
                    else
                        echoShortSamples[i] = 0;
                }
            }

            // 2. Process audio
            AudioProcessingWebRTCWrapper.ProcessAudio(micShortSamples, echoShortSamples, outShortSamples);

            // 3. Convert output to float.
            for (int i = 0; i < bufferSize; i++)
            {
                samples[i] = (outShortSamples[i] / (float)short.MaxValue);
            }
        }

        void IDisposable.Dispose()
        {
            this.isCapturing = false;
            this.capture?.Dispose();
            AudioProcessingWebRTCWrapper.Destroy();
        }
    }
}