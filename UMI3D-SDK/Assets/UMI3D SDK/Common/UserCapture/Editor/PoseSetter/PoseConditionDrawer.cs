using umi3d.common.userCapture;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace umi3d.common.userCapture.pose.editor
{
    [CustomPropertyDrawer(typeof(PoseConditionDto))]
    public class PoseConditionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (property.type.Contains("Magnitude"))
            {
                label.text = property.type.ToString().Replace("managedReference<", "").Replace(">", "").Replace("Dto", "");
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                var magnitudeRect = new Rect(position.x, position.y, 30, position.height);
                var boneOriginRect = new Rect(position.x + 35, position.y, 50, position.height);
                var targetObjectIdRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

                EditorGUI.PropertyField(magnitudeRect, property.FindPropertyRelative("magnitude"), GUIContent.none);
                EditorGUI.PropertyField(boneOriginRect, property.FindPropertyRelative("boneOrigin"), GUIContent.none);
                EditorGUI.PropertyField(targetObjectIdRect, property.FindPropertyRelative("targetObjectId"), GUIContent.none);
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}