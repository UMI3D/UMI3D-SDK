/*
Copyright 2019 - 2025 Inetum

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

namespace inetum.unityUtils
{
    [Obsolete("Kept for backward compatibility : use inetum.unityUtils.lifeCycle.Quitting instead.\n Will be removed in next major version.")]
    public static class QuittingManager
    {
        static QuittingManager()
        {
            Application.wantsToQuit += WantsToQuit;
        }

        /// <summary>
        /// Method called when <see cref="Application.Quit()"/> is called.
        /// </summary>
        /// <returns></returns>
        static bool WantsToQuit()
        {
            applicationIsQuitting = true;
            return true;
        }

        /// <summary>
        /// Whether or not the application has started to quit.
        /// </summary>
        public static bool applicationIsQuitting { get; private set; }
    }

    [Obsolete("Kept for backward compatibility : use inetum.unityUtils.systemIO.Path instead.\n Will be removed in next major version.")]
    public class Path
    {
        public static string Combine(string path1, string path2)
        {
            return inetum.unityUtils.systemIO.Path.Combine(path1, path2);
        }

        public static string Combine(params string[] paths)
        {
            return inetum.unityUtils.systemIO.Path.Combine(paths);
        }
    }
}
