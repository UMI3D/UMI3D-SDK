/*
Copyright 2019 - 2023 Inetum

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
using UnityEngine;

namespace inetum.unityUtils.saveSystem
{
    /// <summary>
    /// 
    /// </summary>
    // Copy past the line below in a sub class to create a save system.
    //[CreateAssetMenu(fileName = "New SSO Save System", menuName = "Utils/Save System/SSO Save System")]
    public class SSOSaveSystem<T> : ScriptableObject
        where T : SerializableScriptableObject
    {
        /// <summary>
        /// Name of the file.
        /// </summary>
        public string saveFilename;
        public SSOSave saveData = new();

        string backupFilename;

        private void OnValidate()
        {
            backupFilename = $"{saveFilename}_backup";
        }

        /// <summary>
        /// Load the data from disk at <see cref="saveFilename"/>.
        /// </summary>
        /// <returns></returns>
        public bool LoadSaveDataFromDisk()
        {
            if (FileManager.LoadFromFile(saveFilename, out var json))
            {
                saveData.LoadFromJson(json);
                return true;
            }
            else if (FileManager.LoadFromFile(backupFilename, out json))
            {
                saveData.LoadFromJson(json);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Save the data to disk at <see cref="saveFilename"/>.
        /// </summary>
        /// <param name="data"></param>
        public void SaveDataToDisk((T sso, int amount)[] data)
        {
            saveData.ssoStacks.Clear();
            foreach (var datum in data)
            {
                saveData.ssoStacks.Add(new(datum.sso, datum.amount));
            }

            if (FileManager.MoveFile(saveFilename, backupFilename))
            {
                if (FileManager.WriteToFile(saveFilename, saveData.ToJson()))
                {
                    Debug.Log("Save successful");
                }
            }
        }
    }
}
