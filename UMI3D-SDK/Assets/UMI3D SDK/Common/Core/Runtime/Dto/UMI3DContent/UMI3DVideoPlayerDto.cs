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

namespace umi3d.common
{
    /// <summary>
    /// DTO describing a video player, a enriched playable video resource.
    /// </summary>
    public class UMI3DVideoPlayerDto : UMI3DAbstractAnimationDto
    {
        /// <summary>
        /// Video ressource to be played.
        /// </summary>
        public ResourceDto videoResource;

        /// <summary>
        /// UMI3D id of the material on which the video texture is applied.
        /// </summary>
        public ulong materialId;

        /// <summary>
        /// UMI3D id of the audio played during the video.
        /// </summary>
        public ulong audioId;

    }
}