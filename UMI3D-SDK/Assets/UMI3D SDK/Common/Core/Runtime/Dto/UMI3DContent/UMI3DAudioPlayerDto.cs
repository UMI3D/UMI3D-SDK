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

namespace umi3d.common
{
    /// <summary>
    /// DTO describing an audio player, a enriched playable video resource.
    /// </summary>
    public class UMI3DAudioPlayerDto : UMI3DAbstractAnimationDto
    {
        /// <summary>
        /// Ressource containing the audio.
        /// </summary>
        public ResourceDto audioResource;

        /// <summary>
        /// Node where to diffuse the audio from.
        /// </summary>
        public ulong nodeID;

        /// <summary>
        /// Spacial Blend value.
        /// 0:not spacialized. 1:Spacialized on the node.
        /// </summary>
        public float spatialBlend = 0f;

        /// <summary>
        /// Defines which curve model is used to perform a volume attenuation.
        /// </summary>
        public AudioSourceCurveMode volumeAttenuationMode = AudioSourceCurveMode.Logarithmic;

        /// <summary>
        /// Max distance used to perform volume attenuation.
        /// </summary>
        public float volumeMaxDistance = 500f;

        /// <summary>
        /// Volume attenuation curve used if <see cref="volumeAttenuationMode"/> is set to <see cref="AudioSourceCurveMode.Custom"/>.
        /// </summary>
        public SerializableAnimationCurve volumeAttenuationCurve = new SerializableAnimationCurve();

        /// <summary>
        /// Volume value between 0 and 1.
        /// </summary>
        public float volume = 1f;

        /// <summary>
        /// Value of pitch change induced by a slowdown or speed up effect of the audio ressource. Value 1 is normal speed.
        /// </summary>
        public float pitch = 1f;
    }

    public enum AudioSourceCurveMode
    {
        Logarithmic,
        Linear,
        Custom
    }
}