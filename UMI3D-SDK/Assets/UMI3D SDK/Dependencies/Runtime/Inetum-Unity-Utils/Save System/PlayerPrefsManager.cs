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
using UnityEngine;

namespace inetum.unityUtils.saveSystem
{
    public static class PlayerPrefsManager
    {
        /// <summary>
        /// Save <paramref name="fileContents"/> in PlayerPrefs with key <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileContents"></param>
        /// <returns></returns>
        public static bool WriteToFile(string fileName, string fileContents)
        {
            try
            {
                PlayerPrefs.SetString(fileName, fileContents);
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to {fileName} with exception {e}");
                return false;
            }
        }

        /// <summary>
        /// Load the value associated with key <paramref name="fileName"/> from PlayerPrefs into <paramref name="result"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool LoadFromFile(string fileName, out string result)
        {
            try
            {
                result = PlayerPrefs.GetString(fileName);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read from {fileName} with exception {e}");
                result = "";
                return false;
            }
        }

        /// <summary>
        /// Rename the PlayerPrefs key <paramref name="fileName"/> to <paramref name="newFileName"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="newFileName"></param>
        /// <returns></returns>
        public static bool MoveFile(string fileName, string newFileName)
        {
            try
            {
                string fileContents = PlayerPrefs.GetString(fileName);
                PlayerPrefs.DeleteKey(fileName);
                PlayerPrefs.SetString(newFileName, fileContents);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to move file from {fileName} to {newFileName} with exception {e}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Whether a PlayerPrefs key exists with the name <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool Exists(string fileName)
        {
            return PlayerPrefs.HasKey(fileName);
        }
    }
}
