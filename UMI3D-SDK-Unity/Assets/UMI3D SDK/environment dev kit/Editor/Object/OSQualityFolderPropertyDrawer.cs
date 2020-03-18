/*
Copyright 2019 Gfi Informatique

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
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using umi3d.edk;
using UnityEngine;
using System.IO;
using System;
namespace umi3d.edk.editor
{
    [CustomPropertyDrawer(typeof(OSQualitycollection.OSQualityFolder))]
    [CanEditMultipleObjects]
    public class OSQualityFolderPropertyDrawer : PropertyDrawer
    {
        SerializedProperty Os;
        SerializedProperty Quality;
        SerializedProperty Folder;

        public bool passwordDisplay;
        public float LineHeight;
        public float TitleLineHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;
            LineHeight = EditorGUIUtility.singleLineHeight;
            height = LineHeight * 1;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var smallWidth = 60f;
            var offset = 15f;

            EditorGUI.BeginChangeCheck();
            LineHeight = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginProperty(position, label, property);

            Os = property.FindPropertyRelative("os");
            Folder = property.FindPropertyRelative("path");
            Quality = property.FindPropertyRelative("quality");

            var RLine = new Rect(position.x, position.y, position.width, LineHeight);

            var RLabel = new Rect(RLine);
            var Ros = new Rect(RLine);
            var Rquality = new Rect(RLine);
            var Rpath = new Rect(RLine);
            var Rbrowse = new Rect(RLine);

            RLabel.width = EditorGUIUtility.labelWidth;// RLine.width * 2f/ 5f;
            Ros.width = smallWidth;
            Rquality.width = smallWidth;
            Rbrowse.width = smallWidth;
            Rpath.width = RLine.width - RLabel.width - Ros.width - Rquality.width - Rbrowse.width + 3*offset;

            Ros.x = RLabel.width;
            Rquality.x = Ros.x + Ros.width - offset;
            Rpath.x = Rquality.x + Rquality.width - offset;
            Rbrowse.x = Rpath.x + Rpath.width;
            Rbrowse.y -= 1;

            //var GLabel = new GUIContent("OS-Quality folder", "a specific asset folder for a combination of os and quality");
            //var GQuality = new GUIContent("Quality", "The quality of the asset under this folder");
            //var GOs = new GUIContent("OS", "the Os use to compile the asset under this folder");
            //var GPath = new GUIContent("Folder", "The path to the folder on your computer (relative to <Project Path>/Public/)");
            var GBut = new GUIContent("Browse", "Select a file on your computer");

            EditorGUI.LabelField(RLabel, label);
            EditorGUI.PropertyField(Ros, Os, GUIContent.none);
            EditorGUI.PropertyField(Rquality, Quality, GUIContent.none);
            EditorGUI.PropertyField(Rpath, Folder, GUIContent.none);

            if (GUI.Button(Rbrowse, GBut))
            {
                string path =  UMI3D.GetResourceRoot();
                //Debug.Log(path);
                Directory.CreateDirectory(path);
                var FolderPath = EditorUtility.OpenFolderPanel("Select Folder", path, "");
                if (FolderPath != null && FolderPath != "")
                {
                    FolderPath = System.IO.Path.GetFullPath(FolderPath);
                    if (!FolderPath.Contains(path)) EditorUtility.DisplayDialog("Invalid File", "The File should be under <project folder>/Public/ folder or any of its subfolder", "Ok, my bad");
                    else Folder.stringValue = FolderPath.Split(new string[] { path }, StringSplitOptions.None)[1];
                }
            }


            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

    }
}
#endif