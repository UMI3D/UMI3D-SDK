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
using GLTFast.Materials;
using System;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class UMI3DPbrMaterialLoader : AbstractUMI3DMaterialLoader
    {
        public override bool IsSuitableFor(GlTFMaterialDto gltfMatDto)
        {
            if (gltfMatDto.extensions.umi3d is UMI3DMaterialDto)
                return true;
            return false;
        }

        public override void LoadMaterialFromExtension(GlTFMaterialDto dto, Action<Material> callback)
        {
            UMI3DMaterialDto ext = dto.extensions.umi3d as UMI3DMaterialDto;
            KHR_texture_transform KhrTT = dto.extensions.KHR_texture_transform;
            if (ext != null)
            {
                /*    Debug.Log("1");
                    Debug.Log("find shader " + (Shader.Find("glTF/PbrMetallicRoughness") != null));
                    Debug.Log("2");*/
                Material newMat = new Material(Shader.Find("glTF/PbrMetallicRoughness"));
                /*unity standard shader
                           newMat.color = (Vector4)( dto.pbrMetallicRoughness.baseColorFactor);
                           LoadTextureInMaterial(ext.baseColorTexture, "_MainTex", newMat);
                           LoadTextureInMaterial(ext.normalTexture, "_BumpMap", newMat);
                           LoadTextureInMaterial(ext.emissiveTexture, "_EmissionMap", newMat);
                           LoadTextureInMaterial(ext.heightTexture, "_ParallaxMap", newMat);

                           */
                newMat.EnableKeyword(StandardShaderHelper.KW_EMISSION);
                newMat.EnableKeyword(StandardShaderHelper.KW_METALLIC_ROUGNESS_MAP);
                StandardShaderHelper.SetAlphaModeBlend(newMat);

                //gltf shader
                newMat.color = (Color)(dto.pbrMetallicRoughness.baseColorFactor);
                newMat.SetColor("_EmissionColor", (Vector4)(Vector3)dto.emissiveFactor);
                newMat.SetFloat("_Metallic", dto.pbrMetallicRoughness.metallicFactor);
                newMat.SetFloat("_Roughness", dto.pbrMetallicRoughness.roughnessFactor);
                LoadTextureInMaterial(ext.baseColorTexture, "_MainTex", newMat);
                LoadTextureInMaterial(ext.normalTexture, "_BumpMap", newMat);
                LoadTextureInMaterial(ext.emissiveTexture, "_EmissionMap", newMat);
                LoadTextureInMaterial(ext.metallicRoughnessTexture, "_MetallicGlossMap", newMat);
                LoadTextureInMaterial(ext.occlusionTexture, "_OcclusionMap", newMat);
                //LoadTextureInMaterial(ext.heightTexture, "_ParallaxMap", newMat);
                //LoadTextureInMaterial(ext.metallicTexture, "_BumpMap", newMat);
                // LoadTextureInMaterial(ext.roughnessTexture, "_BumpMap", newMat);

                newMat.SetFloat("_BumpScale", ((UMI3DMaterialDto)dto.extensions.umi3d).normalTexture.scale);

                ReadAdditionalShaderProperties(ext.shaderProperties, newMat);
                ApplyTiling(KhrTT.offset, KhrTT.scale, newMat);

                //   return newMat;
                callback.Invoke(newMat);
            }
            else
            {
                Debug.LogWarning("extension is null");
            }
        }



    }

}

