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
using System;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class AvatarMappingDtoLoader
    {
        /// <summary>
        /// Create an AvatarMapping from an AvatarMappingDto.
        /// </summary>
        /// <param name="dto">AvatarMappingDto to load</param>
        public void LoadAvatarMapping(AvatarMappingDto avatarMappingDto)
        {
            try
            {
                GameObject avatarObj = UMI3DBrowserAvatar.Instance.gameObject;
                AvatarMapping avatarMapping = AvatarMapping.Instance;

                avatarMapping.SetMapping(avatarMappingDto.userId, avatarMappingDto.bonePairDictionary);
                UpdateFromDTO(avatarObj, avatarMappingDto);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Update a AvatarMapping from dto.
        /// </summary>
        /// <param name="go">AvatarMapping gameObject to update</param>
        /// <param name="newdto">Dto to update the AvatarMapping to</param>
        public void UpdateFromDTO(GameObject go, AvatarMappingDto newdto)
        {
             AvatarMapping.Instance.SetMapping(newdto.userId, newdto.bonePairDictionary);
        }
    }
}
