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

namespace inetum.unityUtils.versioning
{
    public interface IVersioningDataDelegate 
    {
        string currentVersionForScriptingSymbol
        {
            get
            {
                Version version = GetVersion();
                return $"{version.majorVersion}_{version.minorVersion}";
            }
        }

        string currentReleaseCycleForScriptingSymbol
        {
            get
            {
                ReleaseCycle releaseCycle = GetReleaseCycle();
                return releaseCycle.scriptingSymbol;
            }
        }

        /// <summary>
        /// Get the current version. 
        /// </summary>
        /// <returns></returns>
        Version GetVersion();

        /// <summary>
        /// Get the current release cycle.
        /// </summary>
        /// <returns></returns>
        ReleaseCycle GetReleaseCycle();
    }
}