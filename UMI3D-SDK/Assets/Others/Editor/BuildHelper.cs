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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using umi3d;
using umi3d.cdk;
using UnityEditor;
using UnityEngine;

public class BuildHelper : EditorWindow
{
    const string _scriptableFolderPath = "EXCLUDED";
    const string scriptablePathNoExt = "Assets/" + _scriptableFolderPath + "/BuildHelperData";
    const string scriptablePath = scriptablePathNoExt + ".asset";
    //static string scriptablePath => Application.dataPath + _scriptablePath;

    BuildHelperData _data;

    public string old => UMI3DVersion.version;
    public int major = 1;
    public int minor = 2;
    public string status = "c";
    public DateTime _date;
    public string date => _date.ToString("yyMMdd");

    class VersionRegex {
        Regex reg;
        string pattern;
        Func<string> replacement;

        public VersionRegex(string part1, string part2, Func<object> content)
        {
            pattern = part1 + "(.*)" + part2;
            replacement = () =>
            {
                var c = content();
                return part1 + c.ToString() + part2;
            };
            reg = new Regex(pattern);
        }

        public string Replace(string text)
        {
            return Regex.Replace(text, pattern, replacement());
        }
    }

    public string newVersion => $"{major}.{minor}.{status}.{date}";


    const string browserVersionPath = @"\UMI3D SDK\Common\Core\Runtime\UMI3DVersion.cs";

    VersionRegex patternMajor ;
    VersionRegex patternMinor ;
    VersionRegex patternCount ;
    VersionRegex patternDate ;

    string CommitMessage => $"SDK {newVersion}";


    string info = "";
    Vector2 ScrollPos;
    bool isBuilding = false;
    //\StandardAssets\Changelog

    // Add menu named "My Window" to the Window menu
    [MenuItem("UMI3D/Release")]
    static void Open()
    {

        // \Assets\Scripts\UI\UXML\
        // Get existing open window or if none, make a new one :
        BuildHelper window = (BuildHelper)EditorWindow.GetWindow(typeof(BuildHelper));
        window.Init();
        window.Show();
    }

    void Init()
    {
         patternMajor = new VersionRegex("string major = \"", "\";", () => major);
         patternMinor = new VersionRegex("string minor = \"", "\";", () => minor);
         patternCount = new VersionRegex("string status = \"", "\";", () => status);
         patternDate = new VersionRegex("string date = \"", "\";", () => date);

        ResetVersion();

        _data = GetScriptable();
        GetEditor();
    }

    void OnGUI()
    {
        GUI.enabled = !isBuilding;

        var editor = GetEditor();
        UnityEngine.Debug.Assert(_data != null);
        UnityEngine.Debug.Assert(editor != null);

        editor?.OnInspectorGUI();

        GUILayout.Label("Build Version", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Old version");
        EditorGUILayout.LabelField(old);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("New version");
        int.TryParse(EditorGUILayout.TextField(major.ToString()), out major);
        int.TryParse(EditorGUILayout.TextField(minor.ToString()), out minor);
        status = EditorGUILayout.TextField(status);
        var _day = EditorGUILayout.TextField(date);
        SetDate(_day);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Version"))
            ResetVersion();
        if (GUILayout.Button("Major +1"))
            major += 1;
        if (GUILayout.Button("Minor +1"))
            minor += 1;
        GUI.enabled = false;
        GUILayout.Button("     ");
        GUI.enabled = !isBuilding;
        if (GUILayout.Button("Set To Now"))
            _date = DateTime.Now;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField(_data.CommitMessageCommonTitle);
        _data.Commonmessage = EditorGUILayout.TextArea(_data.Commonmessage);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(_data.CommitMessageEdkTitle);
        _data.Edkmessage = EditorGUILayout.TextArea(_data.Edkmessage);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(_data.CommitMessageCdkTitle);
        _data.Cdkmessage = EditorGUILayout.TextArea(_data.Cdkmessage);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Full Changelog: <...>");
        EditorGUILayout.Separator();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Build but not push"))
            CleanComputeBuild(false);
        if (GUILayout.Button("Build and push"))
            CleanComputeBuild(true);
        EditorGUILayout.EndHorizontal();
        //if (GUILayout.Button("Test"))
        //    Test();

        GUI.enabled = true;
        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);
        GUI.enabled = false;
        EditorGUILayout.TextArea(info);
        GUI.enabled = true;
        EditorGUILayout.EndScrollView();
    }

    async void Test()
    {

    }

