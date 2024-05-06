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
namespace inetum.unityUtils.editor
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
            readonly Func<string, string> replacement;
            readonly Action drawAction;
            readonly Action drawButton;
            readonly Func<string,object> reset;

            public VersionRegex(string name, Func<string,object> reset, Func<string> content, Action draw, Action drawButton)
            {
                var part1 = $"string {name} = \"";
                var part2 = "\";";
                this.reset = reset;

                var pattern = part1 + "(.*)" + part2;
                replacement = (text) =>
                {
                    var c = content();
                    var newVal = part1 + c + part2;
                    return Regex.Replace(text, pattern, newVal);
                };
                //reg = new Regex(pattern);
                this.drawAction = draw;
                this.drawButton = drawButton;
            }

            public VersionRegex(Func<string, object> reset, Action draw, Action drawButton)
            {
                this.reset = reset;
                replacement = (text) =>
                {
                    return text;
                };
                this.drawAction = draw;
                this.drawButton = drawButton;
            }

            public string Replace(string text)
            {
                return replacement(text);
            }

            public void Draw()
            {
                drawAction();
            }

            public void DrawButton()
            {
                drawButton();
            }

            public void Reset(string oldVersion)
            {
                reset(oldVersion);
            }
        }

        readonly string browserVersionPath;
        readonly bool loadingFailed = false;
        VersionRegex[] versions;
        ScriptableLoader<VersionData> data;
        readonly Func<string,string> old;
        readonly Func<string> ComputeVersion;
        readonly Func<string, string, string> Replace;
        public string version => ComputeVersion();

        public VersionGUI(string versionDataName,string filePath, string format, Func<string,string> oldVersion, params (string name, Func<string,object> resetValue)[] datas)
        {
            this.Replace = (s,_) => s;

            browserVersionPath = filePath;
            old = oldVersion;

            data = new ScriptableLoader<VersionData>(versionDataName);

            if (data == null || data.data == null)
            {
                loadingFailed = true;
            }
            else
            {

                if (data.data.versions == null || data.data.versions.Length != datas.Length)
                    data.data.versions = new object[datas.Length];
                versions = new VersionRegex[datas.Length];

                int i = 0;
                int j = 0;

                ComputeVersion = () => "";

                while (i < format.Length)
                {
                    var tmpCompute = ComputeVersion;
                    int k = j;
                    switch (format[i])
                    {
                        case 's':
                            if (data.data.versions[j] == null || data.data.versions[j].GetType() != typeof(string))
                                data.data.versions[j] = "";
                            versions[j] = new VersionRegex(
                                    datas[j].name,
                                    (s) => data.data.versions[k] = datas[k].resetValue(s),
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
                            ComputeVersion = () => tmpCompute() + data.data.versions[k].ToString();
                            j++;
                            i++;
                            break;
                        case 'I':
                            if (data.data.versions[j] == null || data.data.versions[j].GetType() != typeof(int))
                                data.data.versions[j] = 0;
                            versions[j] = new VersionRegex(
                                datas[j].name,
                                (s) => data.data.versions[k] = datas[k].resetValue(s),
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
                            ComputeVersion = () => tmpCompute() + data.data.versions[k].ToString();
                            i++;
                            j++;
                            break;
                        case 'y':
                        case 'm':
                        case 'M':
                        case 'd':
                        case 'h':
                        case 'H':
                            string datePattern = GetDatePattern(format, ref i);

                            if (data.data.versions[j] == null || data.data.versions[j].GetType() != typeof(DateTime))
                                data.data.versions[j] = DateTime.Now.ToString(datePattern);
                            versions[j] = new VersionRegex(
                                    datas[j].name,
                                    (s) => data.data.versions[k] = datas[k].resetValue(s),
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
                            ComputeVersion = () => tmpCompute() + data.data.versions[k].ToString();
                            j++;
                            break;
                        default:
                            int tmpI = i;
                            ComputeVersion = () => tmpCompute() + format[tmpI];
                            i++;
                            break;
                    }
                }
            }
            ResetVersion();
        }

        public VersionGUI(string versionDataName, string filePath, string format, Func<string,string> oldVersion, Func<string, string, string> replace, params (string name, Func<string, object> resetValue)[] datas)
        {
            this.Replace = replace;

            browserVersionPath = filePath;
            old = oldVersion;

            data = new ScriptableLoader<VersionData>(versionDataName);

            if (data == null || data.data == null)
            {
                loadingFailed = true;
            }
            else
            {

                if (data.data.versions == null || data.data.versions.Length != datas.Length)
                    data.data.versions = new object[datas.Length];
                versions = new VersionRegex[datas.Length];

                int i = 0;
                int j = 0;

                ComputeVersion = () => "";

                while (i < format.Length)
                {
                    var tmpCompute = ComputeVersion;
                    int k = j;
                    switch (format[i])
                    {
                        case 's':
                            if (data.data.versions[j] == null || data.data.versions[j].GetType() != typeof(string))
                                data.data.versions[j] = "";
                            versions[j] = new VersionRegex(
                                    (s) => data.data.versions[k] = datas[k].resetValue(s),
                                    () => data.data.versions[k] = EditorGUILayout.TextField(data.data.versions[k].ToString()),
                                    () =>
                                    {
                                        var enable = GUI.enabled;
                                        GUI.enabled = false;
                                        GUILayout.Button("            ");
                                        GUI.enabled = enable;
                                    }
                                );
                            ComputeVersion = () => tmpCompute() + data.data.versions[k].ToString();
                            j++;
                            i++;
                            break;
                        case 'I':
                            if (data.data.versions[j] == null || data.data.versions[j].GetType() != typeof(int))
                                data.data.versions[j] = 0;
                            versions[j] = new VersionRegex(
                                (s) => data.data.versions[k] = datas[k].resetValue(s),
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
                            ComputeVersion = () => tmpCompute() + data.data.versions[k].ToString();
                            i++;
                            j++;
                            break;
                        case 'y':
                        case 'm':
                        case 'M':
                        case 'd':
                        case 'h':
                        case 'H':
                            string datePattern = GetDatePattern(format, ref i);

                            if (data.data.versions[j] == null || data.data.versions[j].GetType() != typeof(DateTime))
                                data.data.versions[j] = DateTime.Now.ToString(datePattern);
                            versions[j] = new VersionRegex(
                                    (s) => data.data.versions[k] = datas[k].resetValue(s),
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
                            ComputeVersion = () => tmpCompute() + data.data.versions[k].ToString();
                            j++;
                            break;
                        default:
                            int tmpI = i;
                            ComputeVersion = () => tmpCompute() + format[tmpI];
                            i++;
                            break;
                    }
                }
            }
            ResetVersion();
        }


        static string GetDatePattern(string format, ref int i)
        {
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
                    case 'h':
                    case 'H':
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
            if(loadingFailed) return;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Old version");
            string text = File.ReadAllText(browserVersionPath);
            EditorGUILayout.LabelField(old(text));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New version");
            try
            {
                foreach (var version in versions)
                    version.Draw();
            }
            catch{  }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Version"))
                ResetVersion();
            try
            {
                foreach (var version in versions)
                version.DrawButton();
            }
            catch { }
            EditorGUILayout.EndHorizontal();
        }

        public void ResetVersion()
        {
            if (loadingFailed) return;
            string text = File.ReadAllText(browserVersionPath);
            string _old = old(text);
            foreach (var version in versions)
                version.Reset(_old);
        }

        public virtual bool UpdateVersion()
        {
            if (loadingFailed) return false;
            string text = File.ReadAllText(browserVersionPath);
            string old = text;
            foreach (var version in versions)
                text = version.Replace(text);

            text = this.Replace(text, version);

            File.WriteAllText(browserVersionPath, text);
            return text == old;
        }
    }
}
#endif