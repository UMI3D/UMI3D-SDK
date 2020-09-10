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
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{

    [CreateAssetMenu(fileName = "NewAssetLibrary", menuName = "UMI3D/Asset Library")]
    public class AssetLibrary : ScriptableObject, UMI3DLoadableEntity
    {
        public string id = "com.compagny.application";
        public SerializableDateTime date;
        [SerializeField]
        public List<UMI3DLocalAssetDirectory> variants = new List<UMI3DLocalAssetDirectory>();

        public AssetLibraryDto ToDto()
        {
            AssetLibraryDto dto = new AssetLibraryDto();
            dto.id = id;
            dto.date = date.ToString();
            dto.variants = new List<UMI3DLocalAssetDirectory>();
            foreach (var variant in variants)
            {
                dto.variants.Add(new UMI3DLocalAssetDirectory(variant));
            }
            dto.baseUrl = UMI3DServer.GetHttpUrl() + UMI3DNetworkingKeys.directory;
            return dto;
        }

        public string Id()
        {
            return id;
        }

        public IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto();
        }
    }
}