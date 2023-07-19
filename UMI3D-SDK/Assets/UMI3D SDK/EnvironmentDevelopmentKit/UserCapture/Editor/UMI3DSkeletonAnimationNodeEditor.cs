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
using umi3d.common.userCapture.animation;
using umi3d.edk.editor;
using UnityEditor;

namespace umi3d.edk.userCapture.animation.editor
{
    /// <summary>
    /// Custom editor for <see cref="UMI3DSkeletonAnimationNode"/>.
    /// </summary>
    [CustomEditor(typeof(UMI3DSkeletonAnimationNode), true)]
    [CanEditMultipleObjects]
    public class UMI3DSkeletonAnimationNodeEditor : UMI3DModelEditor
    {
        private SerializedProperty animationsStates;
        private SerializedProperty relatedAnimationIds;
        private SerializedProperty priority;

        private UMI3DSkeletonAnimationNode targetObject;

        protected override void OnEnable()
        {
            base.OnEnable();
            animationsStates = serializedObject.FindProperty("animationStates");
            relatedAnimationIds = serializedObject.FindProperty("relatedAnimationIds");
            priority = serializedObject.FindProperty("priority");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            targetObject ??= target as UMI3DSkeletonAnimationNode;

            EditorGUI.BeginChangeCheck();

            serializedObject.Update();
            EditorGUILayout.PropertyField(animationsStates);
            EditorGUILayout.PropertyField(relatedAnimationIds);
            EditorGUILayout.PropertyField(priority);
            EditorGUILayout.LabelField("Animator self-tracked parameters:");

            if (targetObject != null)
            {
                foreach (var parameter in targetObject.animatorSelfTrackedParameters)
                {
                    EditorGUILayout.LabelField($"- {(SkeletonAnimatorParameterKeys)parameter.parameterKey}");
                    EditorGUILayout.LabelField($"  Ranges:");
                    if (parameter.ranges.Count > 0)
                    {
                        foreach (var range in parameter.ranges)
                        {
                            EditorGUILayout.LabelField($"   - [{range.startBound};{range.endBound} -> {(range.rawValue ? "Raw" : range.result)}");
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField($"   None");
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif