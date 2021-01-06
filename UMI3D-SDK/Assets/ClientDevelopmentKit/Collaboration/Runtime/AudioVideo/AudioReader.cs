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

#if !UNITY_WEBGL && !WINDOWS_UWP
using System;
using System.Collections.Generic;
using umi3d.common.collaboration;
using UnityEngine;
namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioReader : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        private int readUpdateId = 0;

        /// <summary>
        /// 
        /// </summary>
        private int previousReadUpdateId = -1;

        /// <summary>
        /// 
        /// </summary>
        private List<float> readSamples = null;

        /// <summary>
        /// 
        /// </summary>
        private float READ_FLUSH_TIME = 0.5f;

        /// <summary>
        /// 
        /// </summary>
        private float readFlushTimer = 0.0f;

        /// <summary>
        /// 
        /// </summary>
        private AudioSource audioSource;

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
        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            readFlushTimer += Time.deltaTime;
            if (readFlushTimer > READ_FLUSH_TIME)
            {
                if (readUpdateId != previousReadUpdateId && readSamples != null && readSamples.Count > 0)
                {
                    previousReadUpdateId = readUpdateId;
                    lock (readSamples)
                    {
                        audioSource.clip = AudioClip.Create(gameObject.name + "VoIP", readSamples.Count, channels, frequency, false);
                        audioSource.clip.SetData(readSamples.ToArray(), 0);
                        if (!audioSource.isPlaying) audioSource.Play();
                        readSamples.Clear();
                    }
                }
                readFlushTimer = 0.0f;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dto"></param>
        public void Read(VoiceDto dto)
        {
            float[] tmp = ToFloatArray(dto.data, dto.length);
            if (readSamples == null)
                readSamples = new List<float>(tmp);
            lock (readSamples)
            {
                readSamples.AddRange(tmp);
            }
            readUpdateId++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private float[] ToFloatArray(byte[] data, int length)
        {
            int len = (length) / 4;
            float[] floatArray = new float[len];

            for (int i = 0; i < length; i += 4)
                floatArray[i / 4] = BitConverter.ToSingle(data, i);

            return floatArray;
        }

    }

}
#endif
