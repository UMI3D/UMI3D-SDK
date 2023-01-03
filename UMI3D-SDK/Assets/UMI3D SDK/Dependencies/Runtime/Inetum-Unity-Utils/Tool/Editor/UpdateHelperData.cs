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

    using inetum.unityUtils;
    using System;
    using System.Collections.Generic;
    using System.IO;
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

            string pAN = pA.FindPropertyRelative("projectName").stringValue;
            string pBN = pB.FindPropertyRelative("projectName").stringValue;
            string pAp = pA.FindPropertyRelative("sdkPath").stringValue;
            string pBp = pB.FindPropertyRelative("sdkPath").stringValue;
            var expand = property.FindPropertyRelative("expand");
            var source = property.FindPropertyRelative("sourceIsA").boolValue;

            // Draw label
            label.text = (!string.IsNullOrEmpty(pAN) || !string.IsNullOrEmpty(pBN)) ? pAN + " : " + pBN : "Select Projects";
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
                if (GUI.Button(rectfolder, "Manage Folders")) SubUpdateHelper.Open(source, pAN, pBN, pAp, pBp, Callback(property), folders);
                if (GUI.Button(rectPush, pAN + " -> " + pBN)) { Copy(source, pAp, pBp, folders, true); }
                if (GUI.Button(rectPull, pAN + " <- " + pBN)) { Copy(source, pAp, pBp, folders, false); }

                EditorGUI.indentLevel = indent;
            }
            else
            {
                expand.boolValue = EditorGUI.Foldout(position, expand.boolValue, label);
            }

            //EditorGUI.Popup(position, userIndexProperty.intValue, _choices);

            EditorGUI.EndProperty();
        }

        void Copy(bool sourceIsA, string pathA, string pathB, List<string> folders, bool AtoB)
        {
            if (folders == null || folders.Count == 0)
                return;
            if (folders.Count == 1 && ((sourceIsA && folders[0] == pathA) || (!sourceIsA && folders[0] == pathB)))
            {
                if (AtoB)
                    CopyFolder(pathA, pathB);
                else
                    CopyFolder(pathB, pathA);
                return;
            }

            if (AtoB == sourceIsA)
            {
                if (Directory.Exists(AtoB ? pathB : pathA))
                    Directory.Delete(AtoB ? pathB : pathA, true);
                Directory.CreateDirectory(AtoB ? pathB : pathA);
            }

            folders
                .Select(path => path.Substring((sourceIsA) ? pathA.Length : pathB.Length))
                .Select(path =>
                {
                    if (AtoB) return (pathA + path, pathB + path);
                    return (pathB + path, pathA + path);
                })
                .ForEach(c => CopyFolder(c.Item1, c.Item2));
        }

        void CopyFolder(string pathFrom, string pathTo)
        {
            if (Directory.Exists(pathTo))
                Directory.Delete(pathTo, true);
            Directory.CreateDirectory(pathTo);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(pathFrom, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(pathFrom, pathTo));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(pathFrom, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(pathFrom, pathTo), true);
            }

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