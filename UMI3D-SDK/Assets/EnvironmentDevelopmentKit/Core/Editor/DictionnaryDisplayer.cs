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

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace umi3d.edk.editor
{

    public class DictionnaryDisplayer<T, L>
    {
        const char upArrow = '\u25B2';
        const char downArrow = '\u25bc';
        const char cross = 'X';
        const int buttonWidth = 25;
        bool displayArrows = false;

        Func<SerializedProperty, SerializedProperty, KeyValuePair<T, L>> NewValue;

        public DictionnaryDisplayer(Func<SerializedProperty, SerializedProperty, KeyValuePair<T, L>> newValue, bool displayArrow = false)
        {
            NewValue = newValue;
            this.displayArrows = displayArrow;
        }

        public DictionnaryDisplayer(Func<T> newKey, bool displayArrow = false)
        {
            NewValue = (SerializedProperty k, SerializedProperty v) => { return new KeyValuePair<T, L>(newKey.Invoke(), default); };
            this.displayArrows = displayArrow;
        }

        /// <summary>
        /// Display a seriazible dictionnary
        /// </summary>
        /// <param name="showDict"></param>
        /// <param name="KeyList"></param>
        /// <param name="ValueList"></param>
        /// <param name="Dictionnary"></param>
        public void Display(ref bool showDict, SerializedProperty KeyList, SerializedProperty ValueList, Dictionary<T, L> Dictionnary)
        {
            showDict = EditorGUILayout.Foldout(showDict, KeyList.displayName, true);
            if (showDict)
            {
                int DictSize;
                EditorGUI.indentLevel++;
                int indent = EditorGUI.indentLevel;
                EditorGUILayout.BeginHorizontal();
                DictSize = KeyList.arraySize;
                EditorGUI.BeginChangeCheck();
                DictSize = EditorGUILayout.IntField("Size", DictSize);
                if (EditorGUI.EndChangeCheck())
                    if (DictSize != KeyList.arraySize)
                    {
                        while (DictSize > KeyList.arraySize)
                        {
                            KeyList.InsertArrayElementAtIndex(KeyList.arraySize);
                            ValueList.InsertArrayElementAtIndex(ValueList.arraySize);
                        }
                        while (DictSize < KeyList.arraySize)
                        {
                            KeyList.DeleteArrayElementAtIndex(KeyList.arraySize - 1);
                            ValueList.DeleteArrayElementAtIndex(ValueList.arraySize - 1);
                        }
                    }
                if (GUILayout.Button("Add"))
                {
                    if (KeyList.arraySize == 0)
                    {
                        var k = NewValue(null, null);
                        Dictionnary.Add(k.Key, k.Value);
                    }
                    else
                    {
                        var k = NewValue(KeyList.GetArrayElementAtIndex(KeyList.arraySize - 1), ValueList.GetArrayElementAtIndex(ValueList.arraySize - 1));
                        Dictionnary.Add(k.Key, k.Value);
                    }
                }
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < KeyList.arraySize; i++)
                {
                    SerializedProperty keyRef = KeyList.GetArrayElementAtIndex(i);
                    SerializedProperty ValueRef = ValueList.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(keyRef.displayName);
                    EditorGUI.indentLevel = 0;
                    EditorGUILayout.PropertyField(keyRef, GUIContent.none);
                    EditorGUILayout.PropertyField(ValueRef, GUIContent.none);
                    if (displayArrows)
                    {
                        EditorGUI.BeginDisabledGroup(i == 0);
                        if (GUILayout.Button(upArrow.ToString(), GUILayout.MaxWidth(buttonWidth)))
                        {
                            KeyList.MoveArrayElement(i, i - 1);
                            ValueList.MoveArrayElement(i, i - 1);
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.BeginDisabledGroup(i == KeyList.arraySize - 1);
                        if (GUILayout.Button(downArrow.ToString(), GUILayout.MaxWidth(buttonWidth)))
                        {
                            KeyList.MoveArrayElement(i, i + 1);
                            ValueList.MoveArrayElement(i, i + 1);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    if (GUILayout.Button(cross.ToString(), GUILayout.Width(buttonWidth)))
                    {
                        var elementProperty = KeyList.GetArrayElementAtIndex(i);
                        if (elementProperty.propertyType == SerializedPropertyType.ObjectReference && elementProperty.objectReferenceValue != default)
                            elementProperty.objectReferenceValue = default;
                        var elementProperty1 = ValueList.GetArrayElementAtIndex(i);
                        if (elementProperty1.propertyType == SerializedPropertyType.ObjectReference && elementProperty1.objectReferenceValue != default)
                            elementProperty1.objectReferenceValue = default;
                        KeyList.DeleteArrayElementAtIndex(i);
                        ValueList.DeleteArrayElementAtIndex(i);
                    }
                    EditorGUI.indentLevel = indent;
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
        }
    }
}
#endif