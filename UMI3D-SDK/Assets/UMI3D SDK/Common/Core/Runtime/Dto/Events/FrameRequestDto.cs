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
    public class FrameRequestDto : AbstractOperationDto
    {
        /// <summary>
        /// Frame's UMI3D id.
        /// </summary>
        public ulong FrameId { get; set; } = 0;

        /// <summary>
        /// Scale of the user in the new referentiel. 
        /// Not to be misunderstood with the height.
        /// A user should be at full height with a scale of one.
        /// </summary>
        public float scale { get; set; } = 1;
    }
}