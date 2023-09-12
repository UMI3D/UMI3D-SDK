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
    public interface IUMI3DPoseData
    {
        /// <summary>
        /// The bone that anchor the pose
        /// </summary>
        UMI3DPose_so.BonePoseField boneAnchorDto { get; }

        /// <summary>
        /// All the bones that describe the pose
        /// </summary>
        IList<UMI3DPose_so.BoneField> BoneDtos { get; }

        /// <summary>
        /// Pose index
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Gets a copy of the bone pose
        /// </summary>
        /// <returns></returns>
        BonePoseDto GetBonePoseCopy();

        /// <summary>
        /// Gets a copy of all the bones
        /// </summary>
        /// <returns></returns>
        List<BoneDto> GetBonesCopy();

        /// <summary>
        /// Stores the data inside the object
        /// </summary>
        /// <param name="bones"></param>
        /// <param name="bonePoseDto"></param>
        void Init(List<BoneDto> bones, BonePoseDto bonePoseDto);

        /// <summary>
        /// Transforms the Object to its DTO counterpart
        /// </summary>
        /// <returns></returns>
        PoseDto ToDto();
    }
}