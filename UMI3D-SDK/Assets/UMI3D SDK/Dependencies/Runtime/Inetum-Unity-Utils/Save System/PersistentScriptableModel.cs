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

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace inetum.unityUtils.saveSystem
{
    public class PersistentScriptableModel : ScriptableObject
    {
        const string BACKUP_PREFIX = "BACKUP";

        /// <summary>
        /// Name of the file.
        /// </summary>
        public string saveFilename;

        /// <summary>
        /// Name of the backup file.
        /// </summary>
        public string BackupFileName
        {
            get
            {
                return $"{BACKUP_PREFIX}_{saveFilename}";
            }
        }

        bool isWaitingToSave;

        public PersistentScriptableModel()
        {
            saveFilename = GetType().Name;
        }

        /// <summary>
        /// Load the data.
        /// </summary>
        public void Load(bool editorOnly = false)
        {
            if (editorOnly)
            {
#if UNITY_EDITOR

#endif
            }
            else
            {
                LoadDataFromDisk();
            }
        }

        /// <summary>
        /// Save the data.
        /// </summary>
        /// <param name="waitingDuration">The duration in second to wait before saving to avoid spamming.</param>
        /// <param name="editorOnly">Whether to use the built-in scriptable serialization feature or to save on disk.</param>
        public async void Save(int waitingDuration = 5, bool editorOnly = false)
        {
            if (waitingDuration > 0)
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
                SaveDataToDisk();
            }
        }

        #region Load and Save persistently on disk

        public void LoadBackup()
        {
            if (!SaveManager.LoadFromFile(BackupFileName, out string json))
            {
                throw new Exception($"[{nameof(PersistentScriptableModel)}]: couldn't load json backup to this ({GetType().Name})");
            }

            try
            {
                LoadFromJson(json);
            }
            catch (Exception e)
            {
                var exception = new Exception($"[{nameof(PersistentScriptableModel)}]: couldn't load json to this ({GetType().Name})");
                UnityEngine.Debug.LogException(exception);
                throw e;
            }
        }

        [ContextMenu("Save Backup file")]
        public void SaveBackup()
        {
            if (!SaveManager.WriteToFile(BackupFileName, SaveToJson()))
            {
                throw new Exception($"[{nameof(PersistentScriptableModel)}]: couldn't save backup file for ({GetType().Name})");
            }
        }

        /// <summary>
        /// Load the data from disk at <see cref="saveFilename"/> or <see cref="BACKUP_PREFIX"/>_<see cref="saveFilename"/>.
        /// </summary>
        /// <returns></returns>
        void LoadDataFromDisk()
        {
            if (!SaveManager.Exists(BackupFileName))
            {
                SaveBackup();
            }

            string json = null;
            Func<bool> isJsonLoaded = () =>
            {
                return SaveManager.LoadFromFile(saveFilename, out json)
                || SaveManager.LoadFromFile(BackupFileName, out json);
            };
            
            if (!isJsonLoaded())
            {
                throw new Exception($"[{nameof(PersistentScriptableModel)}]: no Json file found for ({GetType().Name})");
            }

            try
            {
                LoadFromJson(json);
            }
            catch (Exception e)
            {
                var exception = new Exception($"[{nameof(PersistentScriptableModel)}]: couldn't load json to this ({GetType().Name})");
                UnityEngine.Debug.LogException(exception);
                throw e;
            }
        }

        /// <summary>
        /// Save the data to disk at <see cref="saveFilename"/>.
        /// </summary>
        /// <param name="data"></param>
        void SaveDataToDisk()
        {
            if (!SaveManager.WriteToFile(saveFilename, SaveToJson()))
            {
                UnityEngine.Debug.LogError($"[{nameof(PersistentScriptableModel)}]: couldn't save file for ({GetType().Name})");
            }
        }

        #endregion

        #region JSON

        /// <summary>
        /// Convert this to a Json file.
        /// </summary>
        /// <returns></returns>
        string SaveToJson()
        {
            return JsonUtility.ToJson(this);
        }

        /// <summary>
        /// Apply a Json file to overwrite this data.
        /// </summary>
        /// <param name="json"></param>
        void LoadFromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }

        #endregion
    }
}