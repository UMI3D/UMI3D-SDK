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

namespace umi3d.cdk.collaboration
{
    [RequireComponent(typeof(AudioSource))]
    public class MicrophoneListener : Singleton<MicrophoneListener>
    {
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

        /// <summary>
        /// Starts to stream the input of the current Mic device
        /// </summary>
        void StartRecording()
        {
            reading = true;
            clip = Microphone.Start(null, true, lengthSeconds, (int)samplingFrequency);
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
            reading = false;
            Destroy(clip);
            Microphone.End(null);
        }

        #region ReadMicrophone

        /// <summary>
        /// 
        /// </summary>
        [SerializeField, EditorReadOnly]
        bool muted = false;
        bool reading = false;

        const SamplingFrequency samplingFrequency = SamplingFrequency.Frequency_12000;

        const int lengthSeconds = 1;

        AudioClip clip;
        int head = 0;
        float[] microphoneBuffer = new float[lengthSeconds * (int)samplingFrequency];


        private Thread thread;
        int sleepTimeMiliseconde = 5;

        void Update()
        {
            if (!reading) return;

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

        const int bitrate = 96000;
        const int frameSize = 240; //at least frequency/100
        const int outputBufferSize = frameSize * 4; // at least frameSize * sizeof(float)

        Encoder encoder;
        Queue<float> pcmQueue = new Queue<float>();
        readonly float[] frameBuffer = new float[frameSize];
        readonly byte[] outputBuffer = new byte[outputBufferSize];

        void OnEnable()
        {
            encoder = new Encoder(
                samplingFrequency,
                NumChannels.Mono,
                OpusApplication.Audio)
            {
                Bitrate = bitrate,
                Complexity = 10,
                Signal = OpusSignal.Voice
            };
        }

        void OnDisable()
        {
            encoder.Dispose();
            encoder = null;
            pcmQueue.Clear();
            reading = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            reading = false;
        }

        void ThreadUpdate()
        {
            while (reading)
            {
                bool ok = false;
                lock (pcmQueue)
                {
                    ok = pcmQueue.Count >= frameSize;
                }
                if (ok)
                {
                    lock (pcmQueue)
                    {
                        for (int i = 0; i < frameSize; i++)
                        {
                            frameBuffer[i] = pcmQueue.Dequeue();
                        }
                    }
                    var encodedLength = encoder.Encode(frameBuffer, outputBuffer);
                    if (UMI3DCollaborationClientServer.Exists
                        && UMI3DCollaborationClientServer.Instance?.ForgeClient != null
                        && UMI3DCollaborationClientServer.UserDto.status == StatusType.ACTIVE)
                    {
                        UMI3DCollaborationClientServer.Instance.ForgeClient.SendVOIP(encodedLength, outputBuffer);
                    }
                }
                Thread.Sleep(sleepTimeMiliseconde);
            }
            thread = null;
        }


        #endregion
    }
}
