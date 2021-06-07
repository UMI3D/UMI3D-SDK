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

using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{

    [CreateAssetMenu(fileName = "NewAssetLibrary", menuName = "UMI3D/Asset Library")]
    public class AssetLibrary : ScriptableObject, UMI3DLoadableEntity
    {
        public string id = "com.compagny.application";

        ulong eId = 0;
        [SerializeField]
        public SerializableDateTime date;
        [SerializeField]
        public List<UMI3DLocalAssetDirectory> variants = new List<UMI3DLocalAssetDirectory>();

        public AssetLibraryDto ToDto()
        {
            AssetLibraryDto dto = new AssetLibraryDto();
            dto.libraryId = id;
            dto.id = Id();
            dto.format = date.Format();
            dto.culture = date.Culture();
            dto.date = date.ToString();
            dto.variants = new List<UMI3DLocalAssetDirectory>();
            foreach (var variant in variants)
            {
                dto.variants.Add(new UMI3DLocalAssetDirectory(variant));
            }
            dto.baseUrl = UMI3DServer.GetHttpUrl() + UMI3DNetworkingKeys.directory;
            return dto;
        }

        public Bytable ToBytes(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(id)
                + UMI3DNetworkingHelper.Write(Id())
                + UMI3DNetworkingHelper.Write(date.Format())
                + UMI3DNetworkingHelper.Write(date.Culture())
                + UMI3DNetworkingHelper.Write(date.ToString())
                + UMI3DNetworkingHelper.ToBytes(variants.Select(v => new UMI3DLocalAssetDirectory(v)));
        }

        public ulong Id()
        {
            if (eId == 0)
                eId = umi3d.edk.UMI3DEnvironment.Register(this);
            return eId;
        }

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
                entity = this,
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntitiesWhere<UMI3DUser>(u => u.hasJoined))
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
                users = new HashSet<UMI3DUser>(users ?? UMI3DEnvironment.GetEntities<UMI3DUser>())
            };
            return operation;
        }

        #region filter
        HashSet<UMI3DUserFilter> ConnectionFilters = new HashSet<UMI3DUserFilter>();

        public bool LoadOnConnection(UMI3DUser user)
        {
            return ConnectionFilters.Count == 0 || !ConnectionFilters.Any(f => !f.Accept(user));
        }

        public bool AddConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Add(filter);
        }

        public bool RemoveConnectionFilter(UMI3DUserFilter filter)
        {
            return ConnectionFilters.Remove(filter);
        }
        #endregion

    }
}