/*
Copyright 2019 Gfi Informatique

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

using System.Collections.Generic;

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
        public string Name;

        /// <summary>
        /// Type of Navigation.
        /// </summary>
        public NavigationType NavigationType;

        /// <summary>
        /// Type of Environment
        /// </summary>
        public EnvironmentType EnvironmentType;

        /// <summary>
        /// Resource of the Icon of the media
        /// </summary>
        public ResourceDto Icon;

        /// <summary>
        /// Icon 3D for this media (if any).
        /// </summary>
        public ResourceDto Icon3D;

        /// <summary>
        /// Skybox to use (if any).
        /// </summary>
        public ResourceDto Skybox;

        public List<ResourceDto> requiredResources;

        public string VersionMajor;
        public string VersionMinor;
        public string VersionStatus;
        public string VersionDate;

        /// <summary>
        /// List of extension that could be send by the environement in ResourceDto
        /// </summary>
        public List<string> extensionNeeded;

        /// <summary>
        /// Url of the media
        /// </summary>
        public string Url;

        public UMI3DDto Connection;

        public MediaDto() : base() { }

    }
}
