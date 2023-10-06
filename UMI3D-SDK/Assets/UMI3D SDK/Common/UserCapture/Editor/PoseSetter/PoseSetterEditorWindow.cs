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
using System.Collections.Generic;
using System.Linq;
using umi3d.common.userCapture.description;
using umi3d.edk.userCapture.pose.editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace umi3d.common.userCapture.pose.editor
{

    public class PoseSetterEditorWindow : EditorWindow
    {
        #region Attributes

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

        Toggle generate_overrider_toggle;
        
        CustomObjectField skeleton_object_field;
        Button reset_skeleton;

        //Bones
        IMGUIContainer bone_container;
        #endregion

        public enum SymmetryTarget { None, Hands, Arms }

        SymmetryTarget symmetryTarget = SymmetryTarget.None;

        BoneTreeView treeView;

        UMI3DPose_so currentPose;
        List<PoseSetterBoneComponent> bone_components = new();

        #endregion

        /// <summary>
        /// Open the tool 
        /// </summary>
        [MenuItem("UMI3D/Pose Editor")]
        public static void ShowWindow()
        {
            PoseSetterEditorWindow wnd = GetWindow<PoseSetterEditorWindow>();
            wnd.titleContent = new GUIContent("Pose Editor");
            wnd.maxSize = new Vector2(350, 650);
        }

        #region Initialisation 
        // (Every thing that goes onEnable)

        /// <summary>
        /// Creates the UI using th UI_element framework
        /// </summary>
        public void OnEnable()
        {
            VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>("PoseSetterEditorWindow");

            visualTreeAsset.CloneTree(rootVisualElement);
            GetAllRefs();
            BindUI();
        }

        UnityEngine.Object selected;

        public void OnSelectionChange()
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
            pose_name_field.RegisterValueChangedCallback((changeEvent) => UpdateSaveButton(changeEvent.newValue));
            SetOnGUIContainer();
            InitObjectField();
            FillDropDownFields();
        }

        /// <summary>
        /// Gets all the references to all the UI_elements in the UXML file
        /// </summary>
        private void GetAllRefs()
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
            generate_overrider_toggle = root.Q<Toggle>("GeneratePoseOverriderToggle");

            symmetry_dropdown = root.Q<DropdownField>("symmetry_dropdown");
            symmetry_from_left_button = root.Q<Button>("btn_from_left");
            symmetry_from_right_button = root.Q<Button>("btn_from_right");
            reset_skeleton = root.Q<Button>("reset_skeleton");

            bone_container = root.Q<IMGUIContainer>("bone_container");

            Assert.IsNotNull(pose_name_field);
            Assert.IsNotNull(pose_path_field);
            Assert.IsNotNull(skeleton_object_field);
            Assert.IsNotNull(so_field);
            Assert.IsNotNull(generate_overrider_toggle);
            Assert.IsNotNull(symmetry_dropdown);
            Assert.IsNotNull(symmetry_from_left_button);
            Assert.IsNotNull(symmetry_from_right_button);
            Assert.IsNotNull(save_button);
            Assert.IsNotNull(load_button);
            Assert.IsNotNull(reset_skeleton);
            Assert.IsNotNull(bone_container);
        }

        public void UpdateSaveButton(string name)
        {
            if (name != string.Empty && !save_button.enabledSelf)
                save_button.SetEnabled(true);
            else if (name == string.Empty && save_button.enabledSelf)
                save_button.SetEnabled(false);
        }

        /// <summary>
        /// Inits the objects fields to be able to filter the drag n dropped files 
        /// Also it sets up the change value event to make sure that when de files are changed the tool updates
        /// </summary>
        private void InitObjectField()
        {
            skeleton_object_field.Init(typeof(GameObject));
            // TODO ==> add a call back where you make sure to clean all the roots and the savable and the selected on the skeleton
            skeleton_object_field.RegisterValueChangedCallback(changeEvent => { OpenEditor(changeEvent.newValue);  ReadHierarchy(changeEvent); });

            so_field.Init(typeof(UMI3DPose_so));
            so_field.RegisterValueChangedCallback(value => { ChangeActiveSO(value); });
        }

        private void OpenEditor(UnityEngine.Object value)
        {
            if (editor_container.resolvedStyle.display == DisplayStyle.None && create_load_container.resolvedStyle.display == DisplayStyle.None)
                DisplayCreateLoadContainer();
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

        /// <summary>
        /// Binds All the buttons 
        /// </summary>
        private void BindButtons()
        {
            create_button.clicked += () => { DisplayEditorContainer(); save_button.SetEnabled(false); };
            save_button.clicked += () => SaveToScriptableObjectAtPath();
            load_button.clicked += () => { LoadUMI3DPose_so(); DisplayEditorContainer(); };
            reset_skeleton.clicked += () => { ResetSkeleton(); };

            symmetry_from_left_button.clicked += () => { ApplySymmetry(true); };
            symmetry_from_right_button.clicked += () => { ApplySymmetry(false); };
        }

        private void DisplayCreateLoadContainer()
        {
            create_load_container.style.display = DisplayStyle.Flex;
        }

        private void DisplayEditorContainer()
        {
            editor_container.style.display = DisplayStyle.Flex;
            create_load_container.style.display = DisplayStyle.None;
        }

        private void FillDropDownFields()
        {
            symmetry_dropdown.choices.RemoveAt(0);
            Enum.GetNames(typeof(SymmetryTarget)).ForEach(name =>
            {
                symmetry_dropdown.choices.Add(name);
            });
            symmetry_dropdown.value = symmetry_dropdown.choices[0];

            symmetry_dropdown.RegisterValueChangedCallback((data) =>
            {
                symmetryTarget = Enum.Parse<SymmetryTarget>(data.newValue);
            });
        }

        #endregion

        /// <summary>
        /// This should be called each time you change the skeleton prefab
        /// Reads all of the PoseSetterBineComponent which are not None Bones and add them in the treeView
        /// </summary>
        /// <param name="value"></param>
        private void ReadHierarchy(ChangeEvent<UnityEngine.Object> value)
        {
            bone_components = (value.newValue as GameObject).GetComponentsInChildren<PoseSetterBoneComponent>()
                                                            .Where(bc => bc.BoneType != BoneType.None)
                                                            .ToList();

            List<TreeViewItem<BoneTreeElement>> treeViewItems = new ();
            bone_components.ForEach(bc =>
            {
                treeViewItems.Add(GetBoneTreeViewItem(bc));
            });

            treeView.UpdateTreeView(treeViewItems);
        }

        private TreeViewItem<BoneTreeElement> GetBoneTreeViewItem(PoseSetterBoneComponent bc)
        {
            BoneTreeElement boneTreeElement = new (bc.isRoot);
            string boneName = bc.name.Split(":")[1]; // this is a WIP line because the skeleton has "Mixamo:" every where as prefix
            TreeViewItem<BoneTreeElement> boneTreeViewItem = new ((int)bc.BoneType, 1, boneName, boneTreeElement);

            boneTreeElement?.RootChanged.AddListener(ChangeIsRoot);
            return boneTreeViewItem;
        }

        #region Update View

        private void ChangeActiveSO(ChangeEvent<UnityEngine.Object> value)
        {
            currentPose = value.newValue as UMI3DPose_so;
        }

        private void ChangeIsRoot(BoolChangeData boolChangeData)
        {
            if (bone_components.Count == 0)
                return;
            
            PoseSetterBoneComponent boneComponent = bone_components.Find(bc => bc.BoneType == boolChangeData.itemID);
            UpdateRootOnSkeleton(boneComponent, boolChangeData.boolValue);

            if (bone_components.All(bc=>!bc.isRoot))
                ResetRoot();
        }

        private void ResetRoot()
        {
            var hipsBoneComponent = bone_components.Find(bc => bc.BoneType == BoneType.Hips);
            UpdateRootOnSkeleton(hipsBoneComponent, true);
        }

        private void ChangeIsRootInEditorWindow(bool value, uint id)
        {
            treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(value, id);
        }

        private void UpdateRootOnSkeleton(PoseSetterBoneComponent boneComponent, bool value)
        {
            Assert.IsNotNull(boneComponent);

            // Cleans children roots and update child savability
            boneComponent.GetComponentsInChildren<PoseSetterBoneComponent>()
                         .Where(bc => bc.BoneType != BoneType.None)
                         .ForEach(bc =>
                         {
                             if (value == true)
                             {
                                 if (bc.isRoot) // cannot be root if a parent is root
                                 {
                                     bc.isRoot = false;
                                     ChangeIsRootInEditorWindow(false, bc.BoneType);
                                 }
                             }
                         });

            boneComponent.isRoot = value;
        }

        #endregion Update View

        #region Save & load
        /// <summary>
        /// Saves a scriptable object at given path
        /// --> if you got many root on your skeleton it wil generate a scriptable object per root
        /// --> keep in mind that its normal that is you add a parent root you delete all the children root a
        ///         and that when you add a children root it dosent touch the parent once, (this last feature has to be changed at somepoint)
        /// </summary>
        private void SaveToScriptableObjectAtPath()
        {
            string name = this.pose_name_field.value;
            string path = this.pose_path_field.value;
            if (path == string.Empty) 
                path = "Assets";
            
            var roots = bone_components.Where(bc => bc.isRoot).ToList();
            if (roots.Count == 0)
            {
                ResetRoot();
                roots = bone_components.Where(bc => bc.isRoot).ToList();
            } 


            foreach (PoseSetterBoneComponent r in roots)
            {
                r.transform.rotation = Quaternion.identity; // security to make sure that the positions and rotation are right
                List<BoneDto> bonsPoseSos = new();
                var pose_So = (UMI3DPose_so)CreateInstance(typeof(UMI3DPose_so));
                pose_So.name = name;

                string fileName = roots.Count == 1 ? name : $"{name}_from_{BoneTypeHelper.GetBoneName(r.BoneType)}";
                string completeName = System.IO.Path.ChangeExtension(fileName, ".asset");
                AssetDatabase.CreateAsset(pose_So, System.IO.Path.Combine(path, completeName));

                List<PoseSetterBoneComponent> bonesToSave = r.GetComponentsInChildren<PoseSetterBoneComponent>()
                                                             .Where(bc => bc.BoneType != BoneType.None)
                                                             .ToList();

                Vector4Dto rootRotation = r.transform.rotation.Dto();
                BonePoseDto bonePoseDto = CreateBonePoseDTOOfType(rootRotation, r);
                bonesToSave.RemoveAt(0);

                bonesToSave.ForEach(bc =>
                {
                    Vector4Dto boneRotation = bc.transform.rotation.Dto();
                    var bonePose_So = new BoneDto()
                    {
                        boneType = bc.BoneType,
                        rotation = boneRotation
                    };

                    AssetDatabase.SaveAssets();

                    bonsPoseSos.Add(bonePose_So);
                });

                pose_So.Init(bonsPoseSos, bonePoseDto);
                EditorUtility.SetDirty(pose_So);
                AssetDatabase.SaveAssets();

                SavePoseOverrider(pose_So, path);

            }
        }

        private BonePoseDto CreateBonePoseDTOOfType(Vector4Dto rootRotation, PoseSetterBoneComponent r)
        {
            var bonePoseDto = new BonePoseDto()
            {
                bone = r.BoneType,
                position = r.transform.position.Dto(),
                rotation = rootRotation
            };
            return new NodeAnchoredBonePoseDto()
            {
                bone = bonePoseDto.bone,
                position = bonePoseDto.position,
                rotation = bonePoseDto.rotation
            };
        }

        private void SavePoseOverrider(UMI3DPose_so pose_So, string parentPath)
        {
            if (generate_overrider_toggle.value is not true)
                return;
            
            UMI3DPoseOverrider_so poseOverrider = (UMI3DPoseOverrider_so)ScriptableObject.CreateInstance(typeof(UMI3DPoseOverrider_so));
            poseOverrider.pose = pose_So;
            poseOverrider.name = $"{pose_So.name}_overrider";
                
            string path = System.IO.Path.ChangeExtension(System.IO.Path.Combine(parentPath, poseOverrider.name), ".asset");
            AssetDatabase.CreateAsset(poseOverrider, path);
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(poseOverrider);
        }

        /// <summary>
        /// Load one scriptable object and apply all bone pose to the current skeleton in scene view
        /// </summary>
        private void LoadUMI3DPose_so()
        {
            if (bone_components?.Count == 0)
            {
                Debug.Log($"<color=red>A skeleton with rigs is expected.</color>");
                return;
            }
                
            if (currentPose == null)
            {
                Debug.Log($"<color=red>An UMI3DPose_so is expected.</color>");
                return;
            }

            pose_name_field.value = currentPose.name;
            string a = AssetDatabase.GetAssetPath(currentPose);
            this.pose_path_field.value = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(currentPose));
                
            ResetAllBones();

            PoseSetterBoneComponent root_boneComponent = bone_components.Find(bc => bc.BoneType == currentPose.GetBonePoseCopy().bone);
            root_boneComponent.isRoot = true;
            treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(true, root_boneComponent.BoneType);

            UpdateBoneRotation(currentPose.GetBonePoseCopy());

            currentPose.GetBonesCopy().ForEach(UpdateBoneRotation);

        }

        /// <summary>
        /// Set all bones isSavable & isRoot to false
        /// </summary>
        private void ResetAllBones()
        {
            bone_components.ForEach(bc =>
            {
                if (bc.isRoot != false)
                {
                    bc.isRoot = false;
                    treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(false, bc.BoneType);
                }
            });
        }

        private void ResetSkeleton()
        {
            ResetAllBones();
            if (skeleton_object_field.value is not GameObject go)
                return;
            go.GetComponentsInChildren<PoseSetterBoneComponent>()
                .ForEach(bc =>
                {
                    bc.transform.rotation = Quaternion.identity;
                });
        }

        private void UpdateBoneRotation(BoneDto boneDto)
        {
            PoseSetterBoneComponent bone_component = bone_components.Find(bc => bc.BoneType == boneDto.boneType);
            if (bone_component == null)
                return;
            bone_component.transform.rotation = boneDto.rotation.Quaternion();
        }

        private void UpdateBoneRotation(BonePoseDto bonePoseDto)
        {
            PoseSetterBoneComponent bone_component = bone_components.Find(bc => bc.BoneType == bonePoseDto.bone);
            if (bone_component == null)
                return;
            bone_component.transform.rotation = bonePoseDto.rotation.Quaternion();            
        }
        #endregion

        #region Symmetry
        private void ApplySymmetry(bool isFromLeft)
        {
            if (symmetryTarget == SymmetryTarget.None) return;
            if (skeleton_object_field.value == null) return;

            Transform[] origin_target = GetSymRoot(isFromLeft, symmetryTarget);
            PoseSetterBoneComponent[] originBC = origin_target[0].GetComponentsInChildren<PoseSetterBoneComponent>();
            PoseSetterBoneComponent[] targetBC = origin_target[1].GetComponentsInChildren<PoseSetterBoneComponent>();

            for (int i = 0; i < originBC.Length; i++)
            {
                Transform ot = originBC[i].transform;
                targetBC[i].transform.rotation = new Quaternion(ot.rotation.x * -1.0f,
                                            ot.rotation.y,
                                            ot.rotation.z,
                                            ot.rotation.w * -1.0f);
            }
        }

        private Transform[] GetSymRoot(bool isFromLeft, SymmetryTarget symmetryTarget)
        {
            var origin_target = new Transform[2];
            switch (symmetryTarget)
            {
                case SymmetryTarget.Hands:
                    if (isFromLeft)
                    {
                        origin_target[0] = bone_components.Find(bc => bc.BoneType == BoneType.LeftHand).transform;
                        origin_target[1] = bone_components.Find(bc => bc.BoneType == BoneType.RightHand).transform;
                    }
                    else
                    {
                        origin_target[0] = bone_components.Find(bc => bc.BoneType == BoneType.RightHand).transform;
                        origin_target[1] = bone_components.Find(bc => bc.BoneType == BoneType.LeftHand).transform;
                    }
                    break;
                case SymmetryTarget.Arms:
                    if (isFromLeft)
                    {
                        origin_target[0] = bone_components.Find(bc => bc.BoneType == BoneType.LeftUpperArm).transform;
                        origin_target[1] = bone_components.Find(bc => bc.BoneType == BoneType.RightUpperArm).transform;
                    }
                    else
                    {
                        origin_target[0] = bone_components.Find(bc => bc.BoneType == BoneType.RightUpperArm).transform;
                        origin_target[1] = bone_components.Find(bc => bc.BoneType == BoneType.LeftUpperArm).transform;
                    }
                    break;
            }

            return origin_target;
        }



        #endregion
    }
}
#endif
