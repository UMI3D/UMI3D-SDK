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

using System.Collections;
using umi3d.common;
using UnityEngine;
using UnityEngine.Video;

namespace umi3d.cdk
{
    public class UMI3DVideoPlayer : UMI3DAbstractAnimation
    {
        const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Animation;

        private readonly VideoPlayer videoPlayer;
        private readonly Material mat;
        private readonly RenderTexture renderTexture;

        public static new UMI3DVideoPlayer Get(ulong id) { return UMI3DAbstractAnimation.Get(id) as UMI3DVideoPlayer; }

        public UMI3DVideoPlayer(UMI3DVideoPlayerDto dto) : base(dto)
        {
            //init material
            renderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.RGB565);
            renderTexture.Create();
            renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            mat = UMI3DEnvironmentLoader.GetEntity(dto.materialId).Object as Material;
            if (mat == null)
            {
                UMI3DLogger.LogWarning("Material not found to display video",scope);
                return;
            }
            mat.DisableKeyword("_DISABLE_ALBEDO_MAP");
            mat.mainTexture = renderTexture;

            // create unity VideoPlayer
            var videoPlayerGameObject = new GameObject("video");
            videoPlayerGameObject.transform.SetParent(UMI3DResourcesManager.Instance.transform);
            videoPlayer = videoPlayerGameObject.AddComponent<VideoPlayer>();
            videoPlayer.url = UMI3DEnvironmentLoader.Parameters.ChooseVariante(dto.videoResource.variants).url;
            videoPlayer.targetTexture = renderTexture;

            videoPlayer.source = VideoSource.Url;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.playOnAwake = dto.playing;
            videoPlayer.skipOnDrop = true;
            videoPlayer.waitForFirstFrame = false;
            videoPlayer.isLooping = dto.looping;
            //videoPlayer.prepareCompleted += (v) => UMI3DLogger.LogWarning("PREPARED !");
            videoPlayer.Prepare();


            if (dto.playing)
            {
                UMI3DAnimationManager.Instance.StartCoroutine(StartAfterLoading());
            }
            else
            {
                videoPlayer.Pause(); // Don't call Stop() because it cancel videoPlayer.Prepare()

                UMI3DAnimationManager.Instance.StartCoroutine(SetTime(dto.pauseTime));
            }

            //audio
            if (dto.audioId != 0)
            {
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.audioId, (e) =>
                {
                    videoPlayer.SetTargetAudioSource(0, ((UMI3DAudioPlayer)e.Object).audioSource);
                });
            }
        }

        private IEnumerator StartAfterLoading()
        {
            while (!videoPlayer.isPrepared)
            {
                yield return new WaitForEndOfFrame();
            }

            if (dto.playing)
            {
                ulong now = UMI3DClientServer.Instance.GetTime();
                Start((float)(now - dto.startTime));
            }
        }

        private IEnumerator SetTime(long time)
        {
            dto.pauseTime = time;
            yield return SetTime();
        }

        private IEnumerator SetTime()
        {
            while (!videoPlayer.isPrepared)
            {
                yield return new WaitForEndOfFrame();
            }
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
            }
        }

        private IEnumerator MakeSureTimeIsCorrectltySet(float time)
        {
            yield return new WaitForEndOfFrame();

            if (videoPlayer.frame < 0 || videoPlayer.time < 0)
            {
                videoPlayer.Stop();

                yield return new WaitForEndOfFrame();

                if ((dto as UMI3DVideoPlayerDto).audioId != 0)
                {
                    videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                    UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded((dto as UMI3DVideoPlayerDto).audioId, (e) =>
                    {
                        videoPlayer.SetTargetAudioSource(0, ((UMI3DAudioPlayer)e.Object).audioSource);
                    });
                }

                videoPlayer.Play();
                videoPlayer.time = time;

                if (!dto.playing)
                {
                    videoPlayer.Pause();
                }
            }
        }

        ///<inheritdoc/>
        public override float GetProgress()
        {
            float res = 0;
            if (videoPlayer != null)
                res = (float)videoPlayer.time;
            return res;
        }

        ///<inheritdoc/>
        public override void Start()
        {
            Start(0);
        }

        ///<inheritdoc/>
        public override void Stop()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Pause();
            }
        }

        public void SetLoopValue(bool b)
        {
            videoPlayer.isLooping = b;
        }

        ///<inheritdoc/>
        public override void Start(float atTime)
        {
            if (videoPlayer != null)
            {
                if (videoPlayer.isPrepared)
                {
                    videoPlayer.frame = (int)(Mathf.Max(0f, atTime * videoPlayer.frameRate / 1000));
                    videoPlayer.Play();
                }
                else
                {
                    videoPlayer.Prepare();
                    MainThreadDispatcher.UnityMainThreadDispatcher.Instance().StartCoroutine(StartAfterLoading());

                }
            }
        }

        public override void SetProgress(long frame)
        {

            UMI3DAnimationManager.Instance.StartCoroutine(SetTime());
        }
    }
}