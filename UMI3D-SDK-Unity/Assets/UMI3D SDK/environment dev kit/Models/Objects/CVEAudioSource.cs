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
using umi3d.common;
using UnityEngine;
using UnityEngine.Networking;

namespace umi3d.edk
{
    /// <summary>
    /// Scene audio source.
    /// </summary>
    [System.Serializable]
    public class CVEAudioSource : AbstractObject3D<AudioSourceDto>
    {
        /// <summary>
        /// Audio clip resource to play.
        /// </summary>
        public CVEResource AudioClipResource = new CVEResource();

        /// <summary>
        /// Mute the sound.
        /// </summary>
        public bool Mute = false;

        /// <summary>
        /// Should bypass all effects ?
        /// </summary>
        public bool BypassEffects = false;

        /// <summary>
        /// Should bypass listener effects ?
        /// </summary>
        public bool BypassListenerEffects = false;

        /// <summary>
        /// Should bypass reverb effects.
        /// </summary>
        public bool BypassReverbZone = false;

        /// <summary>
        /// Should the sound be payed on object awake.
        /// </summary>
        public bool PlayOnAwake = true;

        /// <summary>
        /// Should the sound loop.
        /// </summary>
        public bool Loop = false;

        /// <summary>
        /// Sound priority among other sound sources.
        /// </summary>
        [RangeAttribute(0, 256)]
        public int Priority = 128;

        /// <summary>
        /// Audio volume.
        /// </summary>
        [RangeAttribute(0f, 1f)]
        public float Volume = 1f;

        /// <summary>
        /// Audio pitch alteration.
        /// </summary>
        [RangeAttribute(-3f, 3f)]
        public float Pitch = 1f;

        /// <summary>
        /// Stereo paning of the sound.
        /// </summary>
        [RangeAttribute(-1f, 1f)]
        public float StereoPan = 0f;

        /// <summary>
        /// Amount of 3D spatialisation.
        /// </summary>
        [RangeAttribute(0f, 1f)]
        public float SpatialBlend = 0f;

        /// <summary>
        /// Amount af sound sent to reverb effects.
        /// </summary>
        [RangeAttribute(0f, 1.1f)]
        public float ReverbZoneMix = 1f;

        /// <summary>
        /// Dopler level amount.
        /// </summary>
        [RangeAttribute(0f, 5f)]
        public float Sound3D_DopplerLevel = 1f;

        /// <summary>
        /// Stereo spread angle.
        /// </summary>
        [RangeAttribute(0f, 360f)]
        public float Sound3D_Spread = 0f;

        /// <summary>
        /// Volume fading over distance method.
        /// </summary>
        /// <see cref="AudioRolloffMode"/>
        public AudioRolloffMode Sound3D_VolumeRolloff = AudioRolloffMode.Logarithmic;

        /// <summary>
        /// Minimum distance from audio source to hear it.
        /// </summary>
        public float Sound3D_MinDistance = 1f;

        /// <summary>
        /// Maximum distance from audio source to hear it.
        /// </summary>
        public float Sound3D_MaxDistance = 500f;


        private double PlayStartTimeInMs = 0;
        private double PauseStartTimeInMs = 0;

        private bool Playing = false;
        private bool Paused = false;

        


        private string LastAudioClipUrl = null;

        #region Properties
        public UMI3DAsyncProperty<bool> objectMute;
        public  UMI3DAsyncProperty<bool> objectBypassEffects;
        public  UMI3DAsyncProperty<bool> objectBypassListenerEffects;
        public  UMI3DAsyncProperty<bool> objectBypassReverbZone;
        public  UMI3DAsyncProperty<bool> objectPlayOnAwake;
        public  UMI3DAsyncProperty<bool> objectLoop;
        public  UMI3DAsyncProperty<int> objectPriority;
        public  UMI3DAsyncProperty<float> objectVolume;
        public  UMI3DAsyncProperty<float> objectPitch;
        public  UMI3DAsyncProperty<float> objectStereoPan;
        public  UMI3DAsyncProperty<float> objectSpatialBlend;
        public  UMI3DAsyncProperty<float> objectReverbZoneMix;

