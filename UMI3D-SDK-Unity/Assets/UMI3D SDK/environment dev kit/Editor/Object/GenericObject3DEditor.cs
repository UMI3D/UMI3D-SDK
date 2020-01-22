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
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditorInternal;
using umi3d.edk;
using umi3d.common;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(umi3d.edk.GenericObject3D),true)]
    [CanEditMultipleObjects]
    public class GenericObject3DEditor : Editor
    {
        SerializedProperty isStatic;
        SerializedProperty billboard;
        SerializedProperty ImmerseiveOnly;
        SerializedProperty interactable;
        SerializedProperty onHoverEnter;
        SerializedProperty onHoverExit;
        SerializedProperty onHovered;
        SerializedProperty trackHoverPosition;

        protected virtual void OnEnable()
        {
            isStatic = serializedObject.FindProperty("isStatic");

            billboard = serializedObject.FindProperty("billboard");
            ImmerseiveOnly = serializedObject.FindProperty("immersiveOnly");

            interactable = serializedObject.FindProperty("interactable");

            onHoverEnter = serializedObject.FindProperty("onHoverEnter");
            onHovered = serializedObject.FindProperty("onHovered");
            onHoverExit = serializedObject.FindProperty("onHoverExit");
            trackHoverPosition = serializedObject.FindProperty("trackHoverPosition");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(interactable);

            if (interactable.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(trackHoverPosition);

                EditorGUILayout.PropertyField(onHoverEnter);
                EditorGUILayout.PropertyField(onHoverExit);
                if (trackHoverPosition.boolValue)
                    EditorGUILayout.PropertyField(onHovered);

                EditorGUILayout.Space();
            }


            EditorGUILayout.PropertyField(isStatic);
            EditorGUILayout.PropertyField(billboard);
            EditorGUILayout.PropertyField(ImmerseiveOnly);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif