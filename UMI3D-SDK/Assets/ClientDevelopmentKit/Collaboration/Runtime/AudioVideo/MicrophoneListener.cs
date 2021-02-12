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

using System;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityOpus;

namespace umi3d.cdk.collaboration
{
    [RequireComponent(typeof(AudioSource))]
    public class MicrophoneListener : Singleton<MicrophoneListener>
    {
        /// <summary>
        /// Whether the microphone is running
        /// </summary>
        public static bool IsMute { get { return Exists ? Instance.muted : false; } set { if (Exists) Instance.muted = value; } }

        /// <summary>
        /// Starts to stream the input of the current Mic device
        /// </summary>
        public void StartRecording()
        {
            reading = true;
            clip = Microphone.Start(null, true, lengthSeconds, samplingFrequency);
        }

        /// <summary>
        /// Ends the Mic stream.
        /// </summary>
        public void StopRecording()
        {
            reading = false;
        }

        #region ReadMicrophone

        /// <summary>
        /// 
        /// </summary>
        [SerializeField, EditorReadOnly]
        bool muted = false;
        bool reading = false;

        const int samplingFrequency = 48000;
        const int lengthSeconds = 1;

        AudioClip clip;
        int head = 0;
        float[] processBuffer = new float[512];
        float[] microphoneBuffer = new float[lengthSeconds * samplingFrequency];

        public float GetRMS()
        {
            float sum = 0.0f;
            foreach (var sample in processBuffer)
            {
                sum += sample * sample;
            }
            return Mathf.Sqrt(sum / processBuffer.Length);
        }

        void Update()
        {
            if (!reading) return;

            var position = Microphone.GetPosition(null);
            if (position < 0 || head == position)
            {
                return;
            }

            clip.GetData(microphoneBuffer, 0);
            while (GetDataLength(microphoneBuffer.Length, head, position) > processBuffer.Length)
            {
                var remain = microphoneBuffer.Length - head;
                if (remain < processBuffer.Length)
                {
                    Array.Copy(microphoneBuffer, head, processBuffer, 0, remain);
                    Array.Copy(microphoneBuffer, 0, processBuffer, remain, processBuffer.Length - remain);
                }
                else
                {
                    Array.Copy(microphoneBuffer, head, processBuffer, 0, processBuffer.Length);
                }

                if (!muted)
                {
                    OnAudioReady(processBuffer);
                }

                head += processBuffer.Length;
                if (head > microphoneBuffer.Length)
                {
                    head -= microphoneBuffer.Length;
                }
            }
        }

        static int GetDataLength(int bufferLength, int head, int tail)
        {
            if (head < tail)
            {
                return tail - head;
            }
            else
            {
                return bufferLength - head + tail;
            }
        }

        #endregion

        #region Encoder

        const int bitrate = 96000;
        const int frameSize = 120;
        const int outputBufferSize = frameSize * 4; // at least frameSize * sizeof(float)

        Encoder encoder;
        Queue<float> pcmQueue = new Queue<float>();
        readonly float[] frameBuffer = new float[frameSize];
        readonly byte[] outputBuffer = new byte[outputBufferSize];

        void OnEnable()
        {
            encoder = new Encoder(
                SamplingFrequency.Frequency_48000,
                NumChannels.Mono,
                OpusApplication.Audio)
            {
                Bitrate = bitrate,
                Complexity = 10,
                Signal = OpusSignal.Music
            };
        }

        void OnDisable()
        {
            encoder.Dispose();
            encoder = null;
            pcmQueue.Clear();
        }

        void OnAudioReady(float[] data)
        {
            foreach (var sample in data)
            {
                pcmQueue.Enqueue(sample);
            }
            while (pcmQueue.Count > frameSize)
            {
                for (int i = 0; i < frameSize; i++)
                {
                    frameBuffer[i] = pcmQueue.Dequeue();
                }
                var encodedLength = encoder.Encode(frameBuffer, outputBuffer);
                if (UMI3DCollaborationClientServer.Exists
                    && UMI3DCollaborationClientServer.Instance?.ForgeClient != null
                    && UMI3DCollaborationClientServer.UserDto.status == StatusType.ACTIVE)
                    UMI3DCollaborationClientServer.Instance.ForgeClient.SendVOIP(encodedLength, outputBuffer);
            }
        }

        #endregion
    }
}
