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

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;
using UnityEditor;
using UnityEngine;

namespace umi3d.common.userCapture
{
    class BoneTreeView : TreeView
    {
        public BoneTreeView(TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();

            headerState = CreateDefaultMultiColumnHeaderState();

            MultiColumnHeader multiColumnHeader = new MultiColumnHeader(headerState);
            multiColumnHeader.ResizeToFit();
            multiColumnHeader.SetSorting(0, true);

            this.multiColumnHeader = multiColumnHeader;
        }

        /// <summary>
        /// Change the treview hierachy with the new items
        /// </summary>
        /// <param name="elements"></param>
        public void UpdateTreeView(List<TreeViewItem<BoneTreeElement>> elements)
        {
            this.elements = elements;
            Reload();
        }

        List<TreeViewItem<BoneTreeElement>> elements = new List<TreeViewItem<BoneTreeElement>>();

        /// <summary>
        /// Method to update a is root toggle but without sending the event to the editor window
        /// Use this method when you are making changes to the value outside of the tree view
        /// </summary>
        /// <param name="value">new vale of the bool</param>
        /// <param name="id">bone id of the looked for boneElement</param>
        public void UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(bool value, uint id)
        {
            var element = elements.Find(e => e.id == id);
            if (element is null)
                return;
            element.data.isRoot = value;
        }

        private MultiColumnHeaderState headerState;

        private MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Bones"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 150,
                    minWidth = 30,
                    maxWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("IsRoot"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                     width = 60,
                    minWidth = 60,
                    maxWidth = 120,
                    autoResize = true,
                    allowToggleVisibility = true
                }
            };

            var state = new MultiColumnHeaderState(columns);
            return state;
        }

        protected override TreeViewItem BuildRoot()
        {
            // TODO --> Build a real hierarchy
            TreeViewItem<BoneTreeElement> root = new TreeViewItem<BoneTreeElement>(0, -1, "Root", new BoneTreeElement(false));
            TreeViewItem<BoneTreeElement> skeletonRoot = new TreeViewItem<BoneTreeElement>(0, 0, "Root", new BoneTreeElement(false));

            root.AddChild(skeletonRoot);

            elements?.ForEach(e =>
            {
                skeletonRoot.AddChild(e);
            });

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }


        protected override void RowGUI(RowGUIArgs args)
        {
            TreeViewItem<BoneTreeElement> item = (TreeViewItem<BoneTreeElement>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                Rect cellRect = args.GetCellRect(i);
                int col = args.GetColumn(i);

                if (col == 0)
                {
                    EditorGUI.LabelField(cellRect, "     " + item.data.name, EditorStyles.boldLabel);
                }
                else if (col == 1)
                {
                    Rect toggleRect = new Rect(cellRect.center.x - 8f, cellRect.y, 16f, cellRect.height);
                    bool newValue = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, item.data.isRoot);
                    if (newValue != item.data.isRoot)
                    {
                        item.data.isRoot = newValue;
                        item.data.RootChanged?.Invoke(new BoneTreeElement.ChangeData { boneTreeElements = item.data, itemID = item.id, boolValue = newValue });
                    }
                }
            }
        }
    }

    public class TreeViewItem<T> : TreeViewItem where T : TreeElement
    {
        public T data { get; set; }

        public TreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
        {
            this.data = data;
            this.data.name = displayName;
        }
    }
}
#endif
