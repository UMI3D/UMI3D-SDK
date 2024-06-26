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


namespace umi3d.common
{
    /// <summary>
    /// Reserved and fixed UMI3D ids.
    /// </summary>
    /// Those id will always be the same on every environment.
    [System.Serializable]
    public static class UMI3DGlobalID
    {
        /// <summary>
        /// Reserved UMI3D id for the server.
        /// </summary>
        public const ulong ServerId = 100001;

        /// <summary>
        /// Reserved UMI3D id for the environment.
        /// </summary>
        public const ulong EnvironmentId = 100002;
    }
}