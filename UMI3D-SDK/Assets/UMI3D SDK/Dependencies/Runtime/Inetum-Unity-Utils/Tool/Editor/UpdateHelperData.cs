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
namespace inetum.unityUtils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    //[CreateAssetMenu(fileName = "UpdateHelperData", menuName = "Build Helper/Build Helper Data", order = 1)]
    public class UpdateHelperData : ScriptableObject
    {
        //public List<ProjectData> projects = new List<ProjectData>();
        public ProjectLink[] projectsLink;
    }

    [Serializable]
    public class ProjectData
    {
        public string sdkPath;
        public string projectName;
    }

    [Serializable]
    public class ProjectLink
    {
        public bool expand;
        public ProjectData projectA;
        public ProjectData projectB;
        public List<string> folders;
        public bool sourceIsA;
    }

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

    [CustomPropertyDrawer(typeof(ProjectLink))]
    public class ProjectLinkEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var expand = property.FindPropertyRelative("expand");
            if (!expand.boolValue)
                return base.GetPropertyHeight(property, label);
            return EditorGUIUtility.singleLineHeight * 5;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            var pA = property.FindPropertyRelative("projectA");
            var pB = property.FindPropertyRelative("projectB");
            var f = property.FindPropertyRelative("folder");

            string projectAName = pA.FindPropertyRelative("projectName").stringValue;
            string projectBName = pB.FindPropertyRelative("projectName").stringValue;
            string projectAPath = pA.FindPropertyRelative("sdkPath").stringValue;
            string projectBPath = pB.FindPropertyRelative("sdkPath").stringValue;
            var expand = property.FindPropertyRelative("expand");
            bool isAReferenceFolder = property.FindPropertyRelative("sourceIsA").boolValue;

            // Draw label
            label.text = (!string.IsNullOrEmpty(projectAName) || !string.IsNullOrEmpty(projectBName)) ? projectAName + " : " + projectBName : "Select Projects";
            if (expand.boolValue)
            {
                var line = EditorGUIUtility.singleLineHeight;

                var indentation = 50;
                var rectfold = new Rect(position.x, position.y, position.width, line);
                var rectPa = new Rect(position.x + indentation, position.y + 1 * line, position.width - indentation, line);
                var rectPb = new Rect(position.x + indentation, position.y + 2 * line, position.width - indentation, line);
                var rectfolder = new Rect(position.x + indentation, position.y + 3 * line, position.width - indentation, line);
                var half = (position.width - indentation - 5) / 2;
                var rectPush = new Rect(position.x + indentation, position.y + 4 * line, half, line);
                var rectPull = new Rect(position.x + indentation + half + 5, position.y + 4 * line, half, line);

                expand.boolValue = EditorGUI.Foldout(rectfold, expand.boolValue, label);

                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;

                EditorGUI.PropertyField(rectPa, pA);
                EditorGUI.PropertyField(rectPb, pB);
                var folders = GetFolders(property);
                if (GUI.Button(rectfolder, "Manage Folders"))
                    SubUpdateHelper.Open(isAReferenceFolder, projectAName, projectBName, projectAPath, projectBPath, Callback(property), folders);

                if (GUI.Button(rectPush, projectAName + " -> " + projectBName))
                {
                    ConfirmWindow.Open(isAReferenceFolder, projectAName, projectBName, projectAPath, projectBPath, folders);
                }
                if (GUI.Button(rectPull, projectBName + " -> " + projectAName))
                {
                    ConfirmWindow.Open(!isAReferenceFolder, projectBName, projectAName, projectBPath, projectAPath, folders);
                }



                EditorGUI.indentLevel = indent;
            }
            else
            {
                expand.boolValue = EditorGUI.Foldout(position, expand.boolValue, label);
            }

            //EditorGUI.Popup(position, userIndexProperty.intValue, _choices);

            EditorGUI.EndProperty();
        }

        Action<(bool, List<string>)> Callback(SerializedProperty property)
        {
            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            ProjectLink pl = obj as ProjectLink;
            if (obj.GetType().IsArray)
            {
                var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                pl = ((ProjectLink[])obj)[index];
            }

            return c =>
            {
                pl.sourceIsA = c.Item1;
                pl.folders = c.Item2;
            };
        }

        List<string> GetFolders(SerializedProperty property)
        {
            var folder = property.FindPropertyRelative("folders");
            var list = new List<string>();
            var folderCount = folder.arraySize;
            for (int i = 0; i < folderCount; i++)
            {
                list.Add(folder.GetArrayElementAtIndex(i).stringValue);
            }
            return list;
        }


        void Browse(SerializedProperty property, string path = null)
        {
            if (string.IsNullOrEmpty(path))
                path = Application.dataPath + "/../";
            var res = EditorUtility.OpenFolderPanel("Sdk folder", path, "");

            var s = res.Split(new char[] { '/', '\\' });

            var projectName = "None";
            var tmp = "";
            foreach (var folder in s)
            {
                if (folder == "Assets")
                {
                    projectName = tmp;
                }
                tmp = folder;
            }
            property.FindPropertyRelative("projectName").stringValue = projectName;
            property.FindPropertyRelative("sdkPath").stringValue = projectName;
        }

    }
}
#endif