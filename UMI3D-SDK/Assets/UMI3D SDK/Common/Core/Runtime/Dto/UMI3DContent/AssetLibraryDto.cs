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

using System.Collections.Generic;

namespace umi3d.common
{
    /// <summary>
    /// DTO to describe an Assets library, a package of assets that are loaded by a user at the connection, if they do not already possess it.
    /// </summary>
    /// They are used to reduce the amount of data to transfer when joining a same environment several times. 
    /// The assets are stored locally. A same library could be used by several environments.
    public class AssetLibraryDto : UMI3DDto, IEntity
    {

        /// <summary>
        /// UMI3D id.
        /// </summary>
        public ulong id { get; set; }

        /// <summary>
        /// Id of the library. A unique name.
        /// </summary>
        /// Typically "com.compagny.application".
        public string libraryId { get; set; }

        /// <summary>
        /// Base path of all URLs in the library.
        /// </summary>
        public string baseUrl { get; set; }

        /// <summary>
        /// Version of this library.
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// Directories where a stored all the variants of the library.
        /// </summary>
        /// A library can have several variants to propose better suited sets of assets, aiming at improving the experience on some devices.
        public List<UMI3DLocalAssetDirectoryDto> variants { get; set; }
    }
}