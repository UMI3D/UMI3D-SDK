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
using umi3d.common.userCapture.pose;

using UnityEditor;

using UnityEngine;

namespace umi3d.edk.userCapture.pose.editor
{
    [CustomEditor(typeof(UMI3DPoseAnimator), true)]
    [CanEditMultipleObjects]
    public class UMI3DPoseAnimatorEditor : Editor
    {
        private SerializedProperty poseSOField;
        private SerializedProperty durationField;

        private SerializedProperty interpolableField;
        private SerializedProperty composableField;

        private SerializedProperty activationModeField;

        private SerializedProperty hasMagnitudeConditionField;
        private SerializedProperty magnitudeField;
        private SerializedProperty boneOriginField;
        private SerializedProperty relativeNodeField;

        private SerializedProperty hasDirectionConditionField;
        private SerializedProperty directionField;

        private SerializedProperty hasScaleConditionField;
        private SerializedProperty targetScaleField;

        private readonly Color magnitudeSphereColor = Color.red;
        private readonly Color magnitudeHandlesColor = Color.magenta;

        protected virtual void OnEnable()
        {
            poseSOField = serializedObject.FindProperty("pose_so");
            durationField = serializedObject.FindProperty("duration");

            interpolableField = serializedObject.FindProperty("interpolable");
            composableField = serializedObject.FindProperty("composable");

            activationModeField = serializedObject.FindProperty("activationMode");

            hasMagnitudeConditionField = serializedObject.FindProperty("HasMagnitudeCondition");
            magnitudeField = serializedObject.FindProperty("Magnitude");
            boneOriginField = serializedObject.FindProperty("BoneOrigin");
            relativeNodeField = serializedObject.FindProperty("relativeNode");

            hasDirectionConditionField = serializedObject.FindProperty("HasDirectionCondition");
            directionField = serializedObject.FindProperty("Direction");

            hasScaleConditionField = serializedObject.FindProperty("HasScaleCondition");
            targetScaleField = serializedObject.FindProperty("TargetScale");
        }

        protected virtual void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(poseSOField);
            EditorGUILayout.PropertyField(durationField);

            EditorGUILayout.PropertyField(interpolableField);
            EditorGUILayout.PropertyField(composableField);

            EditorGUILayout.PropertyField(activationModeField);

            EditorGUILayout.PropertyField(hasMagnitudeConditionField);
            EditorGUILayout.PropertyField(magnitudeField);
            EditorGUILayout.PropertyField(boneOriginField);
            EditorGUILayout.PropertyField(relativeNodeField);

            EditorGUILayout.PropertyField(hasDirectionConditionField);
            EditorGUILayout.PropertyField(directionField);

            EditorGUILayout.PropertyField(hasScaleConditionField);
            EditorGUILayout.PropertyField(targetScaleField);
        }

        public override void OnInspectorGUI()
        {
            OnInspectorGUIInternal();

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            if (hasMagnitudeConditionField.boolValue)
            {
                var t = target as UMI3DPoseAnimator;
                Handles.color = magnitudeSphereColor;

                Transform targetTransform = t.relativeNode == null ? t.transform : t.relativeNode.transform;

                var capPos = targetTransform.position + t.Magnitude * Vector3.forward;
                t.Magnitude = Handles.ScaleValueHandle(t.Magnitude,
                                                                    capPos,
                                                                    Quaternion.identity,
                                                                    0.25f,
                                                                    Handles.SphereHandleCap,
                                                                    1f);

                Handles.color = magnitudeHandlesColor;
                Handles.DrawWireDisc(targetTransform.position, Vector3.up, t.Magnitude);
                Handles.DrawWireDisc(targetTransform.position, Vector3.right, t.Magnitude);
                Handles.DrawWireDisc(targetTransform.position, Vector3.forward, t.Magnitude);
                Handles.DrawDottedLine(targetTransform.position, capPos, 5);
            }

        }
    }
}
#endif
