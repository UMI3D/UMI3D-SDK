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
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Texture resource 
    /// </summary>
    [System.Serializable]
    public class UMI3DTextureResource : UMI3DResource
    {
        [Header("Options")]
        /// <summary>
        /// Optional animator id (used for video texture synchronisation).
        /// </summary>
        [Tooltip("Optional animator id (used for video texture synchronisation).")]
        public string animationId = null;

        /// <summary>
        /// Optional audio source id (used for video texture synchronisation).
        /// </summary>
        [Tooltip("Optional audio source id (used for video texture synchronisation).")]
        public string audioSourceId = null;

        /// <summary>
        /// Optional user Id for video chat.
        /// </summary>
        [Tooltip("Optional user Id for video chat.")]
        public string streamingFromUserId = null;

        /// <inheritdoc/>
        public new TextureDto ToDto()
        {
            var res = new TextureDto();
            ResourceDto resource = base.ToDto();
            res.variants = resource.variants;
            res.animationId = animationId;
            res.audioSourceId = audioSourceId;
            res.streamingFromUserId = streamingFromUserId;
            return res;
        }
        /*
                public static implicit operator TextureDto (UMI3DTextureResource r)
                {
                    return r.ToDto();
                }
                */
    }

    /// <summary>
    /// Texture resource that could be scaled.
    /// </summary>
    [System.Serializable]
    public class UMI3DScalableTextureResource : UMI3DTextureResource
    {
        /// <summary>
        /// Scale of the texture
        /// </summary>
        [Tooltip("Scale of the texture")]
        public float scale = 1f;

        /// <inheritdoc/>
        public new ScalableTextureDto ToDto()
        {
            TextureDto textureDto = base.ToDto();
            return new ScalableTextureDto()
            {
                animationId = textureDto.animationId,
                variants = textureDto.variants,
                audioSourceId = textureDto.audioSourceId,
                scale = scale,
                streamingFromUserId = textureDto.streamingFromUserId
            };
        }

        /*
        public static implicit operator ScalableTextureDto(UMI3DScalableTextureReource r)
        {
            return r.ToDto();
        }*/
    }
}
