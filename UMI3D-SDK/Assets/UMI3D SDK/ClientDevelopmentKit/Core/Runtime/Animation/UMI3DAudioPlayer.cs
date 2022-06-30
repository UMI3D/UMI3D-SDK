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

using MainThreadDispatcher;
using System.Collections;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class UMI3DAudioPlayer : UMI3DAbstractAnimation
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Animation;

        public static new UMI3DAudioPlayer Get(ulong id) { return UMI3DAbstractAnimation.Get(id) as UMI3DAudioPlayer; }
        public AudioSource audioSource { get; private set; }


        public UMI3DAudioPlayer(UMI3DAudioPlayerDto dto) : base(dto)
        {
            InitPlayer(dto);
        }

        private void InitPlayer(UMI3DAudioPlayerDto dto)
        {
            if (dto.nodeID == 0)
                UnityMainThreadDispatcher.Instance().Enqueue(_InitPlayer(dto, UMI3DEnvironmentLoader.Instance.gameObject));
            else
                UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.nodeID, e => UnityMainThreadDispatcher.Instance().Enqueue(_InitPlayer(dto, (e as UMI3DNodeInstance).gameObject)));
        }
        private IEnumerator _InitPlayer(UMI3DAudioPlayerDto dto, GameObject gameObject)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.pitch = dto.pitch;
            audioSource.volume = dto.volume;
            audioSource.spatialBlend = dto.spatialBlend;

            if (dto.audioResource == null || dto.audioResource.variants == null || dto.audioResource.variants.Count < 1)
            {
                dto.audioResource = null;
                yield break;
            }

            FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariant(dto.audioResource.variants);

            string url = fileToLoad.url;
            string ext = fileToLoad.extension;
            string authorization = fileToLoad.authorization;
            IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
            if (loader != null)
            {
                UMI3DResourcesManager.LoadFile(
                    dto.id,
                    fileToLoad,
                    loader.UrlToObject,
                    loader.ObjectFromCache,
                    (o) =>
                    {
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
                    },
                     e => UMI3DLogger.LogWarning(e, scope),
                    loader.DeleteObject
                    );
            }
        }




        ///<inheritdoc/>
        public override float GetProgress()
        {
            return (audioSource != null && audioSource.clip != null && audioSource.clip.length > 0) ? audioSource.time : -1;
        }

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public override void Stop()
        {
            audioSource?.Stop();
            if (OnEndCoroutine != null) UMI3DAnimationManager.StopCoroutine(OnEndCoroutine);
        }

        ///<inheritdoc/>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base.SetUMI3DProperty(entity, property)) return true;
            var ADto = dto as UMI3DAudioPlayerDto;
            if (ADto == null) return false;

            switch (property.property)
            {
                case UMI3DPropertyKeys.AnimationVolume:
                    audioSource.volume = ADto.volume = (float)(double)property.value;
                    break;
                case UMI3DPropertyKeys.AnimationPitch:
                    audioSource.pitch = ADto.pitch = (float)(double)property.value;
                    break;
                case UMI3DPropertyKeys.AnimationSpacialBlend:
                    audioSource.spatialBlend = ADto.spatialBlend = (float)(double)property.value;
                    break;
                case UMI3DPropertyKeys.AnimationResource:
                    ResourceDto res = ADto.audioResource;
                    ADto.audioResource = (ResourceDto)property.value;
                    if (ADto.audioResource == res) return true;
                    FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariant(ADto.audioResource.variants);
                    if (ADto.audioResource == null || ADto.audioResource.variants == null || ADto.audioResource.variants.Count < 1)
                    {
                        ADto.audioResource = null;
                        return true;
                    }
                    string url = fileToLoad.url;
                    string ext = fileToLoad.extension;
                    string authorization = fileToLoad.authorization;
                    IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
                    if (loader != null)
                    {
                        UMI3DResourcesManager.LoadFile(
                            ADto.id,
                            fileToLoad,
                            loader.UrlToObject,
                            loader.ObjectFromCache,
                            (o) =>
                            {
                                var clipa = (AudioClip)o;
                                if (clipa != null)
                                    audioSource.clip = clipa;
                                else
                                    UMI3DLogger.LogWarning($"invalid cast from {o.GetType()} to {typeof(AudioClip)}", scope);
                            },
                            e => UMI3DLogger.LogWarning(e, scope),
                            loader.DeleteObject
                            );
                    }

                    break;
                case UMI3DPropertyKeys.AnimationNode:
                    AudioClip clip = audioSource?.clip;
                    GameObject g = audioSource.gameObject;
                    GameObject.Destroy(audioSource);
                    audioSource = null;
                    ADto.nodeID = (ulong)(long)property.value;
                    InitPlayer(ADto);
                    break;
                default:
                    return false;
            }
            return true;

        }

        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (base.SetUMI3DProperty(entity, operationId, propertyKey, container)) return true;
            var ADto = entity.dto as UMI3DAudioPlayerDto;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.AnimationVolume:
                    audioSource.volume = ADto.volume = UMI3DNetworkingHelper.Read<float>(container);
                    break;
                case UMI3DPropertyKeys.AnimationPitch:
                    audioSource.pitch = ADto.pitch = UMI3DNetworkingHelper.Read<float>(container);
                    break;
                case UMI3DPropertyKeys.AnimationSpacialBlend:
                    audioSource.spatialBlend = ADto.spatialBlend = UMI3DNetworkingHelper.Read<float>(container);
                    break;
                case UMI3DPropertyKeys.AnimationResource:
                    ResourceDto res = ADto.audioResource;
                    ADto.audioResource = (ResourceDto)UMI3DNetworkingHelper.Read<ResourceDto>(container);
                    if (ADto.audioResource == res) return true;
                    FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariant(ADto.audioResource.variants);
                    if (ADto.audioResource == null || ADto.audioResource.variants == null || ADto.audioResource.variants.Count < 1)
                    {
                        ADto.audioResource = null;
                        return true;
                    }
                    string url = fileToLoad.url;
                    string ext = fileToLoad.extension;
                    string authorization = fileToLoad.authorization;
                    IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
                    if (loader != null)
                    {
                        UMI3DResourcesManager.LoadFile(
                            ADto.id,
                            fileToLoad,
                            loader.UrlToObject,
                            loader.ObjectFromCache,
                            (o) =>
                            {
                                var clipa = (AudioClip)o;
                                if (clipa != null)
                                    audioSource.clip = clipa;
                                else
                                    UMI3DLogger.LogWarning($"invalid cast from {o.GetType()} to {typeof(Texture2D)}", scope);
                            },
                            e => UMI3DLogger.LogWarning(e, scope),
                            loader.DeleteObject
                            );
                    }

                    break;
                case UMI3DPropertyKeys.AnimationNode:
                    AudioClip clip = audioSource?.clip;
                    GameObject g = audioSource.gameObject;
                    GameObject.Destroy(audioSource);
                    audioSource = null;
                    ADto.nodeID = UMI3DNetworkingHelper.Read<ulong>(container);
                    InitPlayer(ADto);
                    break;
                default:
                    return false;
            }
            return true;
        }

        public static bool ReadMyUMI3DProperty(ref object value, uint propertyKey, ByteContainer container) { return false; }

        ///<inheritdoc/>
        public override void Start(float atTime)
        {
            if ((audioSource != null))
            {
                if ((audioSource.clip != null))
                {
                    audioSource.Stop();
                    if (atTime != 0)
                        audioSource.time = atTime;
                    audioSource.Play();
                    OnEndCoroutine = UMI3DAnimationManager.StartCoroutine(WaitUntilTheEnd(audioSource.clip.length));
                }
                else
                {
                    MainThreadDispatcher.UnityMainThreadDispatcher.Instance().StartCoroutine(StartAfterLoading());
                }
            }
        }

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
                Start((float)(now - dto.startTime));
            }
        }
    }
}