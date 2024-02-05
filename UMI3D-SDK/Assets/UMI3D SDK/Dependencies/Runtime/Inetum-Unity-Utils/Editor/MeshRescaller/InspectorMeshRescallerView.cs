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
using UnityEngine.UIElements;
using UnityEditor;


namespace inetum.unityUtils.editor
{
    public class InspectorMeshRescallerView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorMeshRescallerView, VisualElement.UxmlTraits>
        {

        }

        /// <summary>
        /// The inspector instance in the tool
        /// </summary>
        Editor editor;

        public InspectorMeshRescallerView()
        {

        }

        /// <summary>
        /// A fonction to show the unity inspector in the tool
        /// </summary>
        /// <param name="meshContainer"></param>
        internal void UpdateSelection(MeshContainer meshContainer)
        {
            Clear();

            UnityEngine.Object.DestroyImmediate(editor);

            editor = Editor.CreateEditor(meshContainer);
            IMGUIContainer container = new IMGUIContainer(() => {
                if (editor.target) editor.OnInspectorGUI();
            });
            Add(container);
        }
    }
}
#endif
