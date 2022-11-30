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

using umi3d.common;
using UnityEditor;
using UnityEngine;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(UMI3DAudioPlayer), true)]
    [CanEditMultipleObjects]
    public class UMI3DAudioPlayerEditor : UMI3DAbstractAnimationEditor
    {
        private SerializedProperty node;
        private SerializedProperty variants;
        private SerializedProperty volume;
        private SerializedProperty pitch;
        private SerializedProperty spatialBlend;
        private SerializedProperty maxDistance;
        private SerializedProperty volumeAttenuationMode;
        private SerializedProperty volumeCustomCurve;

        GUILayoutOption[] options = new GUILayoutOption[] { };

        protected override void OnEnable()
        {
            base.OnEnable();

            node = serializedObject.FindProperty("node");
            variants = serializedObject.FindProperty("audioResources");
            volume = serializedObject.FindProperty("volume");
            pitch = serializedObject.FindProperty("pitch");
            maxDistance = serializedObject.FindProperty("maxDistance");
            volumeAttenuationMode = serializedObject.FindProperty("volumeAttenuationMode");
            spatialBlend = serializedObject.FindProperty("spatialBlend");
            volumeCustomCurve = serializedObject.FindProperty("volumeCustomCurve");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            EditorGUILayout.PropertyField(node);
            EditorGUILayout.PropertyField(variants, true);
            EditorGUILayout.Slider(volume, 0, 1, options);
            EditorGUILayout.Slider(pitch, 0, 1, options);

            EditorGUILayout.LabelField("Spatialization", EditorStyles.boldLabel);

            DrawSliderWithCaption(spatialBlend, "Spatial Blend", "2D", "3D");

            EditorGUILayout.PropertyField(maxDistance, true);
            EditorGUILayout.PropertyField(volumeAttenuationMode, true);

            if (volumeAttenuationMode.enumValueIndex == (int)AudioSourceCurveMode.Custom)
                EditorGUILayout.CurveField(volumeCustomCurve, Color.red, new Rect { x = 0, y = 0, width = (maxDistance.floatValue >= 0 ? maxDistance.floatValue : 0), height = 1 }, options);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSliderWithCaption(SerializedProperty property, string label, string rightCaption, string leftCaption)
        {
            Rect position = EditorGUILayout.GetControlRect(false, 2 * EditorGUIUtility.singleLineHeight); // Get two lines for the control
            position.height *= 0.5f;
            property.floatValue = EditorGUI.Slider(position, label, property.floatValue, 0, 1);
            // Set the rect for the sub-labels
            position.y += position.height;
            position.x += EditorGUIUtility.labelWidth;
            position.width -= EditorGUIUtility.labelWidth + 54; //54 seems to be the width of the slider's float field
                                                                //sub-labels
            GUIStyle style = GUI.skin.label;
            style.alignment = TextAnchor.UpperLeft; EditorGUI.LabelField(position, rightCaption, style);
            style.alignment = TextAnchor.UpperRight; EditorGUI.LabelField(position, leftCaption, style);
        }
    }
}