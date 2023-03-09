/*
Copy 2019 - 2023 Inetum

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

using inetum.unityUtils.editor;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Configuration;
using umi3d.common.userCapture;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.IMGUI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.TreeViewExamples;
using inetum.unityUtils;
using umi3d.edk.userCapture;

namespace intetum.unityUtils
{

    public class PoseSetterEditorWindow : EditorWindow
    {
        VisualElement root = null;
        TextField name = null;
        DropdownField loa_dropdown = null;
        TextField path = null;
        CustomObjectField object_field = null;
        CustomObjectField so_field = null;

        ListView lv_root_selected = null;
        Button add_root = null;
        Button remove_root = null;
        Button save = null;

        TextField filter = null;
        IMGUIContainer bone_container = null;

        Slider x_rot_slider = null;
        Slider y_rot_slider = null;
        Slider z_rot_slider = null;
        /// <summary>
        /// Open the tool 
        /// </summary>
        [MenuItem("UMI3D/PoseSetter")]
        public static void ShowExample()
        {
            PoseSetterEditorWindow wnd = GetWindow<PoseSetterEditorWindow>();
            wnd.titleContent = new GUIContent("PoseSetter");
            wnd.minSize = new Vector2(1200, 720);
        }

        PoseDto_writter poseDto_Writter = new PoseDto_writter();

        #region Initialisation
        /// <summary>
        /// Creates the UI using th UI_element framework
        /// </summary>
        public void OnEnable()
        {
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                                        "Assets\\UMI3D SDK\\EnvironmentDevelopmentKit\\UserCapture\\Runtime\\Animations\\PoseSetter\\PoseSetterEditorWindow.uxml"
                                   );
            uxml.CloneTree(rootVisualElement);
            GetAllRefs();
            InitTextFields();
            BindButtons();
            SetOnGUIContainer();
            InitSliders();
            InitObjectField();
        }

        private void GetAllRefs()
        {
            root = rootVisualElement;
            name = root.Q<TextField>("name");
            loa_dropdown = root.Q<DropdownField>("loa_dropdown");
            path = root.Q<TextField>("path");
            object_field = root.Q<CustomObjectField>("object_field");
            so_field = root.Q<CustomObjectField>("so_field");

            lv_root_selected = root.Q<ListView>("lv_root_selected");
            add_root = root.Q<Button>("add_root");
            remove_root = root.Q<Button>("remove_root");
            save = root.Q<Button>("save");

            filter = root.Q<TextField>("filter");
            bone_container = root.Q<IMGUIContainer>("bone_container");

            x_rot_slider = root.Q<Slider>("x_rot_slider");
            y_rot_slider = root.Q<Slider>("y_rot_slider");
            z_rot_slider = root.Q<Slider>("z_rot_slider");
        }

        private void InitObjectField()
        {
            object_field.Init(typeof(GameObject));
            object_field.RegisterValueChangedCallback(value => { ReadHierachy(value); });

            //so_field.Init()

        }

        private void SetOnGUIContainer()
        {
            TreeViewState m_TreeViewState = new TreeViewState();
            BoneTreeView treeView = new BoneTreeView(null, m_TreeViewState);

            bone_container.onGUIHandler = () =>
            {
                treeView.OnGUI(new Rect(0, 0, position.width, position.height));

                bone_container.style.height = treeView.totalHeight;

                EditorGUI.BeginChangeCheck();
                {

                }
                if (EditorGUI.EndChangeCheck())
                {

                }
                treeView.OnGUI(GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)));
            };
        }

        private void BindButtons()
        {
            add_root.clicked += () => AddAnEmptyRootToListView();
            remove_root.clicked += () => RemoveLastRootFromListView();
            save.clicked += () => SaveToScriptableObjectAtPath();
        }

        Transform selectedBone = null;

        private void InitSliders()
        {
            x_rot_slider.value = 0;
            y_rot_slider.value = 0;
            z_rot_slider.value = 0;

            x_rot_slider.RegisterValueChangedCallback(value => { ChangeXRotationOfSelectedBone(value); });
            y_rot_slider.RegisterValueChangedCallback(value => { ChangeYRotationOfSelectedBone(value); });
            z_rot_slider.RegisterValueChangedCallback(value => { ChangeZRotationOfSelectedBone(value); });
        }

        private void InitTextFields()
        {

        }

        #endregion

        private void ReadHierachy(ChangeEvent<UnityEngine.Object> value)
        {

        }

        private void AddAnEmptyRootToListView()
        {

        }

        private void RemoveLastRootFromListView()
        {

        }

        private void SaveToScriptableObjectAtPath()
        {

        }

        #region change bone rotation
        private void ChangeZRotationOfSelectedBone(ChangeEvent<float> value)
        {
            if (selectedBone != null)
            {
                Vector3 rotation = selectedBone.rotation.eulerAngles;
                rotation.x = value.newValue;
                Quaternion quaternion = Quaternion.Euler(rotation);
                selectedBone.rotation = quaternion;
            }
        }

        private void ChangeYRotationOfSelectedBone(ChangeEvent<float> value)
        {
            if (selectedBone != null)
            {
                Vector3 rotation = selectedBone.rotation.eulerAngles;
                rotation.y = value.newValue;
                Quaternion quaternion = Quaternion.Euler(rotation);
                selectedBone.rotation = quaternion;
            }
        }

        private void ChangeXRotationOfSelectedBone(ChangeEvent<float> value)
        {
            if (selectedBone != null)
            {
                Vector3 rotation = selectedBone.rotation.eulerAngles;
                rotation.z = value.newValue;
                Quaternion quaternion = Quaternion.Euler(rotation);
                selectedBone.rotation = quaternion;
            }
        }
        #endregion
    }

    class BoneTreeView : TreeView
    {
        public BoneTreeView(List<TreeViewItem<BoneTreeElement>> elements, TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();
            headerState = CreateDefaultMultiColumnHeaderState();

            MultiColumnHeader multiColumnHeader = new MultiColumnHeader(headerState);
            multiColumnHeader.ResizeToFit();
            multiColumnHeader.SetSorting(0, true);

            this.multiColumnHeader = multiColumnHeader;

            this.elements = elements;
        }

        List<TreeViewItem<BoneTreeElement>> elements = new List<TreeViewItem<BoneTreeElement>>();
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
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("IsSelected"),
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
            BoneTreeElement boneTreeElement_1 = new BoneTreeElement(false, false);
            BoneTreeElement boneTreeElement_2 = new BoneTreeElement(false, false);
            BoneTreeElement boneTreeElement_3 = new BoneTreeElement(false, false);

            var root = new TreeViewItem<BoneTreeElement>( 0,  -1,"Root", boneTreeElement_1);
            var hips = new TreeViewItem<BoneTreeElement>(0, -1, "hips", boneTreeElement_2);
            var arm = new TreeViewItem<BoneTreeElement>(0, -1, "arm", boneTreeElement_3);

            root.AddChild(hips);
            hips.AddChild(arm);

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
                    EditorGUI.LabelField(cellRect, "     "+item.data.name, EditorStyles.boldLabel);
                }
                else if (col == 1)
                {
                    Rect toggleRect = new Rect(cellRect.center.x - 8f, cellRect.y, 16f, cellRect.height);
                    bool newValue = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, item.data.isRoot);
                    if (newValue != item.data.isRoot)
                    {
                        item.data.isRoot = newValue;
                        //event ? 
                    }
                }
                else if (col == 2)
                {
                    Rect toggleRect = new Rect(cellRect.center.x - 8f, cellRect.y, 16f, cellRect.height);
                    bool newValue = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, item.data.isSelected);
                    if (newValue != item.data.isSelected)
                    {
                        item.data.isSelected = newValue;
                        //event ? 
                    }
                }
            }
        }

        internal class TreeViewItem<T> : TreeViewItem where T : TreeElement
        {
            public T data { get; set; }

            public TreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
            {
                this.data = data;
                this.data.name = displayName;
            }
        }
    }
}
#endif
