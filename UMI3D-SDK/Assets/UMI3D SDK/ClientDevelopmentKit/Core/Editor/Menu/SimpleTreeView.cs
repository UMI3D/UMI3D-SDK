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
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

#if UNITY_EDITOR
namespace umi3d.cdk.editor
{
    internal class SimpleTreeView : TreeView
    {
        public IList<TreeViewItem> content = new List<TreeViewItem>();

        public SimpleTreeView(TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();
        }

        ///<inheritdoc/>
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            IList<TreeViewItem> allItems = content;

            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
        }

        public void SetItems(IList<TreeViewItem> rows)
        {
            content = rows;
        }
    }
}
#endif