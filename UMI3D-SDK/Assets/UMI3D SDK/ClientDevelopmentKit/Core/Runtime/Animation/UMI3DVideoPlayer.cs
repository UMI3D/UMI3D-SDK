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
        VideoPlayer videoPlayer;
        Material mat;
        RenderTexture renderTexture;

        new public static UMI3DVideoPlayer Get(ulong id) { return UMI3DAbstractAnimation.Get(id) as UMI3DVideoPlayer; }

        public UMI3DVideoPlayer(UMI3DVideoPlayerDto dto) : base(dto)
        {
            //init material
            renderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.RGB565);
            renderTexture.Create();
            renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            mat = UMI3DEnvironmentLoader.GetEntity(dto.materialId).Object as Material;
            if (mat == null)
            {
                Debug.LogWarning("Material not found to display video");
                return;
            }
            mat.DisableKeyword("_DISABLE_ALBEDO_MAP");
            mat.mainTexture = renderTexture;

            // create unity VideoPlayer
            GameObject videoPlayerGameObject = new GameObject("video");
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
            //videoPlayer.prepareCompleted += (v) => Debug.LogWarning("PREPARED !");
            videoPlayer.Prepare();


            if (dto.playing)
            {
                UMI3DAnimationManager.Instance.StartCoroutine(StartAfterLoading());
            }
            else
            {
                videoPlayer.Pause(); // Don't call Stop() because it cancel videoPlayer.Prepare()

                UMI3DAnimationManager.Instance.StartCoroutine(SetFrame(dto.pauseFrame));
            }

            //audio
            if (dto.audioId != 0)
            {
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                UMI3DAnimationManager.Instance.StartCoroutine(SetAudioSource(dto.audioId));
            }
        }

        private IEnumerator SetAudioSource(ulong audioId)
        {
            var delay = new WaitForSeconds(1f);
            while (UMI3DEnvironmentLoader.GetEntity(audioId) == null)
            {
                yield return delay;
            }

            videoPlayer.SetTargetAudioSource(0, ((UMI3DAudioPlayer)UMI3DEnvironmentLoader.GetEntity(audioId).Object).audioSource);
        }

        private IEnumerator StartAfterLoading()
        {
            while (!videoPlayer.isPrepared)
            {
                yield return new WaitForEndOfFrame();
            }
            ulong now = UMI3DClientServer.Instance.GetTime();
            Start((float)(now - dto.startTime));

        }

        private IEnumerator SetFrame(long frame)
        {
            dto.pauseFrame = frame;
            yield return SetFrame();
        }

        private IEnumerator SetFrame()
        {
            while (!videoPlayer.isPrepared)
            {
                yield return new WaitForEndOfFrame();
            }
            if (!dto.playing)
            {
                videoPlayer.frame = dto.pauseFrame;
            }
        }

        ///<inheritdoc/>
        public override float GetProgress()
        {
            float res = 0;
            if (videoPlayer != null)
                res = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
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

            UMI3DAnimationManager.Instance.StartCoroutine(SetFrame());
        }
    }
}