/*
Copyright 2019 Gfi Informatique

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

using BeardedManStudios;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Threading;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;

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
		public void StartRecording(int frequency = 16000, int sampleLen = 10)
		{
            StartVOIP();
        }

		/// <summary>
		/// Ends the Mic stream.
		/// </summary>
		public void StopRecording()
		{
            StopVoip();
		}

        /// <summary>
        /// 
        /// </summary>
        private int lastSample = 0;

        /// <summary>
        /// 
        /// </summary>
        private AudioClip mic = null;

        /// <summary>
        /// 
        /// </summary>
        private int channels = 1;

        /// <summary>
        /// 
        /// </summary>
        private int frequency = 8000;

        /// <summary>
        /// 
        /// </summary>
        private float[] samples = null;

        /// <summary>
        /// 
        /// </summary>
        private List<float> writeSamples = null;

        /// <summary>
        /// 
        /// </summary>
        private float WRITE_FLUSH_TIME = 0.5f;

        /// <summary>
        /// 
        /// </summary>
        private float writeFlushTimer = 0.0f;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField,EditorReadOnly]
        bool muted = false;

        /// <summary>
        /// 
        /// </summary>
        private void StartVOIP()
        {
            writeSamples = new List<float>(1024);
            MainThreadManager.Run(() =>
            {
                mic = Microphone.Start(null, true, 100, frequency);
                channels = mic.channels;
                if (mic == null)
                {
                    Debug.LogError("A default microphone was not found or plugged into the system");
                    return;
                }
                Task.Queue(VOIPWorker);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopVoip()
        {
            Microphone.End(null);
            mic = null;
        }

        /// <summary>
        /// 
        /// </summary>
        BMSByte writeBuffer = new BMSByte();

        /// <summary>
        /// 
        /// </summary>
        private void VOIPWorker()
        {
            while (!UMI3DCollaborationClientServer.Connected())
            {
                MainThreadManager.ThreadSleep(1000);
            }
            while (UMI3DCollaborationClientServer.Connected())
            {
                if (writeFlushTimer >= WRITE_FLUSH_TIME && writeSamples.Count > 0)
                {
                    writeFlushTimer = 0.0f;
                    lock (writeSamples)
                    {
                        writeBuffer.Clone(ToByteArray(writeSamples));
                        writeSamples.Clear();
                    }
                    if (!muted)
                    {
                        UMI3DCollaborationClientServer.Instance.ForgeClient.SendVOIP(writeBuffer.Size, writeBuffer.byteArr);
                    }
                }
                MainThreadManager.ThreadSleep(10);
            }
            StopVoip();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReadMic()
        {
            if (mic != null && UMI3DCollaborationClientServer.Connected())
            {
                writeFlushTimer += Time.deltaTime;
                int pos = Microphone.GetPosition(null);
                int diff = pos - lastSample;

                if (diff > 0)
                {
                    samples = new float[diff * channels];
                    mic.GetData(samples, lastSample);

                    lock (writeSamples)
                    {
                        writeSamples.AddRange(samples);
                    }
                }
                lastSample = pos;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sampleList"></param>
        /// <returns></returns>
        private byte[] ToByteArray(List<float> sampleList)
        {
            int len = sampleList.Count * 4;
            byte[] byteArray = new byte[len];
            int pos = 0;

            for (int i = 0; i < sampleList.Count; i++)
            {
                byte[] data = System.BitConverter.GetBytes(sampleList[i]);
                System.Array.Copy(data, 0, byteArray, pos, 4);
                pos += 4;
            }

            return byteArray;
        }

        /// <summary>
        /// 
        /// </summary>
        private void FixedUpdate()
        {
            ReadMic();
        }

    }
}
