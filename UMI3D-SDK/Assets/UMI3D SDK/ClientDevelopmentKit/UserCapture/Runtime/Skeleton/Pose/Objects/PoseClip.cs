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

        public IReadOnlyDictionary<uint, BonePose> BonePoses {  get; protected set; }

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
            BonePoses = dto.pose.bones.ToDictionary(k=>k.boneType, x => new BonePose(x.boneType, x.rotation.Quaternion()));
        }

        /// <inheritdoc/>
        public SubskeletonPose GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new ArgumentNullException(nameof(hierarchy));

            Dictionary<uint, SubskeletonBonePose> bonePoses = new();

            foreach (var bone in BonePoses.Values)
            {
                if (bonePoses.ContainsKey(bone.boneType))
                    continue;

                bonePoses[bone.boneType] = GetBonePose(hierarchy, bone).subBone;
            }

            computedMap.Clear();

            return new SubskeletonPose(default, bonePoses.Values.ToList());
        }

        private Dictionary<uint, (BonePose bone, SubskeletonBonePose subBone)> computedMap = new();

        /// <summary>
        /// Recursively compute local rotation for a bone.
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="boneDto"></param>
        /// <param name="pose"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private (BonePose bone, SubskeletonBonePose subBone) GetBonePose(UMI3DSkeletonHierarchy hierarchy, BonePose boneDto)
        {
            uint boneType = boneDto.boneType;

            if (computedMap.ContainsKey(boneType))
                return computedMap[boneType];

            if (!hierarchy.Relations.TryGetValue(boneType, out var relation))
                throw new ArgumentException($"Bone ({boneType}, \"{BoneTypeHelper.GetBoneName(boneType)}\") not defined in hierarchy.", nameof(boneDto));

            Quaternion localRotation;
            if (BonePoses.TryGetValue(relation.boneTypeParent, out BonePose parentBonePose) || parentBonePose.boneType == BoneType.None) // bone has no parent
            {
                localRotation = boneDto.rotation;
            }
            else // bone has a parent and thus its rotation depends on it
            {
                var parent = GetBonePose(hierarchy, parentBonePose);
                localRotation = (Quaternion.Inverse(parent.bone.rotation) * boneDto.rotation);
            }

            computedMap[boneType] = new()
            {
                bone = boneDto,
                subBone = new(boneType, localRotation) 
            };

            return computedMap[boneType];
        }
    }
}