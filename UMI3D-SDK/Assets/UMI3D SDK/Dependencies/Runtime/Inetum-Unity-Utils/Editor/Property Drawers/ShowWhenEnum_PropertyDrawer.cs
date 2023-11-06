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
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace inetum.unityUtils.editor
{
    [CustomPropertyDrawer(typeof(ShowWhenEnumAttribute))]
    public class ShowWhenEnum_PropertyDrawer : PropertyDrawer
    {
        bool showField = true;
        PropertyDrawer propertyDrawer;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (showField)
            {
                if (propertyDrawer == null && !TryGetOriginalPropertyDrawer(out propertyDrawer))
                {
                    return EditorGUI.GetPropertyHeight(property);
                }
                else
                {
                    return propertyDrawer.GetPropertyHeight(property, label);
                }
            }
            else
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowWhenEnumAttribute attribute = (ShowWhenEnumAttribute)this.attribute;

            var pathToEnum = property.propertyPath.Split(property.name)[0] + attribute.enumFieldName;
            SerializedProperty enumField = property.serializedObject.FindProperty(pathToEnum);

            if (enumField == null)
            {
                UnityEngine.Debug.LogError($"Enum field [{attribute.enumFieldName}] is null");
            }
            else
            {
                showField = false;

                foreach (int value in attribute.values)
                {
                    if (enumField.enumValueIndex == value)
                    {
                        showField = true;
                        break;
                    }
                }
            }

            if (showField)
            {
                if (propertyDrawer == null && !TryGetOriginalPropertyDrawer(out propertyDrawer))
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }
                else
                {
                    propertyDrawer.OnGUI(position, property, label);
                }
            }
        }

        bool TryGetOriginalPropertyDrawer(out PropertyDrawer propertyDrawer)
        {
            // Getting the field type this way assumes that the property instance is not a managed reference (with a SerializeReference attribute); if it was, it should be retrieved in a different way:
            Type fieldType = fieldInfo.FieldType;

            Type propertyDrawerType = (Type)Type.GetType("UnityEditor.ScriptAttributeUtility,UnityEditor")
                .GetMethod("GetDrawerTypeForType", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Invoke(null, new object[] { fieldType });

            propertyDrawer = null;
            if (typeof(PropertyDrawer).IsAssignableFrom(propertyDrawerType))
            {
                propertyDrawer = (PropertyDrawer)Activator.CreateInstance(propertyDrawerType);
            }

            if (propertyDrawer != null)
            {
                typeof(PropertyDrawer)
                    .GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .SetValue(propertyDrawer, fieldInfo);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
