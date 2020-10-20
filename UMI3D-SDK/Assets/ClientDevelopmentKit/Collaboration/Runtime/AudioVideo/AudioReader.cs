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
using umi3d.edk.collaboration;
using UnityEngine;
using System.Collections.Generic;

namespace umi3d.cdk.collaboration
{

    /// <summary>
    ///global reader for AudioDto.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioReader : MonoBehaviour, IAudioReader
    {
        AudioSource _audio;
        public int position = 0;
        public int samplerate = 44100;


        /// <summary>
        /// Read an AudioDto and play it in an audioSource.
        /// </summary>
        /// <param name="sample">AudioDto  to play</param>
        public void Read(AudioDto sample)
        {
            if (sample != null)
            {
                if (_audio == null)
                {
                    _audio = GetComponent<AudioSource>();
                    _audio.clip = AudioClip.Create("GlobalAudio", samplerate * 10, 1, samplerate, false, OnAudioRead, OnAudioSetPosition);
                }

                // Put the data in the audio source.
                _audio.clip.SetData(sample.sample, sample.pos);
                if (!_audio.isPlaying) _audio.Play();
            }
        }
        

        public void OnAudioRead(float[] data)
        {

        }

        public void OnAudioSetPosition(int newPosition)
        {
            position = newPosition;
        }
    }

}