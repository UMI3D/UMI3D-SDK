﻿/*
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

using System.Collections;
using umi3d.common;
using UnityEngine;
using UnityEngine.Video;

namespace umi3d.cdk
{
    /// <summary>
    /// Video player, enriched playable video resource.
    /// </summary>
    public class UMI3DVideoPlayer : UMI3DAbstractAnimation
    {
        UMI3DVersion.VersionCompatibility _version = new UMI3DVersion.VersionCompatibility("2.6", "*");
        public override UMI3DVersion.VersionCompatibility version => _version;

        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Animation;

        private VideoPlayer videoPlayer;
        private Material mat;
        private RenderTexture renderTexture;

        /// <summary>
        /// Get an <see cref="UMI3DVideoPlayer"/> by id.
        /// </summary>
        /// <param name="id">UMI3D id</param>
        /// <returns></returns>
        public static new UMI3DVideoPlayer Get(ulong id) { return UMI3DAbstractAnimation.Get(id) as UMI3DVideoPlayer; }

        /// <summary>
        /// Has the VideoPlayer successfully prepared the content to be played ?
        /// </summary>
        public bool isPrepared => videoPlayer?.isPrepared ?? false;

        bool readyOrFailed => isPrepared || preparationFailed;

        /// <summary>
        /// Has the preparation of the video content failed?
        /// </summary>
        public bool preparationFailed { get; private set; } = false;

        /// <inheritdoc/>
        public override bool IsPlaying() => videoPlayer.isPlaying;

        public UMI3DVideoPlayer(UMI3DVideoPlayerDto dto) : base(dto)
        { 
        }

        /// <inheritdoc/>
        public override void Init()
        {
            var dto = this.dto as UMI3DVideoPlayerDto;

            //init material
            renderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.RGB565);
            renderTexture.Create();
            renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;

            mat = UMI3DEnvironmentLoader.GetEntity(dto.materialId).Object as Material;
            if (mat == null)
            {
                UMI3DLogger.LogWarning("Material not found to display video", scope);
                return;
            }
            mat.DisableKeyword("_DISABLE_ALBEDO_MAP");
            mat.mainTexture = renderTexture;

            // create unity VideoPlayer
            var videoPlayerGameObject = new GameObject("video");
            videoPlayerGameObject.transform.SetParent(UMI3DResourcesManager.Instance.transform);
            videoPlayer = videoPlayerGameObject.AddComponent<VideoPlayer>();

            // setup video player
            FileDto fileDto = UMI3DEnvironmentLoader.Parameters.ChooseVariant(dto.videoResource.variants);
            if (!UMI3DClientServer.Instance.AuthorizationInHeader)
                videoPlayer.url = UMI3DResourcesManager.Instance.SetAuthorisationWithParameter(fileDto.url, UMI3DClientServer.getAuthorization());
            else
                videoPlayer.url = fileDto.url;

            videoPlayer.targetTexture = renderTexture;

            videoPlayer.source = VideoSource.Url;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.playOnAwake = false;
            videoPlayer.skipOnDrop = true;
            videoPlayer.waitForFirstFrame = false;
            videoPlayer.isLooping = dto.looping;
            videoPlayer.Prepare();
            videoPlayer.errorReceived += VideoPlayer_errorReceived;



            if (dto.playing)
            {
                UMI3DAnimationManager.StartCoroutine(StartAfterLoading());
            }
            else
            {
                videoPlayer.Pause(); // Don't call Stop() because it cancel videoPlayer.Prepare()

                UMI3DAnimationManager.StartCoroutine(SetTime(dto.pauseTime));
            }

            //audio
            if (dto.audioId != 0)
            {
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.audioId, (e) =>
                {
                    UMI3DAnimationManager.StartCoroutine(SetAudioSource(dto, e));
                });
            }
        }

        public void Clean()
        {
            videoPlayer.Stop();
        }

        private async void VideoPlayer_errorReceived(VideoPlayer source, string message)
        {
            await UMI3DAsyncManager.Delay(100);
            preparationFailed = true;
        }

        /// <summary>
        /// Coroutine to set <see cref="videoPlayer"/> audioSource.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        private IEnumerator SetAudioSource(UMI3DVideoPlayerDto dto, UMI3DEntityInstance entity)
        {
            yield return null;
            videoPlayer.Stop();
            videoPlayer.SetTargetAudioSource(0, ((UMI3DAudioPlayer)entity.Object).audioSource);

            videoPlayer.Prepare();

            while (!readyOrFailed)
                yield return null;

            if (dto.playing)
            {
                if (dto.startTime == default)
                {
                    videoPlayer.Play();
                }
                else
                {
                    Start(UMI3DClientServer.Instance.GetTime() - dto.startTime);
                }
            }
        }

        private IEnumerator StartAfterLoading()
        {
            while (!readyOrFailed)
            {
                yield return new WaitForEndOfFrame();
            }

            if (dto.playing)
            {
                ulong now = UMI3DClientServer.Instance.GetTime();
                Start(now - dto.startTime);
            }
        }

        private IEnumerator SetTime(long time)
        {
            dto.pauseTime = time;
            yield return SetTime();
        }

        private IEnumerator SetTime()
        {
            while (videoPlayer == null)
            {
                yield return null;
            }

            yield return null;
            yield return null;

            while (!readyOrFailed)
            {
                yield return new WaitForEndOfFrame();
            }

            yield return null;
            yield return null;

            videoPlayer.Play();

            if (!dto.playing)
            {
                if (dto.pauseTime > 0)
                {
                    float time = dto.pauseTime / 1000f;
                    videoPlayer.time = time;
#if UNITY_ANDROID
                    yield return MakeSureTimeIsCorrectltySet(time);
#endif
                }
                else
                    videoPlayer.frame = 3;

                videoPlayer.Pause();
            }
        }

        /// <summary>
        /// Fix added for some issues encountered on Android (<see cref="VideoPlayer.time"/> not necessarily correctly set).
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private IEnumerator MakeSureTimeIsCorrectltySet(float time)
        {
            yield return null;
            yield return null;

            if (videoPlayer.frame < 0 || videoPlayer.time < 0)
            {
                videoPlayer.Stop();

                yield return new WaitForEndOfFrame();

                if ((dto as UMI3DVideoPlayerDto).audioId != 0)
                {
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                    UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded((dto as UMI3DVideoPlayerDto).audioId, (e) =>
                    {
                        UMI3DAnimationManager.StartCoroutine(ReSetAudioSource(dto as UMI3DVideoPlayerDto, e, time));
                    });
                }
                else
                {
                    videoPlayer.Play();
                    videoPlayer.time = time;

                    if (!dto.playing)
                    {
                        videoPlayer.Pause();
                    }
                }
            }
        }

        private IEnumerator ReSetAudioSource(UMI3DVideoPlayerDto dto, UMI3DEntityInstance entity, float time)
        {
            yield return null;

            videoPlayer.Stop();
            videoPlayer.SetTargetAudioSource(0, ((UMI3DAudioPlayer)entity.Object).audioSource);

            videoPlayer.Play();
            videoPlayer.time = time;
            if (!dto.playing)
            {
                videoPlayer.Pause();
            }
        }

        /// <inheritdoc/>
        public override float GetProgress()
        {
            float res = 0;
            if (videoPlayer != null)
                res = (float)videoPlayer.time;
            return res;
        }

        /// <inheritdoc/>
        public override void Start()
        {
            Start(0);
        }

        /// <inheritdoc/>
        public override void Stop()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Pause();
            }
        }

        /// <summary>
        /// When set to true, makes the video loop.
        /// </summary>
        /// <param name="b"></param>
        public void SetLoopValue(bool b)
        {
            videoPlayer.isLooping = b;
        }

        /// <inheritdoc/>
        public override void Start(float atTime)
        {
            if (videoPlayer != null)
            {
                if (readyOrFailed)
                {
                    videoPlayer.frame = (int)Mathf.Max(0f, atTime * videoPlayer.frameRate / 1000);
                    videoPlayer.Play();
                }
                else
                {
                    videoPlayer.Prepare();
                    MainThreadDispatcher.UnityMainThreadDispatcher.Instance().StartCoroutine(StartAfterLoading());

                }
            }
        }

        /// <inheritdoc/>
        public override void SetProgress(long frame)
        {
            UMI3DAnimationManager.StartCoroutine(SetTime());
        }
    }
}