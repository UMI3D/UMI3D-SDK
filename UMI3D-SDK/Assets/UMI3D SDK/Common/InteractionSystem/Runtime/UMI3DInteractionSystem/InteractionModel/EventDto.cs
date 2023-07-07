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

namespace umi3d.common.interaction
{
    /// <summary>
    /// DTO describing an event interaction block
    /// </summary>
    [System.Serializable]
    public class EventDto : AbstractInteractionDto
    {
        /// <summary>
        /// Should the environment be notified of the event rising edge only (false) or both rising edge and falling edge (true).
        /// </summary>
        public bool hold { get; set; } = false;

        /// <summary>
        /// Id of the animation to be triggered when the interaction is triggered.
        /// </summary>
        public ulong TriggerAnimationId { get; set; }

        /// <summary>
        /// Id of the animation to be triggered when the interaciton is released.
        /// </summary>
        public ulong ReleaseAnimationId { get; set; }


        public EventDto() : base() { }
    }
}
