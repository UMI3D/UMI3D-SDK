/*
Copyright 2019 - 2024 Inetum

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
using UnityEngine;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(UMI3DVideoPlayer), true)]
    [CanEditMultipleObjects]
    public class UMI3DVideoPlayerEditor : UMI3DAbstractAnimationEditor
    {
        private SerializedProperty video;
        private SerializedProperty material;
        private SerializedProperty videoResources;
        private SerializedProperty audioPlayer;

        GUILayoutOption[] options = new GUILayoutOption[] { };

        protected override void OnEnable()
        {
            base.OnEnable();

            video = serializedObject.FindProperty("video");
            material = serializedObject.FindProperty("material");
            videoResources = serializedObject.FindProperty("videoResources");
            audioPlayer = serializedObject.FindProperty("audioPlayer");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            EditorGUILayout.PropertyField(material);
            EditorGUILayout.PropertyField(videoResources);
            EditorGUILayout.PropertyField(audioPlayer);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(video);

            serializedObject.ApplyModifiedProperties();
        }
    }
}