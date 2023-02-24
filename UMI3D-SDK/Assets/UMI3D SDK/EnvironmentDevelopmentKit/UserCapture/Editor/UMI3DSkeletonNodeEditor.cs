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

using umi3d.edk.editor;
using UnityEditor;

namespace umi3d.edk.userCapture.editor
{
    [CustomEditor(typeof(UMI3DSkeletonNode), true)]
    [CanEditMultipleObjects]
    public class UMI3DSkeletonNodeEditor : UMI3DModelEditor
    {
        private SerializedProperty animationsStates;

        protected override void OnEnable()
        {
            base.OnEnable();
            animationsStates = serializedObject.FindProperty("animationsStates");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            EditorGUILayout.PropertyField(animationsStates);

            serializedObject.ApplyModifiedProperties();
        }
    }
}