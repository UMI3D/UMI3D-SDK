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

using inetum.unityUtils;
using inetum.unityUtils.editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using umi3d;
using UnityEditor;
using UnityEngine;

public class UMI3DBuilder : InitedWindow<UMI3DBuilder>
{
    const string filename = "UMI3DBuilderData";
    ScriptableLoader<UMI3DBuilderData> data;
    VersionGUI version;
    LogScrollView info;

    string CommitMessage => $"SDK {version.version}";

    bool isBuilding = false;

    [MenuItem("UMI3D/Release SDK")]
    static void Open()
    {
        OpenWindow();
    }

    protected override void Init()
    {
        version = new VersionGUI(
                Application.dataPath + @"\UMI3D SDK\Common\Core\Runtime\UMI3DVersion.cs",
                "I.I.s.yyMMdd",
                () => UMI3DVersion.version,
                ("major", (s) => UMI3DVersion.major),
                ("minor", (s) => UMI3DVersion.minor),
                ("status", (s) => UMI3DVersion.status),
                ("date", (s) => UMI3DVersion.date)
            );
        data = new ScriptableLoader<UMI3DBuilderData>(filename);
        info = new LogScrollView();
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
        if (GUILayout.Button("Build Packages but don't push"))
            CleanComputeBuild(false);
        if (GUILayout.Button($"Build and push on {data.data.Branch}"))
            CleanComputeBuild(true);
        EditorGUILayout.EndHorizontal();

        info.Draw();
    }

    async void CleanComputeBuild(bool comit)
    {
        isBuilding = true;
        await Task.Yield();
        try
        {
            info.Clear();
            info.NewTitle($"Build Packages");
            version.UpdateVersion();

            cleanBuildFolder(data.data.PackageFolderPath);

            await Task.Delay(100);
            // Build player.
            var assets = await Build(data.data.PackageFolderPath);

            foreach (var asset in assets)
                info.NewLine($"Build {asset.Item2} : {asset.Item1}");

            await Task.Delay(100);

            if (comit)
            {
                info.NewTitle($"Commit");

                await Git.CommitAll(CommitMessage, info.NewLine, info.NewError);

                info.NewTitle($"Release");

                var url = await ReleaseSdk.Release(data.data.Token, version.version, data.data.Branch, assets, data.data.message);

                Application.OpenURL(url);
            }

            //Open folder
            Command.OpenFile(Application.dataPath + "/../" + data.data.PackageFolderPath);
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

    async Task<List<(string, string)>> Build(string buildFolder)
    {
        return await PackagesExporter.ExportPackages(buildFolder + "/");
    }
    #endregion
}
#endif