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

using umi3d.edk.userCapture.pose.editor;
using UnityEditor;

namespace umi3d.edk.collaboration.userCapture.pose.editor
{
    [CustomEditor(typeof(UMI3DCollaborationPoseAnimator), true)]
    public class UMI3DCollaborationPoseAnimatorEditor : UMI3DPoseAnimatorEditor
    {
        private SerializedProperty hasProjectionConditionField;
        private SerializedProperty interactableField;

        protected override void OnEnable()
        {
            base.OnEnable();

            hasProjectionConditionField = serializedObject.FindProperty("HasProjectionCondition");
            interactableField = serializedObject.FindProperty("interactable");
        }

        protected override void OnInspectorGUIInternal()
        {
            base.OnInspectorGUIInternal();

            EditorGUILayout.PropertyField(hasProjectionConditionField);
            EditorGUILayout.PropertyField(interactableField);
        }
    }
}

#endif