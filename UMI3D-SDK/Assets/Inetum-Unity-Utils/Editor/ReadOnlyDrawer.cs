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

namespace inetum.unityUtils.editor
{
    // <summary>Property will be editable only is the application isn't running.</summary>
    [CustomPropertyDrawer(typeof(EditorReadOnlyAttribute))]
    public class EditorReadOnlyDrawer : PropertyDrawer
    {
        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }

        /// <summary>Property will be editable only is the application isn't running.</summary>
        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Application.isPlaying)
                GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            if (Application.isPlaying)
                GUI.enabled = true;
        }
    }

    /// <summary>Property will not be editable.</summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }

        /// <summary>Property will not be editable.</summary>
        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ReadOnlyAttribute readOnlyAttribute = attribute as ReadOnlyAttribute;

            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
}
#endif