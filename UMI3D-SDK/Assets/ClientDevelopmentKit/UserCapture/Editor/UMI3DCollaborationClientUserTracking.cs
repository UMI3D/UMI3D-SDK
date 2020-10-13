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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using umi3d.common.editor;
using umi3d.common.userCapture;
using umi3d.cdk.userCapture;
using umi3d.cdk.collaboration;

namespace umi3d.cdk.editor
{
    [CustomEditor(typeof(UMI3DCollaborationClientUserTracking))]
    public class UMI3DCollaborationClientUserTrackingEditor : Editor
    {
        SerializedProperty anchor;
        SerializedProperty viewpoint;
        SerializedProperty viewpointBonetype;
        SerializedProperty skeletonParsingIterationCooldown;
        SerializedProperty time;
        SerializedProperty max;

        ConstStringDisplayer constDisplayer;

        // Start is called before the first frame update
        void OnEnable()
        {
            anchor = serializedObject.FindProperty("anchor");
            viewpoint = serializedObject.FindProperty("viewpoint");
            viewpointBonetype = serializedObject.FindProperty("viewpointBonetype");
            skeletonParsingIterationCooldown = serializedObject.FindProperty("skeletonParsingIterationCooldown");
            time = serializedObject.FindProperty("time");
            max = serializedObject.FindProperty("max");

            constDisplayer = new ConstStringDisplayer(viewpointBonetype.name, typeof(BoneType), viewpointBonetype.stringValue);
        }

        // Update is called once per frame
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(anchor);
            EditorGUILayout.PropertyField(viewpoint);
            viewpointBonetype.stringValue = constDisplayer.display();
            EditorGUILayout.PropertyField(skeletonParsingIterationCooldown);
            EditorGUILayout.PropertyField(time);
            EditorGUILayout.PropertyField(max);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif