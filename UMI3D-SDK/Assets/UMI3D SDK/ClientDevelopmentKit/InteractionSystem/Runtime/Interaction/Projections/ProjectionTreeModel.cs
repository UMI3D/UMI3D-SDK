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

using System.Collections.Generic;

namespace umi3d.cdk.interaction
{
    public class ProjectionTreeModel 
    {
        debug.UMI3DLogger logger = new(mainTag: nameof(ProjectionTreeModel));

        public ProjectionTree_SO projectionTree_SO;

        public ProjectionTreeModel(
            ProjectionTree_SO projectionTree_SO,
            string treeId
        )
        {
            this.projectionTree_SO = projectionTree_SO;
            projectionTree_SO.treeId = treeId;
        }

        public int IndexOf(ulong nodeId)
        {
            return projectionTree_SO.nodes.FindIndex(node =>
            {
                return node.id == nodeId;
            });
        }

        public int IndexOfChildInParent(ProjectionTreeNodeData parent, ulong child)
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

            return IndexOfChildInParent(projectionTree_SO.nodes[parentIndex], child) != -1;
        }

        /// <summary>
        /// Breadth-first search to get all sub nodes of the <paramref name="parent"/> nodes.<br/>
        /// 
        /// <b>WARNING:</b> Please avoid calling this field too often as it is slow to compute.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public List<ProjectionTreeNodeData> GetAllSubNodes(ProjectionTreeNodeData parent)
        {
            List<ProjectionTreeNodeData> buffer = new();

            Queue<List<ProjectionTreeNodeData>> queue = new();
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

        public bool AddRoot(ProjectionTreeNodeData root)
        {
            projectionTree_SO.root = root;
            projectionTree_SO.nodes.Add(root);
            return true;
        }

        /// <summary>
        /// Add a child to this parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child">Node to add</param>
        public bool AddChild(ulong parent, ProjectionTreeNodeData child)
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

            var parentNode = projectionTree_SO.nodes[parentIndex];
            if (parentNode.children == null)
            {
                parentNode.children = new();
            }

            child.parentId = parent;
            parentNode.children.Add(child);

            var childIndex = IndexOf(child.id);
            if (childIndex == -1)
            {
                projectionTree_SO.nodes.Add(child);
            }
            else
            {
                projectionTree_SO.nodes[childIndex] = child;
            }
            projectionTree_SO.nodes[parentIndex] = parentNode;
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
                projectionTree_SO.nodes.RemoveAt(childIndex);
            }

            var parentNode = projectionTree_SO.nodes[parentIndex];
            parentNode.children?.RemoveAll(_child =>
            {
                return _child.id == child;
            });

            return true;
        }
    }
}