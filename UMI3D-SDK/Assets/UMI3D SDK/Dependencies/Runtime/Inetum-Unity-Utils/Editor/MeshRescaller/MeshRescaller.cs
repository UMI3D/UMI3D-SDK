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
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;


namespace inetum.unityUtils.editor
{
    public class MeshRescaller : EditorWindow
    {
        /// <summary>
        /// An instance of calculation to make a new mesh 
        /// </summary>
        MeshContainer meshContainer = null;
        /// <summary>
        /// Open the tool 
        /// </summary>
        [MenuItem("Inetum/Utils/MeshRescaller")]
        public static void ShowExample()
        {
            MeshRescaller wnd = GetWindow<MeshRescaller>();
            wnd.titleContent = new GUIContent("MeshRescaller");
            wnd.minSize = new Vector2(1200, 720);
        }

        /// <summary>
        /// Creates the UI using th UI_element framework
        /// </summary>
        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            VisualElement editorWindow = new VisualElement();
            root.Add(editorWindow);
            root.style.flexGrow = 1;

            Label label_1 = new Label("1");
            Label label_2 = new Label("2");
            Label label_3 = new Label("3");

            VisualElement top_panel = new VisualElement() { name = "top_panel" };
            VisualElement middle_panel = new VisualElement() { name = "middle_panel" };
            VisualElement bottom_panel = new VisualElement() { name = "bottom_panel" };
            editorWindow.Add(top_panel);
            editorWindow.Add(middle_panel);
            editorWindow.Add(bottom_panel);

            #region top panel
            meshContainer = new MeshContainer();
            InspectorMeshRescallerView inspectorMeshRescallerView = new InspectorMeshRescallerView();
            top_panel.Add(inspectorMeshRescallerView);
            inspectorMeshRescallerView.UpdateSelection(meshContainer);

            Button calcul_button = new Button { text = "Calculate new mesh ?" };
            top_panel.Add(calcul_button);
            calcul_button.clicked += () => CalculateNewMesh();
            #endregion

            #region middle_panel
            middle_panel.style.paddingBottom = 2;
            middle_panel.style.paddingLeft = 2;
            middle_panel.style.paddingRight = 2;
            middle_panel.style.paddingTop = 2;
            #endregion

            #region bottom panel
            bottom_panel.style.flexDirection = FlexDirection.Row;

            Button validate_button = new Button { text = "Validate ?" };
            Button exit_button = new Button { text = "Exit?" };
            validate_button.style.flexGrow = 1;
            exit_button.style.flexGrow = 1;

            bottom_panel.Add(validate_button);
            bottom_panel.Add(exit_button);

            validate_button.clicked += () => ValidateMesh();
            exit_button.clicked += () => ExitEditor();
            #endregion
        }

        /// <summary>
        /// Binded to the button calcul button :: calculate the new mesh depending on the selected scaling
        /// </summary>
        private void CalculateNewMesh()
        {
            meshContainer.CalculateNewMesh(UpdateEditorView);
        }
        /// <summary>
        /// Binded to the button validate button :: validates the obtained mesh
        /// </summary>
        private void ValidateMesh()
        {
            meshContainer.CreateNewMeshAsset();
        }
        /// <summary>
        /// Binded to the button exit button :: Close the editor and saves nothing
        /// </summary>
        private void ExitEditor()
        {
            GetWindow<MeshRescaller>().Close();
        }
        /// <summary>
        /// Update the editor to show a representation of the new scalling of the new mesh 
        /// in new version of unity it would be possible to show a 3d render.
        /// </summary>
        /// <param name="oldMesh"></param>
        /// <param name="newMesh"></param>
        /// <param name="scaling"></param>
        private void UpdateEditorView(Mesh oldMesh, Mesh newMesh, float scaling)
        {

        }
    }
}
#endif
