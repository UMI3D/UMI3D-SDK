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

using UnityEditor;
using UnityEngine;

namespace umi3d.edk.editor
{

    [CustomPropertyDrawer(typeof(SerializableDateTime))]
    public class SerializableDateTimePropertyDrawer : PropertyDrawer
    {
        private const int space = 5;
        private const int nowButtonSize = 40;
        private const int twoDigitField = 20;
        private const int fourDigitField = 40;

        ///<inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var year = new Rect(position.x, position.y, fourDigitField, position.height);
            var labelRect = new Rect(position.x, position.y, twoDigitField, position.height);
            var month = new Rect(year.size.x + space + year.position.x, position.y, twoDigitField, position.height);
            var day = new Rect(month.size.x + space + month.position.x, position.y, twoDigitField, position.height);

            var hour = new Rect(day.size.x + space + day.position.x, position.y, twoDigitField, position.height);
            var minute = new Rect(hour.size.x + space + hour.position.x, position.y, twoDigitField, position.height);
            var seconde = new Rect(minute.size.x + space + minute.position.x, position.y, twoDigitField, position.height);

            var buttonRect = new Rect(seconde.x + space + seconde.size.x, position.y, nowButtonSize, position.height);

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(year, property.FindPropertyRelative("year"), GUIContent.none);
            EditorGUI.PropertyField(month, property.FindPropertyRelative("month"), GUIContent.none);
            EditorGUI.PropertyField(day, property.FindPropertyRelative("day"), GUIContent.none);
            labelRect.position = new Vector2(year.position.x + year.width - 1, year.position.y);
            GUI.Label(labelRect, "/");
            labelRect.position = new Vector2(month.position.x + month.width - 1, month.position.y);
            GUI.Label(labelRect, "/");

            EditorGUI.PropertyField(hour, property.FindPropertyRelative("hours"), GUIContent.none);
            EditorGUI.PropertyField(minute, property.FindPropertyRelative("minutes"), GUIContent.none);
            EditorGUI.PropertyField(seconde, property.FindPropertyRelative("seconds"), GUIContent.none);
            labelRect.position = new Vector2(hour.position.x + hour.width - 1, hour.position.y);
            GUI.Label(labelRect, ":");
            labelRect.position = new Vector2(minute.position.x + minute.width - 1, minute.position.y);
            GUI.Label(labelRect, ":");
            if (GUI.Button(buttonRect, "now")) { property.FindPropertyRelative("setNow").boolValue = true; }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
#endif