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


using System;
using System.Collections.Generic;
using umi3d.common;
using UnityEditor;
using UnityEngine;

namespace umi3d.edk.editor
{
    [CustomEditor(typeof(UMI3DAnimatorAnimation))]
    public class UMI3DAnimatorAnimationEditor : UMI3DAbstractAnimationEditor
    {
        private SerializedProperty node;
        private SerializedProperty stateName;

        protected override void OnEnable()
        {
            base.OnEnable();
            node = serializedObject.FindProperty("node");
            stateName = serializedObject.FindProperty("stateName");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            EditorGUILayout.PropertyField(playing);
            EditorGUILayout.PropertyField(looping);
            EditorGUILayout.PropertyField(startTime);
            EditorGUILayout.PropertyField(pauseFrame);
            EditorGUILayout.PropertyField(node);
            EditorGUILayout.PropertyField(stateName);

            serializedObject.ApplyModifiedProperties();
        }



    }
}
