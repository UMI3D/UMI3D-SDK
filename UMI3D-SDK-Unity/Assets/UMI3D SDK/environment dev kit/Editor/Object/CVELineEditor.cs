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
    [CustomEditor(typeof(umi3d.edk.CVELine), true)]
    [CanEditMultipleObjects]
    public class CVELineEditor : GenericObject3DEditor
    {

        SerializedProperty loop;
        SerializedProperty width;
        SerializedProperty points;
        SerializedProperty color;

        protected override void OnEnable()
        {
            base.OnEnable();
            loop = serializedObject.FindProperty("loop");
            width = serializedObject.FindProperty("width");
            points = serializedObject.FindProperty("points");
            color = serializedObject.FindProperty("color");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(loop, true);
            EditorGUILayout.PropertyField(width, true);
            EditorGUILayout.PropertyField(points, true);
            EditorGUILayout.PropertyField(color, true);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif