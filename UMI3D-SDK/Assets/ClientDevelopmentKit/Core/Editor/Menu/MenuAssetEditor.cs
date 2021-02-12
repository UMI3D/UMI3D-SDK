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
using umi3d.cdk.menu;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

#if UNITY_EDITOR
namespace umi3d.cdk.editor
{
    [CustomEditor(typeof(MenuAsset))]
    public class MenuAssetEditor : Editor
    {
        MenuAsset menuAsset;

        // SerializeField is used to ensure the view state is written to the window 
        // layout file. This means that the state survives restarting Unity as long as the window
        // is not closed. If the attribute is omitted then the state is still serialized/deserialized.
        [SerializeField] TreeViewState m_TreeViewState;
        SimpleTreeView treeView;


        void OnEnable()
        {
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

            treeView = new SimpleTreeView(m_TreeViewState);
        }

        ///<inheritdoc/>
        public override void OnInspectorGUI()
        {
            menuAsset = target as MenuAsset;

            if ((menuAsset != null) && (menuAsset.menu != null))
            {
                List<TreeViewItem> list = new List<TreeViewItem>();
                list = toList(menuAsset.menu, 0, 1);
                treeView.SetItems(list);
                treeView.Reload();
                treeView.OnGUI(EditorGUILayout.GetControlRect(true, 500));
            }
            else
            {
                EditorGUILayout.LabelField("No data");
            }

        }

        List<TreeViewItem> toList(umi3d.cdk.menu.Menu menu, int depth, int id)
        {
            List<TreeViewItem> list = new List<TreeViewItem>();
            int localID = id;

            list.Add(new TreeViewItem(localID, depth, menu.Name));
            localID++;


            foreach (umi3d.cdk.menu.Menu submenu in menu.GetSubMenu())
            {
                List<TreeViewItem> subList = toList(submenu, depth + 1, localID);
                foreach (TreeViewItem item in subList)
                    list.Add(item);
                localID += subList.Count;
            }

            foreach (umi3d.cdk.menu.MenuItem item in menu.GetMenuItems())
            {
                list.Add(new TreeViewItem(localID, depth + 1, item.Name));
                localID++;
            }

            return list;
        }

    }
}
#endif