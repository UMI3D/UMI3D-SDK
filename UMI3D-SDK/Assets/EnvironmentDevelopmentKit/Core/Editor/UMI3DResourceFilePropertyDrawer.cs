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

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Path = umi3d.common.Path;

namespace umi3d.edk.editor
{
    [CustomPropertyDrawer(typeof(UMI3DResourceFile), true)]
    [CanEditMultipleObjects]
    public class UMI3DResourceFilePropertyDrawer : PropertyDrawer
    {
        SerializedProperty isLocalFile;
        SerializedProperty isInBundle;
        SerializedProperty isInLibrary;
        SerializedProperty domain;
        SerializedProperty path;
        SerializedProperty format;
        SerializedProperty extension;
        SerializedProperty metrics;
        SerializedProperty resolution;
        SerializedProperty size;
        SerializedProperty pathIfInBundle;
        SerializedProperty libraryKey;


        //int _choiceIndex = 0;
        //string[] choices;

        public bool foldout;
        public bool foldoutMetrics;
        public float LineHeight;
        public float TitleLineHeight;

        ///<inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            isLocalFile = property.FindPropertyRelative("isLocalFile");
            isInBundle = property.FindPropertyRelative("isInBundle");
            isInLibrary = property.FindPropertyRelative("isInLibrary");
            float height = 0;
            LineHeight = EditorGUIUtility.singleLineHeight;
            height = (1.25f * LineHeight) * (foldout ? (isLocalFile.boolValue ? 0 : 1) + 7 : 1);
            if (foldout && foldoutMetrics)
                height += (1.25f * LineHeight) * 2f;
            return height;
        }

        ///<inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            LineHeight = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginProperty(position, label, property);

            isLocalFile = property.FindPropertyRelative("isLocalFile");
            isInBundle = property.FindPropertyRelative("isInBundle");
            isInLibrary = property.FindPropertyRelative("isInLibrary");
            domain = property.FindPropertyRelative("domain");
            path = property.FindPropertyRelative("path");
            format = property.FindPropertyRelative("format");
            extension = property.FindPropertyRelative("extension");
            metrics = property.FindPropertyRelative("metrics");
            resolution = property.FindPropertyRelative("metrics.resolution");
            size = property.FindPropertyRelative("metrics.size");
            pathIfInBundle = property.FindPropertyRelative("pathIfInBundle");
            libraryKey = property.FindPropertyRelative("libraryKey");

