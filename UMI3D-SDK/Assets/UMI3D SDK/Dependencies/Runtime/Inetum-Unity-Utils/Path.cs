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

namespace inetum.unityUtils
{
    public class Path
    {
        public static readonly char[] charsToTrim = { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };

        /// <summary>
        /// Combine <paramref name="path1"/> with <paramref name="path2"/> to form a valide path.
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static string Combine(string path1, string path2)
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
                return path1.Trim().TrimEnd(charsToTrim)
                    + System.IO.Path.AltDirectorySeparatorChar
                    + path2.Trim().TrimStart(charsToTrim);
            }
        }

        /// <summary>
        /// Combine the paths to form a valide path.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
            {
                return null;
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