/*
Copyright 2019 - 2024 Inetum

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

using System;
using System.Collections.Generic;
using umi3d.common;
using umi3d.debug;
using UnityEngine;

namespace umi3d.cdk.interaction
{
    public class ProjectionTreeModel 
    {
        debug.UMI3DLogger logger = new(mainTag: nameof(ProjectionTreeModel));

        public ProjectionTree_SO projectionTree_SO;
        /// <summary>
        /// Tree's id the node belongs to.
        /// </summary>
        public string treeId;

        public ProjectionTreeModel(
            ProjectionTree_SO projectionTree_SO, 
            string treeId
        )
        {
            this.projectionTree_SO = projectionTree_SO;
            this.treeId = treeId;
        }

        public List<ProjectionTreeNodeDto> Nodes
        {
            get
            {
                return projectionTree_SO.trees.Find(tree =>
                {
                    return tree.treeId == treeId;
                }).nodes;
            }
        }

        public int IndexOf(ulong nodeId)
        {
            return Nodes.FindIndex(node =>
            {
                return node.id == nodeId;
            });
        }

        public int IndexOfChildInParent(ProjectionTreeNodeDto parent, ulong child)
        {
            return parent.children?.FindIndex(node =>
            {
                return node.id == child;
            }) ?? -1;
        }

        public bool IsChildOf(ulong parent, ulong child)
        {
            var parentIndex = IndexOf(parent);
            if (parentIndex == -1)
            {
                return false;
            }

            return IndexOfChildInParent(Nodes[parentIndex], child) != -1;
        }

        /// <summary>
        /// Breadth-first search to get all sub nodes of the <paramref name="parent"/> nodes.<br/>
        /// 
        /// <b>WARNING:</b> Please avoid calling this field too often as it is slow to compute.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public List<ProjectionTreeNodeDto> GetAllSubNodes(ProjectionTreeNodeDto parent)
        {
            List<ProjectionTreeNodeDto> buffer = new();

            Queue<List<ProjectionTreeNodeDto>> queue = new();
            queue.Enqueue(parent.children);

            while (queue.Count != 0)
            {
                var level = queue.Dequeue();
                buffer.AddRange(level);

                for (int i = 0; i < (level?.Count ?? 0); i++)
                {
                    queue.Enqueue(level[i].children);
                }
            }

            return buffer;
        }

        public bool AddRoot(ProjectionTreeNodeDto root)
        {
            if (projectionTree_SO.trees.FindIndex(tree =>
            {
                return tree.treeId == root.treeId;
            }) == -1)
            {
                projectionTree_SO.trees.Add(
                    new ProjectionTreeDto()
                    {
                        treeId = root.treeId,
                        root = root,
                        nodes = new()
                    }
                );
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add a child to this parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child">Node to add</param>
        public bool AddChild(ulong parent, ProjectionTreeNodeDto child)
        {
            if (IsChildOf(parent, child.id))
            {
                return true;
            }

            var parentIndex = IndexOf(parent);
            if (parentIndex != -1)
            {
                return false;
            }

            var parentNode = Nodes[parentIndex];
            if (parentNode.children == null)
            {
                parentNode.children = new();
            }

            child.parentId = parent;
            parentNode.children.Add(child);

            var childIndex = IndexOf(child.id);
            if (childIndex == -1)
            {
                Nodes.Add(child);
            }
            else
            {
                Nodes[childIndex] = child;
            }
            Nodes[parentIndex] = parentNode;
            return true;
        }

        public bool RemoveChild(ulong parent, ulong child)
        {
            var parentIndex = IndexOf(parent);
            if (parentIndex != -1)
            {
                return false;
            }

            var childIndex = IndexOf(child);
            if (childIndex != -1)
            {
                Nodes.RemoveAt(childIndex);
            }

            var parentNode = Nodes[parentIndex];
            parentNode.children?.RemoveAll(_child =>
            {
                return _child.id == child;
            });

            return true;
        }
    }
}