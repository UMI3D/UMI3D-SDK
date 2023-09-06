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
using System.Linq;
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

    public bool IsBuilding { get => isBuilding || (data?.data?.buildstepByStep ?? false); set => isBuilding = value; }

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
        info = new LogScrollView(data.data);
        RefreshBranch();
    }

    async void RefreshBranch()
    {
        data.data.Branch = await Git.GetBranchName();
    }

    protected override void Draw()
    {
        bool changed = false;
        GUI.enabled = !IsBuilding ;

        data.editor?.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Current Branch");
        EditorGUILayout.LabelField(data.data.Branch);
        if (GUILayout.Button("Refresh Branch"))
            RefreshBranch();

        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Build Version", EditorStyles.boldLabel);

        version.Draw();

        GUI.enabled = !IsBuilding;

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField(data.data.CommitMessageCommonTitle);
        data.data.Commonmessage = TextArea(data.data.Commonmessage, ref changed);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(data.data.CommitMessageEdkTitle);
        data.data.Edkmessage = TextArea(data.data.Edkmessage, ref changed);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(data.data.CommitMessageCdkTitle);
        data.data.Cdkmessage = TextArea(data.data.Cdkmessage, ref changed);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Full Changelog: <...>");
        EditorGUILayout.Separator();

        if (data.data.buildstepByStep)
            DrawBuildingStepByStep();
        else
            DrawNotBuilding();

        info.Draw();

        if(changed)
            EditorUtility.SetDirty(data.data);
    }

    string TextArea(string text, ref bool changed)
    {
        var _text = EditorGUILayout.TextArea(text);
        if (text != _text)
            changed = true;
        return _text;
    }

    protected virtual void DrawNotBuilding()
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Build Packages but don't push"))
            CleanComputeBuild(false);
        if (GUILayout.Button($"Build and push on {data.data.Branch}"))
            CleanComputeBuild(true);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Build Packages step by step but don't push"))
            CleanComputeBuildStepByStep(false);
        if (GUILayout.Button($"Build step by step and push on {data.data.Branch}"))
            CleanComputeBuildStepByStep(true);
        EditorGUILayout.EndHorizontal();
    }
    protected virtual void DrawBuildingStepByStep()
    {
        GUI.enabled = !isBuilding;
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        DrawNext();
        if (GUILayout.Button($"Reset"))
            _Reset();
        EditorGUILayout.EndHorizontal();
        GUI.enabled = !IsBuilding;
    }


    void DrawNext()
    {

        if (data?.data?.packages != null && data.data.packages.Count > 0)
        {
            var current = data.data.packages.FirstOrDefault(p => !( p.build ?? true) );
            var next = data.data.packages.FirstOrDefault(p => !p.build.HasValue);

            if (current == null && next == null)
            {
                EditorGUILayout.LabelField($"Commiting");
            }
            else if (current == null)
            {
                if (GUILayout.Button($"Build {next.name}"))
                    CleanComputeBuildNext(current, next);
            }
            else if(next == null)
            {
                if (GUILayout.Button($"Mark {current.name} as Build and End"))
                    CleanComputeBuildNext(current, next);
            }
            else
            {
                if (GUILayout.Button($"Mark {current.name} as Build and Build {next.name}"))
                    CleanComputeBuildNext(current, next);
            }
        }
        else
            EditorGUILayout.LabelField("No package to build");
    }

    void _Reset()
    {
        data.data.packages = null;
        data.data.buildstepByStep = false;
        EditorUtility.SetDirty(data.data);
    }

        async void CleanComputeBuildStepByStep(bool comit)
    {
        await CleanComputeBuildStart();
        data.data.commit = comit;
        data.data.buildstepByStep = true;
        isBuilding = false;
    }

    async void CleanComputeBuild(bool comit)
    {
        EditorUtility.SetDirty(data.data);
        await CleanComputeBuildStart();
        EditorUtility.SetDirty(data.data);
        await CleanComputeBuildMiddle();
        EditorUtility.SetDirty(data.data);
        await CleanComputeBuildEnd(comit);
        EditorUtility.SetDirty(data.data);
    }

    async Task CleanComputeBuildStart()
    {
        IsBuilding = true;
        await Task.Yield();
        try
        {
            info.Clear();
            info.NewTitle($"Build Packages");
            version.UpdateVersion();

            cleanBuildFolder(data.data.PackageFolderPath);

            await Task.Delay(100);
            // Build player.
            data.data.packages = GetPackage(data.data.PackageFolderPath);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    async Task CleanComputeBuildMiddle()
    {
        try
        {
            data.data.buildstepByStep = true;
            await BuildAll();
            data.data.buildstepByStep = false;

            foreach (var asset in data.data.packages)
                info.NewLine($"Build {asset.name} : {asset.FullPath}");

            await Task.Delay(100);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }

    async void CleanComputeBuildNext(PackageData current, PackageData next)
    {
        try
        {
            if (current != null)
            {
                current.build = true;
                info.NewLine($"Build {current.name} : {current.FullPath}");
            }

            if (next != null)
            {
                next.build = false;
                EditorUtility.SetDirty(data.data);

                await Task.Delay(100);


                Build(next);
            }
            else
            {
                await CleanComputeBuildEnd(data.data.commit);
                data.data.buildstepByStep = false;
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }


    async Task CleanComputeBuildEnd(bool comit)
    {
        await Task.Yield();
        try
        {

            if (comit)
            {
                info.NewTitle($"Commit");

                await Git.CommitAll(CommitMessage, info.NewLine, info.NewError);

                info.NewTitle($"Release");

                var url = await ReleaseSdk.Release(data.data.Token, version.version, data.data.Branch, data.data.packages, data.data.message);

                Application.OpenURL(url);
            }

            //Open folder
            Command.OpenFile(Application.dataPath + "/../" + data.data.PackageFolderPath);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        IsBuilding = false;
    }

    #region BuildUtils

    void cleanBuildFolder(string buildFolder)
    {
        if (Directory.Exists(buildFolder))
            Directory.Delete(buildFolder, true);
        Directory.CreateDirectory(buildFolder);
    }

    async Task BuildAll()
    {
        await PackagesExporter.ExportPackages(data.data.packages);
    }

    List<PackageData> GetPackage(string buildFolder)
    {
        return PackagesExporter.GetExportPackages(buildFolder);
    }

    void Build(PackageData data)
    {
        PackagesExporter.SyncBuildPackage(data);
    }
    #endregion
}
#endif