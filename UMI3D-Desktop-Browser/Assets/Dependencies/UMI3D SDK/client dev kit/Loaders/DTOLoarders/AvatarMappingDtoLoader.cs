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
    public class AvatarMappingDtoLoader : AbstractObjectDTOLoader<AvatarMappingDto>
    {
        /// <summary>
        /// Create an AvatarMapping from an AvatarMappingDto and raise a given callback.
        /// </summary>
        /// <param name="dto">AvatarMappingDto to load</param>
        /// <param name="callback">Callback to raise (the argument is the ARTrackerObject GameObject)</param>
        public override void LoadDTO(AvatarMappingDto dto, Action<GameObject> callback)
        {
            try
            {
                GameObject obj = new GameObject();
                AvatarMapping am = obj.AddComponent<AvatarMapping>();
                am.SetMapping(dto.userId, dto.bonePairDictionary);
                callback(obj);
                UpdateFromDTO(obj, null, dto);
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
        /// <param name="olddto">Previous dto describing the AvatarMapping</param>
        /// <param name="newdto">Dto to update the AvatarMapping to</param>
        public override void UpdateFromDTO(GameObject go, AvatarMappingDto olddto, AvatarMappingDto newdto)
        {
            base.UpdateFromDTO(go, olddto, newdto);
            go.GetComponent<AvatarMapping>().SetMapping(newdto.userId, newdto.bonePairDictionary);
        }
    }
}
