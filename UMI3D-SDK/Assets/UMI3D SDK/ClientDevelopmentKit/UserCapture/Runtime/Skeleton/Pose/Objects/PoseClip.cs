/*
Copyright 2019 - 2023 Inetum

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
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Browser representation of a pose.
    /// </summary>
    public class PoseClip
    {
        private PoseClipDto dto;

        /// <summary>
        /// See <see cref="PoseClipDto.id"/>.
        /// </summary>
        public ulong Id => dto.id;

        /// <summary>
        /// See <see cref="PoseClipDto.pose"/>.
        /// </summary>
        public PoseAnchorDto Anchor => dto.pose.anchor;

        /// <summary>
        /// See <see cref="PoseDto.bones"/>.
        /// </summary>
        public List<BoneDto> Bones => dto.pose.bones;

        /// <summary>
        /// Description of the pose animation.
        /// </summary>
        public PoseDto Pose => dto.pose;

        public PoseClip(PoseClipDto dto)
        {
            this.dto = dto;
        }
    }
}