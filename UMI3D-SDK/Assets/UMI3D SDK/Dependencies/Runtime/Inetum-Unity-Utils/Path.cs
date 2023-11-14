﻿/*
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

namespace inetum.unityUtils
{
    public static class Path
    {
        public static readonly char[] charsToTrim =
        {
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar
        };

        /// <summary>
        /// Combine two paths but instead of throwing an exception when one path is null or empty it returns the other.
        /// 
        /// <para>
        /// Throw an ArgumentNullException when the two paths are null or empty.
        /// </para>
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static string Combine(string path1, string path2)
        {
            var path1Trimmed = path1.Trim().TrimEnd(charsToTrim);
            var path2Trimmed = path2.Trim().TrimStart(charsToTrim);

            if (string.IsNullOrEmpty(path1Trimmed) && string.IsNullOrEmpty(path2Trimmed))
            {
                throw new ArgumentNullException($"{nameof(path1)} and {nameof(path2)}", $"Try to combine to empty paths.");
            }
            if (string.IsNullOrEmpty(path1Trimmed))
            {
                return path2;
            }
            else if (string.IsNullOrEmpty(path2Trimmed))
            {
                return path1;
            }
            else
            {
                return System.IO.Path.Combine(path1Trimmed, path2Trimmed);
            }
        }

        /// <summary>
        /// Combine an array of string but instead of throwing an exception when one path is null or empty it keeps combining with the other paths.
        /// 
        /// <para>
        /// Throw an ArgumentNullException when the array is null or empty.
        /// </para>
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
            {
                throw new ArgumentNullException($"{nameof(paths)}", $"Try to combine to empty paths.");
            }

            string result = paths[0];

            for (int i = 1; i < paths.Length; i++)
            {
                result = Combine(result, paths[i]);
            }

            return result;
        }
    }

}