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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// Mapper between any skeleton hierarchy and the UMI3D one.
    /// </summary>
    public class SkeletonMapper : MonoBehaviour, ISkeletonMapper
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.Common | DebugScope.UserCapture;

        [SerializeField]
        private SkeletonMappingLinkMarker anchor;

        [SerializeField]
        private List<SkeletonMappingLinkMarker> mappingMarkers = new();

        [SerializeField, ConstEnum(typeof(LevelOfArticulation), typeof(uint))]
        private uint levelOfArticulation;

        [HideInInspector]
        public PoseAnchorDto BoneAnchor
        {
            get
            {
                if (anchor == null)
                    return boneAnchor;

                if (boneAnchor == null || boneAnchor.position.Struct() != anchor.transform.position) // not initialized or has changed
                {
                    boneAnchor = new PoseAnchorDto()
                    {
                        bone = anchor.BoneType,
                        position = anchor.transform.position.Dto(),
                        rotation = anchor.transform.rotation.Dto()
                    };
                }

                return boneAnchor;
            }
            set
            {
                boneAnchor = value;
            }
        }

        private PoseAnchorDto boneAnchor;

        [HideInInspector]
        public IList<SkeletonMapping> Mappings
        {
            get
            {
                if (mappings == null && mappingMarkers.Count == 0)
                    RetrieveMappings();
                return mappingsList;
            }
            set
            {
                mappingsList = value.ToList();
                mappings = mappingsList.ToDictionary(x => x.BoneType);
            }
        }

        private List<SkeletonMapping> mappingsList;

        private Dictionary<uint, SkeletonMapping> mappings;

        private readonly Dictionary<uint, BoneComputation> bonesComputations = new();

        private record BoneComputation
        {
            public bool isComputed = false;
            public BoneDto bone;
            public SubSkeletonBoneDto subBone;
        }

        public void RetrieveMappings()
        {
            mappingMarkers = GetComponentsInChildren<SkeletonMappingLinkMarker>().ToList();
            mappings = new();
            foreach (var marker in mappingMarkers)
            {
                if (mappings.ContainsKey(marker.BoneType))
                    continue;

                if (levelOfArticulation == LevelOfArticulation.NONE || marker.LevelOfArticulation <= levelOfArticulation)
                    mappings.Add(marker.BoneType, marker.ToSkeletonMapping());
            }
            mappingsList = mappings.Values.ToList();
        }

        /// <summary>
        /// Get pose of the bone using mappings.
        /// </summary>
        /// <returns></returns>
        public virtual SubSkeletonPoseDto GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            if (BoneAnchor == null)
            {
                if (anchor != null)
                {
                    BoneAnchor = new PoseAnchorDto()
                    {
                        bone = anchor.BoneType,
                        position = anchor.transform.position.Dto(),
                        rotation = anchor.transform.rotation.Dto()
                    };
                    if (mappingMarkers.Count == 0 && Mappings.Count == 0)
                        RetrieveMappings();
                }
                else
                    UMI3DLogger.LogWarning("BoneAnchor is null.", DEBUG_SCOPE);
            }

            foreach (BoneComputation boneComputation in bonesComputations.Values)
            {
                if (boneComputation != null)
                    boneComputation.isComputed = false;
            }

            List<SubSkeletonBoneDto> bones = new(hierarchy.Relations.Count);
            foreach (SkeletonMapping mapping in Mappings)
            {
                SubSkeletonBoneDto pose = GetBonePose(hierarchy, mapping).subBone;
                if (pose != null)
                    bones.Add(pose);
            }

            return new SubSkeletonPoseDto()
            {
                boneAnchor = BoneAnchor,
                bones = bones
            };
        }

        /// <summary>
        /// Compute bone position for each bone recursively. Start from desired bone.
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="mapping"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private BoneComputation GetBonePose(UMI3DSkeletonHierarchy hierarchy, SkeletonMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            uint boneType = mapping.BoneType;

            // bone already computed
            if (bonesComputations.TryGetValue(boneType, out BoneComputation value) && value.isComputed)
                return value;

            // bone not in hierarchy
            if (!hierarchy.Relations.TryGetValue(boneType, out var relation))
                throw new ArgumentException($"Bone ({boneType}, \"{BoneTypeHelper.GetBoneName(boneType)}\") not defined in hierarchy.", nameof(mapping));

            // get mapping value
            var bone = mapping.GetPose();

            // look for potential parents
            SubSkeletonBoneDto subBone = new() { boneType = boneType };
            if (!mappings.TryGetValue(relation.boneTypeParent, out SkeletonMapping parentMapping) 
                || parentMapping.BoneType == BoneType.None) // bone has no parent
            {
                subBone.localRotation = bone.rotation;
            }
            else // bone has a parent and thus its rotation depends on it
            {
                BoneComputation parentComputation = GetBonePose(hierarchy, parentMapping);
                subBone.localRotation = (UnityEngine.Quaternion.Inverse(parentComputation.bone.rotation.Quaternion()) * bone.rotation.Quaternion()).Dto();
            }

            bonesComputations[boneType] = new()
            {
                isComputed = true,
                bone = mapping.GetPose(),
                subBone = subBone
            };

            return bonesComputations[boneType];
        }
    }
}