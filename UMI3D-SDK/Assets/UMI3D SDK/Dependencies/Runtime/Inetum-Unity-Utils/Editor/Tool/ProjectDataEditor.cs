/*
Copyright 2019 - 2022 Inetum

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
namespace inetum.unityUtils.editor
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(ProjectData))]
    public class ProjectDataEditor : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            label.text = property.FindPropertyRelative("projectName").stringValue;

            if (string.IsNullOrEmpty(label.text))
            {
                var browseRect = new Rect(position.x + 20, position.y, 125, position.height);
                if (GUI.Button(browseRect, "Browse"))
                    Browse(property);
            }
            else
            {

                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                // Don't make child fields be indented
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                // Calculate rects
                var browseRect = new Rect(position.x, position.y, 60, position.height);

                // Draw fields - pass GUIContent.none to each so they are drawn without labels
                if (GUI.Button(browseRect, "Browse"))
                    Browse(property, property.FindPropertyRelative("sdkPath").stringValue);

                var p = property.GetValue() as ProjectData;

                position.x += 65;

                if(p.isSource)
                    position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive),new GUIContent("[Source]"));

                // Set indent back to what it was
                EditorGUI.indentLevel = indent;
            }
            EditorGUI.EndProperty();
        }

        void Browse(SerializedProperty property, string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = Application.dataPath + "/../";
            var res = EditorUtility.OpenFolderPanel("Sdk folder", path, "");

            var s = res.Split(new char[] { '/', '\\' });

            string projectName = null;
            var tmp = "";
            foreach (var folder in s)
            {
                if (folder == "Assets")
                {
                    projectName = tmp;
                }
                tmp = folder;
            }
            property.FindPropertyRelative("projectName").stringValue = projectName ?? path.Split('\\', '/').Last();
            property.FindPropertyRelative("sdkPath").stringValue = res;
        }

    }
}
#endif