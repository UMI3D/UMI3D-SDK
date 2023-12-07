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
using System;
using System.IO;
using UnityEngine;
using InetumPath = inetum.unityUtils.Path;

namespace inetum.unityUtils.saveSystem
{
    /// <summary>
    /// A file manager to load and save file on disk.
    /// </summary>
    public static class FileManager
    {
        /// <summary>
        /// Save <paramref name="fileContents"/> on disk at <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileContents"></param>
        /// <returns></returns>
        public static bool WriteToFile(string fileName, string fileContents)
        {
            var fullPath = InetumPath.Combine(Application.persistentDataPath, fileName);

            try
            {
                File.WriteAllText(fullPath, fileContents);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to {fullPath} with exception {e}");
                return false;
            }
        }

        /// <summary>
        /// load <paramref name="result"/> from disk at <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool LoadFromFile(string fileName, out string result)
        {
            var fullPath = InetumPath.Combine(Application.persistentDataPath, fileName);

            try
            {
                result = File.ReadAllText(fullPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read from {fullPath} with exception {e}");
                result = "";
                return false;
            }
        }

        /// <summary>
        /// Move a file from <paramref name="fileName"/> to <paramref name="newFileName"/>.<br/>
        /// If <paramref name="newFileName"/> already exist it will be overridden.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="newFileName"></param>
        /// <returns></returns>
        public static bool MoveFile(string fileName, string newFileName)
        {
            var fullPath = InetumPath.Combine(Application.persistentDataPath, fileName);
            var newFullPath = InetumPath.Combine(Application.persistentDataPath, newFileName);

            try
            {
                if (File.Exists(newFullPath))
                {
                    File.Delete(newFullPath);
                }
                File.Move(fullPath, newFullPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to move file from {fullPath} to {newFullPath} with exception {e}");
                return false;
            }

            return true;
        }
    }
}
