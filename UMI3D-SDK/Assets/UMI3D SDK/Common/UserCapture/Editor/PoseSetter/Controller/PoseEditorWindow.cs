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

using inetum.unityUtils;
using System;
using System.Linq;
using umi3d.common.userCapture.description;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace umi3d.common.userCapture.pose.editor
{
    public class PoseEditorWindow : EditorWindow
    {

        #region ViewModel Attributes

        //Main INfo
        VisualElement root;

        VisualElement create_load_container;

        Button create_button;

        CustomObjectField so_field;
        Button load_button;

        VisualElement editor_container;

        TextField pose_name_field;
        TextField pose_path_field;
        Button save_button;

        //ControlsButtons
        DropdownField symmetry_dropdown;

        Button symmetry_from_left_button;
        Button symmetry_from_right_button;

        CustomObjectField skeleton_object_field;
        Button reset_skeleton;

        private DropdownField hand_closure_dropdown;

        private class SliderMemory
        {
            public float lastLeftValue;
            public float lastRightValue;
        }

        private Slider thumb_slider;
        private readonly SliderMemory thumbSliderMemory = new();

        private Slider index_slider;
        private readonly SliderMemory indexSliderMemory = new();

        private Slider medial_group_slider;
        private readonly SliderMemory medialGroupSliderMemory = new();


        //Bones
        IMGUIContainer bone_container;
        BoneTreeView treeView;

        #endregion

        private readonly PoseEditor poseEditor;

        public PoseEditorWindow() : base()
        {
            poseEditor = new PoseEditor();
        }

        /// <summary>
        /// Open the tool 
        /// </summary>
        [MenuItem("UMI3D/Pose Editor")]
        public static void ShowWindow()
        {
            PoseEditorWindow wnd = GetWindow<PoseEditorWindow>();
            wnd.titleContent = new GUIContent("Pose Editor");
            wnd.maxSize = new Vector2(350, 650);
        }

        #region Initialisation 
        // (Every thing that goes onEnable)

        /// <summary>
        /// Creates the UI using th UI_element framework
        /// </summary>
        private void OnEnable()
        {
            VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("PoseSetterEditorWindow");

            visualTreeAsset.CloneTree(rootVisualElement);
            BindFields();
            BindUI();
        }

        UnityEngine.Object selected;

        private void OnSelectionChange()
        {
            if (CheckIfIsBoneComponent(Selection.activeObject))
            {
                selected = Selection.activeObject;
                Tools.current = Tool.Rotate;
            }
        }

        private void OnInspectorUpdate()
        {
            if (Tools.current != Tool.Rotate)
            {
                if (CheckIfIsBoneComponent(selected))
                    Tools.current = Tool.Rotate;
            }
        }

        private bool CheckIfIsBoneComponent(UnityEngine.Object obj)
        {
            if (Selection.activeObject is GameObject go)
            {
                if (go.GetComponent<PoseSetterBoneComponent>() != null)
                    return true;
            }
            return false;
        }

        private void BindUI()
        {
            BindButtons();
            pose_name_field.RegisterValueChangedCallback((changeEvent) => UpdateSaveButtonAvailability(changeEvent.newValue));
            SetOnGUIContainer();
            InitSkeletonField();
            FillSymmetryDropdownField();
            FillHandClosure();
        }

        /// <summary>
        /// Gets all the references to all the UI_elements in the UXML file
        /// </summary>
        private void BindFields()
        {
            root = rootVisualElement;

            skeleton_object_field = root.Q<CustomObjectField>("object_field");

            create_load_container = root.Q<VisualElement>("CreateLoadContainer");

            create_button = root.Q<Button>("CreateButton");

            so_field = root.Q<CustomObjectField>("so_field");
            load_button = root.Q<Button>("load");

            editor_container = root.Q<VisualElement>("EditorContainer");

            pose_name_field = root.Q<TextField>("name");
            pose_path_field = root.Q<TextField>("path");
            save_button = root.Q<Button>("save");

            symmetry_dropdown = root.Q<DropdownField>("symmetry_dropdown");
            symmetry_from_left_button = root.Q<Button>("btn_from_left");
            symmetry_from_right_button = root.Q<Button>("btn_from_right");
            reset_skeleton = root.Q<Button>("reset_skeleton");

            hand_closure_dropdown = root.Q<DropdownField>("HandClosureDropdown");
            thumb_slider = root.Q<Slider>("thumb_slider");
            index_slider = root.Q<Slider>("index_slider");
            medial_group_slider = root.Q<Slider>("medial_group_slider");

            bone_container = root.Q<IMGUIContainer>("bone_container");

            Assert.IsNotNull(pose_name_field);
            Assert.IsNotNull(pose_path_field);
            Assert.IsNotNull(skeleton_object_field);
            Assert.IsNotNull(so_field);
            Assert.IsNotNull(symmetry_dropdown);
            Assert.IsNotNull(symmetry_from_left_button);
            Assert.IsNotNull(symmetry_from_right_button);
            Assert.IsNotNull(save_button);
            Assert.IsNotNull(load_button);
            Assert.IsNotNull(reset_skeleton);
            Assert.IsNotNull(hand_closure_dropdown);
            Assert.IsNotNull(thumb_slider);
            Assert.IsNotNull(index_slider);
            Assert.IsNotNull(medial_group_slider);
            Assert.IsNotNull(bone_container);
        }

        /// <summary>
        /// Inits the objects fields to be able to filter the drag n dropped files 
        /// Also it sets up the change value event to make sure that when de files are changed the tool updates
        /// </summary>
        private void InitSkeletonField()
        {
            skeleton_object_field.Init(typeof(GameObject));
            // TODO ==> add a call back where you make sure to clean all the roots and the savable and the selected on the skeleton
            skeleton_object_field.RegisterValueChangedCallback(changeEvent =>
            {
                if (editor_container.resolvedStyle.display == DisplayStyle.None
                    && create_load_container.resolvedStyle.display == DisplayStyle.None)
                    DisplayCreateLoadContainer();

                poseEditor.UpdateSkeletonGameObject(changeEvent.newValue as GameObject);
                var treeViewItems = poseEditor.Skeleton.boneComponents.Select(bc => GetBoneTreeViewItem(bc)).ToList();

                treeView.UpdateTreeView(treeViewItems);
            });

            so_field.Init(typeof(UMI3DPose_so));
        }

        /// <summary>
        /// Sets up the IMGUI container, basically it only contains the Tree view
        /// </summary>
        private void SetOnGUIContainer()
        {
            var m_TreeViewState = new TreeViewState();
            treeView = new BoneTreeView(m_TreeViewState);

            bone_container.onGUIHandler = () =>
            {
                treeView.OnGUI(new Rect(0, 0, position.width, position.height));

                bone_container.style.height = treeView.totalHeight;

                treeView.OnGUI(GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)));
            };
        }

        private void FillSymmetryDropdownField()
        {
            symmetry_dropdown.choices.RemoveAt(0);
            Enum.GetNames(typeof(SymmetryTarget)).ForEach(name =>
            {
                symmetry_dropdown.choices.Add(name);
            });
            symmetry_dropdown.value = symmetry_dropdown.choices[0];
        }

        private void FillHandClosure()
        {
            hand_closure_dropdown.choices.Clear();
            hand_closure_dropdown.choices.Add(BoneTypeHelper.GetBoneName(BoneType.RightHand));
            hand_closure_dropdown.choices.Add(BoneTypeHelper.GetBoneName(BoneType.LeftHand));
            hand_closure_dropdown.value = BoneTypeHelper.GetBoneName(BoneType.RightHand);

            hand_closure_dropdown.RegisterValueChangedCallback((changeEvent) => SetHandClosureHand(changeEvent.newValue));
            thumb_slider.RegisterValueChangedCallback((changeEvent) => SetThumbClosure(changeEvent.newValue));
            index_slider.RegisterValueChangedCallback((changeEvent) => SetIndexClosure(changeEvent.newValue));
            medial_group_slider.RegisterValueChangedCallback((changeEvent) => SetMedialGroupClosure(changeEvent.newValue));
        }

        /// <summary>
        /// Binds All the buttons 
        /// </summary>
        private void BindButtons()
        {
            create_button.clicked += CreatePose;
            save_button.clicked += SavePose;
            load_button.clicked += LoadPose;
            reset_skeleton.clicked += ResetSkeleton;
            symmetry_from_left_button.clicked += () => ApplySymmetry(isLeft: true);
            symmetry_from_right_button.clicked += () => ApplySymmetry(isLeft: false);
        }

        public void UpdateSaveButtonAvailability(string name)
        {
            if (name != string.Empty && !save_button.enabledSelf)
                save_button.SetEnabled(true);
            else if (name == string.Empty && save_button.enabledSelf)
                save_button.SetEnabled(false);
        }

        public void DisplayCreateLoadContainer()
        {
            create_load_container.style.display = DisplayStyle.Flex;
            editor_container.style.display = DisplayStyle.None;
        }

        public void DisplayEditorContainer()
        {
            editor_container.style.display = DisplayStyle.Flex;
            create_load_container.style.display = DisplayStyle.None;
        }

        #endregion

        #region Control

        private void CreatePose()
        {
            poseEditor.CreatePose();
            DisplayEditorContainer();
            save_button.SetEnabled(false);
        }

        private void SavePose()
        {
            poseEditor.SavePose(pose_name_field.value, pose_path_field.value);
        }

        private void LoadPose()
        {
            var pose = so_field.value as UMI3DPose_so;
            poseEditor.LoadPose(pose, out bool success);

            PoseSetterBoneComponent root_boneComponent = poseEditor.Skeleton.boneComponents.Find(bc => bc.BoneType == pose.GetBonePoseCopy().bone);
            treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(true, root_boneComponent.BoneType);
            pose_name_field.value = pose.name;
            this.pose_path_field.value = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(pose));
            DisplayEditorContainer();
            UpdateRootsInEditorWindow();
        }

        private void ResetSkeleton()
        {
            poseEditor.ResetSkeleton(skeleton_object_field.value as GameObject);
        }

        private void ApplySymmetry(bool isLeft)
        {
            if (skeleton_object_field.value == null)
                return;

            SymmetryTarget target = Enum.Parse<SymmetryTarget>(symmetry_dropdown.value);
            poseEditor.ApplySymmetry(isLeft, target);
        }

        private uint handClosureBoneType = BoneType.RightHand;

        private void SetHandClosureHand(string handName)
        {
            handClosureBoneType = BoneTypeHelper.GetBoneNames().First(x => x.Value == handName).Key;

            thumb_slider.value = (handClosureBoneType == BoneType.RightHand) ? thumbSliderMemory.lastRightValue : thumbSliderMemory.lastLeftValue;
            index_slider.value = (handClosureBoneType == BoneType.RightHand) ? indexSliderMemory.lastRightValue : indexSliderMemory.lastLeftValue;
            medial_group_slider.value = (handClosureBoneType == BoneType.RightHand) ? medialGroupSliderMemory.lastRightValue : medialGroupSliderMemory.lastLeftValue;
        }

        private void SetThumbClosure(float rate)
        {
            if (handClosureBoneType == BoneType.RightHand)
                thumbSliderMemory.lastRightValue = rate;
            else
                thumbSliderMemory.lastLeftValue = rate;

            poseEditor.CloseFinger(handClosureBoneType, HandClosureGroup.THUMB, rate);
        }

        private void SetIndexClosure(float rate)
        {
            if (handClosureBoneType == BoneType.RightHand)
                indexSliderMemory.lastRightValue = rate;
            else
                indexSliderMemory.lastLeftValue = rate;

            poseEditor.CloseFinger(handClosureBoneType, HandClosureGroup.INDEX, rate);
        }

        private void SetMedialGroupClosure(float rate)
        {
            if (handClosureBoneType == BoneType.RightHand)
                medialGroupSliderMemory.lastRightValue = rate;
            else
                medialGroupSliderMemory.lastLeftValue = rate;

            poseEditor.CloseFinger(handClosureBoneType, HandClosureGroup.MEDIAL_GROUP, rate);
        }

        #endregion

        #region TreeView utils

        private TreeViewItem<BoneTreeElement> GetBoneTreeViewItem(PoseSetterBoneComponent bc)
        {
            BoneTreeElement boneTreeElement = new(bc.isRoot);
            string boneName = bc.name.Split(":")[1]; // this is a WIP line because the skeleton has "Mixamo:" every where as prefix
            TreeViewItem<BoneTreeElement> boneTreeViewItem = new((int)bc.BoneType, 1, boneName, boneTreeElement);

            boneTreeElement?.RootChanged.AddListener((e) => poseEditor.UpdateIsRoot(bc.BoneType, e.boolValue));

            return boneTreeViewItem;
        }

        private void UpdateRootsInEditorWindow()
        {
            foreach (var boneComponent in poseEditor.Skeleton.boneComponents)
            {
                treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(boneComponent.isRoot, boneComponent.BoneType);
            }
        }

        #endregion

    }
}
#endif
