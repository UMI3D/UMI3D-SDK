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

using UnityEditor;
using UnityEngine;

namespace inetum.unityUtils
{
    [CustomPropertyDrawer(typeof(NotifyingVariable<>), true)]
    public class NotifyingVariable_PropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var fieldSerialized = property.FindPropertyRelative("field");
            return EditorGUI.GetPropertyHeight(fieldSerialized, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var fieldSerialized = property.FindPropertyRelative("field");
            EditorGUI.PropertyField(position, fieldSerialized, label, true);
        }
    }
}