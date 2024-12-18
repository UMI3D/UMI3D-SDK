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
using inetum.unityUtils.systemIO;
using UnityEngine;

namespace inetum.unityUtils.saveSystem
{
    public enum SavingSystem
    {
        /// <summary>
        /// Plateforme specific behavior :
        /// <list type="bullet">
        /// <item>Windows: System.IO.File.</item>
        /// <item>Android: UnityEngine.PlayerPrefs.</item>
        /// </list>
        /// </summary>
        Default,
        /// <summary>
        /// Use System.IO.File to store files.
        /// </summary>
        FileSystem,
        /// <summary>
        /// Use UnityEngine.PlayerPrefs to store files.
        /// </summary>
        PlayerPrefs
    }

    public static class SaveManager
    {
        /// <summary>
        /// Save <paramref name="fileContents"/> with key <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileContents"></param>
        /// <param name="directories"></param>
        /// <param name="fileName"></param>
        /// <param name="path"></param>
        /// <param name="savingSystem"></param>
        /// <returns></returns>
        public static bool WriteToFile(
            string fileContents, 
            string directories,
            string fileName, 
            out string path,
            SavingSystem savingSystem = SavingSystem.Default
        )
        {
            bool hasSaved = false;
            switch (savingSystem)
            {
                case SavingSystem.Default:
#if UNITY_ANDROID && !UNITY_EDITOR
                    path = directories + fileName;
                    hasSaved = PlayerPrefsManager.WriteToFile(path, fileContents);
#else
                    hasSaved = FileManager.WriteToFile(
                        fileContents,
                        directories,
                        fileName, 
                        out path
                    );
#endif
                    break;
                case SavingSystem.FileSystem:
                    hasSaved = FileManager.WriteToFile(
                        fileContents,
                        directories,
                        fileName,
                        out path
                    );
                    break;
                case SavingSystem.PlayerPrefs:
                    path = directories + fileName;
                    hasSaved = PlayerPrefsManager.WriteToFile(path, fileContents);
                    break;
                default:
                    Debug.LogError($"Unknown saving system {savingSystem}");
                    path = null;
                    return false;
            }

            return hasSaved;
        }

        /// <summary>
        /// Load the value associated with key <paramref name="fileName"/> into <paramref name="result"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool LoadFromFile(
            string directories,
            string fileName,
            out string path,
            out string result, 
            SavingSystem savingSystem = SavingSystem.Default
        )
        {
            bool hasLoaded = false;
            switch (savingSystem)
            {
                case SavingSystem.Default:
#if UNITY_ANDROID && !UNITY_EDITOR
                    path = directories + fileName;
                    hasLoaded = PlayerPrefsManager.LoadFromFile(path, out result);
#else
                    hasLoaded = FileManager.LoadFromFile(
                        directories,
                        fileName, 
                        out path,
                        out result
                    );
#endif
                    break;

                case SavingSystem.FileSystem:
                    hasLoaded = FileManager.LoadFromFile(
                        directories,
                        fileName,
                        out path,
                        out result
                    );
                    break;

                case SavingSystem.PlayerPrefs:
                    path = directories + fileName;
                    hasLoaded = PlayerPrefsManager.LoadFromFile(path, out result);
                    break;

                default:
                    Debug.LogError($"Unknown saving system {savingSystem}");
                    path = null;
                    result = "";
                    return false;
            }

            return hasLoaded;
        }

        /// <summary>
        /// Rename the key <paramref name="fileName"/> to <paramref name="newFileName"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="newFileName"></param>
        /// <returns></returns>
        public static bool MoveFile(string fileName, string newFileName, SavingSystem savingSystem = SavingSystem.Default)
        {
            switch (savingSystem)
            {
                case SavingSystem.Default:
#if UNITY_ANDROID && !UNITY_EDITOR
                    return PlayerPrefsManager.MoveFile(fileName, newFileName);
#else
                    //return FileManager.MoveFile(fileName, newFileName);
#endif
                case SavingSystem.FileSystem:
                    //return FileManager.MoveFile(fileName, newFileName);
                case SavingSystem.PlayerPrefs:
                    return PlayerPrefsManager.Move(fileName, newFileName);
                default:
                    Debug.LogError($"Unknown saving system {savingSystem}");
                    return false;
            }
        }

        /// <summary>
        /// Whether the key <paramref name="fileName"/> exists.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool Exists(string fileName, SavingSystem savingSystem = SavingSystem.Default)
        {
            switch (savingSystem)
            {
                case SavingSystem.Default:
#if UNITY_ANDROID && !UNITY_EDITOR
                    return PlayerPrefsManager.Exists(fileName);
#else
                    return FileManager.Exists(fileName);
#endif
                case SavingSystem.FileSystem:
                    return FileManager.Exists(fileName);
                case SavingSystem.PlayerPrefs:
                    return PlayerPrefsManager.Exists(fileName);
                default:
                    Debug.LogError($"Unknown saving system {savingSystem}");
                    return false;
            }
        }
    }
}
