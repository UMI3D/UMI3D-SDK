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
namespace inetum.unityUtils
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    public class VersionGUI
    {
        class VersionRegex
        {
            //Regex reg;
            readonly string pattern;
            readonly Func<string> replacement;
            readonly Action drawAction;
            readonly Action drawButton;
            readonly Func<object> reset;

            public VersionRegex(string name, Func<object> reset, Func<string> content, Action draw, Action drawButton)
            {
                var part1 = $"string {name} = \"";
                var part2 = "\";";
                this.reset = reset;

                pattern = part1 + "(.*)" + part2;
                replacement = () =>
                {
                    var c = content();
                    return part1 + c.ToString() + part2;
                };
                //reg = new Regex(pattern);
                this.drawAction = draw;
                this.drawButton = drawButton;
            }

            public VersionRegex(string part1, string part2, Func<string> content, Action draw, Action drawButton, Func<object> reset)
            {
                pattern = part1 + "(.*)" + part2;
                replacement = () =>
                {
                    var c = content();
                    return part1 + c.ToString() + part2;
                };
                //reg = new Regex(pattern);
                this.drawAction = draw;
                this.drawButton = drawButton;
            }

            public string Replace(string text)
            {
                return Regex.Replace(text, pattern, replacement());
            }

            public void Draw()
            {
                drawAction();
            }

            public void DrawButton()
            {
                drawButton();
            }

            public void Reset()
            {
                reset();
            }
        }

        readonly string browserVersionPath;
        VersionRegex[] versions;
        ScriptableLoader<VersionData> data;
        readonly Func<string> old;
        readonly Func<string> ComputeVersion;
        public string version => ComputeVersion();

        public VersionGUI(string filePath, string format, Func<string> oldVersion, params (string name, Func<object> resetValue)[] datas)
        {
            browserVersionPath = filePath;
            old = oldVersion;

            data = new ScriptableLoader<VersionData>("VersionData");

            if (data.data.versions == null || data.data.versions.Length != datas.Length)
                data.data.versions = new object[datas.Length];
            versions = new VersionRegex[datas.Length];

            int i = 0;
            int j = 0;

            ComputeVersion = () => "";

            while (i < format.Length)
            {
                int k = j;
                switch (format[i])
                {
                    case 's':
                        if (data.data.versions[j] == null || data.data.versions[j].GetType() != typeof(string))
                            data.data.versions[j] = "";
                        versions[j] = new VersionRegex(
                                datas[j].name,
                                () => data.data.versions[k] = datas[k].resetValue(),
                                () => data.data.versions[k].ToString(),
                                () => data.data.versions[k] = EditorGUILayout.TextField(data.data.versions[k].ToString()),
                                () =>
                                {
                                    var enable = GUI.enabled;
                                    GUI.enabled = false;
                                    GUILayout.Button("            ");
                                    GUI.enabled = enable;
                                }
                            );
                        ComputeVersion = () => ComputeVersion() + data.data.versions[j].ToString();
                        j++;
                        i++;
                        break;
                    case 'I':
                        if (data.data.versions[j] == null || data.data.versions[j].GetType() != typeof(int))
                            data.data.versions[j] = 0;
                        versions[j] = new VersionRegex(
                            datas[j].name,
                            () => data.data.versions[k] = datas[k].resetValue(),
                            () => data.data.versions[k].ToString(),
                            () =>
                            {
                                int res;
                                if (int.TryParse(EditorGUILayout.TextField(data.data.versions[k].ToString()), out res))
                                {
                                    data.data.versions[k] = res;
                                }
                            },
                            () =>
                            {
                                if (GUILayout.Button($"{datas[k].name} +1"))
                                    data.data.versions[k] = (int)data.data.versions[k] + 1;
                            }
                        );
                        ComputeVersion = () => ComputeVersion() + data.data.versions[k].ToString();
                        i++;
                        j++;
                        break;
                    case 'y':
                    case 'm':
                    case 'M':
                    case 'd':
                        string datePattern = GetDatePattern(format, ref i);
                        if (data.data.versions[j] == null || data.data.versions[j].GetType() != typeof(DateTime))
                            data.data.versions[j] = DateTime.Now.ToString(datePattern);
                        versions[j] = new VersionRegex(
                                datas[j].name,
                                () => data.data.versions[k] = datas[k].resetValue(),
                                () => data.data.versions[k].ToString(),
                                () =>
                                {
                                    var date = EditorGUILayout.TextField(data.data.versions[k].ToString());
                                    DateTime res;
                                    if (DateTime.TryParseExact($"{date}", datePattern, System.Globalization.CultureInfo.InvariantCulture, DateTimeStyles.None, out res))
                                    {
                                        data.data.versions[k] = res.ToString(datePattern);
                                    }
                                    else
                                        UnityEngine.Debug.Log($"Error in pasing date : {date} with yyMMdd");
                                },
                                () =>
                                {
                                    if (GUILayout.Button("Set To Now"))
                                        data.data.versions[k] = DateTime.Now.ToString(datePattern);
                                }
                            );
                        ComputeVersion = () => ComputeVersion() + data.data.versions[k].ToString();
                        j++;
                        break;
                    default:
                        ComputeVersion = () => ComputeVersion() + format[i];
                        i++;
                        break;
                }
            }
            ResetVersion();
        }

        string GetDatePattern(string format, ref int i)
        {
            int j = 0;

            string dateFormat = "";
            bool date = true;
            while (i < format.Length && date)
            {
                switch (format[i])
                {
                    case 'y':
                    case 'm':
                    case 'M':
                    case 'd':
                        dateFormat += format[i];
                        i++;
                        break;
                    default:
                        date = false;
                        break;
                }
            }
            return dateFormat;
        }


        public void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Old version");
            EditorGUILayout.LabelField(old());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New version");
            foreach (var version in versions)
                version.Draw();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Version"))
                ResetVersion();
            foreach (var version in versions)
                version.DrawButton();
            EditorGUILayout.EndHorizontal();
        }

        public void ResetVersion()
        {
            foreach (var version in versions)
                version.Reset();
        }

        public void UpdateVersion()
        {
            string text = File.ReadAllText(browserVersionPath);
            foreach (var version in versions)
                text = version.Replace(text);

            File.WriteAllText(browserVersionPath, text);
        }
    }

    public class VersionData : ScriptableObject
    {
        public object[] versions = null;
    }
}
#endif