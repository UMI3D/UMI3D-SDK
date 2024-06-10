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

namespace umi3d.edk
{
    /// <summary>
    /// Behaviours for all animated media, such as animations, videoplayers or audioplayers
    /// </summary>
    public interface IAnimation : UMI3DEntity
    {
        /// <summary>
        /// Should the animation start again when reaching its end?
        /// </summary>
        UMI3DAsyncProperty<bool> objectLooping { get; }

        /// <summary>
        /// Animation last pause time in milliseconds.
        /// </summary>
        UMI3DAsyncProperty<long> objectPauseTime { get; }

        /// <summary>
        /// Is the animation playing?
        /// </summary>
        UMI3DAsyncProperty<bool> objectPlaying { get; }

        /// <summary>
        /// Animation last pause time in milliseconds.
        /// </summary>
        UMI3DAsyncProperty<ulong> objectStartTime { get; }

        UMI3DAbstractAnimationDto ToAnimationDto(UMI3DUser user);
    }
}