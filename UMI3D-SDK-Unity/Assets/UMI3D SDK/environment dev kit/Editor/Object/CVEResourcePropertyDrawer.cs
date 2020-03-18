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
    [CustomPropertyDrawer(typeof(CVEResource))]
    [CanEditMultipleObjects]
    public class CVEResourcePropertyDrawer : PropertyDrawer
    {
        SerializedProperty Domain;
        SerializedProperty Path;
        SerializedProperty ApiKey;
        SerializedProperty Login;
        SerializedProperty Password;
        SerializedProperty RequestType;
        SerializedProperty IsLocalFile;

        public bool foldout;
        public bool passwordDisplay;
        public float LineHeight;
        public float TitleLineHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            IsLocalFile = property.FindPropertyRelative("IsLocalFile");
            float height = 0;
            LineHeight = EditorGUIUtility.singleLineHeight;
            height = LineHeight * (foldout ? (IsLocalFile.boolValue ? 0 : 6) + 3 : 1);

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            LineHeight = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginProperty(position, label, property);

            Domain = property.FindPropertyRelative("Domain");
            Path = property.FindPropertyRelative("Path");
            ApiKey = property.FindPropertyRelative("ApiKey");
            Login = property.FindPropertyRelative("Login");
            Password = property.FindPropertyRelative("Password");
            RequestType = property.FindPropertyRelative("RequestType");
            IsLocalFile = property.FindPropertyRelative("IsLocalFile");

            var RLine = new Rect(position.x, position.y, position.width, LineHeight);
            foldout = EditorGUI.Foldout(RLine, foldout, label, true);
            if (foldout)
            {
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 1;

                RLine.y += LineHeight;
                var GisLocalFile = new GUIContent("Is a Local File", "is this resource a local file ?");
                EditorGUI.PropertyField(RLine, IsLocalFile, GisLocalFile);

                if (!IsLocalFile.boolValue)
                {
                    RLine.y += LineHeight;
                    var GDomain = new GUIContent("Domain", "The Domain adresse on which the resources is located");
                    EditorGUI.PropertyField(RLine, Domain, GDomain);


                    RLine.y += LineHeight;
                    var GPath = new GUIContent("Path", "The path to the file on the domain");
                    EditorGUI.PropertyField(RLine, Path, GPath);

                    RLine.y += LineHeight * 1.5f;
                    var GLogin = new GUIContent("Login", "The login to acces the resources");
                    EditorGUI.PropertyField(RLine, Login, GLogin);

                    RLine.y += LineHeight;
                    var GPassword = new GUIContent("Password", "The password to acces the resources");
                    var RPassLine = new Rect(RLine);
                    RPassLine.width -= LineHeight;

                    if (passwordDisplay)
                    {
                        EditorGUI.PropertyField(RPassLine, Password, GPassword);
                    }
                    else
                    {
                        Password.stringValue = EditorGUI.PasswordField(RPassLine, GPassword, Password.stringValue);
                    }
                    RPassLine.x += RPassLine.width - 13f;
                    RPassLine.y -= 1f;
                    var GPasswordD = new GUIContent("", "Display or Hide the password");
                    passwordDisplay = EditorGUI.ToggleLeft(RPassLine, GPasswordD, passwordDisplay);

                    RLine.y += LineHeight * 1.5f;
                    var GApiK = new GUIContent("API Key");
                    EditorGUI.PropertyField(RLine, ApiKey, GApiK);

                    RLine.y += LineHeight;
                    var GRequest = new GUIContent("Request Type", "The request type [Post,Get,...]");
                    EditorGUI.PropertyField(RLine, RequestType, GRequest);
                }
                else
                {

                    RLine.y += LineHeight;
                    var RbrowseLine = new Rect(RLine);
                    RbrowseLine.width -= 55f;
                    var GPath = new GUIContent("Path", "The path to the file on your computer (relative to <Project Path>/Public/)");


                    EditorGUI.PropertyField(RbrowseLine, Path, GPath);
                    RbrowseLine.x += RbrowseLine.width;
                    RbrowseLine.y -= 1;
                    RbrowseLine.width = 55f;
                    var GBut = new GUIContent("Browse", "Select a file on your computer");
                    if (GUI.Button(RbrowseLine, GBut))
                    {
                        string path = System.IO.Path.GetFullPath(UMI3D.GetDefaultRoot());
                        Directory.CreateDirectory(path);
                        var FilePath = EditorUtility.OpenFilePanel("Load Resources", path, "");
                        if (FilePath != null && FilePath != "")
                        {
                            FilePath = System.IO.Path.GetFullPath(FilePath);
                            if (!FilePath.Contains(path)) EditorUtility.DisplayDialog("Invalid File", "The File should be under <project folder>/Public/ folder or any of its subfolder", "Ok, my bad");
                            else Path.stringValue = FilePath.Split(new string[] { path }, StringSplitOptions.None)[1];
                        }
                    }

                }
                EditorGUI.indentLevel = indent;
            }
            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnEnable()
        {

        }
    }
}
#endif