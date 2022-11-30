/*
Copyright 2019 - 2022 Inetum

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
    [CustomEditor(typeof(UMI3DAbstractAnimation), true)]
    [CanEditMultipleObjects]
    public class UMI3DAbstractAnimationEditor : Editor
    {
        private SerializedProperty playing;
        private SerializedProperty looping;
        private SerializedProperty startTime;
        private SerializedProperty pauseFrame;

        protected virtual void OnEnable()
        {
            playing = serializedObject.FindProperty("playing");
            looping = serializedObject.FindProperty("looping");
            startTime = serializedObject.FindProperty("startTime");
            pauseFrame = serializedObject.FindProperty("pauseFrame");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            EditorGUILayout.PropertyField(playing);
            EditorGUILayout.PropertyField(looping);
            EditorGUILayout.PropertyField(startTime);
            EditorGUILayout.PropertyField(pauseFrame);

            serializedObject.ApplyModifiedProperties();
        }
    }
}