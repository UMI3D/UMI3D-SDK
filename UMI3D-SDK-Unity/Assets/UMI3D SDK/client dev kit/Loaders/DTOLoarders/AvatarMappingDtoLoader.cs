using System;
using System.Linq;
using umi3d.common;
using UnityEngine;
using System.Collections.Generic;

namespace umi3d.cdk
{
    public class AvatarMappingDtoLoader : AbstractObjectDTOLoader<AvatarMappingDto>
    {
        public override void LoadDTO(AvatarMappingDto dto, Action<GameObject> callback)
        {
            try
            {
                GameObject obj = new GameObject();
                AvatarMapping um = obj.AddComponent<AvatarMapping>();
                um.SetMapping(dto.userId, dto.bonePairDictionary);
                callback(obj);
                UpdateFromDTO(obj, null, dto);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public override void UpdateFromDTO(GameObject go, AvatarMappingDto olddto, AvatarMappingDto newdto)
        {
            base.UpdateFromDTO(go, olddto, newdto);
            go.GetComponent<AvatarMapping>().SetMapping(newdto.userId, newdto.bonePairDictionary);
        }
    }
}
