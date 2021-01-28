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
using UnityEngine.Video;

namespace umi3d.cdk
{
    public class UMI3DVideoPlayer : UMI3DAbstractAnimation
    {
        VideoPlayer videoPlayer;
        Material mat;
        RenderTexture renderTexture;
        bool started = false;
        new public static UMI3DVideoPlayer Get(string id) { return UMI3DAbstractAnimation.Get(id) as UMI3DVideoPlayer; }

        public UMI3DVideoPlayer(UMI3DVideoPlayerDto dto) : base(dto)
        {
            //init material
            renderTexture = new RenderTexture(1024, 1024, 16, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            renderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;
            //renderTexture.ResolveAntiAliasedSurface();
            //renderTexture.wrapMode = TextureWrapMode.Clamp;
            Debug.Log(renderTexture.isReadable);
            Debug.Log("mat id : " + dto.materialId);
            mat = UMI3DEnvironmentLoader.GetEntity(dto.materialId).Object as Material;
            if (mat == null)
            {
                Debug.LogWarning("Material not found to display video");
                return;
            }
            mat.mainTexture = renderTexture;

            // create unity VideoPlayer
            GameObject videoPlayerGameObject = new GameObject("video");
            videoPlayerGameObject.transform.SetParent(UMI3DResourcesManager.Instance.transform);
            videoPlayer = videoPlayerGameObject.AddComponent<VideoPlayer>();
            videoPlayer.url = UMI3DEnvironmentLoader.Parameters.ChooseVariante(dto.videoResource.variants).url;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.playOnAwake = dto.playing;
            videoPlayer.isLooping = dto.looping;
            if (dto.playing)
            {
                UMI3DAnimationManager.Instance.StartCoroutine(StartAfterLoading(dto.startTime));
            }
            else
            {
                videoPlayer.Stop();
            }
        }

        private IEnumerator StartAfterLoading(DateTime startTime)
        {
            while (!videoPlayer.isPrepared)
            {
                Debug.Log("wait video loading");
                yield return new WaitForEndOfFrame();
            }
            Start((float)(DateTime.Now - dto.startTime).TotalMilliseconds);
            Debug.Log(videoPlayer.isPlaying);
            Debug.Log("Date time now : " + DateTime.Now);
            Debug.Log("Date time dto : " + dto.startTime);
            Debug.Log("time offset : " + (DateTime.Now - dto.startTime).TotalMilliseconds);


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
                started = false;
            }
        }

        ///<inheritdoc/>
        public override void Start(float atTime)
        {
            if (videoPlayer != null)
            {
                videoPlayer.frame = (int)(atTime * videoPlayer.frameRate / 1000);
                videoPlayer.Play();
                started = true;
            }
        }

    }
}