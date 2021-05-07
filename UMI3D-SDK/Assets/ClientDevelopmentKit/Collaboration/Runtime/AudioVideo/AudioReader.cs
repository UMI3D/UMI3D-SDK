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

using BeardedManStudios.Forge.Networking.Unity;
using System;
using System.Linq;
using umi3d.common.collaboration;
using UnityEngine;
using UnityOpus;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioReader : MonoBehaviour
    {

        ulong lastTimeStep = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void Read(byte[] data, ulong timestep)
        {
            MainThreadManager.Run(() => { OnEncoded(data, data.Length, timestep); });
        }

        #region Read
        const NumChannels channels = NumChannels.Mono;
        const SamplingFrequency frequency = SamplingFrequency.Frequency_12000;
        const int audioClipLength = 1024 * 6;
        AudioSource source;
        int head = 0;
        float[] audioClipData;

        void OnEnable()
        {
            source = GetComponent<AudioSource>();
            source.clip = AudioClip.Create("Loopback", audioClipLength, (int)channels, (int)frequency, false);
            source.loop = false;
            decoder = new Decoder(
                frequency,
                channels);
        }

        void OnDisable()
        {
            source.Stop();
            decoder.Dispose();
            decoder = null;
        }

        void OnDecoded(float[] pcm, int pcmLength)
        {
            if (audioClipData == null || audioClipData.Length != pcmLength)
            {
                // assume that pcmLength will not change.
                audioClipData = new float[pcmLength];
            }
            Array.Copy(pcm, audioClipData, pcmLength);
            source.clip.SetData(audioClipData, head);
            head += pcmLength;
            if (!source.isPlaying && head > audioClipLength / 2)
            {
                source.Play();
            }
            head %= audioClipLength;
        }
        #endregion

        #region Decoder


        Decoder decoder;
        readonly float[] pcmBuffer = new float[Decoder.maximumPacketDuration * (int)channels];

        void OnEncoded(byte[] data, int length, ulong timeStep)
        {
            if (timeStep - lastTimeStep > 500)
                head = 0;

            lastTimeStep = timeStep;
            var pcmLength = decoder.Decode(data, length, pcmBuffer);
            OnDecoded(pcmBuffer, pcmLength);
        }
        #endregion


    }

}
