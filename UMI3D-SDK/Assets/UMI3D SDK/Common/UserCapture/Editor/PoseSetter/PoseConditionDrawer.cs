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
#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace umi3d.common.userCapture.pose.editor
{
    [CustomPropertyDrawer(typeof(AbstractPoseConditionDto))]
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

                // TODO: Repair this drawer. beware of the DTO standard.
                //var magnitudeRect = new Rect(position.x, position.y, 30, position.height);
                //var boneOriginRect = new Rect(position.x + 35, position.y, 50, position.height);
                //var targetObjectIdRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

                //EditorGUI.PropertyField(magnitudeRect, property.FindPropertyRelative("magnitude"), GUIContent.none);
                //EditorGUI.PropertyField(boneOriginRect, property.FindPropertyRelative("boneOrigin"), GUIContent.none);
                //EditorGUI.PropertyField(targetObjectIdRect, property.FindPropertyRelative("targetNodeId"), GUIContent.none);
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
#endif