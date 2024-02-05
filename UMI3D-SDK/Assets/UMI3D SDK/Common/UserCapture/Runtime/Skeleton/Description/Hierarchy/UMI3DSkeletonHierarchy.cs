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
    using BoneRelation = IUMI3DSkeletonHierarchyDefinition.BoneRelation;

    /// <summary>
    /// UMI3D hierarchy of bones.
    /// </summary>
    public class UMI3DSkeletonHierarchy
    {
        private const DebugScope DEBUG_SCOPE = DebugScope.Common | DebugScope.UserCapture;

        public UMI3DSkeletonHierarchy(IUMI3DSkeletonHierarchyDefinition definition)
        {
            if (definition == null || definition.Relations.Count == 0) //empty hierarchy has at least a hips
            {
                relations.Add(BoneType.Hips, new() { boneType = BoneType.None, relativePosition = Vector3.zero });
                return;
            }

            var relationGroupings = definition.Relations.GroupBy(x => x.parentBoneType).ToDictionary(x => x.Key, x => x.ToList());

            // find root
            uint rootBoneType = FindRoots(definition);

            // create hierarchy of nodes
            root = CreateHierarchyNode(BoneType.None, rootBoneType, relationGroupings[rootBoneType]);
            relations.Add(rootBoneType, root);

            UMI3DSkeletonHierarchyNode CreateHierarchyNode(uint parentBoneType, uint boneType, List<BoneRelation> relationGroup)
            {
                List<UMI3DSkeletonHierarchyNode> children = new();
                foreach (var childRelation in relationGroup)
                {
                    UMI3DSkeletonHierarchyNode childNode;
                    if (relationGroupings.ContainsKey(childRelation.boneType)) // child has its own relations
                    {
                        childNode = CreateHierarchyNode(childRelation.parentBoneType, childRelation.boneType, relationGroupings[childRelation.boneType]);
                    }
                    else // child is a leaf
                    {
                        childNode = new UMI3DSkeletonHierarchyNode()
                        {
                            boneTypeParent = childRelation.parentBoneType,
                            boneType = childRelation.boneType,
                            relativePosition = childRelation.relativePosition.Struct(),
                            children = new()
                        };
                    }

                    relations.Add(childRelation.boneType, childNode);
                    children.Add(childNode);
                }

                return new UMI3DSkeletonHierarchyNode()
                {
                    boneTypeParent = parentBoneType,
                    boneType = boneType,
                    relativePosition = definition.Relations.FirstOrDefault(x => x.boneType == boneType).relativePosition?.Struct() ?? Vector3.zero,
                    children = children,
                };
            }
        }

        private uint FindRoots(IUMI3DSkeletonHierarchyDefinition definition)
        {
            List<uint> childNodes = definition.Relations.Select(x => x.boneType).ToList(); // bones that are children of other bones

            List<uint> roots = definition.Relations.Where(x => x.parentBoneType == BoneType.None).Select(x => x.boneType).ToList();

            if (roots.Count == 0)
                roots = definition.Relations.Where(x=>!childNodes.Contains(x.parentBoneType)).Select(x=>x.parentBoneType).ToList(); // roots are node that children of no other bones

            if (roots.Count == 0)
            {
                throw new System.ArgumentException("Hierarchy definition has no root. It may be empty or cyclical.", nameof(definition));
            }
            if (roots.Count > 1)
            {
                throw new System.ArgumentException("Hierarchy definition contains several roots. Please ensure it has only one root.", nameof(definition));
            }

            return roots.First();
        }

        public class UMI3DSkeletonHierarchyNode
        {
            /// <summary>
            /// UMI3D bone type of the parent bone. 0 if no parent bone.
            /// </summary>
            public uint boneType;

            /// <summary>
            /// UMI3D bone type of the parent bone. 0 if no parent bone.
            /// </summary>
            public uint boneTypeParent;

            /// <summary>
            /// UMI3D bone type of children bones
            /// </summary>
            public List<UMI3DSkeletonHierarchyNode> children;

            /// <summary>
            /// Relative position of the bone relative to it's parent position.
            /// </summary>
            public Vector3 relativePosition;
        }

        /// <summary>
        /// Cache of the hierarchy.
        /// </summary>
        private readonly Dictionary<uint, UMI3DSkeletonHierarchyNode> relations = new();

        /// <summary>
        /// UMI3D hierarchy nodes, indexed by bone types.
        /// </summary>
        public IReadOnlyDictionary<uint, UMI3DSkeletonHierarchyNode> Relations => relations;

        private UMI3DSkeletonHierarchyNode root;
        public UMI3DSkeletonHierarchyNode Root => root;

        /// <summary>
        /// Create a hierarchy of transform according to the UMI3DHierarchy.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public virtual IDictionary<uint, Transform> Generate(Transform root)
        {
            Dictionary<uint, Transform> hierarchy = new();

            var boneNames = BoneTypeHelper.GetBoneNames();

            CreateNode(root.gameObject, Root);

            void CreateNode(GameObject parentGo, UMI3DSkeletonHierarchyNode node)
            {
                var go = new GameObject(boneNames[node.boneType]);
                hierarchy.Add(node.boneType, go.transform);
                go.transform.SetParent(parentGo.transform);
                go.transform.localPosition = node.relativePosition;
                foreach (var child in node.children)
                    CreateNode(go, child);
            }

            return hierarchy;
        }

        public (uint root, IDictionary<uint, uint[]>) Tree
        {
            get
            {
                if (!isTreeComputed)
                    ToTree();

                return computedTree;
            }
        }

        private (uint root, IDictionary<uint, uint[]>) computedTree;
        private bool isTreeComputed;

        private (uint root, IDictionary<uint, uint[]>) ToTree()
        {
            uint rootBone = relations.First(x => x.Value.boneTypeParent == BoneType.None).Key;

            List<(uint bone, uint[] children)> tree = new();

            AddChildrenToTree(rootBone, tree);

            void AddChildrenToTree(uint bone, List<(uint bone, uint[] children)> tree)
            {
                uint[] children = relations.Where(b => b.Value.boneTypeParent == bone).Select(x => x.Key).ToArray();
                tree.Add((bone, children));

                foreach (var child in children)
                    AddChildrenToTree(child, tree);
            }

            isTreeComputed = true;
            computedTree = (rootBone, tree.ToDictionary(x=>x.bone, x=>x.children));
            return computedTree;
        }

        public IDictionary<uint, Vector4Dto> ToLocalRotations(IDictionary<uint, Vector4Dto> rotations)
        {
            Dictionary<uint, Vector4Dto> computed = new();

            var (root, relations) = Tree;

            computed.Add(root, rotations[root]);

            ComputeChildrenRotations(root);

            void ComputeChildrenRotations(uint bone)
            {
                foreach(var childBone in relations[bone])
                {
                    var childLocalRotation = (UnityEngine.Quaternion.Inverse(computed[bone].Quaternion()) * rotations[childBone].Quaternion()).Dto();
                    computed.Add(childBone, childLocalRotation);
                    ComputeChildrenRotations(childBone);
                }
            }

            return computed;
        }
    }
}