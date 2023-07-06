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

using umi3d.common.userCapture.animation;
using umi3d.edk.editor;
using UnityEditor;

namespace umi3d.edk.userCapture.animation.editor
{
    [CustomEditor(typeof(UMI3DSkeletonAnimationNode), true)]
    [CanEditMultipleObjects]
    public class UMI3DSkeletonAnimationNodeEditor : UMI3DModelEditor
    {
        private SerializedProperty animationsStates;
        private SerializedProperty relatedAnimationIds;
        private SerializedProperty priority;
        private SerializedProperty animatorSelfTrackedParameters;

        protected override void OnEnable()
        {
            base.OnEnable();
            animationsStates = serializedObject.FindProperty("animationStates");
            relatedAnimationIds = serializedObject.FindProperty("relatedAnimationIds");
            priority = serializedObject.FindProperty("priority");
            animatorSelfTrackedParameters = serializedObject.FindProperty("animatorSelfTrackedParameters");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            EditorGUILayout.PropertyField(animationsStates);
            EditorGUILayout.PropertyField(relatedAnimationIds);
            EditorGUILayout.PropertyField(priority);
            EditorGUILayout.LabelField("Animator self-tracked parameters:");
            for (int i = 0; i < animatorSelfTrackedParameters.arraySize; i++)
                EditorGUILayout.LabelField($"- {animatorSelfTrackedParameters.GetArrayElementAtIndex(i)}");

            serializedObject.ApplyModifiedProperties();
        }
    }
}