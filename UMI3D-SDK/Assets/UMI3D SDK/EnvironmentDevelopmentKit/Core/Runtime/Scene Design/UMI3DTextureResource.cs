using umi3d.common;


namespace umi3d.edk
{
    [System.Serializable]
    public class UMI3DTextureResource : UMI3DResource
    {
        /// <summary>
        /// optional animator id (used for video texture synchronisation).
        /// </summary>
        public string animationId = null;

        /// <summary>
        /// optional audio source id (used for video texture synchronisation).
        /// </summary>
        public string audioSourceId = null;

        /// <summary>
        /// optional user Id for video chat.
        /// </summary>
        public string streamingFromUserId = null;


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

    [System.Serializable]
    public class UMI3DScalableTextureResource : UMI3DTextureResource
    {
        public float scale = 1f;

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
