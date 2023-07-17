/*
Copyright 2019 - 2023 Inetum

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

using inetum.unityUtils;
using MainThreadDispatcher;
using System.Collections;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Animation that plays sound.
    /// </summary>
    public class UMI3DAudioPlayer : UMI3DAbstractAnimation
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Animation;

        /// <summary>
        /// Get an <see cref="UMI3DAudioPlayer"/> by id.
        /// </summary>
        /// <param name="id">UMI3D id</param>
        /// <returns></returns>
        public static new UMI3DAudioPlayer Get(ulong id) { return UMI3DAbstractAnimation.Get(id) as UMI3DAudioPlayer; }
        /// <summary>
        /// Audio source in Unity.
        /// </summary>
        public AudioSource audioSource { get; private set; }


        public UMI3DAudioPlayer(UMI3DAudioPlayerDto dto) : base(dto)
        {
        }

        /// <inheritdoc/>
        public override bool IsPlaying() => audioSource.isPlaying;

        /// <inheritdoc/>
        public override void Init()
        {
            InitPlayer(dto as UMI3DAudioPlayerDto);
        }

        private void InitPlayer(UMI3DAudioPlayerDto dto)
        {
            if (dto.nodeID == 0)
                UnityMainThreadDispatcher.Instance().Enqueue(() => _InitPlayer(dto, UMI3DEnvironmentLoader.Instance.gameObject));
            else
                UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.nodeID, e => UnityMainThreadDispatcher.Instance().Enqueue(() => _InitPlayer(dto, (e as UMI3DNodeInstance).gameObject)));
        }

        private async void _InitPlayer(UMI3DAudioPlayerDto dto, GameObject gameObject)
        {
            audioSource = gameObject.GetOrAddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.pitch = dto.pitch;
            audioSource.volume = dto.volume;
            audioSource.spatialBlend = dto.spatialBlend;
            audioSource.maxDistance = dto.volumeMaxDistance;
            SetVolumeAttenuationMode(dto.volumeAttenuationMode);

            if (dto.volumeAttenuationCurve.keys.Count > 0)
                audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, dto.volumeAttenuationCurve.Struct());

            if (dto.audioResource == null || dto.audioResource.variants == null || dto.audioResource.variants.Count < 1)
            {
                dto.audioResource = null;
                return;
            }

            FileDto fileToLoad = UMI3DEnvironmentLoader.AbstractParameters.ChooseVariant(dto.audioResource.variants);

            string url = fileToLoad.url;
            string ext = fileToLoad.extension;
            string authorization = fileToLoad.authorization;
            IResourcesLoader loader = UMI3DEnvironmentLoader.AbstractParameters.SelectLoader(ext);
            if (loader != null)
            {
                var o = await UMI3DResourcesManager.LoadFile(dto.id, fileToLoad, loader);
                var clip = (AudioClip)o;
                if (clip != null)
                {
                    audioSource.clip = clip;

                    if (dto.playing)
                    {
                        if (dto.startTime == default)
                            Start();
                        else
                            Start(UMI3DClientServer.Instance.GetTime() - dto.startTime);
                    }
                }
                else
                {
                    UMI3DLogger.LogWarning($"invalid cast from {o.GetType()} to {typeof(AudioClip)}", scope);
                }
            }
        }

        /// <inheritdoc/>
        public override float GetProgress()
        {
            return (audioSource != null && audioSource.clip != null && audioSource.clip.length > 0) ? audioSource.time : -1;
        }

        /// <inheritdoc/>
        public override void Start()
        {
            Start(0);
        }

        private Coroutine OnEndCoroutine;

        private IEnumerator WaitUntilTheEnd(float time)
        {
            yield return new WaitForSeconds(time);
            OnEnd();
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            try
            {
                audioSource?.Stop();
            }
            catch { }

            if (OnEndCoroutine != null) UMI3DAnimationManager.StopCoroutine(OnEndCoroutine);
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData value)
        {
            if (await base.SetUMI3DProperty(value)) return true;
            var ADto = dto as UMI3DAudioPlayerDto;
            if (ADto == null) return false;

            switch (value.property.property)
            {
                case UMI3DPropertyKeys.AnimationVolume:
                    audioSource.volume = ADto.volume = (float)(double)value.property.value;
                    break;
                case UMI3DPropertyKeys.AnimationPitch:
                    audioSource.pitch = ADto.pitch = (float)(double)value.property.value;
                    break;
                case UMI3DPropertyKeys.AnimationSpacialBlend:
                    audioSource.spatialBlend = ADto.spatialBlend = (float)(double)value.property.value;
                    break;
                case UMI3DPropertyKeys.VolumeMaxDistance:
                    audioSource.maxDistance = ADto.spatialBlend = Mathf.Max((float)(double)value.property.value, 0);
                    break;
                case UMI3DPropertyKeys.VolumeAttenuationMode:
                    var mode = (AudioSourceCurveMode)value.property.value;
                    ADto.volumeAttenuationMode = mode;
                    SetVolumeAttenuationMode(mode);
                    break;
                case UMI3DPropertyKeys.VolumeCustomCurve:
                    if (audioSource.rolloffMode != AudioRolloffMode.Custom)
                        UMI3DLogger.LogWarning("Custom volume curve will not be used because audio source volume attenuation mode is not set to custom.", scope);

                    var curve = (AnimationCurveDto)value.property.value;
                    if (curve.keys.Count > 0)
                        audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve.Struct());
                    ADto.volumeAttenuationCurve = curve;
                    break;
                case UMI3DPropertyKeys.AnimationResource:
                    ResourceDto res = ADto.audioResource;
                    ADto.audioResource = (ResourceDto)value.property.value;
                    if (ADto.audioResource == res) return true;
                    FileDto fileToLoad = UMI3DEnvironmentLoader.AbstractParameters.ChooseVariant(ADto.audioResource.variants);
                    if (ADto.audioResource == null || ADto.audioResource.variants == null || ADto.audioResource.variants.Count < 1)
                    {
                        ADto.audioResource = null;
                        return true;
                    }
                    LoadClip(fileToLoad, ADto);
                    break;
                case UMI3DPropertyKeys.AnimationNode:
                    AudioClip clip = audioSource?.clip;
                    GameObject g = audioSource.gameObject;
                    GameObject.Destroy(audioSource);
                    audioSource = null;
                    ADto.nodeID = (ulong)(long)value.property.value;
                    InitPlayer(ADto);
                    break;
                default:
                    return false;
            }
            return true;

        }

        async void LoadClip(FileDto fileToLoad, UMI3DAudioPlayerDto ADto)
        {
            string ext = fileToLoad.extension;
            IResourcesLoader loader = UMI3DEnvironmentLoader.AbstractParameters.SelectLoader(ext);
            if (loader != null)
            {
                var o = await UMI3DResourcesManager.LoadFile(ADto.id, fileToLoad, loader);
                var clipa = (AudioClip)o;
                if (clipa != null)
                    audioSource.clip = clipa;
                else
                    UMI3DLogger.LogWarning($"invalid cast from {o.GetType()} to {typeof(AudioClip)}", scope);
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData value)
        {
            if (await base.SetUMI3DProperty(value)) return true;
            var ADto = value.entity.dto as UMI3DAudioPlayerDto;

            switch (value.propertyKey)
            {
                case UMI3DPropertyKeys.AnimationVolume:
                    audioSource.volume = ADto.volume = UMI3DSerializer.Read<float>(value.container);
                    break;
                case UMI3DPropertyKeys.AnimationPitch:
                    audioSource.pitch = ADto.pitch = UMI3DSerializer.Read<float>(value.container);
                    break;
                case UMI3DPropertyKeys.AnimationSpacialBlend:
                    audioSource.spatialBlend = ADto.spatialBlend = UMI3DSerializer.Read<float>(value.container);
                    break;
                case UMI3DPropertyKeys.VolumeMaxDistance:
                    audioSource.maxDistance = ADto.spatialBlend = Mathf.Max(UMI3DSerializer.Read<float>(value.container), 0);
                    break;
                case UMI3DPropertyKeys.VolumeAttenuationMode:
                    var mode = (AudioSourceCurveMode)UMI3DSerializer.Read<int>(value.container);
                    ADto.volumeAttenuationMode = mode;
                    SetVolumeAttenuationMode(mode);
                    break;
                case UMI3DPropertyKeys.VolumeCustomCurve:
                    if (audioSource.rolloffMode != AudioRolloffMode.Custom)
                        UMI3DLogger.LogWarning("Custom volume curve will not be used because audio source volume attenuation mode is not set to custom.", scope);

                    var curve = UMI3DSerializer.Read<AnimationCurveDto>(value.container);
                    if (curve.keys.Count > 0)
                        audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, curve.Struct());

                    ADto.volumeAttenuationCurve = curve;
                    break;
                case UMI3DPropertyKeys.AnimationResource:
                    ResourceDto res = ADto.audioResource;
                    ADto.audioResource = UMI3DSerializer.Read<ResourceDto>(value.container);
                    if (ADto.audioResource == res) return true;
                    FileDto fileToLoad = UMI3DEnvironmentLoader.AbstractParameters.ChooseVariant(ADto.audioResource.variants);
                    if (ADto.audioResource == null || ADto.audioResource.variants == null || ADto.audioResource.variants.Count < 1)
                    {
                        ADto.audioResource = null;
                        return true;
                    }
                    LoadClip(fileToLoad, ADto);

                    break;
                case UMI3DPropertyKeys.AnimationNode:
                    AudioClip clip = audioSource?.clip;
                    GameObject g = audioSource.gameObject;
                    GameObject.Destroy(audioSource);
                    audioSource = null;
                    ADto.nodeID = UMI3DSerializer.Read<ulong>(value.container);
                    InitPlayer(ADto);
                    break;
                default:
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public static Task<bool> ReadMyUMI3DProperty(ReadUMI3DPropertyData value) { return Task.FromResult(false); }

        /// <inheritdoc/>
        public override void Start(float atTime)
        {
            atTime = atTime / 1000f; //Convert to seconds

            if (audioSource != null)
            {
                if (audioSource.clip != null)
                {
                    if (dto.looping)
                        atTime = atTime % audioSource.clip.length;

                    audioSource.Stop();

                    if (atTime <= audioSource.clip.length && atTime >= 0)
                    {
                        audioSource.time = atTime;
                        audioSource.Play();
                    }

                    OnEndCoroutine = UMI3DAnimationManager.StartCoroutine(WaitUntilTheEnd(audioSource.clip.length - atTime));
                }
                else
                {
                    UnityMainThreadDispatcher.Instance().StartCoroutine(StartAfterLoading());
                }
            }
        }

        /// <inheritdoc/>
        public override void SetProgress(long frame)
        {
            audioSource.time = frame / 1000f;

        }

        private IEnumerator StartAfterLoading()
        {
            while (!audioSource.clip)
            {
                yield return new WaitForEndOfFrame();
            }

            if (dto.playing)
            {
                ulong now = UMI3DClientServer.Instance.GetTime();
                Start(now - dto.startTime);
            }
        }

        /// <summary>
        /// Setter for <see cref="audioSource.rolloffMode"/>.
        /// </summary>
        /// <param name="modeDto"></param>
        /// <returns></returns>
        private AudioRolloffMode SetVolumeAttenuationMode(AudioSourceCurveMode modeDto)
        {
            AudioRolloffMode mode;
            switch (modeDto)
            {
                case AudioSourceCurveMode.Logarithmic:
                    mode = AudioRolloffMode.Logarithmic;
                    break;
                case AudioSourceCurveMode.Linear:
                    mode = AudioRolloffMode.Linear;
                    break;
                case AudioSourceCurveMode.Custom:
                    mode = AudioRolloffMode.Custom;
                    break;
                default:
                    mode = AudioRolloffMode.Logarithmic;
                    UMI3DLogger.LogError("Audio source, volume attenuation mode not recognized : " + modeDto, scope);
                    break;
            }
            audioSource.rolloffMode = mode;

            return mode;
        }
    }
}