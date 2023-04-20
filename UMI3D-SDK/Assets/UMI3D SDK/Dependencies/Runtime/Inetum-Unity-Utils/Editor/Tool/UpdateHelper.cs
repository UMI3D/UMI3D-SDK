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
namespace inetum.unityUtils.editor
{
    using inetum.unityUtils;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;
    using Path = Path;

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
            draw.data.projectsLink.ForEach(
                l =>
                {
                    if (l?.projectA != null)
                        l.projectA.isSource = l.sourceIsA;
                    if (l?.projectB != null)
                        l.projectB.isSource = !l.sourceIsA;
                });
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
            SubUpdateHelper window = (SubUpdateHelper)EditorWindow.GetWindow(typeof(SubUpdateHelper), false, "Folders");
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
            var d = Directory.GetDirectories(folder.path, "*", System.IO.SearchOption.TopDirectoryOnly);
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


    public class ConfirmWindow : EditorWindow
    {
        bool inited = false;

        string warningMessage;

        bool shouldDeletePermanent;
        bool AutoComplete;

        GUIStyle style = new GUIStyle();

        public class DeletionData
        {
            public string fromPath;
            public string toPath;

            public string fromName;
            public string toName;

            public List<string> modifiedFolders;

            public bool isMovingFromInternal;
        }

        DeletionData data;

        public void Init(bool isAReferenceFolder, string name1, string name2, string path1, string path2, bool shouldDeletePermanent, bool AutoComplete, List<string> folders)
        {
            style.richText = true;
            style.normal.textColor = Color.white;

            data = new DeletionData()
            {
                fromPath = path1,
                toPath = path2,

                fromName = name1,
                toName = name2,

                modifiedFolders = folders,

                isMovingFromInternal = isAReferenceFolder
            };

            this.shouldDeletePermanent = shouldDeletePermanent;
            this.AutoComplete = AutoComplete;

            warningMessage = $"Are you sure your want to use the files from <b>{data.fromName}</b> to replace the ones from <b>{data.toName}</b>?";
            inited = true;
        }


        void OnGUI()
        {
            if (!inited)
                Close();

            EditorGUILayout.LabelField($"<b>{data.fromName} -> {data.toName}</b>", style);

            EditorGUILayout.LabelField(warningMessage, style);

            EditorGUILayout.LabelField("This will override the following folders and all their content:");

            EditorGUILayout.BeginScrollView(Vector2.zero);
            foreach (var folder in data.modifiedFolders)
            {
                EditorGUILayout.LabelField("- " + folder[folder.IndexOf("Assets/")..].Replace("\\", "/"));
            }
            EditorGUILayout.EndScrollView();

            shouldDeletePermanent = EditorGUILayout.ToggleLeft("Delete existing folders permanently", shouldDeletePermanent);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !AutoComplete;

            if (GUILayout.Button("Cancel") && !AutoComplete)
            {
                Close();
                shouldDeletePermanent = false;
                inited = false;
            }

            if (GUILayout.Button("Yes") && !AutoComplete)
            {
                UpdateHelperCopier.Copy(data.isMovingFromInternal, data.fromPath, data.toPath, data.modifiedFolders, shouldDeletePermanent);
                Close();
                shouldDeletePermanent = false;
                inited = false;
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
        }

        public async static void Open(bool isMovingFromReferenceFolder, string name1, string name2, string path1, string path2,bool shouldDeletePermanent, bool AutoComplete, List<string> folders = null)
        {
            // Get existing open window or if none, make a new one :
            var window = (ConfirmWindow)GetWindow(typeof(ConfirmWindow), false, "Confirm copy");
            window.Init(isMovingFromReferenceFolder, name1, name2, path1, path2, shouldDeletePermanent, AutoComplete, folders);
            window.Show();
            if (AutoComplete)
            {
                await Task.Yield();
                UpdateHelperCopier.Copy(window.data.isMovingFromInternal, window.data.fromPath, window.data.toPath, window.data.modifiedFolders, shouldDeletePermanent);
                window.shouldDeletePermanent = false;
                window.inited = false;
                window.Close();
            }
        }
    }


    public static class UpdateHelperCopier
    {
        public static void Copy(bool isMovingFromReferenceFolder, string pathFrom, string pathTo, List<string> modifiedFolders, bool shouldPermanentDelete)
        {
            if (modifiedFolders == null || modifiedFolders.Count == 0)
                return;

            if (!Directory.Exists(pathTo))
                Directory.CreateDirectory(pathTo);

            var movePathsInfos = modifiedFolders
                .Select(path => isMovingFromReferenceFolder ? path[pathFrom.Length..] : path[pathTo.Length..])
                .Select(relativePath =>
                {
                    return (originPath: Path.Combine(pathFrom, relativePath), targetPath: Path.Combine(pathTo, relativePath));
                }).ToList();


            var directoriesToDestroy = movePathsInfos
                                            .Where(movePaths => Directory.Exists(movePaths.targetPath))
                                            .Select(movePaths => movePaths.targetPath).ToList();

            if (directoriesToDestroy.Count > 0 && shouldPermanentDelete)
                foreach (var path in directoriesToDestroy)
                    Directory.Delete(path, true);
            else
                SafeDelete(directoriesToDestroy, isMovingFromReferenceFolder);


            foreach (var (originPath, targetPath) in movePathsInfos)
                CopyFolder(originPath, targetPath);

            if (!isMovingFromReferenceFolder)
                AssetDatabase.Refresh();
        }

        public static void CopyFolder(string pathFrom, string pathTo)
        {
            Directory.CreateDirectory(pathTo);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(pathFrom, "*", System.IO.SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(pathFrom, pathTo));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(pathFrom, "*.*", System.IO.SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(pathFrom, pathTo), true);
            }
        }

        /// <summary>
        /// Optimized for several deletions
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="areAssetExternals"></param>
        public static void SafeDelete(List<string> paths, bool areAssetExternals)
        {
            if (areAssetExternals) //in this case the pathTo is external to the project
            {
                var tmpPaths = paths.Select(path =>
                {
                    string folderName = path[(path.LastIndexOf("/") + 1)..];
                    string tmpPath = Path.Combine(Application.dataPath, @$"EXCLUDED/__TMP_UPDATE_HELPER_SAVE_{folderName}");
                    Directory.Move(path, tmpPath); // import in project to use API
                    return @$"Assets/EXCLUDED/__TMP_UPDATE_HELPER_SAVE_{folderName}";
                }).ToArray();

                AssetDatabase.Refresh();
                AssetDatabase.MoveAssetsToTrash(tmpPaths, new List<string>());
            }
            else
            { //in this case the pathTo is internal to the project
                AssetDatabase.MoveAssetsToTrash(paths.Select(path => path[path.IndexOf("Assets/")..]).ToArray(), new List<string>());
            }
        }
    }

}
#endif