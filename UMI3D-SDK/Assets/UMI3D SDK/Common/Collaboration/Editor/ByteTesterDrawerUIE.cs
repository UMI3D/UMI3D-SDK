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

using inetum.unityUtils.editor;
using UnityEditor;
using UnityEngine;
using inetum.unityUtils;
using System;
using umi3d.common.userCapture;

namespace umi3d.common
{

    [CustomEditor(typeof(OperationReader))]
    public class OperarationReaderEditor : Editor
    {
        private SerializedProperty container, testers, ignorePos;

        public void OnEnable()
        {
            container = serializedObject.FindProperty("container");
            testers = serializedObject.FindProperty("testers");
            ignorePos = serializedObject.FindProperty("ignorePos");

            //base.OnInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(container);
            EditorGUILayout.PropertyField(ignorePos);
            if (GUILayout.Button("Decrypt"))
            {
                var rd = container.GetParent() as OperationReader;
                rd.Decrypt();
            }
            EditorGUILayout.PropertyField(testers);

            serializedObject.ApplyModifiedProperties();
        }
    }


    [CustomPropertyDrawer(typeof(ByteTester))]
    public class ByteTesterDrawerUIE : PropertyDrawer
    {
        static float sl => EditorGUIUtility.singleLineHeight;
        float height;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            var obj = property.GetValue() as ByteTester;
            if (obj == null)
                return sl;
            return obj.height;

            //return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            height = 0;
            var obj = property.GetValue() as ByteTester;
            if (obj == null)
                return;

            EditorGUI.BeginDisabledGroup(true);
            Rect R1 = newRect(position, sl);
            EditorGUI.TextArea(R1, obj.container?.ToString());
            EditorGUI.EndDisabledGroup();

            Rect RE = newRect("Type To Read", position, sl);
            RE.width /= 2;
            Rect RB = new Rect(RE);
            RB.x += RE.width;
            obj.type = (ByteTester.TypeToSerialize)EditorGUI.EnumPopup(RE, obj.type);
            if (GUI.Button(RB, "Read"))
                obj.Read(obj.type);

            Rect Indent = new Rect(position);
            Indent.x += 80;
            Indent.width -= 80;

            if (obj.results != null)
                foreach (var r in obj.results)
                {
                    Rect R2 = newRect(r.type, Indent, sl);
                    R2.width /= 3;
                    Rect R3 = new Rect(R2);
                    R3.x += R2.width;
                    Rect R4 = new Rect(R3);
                    R4.x += R3.width;

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.TextField(R2, r.name);
                    EditorGUI.EndDisabledGroup();
                    r.convertType = (ByteTester.TypeToEmum)EditorGUI.EnumPopup(R3, r.convertType);
                    EditorGUI.BeginDisabledGroup(true);
                    DrawConvert(r, R4);
                    EditorGUI.EndDisabledGroup();
                }

            obj.height = height;
        }

        void DrawConvert(ByteTester.Result r, Rect rect)
        {

            if (r.value == null)
                return;

            ConstEnumAttribute a = null;

            switch (r.convertType)
            {
                case ByteTester.TypeToEmum.PropertyKey:
                    if (r.value is uint)
                        a = new ConstEnumAttribute(typeof(UMI3DPropertyKeys), typeof(uint));
                    break;
                case ByteTester.TypeToEmum.BoneType:
                    if (r.value is uint)
                        a = new ConstEnumAttribute(typeof(BoneType), typeof(uint));
                    break;
                case ByteTester.TypeToEmum.OperationKey:
                    if (r.value is uint)
                        a = new ConstEnumAttribute(typeof(UMI3DOperationKeys), typeof(uint));
                    break;
            }

            if (a != null)
            {
                var i = Array.IndexOf(a.values, r.value);
                if (i >= 0)
                    EditorGUI.TextField(rect, a.options[i]);
                else
                    EditorGUI.TextField(rect, "--");
            }
        }


        Rect newRect(string label, Rect position, float height)
        {
            float labelSize = Mathf.Min((position.width * 30f / 100f), 200);
            Rect rl = new Rect(position.x, position.y + this.height, labelSize, height);
            EditorGUI.LabelField(rl, label);

            Rect r = new Rect(position.x + labelSize, position.y + this.height, position.width - labelSize, height);
            this.height += r.height;
            return r;
        }

        Rect newRect(Rect position, float height)
        {
            Rect r = new Rect(position.x, position.y + this.height, position.width, height);
            this.height += r.height;
            return r;
        }

    }
}
#endif