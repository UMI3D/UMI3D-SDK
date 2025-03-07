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

using System.Collections.Generic;

namespace umi3d.common.interaction
{
    /// <summary>
    /// DTO describing a drawing interaction block
    /// </summary>
    [System.Serializable]
    public class DrawingInteractionDto : EventDto
    {
        /// <summary>
        /// Id of the lineDto use to draw.
        /// </summary>
        public ulong lineId { get; set; }

        /// <summary>
        /// State if the interaction can be done in 3D.
        /// </summary>
        public bool canDrawInSpace { get; set; }

        /// <summary>
        /// Id of the meshDto use to draw.
        /// </summary>
        public List<ulong> meshIds { get; set; }

        public DrawingInteractionDto() : base() { }
    }
}
