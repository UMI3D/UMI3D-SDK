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

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;
using inetum.unityUtils;
using umi3d.edk.userCapture;
using System.Linq;
using System.Reflection;
using GLTF.Schema;

namespace umi3d.common.userCapture
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

        PoseConditionPanel conditionPanel = null;

        //Bones
        TextField filter = null;
        IMGUIContainer bone_container = null;

        Slider x_rot_slider = null;
        Slider y_rot_slider = null;
        Slider z_rot_slider = null;
        #endregion

        public enum SymetryTarget { None, Hands, Arms, Legs, Feet}
        public enum LoaEnum { None, LOA_0, LOA_1, LOA_2, LOA_3, LOA_4}
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
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                                    "Assets\\UMI3D SDK\\Common\\UserCapture\\Editor\\PoseSetter\\PoseSetterEditorWindow.uxml"
                                   );
            uxml.CloneTree(rootVisualElement);
            GetAllRefs();
            BindUI();
            ReadConstEnum(typeof(BoneType));
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
            TreeViewState m_TreeViewState = new TreeViewState();
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
            clear_roots.clicked += () => { ResetAllBones();  UpdateRootListView(); };
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
                {
                    conditionPanel.Enable();
                }
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

            List<TreeViewItem<BoneTreeElement>> treeViewItems = new List<TreeViewItem<BoneTreeElement>>();
            bone_components.ForEach(bc =>
            {
                treeViewItems.Add(GetBoneTreeViewItem(bc));
            });

            treeView.UpdateTreeView(treeViewItems);
            UpdateRootListView();
        }

        private TreeViewItem<BoneTreeElement> GetBoneTreeViewItem(PoseSetterBoneComponent bc)
        {
            BoneTreeElement boneTreeElement = new BoneTreeElement(bc.isRoot, false);
            string boneName = bc.name.Split(":")[1]; // this is a WIP line because the skeleton has "Mixamo:" every where as prefix
            TreeViewItem<BoneTreeElement> boneTreeViewItem = new TreeViewItem<BoneTreeElement>((int)bc.BoneType, 1, boneName, boneTreeElement);

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
                {
                    selectedBone = boneComponent.transform;
                    // //Dont uncomments appart if you like atomic holocaust !!!!
                    //x_rot_slider.value = selectedBone.transform.eulerAngles.x;
                    //y_rot_slider.value = selectedBone.transform.eulerAngles.y;
                    //z_rot_slider.value = selectedBone.transform.eulerAngles.z;
                }
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
            {
                boneComponent.isSavable = true;
            }
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

            List<PoseSetterBoneComponent> roots = bone_components.Where(bc => bc.isRoot == true).ToList();

            roots.ForEach(r =>
            {
                r.transform.rotation = Quaternion.identity; // security to make sure that the positions and rotation are right
                List<UMI3DBonePose_so> bonsPoseSos = new();
                UMI3DPose_so pose_So = (UMI3DPose_so)CreateInstance(typeof(UMI3DPose_so));
                pose_So.name = name;
                AssetDatabase.CreateAsset(pose_So, path + $"/{name}_from_{GetConstEnumField(r.BoneType)}.asset");

                List<PoseSetterBoneComponent> boneToSave = r.GetComponentsInChildren<PoseSetterBoneComponent>()
                                                             .Where(bc => bc.BoneType != BoneType.None)
                                                             .ToList();
                boneToSave.RemoveAt(0);
                                                             
                boneToSave.ForEach(bc =>
                {
                    Vector4 rotation = new Vector4(bc.transform.rotation.x, bc.transform.rotation.y, bc.transform.rotation.z, bc.transform.rotation.w);
                    UMI3DBonePose_so bonePose_So = (UMI3DBonePose_so)CreateInstance(typeof(UMI3DBonePose_so));
                    bonePose_So.Init(bc.BoneType, bc.transform.position, rotation);
                    bonePose_So.name = name + $"_{GetConstEnumField(bonePose_So.bone)}";

                    AssetDatabase.AddObjectToAsset(bonePose_So, pose_So);
                    AssetDatabase.SaveAssets();
                    EditorUtility.SetDirty(bonePose_So);

                    bonsPoseSos.Add(bonePose_So);
                });

                pose_So.Init(bonsPoseSos, r.BoneType);
                EditorUtility.SetDirty(pose_So);
            });
        }

        /// <summary>
        /// Load one scriptable object and apply all bone pose to the current skeleton in scene view
        /// </summary>
        private void LoadA_UMI3DPose_so()
        {
            if (bone_components?.Count == 0)
            {
                Debug.Log($"<color=red> Well you should refer a rigious skeleton</color>");
            }
            else
            {
                name.value = currentPose.name;
                string[] path = AssetDatabase.GetAssetPath(currentPose).Split("/");
                string finalPath = "";
                for (int i = 0; i < path.Length-1; i++)
                {
                    finalPath += path[i] + "/";
                }
                this.path.value = finalPath;

                ResetAllBones();

                PoseSetterBoneComponent root_boneComponent = bone_components.Find(bc => bc.BoneType == currentPose.BoneAnchor);
                root_boneComponent.isRoot = true;
                root_boneComponent.isSavable = false;
                treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(true, root_boneComponent.BoneType);

                currentPose.BonePoses.ForEach(bp =>
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
                {
                    bc.isSavable = false;
                }
            });
        }

        private void UpdateBoneComponent(UMI3DBonePose_so bp)
        {
            PoseSetterBoneComponent bone_component = bone_components.Find(bc => bc.BoneType == bp.bone);
            if (bone_component != null)
            {
                bone_component.transform.rotation = new Quaternion(bp.rotation.x, bp.rotation.y, bp.rotation.z, bp.rotation.w);
                bone_component.transform.position = bp.position;
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
        #region utils
        string[] constEnumFieldName = null;
        private void ReadConstEnum(Type type)
        {
            System.Collections.Generic.IEnumerable<FieldInfo> val = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                            .Where(fi => fi.IsLiteral && !fi.IsInitOnly);
            constEnumFieldName = val.Select(fi => fi.Name).ToArray();
        }

        private string GetConstEnumField(uint id)
        {
            if (constEnumFieldName != null)
            {
                return constEnumFieldName[id];
            }
            return id.ToString();
        }
        #endregion
    }
}
#endif