        public  UMI3DAsyncProperty<float> objectSound3D_DopplerLevel;
        public  UMI3DAsyncProperty<float> objectSound3D_Spread;
        public  UMI3DAsyncProperty<AudioRolloffMode> objectSound3D_VolumeRolloff;
        public  UMI3DAsyncProperty<float> objectSound3D_MinDistance;
        public  UMI3DAsyncProperty<float> objectSound3D_MaxDistance;
        #endregion

        /// <summary>
        /// Initialise component.
        /// </summary>
        protected override void initDefinition()
        {
            base.initDefinition();

            objectMute = new UMI3DAsyncProperty<bool>(PropertiesHandler, Mute);
            objectMute.OnValueChanged += (bool value) => Mute = value;

            objectBypassEffects = new UMI3DAsyncProperty<bool>(PropertiesHandler, BypassEffects);
            objectBypassEffects.OnValueChanged += (bool value) => BypassEffects = value;

            objectBypassListenerEffects = new UMI3DAsyncProperty<bool>(PropertiesHandler, BypassListenerEffects);
            objectBypassListenerEffects.OnValueChanged += (bool value) => BypassListenerEffects = value;

            objectBypassReverbZone = new UMI3DAsyncProperty<bool>(PropertiesHandler, BypassReverbZone);
            objectBypassReverbZone.OnValueChanged += (bool value) => BypassReverbZone = value;

            objectPlayOnAwake = new UMI3DAsyncProperty<bool>(PropertiesHandler, PlayOnAwake);
            objectPlayOnAwake.OnValueChanged += (bool value) => PlayOnAwake = value;

            objectLoop = new UMI3DAsyncProperty<bool>(PropertiesHandler, Loop);
            objectLoop.OnValueChanged += (bool value) => Loop = value;

            objectPriority = new UMI3DAsyncProperty<int>(PropertiesHandler, Priority);
            objectPriority.OnValueChanged += (int value) => Priority = value;

            objectVolume = new UMI3DAsyncProperty<float>(PropertiesHandler, Volume);
            objectVolume.OnValueChanged += (float value) => Volume = value;

            objectPitch = new UMI3DAsyncProperty<float>(PropertiesHandler, Pitch);
            objectPitch.OnValueChanged += (float value) => Pitch = value;

            objectStereoPan = new UMI3DAsyncProperty<float>(PropertiesHandler, StereoPan);
            objectStereoPan.OnValueChanged += (float value) => StereoPan = value;

            objectSpatialBlend = new UMI3DAsyncProperty<float>(PropertiesHandler, SpatialBlend);
            objectSpatialBlend.OnValueChanged += (float value) => SpatialBlend = value;

            objectReverbZoneMix = new UMI3DAsyncProperty<float>(PropertiesHandler, ReverbZoneMix);
            objectReverbZoneMix.OnValueChanged += (float value) => ReverbZoneMix = value;

            objectSound3D_DopplerLevel = new UMI3DAsyncProperty<float>(PropertiesHandler, Sound3D_DopplerLevel);
            objectSound3D_DopplerLevel.OnValueChanged += (float value) => Sound3D_DopplerLevel = value;

            objectSound3D_Spread = new UMI3DAsyncProperty<float>(PropertiesHandler, Sound3D_Spread);
            objectSound3D_Spread.OnValueChanged += (float value) => Sound3D_Spread = value;

            objectSound3D_VolumeRolloff = new UMI3DAsyncProperty<AudioRolloffMode>(PropertiesHandler, Sound3D_VolumeRolloff);
            objectSound3D_VolumeRolloff.OnValueChanged += (AudioRolloffMode value) => Sound3D_VolumeRolloff = value;

            objectSound3D_MinDistance = new UMI3DAsyncProperty<float>(PropertiesHandler, Sound3D_MinDistance);
            objectSound3D_MinDistance.OnValueChanged += (float value) => Sound3D_MinDistance = value;

            objectSound3D_MaxDistance = new UMI3DAsyncProperty<float>(PropertiesHandler, Sound3D_MaxDistance);
            objectSound3D_MaxDistance.OnValueChanged += (float value) => Sound3D_MaxDistance = value;

            AudioClipResource.initDefinition();
            AudioClipResource.addListener(PropertiesHandler);

            AudioSource audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                gameObject.AddComponent<AudioSource>();
            }

            SyncProperties();

            Playing = false;
            Paused = false;

