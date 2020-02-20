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
using UnityEngine;
using System.Collections.Generic;
using umi3d.common;
using System;

namespace umi3d.cdk
{
    public class AvatarMapping : Singleton<AvatarMapping>
    {
        private List<AvatarPartDto> pendingAvatarPartDto = new List<AvatarPartDto>();
        public string userId;
        public BonePairDictionary bonePairDictionary = new BonePairDictionary();

        /// <summary>
        /// Setup a Mapping
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="pairDictionary"></param>
        public void SetMapping(string id, BonePairDictionary pairDictionary)
        {
            userId = id;
            bonePairDictionary = pairDictionary;
        }

        public void AddPendingBone(AvatarPartDto avatarPartDto)
        {
            pendingAvatarPartDto.Add(avatarPartDto);
        }

        /// <summary>
        /// Update the AvatarMapping from an AvatarMappingDto.
        /// </summary>
        /// <param name="dto">AvatarMappingDto to load</param>
        public void LoadAvatarMapping(AvatarMappingDto avatarMappingDto)
        {
            try
            {
                AvatarMapping avatarMapping = AvatarMapping.Instance;
                avatarMapping.SetMapping(avatarMappingDto.userId, avatarMappingDto.bonePairDictionary);
                avatarMapping.LoadPendingBones();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Load the pending AvatarPartDto list.
        /// </summary>
        public void LoadPendingBones()
        {
            if (pendingAvatarPartDto.Count != 0)
            {
                LoadDto loadDto = new LoadDto();

                foreach (AbstractObject3DDto dto in pendingAvatarPartDto)
                {
                    loadDto.Entities.Add(dto);
                }

                UMI3DBrowser.Scene.Load(loadDto);
                pendingAvatarPartDto.Clear();
            }
        }
    }
}
