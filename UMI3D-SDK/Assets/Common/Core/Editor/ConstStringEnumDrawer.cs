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

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace umi3d.common.editor
{
    [CustomPropertyDrawer(typeof(ConstStringEnumAttribute))]
    public class ConstStringEnumDrawer : PropertyDrawer
    {
        ///<inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConstStringEnumAttribute cseAttribute = attribute as ConstStringEnumAttribute;

            if (property.propertyType != SerializedPropertyType.String)
                EditorGUI.LabelField(position, label.text, "Use ConstStringEnum with string.");
            else
            {

                int index = 0;

                string value = property.stringValue;
                if (value != null)
                {
                    index = Array.IndexOf(cseAttribute.options, value);
                    if (index == -1) index = 0;
                }
                index = EditorGUI.Popup(position, label, index, cseAttribute.options.Select(o => new GUIContent(o)).ToArray(), EditorStyles.popup);
                if (index >= cseAttribute.options.Length)
                    property.stringValue = "none";
                else
                    property.stringValue = cseAttribute.options[index];
            }
        }
    }
}
#endif