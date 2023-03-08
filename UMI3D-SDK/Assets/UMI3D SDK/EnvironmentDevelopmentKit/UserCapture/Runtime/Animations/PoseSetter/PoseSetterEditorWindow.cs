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

namespace umi3d.edk.userCapture
{
    public class PoseSetterEditorWindow : EditorWindow
    {
        VisualElement root = null;
        TextField name = null;
        DropdownField loa_dropdown = null;
        TextField path = null;
        UMI3DObjectField object_field = null;

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
            object_field = root.Q<UMI3DObjectField>("object_field");

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
        public BoneTreeView(TreeViewItem<TreeElement> elements, TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Bones"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 30,
                    minWidth = 30,
                    maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("IsSelectedAsRoot"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 30,
                    minWidth = 30,
                    maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("IsSelectedForModifications"),
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 30,
                    minWidth = 30,
                    maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                }
            };

            var state = new MultiColumnHeaderState(columns);
            return state;
        }


        protected override TreeViewItem BuildRoot()
        {

            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            var animals = new TreeViewItem { id = 1, displayName = "Animals" };
            var mammals = new TreeViewItem { id = 2, displayName = "Mammals" };
            var tiger = new TreeViewItem { id = 3, displayName = "Tiger" };
            var elephant = new TreeViewItem { id = 4, displayName = "Elephant" };
            var okapi = new TreeViewItem { id = 5, displayName = "Okapi" };
            var armadillo = new TreeViewItem { id = 6, displayName = "Armadillo" };
            var reptiles = new TreeViewItem { id = 7, displayName = "Reptiles" };
            var croco = new TreeViewItem { id = 8, displayName = "Crocodile" };
            var lizard = new TreeViewItem { id = 9, displayName = "Lizard" };

            root.AddChild(animals);
            animals.AddChild(mammals);
            animals.AddChild(reptiles);
            mammals.AddChild(tiger);
            mammals.AddChild(elephant);
            mammals.AddChild(okapi);
            mammals.AddChild(armadillo);
            reptiles.AddChild(croco);
            reptiles.AddChild(lizard);
            reptiles.AddChild(croco);
            reptiles.AddChild(lizard);
            reptiles.AddChild(croco);
            reptiles.AddChild(lizard);
            reptiles.AddChild(croco);
            reptiles.AddChild(croco);
            reptiles.AddChild(lizard);
            reptiles.AddChild(lizard);

            SetupDepthsFromParentsAndChildren(root);

            return root;
        }
    }

    internal class TreeViewItem<T> : TreeViewItem where T : TreeElement
    {
        public T data { get; set; }

        public TreeViewItem(int id, int depth, string displayName, T data) : base(id, depth, displayName)
        {
            this.data = data;
        }
    }
}
#endif
