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
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    class CVEAvatar : MonoBehaviour
    {
        public string UserId { private get; set; }

        private BonePairDictionary bonePairDictionary = new BonePairDictionary();

        /// <summary>
        /// Create and return a new AvatarMappingDto. 
        /// </summary>
        public AvatarMappingDto ToDto(UMI3DUser user)
        {
            AvatarMappingDto dto = new AvatarMappingDto();
            setDictionary(user);
            dto.bonePairDictionary = bonePairDictionary;
            dto.userId = UserId;
            return dto;
        }

        /// <summary>
        /// Set the list of Id and BoneType pairs.
        /// </summary>
        private void setDictionary(UMI3DUser user)
        {
            this.bonePairDictionary = user.avatar.setUserMapping();
        }
    }
}
