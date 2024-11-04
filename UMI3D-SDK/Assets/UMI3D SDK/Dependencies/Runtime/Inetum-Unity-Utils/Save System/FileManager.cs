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
        public static bool WriteToFile(
            string fileContents,
            string directories,
            string fileName,
            out string path
        )
        {
            path = InetumPath.Combine(Application.persistentDataPath, directories);

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create directory {directories} at path {path}.");
                    UnityEngine.Debug.LogException(e);
                    return false;
                }
            }

            path = InetumPath.Combine(path, fileName);

            try
            {
                File.WriteAllText(path, fileContents);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to {path}.");
                UnityEngine.Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// load <paramref name="result"/> from disk at <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool LoadFromFile(
            string directories,
            string fileName, 
            out string path,
            out string result
        )
        {
            path = InetumPath.Combine(Application.persistentDataPath, directories);

            if (!Directory.Exists(path))
            {
                result = null;
                return false;
            }

            path = InetumPath.Combine(path, fileName);

            if (!File.Exists(path))
            {
                result = null;
                return false;
            }

            try
            {
                result = File.ReadAllText(path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read file from {path}");
                UnityEngine.Debug.LogException(e);
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

        /// <summary>
        /// Whether a file exist at <paramref name="fileName"/>.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool Exists(string fileName)
        {
            var fullPath = InetumPath.Combine(Application.persistentDataPath, fileName);
            return File.Exists(fullPath);
        }
    }
}
