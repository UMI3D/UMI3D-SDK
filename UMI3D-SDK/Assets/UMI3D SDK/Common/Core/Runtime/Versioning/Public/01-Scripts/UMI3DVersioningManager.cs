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

using inetum.unityUtils.conditionalCompilation;
using inetum.unityUtils.versioning;
using System.Diagnostics;
using static inetum.unityUtils.versioning.ReleaseCycle;
using static umi3d.Common.Core.Versioning.UMI3DVersioningDataDelegate;
using Action = System.Action;

namespace umi3d.Common.Core.Versioning
{
    public class UMI3DVersioningManager : IVersioningDataDelegate
    {
        #region Initialization

        public static UMI3DVersioningManager @default => _default.Value;
        static readonly System.Lazy<UMI3DVersioningManager> _default = new(() => new UMI3DVersioningManager());

        UMI3DVersioningManager() { }

        #endregion

        #region IVersioningDataDelegate

        IVersioningDataDelegate _dataDelegate = new UMI3DVersioningDataDelegate();
        public IVersioningDataDelegate dataDelegate
        {
            get => _dataDelegate;
            set
            {
                if (value == null)
                {
                    _dataDelegate = new UMI3DVersioningDataDelegate();
                    return;
                }
                _dataDelegate = value;
            }
        }

        public Version GetVersion()
        {
            return dataDelegate.GetVersion();
        }

        public ReleaseCycle GetReleaseCycle()
        {
            return dataDelegate.GetReleaseCycle();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Versioning/Update UMI3D Version")]
        /// <summary>
        /// Update Scripting Symbols.
        /// </summary>
        public static void UpdateVersioning()
        {
            UMI3DVersioningManager manager = @default;

            string version = UMI3D + manager.dataDelegate.currentVersionForScriptingSymbol;
            string releaseCycle = UMI3D + manager.dataDelegate.currentReleaseCycleForScriptingSymbol;

            UnityEngine.Debug.Log($"[UMI3DVersioningManager] Notice: Update version: {version} and release cycle: {releaseCycle}");

            ScriptingSymbolHelper.@default.UpdateSymbols(allCases, version, releaseCycle);
        }
#endif

#endregion

        static string[] allCases = {
            UMI3D + ALPHA, UMI3D + BETA, UMI3D + PROD,
            UMI3D + Version2_9, UMI3D + Version2_10,
        };

        #region Version

        [Conditional(UMI3D + Version2_9)]
        public static void Version_2_9(Action action)
        {
            action?.Invoke();
        }

        [Conditional(UMI3D + Version2_9), Conditional(UMI3D + Version2_10)]
        public static void Version_2_9_OrAbove(Action action)
        {
            action?.Invoke();
        }

        [Conditional(UMI3D + Version2_10)]
        public static void Version_2_10(Action action)
        {
            action?.Invoke();
        }

        [Conditional(UMI3D + Version2_9)]
        public static void VersionBelow_2_10(Action action)
        {
            action?.Invoke();
        }

        #endregion

        #region Release cycle

        [Conditional(UMI3D + ALPHA)]
        public static void Alpha(Action action)
        {
            action?.Invoke();
        }

        [Conditional(UMI3D + BETA)]
        public static void Beta(Action action)
        {
            action?.Invoke();
        }

        [Conditional(UMI3D + PROD)]
        public static void Prod(Action action)
        {
            action?.Invoke();
        }

        #endregion
    }
}