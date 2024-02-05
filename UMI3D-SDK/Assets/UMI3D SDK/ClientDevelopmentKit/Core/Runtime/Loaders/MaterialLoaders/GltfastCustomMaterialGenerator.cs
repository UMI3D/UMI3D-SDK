/*
Copyright 2019 - 2021 Inetum

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
using GLTFast;
using GLTFast.Materials;
using GLTFast.Schema;
using MrtkShader;
using System.Collections.Generic;
using System.IO;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class GltfastCustomMaterialGenerator : BuiltInMaterialGenerator
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading | DebugScope.Material;

        /// <inheritdoc/>
        public override UnityEngine.Material GenerateMaterial(GLTFast.Schema.Material gltfMaterial, IGltfReadable gltf, string url, int id)
        {
            UnityEngine.Material material;

            if (gltfMaterial?.extensions != null && gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness != null)
            {
                material = GetPbrSpecularGlossinessMaterial(gltfMaterial.doubleSided);
            }
            else
            if (gltfMaterial?.extensions?.KHR_materials_unlit != null)
            {
                material = GetUnlitMaterial(gltfMaterial.doubleSided);
            }
            else
            {
                material = GetPbrMetallicRoughnessMaterial(gltfMaterial.doubleSided);
            }

            if (string.IsNullOrEmpty(gltfMaterial.name))
            {
                material.name = Path.GetFileNameWithoutExtension(url) + " " + id.ToString();
            }
            else
            {
                material.name = gltfMaterial.name;
            }


            // SpecularGlossiness not totaly supported
            if (gltfMaterial.extensions != null)
            {
                GLTFast.Schema.PbrSpecularGlossiness specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;
                if (specGloss != null)
                {
                    material.color = specGloss.diffuseColor.gamma;
                    material.SetVector(specColorPropId, specGloss.specularColor);
                    material.SetFloat(glossinessPropId, specGloss.glossinessFactor);

                    TrySetTexture(specGloss.diffuseTexture, material, gltf, mainTexPropId, -1, -1, -1);

                    if (TrySetTexture(specGloss.specularGlossinessTexture, material, gltf, mainTexPropId, -1, -1, -1))
                    {
                        material.EnableKeyword(KW_SPEC_GLOSS_MAP);
                    }
                }
            }

            if (gltfMaterial.pbrMetallicRoughness != null)
            {
                material.ApplyShaderProperty(MRTKShaderUtils.MainColor, gltfMaterial.pbrMetallicRoughness.baseColor.gamma);
                material.ApplyShaderProperty(MRTKShaderUtils.Metallic, gltfMaterial.pbrMetallicRoughness.metallicFactor);
                material.ApplyShaderProperty(MRTKShaderUtils.Smoothness, 1 - gltfMaterial.pbrMetallicRoughness.roughnessFactor);

            }

            if (gltfMaterial.emissive != null && gltfMaterial.emissive != Color.black)
            {
                material.ApplyShaderProperty(MRTKShaderUtils.EmissiveColor, gltfMaterial.emissive);
            }

            Texture2D emissiveTexture = TryGetTexture(gltfMaterial.emissiveTexture, gltf);
            Texture2D normalTexture = TryGetTexture(gltfMaterial.normalTexture, gltf);
            Texture2D occlusionTexture = TryGetTexture(gltfMaterial.occlusionTexture, gltf);
            Texture2D baseColorTexture = TryGetTexture(gltfMaterial.pbrMetallicRoughness.baseColorTexture, gltf);
            Texture2D metallicRoughnessTexture = TryGetTexture(gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture, gltf);

            ApplyTexture(material, gltfMaterial.normalTexture, MRTKShaderUtils.NormalMap, normalTexture, gltf);
            material.ApplyShaderProperty(MRTKShaderUtils.BumpScale, gltfMaterial.normalTexture.scale);

            if (gltfMaterial.pbrMetallicRoughness != null)
            {
                ApplyTexture(material, gltfMaterial.pbrMetallicRoughness.baseColorTexture, MRTKShaderUtils.MainTex, baseColorTexture, gltf);

                if (emissiveTexture != null || occlusionTexture != null || metallicRoughnessTexture != null)
                {
                    Texture2D chanelMap = TextureCombiner.CombineFromGltfStandard(metallicRoughnessTexture, occlusionTexture, emissiveTexture);
                    ApplyTexture(material, gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture, MRTKShaderUtils.ChannelMap, chanelMap, gltf);
                }
            }

            return material;
        }

        /// <summary>
        /// Apply the newValue texture in matToApply with the parameters in mapInfo and property
        /// </summary>
        /// <param name="matToApply"></param>
        /// <param name="mapInfo"></param>
        /// <param name="property"></param>
        /// <param name="newValue"></param>
        /// <param name="textures"></param>
        protected void ApplyTexture(UnityEngine.Material matToApply, TextureInfo mapInfo, MRTKShaderUtils.ShaderProperty<Texture2D> property, Texture2D newValue, IGltfReadable gltf)
        {
            if (newValue != null)
            {
                matToApply.ApplyShaderProperty(property, newValue);
                int propertyId = matToApply.shader.FindPropertyIndex(property.propertyName);

                if (propertyId > -1 && property == MRTKShaderUtils.MainTex)
                {
                    bool isKtx = false;
                    if (mapInfo.index < gltf.textureCount && mapInfo.index > -1 && gltf.GetSourceTexture(mapInfo.index) != null)
                        isKtx = gltf.GetSourceTexture(mapInfo.index).isKtx;
                    TrySetTextureTransform(mapInfo, matToApply, propertyId, -1, -1, -1, isKtx);
                }
            }
        }

        /// <inheritdoc/>
        protected override void TrySetTextureTransform(
            GLTFast.Schema.TextureInfo textureInfo,
            UnityEngine.Material material,
            int texturePropertyId,
            int scaleTransformPropertyId = -1,
            int rotationPropertyId = -1,
            int uvChannelPropertyId = -1,
            bool flipY = false
            )
        {

            Vector2 offset = Vector2.zero;
            Vector2 scale = Vector2.one;

            if (textureInfo.extensions != null && textureInfo.extensions.KHR_texture_transform != null)
            {
                TextureTransform tt = textureInfo.extensions.KHR_texture_transform;
                if (tt.texCoord != 0)
                {
                    UMI3DLogger.LogError("Multiple UV sets are not supported!", scope);
                }

                float cos = 1;
                float sin = 0;

                if (tt.offset != null)
                {
                    offset.x = tt.offset[0];
                    offset.y = 1 - tt.offset[1];
                }
                if (tt.scale != null)
                {
                    scale.x = tt.scale[0];
                    scale.y = tt.scale[1];
                    material.SetTextureScale(texturePropertyId, scale);
                }
                if (tt.rotation != 0)
                {
                    /* cos = Mathf.Cos(tt.rotation);
                     sin = Mathf.Sin(tt.rotation);
                     material.SetVector(StandardShaderHelper.mainTexRotatePropId, new Vector4(cos, sin, -sin, cos));
                     material.EnableKeyword(StandardShaderHelper.KW_UV_ROTATION);*/
                    UMI3DLogger.LogWarning("Texture rotation is not supported", scope);
                    offset.x += scale.y * sin;
                }
                offset.y -= scale.y * cos;
                material.SetTextureOffset(texturePropertyId, offset);
            }

            if (flipY)
            {
                offset.y = 1 - offset.y;
                scale.y = -scale.y;
            }

            if (material.HasProperty(texturePropertyId))
            {
                material.SetTextureOffset(texturePropertyId, offset);
                material.SetTextureScale(texturePropertyId, scale);
            }
            else
                UMI3DLogger.LogWarning("Impossible to applay texture offset and scale because " + material.shader.name + " has no properpy with id : " + texturePropertyId, scope);
        }

        /// <inheritdoc/>
        protected override UnityEngine.Material GetPbrMetallicRoughnessMaterial(bool doubleSided = false)
        {
            UnityEngine.Material res = UMI3DEnvironmentLoader.Instance.GetBaseMaterial();
            if (doubleSided)
            {
                // Turn of back-face culling
                res.SetFloat(cullModePropId, 0);
            }
            return res;
        }

        private Texture2D TryGetTexture(TextureInfo textureInfo,
           //UnityEngine.Material material,
           //MrtkShader.MRTKShaderUtils.ShaderProperty<UnityEngine.Texture> shaderProperty,


           //ref GLTFast.Schema.Texture[] textures,
           //ref Dictionary<int, Texture2D>[] imageVariants
           IGltfReadable gltf
           )
        {
            if (textureInfo != null && textureInfo.index >= 0)
            {
                int bcTextureIndex = textureInfo.index;
                if (gltf.textureCount > bcTextureIndex)
                {
                    //UMI3DLogger.LogError($"Before GetImage()", scope);
                    Texture2D img = gltf.GetImage(bcTextureIndex);
                    //UMI3DLogger.LogError($"2 " + img.name, scope);
                    if (img != null)
                    {
                        //            int propertyId = material.shader.FindPropertyIndex(shaderProperty.propertyName);
                        //             material.SetTexture(propertyId, img);
                        //             var isKtx = txt.isKtx;
                        //            TrySetTextureTransform(textureInfo, material, propertyId, isKtx);
                        return img;
                    }
                    else
                    {
                        UMI3DLogger.LogError($"Image not found", scope);
                    }
                }
                else
                {
                    UMI3DLogger.LogError($"Texture #{bcTextureIndex} not found", scope);
                }

            }
            return null;

        }

        /// <summary>
        /// used to switch between roughness map and smoothness map
        /// </summary>
        /// <param name="map"></param>
        private void InvertMap(Texture2D map)
        {
            if (map == null) return;
            for (int i = 0; i < map.width; i++)
            {
                for (int j = 0; j < map.height; j++)
                {
                    Color pix = map.GetPixel(i, j);
                    map.SetPixel(i, j, new Color(1 - pix.r, 1 - pix.g, 1 - pix.b));
                }
            }
        }
    }
}