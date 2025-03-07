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

using inetum.unityUtils;
using System.Collections.Generic;
using System.IO;
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
        [SerializeField]
        private string id = "com.compagny.application";

        /// <summary>
        /// UMI3D entity ID.
        /// </summary>
        private ulong eId = 0;

        private bool? isNameTooLongForGuidInVersion = null;
        public bool IsNameTooLongForGuidInVersion
        {
            get
            {
                if (!isNameTooLongForGuidInVersion.HasValue)
                    isNameTooLongForGuidInVersion = libraryId.Length > 30;
                
                return isNameTooLongForGuidInVersion.Value;
            }
        }

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


        public string idVersion => libraryId + ":" + version;

        public string libraryId
        {
            get => this.id;
            set
            {
                this.id = value;
                isNameTooLongForGuidInVersion = this.id.Length > 30;
            }
        }

        /// <inheritdoc/>
        public AssetLibraryDto ToDto()
        {
            var dto = new AssetLibraryDto
            {
                libraryId = libraryId,
                id = Id(),
                version = version, 
                variants = new List<UMI3DLocalAssetFilesDto>()
            };
            foreach (UMI3DLocalAssetDirectory variant in variants)
            {
                dto.variants.Add(variant.ToFileDto());
            }
            dto.baseUrl = UMI3DServer.GetResourcesUrl();
            return dto;
        }

        /// <inheritdoc/>
        public Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DSerializer.Write(libraryId)
                + UMI3DSerializer.Write(Id())
                + UMI3DSerializer.Write(version)
                + UMI3DSerializer.WriteCollection(variants.Select(v => v.ToFileDto()));
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

    /// <summary>
    /// Serialized description of an asset directory, a local folder where variants of an assets are stored.
    /// </summary>
    [System.Serializable]
    public class UMI3DLocalAssetDirectory
    {
        /// <summary>
        /// Name of the directory.
        /// </summary>
        public string name = "new variant";

        /// <summary>
        /// Local path of the directory.
        /// </summary>
        public string path;
        [SerializeField]
        public AssetMetric metrics = new AssetMetric();
        [ConstEnum(typeof(UMI3DAssetFormat), typeof(string))]
        public List<string> formats = new List<string>();

        public UMI3DLocalAssetDirectoryDto ToDirectoryDto()
        {
            return new UMI3DLocalAssetDirectoryDto() { 
            name = name,
            path = path,
            metrics = metrics.ToDto(),
            formats = formats,
            };
        }

        public UMI3DLocalAssetFilesDto ToFileDto()
        {
            var directoryPath = inetum.unityUtils.Path.Combine(Application.dataPath, UMI3DServer.dataPath, path);

            UMI3DLocalAssetFilesDto dto = new()
            {
                name = name,
                files = new FileListDto()
                {
                    baseUrl = inetum.unityUtils.Path.Combine("file", path),
                },
                metrics = metrics.ToDto(),
                formats = formats,
            };

            if (Directory.Exists(directoryPath))
                dto.files.files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories).Select(f => f.Replace(directoryPath, "")).ToList();
            else
            {
                dto.files.files = new();
                UMI3DLogger.LogWarning($"{nameof(UMI3DLocalAssetDirectory)}.{nameof(ToFileDto)} : directory doesn't exist {directoryPath} ", DebugScope.EDK);
            }

            return dto;
        }

        public class AssetMetric
        {
            /// <summary>
            /// Arbitrary level of resolution from low to higher resolution.
            /// </summary>
            public int resolution = 1;

            /// <summary>
            /// File size in Mb.
            /// </summary>
            public float size = 0f;

            public AssetMetricDto ToDto()
            {
                return new AssetMetricDto()
                {
                    resolution = resolution,
                    size = size,
                };
            }
        }

    }
}