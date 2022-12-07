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
    /// DTO for a group of several Asset Libraries as a single entity. See <see cref="AssetLibraryDto"/>.
    /// Assets libraries are package of assets that are loaded by a user at the connection, if they do not already possess it.
    /// </summary>
    /// They are used to reduce the amount of data to transfer when joining a same environment several times. 
    /// The assets are stored locally. A same library could be used by several environments.
    public class LibrariesDto : UMI3DDto, IEntity
    {
        /// <summary>
        /// Libraries are package of assets that are loaded by a user at the connection, if they do not already possess it.
        /// </summary>
        public List<AssetLibraryDto> libraries;
    }
}