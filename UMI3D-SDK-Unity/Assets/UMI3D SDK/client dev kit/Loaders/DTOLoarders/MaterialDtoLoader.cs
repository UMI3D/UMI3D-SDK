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
    public class MaterialDtoLoader : AbstractDTOLoader<MaterialDto, MeshMaterial>
    {

        /// <summary>
        /// LOAD
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="callback"></param>
        public override void LoadDTO(MaterialDto dto, Action<MeshMaterial> callback)
        {
            MeshMaterial meshMaterial = GetComponent<MeshMaterial>();
            if (meshMaterial == null)
                meshMaterial = gameObject.AddComponent<MeshMaterial>();

            //meshMaterial.Unlit = dto.Unlit;
            meshMaterial.ShaderChosen = dto.ShaderChosen;
            meshMaterial.Transparent = dto.Transparent;
            meshMaterial.MainColor = dto.MainColor;
            meshMaterial.EmissiveColor = dto.EmissiveColor;
            meshMaterial.SpecularColor = dto.SpecularColor;
            meshMaterial.Height = dto.Height;
            meshMaterial.Glossiness = dto.Glossiness;
            meshMaterial.NormalMapScale = dto.NormalMapScale;
            meshMaterial.MainMapsTiling = dto.MainMapsTiling;
            meshMaterial.MainMapsOffset = dto.MainMapsOffset;
            meshMaterial.Shininess = dto.Shininess;
            
            meshMaterial.HeightMapResource.Set(dto.HeightMapResource);
            meshMaterial.GlossinessMapResource.Set(dto.MetallicMapResource);
            meshMaterial.TextureResource.Set(dto.TextureResource);
            meshMaterial.NormalMapResource.Set(dto.NormalMapResource);

            callback(meshMaterial);
        }

        /// <summary>
        /// Update Material from DTO
        /// </summary>
        /// <param name="meshMaterial"></param>
        /// <param name="olddto"></param>
        /// <param name="newdto"></param>
        public override void UpdateFromDTO(MeshMaterial meshMaterial, MaterialDto olddto, MaterialDto newdto)
        {
            if (newdto == null)
                return;
        }

    }
}