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
using inetum.unityUtils.editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//[CreateAssetMenu(fileName = "UMI3DBuilderData", menuName = "Build Helper/Build Helper Data", order = 1)]
public class UMI3DBuilderData : ScriptableObject, ILogScrollViewData
{
    public string Branch;
    public string Token;
    public string PackageFolderPath;
    public bool display;

    public string Edkmessage;
    public string Cdkmessage;
    public string Commonmessage;

    public List<PackageData> packages;

    public readonly string CommitMessageEdkTitle = "## EDK";
    public readonly string CommitMessageCdkTitle = "## CDK";
    public readonly string CommitMessageCommonTitle = "## Common";
    public string message => CommitMessageCommonTitle + "\n" + Commonmessage + "\n\n" + CommitMessageEdkTitle + "\n" + Edkmessage + "\n\n" + CommitMessageCdkTitle + "\n" + Cdkmessage + "\n\n";

    public bool buildstepByStep = false;
    public bool commit = false;

    string ILogScrollViewData.text { get => scrollviewText; set => scrollviewText = value; }

    public string scrollviewText;
}

[CustomEditor(typeof(UMI3DBuilderData))]
public class BuildHelperDataEditor : Editor
{

    UMI3DBuilderData data;

    public void OnEnable()
    {
        data = (UMI3DBuilderData)target;
    }

    public override void OnInspectorGUI()
    {
        data.display = EditorGUILayout.BeginFoldoutHeaderGroup(data.display,"Data");
        if (data.display)
        {
            bool changed = false;
            EditorGUILayout.LabelField("Build Folder");
            EditorGUILayout.BeginHorizontal();
            data.PackageFolderPath = TextField(data.PackageFolderPath, ref changed);

            if (GUILayout.Button("Browse"))
            {
                var res = EditorUtility.OpenFolderPanel("Build folder", Application.dataPath + "/../" + data.PackageFolderPath, "");

                Uri path1 = new Uri(Application.dataPath + "/../");
                Uri path2 = new Uri(res);
                Uri diff = path1.MakeRelativeUri(path2);
                data.PackageFolderPath = diff.OriginalString;

                changed = true;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Token");
            EditorGUILayout.BeginHorizontal();
            data.Token = TextField(data.Token, ref changed);
            EditorGUILayout.EndHorizontal();

            if (changed)
                EditorUtility.SetDirty(data);

        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    string TextField(string text,ref bool changed) {
        var _text = EditorGUILayout.TextField(text);
        if(text != _text)
            changed = true;
        return _text;
    }

}

#endif