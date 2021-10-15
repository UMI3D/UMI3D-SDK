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
        new public static UMI3DAudioPlayer Get(ulong id) { return UMI3DAbstractAnimation.Get(id) as UMI3DAudioPlayer; }
        public AudioSource audioSource { get; private set; }


        public UMI3DAudioPlayer(UMI3DAudioPlayerDto dto) : base(dto)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(InitPlayer(dto));
        }

        IEnumerator InitPlayer(UMI3DAudioPlayerDto dto)
        {
            var wait = new WaitForFixedUpdate();
            var gameObject = UMI3DEnvironmentLoader.Instance.gameObject;
            if (dto.nodeID != 0)
            {
                gameObject = UMI3DEnvironmentLoader.GetNode(dto.nodeID)?.gameObject;
                while (gameObject == null)
                {
                    yield return wait;
                    gameObject = UMI3DEnvironmentLoader.GetNode(dto.nodeID)?.gameObject;
                }
            }
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

            FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(dto.audioResource.variants);

            string url = fileToLoad.url;
            string ext = fileToLoad.extension;
            string authorization = fileToLoad.authorization;
            IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
            if (loader != null)
                UMI3DResourcesManager.LoadFile(
                    dto.id,
                    fileToLoad,
                    loader.UrlToObject,
                    loader.ObjectFromCache,
                    (o) =>
                    {
                        var clip = (AudioClip)o;
                        if (clip != null)
                            audioSource.clip = clip;
                        else
                            Debug.LogWarning($"invalid cast from {o.GetType()} to {typeof(AudioClip)}");
                    },
                    Debug.LogWarning,
                    loader.DeleteObject
                    );

        }

        ///<inheritdoc/>
        public override float GetProgress()
        {
            return (audioSource != null && audioSource.clip != null && audioSource.clip.length > 0) ? audioSource.time / audioSource.clip.length : -1;
        }

        ///<inheritdoc/>
        public override void Start()
        {
            audioSource?.Stop();
            audioSource?.Play();
            OnEndCoroutine = UMI3DAnimationManager.Instance.StartCoroutine(WaitUntilTheEnd(audioSource.clip.length));
        }
        Coroutine OnEndCoroutine;

        IEnumerator WaitUntilTheEnd(float time)
        {
            yield return new WaitForSeconds(time);
            OnEnd();
        }

        ///<inheritdoc/>
        public override void Stop()
        {
            audioSource?.Stop();
            if (OnEndCoroutine != null) UMI3DAnimationManager.Instance.StopCoroutine(OnEndCoroutine);
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
                    var res = ADto.audioResource;
                    ADto.audioResource = (ResourceDto)property.value;
                    if (ADto.audioResource == res) return true;
                    FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(ADto.audioResource.variants);
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
                                    Debug.LogWarning($"invalid cast from {o.GetType()} to {typeof(Texture2D)}");
                            },
                            Debug.LogWarning,
                            loader.DeleteObject
                            );
                    break;
                case UMI3DPropertyKeys.AnimationNode:
                    var clip = audioSource?.clip;
                    var g = audioSource.gameObject;
                    GameObject.Destroy(audioSource);
                    ADto.nodeID = (ulong)(long)property.value;
                    g = UMI3DEnvironmentLoader.Instance.gameObject;
                    if (ADto.nodeID != 0)
                    {
                        g = UMI3DEnvironmentLoader.GetNode(ADto.nodeID).gameObject;
                    }
                    audioSource = g.AddComponent<AudioSource>();
                    audioSource.clip = clip;
                    audioSource.playOnAwake = false;
                    audioSource.pitch = ADto.pitch;
                    audioSource.volume = ADto.volume;
                    audioSource.spatialBlend = ADto.spatialBlend;
                    break;
                default:
                    return false;
            }
            return true;

        }

        static public bool ReadMyUMI3DProperty(ref object value, uint propertyKey, ByteContainer container) { return false; }

        ///<inheritdoc/>
        public override void Start(float atTime)
        {
            audioSource?.Stop();
            if (audioSource)
                audioSource.time = atTime;
            audioSource?.Play();
            OnEndCoroutine = UMI3DAnimationManager.Instance.StartCoroutine(WaitUntilTheEnd(audioSource.clip.length));

        }

        public override void SetProgress(long frame)
        {
            audioSource.timeSamples = (int)frame;

        }
    }
}