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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class BuildHelper : InitedWindow<BuildHelper>
{
    const string filename =  "BuildHelperData";
    ScriptableLoader<BuildHelperData> data;
    VersionGUI version;
    LogScrollView info;

    string CommitMessage => $"SDK {version.version}";

    bool isBuilding = false;

    [MenuItem("UMI3D/Release")]
    static void Open()
    {
        OpenWindow();
    }

    protected override void Init()
    {
        UnityEngine.Debug.Log("init");

        version = new VersionGUI();
        UnityEngine.Debug.Log("init a");
        data = new ScriptableLoader<BuildHelperData>(filename);
        UnityEngine.Debug.Log("init b");
        info = new LogScrollView();
        UnityEngine.Debug.Log("init c");
        RefreshBranch();
    }

    async void RefreshBranch()
    {
        data.data.Branch = await Git.GetBranchName();
    }

    protected override void Draw()
    {
        GUI.enabled = !isBuilding;

        data.editor?.OnInspectorGUI();


        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Current Branch");
        EditorGUILayout.LabelField(data.data.Branch);
        if (GUILayout.Button("Refresh Branch"))
            RefreshBranch();

        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Build Version", EditorStyles.boldLabel);

        version.Draw();

        GUI.enabled = !isBuilding;

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField(data.data.CommitMessageCommonTitle);
        data.data.Commonmessage = EditorGUILayout.TextArea(data.data.Commonmessage);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(data.data.CommitMessageEdkTitle);
        data.data.Edkmessage = EditorGUILayout.TextArea(data.data.Edkmessage);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(data.data.CommitMessageCdkTitle);
        data.data.Cdkmessage = EditorGUILayout.TextArea(data.data.Cdkmessage);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Full Changelog: <...>");
        EditorGUILayout.Separator();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Build but not push"))
            CleanComputeBuild(false);
        if (GUILayout.Button($"Build and push on {data.data.Branch}"))
            CleanComputeBuild(true);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Test"))
            Test();

        info.Draw();
    }

    async void Test()
    {
        for (int i = 0; i < 3000; i++)
        {
            info.NewLine($"print {i}");
            await Task.Delay(500);
        }
    }

    async void CleanComputeBuild(bool comit)
    {
        isBuilding = true;
        await Task.Yield();
        try
        {
            info.NewTitle($"Build");

            info.Clear();
            version.UpdateVersion();

            cleanBuildFolder( data.data.PackageFolderPath);

            await Task.Delay(100);
            // Build player.
            var assets = Build(data.data.PackageFolderPath);

            foreach(var asset in assets)
                info.NewLine($"Build {asset.Item2} : {asset.Item1}");

            await Task.Delay(100);

            if (comit)
            {
                info.NewTitle( $"Commit");

                await Git.CommitAll(CommitMessage, info.NewLine, info.NewError);

                info.NewTitle($"Release");

                ReleaseSdk.Release(data.data.Token, version.version, data.data.Branch, assets, data.data.message);
            }

            //Open folder
            OpenFile(Application.dataPath + "/../" + data.data.PackageFolderPath+"/");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        isBuilding = false;
    }

    #region BuildUtils

    void cleanBuildFolder(string buildFolder)
    {
        if (Directory.Exists(buildFolder))
            Directory.Delete(buildFolder, true);
        Directory.CreateDirectory(buildFolder);
    }

    List<(string, string)> Build(string buildFolder)
    {
        return PackagesExporter.ExportPackages(buildFolder+"/");
    }

    static void OpenFile(string path)
    {
        path = path.Replace('/', '\\');

        if (File.Exists(path))
        {
            FileAttributes attr = File.GetAttributes(path);
            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                OpenFileWith("explorer.exe", path, "/root,");
            }
            else
            {
                OpenFileWith("explorer.exe", path, "/select,");
            }
        }
        else
            UnityEngine.Debug.LogError("no file at "+path);
    }

    static void OpenFileWith(string exePath, string path, string arguments)
    {
        if (path == null)
            return;

        try
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
            if (exePath != null)
            {
                process.StartInfo.FileName = exePath;
                //Pre-post insert quotes for fileNames with spaces.
                process.StartInfo.Arguments = string.Format("{0}\"{1}\"", arguments, path);
            }
            else
            {
                process.StartInfo.FileName = path;
                process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
            }
            if (!path.Equals(process.StartInfo.WorkingDirectory))
            {
                process.Start();
            }
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
    }

    #endregion
}
#endif