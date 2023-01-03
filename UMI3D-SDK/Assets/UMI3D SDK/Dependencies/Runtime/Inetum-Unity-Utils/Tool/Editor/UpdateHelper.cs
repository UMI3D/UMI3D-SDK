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
namespace inetum.unityUtils
{

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public class UpdateHelper : InitedWindow<UpdateHelper>
    {
        const string fileName = "UpdateHelperData";

        ScriptableLoader<UpdateHelperData> draw;

        // Add menu named "My Window" to the Window menu
        [MenuItem("UMI3D/Update")]
        static void Open()
        {
            OpenWindow();
        }

        protected override void Init()
        {
            draw = new ScriptableLoader<UpdateHelperData>(fileName);
        }

        protected override void Draw()
        {
            draw.editor?.OnInspectorGUI();
        }
    }


    public class SubUpdateHelper : EditorWindow
    {
        bool inited = false;

        public static void Open(bool sourceIsA, string name1, string name2, string path1, string path2, Action<(bool, List<string>)> callback, List<string> folders = null)
        {
            // Get existing open window or if none, make a new one :
            SubUpdateHelper window = (SubUpdateHelper)EditorWindow.GetWindow(typeof(SubUpdateHelper),false, "Folders");
            window.Init(sourceIsA, name1, name2, path1, path2, folders, callback);
            window.Show();
        }

        bool sourceIsA;
        string name1;
        string name2;
        string path1;
        string path2;
        Vector2 pos;
        List<string> folders;
        folder sdkFolder;
        Action<(bool, List<string>)> callback;
        GUIStyle style;

        public class folder
        {
            public bool display;
            public bool? selected = false;
            public string path;
            public string name;
            public List<folder> folders;

            public folder(string path)
            {
                this.path = path;
                display = false;
                selected = false;
                name = path.Split('\\').Last();
                folders = new List<folder>();
            }
        }

        void Init(bool sourceIsA, string name1, string name2, string path1, string path2, List<string> folders, Action<(bool, List<string>)> callback)
        {
            inited = true;
            if (folders == null)
                folders = new List<string>();

            this.sourceIsA = sourceIsA;
            this.path1 = path1;
            this.path2 = path2;
            this.name1 = name1;
            this.name2 = name2;
            this.folders = folders;
            this.callback = callback;
            InitFolder();

            style = new GUIStyle(EditorStyles.foldout);
            style.stretchWidth = false;
            //GUI.skin.toggle = style;
        }

        void OnGUI()
        {
            if (!inited)
                Close();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Source");
            if (GUILayout.Button(sourceIsA ? name1 : name2))
            {
                sourceIsA = !sourceIsA;
                InitFolder();
            }
            EditorGUILayout.EndHorizontal();
            pos = EditorGUILayout.BeginScrollView(pos);
            var indent = EditorGUI.indentLevel;
            Display(sdkFolder);
            EditorGUI.indentLevel = indent;
            EditorGUILayout.EndScrollView();
            if (GUILayout.Button("Validate"))
            {
                ComputeFolder();
                callback.Invoke((sourceIsA, folders));
                Close();
            }
            if (GUILayout.Button("Cancel"))
            {
                Close();
            }
        }

        void ComputeFolder()
        {
            folders.Clear();
            ComputeFolder(folders, sdkFolder);
        }

        void ComputeFolder(List<string> result, folder folder)
        {
            if (folder.selected == null)
                foreach (var f in folder.folders)
                {
                    ComputeFolder(result, f);
                }
            else if (folder.selected.Value)
                result.Add(folder.path);
        }


        void Display(folder folder, int i = 0)
        {
            EditorGUI.indentLevel = i;
            EditorGUILayout.BeginHorizontal();
            if (folder.folders.Count > 0)
            {
                folder.display = EditorGUILayout.Foldout(folder.display, GUIContent.none, style);
                EditorGUI.indentLevel -= 2;
            }
            else
            {
                EditorGUI.indentLevel += 2;
            }
            var b = folder.selected;

            EditorGUI.showMixedValue = folder.selected == null;
            folder.selected = EditorGUILayout.ToggleLeft(folder.name, (folder.selected ?? false), (GUILayoutOption[])null);
            EditorGUI.showMixedValue = false;
            if (b != folder.selected && !(!(folder.selected.Value) && b == null))
                SelectSub(folder);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (folder.display && folder.folders.Count > 0)
            {
                bool allSelected = true;
                bool allNotSelected = true;
                foreach (var f in folder.folders)
                {
                    Display(f, i + 1);
                    if (!(f.selected ?? false))
                        allSelected = false;
                    if ((f.selected ?? true))
                        allNotSelected = false;
                }
                folder.selected = allSelected ? true : allNotSelected ? false : (bool?)null;
            }

        }

        void SelectSub(folder folder)
        {
            foreach (var f in folder.folders)
            {
                if (f.selected != folder.selected)
                {
                    f.selected = folder.selected;
                    SelectSub(f);
                }
            }
        }

        void InitFolder()
        {
            var path = sourceIsA ? path1 : path2;
            sdkFolder = new folder(path);
            sdkFolder.selected = IsSelected(null, path);
            Found(sdkFolder);
            sdkFolder.display = true;

        }

        void Found(folder folder)
        {
            var d = Directory.GetDirectories(folder.path, "*", SearchOption.TopDirectoryOnly);
            foreach (var p in d)
            {
                var f = new folder(p);
                folder.folders.Add(f);
                f.selected = IsSelected(folder.selected, p);

                Found(f);
            }
        }
        bool? IsSelected(bool? parentState, string path)
        {
            bool? selected = false;
            if (parentState == null)
            {
                var p = folders.FirstOrDefault(pa => pa.StartsWith(path));
                selected = (p == null) ? false : (path == p) ? true : (bool?)null;
            }
            else if (parentState.Value)
                selected = parentState;
            return selected;
        }

    }
}
#endif