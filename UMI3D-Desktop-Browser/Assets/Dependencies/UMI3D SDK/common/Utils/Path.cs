using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace umi3d
{
    public class Path
    {

        public static string Combine(string path1, string path2)
        {
            if (path1 == null || path1 == "") return path2;
            else if (path2 == null) return path1;
            else return path1.Trim().TrimEnd(new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar })
               + System.IO.Path.AltDirectorySeparatorChar
               + path2.Trim().TrimStart(new[] { System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar });
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