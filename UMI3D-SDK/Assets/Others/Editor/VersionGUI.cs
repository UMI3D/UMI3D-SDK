/*
Copyright 2019 - 2021 Inetum

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
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using umi3d;
using UnityEditor;
using UnityEngine;

public class VersionGUI
{

    class VersionRegex
    {
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

    const string browserVersionPath = @"\UMI3D SDK\Common\Core\Runtime\UMI3DVersion.cs";

    ScriptableLoader<VersionData> data;

    public string old => UMI3DVersion.version;
    public string version => data.data.newVersion;

    VersionRegex patternMajor;
    VersionRegex patternMinor;
    VersionRegex patternCount;
    VersionRegex patternDate;

    public VersionGUI()
    {
        data = new ScriptableLoader<VersionData>("VersionData");

        patternMajor = new VersionRegex("string major = \"", "\";", () => data.data.major);
        patternMinor = new VersionRegex("string minor = \"", "\";", () => data.data.minor);
        patternCount = new VersionRegex("string status = \"", "\";", () => data.data.status);
        patternDate = new VersionRegex("string date = \"", "\";", () => data.data.date);

        ResetVersion();
    }

    
    public void Draw()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Old version");
        EditorGUILayout.LabelField(old);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("New version");
        int.TryParse(EditorGUILayout.TextField(data.data.major.ToString()), out data.data.major);
        int.TryParse(EditorGUILayout.TextField(data.data.minor.ToString()), out data.data.minor);
        data.data.status = EditorGUILayout.TextField(data.data.status);
        var _day = EditorGUILayout.TextField(data.data.date);
        SetDate(_day);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset Version"))
            ResetVersion();
        if (GUILayout.Button("Major +1"))
            data.data.major += 1;
        if (GUILayout.Button("Minor +1"))
            data.data.minor += 1;

        var enable = GUI.enabled;
        GUI.enabled = false;
        GUILayout.Button("     ");
        GUI.enabled = enable;

        if (GUILayout.Button("Set To Now"))
            data.data._date = DateTime.Now;
        EditorGUILayout.EndHorizontal();
    }

    public void ResetVersion()
    {
        int.TryParse(UMI3DVersion.major, out data.data.major);
        int.TryParse(UMI3DVersion.minor, out data.data.minor);
        data.data.status = UMI3DVersion.status;
        SetDate(UMI3DVersion.date);
    }

    public void SetDate(string date)
    {
        if (!DateTime.TryParseExact($"{date}", "yyMMdd", System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out data.data._date))
        {
            UnityEngine.Debug.Log($"Error in pasing date : {date} with yyMMdd");
        };
    }

    public void UpdateVersion()
    {
        string text = File.ReadAllText(Application.dataPath + browserVersionPath);
        text = patternMajor.Replace(text);
        text = patternMinor.Replace(text);
        text = patternCount.Replace(text);
        text = patternDate.Replace(text);

        File.WriteAllText(Application.dataPath + browserVersionPath, text);
    }
}

public class VersionData : ScriptableObject
{
    public int major = 1;
    public int minor = 2;
    public string status = "c";
    public DateTime _date;
    public string date => _date.ToString("yyMMdd");

    public string newVersion => $"{major}.{minor}.{status}.{date}";

}

#endif