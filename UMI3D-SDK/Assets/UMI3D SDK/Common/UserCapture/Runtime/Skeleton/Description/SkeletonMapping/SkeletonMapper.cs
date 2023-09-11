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
using System.Linq;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// Mapper between any skeleton hiearchy and the UMI3D one.
    /// </summary>
    public class SkeletonMapper : MonoBehaviour, ISkeletonMapper
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.Common | DebugScope.UserCapture;

        public BonePoseDto BoneAnchor;
        public SkeletonMapping[] Mappings = new SkeletonMapping[0];

        private Dictionary<uint, (BoneDto bone, SubSkeletonBoneDto subBone)> computedMap = new();
        /// <summary>
        /// Get pose of the bone using mappings.
        /// </summary>
        /// <returns></returns>
        public virtual SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            if (BoneAnchor == null)
                UMI3DLogger.LogWarning("BoneAnchor is null.", DEBUG_SCOPE);

            computedMap.Clear();



            return new SubSkeletonPoseDto()
            {
                boneAnchor = BoneAnchor,
                bones = Mappings.Select(b => GetPose(hierarchy,b).subBone).Where(b => b != null).ToList()
            };
        }

        private (BoneDto bone, SubSkeletonBoneDto subBone) GetPose(UMI3DSkeletonHierarchy hierarchy, SkeletonMapping mapping)
        {
            if(mapping == null)
                return (null, null);

            if (computedMap.ContainsKey(mapping.BoneType))
                return computedMap[mapping.BoneType];

            if (!hierarchy.Relations.ContainsKey(mapping.BoneType))
                return (null, null);

            var relation = hierarchy.Relations[mapping.BoneType];

            var bone = mapping.GetPose();
            if (bone == null)
                return (null, null);

            var parentMapping = Mappings.FirstOrDefault(m => m.BoneType == relation.boneTypeParent);
            var parent = GetPose(hierarchy, parentMapping);

            SubSkeletonBoneDto subBone = new();
            subBone.localRotation =  
                parent.bone == null ? 
                    bone.rotation:
                    (Quaternion.Inverse(parent.bone.rotation.Quaternion()) *  bone.rotation.Quaternion()).Dto();

            computedMap[mapping.BoneType] = new()
            {
                bone = mapping.GetPose(),
                subBone = subBone
            };

            return computedMap[mapping.BoneType];
        }
    }
}