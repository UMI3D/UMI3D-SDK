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
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

namespace umi3d.edk.editor
{

    public class ListDisplayer<T>
    {
        const char upArrow = '\u25B2';
        const char downArrow = '\u25bc';
        const char cross = 'X';
        const int buttonWidth = 25;

        Func<SerializedProperty, T> NewValue;

        public ListDisplayer(Func<SerializedProperty, T> newValue)
        {
            NewValue = newValue;
        }

        public ListDisplayer()
        {
            NewValue = (SerializedProperty s)=>default;
        }

        /// <summary>
        /// Display a list whith special displayer.
        /// </summary>
        /// <param name="showList"></param>
        /// <param name="ThisList"></param>
        /// <param name="RealList"></param>
        /// <param name="converter"></param>
        public void Display(ref bool showList,SerializedProperty ThisList,List<T> RealList,Func<UnityEngine.Object,List<T>> converter = null)
        {
            showList = EditorGUILayout.Foldout(showList, ThisList.displayName, true);
            if (converter != null)
            {
                Event evt = Event.current;
                Rect rect = GUILayoutUtility.GetLastRect();
                if (rect.Contains(evt.mousePosition))
                    switch (evt.type)
                    {
                        case EventType.DragUpdated:
                        case EventType.DragPerform:
                            bool ok = false;
                            foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                            {
                                if (converter(dragged_object) != null)
                                {
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                                    ok = true;
                                    break;
                                }
                            }
                            if (ok && evt.type == EventType.DragPerform)
                            {
                                showList = true;
                                DragAndDrop.AcceptDrag();

                                foreach (UnityEngine.Object dragged_object in DragAndDrop.objectReferences)
                                {
                                    List<T> values = converter(dragged_object);
                                    if (values != null && values.Count > 0)
                                        RealList.AddRange(values);
                                }
                            }
                            evt.Use();
                            break;
                    }
            }

            if (showList)
            {
                int ListSize;
                EditorGUI.indentLevel++;
                int indent = EditorGUI.indentLevel;

                EditorGUILayout.BeginHorizontal();
                ListSize = ThisList.arraySize;
                EditorGUI.BeginChangeCheck();
                ListSize = EditorGUILayout.IntField("Size", ListSize);
                if (EditorGUI.EndChangeCheck())
                    if (ListSize != ThisList.arraySize)
                    {
                        while (ListSize > ThisList.arraySize)
                        {
                            ThisList.InsertArrayElementAtIndex(ThisList.arraySize);
                        }
                        while (ListSize < ThisList.arraySize)
                        {
                            ThisList.DeleteArrayElementAtIndex(ThisList.arraySize - 1);
                        }
                    }
                if (GUILayout.Button("Add"))
                {
                    if (ThisList.arraySize == 0)
                        RealList.Add(NewValue.Invoke(null));
                    else
                        RealList.Add(NewValue.Invoke(ThisList.GetArrayElementAtIndex(ThisList.arraySize - 1)));
                }
                EditorGUILayout.EndHorizontal();

                for (int i = 0; i < ThisList.arraySize; i++)
                {
                    SerializedProperty MyListRef = ThisList.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(MyListRef);
                    EditorGUI.indentLevel = 0;
                    EditorGUI.BeginDisabledGroup(i == 0);
                    if (GUILayout.Button(upArrow.ToString(), GUILayout.Width(buttonWidth)))
                    {
                        ThisList.MoveArrayElement(i, i - 1);
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.BeginDisabledGroup(i == ThisList.arraySize - 1);
                    if (GUILayout.Button(downArrow.ToString(), GUILayout.Width(buttonWidth)))
                    {
                        ThisList.MoveArrayElement(i, i + 1);
                    }
                    EditorGUI.EndDisabledGroup();
                    if (GUILayout.Button(cross.ToString(), GUILayout.Width(buttonWidth)))
                    {
                        var elementProperty = ThisList.GetArrayElementAtIndex(i);
                        if (elementProperty.propertyType == SerializedPropertyType.ObjectReference && elementProperty.objectReferenceValue != default)
                            elementProperty.objectReferenceValue = default;
                        ThisList.DeleteArrayElementAtIndex(i);
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