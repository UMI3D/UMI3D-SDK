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

namespace inetum.unityUtils.systemIO
{
    public static class PlayerPrefsManager
    {
        /// <summary>
        /// Writes content to a file identified by a given file name using PlayerPrefs.<br/>
        /// <br/>
        /// <example>
        /// Given a file name and content when writing to the file then return true.
        /// <code>
        /// bool result = PlayerPrefsManager.WriteToFile("TestKey", "This is a content");
        /// // result = true
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="fileName">The name of the file to write to.</param>
        /// <param name="content">The content to write to the file.</param>
        /// <returns>True if the content was written successfully, otherwise false.</returns>
        public static bool WriteToFile(string fileName, string content)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError($"[PlayerPrefsManager.WriteToFile] Try to write to an empty or null file.");
                return false;
            }

            try
            {
                PlayerPrefs.SetString(fileName, content);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerPrefsManager.WriteToFile] Failed to write to {fileName}.");
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads content from a file identified by a given file name using PlayerPrefs.<br/>
        /// <br/>
        /// <example>
        /// Given a file name when loading the file then return true and the content.
        /// <code>
        /// bool result = PlayerPrefsManager.LoadFromFile("TestKey", out string content);
        /// // result = true
        /// // content = "This is a content"
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="fileName">The name of the file to load from.</param>
        /// <param name="content">The content loaded from the file.</param>
        /// <returns>True if the content was loaded successfully, otherwise false.</returns>
        public static bool LoadFromFile(string fileName, out string content)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError($"[PlayerPrefsManager.LoadFromFile] Try to load from an empty or null file.");
                content = null;
                return false;
            }

            if (!Exists(fileName))
            {
                Debug.LogError($"[PlayerPrefsManager.LoadFromFile] Try to load from a key '{fileName}' that does not exist.");
                content = null;
                return false;
            }

            try
            {
                content = PlayerPrefs.GetString(fileName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerPrefsManager.LoadFromFile] Failed to read from {fileName}.");
                Debug.LogException(e);
                content = null;
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Moves the content of a file identified by a given file name to a new file name using PlayerPrefs.<br/>
        /// <br/>
        /// <example>
        /// Given a file name and a new file name when moving the file then return true.
        /// <code>
        /// bool result = PlayerPrefsManager.Move("TestKey", "NewTestKey");
        /// // result = true
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="fileName">The name of the file to move.</param>
        /// <param name="newFileName">The new name of the file.</param>
        /// <returns>True if the file was moved successfully, otherwise false.</returns>
        public static bool Move(string fileName, string newFileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError($"[PlayerPrefsManager.Move] Try to move from an empty or null file.");
                return false;
            }

            if (!Exists(fileName))
            {
                Debug.LogError($"[PlayerPrefsManager.Move] Try to move from a file '{fileName}' that does not exist.");
                return false;
            }

            if (string.IsNullOrEmpty(newFileName))
            {
                Debug.LogError($"[PlayerPrefsManager.Move] Try to move from file to an empty or null new file.");
                return false;
            }

            string fileContents;
            try
            {
                fileContents = PlayerPrefs.GetString(fileName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerPrefsManager.MoveFile] Failed to read from {fileName}");
                Debug.LogException(e);
                return false;
            }

            PlayerPrefs.DeleteKey(fileName);

            try
            {
                PlayerPrefs.SetString(newFileName, fileContents);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerPrefsManager.MoveFile] Failed to write to {fileName}");
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deletes a file identified by a given file name using PlayerPrefs.<br/>
        /// <br/>
        /// <example>
        /// Given a file name when deleting the file then return true.
        /// <code>
        /// bool result = PlayerPrefsManager.Delete("TestKey");
        /// // result = true
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <returns>True if the file was deleted successfully, otherwise false.</returns>
        public static bool Delete(string fileName)
        {
            if (!Exists(fileName))
            {
                Debug.LogError($"[PlayerPrefsManager.Delete] Try to delete a key '{fileName}' that does not exist.");
                return false;
            }

            try
            {
                PlayerPrefs.DeleteKey(fileName);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"[PlayerPrefsManager.Delete] Failed to delete {fileName}");
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if a file identified by a given file name exists using PlayerPrefs.<br/>
        /// <br/>
        /// <example>
        /// Given a file name when checking if the file exists then return true if it exists.
        /// <code>
        /// bool result = PlayerPrefsManager.Exists("TestKey");
        /// // result = true if the key exist.
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="fileName">The name of the file to check for existence.</param>
        /// <returns>True if the file exists, otherwise false.</returns>
        public static bool Exists(string fileName)
        {
            return PlayerPrefs.HasKey(fileName);
        }
    }
}
