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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace inetum.unityUtils.editor
{
    public class TableListDrawer : UMI3DSpecialPropertyDrawer
    {
        public static readonly TableListDrawer Instance = new TableListDrawer();

        List<string> _sectionNames;
        List<SerializedProperty> _properties;

        GUIStyle _headerStyle;
        GUIStyle _rowStyle;
        GUIStyle _row2Style;

        bool _show = true;
        float _cellWidth;

        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (property.isArray)
            {
                if (_sectionNames == null)
                {
                    Initialize(property);
                }
                _properties = new List<SerializedProperty>();
                for (int i = 0; i < property.arraySize; i++)
                {
                    _properties.Add(property.GetArrayElementAtIndex(i));
                }
                bool mustBorder = false;

                EditorGUILayout.BeginVertical(GUI.skin.box);
                //Header
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("v", GUILayout.Width(19)))
                {
                    _show = !_show;
                }
                EditorGUILayout.LabelField(property.displayName + " [" + property.arraySize + " items]");
                EditorGUILayout.EndHorizontal();

                if (_show)
                {
                    bool hadChange = false;
                    // Section
                    EditorGUILayout.BeginHorizontal(_headerStyle);
                    foreach (var sectionName in _sectionNames)
                    {
                        GUILayout.Label(sectionName, GUILayout.Width(_cellWidth));
                    }
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        property.InsertArrayElementAtIndex(0);
                        hadChange = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    // Elements
                    for (int i = _properties.Count - 1; i >= 0; i--)
                    {
                        EditorGUILayout.BeginHorizontal(mustBorder ? _rowStyle : _row2Style);
                        mustBorder = !mustBorder;
                        foreach (var propertyName in _sectionNames)
                        {
                            EditorGUILayout.PropertyField(_properties[i].FindPropertyRelative(propertyName), GUIContent.none, GUILayout.Width(_cellWidth));
                        }
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            property.DeleteArrayElementAtIndex(i);
                            hadChange = true;
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (hadChange)
                        Initialize(property);
                }
                EditorGUILayout.EndVertical();
            }
            else
                Debug.LogError("TableList attribute must be placed on an Array!");
        }

        private void Initialize(SerializedProperty property)
        {
            _sectionNames = new List<string>();

            _headerStyle = new GUIStyle();
            _rowStyle = new GUIStyle();
            _row2Style = new GUIStyle();

            Type parentType = property.serializedObject.targetObject.GetType();
            FieldInfo fi = parentType.GetField(property.propertyPath);
            var arrayType = fi.FieldType;
            var propertyType = arrayType.GenericTypeArguments[0];

            foreach (var arg in propertyType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (!arg.IsPublic && !arg.CustomAttributes.Any(p => p.AttributeType.Equals(typeof(SerializeField)))) continue;
                _sectionNames.Add(arg.Name);
            }

            _cellWidth = (EditorGUIUtility.currentViewWidth - 40 - 20) / _sectionNames.Count - 2;


            _headerStyle.normal.background = MakeTex((int)_cellWidth, 20, new Color(.3f, .3f, .3f));
            _row2Style.normal.background = MakeTex((int)_cellWidth, 20, new Color(.2f, .2f, .2f));
            _rowStyle.normal.background = MakeTex((int)_cellWidth, 20, new Color(.15f, .15f, .15f));
        }

        public void ClearCache()
        {
            _sectionNames = null;
            _properties = null;

            _headerStyle = null;
            _rowStyle = null;
            _row2Style = null;

            _show = true;
            _cellWidth = 0;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        protected override float GetPropertyHeight_Internal(SerializedProperty property)
        {
            if (_properties == null) return 20;
            return (_properties.Count + 1) * 20;
        }
    }
}
#endif