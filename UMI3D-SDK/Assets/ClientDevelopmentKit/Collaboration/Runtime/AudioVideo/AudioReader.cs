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
    ///global reader for AudioDto.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioReader : MonoBehaviour, IAudioReader
    {
        AudioSource _audio;
        [SerializeField, ReadOnly]
        int sampleLength;

        /// <summary>
        /// Read an AudioDto and play it in an audioSource.
        /// </summary>
        /// <param name="sample">AudioDto  to play</param>
        public void Read(AudioDto sample)
        {
            if (_audio == null)
                SetUp(sample);
            var index = sample.pos % sampleLength;
            if (sample != null)
            {
                _audio.clip.SetData(sample.sample, index);
                if (!_audio.isPlaying) _audio.Play();
            }
        }


        public void OnAudioRead(float[] data)
        {

        }

        public void OnAudioSetPosition(int newPosition)
        {
        }

        public void SetUp(AudioDto sample)
        {
            sampleLength = sample.sample.Length;
            _audio = GetComponent<AudioSource>();
            _audio.clip = AudioClip.Create("GlobalAudio", sampleLength, 1, sample.frequency, false, OnAudioRead, OnAudioSetPosition);
        }
    }
}