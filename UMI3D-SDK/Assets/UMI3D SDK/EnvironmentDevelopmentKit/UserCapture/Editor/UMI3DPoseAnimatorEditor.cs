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
            durationField = serializedObject.FindProperty("duration");

            interpolableField = serializedObject.FindProperty("interpolable");
            composableField = serializedObject.FindProperty("composable");

            activationModeField = serializedObject.FindProperty("activationMode");

            activationConditionsField = serializedObject.FindProperty("activationConditions");
        }

        protected virtual void OnInspectorGUIInternal()
        {
            EditorGUILayout.PropertyField(poseSOField);
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
            Handles.DrawWireDisc(targetTransform.position, Vector3.up, t.Distance);
            Handles.DrawWireDisc(targetTransform.position, Vector3.right, t.Distance);
            Handles.DrawWireDisc(targetTransform.position, Vector3.forward, t.Distance);
            Handles.DrawDottedLine(targetTransform.position, capPos, 5);
            

        }
    }
}
#endif
