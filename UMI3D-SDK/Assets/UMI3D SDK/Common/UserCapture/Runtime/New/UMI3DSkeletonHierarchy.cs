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
using System.Collections;
using System.Collections.Generic;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.cdk.userCapture
{
    [CreateAssetMenu(fileName = "UMI3DSkeletonHierarchy", menuName = "UMI3D/UMI3D Skeleton Hierarchy")]
    public class UMI3DSkeletonHierarchy : ScriptableObject
    {
        [Serializable]
        public class BoneRelation
        {
            [ConstEnum(typeof(BoneType), typeof(uint)), Tooltip("Bone type in UMI3D standards.")]
            public uint Bonetype;
            [ConstEnum(typeof(BoneType), typeof(uint)), Tooltip("Bone type in UMI3D standards.")]
            public uint BonetypeParent;
            public Vector3 RelativePosition;

            public BoneRelation(uint boneType, uint boneTypeParent, Vector3 relationPosition)
            {
                this.Bonetype = boneType;
                this.BonetypeParent = boneTypeParent;
                this.RelativePosition = relationPosition;
            }
        }

        public List<BoneRelation> BoneRelations = new List<BoneRelation>();

        public static Dictionary<uint, (uint, Vector3)> SkeletonHierarchy { get; protected set; }

        public static UnityEvent SetSkeletonHierarchy = new UnityEvent();

        private void OnEnable()
        {
            ToSkeletonHierarchy();
        }

        protected void ToSkeletonHierarchy()
        {
            if (SkeletonHierarchy == null) { SkeletonHierarchy = new Dictionary<uint, (uint, Vector3)>(); }

            SkeletonHierarchy.Clear();

            foreach (var relation in BoneRelations)
            {
                SkeletonHierarchy.Add(relation.Bonetype, (relation.BonetypeParent, relation.RelativePosition));
            }

            SetSkeletonHierarchy.Invoke();
        }
    }
}