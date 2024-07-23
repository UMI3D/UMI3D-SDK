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

        public IUMI3DSkeletonHierarchyDefinition Definition { get; private set; }

        public UMI3DSkeletonHierarchy(IUMI3DSkeletonHierarchyDefinition definition, IUMI3DSkeletonMusclesDefinition musclesDefinition = null)
        {
            hierarchicalComparer = new BoneHierarchicalComparer(this);
            Definition = definition;

            if (definition == null || definition.Relations.Count == 0) //empty hierarchy has at least a hips
            {
                relations.Add(BoneType.Hips, new() { boneType = BoneType.None, relativePosition = Vector3.zero });
                linearDepthOrderedBones = new List<uint>();
                return;
            }

            var relationGroupings = definition.Relations.GroupBy(x => x.parentBoneType).ToDictionary(x => x.Key, x => x.ToList());

            // find root
            uint rootBoneType = FindRoots(definition);

            // create hierarchy of nodes
            List<uint> depthOrderedBones = new List<uint>() { rootBoneType };
            root = CreateHierarchyNode(BoneType.None, rootBoneType, relationGroupings[rootBoneType]);
            linearDepthOrderedBones = depthOrderedBones;
            relations.Add(rootBoneType, root);

            // manages muscles
            if (musclesDefinition?.Muscles != null)
            {
                foreach (var muscle in musclesDefinition?.Muscles)
                {
                    if (muscles.ContainsKey(muscle.Bonetype))
                        continue;

                    muscles.Add(muscle.Bonetype, muscle);
                }
            }

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

                    depthOrderedBones.Append(childRelation.boneType);

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

        private readonly Dictionary<uint, IUMI3DSkeletonMusclesDefinition.Muscle> muscles = new();
        public IReadOnlyDictionary<uint, IUMI3DSkeletonMusclesDefinition.Muscle> Muscles => muscles;


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

        public (uint root, IDictionary<uint, uint[]> nodes) Tree
        {
            get
            {
                if (!isTreeComputed)
                    ToTree();

                return computedTree;
            }
        }

        private (uint root, IDictionary<uint, uint[]> nodes) computedTree;
        private bool isTreeComputed;

        private (uint root, IDictionary<uint, uint[]> nodes) ToTree()
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

        public uint[] GetParents(uint bone)
        {
            List<uint> boneParents = new();

            uint currentBone = bone;

            while (currentBone != Tree.root && currentBone != BoneType.None)
            {
                if (!relations.TryGetValue(currentBone, out var relation))
                    break;

                uint parentBone = relation.boneTypeParent;
                boneParents.Add(parentBone);
                currentBone = parentBone;
            }

            return boneParents.ToArray();
        }

        /// <summary>
        /// Compare two bones within the hierarchy.
        /// </summary>
        /// <param name="bone1"></param>
        /// <param name="bone2"></param>
        /// <returns>
        /// -1 if bone1 is a child of bone2. <br/>
        /// +1 if bone1 is a parent of bone2. <br/>
        /// 0 otherwise.
        /// </returns>
        public int CompareBones(uint bone1, uint bone2)
        {
            if (bone1 == bone2)
                return 0;

            if (GetParents(bone1).Contains(bone2))
                return -1;

            if (GetParents(bone2).Contains(bone1))
                return 1;

            return 0;
        }

        private readonly IComparer<uint> hierarchicalComparer;

        public class BoneHierarchicalComparer : IComparer<uint>
        {
            private UMI3DSkeletonHierarchy hierarchy;

            public BoneHierarchicalComparer(UMI3DSkeletonHierarchy hierarchy)
            {
                this.hierarchy = hierarchy;
            }

            public int Compare(uint x, uint y)
            {
                return hierarchy.CompareBones(x, y);
            }
        }

        public IComparer<uint> Comparer => hierarchicalComparer;

        public uint GetHighestBone(IEnumerable<uint> bones)
        {
            return bones.OrderBy(x=>x, hierarchicalComparer).LastOrDefault();
        }

        public uint GetHighestBone(params uint[] bones)
        {
            return GetHighestBone(bones);
        }

        /// <summary>
        /// Recursive <paramref name="action"/> applied on all children from a certain <paramref name="startBone"/>.
        /// </summary>
        /// <param name="action">Action to perform on each bone. Cannot be null.</param>
        /// <param name="startBone">Bone from where to start recursive descending application. Cannot be None bone type.</param>
        public void Apply(System.Action<uint> action, uint startBone)
        {
            if (action == null)
                throw new System.ArgumentNullException(nameof(action));

            if (startBone is BoneType.None || !Relations.ContainsKey(startBone))
                return;

            Recurse(startBone);

            // recursive on the hierarchy
            void Recurse(uint bone)
            {
                action(bone);

                if (!Relations.TryGetValue(bone, out var relation))
                    return;

                var children = relation.children;
                if (children is null || children.Count == 0) // stop condition, require no loop in hierarchy (ensured by construction)
                    return;

                foreach (var hierarchyNode in children)
                {
                    Recurse(hierarchyNode.boneType);
                }
            }
        }

        /// <summary>
        /// Bones, ordered by depth.
        /// </summary>
        public IReadOnlyList<uint> OrderedBones
        {
            get => linearDepthOrderedBones;
            set
            {
                if (value is null)
                    throw new System.ArgumentNullException();

                linearDepthOrderedBones = value;
                _linearReverseDepthOrderedBones = linearDepthOrderedBones.Reverse().ToList();
            }
        }

        private IReadOnlyList<uint> linearDepthOrderedBones;

        private IReadOnlyList<uint> LinearReverseDepthOrderedBones
        {
            get
            {
                if (_linearReverseDepthOrderedBones == null)
                    _linearReverseDepthOrderedBones = linearDepthOrderedBones.Reverse().ToList();
                return _linearReverseDepthOrderedBones;
            }
        }
        private IReadOnlyList<uint> _linearReverseDepthOrderedBones;

        /// <summary>
        /// Apply an <paramref name="action"/> to all bones, starting from the hips.
        /// </summary>
        /// <param name="action">Action to perform on each bone. Cannot be null.</param>
        /// <param name="startBone">Bone from where to start recursive descending application. Cannot be None bone type.</param>
        public void Apply(System.Action<uint> action)
        {
            if (action == null)
                throw new System.ArgumentNullException(nameof(action));

            foreach (uint bone in OrderedBones)
            {
                action(bone);
            }
        }

        /// <summary>
        /// Apply an <paramref name="action"/> to all bones, starting from the lowest bones.
        /// </summary>
        /// <param name="action">Action to perform on each bone. Cannot be null.</param>
        /// <param name="startBone">Bone from where to start recursive descending application. Cannot be None bone type.</param>
        public void ApplyReverse(System.Action<uint> action)
        {
            if (action == null)
                throw new System.ArgumentNullException(nameof(action));

            foreach (uint bone in LinearReverseDepthOrderedBones)
            {
                action(bone);
            }
        }
    }
}