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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using umi3d.common;
using UnityEngine;
using UnityEngine.Rendering;
using VolumetricLines;

namespace umi3d.cdk
{
    /// <summary>
    /// Load audio source from dto.
    /// </summary>
    public class AudioSourceDtoLoader : AbstractObjectDTOLoader<AudioSourceDto>
    {
        /// <summary>
        /// Create an audio source from an audiosource dto and raise a given callback.
        /// </summary>
        /// <param name="dto">Audio source dto to load</param>
        /// <param name="callback">Callback to raise (the argument is the audiosource)</param>
        public override void LoadDTO(AudioSourceDto dto, Action<GameObject> callback)
        {
            GameObject res = new GameObject();
            res.AddComponent<AudioSource>();
            callback(res);
            InitObjectFromDto(res, dto);
        }

        /// <summary>
        /// Update an audio source from dto.
        /// </summary>
        /// <param name="go">Audio source to update</param>
        /// <param name="olddto">Previous dto describing the audio source</param>
        /// <param name="newdto">Dto to update the audio source to</param>
        public override void UpdateFromDTO(GameObject go, AudioSourceDto olddto, AudioSourceDto newdto)
        {
            AudioSource audioSource = go.GetComponent<AudioSource>();

            UpdateProperties(audioSource, olddto, newdto);
            UpdateClip(audioSource, olddto, newdto);
            UpdateClipPlaying(audioSource, olddto, newdto);

            base.UpdateFromDTO(go, olddto, newdto);
        }


        /// <summary>
        /// Update an audio source from dto (internal use).
        /// </summary>
        /// <param name="audioSource">Audio source to update</param>
        /// <param name="olddto">Previous dto describing the audio source</param>
        /// <param name="newdto">Dto to update the audio source to</param>
        void UpdateProperties(AudioSource audioSource, AudioSourceDto olddto, AudioSourceDto newdto)
        {
            audioSource.mute = newdto.Mute;
            audioSource.bypassEffects = newdto.BypassEffects;
            audioSource.bypassListenerEffects = newdto.BypassListenerEffects;
            audioSource.bypassReverbZones = newdto.BypassReverbZone;
            audioSource.loop = newdto.Loop;
            audioSource.priority = newdto.Priority;
            audioSource.volume = newdto.Volume;
            audioSource.pitch = newdto.Pitch;
            audioSource.panStereo = newdto.StereoPan;
            audioSource.spatialBlend = newdto.SpatialBlend;
            audioSource.reverbZoneMix = newdto.ReverbZoneMix;

            audioSource.dopplerLevel = newdto.Sound3D_DopplerLevel;
            audioSource.spread = newdto.Sound3D_Spread;
            audioSource.rolloffMode = newdto.Sound3D_VolumeRolloff;
            audioSource.minDistance = newdto.Sound3D_MinDistance;
            audioSource.maxDistance = newdto.Sound3D_MaxDistance;
            
            audioSource.playOnAwake = newdto.PlayOnAwake;
        }

        /// <summary>
        /// Update the audio clip of the audio source (internal use).
        /// </summary>
        /// <param name="audioSource">Audio source to update</param>
        /// <param name="olddto">Previous dto describing the audio source</param>
        /// <param name="newdto">Dto to update the audio source to</param>
        void UpdateClip(AudioSource audioSource, AudioSourceDto olddto, AudioSourceDto newdto)
        {
            if (olddto == null  ||  !olddto.AudioClipResource.Equals(newdto.AudioClipResource))
            {
                var resource = new Resource();
                resource.Set(newdto.AudioClipResource);
                LoadClip(resource, audioSource, newdto);
            }
        }

        /// <summary>
        /// Play, pause or stop the audio source.
        /// </summary>
        /// <param name="audioSource">Audio source to update</param>
        /// <param name="olddto">Previous dto describing the audio source</param>
        /// <param name="newdto">Dto to update the audio source to</param>
        void UpdateClipPlaying(AudioSource audioSource, AudioSourceDto olddto, AudioSourceDto newdto)
        {
            if (olddto == null)
            {
                if (newdto.Playing)
                {
                    audioSource.Play();
                }
                else
                {
                    audioSource.Stop();
                }
            }
            else
            {
                if (!olddto.Playing  &&  newdto.Playing)
                {
                    audioSource.Play();
                }
                else if (olddto.Playing  &&  !newdto.Playing)
                {
                    if (newdto.Paused)
                    {
                        audioSource.Pause();
                    }
                    else
                    {
                        audioSource.Stop();
                    }
                }
            }

            if (olddto != null  &&  olddto.PlayStartTimeInMs != newdto.PlayStartTimeInMs)
                audioSource.time = ((float) (getTimeInMilliseconds() - double.Parse(newdto.PlayStartTimeInMs))) / 1000f;
        }


        /// <summary>
        /// Load the audio clip of the audio source (internal use).
        /// </summary>
        /// <param name="resource">Audio clip to download</param>
        /// <param name="audioSource">Audio source to update</param>
        /// <param name="newdto">Dto to update the audio source to</param>
        void LoadClip(Resource resource, AudioSource audioSource, AudioSourceDto newdto)
        {
            if (resource.Url != "")
            HDResourceCache.Download(resource, (AudioClip audioclip) =>
            {
                audioSource.clip = audioclip;

                if (audioclip != null)
                {

                    if (newdto.Playing)
                    {
                        audioSource.Play();

                        audioSource.time = ((float)((getTimeInMilliseconds() - double.Parse(newdto.PlayStartTimeInMs)) % (audioclip.length * 1000))) / 1000f;
                    }
                }
            });
        }



        double getTimeInMilliseconds()
        {
            return (DateTime.Now - DateTime.MinValue).TotalMilliseconds;
        }
    }
}