            var RLine = new Rect(position.x, position.y, position.width, LineHeight);
            foldout = EditorGUI.Foldout(RLine, foldout, format.stringValue + " [ Resolution " + resolution.intValue + " ]", true);
            if (foldout)
            {
                RLine.y += LineHeight;
                var GFormat = new GUIContent("Format", "The file's format");
                EditorGUI.PropertyField(RLine, format, GFormat);

                RLine.y += 1.25f * LineHeight;
                float metricsIndent = 10f;
                RLine.x += metricsIndent; RLine.width -= metricsIndent;
                foldoutMetrics = EditorGUI.Foldout(RLine, foldoutMetrics, metrics.displayName, true);
                if (foldoutMetrics)
                {
                    RLine.y += 1.25f * LineHeight;
                    var GResolution = new GUIContent("resolution", "The file's level of resolution.");
                    EditorGUI.PropertyField(RLine, resolution, GResolution);

                    RLine.y += 1.25f * LineHeight;
                    var GSize = new GUIContent("Size", "The file's arbitrary resolution");
                    EditorGUI.PropertyField(RLine, size, GSize);
                }
                RLine.x -= metricsIndent; RLine.width += metricsIndent;

                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 1;

                RLine.y += 1.25f * LineHeight;
                var GisLocalFile = new GUIContent("Is a Local File", "is this resource a local file ?");
                EditorGUI.PropertyField(RLine, isLocalFile, GisLocalFile);

                if (!isLocalFile.boolValue)
                {
                    RLine.y += LineHeight;
                    var GDomain = new GUIContent("Domain", "The Domain adresse on which the resources is located");
                    EditorGUI.PropertyField(RLine, domain, GDomain);


                    RLine.y += 1.25f * LineHeight;
                    var GPath = new GUIContent("Path", "The path to the file on the domain");
                    EditorGUI.PropertyField(RLine, path, GPath);
                }
                else
                {

                    RLine.y += LineHeight;
                    var RbrowseLine = new Rect(RLine);
                    RbrowseLine.width -= 55f;
                    var GPath = new GUIContent("Path", "The path to the file on your computer (relative to <Project Path>/Public/)");

                    EditorGUI.PropertyField(RbrowseLine, path, GPath);
                    RbrowseLine.x += RbrowseLine.width;
                    RbrowseLine.y -= 1;
                    RbrowseLine.width = 55f;
                    var GBut = new GUIContent("Browse", "Select a file on your computer");
                    if (GUI.Button(RbrowseLine, GBut))
                    {
                        string path = System.IO.Path.GetFullPath(Path.Combine(Application.dataPath, UMI3DServer.dataPath));
                        var FilePath = EditorUtility.OpenFilePanel("Load Resources", path, "");
                        if (FilePath != null && FilePath != "")
                        {
                            FilePath = System.IO.Path.GetFullPath(FilePath);
                            if (!FilePath.Contains(path)) EditorUtility.DisplayDialog("Invalid File", "The File should be under <project folder>/Public/ folder or any of its subfolder", "Ok, my bad");
                            else
                            {
                                this.path.stringValue = FilePath.Split(new string[] { path }, StringSplitOptions.None)[1];
                                this.extension.stringValue = System.IO.Path.GetExtension(FilePath);
                                this.size.floatValue = new FileInfo(FilePath).Length / 1000f;
                            }
                        }
                    }

                }

                RLine.y += LineHeight;
                var GExtension = new GUIContent("Extension", "The file's extension");
                EditorGUI.PropertyField(RLine, extension, GExtension);


                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 0;
                int offset = 2;

                RLine.y += LineHeight;
                Rect BundleLabel = new Rect(RLine.x, RLine.y, labelWidth - 20f, RLine.height);

                var GIsInBundleLabel = new GUIContent("Is in a bundle");
                if (isInBundle.boolValue)
                {
                    GIsInBundleLabel.text = "Path in bundle";
                }
                EditorGUI.LabelField(BundleLabel, GIsInBundleLabel);
                EditorGUI.indentLevel = 0;
                Rect BundleRect = new Rect(RLine.x + BundleLabel.width + offset, RLine.y, 20f, RLine.height);
                Rect BundleRect2 = new Rect(BundleRect.x + BundleRect.width, RLine.y, RLine.width - BundleRect.width - BundleLabel.width - offset, RLine.height);
                var GisInBundle = new GUIContent("", "is this resource in a Bundle ?");
                EditorGUI.PropertyField(BundleRect, isInBundle, GisInBundle);
                if (isInBundle.boolValue)
                {
                    var GBundleKey = new GUIContent("", "The path to the object in his bundle");
                    EditorGUI.PropertyField(BundleRect2, pathIfInBundle, GBundleKey);
                }

                RLine.y += LineHeight;

                Rect LibLabel = new Rect(RLine.x, RLine.y, labelWidth - 20f, RLine.height);
                EditorGUIUtility.labelWidth = 0;
                var GisInLibraryLabel = new GUIContent("Is in a library");
                if (isInLibrary.boolValue)
                {
                    GisInLibraryLabel.text = "Library Key";
                }
                EditorGUI.indentLevel = indent;
                EditorGUI.LabelField(LibLabel, GisInLibraryLabel);
                EditorGUI.indentLevel = 0;
                Rect libRect = new Rect(RLine.x + LibLabel.width + offset, RLine.y, 20f, RLine.height);
                Rect libRect2 = new Rect(libRect.x + libRect.width, RLine.y, RLine.width - libRect.width - LibLabel.width - offset, RLine.height);
                var GisInLibrary = new GUIContent("", "is this resource in a library ?");
                EditorGUI.PropertyField(libRect, isInLibrary, GisInLibrary);
                if (isInLibrary.boolValue)
                {
                    var GLibraryKey = new GUIContent("", "Id of the Asset library");
                    EditorGUI.PropertyField(libRect2, libraryKey, GLibraryKey);
                }
                EditorGUIUtility.labelWidth = labelWidth;

                EditorGUI.indentLevel = indent;
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.EndProperty();
        }

        private void DrawMetrics()
        {

        }

        private void OnEnable()
        {

        }
    }
}
#endif