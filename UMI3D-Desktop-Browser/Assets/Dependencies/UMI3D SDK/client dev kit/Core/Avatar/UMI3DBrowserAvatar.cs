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
using UnityEngine;
using umi3d.common;

namespace umi3d.cdk
{
    public class UMI3DBrowserAvatar : Singleton<UMI3DBrowserAvatar>
    {
        public List<BoneType> BonesToFilter;

        [HideInInspector]
        public AvatarDto avatar;

        /// <summary>
        /// Iterate through the bones of the browser's skeleton to create BoneDto
        /// </summary>
        public void BonesIterator(AvatarDto avatar, Transform viewpoint)
        {
            List<BoneDto> bonesList = new List<BoneDto>();
            foreach (UMI3DBrowserAvatarBone bone in UMI3DBrowserAvatarBone.instances.Values)
            {
                BoneDto dto = bone.ToDto(viewpoint);
                if (dto != null)
                {
                    bonesList.Add(dto);
                }
            }
            avatar.boneList = bonesList;
        }   
        
        public void LoadAvatarMapping(AvatarMappingDto avatarMappingDto)
        {
            AvatarMappingDtoLoader avatarMappingDtoLoader = new AvatarMappingDtoLoader();
            avatarMappingDtoLoader.LoadAvatarMapping(avatarMappingDto);
        }
    }
}
