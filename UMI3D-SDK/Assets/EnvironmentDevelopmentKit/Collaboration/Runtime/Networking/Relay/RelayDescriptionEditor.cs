/*
Copyright 2019 - 2021 Inetum

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

using umi3d.edk.collaboration;
using UnityEditor;
using UnityEngine;


namespace umi3d.edk.collaboration.editor
{
    [CustomEditor(typeof(RelayDescription), true)]
    [CanEditMultipleObjects]
    public class RelayDescriptionEditor : Editor
    {
        static bool showInsideVolume = false;
        static bool showOutsideVolume = false; 

        SerializedProperty channelType;

        SerializedProperty insideVolumeSendData;
        SerializedProperty insideDataRule;
        SerializedProperty insideConstantFPS;
        SerializedProperty insideMinProximityFPS;
        SerializedProperty insideMaxProximityFPS;
        SerializedProperty insideStartingProximityDistance;
        SerializedProperty insideStoppingProximityDistance;

        SerializedProperty outsideVolumeSendData;
        SerializedProperty outsideDataRule;
        SerializedProperty outsideConstantFPS;
        SerializedProperty outsideMinProximityFPS;
        SerializedProperty outsideMaxProximityFPS;
        SerializedProperty outsideStartingProximityDistance;
        SerializedProperty outsideStoppingProximityDistance;

        protected virtual void OnEnable()
        {
            channelType = serializedObject.FindProperty("channelType");

            insideVolumeSendData = serializedObject.FindProperty("InsideVolume.sendData");
            insideDataRule = serializedObject.FindProperty("InsideVolume.dataRule");
            insideConstantFPS = serializedObject.FindProperty("InsideVolume.constantFPS");
            insideMinProximityFPS = serializedObject.FindProperty("InsideVolume.minProximityFPS");
            insideMaxProximityFPS = serializedObject.FindProperty("InsideVolume.maxProximityFPS");
            insideStartingProximityDistance = serializedObject.FindProperty("InsideVolume.startingProximityDistance");
            insideStoppingProximityDistance = serializedObject.FindProperty("InsideVolume.stoppingProximityDistance");

            outsideVolumeSendData = serializedObject.FindProperty("OutsideVolume.sendData");
            outsideDataRule = serializedObject.FindProperty("OutsideVolume.dataRule");
            outsideConstantFPS = serializedObject.FindProperty("OutsideVolume.constantFPS");
            outsideMinProximityFPS = serializedObject.FindProperty("OutsideVolume.minProximityFPS");
            outsideMaxProximityFPS = serializedObject.FindProperty("OutsideVolume.maxProximityFPS");
            outsideStartingProximityDistance = serializedObject.FindProperty("OutsideVolume.startingProximityDistance");
            outsideStoppingProximityDistance = serializedObject.FindProperty("OutsideVolume.stoppingProximityDistance");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            EditorGUILayout.PropertyField(channelType);
            EditorGUILayout.Space();
            showInsideVolume = EditorGUILayout.Foldout(showInsideVolume, "Inside Volume");

            if (showInsideVolume)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(insideVolumeSendData);

                if (insideVolumeSendData.boolValue)
                {
                    EditorGUILayout.Space(0.3f);
                    EditorGUILayout.PropertyField(insideDataRule);

                    if (insideDataRule.intValue == 0)
                    {
                        EditorGUILayout.Space(0.3f);
                        EditorGUILayout.PropertyField(insideConstantFPS);
                    }
                    else if (insideDataRule.intValue == 1)
                    {
                        EditorGUILayout.Space(0.3f);
                        EditorGUILayout.PropertyField(insideMinProximityFPS);
                        EditorGUILayout.PropertyField(insideMaxProximityFPS);
                        EditorGUILayout.PropertyField(insideStartingProximityDistance);
                        EditorGUILayout.PropertyField(insideStoppingProximityDistance);
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            showOutsideVolume = EditorGUILayout.Foldout(showOutsideVolume, "Outside Volume");

            if (showOutsideVolume)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(outsideVolumeSendData);

                if (outsideVolumeSendData.boolValue)
                {
                    EditorGUILayout.Space(0.3f);
                    EditorGUILayout.PropertyField(outsideDataRule);

                    if (outsideDataRule.intValue == 0)
                    {
                        EditorGUILayout.Space(0.3f);
                        EditorGUILayout.PropertyField(outsideConstantFPS);
                    }
                    else if (outsideDataRule.intValue == 1)
                    {
                        EditorGUILayout.Space(0.3f);
                        EditorGUILayout.PropertyField(outsideMinProximityFPS);
                        EditorGUILayout.PropertyField(outsideMaxProximityFPS);
                        EditorGUILayout.PropertyField(outsideStartingProximityDistance);
                        EditorGUILayout.PropertyField(outsideStoppingProximityDistance);
                    }
                }
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif