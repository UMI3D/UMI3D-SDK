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
        private Dictionary<AvatarPartDto, KeyValuePair<Action<GameObject>, Action<string>>> pendingAvatarPartDto = new Dictionary<AvatarPartDto, KeyValuePair<Action<GameObject>, Action<string>>>();
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

        public void AddPendingBone(AvatarPartDto avatarPartDto, Action<GameObject> onLoad, Action<string> onError)
        {
            pendingAvatarPartDto.Add(avatarPartDto, new KeyValuePair<Action<GameObject>, Action<string>>(onLoad, onError));

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
                Dictionary<AvatarPartDto, KeyValuePair<Action<GameObject>, Action<string>>> buffer =
                    new Dictionary<AvatarPartDto, KeyValuePair<Action<GameObject>, Action<string>>>(pendingAvatarPartDto);
                pendingAvatarPartDto.Clear();

                foreach (KeyValuePair<AvatarPartDto, KeyValuePair<Action<GameObject>, Action<string>>> dto in buffer)
                {
                    Debug.Log(dto.Key.name);
                    LoadDto loadDto = new LoadDto();
                    loadDto.Entities.Add(dto.Key);
                    UMI3DBrowser.Scene.Load(loadDto,
                        list => { dto.Value.Key.Invoke(list.GetEnumerator().Current); },
                        (e) => {
                            Debug.Log("fail to load pending part " + e);
                            dto.Value.Value.Invoke(e);
                        });
                }
            }
        }
    }
}
