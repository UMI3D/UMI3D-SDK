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
    public class DrawingDto : InteractionRequestDto
    {
        public bool drawingEnd { get; set; }

        public ulong clientDrawingId { get; set; } = 0;

        public ulong clientLineId { get; set; } = 0;

        public ulong surfaceId { get; set; } = 0;

        public List<Vector3Dto> positions { get; set; } = new List<Vector3Dto>();
    }
}