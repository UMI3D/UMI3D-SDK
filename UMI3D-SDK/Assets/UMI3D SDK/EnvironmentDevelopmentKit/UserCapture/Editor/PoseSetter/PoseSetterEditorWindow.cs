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
using System.Linq;
using System.IO;
using System.Reflection;
using static intetum.unityUtils.BoneTreeView;
using static UnityEditor.Progress;

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
        Button load = null;

        TextField filter = null;
        IMGUIContainer bone_container = null;

        Slider x_rot_slider = null;
        Slider y_rot_slider = null;
        Slider z_rot_slider = null;

        BoneTreeView treeView = null;

        UMI3DPose_so currentPose = null;
        List<PoseSetterBoneComponent> bone_components = new List<PoseSetterBoneComponent>();
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

        PoseDto_writter poseDto_Writter = new PoseDto_writter();

        #region Initialisation (Every thing that goes onEnable)
        /// <summary>
        /// Creates the UI using th UI_element framework
        /// </summary>
        public void OnEnable()
        {
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                                    "Assets\\UMI3D SDK\\EnvironmentDevelopmentKit\\UserCapture\\Editor\\PoseSetter\\PoseSetterEditorWindow.uxml"
                                   );
            uxml.CloneTree(rootVisualElement);
            GetAllRefs();
            InitTextFields();
            BindButtons();
            SetOnGUIContainer();
            InitSliders();
            InitObjectField();
            ReadConstEnum(typeof(BoneType));
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

            lv_root_selected = root.Q<ListView>("lv_root_selected");
            add_root = root.Q<Button>("add_root");
            remove_root = root.Q<Button>("remove_root");
            save = root.Q<Button>("save");
            load = root.Q<Button>("load");

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
            add_root.clicked += () => AddAnEmptyRootToListView();
            remove_root.clicked += () => RemoveLastRootFromListView();
            save.clicked += () => SaveToScriptableObjectAtPath();
            load.clicked += () => LoadA_UMI3DPose_so();
        }

        Transform selectedBone = null;

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

        private void InitTextFields()
        {

        }

        #endregion

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

        private void AddAnEmptyRootToListView()
        {

        }

        private void RemoveLastRootFromListView()
        {

        }

        private void ChangeIsRoot(BoolChangeData boolChangeData)
        {
            PoseSetterBoneComponent boneComponent;
            if (bone_components.Count != 0)
            {
                boneComponent = bone_components.Find(bc => bc.BoneType == boolChangeData.itemID);

                UpdateChildsSavebility(boneComponent, boolChangeData.boolValue);  
            }
        }

        private void ChangeIsRootInEditorWindow(bool value, uint id)
        {
            treeView.UpdateSingleIsRootToggleWithNoSkeletonUpdate_ById(value, id);
        }

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
            if (boneComponent.GetComponentsInParent<PoseSetterBoneComponent>().Where(bc => bc.isRoot != null).FirstOrDefault() != null)
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
        private void SaveToScriptableObjectAtPath()
        {
            string name = this.name.value;
            string path = this.path.value;
            if (path == "") path = "Assets/";

            UMI3DPose_so pose_So = (UMI3DPose_so)CreateInstance(typeof(UMI3DPose_so));
            pose_So.name = name;
            AssetDatabase.CreateAsset(pose_So, path + $"/{name}.asset");

            //Save each bones 
            List<UMI3DBonePose_so> bonsPoseSos = new();
            bone_components.ForEach(bc =>
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

            pose_So.Init(bonsPoseSos, 0);
            EditorUtility.SetDirty(pose_So);
        }

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

                currentPose.BonePoses.ForEach(bp =>
                {
                    UpdateBoneComponent(bp);
                });
            }
        }

        private void UpdateBoneComponent(UMI3DBonePose_so bp)
        {
            PoseSetterBoneComponent bone_component = bone_components.Find(bc => bc.BoneType == bp.bone);
            if (bone_component != null)
            {
                bone_component.transform.rotation = new Quaternion(bp.rotation.x, bp.rotation.y, bp.rotation.z, bp.rotation.w);
                bone_component.transform.position = bp.position;
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
            elements.Find(e => e.id == id).data.isRoot = value;
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
            TreeViewItem<BoneTreeElement> root = new TreeViewItem<BoneTreeElement>(0, -1, "Root", new BoneTreeElement(false, false));
            TreeViewItem<BoneTreeElement> skeletonRoot = new TreeViewItem<BoneTreeElement>(0, 0, "Root", new BoneTreeElement(false, false));

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
                    EditorGUI.LabelField(cellRect, "     "+item.data.name, EditorStyles.boldLabel);
                }
                else if (col == 1)
                {
                    Rect toggleRect = new Rect(cellRect.center.x - 8f, cellRect.y, 16f, cellRect.height);
                    bool newValue = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, item.data.isRoot);
                    if (newValue != item.data.isRoot)
                    {
                        item.data.isRoot = newValue;
                        item.data.onIsRootChanged?.Invoke(new BoolChangeData { boneTreeEleements = item.data, itemID = item.id, boolValue = newValue });
                    }
                }
                else if (col == 2)
                {
                    Rect toggleRect = new Rect(cellRect.center.x - 8f, cellRect.y, 16f, cellRect.height);
                    bool newValue = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, item.data.isSelected);
                    if (newValue != item.data.isSelected)
                    {
                        item.data.isSelected = newValue;
                        item.data.onIsSelectedChanged?.Invoke(new BoolChangeData { boneTreeEleements = item.data, itemID = item.id, boolValue = newValue });
                        ResetEveryOtherSelectedToFalse(item.id);
                    }
                }
            }
        }

        private void ResetEveryOtherSelectedToFalse(int ignoreId)
        {
            elements.ForEach(e =>
            {
                if (e.data.isSelected && e.id != ignoreId)
                {
                    e.data.isSelected = false;
                    e.data.onIsSelectedChanged?.Invoke(new BoolChangeData { itemID = e.id, boolValue = false });
                }
            });
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
