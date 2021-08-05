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

        public static string Combine(string path1, string path2)
        {
            char[] charsToTrim = { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar };
            if (path1 == null || path1 == "") return path2;
            else if (path2 == null) return path1;
            else return path1.Trim().TrimEnd(charsToTrim)
               + System.IO.Path.AltDirectorySeparatorChar
               + path2.Trim().TrimStart(charsToTrim);
        }

        public static string Combine(params string[] paths)
        {
            if (paths == null || paths.Length < 1 || paths[0] == null)
                return null;
            string result = paths[0];
            for (int i = 1; i < paths.Length; i++)
            {
                result = Combine(result, paths[i]);
            }
            return result;
        }
    }

}