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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace umi3d.common.userCapture.description
{
    [CreateAssetMenu(fileName = "UMI3DSkeletonHierarchyDefinition", menuName = "UMI3D/UMI3D Skeleton Hierarchy Definition")]
    public class UMI3DSkeletonHierarchyDefinition : ScriptableObject, IUMI3DSkeletonHierarchyDefinition, IUMI3DSkeletonMusclesDefinition
    {
        /// <summary>
        /// Relation between a bone and its parent.
        /// </summary>
        [Serializable]
        public class BoneRelation
        {
            /// <summary>
            /// Bone type in UMI3D standards.
            /// </summary>
            [ConstEnum(typeof(BoneType), typeof(uint))]
            public uint Bonetype;

            /// <summary>
            /// Parent bone in the hierarchy.
            /// </summary>
            [ConstEnum(typeof(BoneType), typeof(uint))]
            public uint BonetypeParent;

            /// <summary>
            /// The position of the current bone type relative to its parent.
            /// </summary>
            [Tooltip("The relative position of the current bone type.")]
            public Vector3 RelativePosition;

            public BoneRelation(uint boneType, uint boneTypeParent, Vector3 relationPosition)
            {
                this.Bonetype = boneType;
                this.BonetypeParent = boneTypeParent;
                this.RelativePosition = relationPosition;
            }
        }

        /// <summary>
        /// Collection of relation between a bone and its parent.
        /// </summary>
        [SerializeField, Tooltip("Collection of relation between a bone and its parent. Declare the hierarchy.")]
        private List<BoneRelation> BoneRelations = new List<BoneRelation>();

        [SerializeField]
        private GameObject skeletonPrefab;

        public GameObject SkeletonPrefab => skeletonPrefab;

        public IList<IUMI3DSkeletonHierarchyDefinition.BoneRelation> Relations
            => BoneRelations.Select(x => new IUMI3DSkeletonHierarchyDefinition.BoneRelation()
            {
                boneType = x.Bonetype,
                parentBoneType = x.BonetypeParent,
                relativePosition = x.RelativePosition.Dto()
            }).ToList();

        /// <summary>
        /// Constraint on a bone rotation.
        /// </summary>
        [Serializable]
        public class Muscle
        {
            /// <summary>
            /// Bone type in UMI3D standards.
            /// </summary>
            [ConstEnum(typeof(BoneType), typeof(uint)), Tooltip("Bone type in UMI3D standards.")]
            public uint Bonetype;

            [Tooltip("Offset for restrictions. Typically used for fingers.")]
            public Quaternion ReferenceFrameRotation = Quaternion.identity;

            [Tooltip("Rotation restriction on rotation axis X.")]
            public RotationRestriction XRotationRestriction = new();

            [Tooltip("Rotation restriction on rotation axis Y.")]
            public RotationRestriction YRotationRestriction = new();

            [Tooltip("Rotation restriction on rotation axis Z.")]
            public RotationRestriction ZRotationRestriction = new();

            [Serializable]
            public class RotationRestriction
            {
                [Tooltip("If true, the axis rotation restriction is taken into account.")]
                public bool isRestricted = false;

                [Tooltip("Lowest tolerated angle in degrees.")]
                public float min = -180f;

                [Tooltip("Greatest tolerated angle in degrees.")]
                public float max = 180f;

                /// <summary>
                /// Ensure that rotations are between -180 and 180 and min <= max
                /// </summary>
                /// <returns></returns>
                public RotationRestriction Format()
                {
                    this.min = (min % 360f) < 180f ? min % 360f : ((min % 360f) - 360f);
                    this.max = (max % 360f) < 180f ? max % 360f : ((max % 360f) - 360f);

                    if (min > max)
                        max = min;

                    return this;
                }
            }
        }

        [SerializeField, Tooltip(" Rotation restrictions to apply to bones.")]
        private List<Muscle> _muscles = new();

        /// <summary>
        /// Rotation restrictions to apply to bones.
        /// </summary>
        public List<IUMI3DSkeletonMusclesDefinition.Muscle> Muscles => _muscles.Select(ConvertMuscle).ToList();

        // Controls and format values.
        private IUMI3DSkeletonMusclesDefinition.Muscle ConvertMuscle(Muscle muscleInput)
        {
            muscleInput.XRotationRestriction.Format();
            muscleInput.YRotationRestriction.Format();
            muscleInput.ZRotationRestriction.Format();

            return new()
            {
                Bonetype = muscleInput.Bonetype,
                ReferenceFrameRotation = muscleInput.ReferenceFrameRotation != default ? muscleInput.ReferenceFrameRotation.Dto() : Quaternion.identity.Dto(),

                XRotationRestriction = muscleInput.XRotationRestriction.isRestricted ? new() { min = muscleInput.XRotationRestriction.min, max = muscleInput.XRotationRestriction.max } : null,
                YRotationRestriction = muscleInput.YRotationRestriction.isRestricted ? new() { min = muscleInput.YRotationRestriction.min, max = muscleInput.YRotationRestriction.max } : null,
                ZRotationRestriction = muscleInput.ZRotationRestriction.isRestricted ? new() { min = muscleInput.ZRotationRestriction.min, max = muscleInput.ZRotationRestriction.max } : null
            };
        }
    }
}