    async void CleanComputeBuild(bool comit)
    {
        isBuilding = true;
        await Task.Yield();
        try
        {
            info = "";
            UpdateVersion();

            cleanBuildFolder( _data.PackageFolderPath);

            await Task.Delay(100);
            // Build player.
            var assets = Build(_data.PackageFolderPath);
            foreach(var asset in assets)
            {
                info += $"Build {asset.Item2} : {asset.Item1}\n";
            }
            await Task.Delay(100);

            if (comit)
            {
                info += $"Commit";


                await CommitAll();

                info += $"Release";

                ReleaseSdk._ReleaseSdk(_data.Token, newVersion, _data.Branch, assets, _data.message);
            }

            //Open folder
            OpenFile(_data.PackageFolderPath);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        isBuilding = false;
    }

    #region OnGUI Utils
    void SetDate(string date)
    {
        if (!DateTime.TryParseExact($"{date}", "yyMMdd", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out _date))
        {
            UnityEngine.Debug.Log($"Error in pasing date : {date} with yyMMdd");
        };
    }

    void ResetVersion()
    {
        int.TryParse(UMI3DVersion.major, out major);
        int.TryParse(UMI3DVersion.minor, out minor);
        status = UMI3DVersion.status;
        SetDate(UMI3DVersion.date);
    }

    #endregion

    #region BuildUtils

    async Task CommitAll()
    {
        string gitCommand = "git";
        string gitAddArgument = @"add .";
        string gitCommitArgument = $"commit -m \"{CommitMessage}\"";
        string gitPushArgument = @"push";

        await ExecuteCommand(gitCommand, gitAddArgument, (s) => info += $"\n{s}", (s) => info += $"\nError : {s}");
        await ExecuteCommand(gitCommand, gitCommitArgument, (s) => info += $"\n{s}", (s) => info += $"\nError : {s}");
        await ExecuteCommand(gitCommand, gitPushArgument, (s) => info += $"\n{s}", (s) => info += $"\nError : {s}");

    }

    void UpdateVersion()
    {
        string text = File.ReadAllText(Application.dataPath + browserVersionPath);
        text = patternMajor.Replace(text);
        text = patternMinor.Replace(text);
        text = patternCount.Replace(text);
        text = patternDate.Replace(text);

        File.WriteAllText(Application.dataPath + browserVersionPath, text);
    }

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

    #region Scriptable Handler

    Editor ScriptableEditor;
    Editor GetEditor()
    {
        if (ScriptableEditor == null)
            ScriptableEditor = Editor.CreateEditor(_data);
        return ScriptableEditor;
    }

    static BuildHelperData GetScriptable() => LoadScriptable() ?? CreateScriptable();

    static BuildHelperData CreateScriptable()
    {
        CreateFolder();
        BuildHelperData asset = ScriptableObject.CreateInstance<BuildHelperData>();
        AssetDatabase.CreateAsset(asset, scriptablePath);
        AssetDatabase.SaveAssets();
        return asset;
    }

    static void CreateFolder()
    {
        if (!System.IO.Directory.Exists(Application.dataPath + System.IO.Path.GetDirectoryName(scriptablePath).TrimStart("Assets".ToCharArray())))
        {
            AssetDatabase.CreateFolder("Assets", _scriptableFolderPath);
        }

    }

    static BuildHelperData LoadScriptable()
    {
        var asset = AssetDatabase.LoadAssetAtPath<BuildHelperData>(scriptablePath);
        UnityEngine.Debug.Assert(asset != null, scriptablePath);
        return asset;
    }

    #endregion

    //
    #region Command
    public async Task ExecuteISCC(string command)
    {
        command = command.Replace('/', '\\');
        string exe = "\"C:\\Program Files (x86)\\Inno Setup 6\\ISCC.exe\"";
        //ExecuteCommandSync(exe + " " + command);
        await ExecuteCommand(exe, $"\"{command}\"", (s)=>info +=$"\n{s}", (s) => info += $"\nError : {s}");
    }

    public static async Task ExecuteCommand(string command, string args, Action<string> output, Action<string> error)
    {
        var processInfo = new ProcessStartInfo(command, args)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        var process = Process.Start(processInfo);

        if (output != null)
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => output(e.Data);
        else
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            UnityEngine.Debug.Log("Information while executing command { <i>" + command + " " + args + "</i> } : <b>D>" + e.Data + "</b>");

        process.BeginOutputReadLine();

        if (error != null)
            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => error(e.Data);
        else
            process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            UnityEngine.Debug.Log("Error while executing command { <i>" + command + " " + args + "</i> } : <b>E>" + e.Data + "</b>");

        process.BeginErrorReadLine();

        while (!process.HasExited)
        {
            await Task.Yield();
        }

        process.Close();
    }
    #endregion
}
#endif