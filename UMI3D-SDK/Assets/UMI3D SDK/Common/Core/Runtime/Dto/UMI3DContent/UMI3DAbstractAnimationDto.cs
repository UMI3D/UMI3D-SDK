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
    /// DTO describing animations, components that can be played.
    /// </summary>
    [System.Serializable]
    public class UMI3DAbstractAnimationDto : AbstractEntityDto, IEntity
    {
        /// <summary>
        /// Is the animation running?
        /// </summary>
        public bool playing { get; set; } = false;

        /// <summary>
        /// Should the animation restart from its beginning at its end?
        /// </summary>
        public bool looping { get; set; } = false;

        /// <summary>
        /// Time since start in miliseconds, represented in server time since its start.
        /// </summary>
        public ulong startTime { get; set; }

        /// <summary>
        /// Time of last pause in miliseconds, represented in server time since its start.
        /// </summary>
        public long pauseTime { get; set; }

    }
}
