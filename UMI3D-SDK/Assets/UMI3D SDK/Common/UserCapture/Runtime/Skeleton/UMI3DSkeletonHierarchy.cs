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
using System.Linq;

using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.common.userCapture
{
    [CreateAssetMenu(fileName = "UMI3DSkeletonHierarchy", menuName = "UMI3D/UMI3D Skeleton Hierarchy")]
    public class UMI3DSkeletonHierarchy : ScriptableObject
    {
        [Serializable]
        public class BoneRelation
        {
            [ConstEnum(typeof(BoneType), typeof(uint)), Tooltip("Bone type in UMI3D standards.")]
            public uint Bonetype;
            [ConstEnum(typeof(BoneType), typeof(uint)), Tooltip("Parent bone in the hierarchy.")]
            public uint BonetypeParent;
            [Tooltip("The relative position of the current bone type.")]
            public Vector3 RelativePosition;

            public BoneRelation(uint boneType, uint boneTypeParent, Vector3 relationPosition)
            {
                this.Bonetype = boneType;
                this.BonetypeParent = boneTypeParent;
                this.RelativePosition = relationPosition;
            }
        }

        public List<BoneRelation> BoneRelations = new List<BoneRelation>();

        public Dictionary<uint, (uint boneTypeParent, Vector3 relativePosition)> SkeletonHierarchy
        { 
            get
            {
                _skeletonHierarchy ??= ToSkeletonHierarchyDict();
                return _skeletonHierarchy;
            }
            
            protected set
            {
                _skeletonHierarchy = value;
            }
        }

        [NonSerialized]
        private Dictionary<uint, (uint boneTypeParent, Vector3 relativePosition)> _skeletonHierarchy;

        private Dictionary<uint, (uint boneTypeParent, Vector3 relativePosition)> ToSkeletonHierarchyDict()
        {
            _skeletonHierarchy = new();

            foreach (var relation in BoneRelations)
            {
                _skeletonHierarchy.Add(relation.Bonetype, (relation.BonetypeParent, relation.RelativePosition));
            }

            return _skeletonHierarchy;
        }
    }

    public static class UMI3DSkeletonHirearchyExtensions
    {
        /// <summary>
        /// Create a hierarchy of transform according to the UMI3DHierarchy in the parameters.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static (uint umi3dBoneType, Transform boneTransform)[] Generate(this UMI3DSkeletonHierarchy hierarchyToCopy, Transform root)
        {
            var copiedHierarchy = hierarchyToCopy.SkeletonHierarchy;

            Dictionary<uint, bool> hasBeenCreated = new();
            foreach (var bone in copiedHierarchy.Keys)
                hasBeenCreated[bone] = false;

            Dictionary<uint, Transform> hierarchy = new();

            var boneNames = BoneTypeHelper.GetBoneNames();

            foreach (uint bone in copiedHierarchy.Keys)
            {
                if (!hasBeenCreated[bone])
                    CreateNode(bone);
            }

            void CreateNode(uint bone)
            {
                var go = new GameObject(boneNames[bone]);
                hierarchy[bone] = go.transform;
                if (bone != BoneType.Hips) // root
                {
                    if (!hasBeenCreated[copiedHierarchy[bone].boneTypeParent])
                        CreateNode(copiedHierarchy[bone].boneTypeParent);
                    go.transform.SetParent(hierarchy[copiedHierarchy[bone].boneTypeParent]);
                }
                else
                {
                    go.transform.SetParent(root);
                }
                go.transform.localPosition = copiedHierarchy[bone].relativePosition;
                hasBeenCreated[bone] = true;
            }

            return hierarchy.Select(x => (umi3dBoneType: x.Key, boneTransform: x.Value)).ToArray();
        }
    }
}