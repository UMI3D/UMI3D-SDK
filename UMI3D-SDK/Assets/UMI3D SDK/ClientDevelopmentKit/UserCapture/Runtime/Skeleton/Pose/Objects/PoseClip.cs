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

using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Browser representation of a pose.
    /// </summary>
    public class PoseClip : IBrowserEntity, ISubskeletonDescriptor
    {
        private PoseClipDto dto;
        protected internal PoseClipDto Dto => dto;

        /// <summary>
        /// See <see cref="PoseClipDto.id"/>.
        /// </summary>
        public ulong Id => dto.id;

        /// <summary>
        /// See <see cref="PoseDto.bones"/>.
        /// </summary>
        public List<BoneDto> Bones => dto.pose.bones;

        /// <summary>
        /// See <see cref="PoseClipDto.isComposable"/>.
        /// </summary>
        public bool IsComposable => dto.isComposable;

        /// <summary>
        /// See <see cref="PoseClipDto.isInterpolable"/>.
        /// </summary>
        public bool IsInterpolable => dto.isInterpolable;

        /// <summary>
        /// Description of the pose animation.
        /// </summary>
        public PoseDto Pose => dto.pose;

        public PoseClip(PoseClipDto dto)
        {
            this.dto = dto;
        }

        /// <inheritdoc/>
        public SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            Dictionary<uint, SubSkeletonBoneDto> bonePoses = new();

            foreach (BoneDto bone in Bones)
            {
                if (bonePoses.ContainsKey(bone.boneType))
                    continue;

                bonePoses[bone.boneType] = GetBonePose(hierarchy, bone).subBone;
            }

            computedMap.Clear();

            return new SubSkeletonPoseDto()
            {
                bones = bonePoses.Values.ToList()
            };
        }

        private Dictionary<uint, (BoneDto bone, SubSkeletonBoneDto subBone)> computedMap = new();

        /// <summary>
        /// Recursively compute local rotation for a bone.
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="boneDto"></param>
        /// <param name="pose"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private (BoneDto bone, SubSkeletonBoneDto subBone) GetBonePose(UMI3DSkeletonHierarchy hierarchy, BoneDto boneDto)
        {
            if (boneDto == null)
                throw new ArgumentNullException(nameof(boneDto));

            uint boneType = boneDto.boneType;

            if (computedMap.ContainsKey(boneType))
                return computedMap[boneType];

            if (!hierarchy.Relations.ContainsKey(boneType))
                throw new ArgumentException($"Bone ({boneType}, \"{BoneTypeHelper.GetBoneName(boneType)}\") not defined in hierarchy.", nameof(boneDto));

            var relation = hierarchy.Relations[boneType];

            var parentBone = Bones.Find(b => b.boneType == relation.boneTypeParent);

            SubSkeletonBoneDto subBone = new() { boneType = boneType };
            if (parentBone == default || parentBone.boneType == BoneType.None) // bone has no parent
            {
                subBone.localRotation = boneDto.rotation;
            }
            else // bone has a parent and thus its rotation depends on it
            {
                var parent = GetBonePose(hierarchy, parentBone);
                subBone.localRotation = (Quaternion.Inverse(parent.bone.rotation.Quaternion()) * boneDto.rotation.Quaternion()).Dto();
            }

            computedMap[boneType] = new()
            {
                bone = boneDto,
                subBone = subBone
            };

            return computedMap[boneType];
        }
    }
}