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

using UnityEditor;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(UMI3DAnimatorAnimation), true)]
    [CanEditMultipleObjects]
    public class UMI3DAnimatorAnimationEditor : UMI3DAbstractAnimationEditor
    {
        private SerializedProperty node;
        private SerializedProperty stateName;

        protected override void OnEnable()
        {
            base.OnEnable();

            node = serializedObject.FindProperty("node");
            stateName = serializedObject.FindProperty("stateName");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            EditorGUILayout.PropertyField(node);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            EditorGUILayout.PropertyField(stateName);
            EditorGUILayout.LabelField("Animator parameters can be changed via script (objectParameters field).");
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
        }
    }
}