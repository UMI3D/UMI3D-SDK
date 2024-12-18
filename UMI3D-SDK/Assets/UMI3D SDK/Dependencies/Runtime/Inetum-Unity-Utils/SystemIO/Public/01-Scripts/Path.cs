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

using System;

namespace inetum.unityUtils.systemIO
{
    public static class Path
    {
        static readonly char[] charsToTrim = { 
            System.IO.Path.DirectorySeparatorChar, 
            System.IO.Path.AltDirectorySeparatorChar 
        };

        static readonly char[] invalidPathChars
            = System.IO.Path.GetInvalidPathChars();

        static readonly char[] invalidFileNameChars
            = System.IO.Path.GetInvalidFileNameChars();

        /// <summary>
        /// This method combines multiple path strings into a single path, ensuring that directory separators are correctly handled.<br/>
        /// <br/>
        /// <example>
        /// Given multiple path strings when combining them then a single combined path.<br/>
        /// <code>
        /// string result = Path.Combine("Value1", "Value2"); // result = "Value1/Value2"
        /// </code> 
        /// </example>
        /// </summary>
        /// <param name="paths">An array of path strings to be combined.</param>
        /// <returns>The combined path string, or null if the input array is null or empty.</returns>
        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
            {
                return null;
            }

            string[] trimmedPaths = new string[paths.Length];
            Array.Copy(paths, trimmedPaths, paths.Length);
            trimmedPaths[0] = trimmedPaths[0]?.TrimEnd(charsToTrim);
            string result = trimmedPaths[0];
            for (int i = 1; i < trimmedPaths.Length; i++)
            {
                trimmedPaths[i] = trimmedPaths[i].TrimDirectorySeparator();
                result = _Combine(result, trimmedPaths[i]);
            }

            return result.ReplaceSeparatorByAltDirectorySeparatorChar();
        }

        static string _Combine(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1))
            {
                return path2;
            }
            else if (string.IsNullOrEmpty(path2))
            {
                return path1;
            }
            else
            {
                return path1 + System.IO.Path.AltDirectorySeparatorChar + path2;
            }
        }

        /// <summary>
        /// This method trims directory separators ('/' and '\') from the start and end of the given path string.<br/>
        /// <br/>
        /// <example>
        /// Given a string path when trimming directory separators then the path without leading or trailing separators.<br/>
        /// <code>
        /// string result = "Value/".TrimDirectorySeparator(); // result = "Value"
        /// </code> 
        /// </example>
        /// </summary>
        /// <param name="path">The path string to be trimmed.</param>
        /// <returns>The trimmed path string, or null if the input path is null.</returns>
        public static string TrimDirectorySeparator(this string path)
        {
            return path?.Trim(charsToTrim).TrimEnd(charsToTrim) ?? null;
        }

        /// <summary>
        /// This method replaces the directory separator character with the alternative directory separator character in the given path string.<br/>
        /// <br/>
        /// <example>
        /// Given a path string when replacing directory separators then the path with alternative directory separators.<br/>
        /// <code>
        /// string result = Path.ReplaceSeparatorByAltDirectorySeparatorChar("Value0\\Value1/Value2"); // result = "Value0/Value1/Value2"
        /// </code> 
        /// </example>
        /// </summary>
        /// <param name="path">The path string in which the directory separator character will be replaced.</param>
        /// <returns>The path string with the directory separator character replaced by the alternative directory separator character, or null if the input path is null.</returns>
        public static string ReplaceSeparatorByAltDirectorySeparatorChar(this string path)
        {
            return path?.Replace(
                System.IO.Path.DirectorySeparatorChar, 
                System.IO.Path.AltDirectorySeparatorChar
            ) ?? null;
        }

        /// <summary>
        /// This method inserts the alternative directory separator character at the specified index in the given path string.<br/>
        /// <br/>
        /// <example>
        /// Given a path string and an index when inserting the alternative directory separator character then the path with the character inserted.<br/>
        /// <code>
        /// string result = "Value".InsertAltDirectorySeparatorChar(0); // result = "/Value"
        /// </code> 
        /// </example>
        /// </summary>
        /// <param name="path">The path string in which the alternative directory separator character will be inserted.</param>
        /// <param name="index">The index at which the alternative directory separator character will be inserted.</param>
        /// <returns>The path string with the alternative directory separator character inserted at the specified index.</returns>
        /// <exception cref="System.IndexOutOfRangeException">Thrown when the index is out of range.</exception>
        public static string InsertAltDirectorySeparatorChar(this string path, int index)
        {
            if (index < 0)
            {
                throw new System.IndexOutOfRangeException("Index must be >= 0");
            }

            if ((path?.Length ?? 0) < index)
            {
                throw new System.IndexOutOfRangeException($"Index must be <= path.Length (index: {index}; path/Length: {path.Length})");
            }

            if (string.IsNullOrEmpty(path))
            {
                return System.IO.Path.AltDirectorySeparatorChar.ToString();
            }

            if (index == 0)
            {
                if (path[0] != System.IO.Path.AltDirectorySeparatorChar)
                {
                    return System.IO.Path.AltDirectorySeparatorChar + path;
                }
                else
                {
                    return path;
                }
            }

            if (index == path.Length)
            {
                if (path[path.Length - 1] != System.IO.Path.AltDirectorySeparatorChar)
                {
                    return path + System.IO.Path.AltDirectorySeparatorChar;
                }
                else
                {
                    return path;
                }
            }

            if (path[index - 1] != System.IO.Path.AltDirectorySeparatorChar 
                && path[index] != System.IO.Path.AltDirectorySeparatorChar)
            {
                return path.Insert(index, System.IO.Path.AltDirectorySeparatorChar.ToString());
            }

            return path;
        }

        /// <summary>
        /// This method checks if a given file name is valid.<br/>
        /// A valid file name is not null, not empty, and does not contain any invalid characters.<br/>
        /// <br/>
        /// <example>
        /// Given a file name when checking if it is valid then return true or false.<br/>
        /// <code>
        /// bool result1 = Path.IsValideFileName(null); // result1 = false
        /// bool result2 = "".IsValideFileName(); // result2 = false
        /// bool result3 = "Value".IsValideFileName(); // result3 = true
        /// </code> 
        /// </example>
        /// </summary>
        /// <param name="fileName">The file name to check.</param>
        /// <returns>True if the file name is valid, otherwise false.</returns>
        public static bool IsValideFileName(this string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) { return false; }

            foreach (char c in invalidFileNameChars)
            {
                if (fileName.Contains(c)) { return false; }
            }
            return true;
        }

        /// <summary>
        /// This method checks if a given path is valid.<br/>
        /// A valid path is not null, not empty, and does not contain any invalid characters unless allowed.<br/>
        /// <br/>
        /// <example>
        /// Given a path when checking if it is valid then return true or false.<br/>
        /// <code>
        /// bool result1 = Path.IsValidePath(null); // result1 = false
        /// bool result2 = Path.IsValidePath(null, true); // result2 = true
        /// bool result3 = "".IsValidePath(); // result3 = false
        /// bool result4 = "".IsValidePath(true); // result4 = true
        /// bool result5 = "Value".IsValidePath(); // result5 = true
        /// </code> 
        /// </example>
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <param name="allowNullAndEmpty">If true, allows null and empty paths.</param>
        /// <returns>True if the path is valid, otherwise false.</returns>
        public static bool IsValidePath(this string path, bool allowNullAndEmpty = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                if (allowNullAndEmpty) { return true; }
                else { return false; }
            }

            foreach (char c in invalidPathChars)
            {
                if (path.Contains(c)) { return false; }
            }
            return true;
        }
    }
}