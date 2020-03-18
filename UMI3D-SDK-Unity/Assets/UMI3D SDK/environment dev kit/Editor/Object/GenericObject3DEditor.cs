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
    [CustomEditor(typeof(umi3d.edk.AbstractObject3D), true)]
    [CanEditMultipleObjects]
    public class GenericObject3DEditor : Editor
    {
        SerializedProperty isStatic;
        SerializedProperty Xbillboard, Ybillboard;
        SerializedProperty ImmerseiveOnly;
        SerializedProperty interactable;
        SerializedProperty onHoverEnter;
        SerializedProperty onHoverExit;
        SerializedProperty onHovered;
        SerializedProperty trackHoverPosition;

        protected virtual void OnEnable()
        {
            isStatic = serializedObject.FindProperty("isStatic");

            Xbillboard = serializedObject.FindProperty("Xbillboard");
            Ybillboard = serializedObject.FindProperty("Ybillboard");
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

            float filedWidth = EditorGUIUtility.fieldWidth;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.fieldWidth = 0;
            EditorGUIUtility.labelWidth = 0;
            EditorGUILayout.PropertyField(isStatic);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Billboard", "Force the forward vector of the object in a client to be colinear with the forward vector of the client viewpoint"), GUILayout.Width(labelWidth));
            EditorGUILayout.LabelField(new GUIContent("X", "Enable the billboard on world axis X"), GUILayout.Width(10));
            EditorGUILayout.PropertyField(Xbillboard, GUIContent.none, true, GUILayout.Width(10));
            EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(10));
            EditorGUILayout.LabelField(new GUIContent("Y", "Enable the billboard on world axis Y"), GUILayout.Width(10));
            EditorGUILayout.PropertyField(Ybillboard, GUIContent.none, true, GUILayout.Width(10));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.fieldWidth = filedWidth;


            EditorGUILayout.PropertyField(ImmerseiveOnly);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif