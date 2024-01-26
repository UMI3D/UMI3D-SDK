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
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace umi3d.common.userCapture.pose.editor
{
    public class PoseEditorWindow : EditorWindow
    {
        public const string DEFAULT_MAIN_LABEL = "UMI3D Pose Editor";

        #region ViewModel Attributes

        //Main INfo
        private VisualElement root;

        private VisualElement create_load_container;

        private Button create_button;

        private Button load_button;

        private VisualElement editor_container;

        private Label title_label;
        private Button export_button;

        //ControlsButtons
        private DropdownField symmetry_dropdown;

        private Button symmetry_from_left_button;
        private Button symmetry_from_right_button;

        private Button reset_skeleton;

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
        private IMGUIContainer bone_container;

        private BoneTreeView treeView;

        #endregion ViewModel Attributes

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

        // (Every thing that goes onEnable)
        #region Initialisation

        /// <summary>
        /// Creates the UI using th UI_element framework
        /// </summary>
        private void OnEnable()
        {
            VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("PoseSetterEditorWindow");
            visualTreeAsset.CloneTree(rootVisualElement);

            BindUI();
        }

        private UnityEngine.Object selected;

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
            BindFields();
            BindEditionButtons();
            InitSymmetryDropdownField();
            InitHandClosure();
        }

        /// <summary>
        /// Gets all the references to all the UI_elements in the UXML file
        /// </summary>
        private void BindFields()
        {
            root = rootVisualElement;

            create_load_container = root.Q<VisualElement>("CreateLoadContainer");

            create_button = root.Q<Button>("CreateButton");
            load_button = root.Q<Button>("load");

            editor_container = root.Q<VisualElement>("EditorContainer");

            title_label = root.Q<Label>("MainTitle");
            export_button = root.Q<Button>("export");

            symmetry_dropdown = root.Q<DropdownField>("symmetry_dropdown");
            symmetry_from_left_button = root.Q<Button>("btn_from_left");
            symmetry_from_right_button = root.Q<Button>("btn_from_right");
            reset_skeleton = root.Q<Button>("reset_skeleton");

            hand_closure_dropdown = root.Q<DropdownField>("HandClosureDropdown");
            thumb_slider = root.Q<Slider>("thumb_slider");
            index_slider = root.Q<Slider>("index_slider");
            medial_group_slider = root.Q<Slider>("medial_group_slider");

            bone_container = root.Q<IMGUIContainer>("bone_container");

            Assert.IsNotNull(symmetry_dropdown);
            Assert.IsNotNull(symmetry_from_left_button);
            Assert.IsNotNull(symmetry_from_right_button);
            Assert.IsNotNull(export_button);
            Assert.IsNotNull(load_button);
            Assert.IsNotNull(reset_skeleton);
            Assert.IsNotNull(hand_closure_dropdown);
            Assert.IsNotNull(thumb_slider);
            Assert.IsNotNull(index_slider);
            Assert.IsNotNull(medial_group_slider);
            Assert.IsNotNull(bone_container);
        }

        private void InitSymmetryDropdownField()
        {
            symmetry_dropdown.choices.RemoveAt(0);
            Enum.GetNames(typeof(SymmetryTarget)).ForEach(name =>
            {
                symmetry_dropdown.choices.Add(name);
            });
            symmetry_dropdown.value = symmetry_dropdown.choices[0];
        }

        private void InitHandClosure()
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

        private void InitAnchorsTreeView()
        {
            var m_TreeViewState = new TreeViewState();
            treeView = new BoneTreeView(m_TreeViewState);

            bone_container.onGUIHandler = () =>
            {
                treeView.OnGUI(new Rect(0, 0, position.width, position.height));

                bone_container.style.height = treeView.totalHeight;

                treeView.OnGUI(GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)));
            };

            var treeViewItems = poseEditor.Skeleton.boneComponents.Select(bc => GetBoneTreeViewItem(bc)).ToList();

            treeView.UpdateTreeView(treeViewItems);

            treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(true, poseEditor.Skeleton.anchor);
            UpdateRootsInEditorWindow();
        }

        /// <summary>
        /// Binds All the buttons
        /// </summary>
        private void BindEditionButtons()
        {
            create_button.clicked += CreatePose;
            load_button.clicked += LoadPose;

            export_button.clicked += ExportPose;

            reset_skeleton.clicked += ResetSkeleton;

            symmetry_from_left_button.clicked += () => ApplySymmetry(isLeft: true);
            symmetry_from_right_button.clicked += () => ApplySymmetry(isLeft: false);
        }

        #endregion Initialisation

        #region Navigation

        private void CreatePose()
        {
            ChangeDisplayedPoseName(PoseEditorParameters.DEFAULT_UNSAVED_POSE_NAME);
            OpenEditionScene();
            DisplayEditorContainer();
        }

        public void DisplayEditorContainer()
        {
            editor_container.style.display = DisplayStyle.Flex;
            create_load_container.style.display = DisplayStyle.None;
        }

        private void OpenEditionScene()
        {
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(PoseEditorParameters.SKELETON_PREFAB_PATH));

            skeletonGo = GetDefaultSkeleton();
            poseEditor.InitSkeleton(skeletonGo);
            InitAnchorsTreeView();            
        }

        private GameObject GetDefaultSkeleton()
        {
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            return prefabStage.prefabContentsRoot;
        }

        #endregion Navigation

        #region IO

        private void ExportPose()
        {
            string filePath = EditorUtility.SaveFilePanel("Save pose", "", PoseEditorParameters.DEFAULT_POSE_NAME, PoseEditorParameters.POSE_FORMAT_EXTENSION);

            string poseName = System.IO.Path.GetFileName(filePath).Replace($".{PoseEditorParameters.POSE_FORMAT_EXTENSION}", "");

            if (poseName == string.Empty)
                return; // name not entered, no save

            poseEditor.SavePose(filePath);

            ChangeDisplayedPoseName(poseName);
            RemoveUnsavedMark();
        }

        private void LoadPose()
        {
            string path = EditorUtility.OpenFilePanel("Open a pose", "", PoseEditorParameters.POSE_FORMAT_EXTENSION);

            if (path == null || path == string.Empty) // aborted
                return;

            OpenEditionScene();

            poseEditor.LoadPose(path, out bool success);

            if (!success)
                return;

            string poseName = System.IO.Path.GetFileName(path).Replace($".{PoseEditorParameters.POSE_FORMAT_EXTENSION}", "");
            ChangeDisplayedPoseName(poseName);

            DisplayEditorContainer();
            treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(true, poseEditor.Skeleton.anchor);
            UpdateRootsInEditorWindow();
        }

        #endregion IO

        #region Edition

        private uint handClosureBoneType = BoneType.RightHand;
        private GameObject skeletonGo;

        private void ChangeDisplayedPoseName(string newPoseName)
        {
            title_label.text = DEFAULT_MAIN_LABEL + " - " + newPoseName;
        }

        private void AddUnsavedMark()
        {
            if (title_label.text[^1] != '*')
                title_label.text += "*";
        }

        private void RemoveUnsavedMark()
        {
            title_label.text = title_label.text.Replace("*", "");
        }

        private void ResetSkeleton()
        {
            poseEditor.ResetSkeleton(skeletonGo);
            AddUnsavedMark();
        }

        private void ApplySymmetry(bool isLeft)
        {
            SymmetryTarget target = Enum.Parse<SymmetryTarget>(symmetry_dropdown.value);
            poseEditor.ApplySymmetry(isLeft, target);
            AddUnsavedMark();
        }

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
            AddUnsavedMark();
        }

        private void SetIndexClosure(float rate)
        {
            if (handClosureBoneType == BoneType.RightHand)
                indexSliderMemory.lastRightValue = rate;
            else
                indexSliderMemory.lastLeftValue = rate;

            poseEditor.CloseFinger(handClosureBoneType, HandClosureGroup.INDEX, rate);
            AddUnsavedMark();
        }

        private void SetMedialGroupClosure(float rate)
        {
            if (handClosureBoneType == BoneType.RightHand)
                medialGroupSliderMemory.lastRightValue = rate;
            else
                medialGroupSliderMemory.lastLeftValue = rate;

            poseEditor.CloseFinger(handClosureBoneType, HandClosureGroup.MEDIAL_GROUP, rate);
            AddUnsavedMark();
        }

        #endregion Edition

        #region TreeView utils

        private TreeViewItem<BoneTreeElement> GetBoneTreeViewItem(PoseSetterBoneComponent bc)
        {
            BoneTreeElement boneTreeElement = new(bc.isRoot);
            string boneName = bc.name.Split(":")[1]; // this is a WIP line because the skeleton has "Mixamo:" every where as prefix
            TreeViewItem<BoneTreeElement> boneTreeViewItem = new((int)bc.BoneType, 1, boneName, boneTreeElement);

            boneTreeElement?.RootChanged.AddListener((e) =>
            {
                poseEditor.UpdateIsRoot(bc.BoneType, e.boolValue);
                AddUnsavedMark();
            });

            return boneTreeViewItem;
        }

        private void UpdateRootsInEditorWindow()
        {
            foreach (var boneComponent in poseEditor.Skeleton.boneComponents)
            {
                treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(boneComponent.isRoot, boneComponent.BoneType);
            }
        }

        #endregion TreeView utils
    }
}
#endif