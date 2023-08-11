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
using System.IO;
using System.Linq;
using System.Reflection;
using umi3d.common.userCapture.description;
using umi3d.edk.userCapture.pose.editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace umi3d.common.userCapture.pose.editor
{

    public class PoseSetterEditorWindow : EditorWindow
    {
        // TODO --> Add a fonctionnality to spawn a new dummy in T pose when you need it :D
        #region Attributes
        #region UXML fron ref attributes <They are ordered like in the corresponding UXML (almost)>

        //Main INfo
        VisualElement root = null;
        TextField name = null;
        DropdownField loa_dropdown = null; // TODO --> implement the LOA logic
        DropdownField anchor_dropdown = null;
        TextField path = null;
        CustomObjectField object_field = null;
        CustomObjectField so_field = null;

        //ControlsButtons

        DropdownField symetry_dropdown = null;

        Button btn_from_left = null;
        Button btn_from_right = null;

        Toggle tg_enable_condtion = null;

        ScrollView root_scroll_view = null;
        Button save = null;
        Button load = null;
        Button clear_roots = null;
        Button reset_skeleton = null;

        PoseConditionPanel conditionPanel = null;

        //Bones
        TextField filter = null;
        IMGUIContainer bone_container = null;

        Slider x_rot_slider = null;
        Slider y_rot_slider = null;
        Slider z_rot_slider = null;
        #endregion

        public enum SymetryTarget { None, Hands, Arms }
        public enum LoaEnum { None, LOA_0, LOA_1, LOA_2, LOA_3, LOA_4 }
        SymetryTarget symetryTarget = SymetryTarget.None;
        LoaEnum loaEnum = LoaEnum.None;
        BoneTreeView treeView = null;

        UMI3DPose_so currentPose = null;
        Transform selectedBone = null;
        List<PoseSetterBoneComponent> bone_components = new List<PoseSetterBoneComponent>();

        #endregion
        /// <summary>
        /// Open the tool 
        /// </summary>
        [MenuItem("UMI3D/PoseSetter")]
        public static void ShowExample()
        {
            PoseSetterEditorWindow wnd = GetWindow<PoseSetterEditorWindow>();
            wnd.titleContent = new GUIContent("PoseSetter");
            wnd.maxSize = new Vector2(350, 650);
        }

        #region Initialisation (Every thing that goes onEnable)
        /// <summary>
        /// Creates the UI using th UI_element framework
        /// </summary>
        public void OnEnable()
        {
            VisualTreeAsset uxml = Resources.Load<VisualTreeAsset>("PoseSetterEditorWindow");

            uxml.CloneTree(rootVisualElement);
            GetAllRefs();
            BindUI();
            ReadConstEnum(typeof(BoneType));
        }

        UnityEngine.Object selected;
        public void OnSelectionChange()
        {
            if (CheckIfIsBoneComponent(Selection.activeObject)) ;
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
            SetOnGUIContainer();
            InitSliders();
            InitObjectField();
            InitConditionContainer();
            FillDropDownFields();
        }

        /// <summary>
        /// Gets all the references to all the UI_elements in the UXML file
        /// </summary>
        private void GetAllRefs()
        {
            root = rootVisualElement;
            name = root.Q<TextField>("name");
            loa_dropdown = root.Q<DropdownField>("loa_dropdown");
            anchor_dropdown = root.Q<DropdownField>("anchor_dropdown");
            path = root.Q<TextField>("path");
            object_field = root.Q<CustomObjectField>("object_field");
            so_field = root.Q<CustomObjectField>("so_field");

            symetry_dropdown = root.Q<DropdownField>("symetry_dropdown");
            btn_from_left = root.Q<Button>("btn_from_left");
            btn_from_right = root.Q<Button>("btn_from_right");

            root_scroll_view = root.Q<ScrollView>("root_scroll_view");
            save = root.Q<Button>("save");
            load = root.Q<Button>("load");
            clear_roots = root.Q<Button>("clear_roots");
            reset_skeleton = root.Q<Button>("reset_skeleton");

            tg_enable_condtion = root.Q<Toggle>("tg_enable_condtion");
            conditionPanel = root.Q<PoseConditionPanel>("condition_panel");

            filter = root.Q<TextField>("filter");
            bone_container = root.Q<IMGUIContainer>("bone_container");

            x_rot_slider = root.Q<Slider>("x_rot_slider");
            y_rot_slider = root.Q<Slider>("y_rot_slider");
            z_rot_slider = root.Q<Slider>("z_rot_slider");
        }

        /// <summary>
        /// Inits the obbjects fields to be able to filter the drag n dropped files 
        /// Also it sets up the change value event to make sure that when de files are changed the tool updates
        /// </summary>
        private void InitObjectField()
        {
            object_field.Init(typeof(GameObject));
            // TODO ==> add a call back where you make sure to clean all the roots and the savable and the selected on the skeleton
            object_field.RegisterValueChangedCallback(value => { ReadHierachy(value); });

            so_field.Init(typeof(UMI3DPose_so));
            so_field.RegisterValueChangedCallback(value => { ChangeActiveSO(value); });
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

                EditorGUI.BeginChangeCheck();
                {

                }
                if (EditorGUI.EndChangeCheck())
                {

                }
                treeView.OnGUI(GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)));
            };
        }

        /// <summary>
        /// Binds All the buttons 
        /// </summary>
        private void BindButtons()
        {
            save.clicked += () => SaveToScriptableObjectAtPath();
            load.clicked += () => LoadA_UMI3DPose_so();
            clear_roots.clicked += () => { ResetAllBones(); UpdateRootListView(); };
            reset_skeleton.clicked += () => { ResetSkeleton(); };

            btn_from_left.clicked += () => { ApllySymetry(true); };
            btn_from_right.clicked += () => { ApllySymetry(false); };
        }

        private void FillDropDownFields()
        {
            loa_dropdown.choices.RemoveAt(0);
            Enum.GetNames(typeof(LoaEnum)).ForEach(name =>
            {
                loa_dropdown.choices.Add(name);
            });
            loa_dropdown.value = loa_dropdown.choices[0];

            loa_dropdown.RegisterValueChangedCallback((data) =>
            {
                loaEnum = Enum.Parse<LoaEnum>(data.newValue);
            });

            anchor_dropdown.choices.RemoveAt(0);
            TypeCache.GetTypesDerivedFrom<BonePoseDto>().ForEach(type =>
            {
                anchor_dropdown.choices.Add(type.Name);
            });
            anchor_dropdown.value = anchor_dropdown.choices[0];

            symetry_dropdown.choices.RemoveAt(0);
            Enum.GetNames(typeof(SymetryTarget)).ForEach(name =>
            {
                symetry_dropdown.choices.Add(name);
            });
            symetry_dropdown.value = symetry_dropdown.choices[0];

            symetry_dropdown.RegisterValueChangedCallback((data) =>
            {
                symetryTarget = Enum.Parse<SymetryTarget>(data.newValue);
            });
        }

        /// <summary>
        /// Init the sliders initial values, and bind their on change events.
        /// </summary>
        private void InitSliders()
        {
            x_rot_slider.value = 0;
            y_rot_slider.value = 0;
            z_rot_slider.value = 0;

            x_rot_slider.RegisterValueChangedCallback(value => { ChangeXRotationOfSelectedBone(value); });
            y_rot_slider.RegisterValueChangedCallback(value => { ChangeYRotationOfSelectedBone(value); });
            z_rot_slider.RegisterValueChangedCallback(value => { ChangeZRotationOfSelectedBone(value); });
        }

        private void InitConditionContainer()
        {
            conditionPanel.Init();
            tg_enable_condtion.RegisterValueChangedCallback(b =>
            {
                if (b.newValue == true)
                    conditionPanel.Enable();
                else
                {
                    conditionPanel.Disable();
                }
            });
        }

        #endregion
        /// <summary>
        /// This should be called each time you change the skeleton prefab
        /// Reads all of the PoseSetterBineComponent which are not None Bones and add them in the treeView
        /// </summary>
        /// <param name="value"></param>
        private void ReadHierachy(ChangeEvent<UnityEngine.Object> value)
        {
            bone_components = (value.newValue as GameObject).GetComponentsInChildren<PoseSetterBoneComponent>()
                                                            .Where(bc => bc.BoneType != 0)
                                                            .ToList();

            var treeViewItems = new List<TreeViewItem<BoneTreeElement>>();
            bone_components.ForEach(bc =>
            {
                treeViewItems.Add(GetBoneTreeViewItem(bc));
            });

            treeView.UpdateTreeView(treeViewItems);
            UpdateRootListView();
        }

        private TreeViewItem<BoneTreeElement> GetBoneTreeViewItem(PoseSetterBoneComponent bc)
        {
            var boneTreeElement = new BoneTreeElement(bc.isRoot, false);
            string boneName = bc.name.Split(":")[1]; // this is a WIP line because the skeleton has "Mixamo:" every where as prefix
            var boneTreeViewItem = new TreeViewItem<BoneTreeElement>((int)bc.BoneType, 1, boneName, boneTreeElement);

            boneTreeElement?.onIsRootChanged.AddListener((data) => { ChangeIsRoot(data); });
            boneTreeElement?.onIsSelectedChanged.AddListener((data) => { ChangeIsSelected(data); });
            return boneTreeViewItem;
        }

        private void ChangeActiveSO(ChangeEvent<UnityEngine.Object> value)
        {
            currentPose = value.newValue as UMI3DPose_so;
        }

        private void UpdateRootListView()
        {
            root_scroll_view.Clear();
            bone_components.Where(bc => bc.isRoot == true)
                            .ForEach(bc =>
                            {
                                root_scroll_view.Add(new Label("      " + bc.name.Split(":")[1]));
                            });
        }

        private void ChangeIsRoot(BoolChangeData boolChangeData)
        {
            PoseSetterBoneComponent boneComponent;
            if (bone_components.Count != 0)
            {
                boneComponent = bone_components.Find(bc => bc.BoneType == boolChangeData.itemID);

                UpdateChildsSavebility(boneComponent, boolChangeData.boolValue);
                UpdateRootListView();
            }
        }

        private void ChangeIsRootInEditorWindow(bool value, uint id)
        {
            treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(value, id);
        }

        /// <summary>
        /// Change the isSelected parameter on both tree view and the used skeleton
        /// </summary>
        /// <param name="boolChangeData"></param>
        private void ChangeIsSelected(BoolChangeData boolChangeData)
        {
            PoseSetterBoneComponent boneComponent;
            if (bone_components.Count != 0)
            {
                boneComponent = bone_components.Find(bc => bc.BoneType == boolChangeData.itemID);
                boneComponent.isSelected = boolChangeData.boolValue;
                if (boolChangeData.boolValue)
                    selectedBone = boneComponent.transform;
            }
        }

        private void UpdateChildsSavebility(PoseSetterBoneComponent boneComponent, bool value)
        {
            // Cleans childs roots and update child savability
            boneComponent.GetComponentsInChildren<PoseSetterBoneComponent>()
                         .Where(bc => bc.BoneType != BoneType.None)
                         .ForEach(bc =>
                         {
                             if (value == true)
                             {
                                 bc.isSavable = true;
                                 if (bc.isRoot == true)
                                 {
                                     bc.isRoot = false;
                                     ChangeIsRootInEditorWindow(false, bc.BoneType);
                                 }
                             }
                             else
                             {
                                 bc.isSavable = false;
                             }
                         });

            // For now if there is a parent which is a root, it stays there and will be saved as one more pose containing everything else
            // TODO -> fragment the parent == when a parent is a root, set it as a none root and then look for every child branches 
            //                                  that do not contain the current new root and set them as root.
            //                                      --> this would be a great way to make sur to never loose work and dont get confusing poses.
            if (boneComponent.GetComponentsInParent<PoseSetterBoneComponent>().Where(bc => bc.isRoot != null).FirstOrDefault() != boneComponent)
                boneComponent.isSavable = true;
            else
            {
                boneComponent.isSavable = false;
            }

            boneComponent.isRoot = value;
        }


        #region Save & load
        /// <summary>
        /// Saves a scriptable object at given path
        /// --> if you got many root on your skeleton it wil generate a scriptable object per root
        /// --> keep in mind that its normal that is you add a parent root you delete all the children root a
        ///         and that when you add a children root it dosent touch the parent once, (this last feature has to be changed at somepoint)
        /// </summary>
        private void SaveToScriptableObjectAtPath()
        {
            // TODO -> add a way load batches of poses

            string name = this.name.value;
            string path = this.path.value;
            if (path == "") path = "Assets/";

            var roots = bone_components.Where(bc => bc.isRoot == true).ToList();

            foreach (PoseSetterBoneComponent r in roots)
            {
                r.transform.rotation = Quaternion.identity; // security to make sure that the positions and rotation are right
                List<BoneDto> bonsPoseSos = new();
                var pose_So = (UMI3DPose_so)CreateInstance(typeof(UMI3DPose_so));
                pose_So.name = name;
                AssetDatabase.CreateAsset(pose_So, path + $"/{name}_from_{GetConstEnumField(r.BoneType)}.asset");

                List<PoseSetterBoneComponent> boneToSave = r.GetComponentsInChildren<PoseSetterBoneComponent>()
                                                             .Where(bc => bc.BoneType != BoneType.None)
                                                             .ToList();

                Vector4Dto rootRotation = r.transform.rotation.Dto();
                BonePoseDto bonePoseDto = CreateBonePoseDTOOfType(rootRotation, r);
                boneToSave.RemoveAt(0);

                boneToSave.ForEach(bc =>
                {
                    Vector4Dto bonerotation = bc.transform.rotation.Dto();
                    var bonePose_So = new BoneDto()
                    {
                        boneType = bc.BoneType,
                        rotation = bonerotation
                    };

                    AssetDatabase.SaveAssets();

                    bonsPoseSos.Add(bonePose_So);
                });

                pose_So.Init(bonsPoseSos, bonePoseDto);
                EditorUtility.SetDirty(pose_So);
                AssetDatabase.SaveAssets();

                SavePoseOverrider(pose_So, path);
                EditorUtility.SetDirty(pose_So);
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
            string anchor = anchor_dropdown.value;
            switch (anchor_dropdown.value)
            {
                case "AnchoredBonePoseDto":
                    return new AnchoredBonePoseDto()
                    {
                        bone = bonePoseDto.bone,
                        position = bonePoseDto.position,
                        rotation = bonePoseDto.rotation
                    };
                case "NodeAnchor":
                    return new NodeAnchoredBonePoseDto()
                    {
                        bone = bonePoseDto.bone,
                        position = bonePoseDto.position,
                        rotation = bonePoseDto.rotation
                    };
                case "FloorAnchoredBonePoseDto":
                    return new FloorAnchoredBonePoseDto()
                    {
                        bone = bonePoseDto.bone,
                        position = bonePoseDto.position,
                        rotation = bonePoseDto.rotation,
                    };
            }
            return bonePoseDto;
        }

        private void SavePoseOverrider(UMI3DPose_so pose_So, string path)
        {
            if (tg_enable_condtion.value == true)
            {
                UMI3DPoseOverrider_so poseOverrider = conditionPanel.GetPoseOverrider_So();
                poseOverrider.pose = pose_So;
                string name = poseOverrider.name = pose_So.name + "_overrider";
                AssetDatabase.CreateAsset(poseOverrider, path + $"/{name}.asset");
                AssetDatabase.SaveAssets();
                EditorUtility.SetDirty(poseOverrider);
            }
        }

        /// <summary>
        /// Load one scriptable object and apply all bone pose to the current skeleton in scene view
        /// </summary>
        private void LoadA_UMI3DPose_so()
        {
            if (bone_components?.Count == 0)
                Debug.Log($"<color=red> Well you should refer a rigious skeleton</color>");
            else
            {
                name.value = currentPose.name;
                string[] path = AssetDatabase.GetAssetPath(currentPose).Split("/");
                string finalPath = "";
                for (int i = 0; i < path.Length - 1; i++)
                {
                    finalPath += path[i] + "/";
                }
                this.path.value = finalPath;

                ResetAllBones();

                PoseSetterBoneComponent root_boneComponent = bone_components.Find(bc => bc.BoneType == currentPose.GetBonePoseCopy().bone);
                root_boneComponent.isRoot = true;
                root_boneComponent.isSavable = false;
                treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(true, root_boneComponent.BoneType);

                UpdateBoneComponent(currentPose.GetBonePoseCopy());
                anchor_dropdown.SetValueWithoutNotify(anchor_dropdown.choices.Find(c => c == currentPose.GetBonePoseCopy().GetType().ToString().Split(".").Last()));

                currentPose.GetBonesCopy().ForEach(bp =>
                {
                    UpdateBoneComponent(bp);
                });

                UpdateRootListView();
            }
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

                if (bc.isSavable != false)
                    bc.isSavable = false;
            });
        }

        private void ResetSkeleton()
        {
            ResetAllBones();
            (object_field.value as GameObject).GetComponentsInChildren<PoseSetterBoneComponent>()
                                            .ForEach(bc =>
                                            {
                                                bc.transform.rotation = Quaternion.identity;
                                            });
        }

        private void UpdateBoneComponent(BoneDto bonedto)
        {
            PoseSetterBoneComponent bone_component = bone_components.Find(bc => bc.BoneType == bonedto.boneType);
            if (bone_component != null)
            {
                bone_component.transform.rotation = bonedto.rotation.Quaternion();
                bone_component.isSavable = true;
            }
        }

        private void UpdateBoneComponent(BonePoseDto bonePoseDto)
        {
            PoseSetterBoneComponent bone_component = bone_components.Find(bc => bc.BoneType == bonePoseDto.bone);
            if (bone_component != null)
            {
                bone_component.transform.rotation = bonePoseDto.rotation.Quaternion();
                bone_component.isSavable = true;
            }
        }
        #endregion
        #region change bone rotation
        private void ChangeZRotationOfSelectedBone(ChangeEvent<float> value)
        {
            if (selectedBone != null)
            {
                Vector3 rotation = selectedBone.rotation.eulerAngles;
                rotation.x = value.newValue;
                var quaternion = Quaternion.Euler(rotation);
                selectedBone.rotation = quaternion;
            }
        }

        private void ChangeYRotationOfSelectedBone(ChangeEvent<float> value)
        {
            if (selectedBone != null)
            {
                Vector3 rotation = selectedBone.rotation.eulerAngles;
                rotation.y = value.newValue;
                var quaternion = Quaternion.Euler(rotation);
                selectedBone.rotation = quaternion;
            }
        }

        private void ChangeXRotationOfSelectedBone(ChangeEvent<float> value)
        {
            if (selectedBone != null)
            {
                Vector3 rotation = selectedBone.rotation.eulerAngles;
                rotation.z = value.newValue;
                var quaternion = Quaternion.Euler(rotation);
                selectedBone.rotation = quaternion;
            }
        }
        #endregion
        #region Symetri
        private void ApllySymetry(bool isFromLeft)
        {
            if (symetryTarget == SymetryTarget.None) return;
            if (object_field.value == null) return;

            var origin_target = new Transform[2];
            origin_target = GetSymRoot(isFromLeft, symetryTarget);
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

        private Transform[] GetSymRoot(bool isFromLeft, SymetryTarget symetryTarget)
        {
            var origin_target = new Transform[2];
            switch (symetryTarget)
            {
                case SymetryTarget.Hands:
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
                case SymetryTarget.Arms:
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
        #region utils
        string[] constEnumFieldName = null;
        private void ReadConstEnum(Type type)
        {
            IEnumerable<FieldInfo> val = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                            .Where(fi => fi.IsLiteral && !fi.IsInitOnly);
            constEnumFieldName = val.Select(fi => fi.Name).ToArray();
        }

        private string GetConstEnumField(uint id)
        {
            if (constEnumFieldName != null)
                return constEnumFieldName[id];
            return id.ToString();
        }
        #endregion
    }
}
#endif
