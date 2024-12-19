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
    /// <summary>
    /// A file manager to load and save file on disk.
    /// </summary>
    public static class FileManager
    {
        /// <summary>
        /// This method writes the given contents to a file at the specified directory and file name.<br/>
        /// It validates the file name and directory path, creates directories if needed, and writes the contents to the file.<br/>
        /// <br/>
        /// <example>
        /// Given file contents, directories, and file name when writing to a file then return true or false.<br/>
        /// <code>
        /// bool result1 = FileManager.WriteToFile(null, null, null, out string path1); // result1 = false
        /// bool result2 = FileManager.WriteToFile(null, null, "", out string path2); // result2 = false
        /// bool result3 = FileManager.WriteToFile(null, null, "FileName.txt", out string path3); // result3 = true
        /// bool result4 = FileManager.WriteToFile(null, "TestDirectory", "FileName.txt", out string path4); // result4 = true
        /// </code> 
        /// </example>
        /// </summary>
        /// <param name="fileContents">The contents to write to the file.</param>
        /// <param name="directories">The directories where the file should be placed.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="path">The full path of the written file.</param>
        /// <returns>True if the file was successfully written, otherwise false.</returns>
        public static bool WriteToFile(
            string fileContents,
            string directories,
            string fileName,
            out string path
        )
        {
            fileName = fileName.TrimDirectorySeparator();
            if (!fileName.IsValideFileName())
            {
                Debug.LogError($"[FileManager.WriteToFile] Invalide file name '{fileName}'.");
                path = null;
                return false;
            }

            if (!directories.IsValidePath(allowNullAndEmpty: true))
            {
                Debug.LogError($"[FileManager.WriteToFile] Invalide directory '{directories}'.");
                path = null;
                return false;
            }

            CreateDirectoriesIfNeeded(directories, out bool exists);
            if (!exists) 
            {
                Debug.LogError($"[FileManager.WriteToFile] Failed to write to file because directories '{directories}' couldn't be created.");
                path = null;
                return false; 
            }

            path = FullPathFromPersistentDataPath(directories);

            path = Path.Combine(path, fileName);

            try
            {
                System.IO.File.WriteAllText(path, fileContents);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileManager.WriteToFile] Failed to write to {path}.");
                UnityEngine.Debug.LogException(e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// This method loads the contents of a file from the specified directory and file name.<br/>
        /// It validates the file name and directory path, checks if the file exists, and reads the contents of the file.<br/>
        /// <br/>
        /// <example>
        /// Given directories and file name when loading from a file then return true or false.<br/>
        /// <code>
        /// bool result1 = FileManager.LoadFromFile(null, null, out string path1, out string content1); // result1 = false
        /// bool result2 = FileManager.LoadFromFile(null, "File That Does not exist", out string path2, out string content2); // result2 = false
        /// bool result3 = FileManager.LoadFromFile("TestDirectory", "TestFile.txt", out string path3, out string content3); // result3 = true (if TestFile.txt exists.)
        /// </code> 
        /// </example>
        /// </summary>
        /// <param name="directories">The directories where the file is located.</param>
        /// <param name="fileName">The name of the file to load.</param>
        /// <param name="path">The full path of the loaded file.</param>
        /// <param name="content">The contents of the loaded file.</param>
        /// <returns>True if the file was successfully loaded, otherwise false.</returns>
        public static bool LoadFromFile(
            string directories,
            string fileName, 
            out string path,
            out string content
        )
        {
            fileName = fileName.TrimDirectorySeparator();
            if (!fileName.IsValideFileName())
            {
                Debug.LogError($"[FileManager.LoadFromFile] Invalide file name '{fileName}'.");
                path = null;
                content = null;
                return false;
            }

            if (!directories.IsValidePath(allowNullAndEmpty: true))
            {
                Debug.LogError($"[FileManager.LoadFromFile] Invalide directory '{directories}'.");
                path = null;
                content = null;
                return false;
            }

            string partialPath = Path.Combine(directories, fileName);
            if (!Exists(partialPath))
            {
                Debug.LogError($"[FileManager.LoadFromFile] File at {partialPath} doesn't exist.");
                path = null;
                content = null;
                return false;
            }

            path = FullPathFromPersistentDataPath(partialPath);

            try
            {
                content = System.IO.File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileManager.LoadFromFile] Failed to read file from {path}");
                Debug.LogException(e);
                content = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method deletes a file at the given path.<br/>
        /// It constructs the full path from the partial path, checks if the file exists, and attempts to delete it.<br/>
        /// <br/>
        /// <example>
        /// Given a partial file path when deleting the file then return true or false.<br/>
        /// <code>
        /// bool result1 = FileManager.Delete(null); // result1 = false
        /// bool result2 = FileManager.Delete("TestFile"); // result2 = false (if file does not exist)
        /// bool result3 = FileManager.Delete("TestFile"); // result3 = true (if file exists)
        /// </code> 
        /// </example>
        /// </summary>
        /// <param name="filePath">The file path of the file to delete.</param>
        /// <returns>True if the file was successfully deleted, otherwise false.</returns>
        public static bool Delete(string filePath)
        {
            string fullPath = FullPathFromPersistentDataPath(filePath);

            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogError($"[FileManager.Delete] Try to delete a file (at: {fullPath}) that doesn't exist.");
                return false;
            }

            if (!Exists(filePath))
            {
                if (System.IO.Directory.Exists(fullPath))
                {
                    Debug.LogError($"[FileManager.Delete] Try to delete a file but path point to a directory '{fullPath}'.");
                    return false;
                }
                else
                {
                    Debug.LogError($"[FileManager.Delete] Try to delete a file (at: {fullPath}) that doesn't exist.");
                    return false;
                }
            }

            try
            {
                System.IO.File.Delete(fullPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileManager.Delete] Failed to delete file {fullPath}.");
                UnityEngine.Debug.LogException(e);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Moves a file from one location to another, creating directories if needed.<br/>
        /// <br/>
        /// <example>
        /// Given a file path, target directory, and new file name when moving the file then return true and the new path.
        /// <code>
        /// bool result = FileManager.Move("TestDirectory/TestFile", "NewTestDirectory", "newTestFile", out string newPath);
        /// // result = true
        /// // newPath = "/AppData/LocalLow/CompanyName/ProductName/NewTestDirectory/newTestFile"
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="fileToMovePath">The path of the file to move.</param>
        /// <param name="directories">The target directory where the file should be moved.</param>
        /// <param name="fileName">The new name of the file.</param>
        /// <param name="newPath">The new path of the moved file.</param>
        /// <returns>True if the file was moved successfully, otherwise false.</returns>
        public static bool Move(
            string fileToMovePath,
            string directories,
            string fileName,
            out string newPath
        )
        {
            string fullPath = FullPathFromPersistentDataPath(fileToMovePath);

            if (!Exists(fullPath))
            {
                Debug.LogError($"[FileManager.MoveFile] Failed to move file because file does not exist {fullPath}.");
                newPath = null;
                return false;
            }

            CreateDirectoriesIfNeeded(directories, out bool exists);
            if (!exists)
            {
                Debug.LogError($"[FileManager.MoveFile] Failed to write to file because directories '{directories}' couldn't be created.");
                newPath = null;
                return false;
            }

            string newPartialPath = Path.Combine(directories, fileName);
            newPath = FullPathFromPersistentDataPath(newPartialPath);

            try
            {
                if (System.IO.File.Exists(newPath))
                {
                    System.IO.File.Delete(newPath);
                }
                System.IO.File.Move(fullPath, newPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[FileManager.MoveFile] Failed to move file from {fullPath} to {newPath}.");
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        static void CreateDirectoriesIfNeeded(string directories, out bool exists)
        {
            string path = FullPathFromPersistentDataPath(directories);

            if (!System.IO.Directory.Exists(path))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create directory {directories} at path {path}.");
                    UnityEngine.Debug.LogException(e);
                    exists = false;
                }
            }
            exists = true;
        }

        /// <summary>
        /// This method checks if a file exists at the given file path.<br/>
        /// It constructs the full path from the partial path and checks for the file's existence.<br/>
        /// <br/>
        /// <example>
        /// Given a file path when checking if the file exists then return true or false.<br/>
        /// <code>
        /// bool result1 = FileManager.Exists(null); // result1 = false
        /// bool result2 = FileManager.Exists(""); // result2 = false
        /// bool result3 = FileManager.Exists("NotAFile"); // result3 = false
        /// bool result4 = FileManager.Exists("TestDirectory/TestFile.txt"); // result4 = true if the file exist.
        /// </code> 
        /// </example>
        /// </summary>
        /// <param name="filePath">The partial or full file path to check.</param>
        /// <returns>True if the file exists, otherwise false.</returns>
        public static bool Exists(string filePath)
        {
            string fullPath = FullPathFromPersistentDataPath(filePath);
            return System.IO.File.Exists(fullPath);
        }

        /// <summary>
        /// This method combines the given path with the persistent data path of the application to create a full path.<br/>
        /// <br/>
        /// <example>
        /// Given a relative path when combining with the persistent data path then the full path.<br/>
        /// <code>
        /// string fullPath = FileManager.FullPathFromPersistentDataPath("Test.txt"); // fullPath ends with "/AppData/LocalLow/[CompanyName]/[ProductName]/Test.txt"
        /// </code> 
        /// </example>
        /// </summary>
        /// <param name="path">The relative path to be combined with the persistent data path.</param>
        /// <returns>The full path combining the persistent data path and the given relative path.</returns>
        public static string FullPathFromPersistentDataPath(string path)
        {
            if (path?.StartsWith(Application.persistentDataPath) ?? false)
            {
                path = path.Substring(Application.persistentDataPath.Length);
            }

            return Path.Combine(Application.persistentDataPath, path);
        }
    }
}