            if (PlayOnAwake)
                Play();
        }
        
        protected override void Update()
        {
            base.Update();

            if (Playing  &&  Pitch != 1.0f)
            {
                PlayStartTimeInMs = getTimeInMilliseconds() - ((double) GetComponent<AudioSource>().time) * 1000.0;
            }
        }

        /// <summary>
        /// Play sound.
        /// </summary>
        public void Play()
        {
            if (!Playing)
            {
                bool lastPaused = Paused;
                Playing = true;
                Paused = false;
                PropertiesHandler.NotifyUpdate();
                SyncProperties();

                AudioSource audioSource = GetComponent<AudioSource>();

                if (lastPaused != Paused)
                {
                    PlayStartTimeInMs += getTimeInMilliseconds() - PauseStartTimeInMs;
                    audioSource.UnPause();
                }
                else
                {
                    PlayStartTimeInMs = getTimeInMilliseconds();
                    audioSource.Play();
                }
            }
        }

        /// <summary>
        /// Pause sound.
        /// </summary>
        public void Pause()
        {
            if (Playing)
            {
                Playing = false;
                Paused = true;
                PropertiesHandler.NotifyUpdate();
                SyncProperties();

                AudioSource audioSource = GetComponent<AudioSource>();

                PauseStartTimeInMs = getTimeInMilliseconds();

                audioSource.Pause();
            }
        }

        /// <summary>
        /// Stop audio.
        /// </summary>
        public void Stop()
        {
            if (Playing || Paused)
            {
                Playing = false;
                Paused = false;
                PropertiesHandler.NotifyUpdate();
                SyncProperties();

                AudioSource audioSource = GetComponent<AudioSource>();

                audioSource.Stop();
            }
        }

        /// <summary>
        /// Create an empty Dto.
        /// </summary>
        /// <returns></returns>
        public override AudioSourceDto CreateDto()
        {
            return new AudioSourceDto();
        }
        
        /// <summary>
        /// Update properties.
        /// </summary>
        protected override void SyncProperties()
        {
            base.SyncProperties();

            if (inited)
            {
                //objectAudioClipUrl.SetValue(AudioClipUrl);

                AudioSource audioSource = GetComponent<AudioSource>();

                AudioClipResource.SyncProperties();
                SyncAudioSourceProp();
                SyncAudioSource(audioSource);
            }
        }

        /// <summary>
        /// Update audio source properties.
        /// </summary>
        void SyncAudioSourceProp()
        {
            objectMute.SetValue(Mute);
            objectBypassEffects.SetValue(BypassEffects);
            objectBypassListenerEffects.SetValue(BypassListenerEffects);
            objectBypassReverbZone.SetValue(BypassReverbZone);
            objectPlayOnAwake.SetValue(PlayOnAwake);
            objectLoop.SetValue(Loop);
            objectPriority.SetValue(Priority);
            objectVolume.SetValue(Volume);
            objectPitch.SetValue(Pitch);
            objectStereoPan.SetValue(StereoPan);
            objectSpatialBlend.SetValue(SpatialBlend);
            objectReverbZoneMix.SetValue(ReverbZoneMix);

            objectSound3D_DopplerLevel.SetValue(Sound3D_DopplerLevel);
            objectSound3D_Spread.SetValue(Sound3D_Spread);
            objectSound3D_VolumeRolloff.SetValue(Sound3D_VolumeRolloff);
            objectSound3D_MinDistance.SetValue(Sound3D_MinDistance);
            objectSound3D_MaxDistance.SetValue(Sound3D_MaxDistance);
        }

