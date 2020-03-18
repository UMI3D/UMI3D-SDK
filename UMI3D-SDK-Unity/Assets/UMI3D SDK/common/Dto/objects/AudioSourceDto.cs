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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d.common
{
    [System.Serializable]
    public class AudioSourceDto : EmptyObject3DDto
    {

        public ResourceDto AudioClipResource = null;
        public bool Mute = false;
        public bool BypassEffects = false;
        public bool BypassListenerEffects = false;
        public bool BypassReverbZone = false;
        public bool PlayOnAwake = true;
        public bool Loop = false;
        public int Priority = 128;
        public float Volume = 1f;
        public float Pitch = 1f;
        public float StereoPan = 0f;
        public float SpatialBlend = 0f;
        public float ReverbZoneMix = 1f;

        public string PlayStartTimeInMs = "";

        public bool Playing = false;
        public bool Paused = false;
        
        public float Sound3D_DopplerLevel = 1f;
        public float Sound3D_Spread = 0f;
        public AudioRolloffMode Sound3D_VolumeRolloff = AudioRolloffMode.Logarithmic;
        public float Sound3D_MinDistance = 1f;
        public float Sound3D_MaxDistance = 500f;

        public AudioSourceDto() : base() { }

    }
}
