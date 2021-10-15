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
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace inetum.unityUtils.editor
{
    public static class ListReadOnlyDisplayer
    {

        public static void Display(SerializedProperty property)
        {

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    Display(property.GetName(), property.intValue);
                    break;
                case SerializedPropertyType.Boolean:
                    Display(property.GetName(), property.boolValue);
                    break;
                case SerializedPropertyType.Float:
                    Display(property.GetName(), property.floatValue);
                    break;
                case SerializedPropertyType.String:
                    Display(property.GetName(), property.stringValue);
                    break;
                case SerializedPropertyType.Enum:
                    Display(property.GetName(), property.enumNames[property.enumValueIndex]);
                    break;
                case SerializedPropertyType.Vector2:
                    {
                        var v = property.vector2Value;
                        Display(property.GetName(), $"{v.x}{v.y}");
                    }
                    break;
                case SerializedPropertyType.Vector3:
                    {
                        var v = property.vector3Value;
                        Display(property.GetName(), $"{v.x}{v.y}{v.z}");
                    }
                    break;
                case SerializedPropertyType.Vector4:
                    {
                        var v = property.vector4Value;
                        Display(property.GetName(), $"{v.x}{v.y}{v.z}{v.w}");
                    }
                    break;
                case SerializedPropertyType.Quaternion:
                    {
                        var v = property.quaternionValue.eulerAngles;
                        Display(property.GetName(), $"{v.x}{v.y}{v.z}");
                    }
                    break;
                case SerializedPropertyType.Vector2Int:
                    {
                        var v = property.vector2IntValue;
                        Display(property.GetName(), $"{v.x}{v.y}");
                    }
                    break;
                case SerializedPropertyType.Vector3Int:
                    {
                        var v = property.vector3IntValue;
                        Display(property.GetName(), $"{v.x}{v.y}{v.z}");
                    }
                    break;
                default:
                    DisplayOther(property);
                    break;
            }
        }

        static void Display(string name, object value)
        {
            var h = GUILayout.Height(15);
            EditorGUILayout.BeginHorizontal(h);

            EditorGUILayout.SelectableLabel(name, h);
            EditorGUILayout.SelectableLabel(value.ToString(), h);
            EditorGUILayout.EndHorizontal();
        }


        static void DisplayOther(SerializedProperty property)
        {
            var h = GUILayout.Height(15);
            if (property.isArray)
            {
                if (property.arraySize > 0)
                {
                    EditorGUILayout.BeginHorizontal(h);
                    property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, property.GetName());
                    EditorGUILayout.LabelField("Size : ", EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.SelectableLabel(property.arraySize.ToString(), EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.EndHorizontal();
                    if (property.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            GUILayout.FlexibleSpace();
                            Display(property.GetArrayElementAtIndex(i));
                        }
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    EditorGUILayout.BeginHorizontal(h);
                    EditorGUILayout.SelectableLabel(property.GetName(), h);
                    EditorGUILayout.LabelField("Size : ", EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.SelectableLabel(property.arraySize.ToString(), EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                var childs = property.GetVisibleChildren();
                if (childs.Count() > 0)
                {
                    property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, property.GetName());
                    if (property.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        childs.ForEach(so => { GUILayout.FlexibleSpace(); Display(so); });
                        EditorGUI.indentLevel--;
                    }
                }
                else
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(property, true);
                    GUI.enabled = true;
                }


            }
        }

        public static string GetName(this SerializedProperty serializedProperty)
        {
            string name = serializedProperty.displayName;
            if (name == null || name == "")
                name = serializedProperty.name;
            return name;
        }

        /// <summary>
        /// Gets all children of `SerializedProperty` at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.Next(false);
            }

            if (currentProperty.Next(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty;
                }
                while (currentProperty.Next(false));
            }
        }

        /// <summary>
        /// Gets visible children of `SerializedProperty` at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        public static IEnumerable<SerializedProperty> GetVisibleChildren(this SerializedProperty serializedProperty)
        {
            SerializedProperty currentProperty = serializedProperty.Copy();
            SerializedProperty nextSiblingProperty = serializedProperty.Copy();
            {
                nextSiblingProperty.NextVisible(false);
            }

            if (currentProperty.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                        break;

                    yield return currentProperty;
                }
                while (currentProperty.NextVisible(false));
            }
        }


    }
}
#endif