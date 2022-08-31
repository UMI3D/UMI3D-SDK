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
    /// Data Tranfert Object for UMI3D media
    /// </summary>
    [System.Serializable]
    public class MediaDto : UMI3DDto
    {
        /// <summary>
        /// Name of the media.
        /// </summary>
        public string name;

        /// <summary>
        /// The interaction's icon 2D. 
        /// </summary>
        public ResourceDto icon2D;

        /// <summary>
        /// The interaction's icon 3D. 
        /// </summary>
        public ResourceDto icon3D;

        /// <summary>
        /// Major part of the version of the media.
        /// </summary>
        /// Versions are Major.Minor.Status.Date
        public string versionMajor;
        /// <summary>
        /// Minor part of the version of the media.
        /// </summary>
        /// Versions are Major.Minor.Status.Date
        public string versionMinor;
        /// <summary>
        /// Status part of the version of the media.
        /// </summary>
        /// Versions are Major.Minor.Status.Date
        public string versionStatus;
        /// <summary>
        /// Date part of the version of the media.
        /// </summary>
        /// Versions are Major.Minor.Status.Date
        public string versionDate;

        /// <summary>
        /// Path of the media.
        /// </summary>
        public string url;

        public MediaDto() : base() { }
    }
}
