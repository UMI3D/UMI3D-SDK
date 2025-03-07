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

namespace inetum.unityUtils.versioning
{
    internal class InetumVersioningDataDelegate: IVersioningDataDelegate
    {
        public const string INETUM = "INETUM_";

        /// <summary>
        /// Current version.
        /// </summary>
        public const string Version1_0 = "1_0";
        /// <summary>
        /// Next version.
        /// </summary>
        public const string Version1_1 = "1_1";

        public readonly Version version = new Version(
            majorVersion: 1, 
            minorVersion: 0, 
            buildCountVersion: 0
        );
        public readonly ReleaseCycle releaseCycle = ReleaseCycle.production;

        ReleaseCycle IVersioningDataDelegate.GetReleaseCycle()
        {
            return releaseCycle;
        }

        Version IVersioningDataDelegate.GetVersion()
        {
            return version;
        }
    }
}