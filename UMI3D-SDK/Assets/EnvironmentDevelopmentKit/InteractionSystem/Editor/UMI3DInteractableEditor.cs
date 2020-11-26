/*
Copyright 2019 Gfi Informatique

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

using umi3d.edk.interaction;
using UnityEditor;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(UMI3DInteractable), true)]
    public class UMI3DInteractableEditor : UMI3DAbstractToolEditor
    {
        SerializedProperty Node;
        SerializedProperty NotifySubObject;
        SerializedProperty NotifyHoverPosition;
        SerializedProperty onHoverEnter;
        SerializedProperty onHoverExit;
        SerializedProperty onHovered;

        ///<inheritdoc/>
        protected override void OnEnable()
        {
            base.OnEnable();
            Node = _target.FindProperty("Node");
            NotifyHoverPosition = _target.FindProperty("NotifyHoverPosition");
            NotifySubObject = _target.FindProperty("NotifySubObject");
            onHoverEnter = serializedObject.FindProperty("onHoverEnter");
            onHovered = serializedObject.FindProperty("onHovered");
            onHoverExit = serializedObject.FindProperty("onHoverExit");
        }

        static bool displayEvent = false;

        ///<inheritdoc/>
        protected override void _OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(Node);
            EditorGUILayout.PropertyField(NotifyHoverPosition);
            EditorGUILayout.PropertyField(NotifySubObject);
            base._OnInspectorGUI();
            displayEvent = EditorGUILayout.Foldout(displayEvent, "Interaction Events", true);
            if (displayEvent)
            {
                EditorGUILayout.PropertyField(onHoverEnter, true);
                EditorGUILayout.PropertyField(onHoverExit, true);
                if (NotifyHoverPosition.boolValue)
                    EditorGUILayout.PropertyField(onHovered, true);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif