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

    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// This class let you use a <see cref="ScriptableObject"/> in an editor window.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScriptableLoader<T> where T : ScriptableObject
    {
        public readonly string path;
        public readonly string folderPath;

        public T data
        {
            get { return GetScriptable(); }
        }

        public Editor editor
        {
            get { return GetEditor(data); }
        }

        /// <summary>
        /// Create a Scriptable Loader.
        /// This class let you use a <see cref="ScriptableObject"/> in an editor window.
        /// It will load or create the scriptableObject.
        /// The path of the scriptable is Asset/<folder>/<fileName>
        /// </summary>
        /// <param name="folder">folder in which the scriptable should be.
        /// The folder should be relative to the AssetFolder.
        /// </param>
        /// <param name="fileName">name of the scriptableObject</param>
        public ScriptableLoader(string folder, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new System.Exception("Filename can not be null or empty");
            if (!fileName.EndsWith(".asset"))
                fileName += ".asset";

            string sep = "";

            if (string.IsNullOrEmpty(folder))
                folder = "";
            else
            {
                sep = "/";
                if (folder.EndsWith("/"))
                    folder = folder.Remove(folder.Length - 1);
            }

            this.path = "Assets/" + folder + sep + fileName;
            this.folderPath = folder;
        }

        /// <summary>
        /// Create a Scriptable Loader.
        /// This class let you use a <see cref="ScriptableObject"/> in an editor window.
        /// It will load or create the scriptableObject.
        /// The path of the scriptable is Asset/EXCLUDED/<fileName>
        /// </summary>
        /// <param name="fileName">name of the scriptableObject</param>
        public ScriptableLoader(string fileName) : this("EXCLUDED", fileName)
        {
        }

        #region Scriptable Handler
        T _data;
        Editor _editor;



        Editor GetEditor(ScriptableObject _data)
        {
            if (_editor == null)
                _editor = Editor.CreateEditor(_data);
            return _editor;
        }

        T GetScriptable() => _data ?? LoadScriptable() ?? CreateScriptable();

        T CreateScriptable()
        {
            CreateFolder();
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        void CreateFolder()
        {
            if (!string.IsNullOrEmpty(folderPath))
                if (!System.IO.Directory.Exists(Application.dataPath + System.IO.Path.GetDirectoryName(path).TrimStart("Assets".ToCharArray())))
                {
                    AssetDatabase.CreateFolder("Assets", folderPath);
                }
        }

        T LoadScriptable()
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            return asset;
        }
        #endregion
    }
}
#endif