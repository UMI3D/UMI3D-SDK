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
    [CustomEditor(typeof(umi3d.edk.UICanvas), true)]
    [CanEditMultipleObjects]
    public class UICanvasEditor : GenericObject3DEditor
    {
        SerializedProperty autoCascade;

        protected override void OnEnable()
        {
            base.OnEnable();
            autoCascade = serializedObject.FindProperty("autoCascade");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(autoCascade, true);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif