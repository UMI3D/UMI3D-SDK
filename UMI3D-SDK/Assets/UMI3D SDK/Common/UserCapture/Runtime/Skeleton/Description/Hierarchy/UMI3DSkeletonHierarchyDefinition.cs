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
using UnityEngine;

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// Define a UMI3D skeleton hiearchy of bones.
    /// </summary>
    [CreateAssetMenu(fileName = "UMI3DSkeletonHierarchyDefinition", menuName = "UMI3D/UMI3D Skeleton Hierarchy Definition")]
    public class UMI3DSkeletonHierarchyDefinition : ScriptableObject
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
            [ConstEnum(typeof(BoneType), typeof(uint)), Tooltip("Bone type in UMI3D standards.")]
            public uint Bonetype;

            /// <summary>
            /// Parent bone in the hierarchy.
            /// </summary>
            [ConstEnum(typeof(BoneType), typeof(uint)), Tooltip("Parent bone in the hierarchy.")]
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
        [Tooltip("Collection of relation between a bone and its parent. Declare the hierarchy.")]
        public List<BoneRelation> BoneRelations = new List<BoneRelation>();
    }
}