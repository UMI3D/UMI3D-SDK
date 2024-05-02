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

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(UMI3DWebView), false)]
    public class UMI3DWebViewEditor : UMI3DNodeEditor
    {
        private SerializedProperty canInteract;
        private SerializedProperty isAdmin;
        private SerializedProperty size;
        private SerializedProperty textureSize;
        private SerializedProperty url;
        private SerializedProperty canBeForced;
        private SerializedProperty useWhiteList;
        private SerializedProperty whiteList;
        private SerializedProperty useBlackList;
        private SerializedProperty blackList;

        protected override void OnEnable()
        {
            base.OnEnable();

            canInteract = serializedObject.FindProperty("canInteract");
            isAdmin = serializedObject.FindProperty("isAdmin");
            size = serializedObject.FindProperty("size");
            textureSize = serializedObject.FindProperty("textureSize");
            url = serializedObject.FindProperty("url");
            canBeForced = serializedObject.FindProperty("canUrlBeForced");
            useWhiteList = serializedObject.FindProperty("useWhiteList");
            whiteList = serializedObject.FindProperty("whiteList");
            useBlackList = serializedObject.FindProperty("useBlackList");
            blackList = serializedObject.FindProperty("blackList");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(url);
            EditorGUILayout.PropertyField(canInteract);
            EditorGUILayout.PropertyField(isAdmin);
            EditorGUILayout.PropertyField(canBeForced);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(size);
            EditorGUILayout.PropertyField(textureSize);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(useWhiteList);
            if (useWhiteList.boolValue)
                EditorGUILayout.PropertyField(whiteList);
            EditorGUILayout.PropertyField(useBlackList);
            if (useBlackList.boolValue)
                EditorGUILayout.PropertyField(blackList);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif