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
using UnityEngine;

namespace umi3d.common.userCapture.description
{
    /// <summary>
    /// UMI3D hierarchy of bones.
    /// </summary>
    public class UMI3DSkeletonHierarchy
    {
        public UMI3DSkeletonHierarchy(UMI3DSkeletonHierarchyDefinition definition)
        {
            if (definition is null) //empty hierarchy has at least a hips
            {
                relations.Add(BoneType.Hips, new() { boneTypeParent = BoneType.None, relativePosition = Vector3.zero });
                return;
            }

            foreach (var relation in definition.BoneRelations)
            {
                relations.Add(relation.Bonetype, new UMI3DSkeletonHierarchyNode { boneTypeParent = relation.BonetypeParent, relativePosition = relation.RelativePosition });
            }
        }

        public struct UMI3DSkeletonHierarchyNode
        {
            /// <summary>
            /// UMI3D bone type of the parent bone. 0 if no parent bone.
            /// </summary>
            public uint boneTypeParent;

            /// <summary>
            /// Relative position of the bone relative to it's parent position.
            /// </summary>
            public Vector3 relativePosition;

            public static implicit operator (uint boneTypeParent, Vector3 relativePosition)(UMI3DSkeletonHierarchyNode node)
            {
                return (node.boneTypeParent, node.relativePosition);
            }
        }

        /// <summary>
        /// Cache of the hiearchy.
        /// </summary>
        private readonly Dictionary<uint, UMI3DSkeletonHierarchyNode> relations = new();
        /// <summary>
        /// UMI3D hiearchy nodes, indexed by bonetypes.
        /// </summary>
        public IReadOnlyDictionary<uint, UMI3DSkeletonHierarchyNode> Relations => relations;

        /// <summary>
        /// Create a hierarchy of transform according to the UMI3DHierarchy.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public virtual IEnumerable<(uint umi3dBoneType, Transform boneTransform)> Generate(Transform root)
        {
            Dictionary<uint, bool> hasBeenCreated = new();
            foreach (var bone in Relations.Keys)
                hasBeenCreated[bone] = false;

            Dictionary<uint, Transform> hierarchy = new();

            var boneNames = BoneTypeHelper.GetBoneNames();

            foreach (uint bone in Relations.Keys)
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
                    if (!hasBeenCreated[Relations[bone].boneTypeParent])
                        CreateNode(Relations[bone].boneTypeParent);
                    go.transform.SetParent(hierarchy[Relations[bone].boneTypeParent]);
                }
                else
                {
                    go.transform.SetParent(root);
                }
                go.transform.localPosition = Relations[bone].relativePosition;
                hasBeenCreated[bone] = true;
            }

            return hierarchy.Select(x => (umi3dBoneType: x.Key, boneTransform: x.Value)).ToArray();
        }
    }
}