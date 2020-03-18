/*
Copyright 2019 Gfi Informatique

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
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditorInternal;
using umi3d.edk;
using umi3d.common;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(umi3d.edk.CVEAudioSource), true)]
    [CanEditMultipleObjects]
    public class CVEAudioSourceEditor : GenericObject3DEditor
    {

        SerializedProperty Resource;
        SerializedProperty Mute;
        SerializedProperty BypassEffects;
        SerializedProperty BypassListenerEffects;
        SerializedProperty BypassReverbZone;
        SerializedProperty PlayOnAwake;
        SerializedProperty Loop;
        SerializedProperty Priority;
        SerializedProperty Volume;
        SerializedProperty Pitch;
        SerializedProperty StereoPan;
        SerializedProperty SpatialBlend;
        SerializedProperty ReverbZoneMix;
        SerializedProperty Sound3D_DopplerLevel;
        SerializedProperty Sound3D_Spread;
        SerializedProperty Sound3D_VolumeRolloff;
        SerializedProperty Sound3D_MinDistance;
        SerializedProperty Sound3D_MaxDistance;

        protected override void OnEnable()
        {
            base.OnEnable();
            Resource = serializedObject.FindProperty("AudioClipResource");
            Mute = serializedObject.FindProperty("Mute");
            BypassEffects = serializedObject.FindProperty("BypassEffects");
            BypassListenerEffects = serializedObject.FindProperty("BypassListenerEffects");
            BypassReverbZone = serializedObject.FindProperty("BypassReverbZone");
            PlayOnAwake = serializedObject.FindProperty("PlayOnAwake");
            Loop = serializedObject.FindProperty("Loop");
            Priority = serializedObject.FindProperty("Priority");
            Volume = serializedObject.FindProperty("Volume");
            Pitch = serializedObject.FindProperty("Pitch");
            StereoPan = serializedObject.FindProperty("StereoPan");
            SpatialBlend = serializedObject.FindProperty("SpatialBlend");
            ReverbZoneMix = serializedObject.FindProperty("ReverbZoneMix");
            Sound3D_DopplerLevel = serializedObject.FindProperty("Sound3D_DopplerLevel");
            Sound3D_Spread = serializedObject.FindProperty("Sound3D_Spread");
            Sound3D_VolumeRolloff = serializedObject.FindProperty("Sound3D_VolumeRolloff");
            Sound3D_MinDistance = serializedObject.FindProperty("Sound3D_MinDistance");
            Sound3D_MaxDistance = serializedObject.FindProperty("Sound3D_MaxDistance");

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(Resource, true);
            EditorGUILayout.PropertyField(Mute, true);
            EditorGUILayout.PropertyField(BypassEffects, true);
            EditorGUILayout.PropertyField(BypassListenerEffects, true);
            EditorGUILayout.PropertyField(BypassReverbZone, true);
            EditorGUILayout.PropertyField(PlayOnAwake, true);
            EditorGUILayout.PropertyField(Loop, true);
            EditorGUILayout.PropertyField(Priority, true);
            EditorGUILayout.PropertyField(Volume, true);
            EditorGUILayout.PropertyField(Pitch, true);
            EditorGUILayout.PropertyField(StereoPan, true);
            EditorGUILayout.PropertyField(SpatialBlend, true);
            EditorGUILayout.PropertyField(ReverbZoneMix, true);
            EditorGUILayout.PropertyField(Sound3D_DopplerLevel, true);
            EditorGUILayout.PropertyField(Sound3D_Spread, true);
            EditorGUILayout.PropertyField(Sound3D_VolumeRolloff, true);
            EditorGUILayout.PropertyField(Sound3D_MinDistance, true);
            EditorGUILayout.PropertyField(Sound3D_MaxDistance, true);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif