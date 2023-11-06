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

namespace umi3d.common.userCapture.description
{
    [System.Serializable]
    public static class LevelOfArticulation
    {
        /// <summary>
        /// No constraint.
        /// </summary>
        public const uint NONE = 0;

        /// <summary>
        /// Very basic articulations. Only root.
        /// </summary>
        public const uint LOA_0 = 1;

        /// <summary>
        /// Basic articulations.
        /// </summary>
        public const uint LOA_1 = 2;

        /// <summary>
        /// Normal articulations.
        /// </summary>
        public const uint LOA_2 = 3;

        /// <summary>
        /// Detailed articulations.
        /// </summary>
        public const uint LOA_3 = 4;

        /// <summary>
        /// Very detailed articulations.
        /// </summary>
        public const uint LOA_4 = 5;
    }
}