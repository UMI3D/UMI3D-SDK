#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
