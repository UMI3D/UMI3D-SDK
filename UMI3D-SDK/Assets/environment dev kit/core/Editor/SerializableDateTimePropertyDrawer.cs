#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using umi3d.edk;

namespace umi3d.edk.editor
{

    [CustomPropertyDrawer(typeof(SerializableDateTime))]
    public class SerializableDateTimePropertyDrawer : PropertyDrawer
    {
        const int space = 5;
        const int nowButtonSize = 40;
        const int twoDigitField = 20;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            Rect year = new Rect(position.x, position.y, twoDigitField, position.height);
            Rect labelRect = new Rect(position.x, position.y, twoDigitField, position.height);
            Rect month = new Rect(year.size.x + space + year.position.x, position.y, twoDigitField, position.height);
            Rect day = new Rect(month.size.x + space + month.position.x, position.y, twoDigitField, position.height);

            Rect hour = new Rect(day.size.x + space + day.position.x, position.y, twoDigitField, position.height);
            Rect minute = new Rect(hour.size.x + space + hour.position.x, position.y, twoDigitField, position.height);
            Rect seconde = new Rect(minute.size.x + space + minute.position.x, position.y, twoDigitField, position.height);

            Rect buttonRect = new Rect(seconde.x + space + seconde.size.x, position.y, nowButtonSize, position.height);

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