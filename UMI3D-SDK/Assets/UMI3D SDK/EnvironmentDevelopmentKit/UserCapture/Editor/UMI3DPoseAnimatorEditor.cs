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
using System.Collections.Generic;
using System.Linq;

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

        private SerializedProperty isAnchoredField;
        private SerializedProperty anchoringParametersField;

        private SerializedProperty durationField;

        private SerializedProperty interpolableField;
        private SerializedProperty composableField;

        private SerializedProperty activationModeField;

        private SerializedProperty activationConditionsField;

        private readonly Color magnitudeSphereColor = Color.red;
        private readonly Color magnitudeHandlesColor = Color.magenta;

        protected virtual void OnEnable()
        {
            poseSOField = serializedObject.FindProperty("pose_so");

            isAnchoredField = serializedObject.FindProperty("isAnchored");
            anchoringParametersField = serializedObject.FindProperty("anchoringParameters");

            durationField = serializedObject.FindProperty("duration");

            interpolableField = serializedObject.FindProperty("interpolable");
            composableField = serializedObject.FindProperty("composable");

            activationModeField = serializedObject.FindProperty("activationMode");

            activationConditionsField = serializedObject.FindProperty("activationConditions");
        }

        protected virtual void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(poseSOField);

            EditorGUILayout.PropertyField(isAnchoredField);
            if (isAnchoredField.boolValue)
                EditorGUILayout.PropertyField(anchoringParametersField);

            EditorGUILayout.PropertyField(durationField);

            EditorGUILayout.PropertyField(interpolableField);
            EditorGUILayout.PropertyField(composableField);

            EditorGUILayout.PropertyField(activationModeField);

            EditorGUILayout.PropertyField(activationConditionsField);
        }

        public override void OnInspectorGUI()
        {
            OnInspectorGUIInternal();

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            UMI3DPoseAnimator animator = target as UMI3DPoseAnimator;
            if (animator == null)
                return;


            MagnitudeCondition t = null;
            foreach (var conditionField in animator.activationConditions)
            {
                if (conditionField.conditionType == BrowserPoseAnimatorActivationConditionField.ConditionType.MAGNITUDE)
                {
                    t = conditionField.magnitudeCondition;
                    break;
                }
            }
            if (t == null)
                return;


            Handles.color = magnitudeSphereColor;

            Transform targetTransform = t.RelativeNode == null ? animator.transform : t.RelativeNode.transform;

            var capPos = targetTransform.position + t.Distance * Vector3.forward;
            t.Distance = Handles.ScaleValueHandle(t.Distance,
                                                                capPos,
                                                                Quaternion.identity,
                                                                0.25f,
                                                                Handles.SphereHandleCap,
                                                                1f);

            Handles.color = magnitudeHandlesColor;

            var previousZtest = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            DrawMagnitudeZone(targetTransform, t);

            Handles.zTest = previousZtest;
            Handles.DrawDottedLine(targetTransform.position, capPos, 5);
        }

        private void DrawMagnitudeZone(Transform targetTransform, MagnitudeCondition t)
        {
            Handles.DrawWireDisc(targetTransform.position, Vector3.up, t.Distance);

            if (t.IgnoreHeight) // create a nearly infinite cylinder
            {
                for (int i = 0; i < NUMBER_OF_CYLINDER_LINES; i++)
                {
                    float angle = 2 * Mathf.PI * i / NUMBER_OF_CYLINDER_LINES;
                    Vector3 pos = t.Distance * (Mathf.Cos(angle) * Vector3.forward + Mathf.Sin(angle) * Vector3.left);
                    Handles.DrawDottedLine(targetTransform.position + pos + Vector3.up * Y_LIMIT,
                                           targetTransform.position + pos - Vector3.up * Y_LIMIT, DASH_SIZE);
                }
            }
            else // create a sphere by rotating disks
            {
                Handles.color = magnitudeHandlesColor * new Color(1, 1, 1, 0.5f);
                for (int i = 0; i < NUMBER_OF_DISK_LINES; i++)
                {
                    float angle = 2 * Mathf.PI * i / NUMBER_OF_DISK_LINES;
                    Vector3 normal = t.Distance * (Mathf.Cos(angle) * Vector3.forward + Mathf.Sin(angle) * Vector3.left);
                    Handles.DrawWireDisc(targetTransform.position, normal, t.Distance);
                }
            }
        }

        const float Y_LIMIT = 20;
        const float DASH_SIZE = 2;
        const int NUMBER_OF_CYLINDER_LINES = 32;
        const int NUMBER_OF_DISK_LINES = 12;
    }
}
#endif
