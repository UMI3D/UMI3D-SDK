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

namespace umi3d.common.userCapture.pose
{
    /// <summary>
    /// Describes a skeleton pose.
    /// </summary>
    public interface IUMI3DPoseData
    {
        /// <summary>
        /// The bone that anchor the pose
        /// </summary>
        PoseAnchorDto Anchor { get; }

        /// <summary>
        /// All the bones that describe the pose
        /// </summary>
        IList<BoneDto> Bones { get; }

        /// <summary>
        /// Transforms the Object to its DTO counterpart
        /// </summary>
        /// <returns></returns>
        PoseDto ToPoseDto();
    }
}