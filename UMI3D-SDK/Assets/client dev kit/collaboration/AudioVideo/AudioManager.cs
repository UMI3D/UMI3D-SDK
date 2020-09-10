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

using umi3d.common;
using UnityEngine;

namespace umi3d.cdk.collaboration
{

    /// <summary>
    /// Singleton use to read AudioDto.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : Singleton<AudioManager>
    {
        AudioSource _audio;
        public int position = 0;
        public int samplerate = 44100;

        /// <summary>
        /// Read an Audio Dto and play it in an audioSource.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="channel"></param>
        public void Read(byte[] sample, DataChannel channel)
        {
            var dto = UMI3DDto.FromBson(sample) as AudioDto;
            if (_audio == null)
            {
                _audio = GetComponent<AudioSource>();
                _audio.clip = AudioClip.Create("test", samplerate * 10, 1, samplerate, false, OnAudioRead, OnAudioSetPosition);
            }
            if (sample != null)
            {
                // Put the data in the audio source.
                _audio.clip.SetData(dto.sample, dto.pos);
                if (!_audio.isPlaying) _audio.Play();
            }
        }

        void OnAudioRead(float[] data)
        {

        }

        void OnAudioSetPosition(int newPosition)
        {
            position = newPosition;
        }
    }
}