        /// <summary>
        /// Convert to Dto for a given user.
        /// </summary>
        /// <param name="user">User to convert for</param>
        /// <returns></returns>
        public override AudioSourceDto ToDto(UMI3DUser user)
        {
            var dto = base.ToDto(user);

            dto.AudioClipResource = AudioClipResource.ToDto(user);
            dto.Mute = objectMute.GetValue(user);
            dto.BypassEffects = objectBypassEffects.GetValue(user);
            dto.BypassListenerEffects = objectBypassListenerEffects.GetValue(user);
            dto.BypassReverbZone = objectBypassReverbZone.GetValue(user);
            dto.PlayOnAwake = objectPlayOnAwake.GetValue(user);
            dto.Loop = objectLoop.GetValue(user);
            dto.Priority = objectPriority.GetValue(user);
            dto.Volume = objectVolume.GetValue(user);
            dto.Pitch = objectPitch.GetValue(user);
            dto.StereoPan = objectStereoPan.GetValue(user);
            dto.SpatialBlend = objectSpatialBlend.GetValue(user);
            dto.ReverbZoneMix = objectReverbZoneMix.GetValue(user);

            dto.PlayStartTimeInMs = PlayStartTimeInMs.ToString();

            dto.Playing = Playing;
            dto.Paused = Paused;

            dto.Sound3D_DopplerLevel = objectSound3D_DopplerLevel.GetValue(user);
            dto.Sound3D_Spread = objectSound3D_Spread.GetValue(user);
            dto.Sound3D_VolumeRolloff = objectSound3D_VolumeRolloff.GetValue(user);
            dto.Sound3D_MinDistance = objectSound3D_MinDistance.GetValue(user);
            dto.Sound3D_MaxDistance = objectSound3D_MaxDistance.GetValue(user);

            return dto;
        }

        /// <summary>
        /// Copy this audio source into a other one.
        /// </summary>
        /// <param name="audioSource">Audio source to modify</param>
        public void SyncAudioSource(AudioSource audioSource)
        {
            if (audioSource != null)
            {
                SyncAudioSourceProperties(audioSource);

                if ((LastAudioClipUrl != null) && (LastAudioClipUrl != AudioClipResource.GetUrl()))
                {
                    SyncAudioSourceClip(audioSource);
                    LastAudioClipUrl = AudioClipResource.GetUrl();
                }
            }
        }

        /// <summary>
        /// Copy this audio source properties into a other one.
        /// </summary>
        /// <param name="audioSource">Audio source to modify</param>
        public void SyncAudioSourceProperties(AudioSource audioSource)
        {
            audioSource.mute = Mute;
            audioSource.bypassEffects = BypassEffects;
            audioSource.bypassListenerEffects = BypassListenerEffects;
            audioSource.bypassReverbZones = BypassReverbZone;
            audioSource.loop = Loop;
            audioSource.priority = Priority;
            audioSource.volume = Volume;
            audioSource.pitch = Pitch;
            audioSource.panStereo = StereoPan;
            audioSource.spatialBlend = SpatialBlend;
            audioSource.reverbZoneMix = ReverbZoneMix;

            audioSource.dopplerLevel = Sound3D_DopplerLevel;
            audioSource.spread = Sound3D_Spread;
            audioSource.rolloffMode = Sound3D_VolumeRolloff;
            audioSource.minDistance = Sound3D_MinDistance;
            audioSource.maxDistance = Sound3D_MaxDistance;

            audioSource.playOnAwake = PlayOnAwake;
        }

        /// <summary>
        /// Copy this audio source clip into a other one.
        /// </summary>
        /// <param name="audioSource">Audio source to modify</param>
        public void SyncAudioSourceClip(AudioSource audioSource)
        {            
            if (AudioClipResource.GetUrl() != null && AudioClipResource.GetUrl().Length > 0)
                StartCoroutine(LoadClip(AudioClipResource, audioSource));
            else
                audioSource.clip = null;
        }

        IEnumerator LoadClip(CVEResource resource, AudioSource audioSource)
        {
            while (!UMI3D.Ready)
                yield return new WaitForSeconds(0.1f);

            string url = resource.GetUrl();

            if (!resource.IsLocalFile)
            {
                if (url.StartsWith("http:/") && !url.StartsWith("http://"))
                {
                    url = url.Insert(6, "/");
                }
                else if (url.StartsWith("https:/") && !url.StartsWith("https://"))
                {
                    url = url.Insert(7, "/");
                }
            }
            

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
            {
                yield return www.SendWebRequest();

                if (www.isHttpError || www.isNetworkError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                    if (PlayOnAwake && Playing)
                    {
                        PlayStartTimeInMs = getTimeInMilliseconds();
                        audioSource.Play();
                        PropertiesHandler.NotifyUpdate();
                    }
                }
            }
        }

        

        double getTimeInMilliseconds()
        {
            return (DateTime.Now - DateTime.MinValue).TotalMilliseconds;
        }

    }

}