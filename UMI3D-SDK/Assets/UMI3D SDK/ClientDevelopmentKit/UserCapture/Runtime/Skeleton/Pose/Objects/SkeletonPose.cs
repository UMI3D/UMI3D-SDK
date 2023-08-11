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
    public class SkeletonPose
    {
        private PoseDto dto;

        /// <summary>
        /// See <see cref="PoseDto.index"/>.
        /// </summary>
        public int Index => dto.index;

        /// <summary>
        /// See <see cref="PoseDto.boneAnchor"/>.
        /// </summary>
        public BonePoseDto BoneAnchor => dto.boneAnchor;

        /// <summary>
        /// See <see cref="PoseDto.bones"/>.
        /// </summary>
        public List<BoneDto> Bones => dto.bones;

        /// <summary>
        /// If true the pose is defined by the browser, not the environment.
        /// </summary>
        public bool IsCustom { get; private set; } = false;

        public SkeletonPose(PoseDto dto, bool isCustom = false)
        {
            this.dto = dto;
            IsCustom = isCustom;
        }
    }
}