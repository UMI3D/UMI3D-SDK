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

using inetum.unityUtils;
using umi3d.common;
using UnityEngine;
using UnityEngine.Video;

namespace umi3d.edk
{
    /// <summary>
    /// Video animation support in UMI3D. A video player enables the playing of video resources.
    /// </summary>
    public class UMI3DVideoPlayer : UMI3DAbstractAnimation
    {
        /// <summary>
        /// VideoPlayer from Unity that used to synchonize video progress.
        /// </summary>
        /// It could be null.
        [Tooltip("VideoPlayer from Unity. Could be null. mainly used to synchronize the video progress.")]
        public VideoPlayer video;

        /// <summary>
        /// Material on which the video is applied to.
        /// </summary>
        [SerializeField, Tooltip("Material on which the video is applied to.")]
        private MaterialSO material;

        /// <summary>
        /// Video to play as a resource.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Video to play as a resource.")]
        private UMI3DResource videoResources;

        /// <summary>
        /// Audio player for the sound of the video.
        /// </summary>
        [Tooltip("Audio player for the sound of the video.")]
        public UMI3DAudioPlayer audioPlayer;
        /// <summary>
        /// See <see cref="videoResources"/>.
        /// </summary>
        private UMI3DAsyncProperty<UMI3DResource> objectVideoResource;
        /// <summary>
        /// See <see cref="material"/>.
        /// </summary>
        public UMI3DAsyncProperty<MaterialSO> ObjectMaterial;
        /// <summary>
        /// See <see cref="videoResources"/>.
        /// </summary>
        public UMI3DAsyncProperty<UMI3DResource> ObjectVideoResource { get { Register(); return objectVideoResource; } protected set => objectVideoResource = value; }

        /// <inheritdoc/>
        protected override void InitDefinition(ulong id)
        {
            base.InitDefinition(id);
            ObjectMaterial = new UMI3DAsyncProperty<MaterialSO>(id, UMI3DPropertyKeys.AnimationNode, material, (m, u) => m?.Id());
            ObjectVideoResource = new UMI3DAsyncProperty<UMI3DResource>(id, UMI3DPropertyKeys.AnimationResource, videoResources, (r, u) => r?.ToDto());

            ObjectMaterial.OnValueChanged += (n) => material = n;
            ObjectVideoResource.OnValueChanged += (r) => videoResources = r;
        }

        /// <inheritdoc/>
        protected override void WriteProperties(UMI3DAbstractAnimationDto dto, UMI3DUser user)
        {
            base.WriteProperties(dto, user);
            var Adto = dto as UMI3DVideoPlayerDto;
            Adto.materialId = ObjectMaterial.GetValue(user)?.Id() ?? 0;
            Adto.videoResource = ObjectVideoResource.GetValue(user)?.ToDto();
            if (audioPlayer != null)
            {
                Adto.audioId = audioPlayer.Id();
            }
        }

        /// <inheritdoc/>
        protected override UMI3DAbstractAnimationDto CreateDto()
        {
            return new UMI3DVideoPlayerDto();
        }

        /// <inheritdoc/>
        protected override Bytable ToBytesAux(UMI3DUser user)
        {
            return UMI3DSerializer.Write(ObjectMaterial.GetValue(user)?.Id() ?? 0)
                + UMI3DSerializer.Write(audioPlayer?.Id() ?? 0)
                + ObjectVideoResource.GetValue(user).ToByte();
        }
    }
}