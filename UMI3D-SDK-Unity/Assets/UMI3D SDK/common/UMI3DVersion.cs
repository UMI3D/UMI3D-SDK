using UnityEngine;
using System.Collections;

namespace umi3d
{
    static public class UMI3DVersion
    {
        public static string version { get { return major + "." + minor + "." + status + "." + date; } }
        public readonly static string major = "0";
        public readonly static string minor = "9";
        public readonly static string status = "b";
        public readonly static string date = "191018";

    }
}