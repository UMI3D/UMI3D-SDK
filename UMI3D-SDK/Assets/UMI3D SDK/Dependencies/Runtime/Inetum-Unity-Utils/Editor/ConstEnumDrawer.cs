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

namespace inetum.unityUtils.editor
{
    [CustomPropertyDrawer(typeof(ConstEnumAttribute))]
    public class ConstEnumDrawer : PropertyDrawer
    {
        ///<inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ConstEnumAttribute cseAttribute = attribute as ConstEnumAttribute;

            int index = 0;

            var value = cseAttribute.reader(property);
            if (value != null)
            {
                index = Array.IndexOf(cseAttribute.values, value);
                if (index == -1) index = 0;
            }
            index = EditorGUI.Popup(position, label, index, cseAttribute.options.Select(o => new GUIContent(o)).ToArray(), EditorStyles.popup);
            if (index >= cseAttribute.options.Length)
                cseAttribute.writer(property, null);
            else
                cseAttribute.writer(property, cseAttribute.values[index]);

        }
    }
}
#endif