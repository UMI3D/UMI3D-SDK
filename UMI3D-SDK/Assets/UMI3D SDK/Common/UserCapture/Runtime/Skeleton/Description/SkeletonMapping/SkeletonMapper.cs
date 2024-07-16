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
        public PoseAnchor BoneAnchor
        {
            get
            {
                if (anchor == null)
                    return boneAnchor;

                if (boneAnchor.bone is BoneType.None || boneAnchor.position != anchor.transform.position) // not initialized or has changed
                {
                    boneAnchor = new PoseAnchor()
                    {
                        bone = anchor.BoneType,
                        position = anchor.transform.position,
                        rotation = anchor.transform.rotation
                    };
                }

                return boneAnchor;
            }
            set
            {
                boneAnchor = value;
            }
        }

        private PoseAnchor boneAnchor;

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
                mappingsList = value.Where(x => x != null).OrderByDescending(x => x.BoneType, registeredHierarchy?.Comparer).ToList();
                mappings = mappingsList.ToDictionary(x => x.BoneType);
            }
        }

        private List<SkeletonMapping> mappingsList;

        private Dictionary<uint, SkeletonMapping> mappings;

        private readonly Dictionary<uint, BoneComputation> bonesComputations = new();

        private class BoneComputation
        {
            public bool isComputed;
            public BonePose bone;
            public SubskeletonBonePose subBone;
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
            mappingsList = mappings.Values.Where(x=>x != null).OrderByDescending(x=>x.BoneType, registeredHierarchy?.Comparer).ToList();
        }

        private UMI3DSkeletonHierarchy registeredHierarchy;

        public virtual void SetupHierarchy(UMI3DSkeletonHierarchy hierarchy)
        {
            if (hierarchy == null)
                throw new System.ArgumentNullException(nameof(hierarchy));

            registeredHierarchy = hierarchy;
            mappingsList = mappings.Values.Where(x => x != null).OrderByDescending(x => x.BoneType, registeredHierarchy?.Comparer).ToList();
        }

        /// <summary>
        /// Get pose of the bone using mappings.
        /// </summary>
        /// <returns></returns>
        public virtual SubskeletonPose GetPose(UMI3DSkeletonHierarchy hierarchy)
        {
            if (registeredHierarchy == null && hierarchy != null)
                SetupHierarchy(hierarchy);
                
            if (BoneAnchor.bone is BoneType.None)
            {
                if (anchor != null)
                {
                    BoneAnchor = new PoseAnchor()
                    {
                        bone = anchor.BoneType,
                        position = anchor.transform.position,
                        rotation = anchor.transform.rotation
                    };
                    if (mappingMarkers.Count == 0 && Mappings.Count == 0)
                        RetrieveMappings();
                }
                else
                    UMI3DLogger.LogWarning("BoneAnchor is null.", DEBUG_SCOPE);
            }

            foreach (var boneComputation in bonesComputations.Values)
            {
                if (boneComputation != null)
                    boneComputation.isComputed = false;
            }

            List<SubskeletonBonePose> bones = new(hierarchy.Relations.Count);
            foreach (SkeletonMapping mapping in mappingsList)
            {
                SubskeletonBonePose bonePose = GetBonePose(hierarchy, mapping).subBone;
                bones.Add(bonePose);
            }

            currentPose = new SubskeletonPose(BoneAnchor, bones);

            return currentPose;
        }

        private SubskeletonPose currentPose;

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
            uint boneType = mapping.BoneType;

            if (!bonesComputations.TryGetValue(boneType, out BoneComputation boneComputation)) // bone not existing  yet
            {
                boneComputation = new BoneComputation() { isComputed = false };
                bonesComputations.Add(boneType, boneComputation);
            }
            else if (boneComputation.isComputed) // bone already computed
            {
                return boneComputation;
            }

            // bone not in hierarchy
            if (!hierarchy.Relations.TryGetValue(boneType, out var relation))
                throw new ArgumentException($"Bone ({boneType}, \"{BoneTypeHelper.GetBoneName(boneType)}\") not defined in hierarchy.", nameof(mapping));

            // get mapping value
            BonePose bone = mapping.GetBonePose();

            // look for potential parents
            Quaternion localRotation;
            if (!mappings.TryGetValue(relation.boneTypeParent, out SkeletonMapping parentMapping) 
                || parentMapping.BoneType == BoneType.None) // bone has no parent mapping
            {
                localRotation = bone.rotation;
            }
            else // bone has a parent mapping and thus its rotation depends on it
            {
                BoneComputation parentComputation = GetBonePose(hierarchy, parentMapping);
                localRotation = (UnityEngine.Quaternion.Inverse(parentComputation.bone.rotation) * bone.rotation);
            }

            SubskeletonBonePose subBone = new(boneType, localRotation: localRotation);
            boneComputation.bone = mapping.GetBonePose();
            boneComputation.subBone = subBone;
            boneComputation.isComputed = true;

            return boneComputation;
        }
    }
}