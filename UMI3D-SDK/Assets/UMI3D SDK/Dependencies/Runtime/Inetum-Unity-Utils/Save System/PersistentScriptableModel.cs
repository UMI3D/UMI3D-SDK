/*
Copyright 2019 - 2024 Inetum

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

using inetum.unityUtils.lifeCycle;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace inetum.unityUtils.saveSystem
{
    public class PersistentScriptableModel : ScriptableObject
    {
        const string PREFIX_BACKUP = "BACKUP";

        /// <summary>
        /// The directories where to save the file.
        /// </summary>
        public string directories;

        /// <summary>
        /// Name of the file.
        /// </summary>
        public string saveFilename;

        /// <summary>
        /// Name of the backup file.
        /// </summary>
        public string BackupFileName => $"{PREFIX_BACKUP}_{saveFilename}";

        string path;

        public PersistentScriptableModel()
        {
            saveFilename = GetType().Name;
        }

        #region Save Model to disk

        bool isWaitingToSave;

        /// <summary>
        /// Save the data.
        /// </summary>
        /// <param name="waitingDuration">The duration in second to wait before saving to avoid spamming.</param>
        /// <param name="editorOnly">Whether to use the built-in scriptable serialization feature or to save on disk.</param>
        public async void Save(int waitingDuration = 5, bool editorOnly = false)
        {
            if (!Quitting.instance && waitingDuration > 0)
            {
                if (isWaitingToSave)
                {
                    return;
                }

                isWaitingToSave = true;
                await Task.Delay(waitingDuration * 1000);

                if (!isWaitingToSave)
                {
                    return;
                }
            }

            isWaitingToSave = false;
            if (editorOnly)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
            else
            {
                TrySaveDataToDisk(saveFilename);
            }
        }

        [ContextMenu("Save Backup file")]
        public void SaveBackup()
        {
            TrySaveDataToDisk(BackupFileName);
        }

        /// <summary>
        /// Save the data to disk at <see cref="saveFilename"/>.
        /// </summary>
        /// <param name="data"></param>
        bool TrySaveDataToDisk(string fileName)
        {
            if (!TrySaveToJson(out string json))
            {
                return false;
            }

            try
            {
                SaveManager.WriteToFile(
                    json,
                    directories,
                    fileName,
                    out path
                );
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[PersistentScriptableModel] Cannot save file [{fileName}]");
                UnityEngine.Debug.LogException(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Convert this to a Json file.
        /// </summary>
        /// <returns></returns>
        bool TrySaveToJson(out string json)
        {
            try
            {
                json = JsonUtility.ToJson(this);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[PersistentScriptableModel] Cannot convert the file [{saveFilename}] to its json.");
                UnityEngine.Debug.LogException(e);
                json = null;
                return false;
            }

            return true;
        }

        #endregion

        #region Load Model from disk

        /// <summary>
        /// Whether the backup file has been saved.
        /// </summary>
        bool isBackupSaved = false;

        /// <summary>
        /// Load the data.
        /// </summary>
        public void Load(bool editorOnly = false)
        {
            bool hasLoaded = TryLoadDataFromDisk(saveFilename);

            if (!editorOnly && hasLoaded && !isBackupSaved)
            {
                isBackupSaved = TrySaveDataToDisk(BackupFileName);
            }

            TryLoadDataFromDisk(BackupFileName);
        }

        /// <summary>
        /// Load the backup file.
        /// </summary>
        [ContextMenu("Load Backup file")]
        public void LoadBackup()
        {
            TryLoadDataFromDisk(BackupFileName);
        }

        /// <summary>
        /// Load the data from disk.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool TryLoadDataFromDisk(string fileName)
        {
            bool hasGetJson = SaveManager.LoadFromFile(
                directories,
                fileName,
                out string path,
                out string json
            );

            if (!hasGetJson)
            {
                return false;
            }

            return TryLoadFromJson(json);
        }

        /// <summary>
        /// Apply a Json file to overwrite this data.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        bool TryLoadFromJson(string json)
        {
            try
            {
                JsonUtility.FromJsonOverwrite(json, this);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[PersistentScriptableModel] Cannot override the file [{saveFilename}] from its json.");
                UnityEngine.Debug.LogException(e);
                return false;
            }

            return true;
        }

        #endregion
    }
}