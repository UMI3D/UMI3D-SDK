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
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Assets libraries are package of assets that are loaded by a user at the connection, if they do not already possess it.
    /// </summary>
    /// They are used to reduce the amount of data to transfer when joining a same environment several times. 
    /// The assets are stored locally. A same library could be used by several environments.
    [CreateAssetMenu(fileName = "NewAssetLibrary", menuName = "UMI3D/Asset Library")]
    public class AssetLibrary : ScriptableObject, UMI3DLoadableEntity
    {
        /// <summary>
        /// Id of the library. Choose a unique name.
        /// </summary>
        /// Typically "com.compagny.application".
        [Tooltip("Id of the library. Choose a unique name. Typically 'com.compagny.application'.")]
        public string id = "com.compagny.application";

        /// <summary>
        /// UMI3D entity ID.
        /// </summary>
        private ulong eId = 0;

        /// <summary>
        /// Version of the library.
        /// </summary>
        /// Note that it should be updated when the content is updated.
        [SerializeField]
        [Tooltip("version of the library, it should be updated when the content is updated")]
        public string version = "0.0";

        /// <summary>
        /// Directories where a stored all the variants of the library.
        /// </summary>
        /// A library can have several variants to propose better suited sets of assets, aiming at improving the experience on some devices.
        [SerializeField]
        [Tooltip("Variants of the library. " +
                 "A library can have several variants to propose better suited sets of assets, aiming at improving the experience on some devices.")]
        public List<UMI3DLocalAssetDirectory> variants = new List<UMI3DLocalAssetDirectory>();


        public string idVersion => id + ":" + version;

        /// <inheritdoc/>
        public AssetLibraryDto ToDto()
        {
            var dto = new AssetLibraryDto
            {
                libraryId = id,
                id = Id(),
                version = version, 
                variants = new List<UMI3DLocalAssetDirectory>()
            };
            foreach (UMI3DLocalAssetDirectory variant in variants)
            {
                dto.variants.Add(new UMI3DLocalAssetDirectory(variant));
            }
            dto.baseUrl = UMI3DServer.GetHttpUrl() + UMI3DNetworkingKeys.directory;
            return dto;
        }

        /// <inheritdoc/>
        public Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DSerializer.Write(id)
                + UMI3DSerializer.Write(Id())
                + UMI3DSerializer.Write(version)
                + UMI3DSerializer.WriteIBytableCollection(variants.Select(v => new UMI3DLocalAssetDirectory(v)));
        }

        /// <inheritdoc/>
        public ulong Id()
        {
            if (eId == 0)
                eId = umi3d.edk.UMI3DEnvironment.Register(this);
            return eId;
        }

        /// <inheritdoc/>
        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto();
        }

        /// <summary>
        /// Return load operation
        /// </summary>
        /// <returns></returns>
        public virtual LoadEntity GetLoadEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new LoadEntity()
            {
                entities = new List<UMI3DLoadableEntity>() { this },
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSet()
            };
            return operation;
        }

        /// <summary>
        /// Return delete operation
        /// </summary>
        /// <returns></returns>
        public DeleteEntity GetDeleteEntity(HashSet<UMI3DUser> users = null)
        {
            var operation = new DeleteEntity()
            {
                entityId = Id(),
                users = users != null ? new HashSet<UMI3DUser>(users) : UMI3DServer.Instance.UserSet()
            };
            return operation;
        }

        #region filter
        private readonly HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        /// <inheritdoc/>
        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        /// <inheritdoc/>
        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        /// <inheritdoc/>
        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }
        #endregion

    }
}