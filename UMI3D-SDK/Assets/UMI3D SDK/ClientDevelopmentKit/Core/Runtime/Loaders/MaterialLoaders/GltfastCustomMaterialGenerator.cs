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
using UnityEngine;

namespace umi3d.cdk
{
    public class GltfastCustomMaterialGenerator : DefaultMaterialGenerator
    {
        ///<inheritdoc/>
        public override UnityEngine.Material GenerateMaterial(GLTFast.Schema.Material gltfMaterial, ref GLTFast.Schema.Texture[] textures, ref GLTFast.Schema.Image[] schemaImages, ref Dictionary<int, Texture2D>[] imageVariants, string url, int id)
        {
            UnityEngine.Material material;

            if (gltfMaterial.extensions != null && gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness != null)
            {
                material = GetPbrSpecularGlossinessMaterial(gltfMaterial.doubleSided);
            }
            else
            if (gltfMaterial.extensions.KHR_materials_unlit != null)
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
                material.name = Path.GetFileNameWithoutExtension(url) + " " + gltfMaterial.name;
            }


            // SpecularGlossiness not totaly supported
            if (gltfMaterial.extensions != null)
            {
                GLTFast.Schema.PbrSpecularGlossiness specGloss = gltfMaterial.extensions.KHR_materials_pbrSpecularGlossiness;
                if (specGloss != null)
                {
                    material.color = specGloss.diffuseColor.gamma;
                    material.SetVector(StandardShaderHelper.specColorPropId, specGloss.specularColor);
                    material.SetFloat(StandardShaderHelper.glossinessPropId, specGloss.glossinessFactor);

                    TrySetTexture(specGloss.diffuseTexture, material, StandardShaderHelper.mainTexPropId, ref textures, ref schemaImages, ref imageVariants);

                    if (TrySetTexture(specGloss.specularGlossinessTexture, material, StandardShaderHelper.specGlossMapPropId, ref textures, ref schemaImages, ref imageVariants))
                    {
                        material.EnableKeyword(StandardShaderHelper.KW_SPEC_GLOSS_MAP);
                    }
                }
            }

            if (gltfMaterial.pbrMetallicRoughness != null)
            {
                material.color = gltfMaterial.pbrMetallicRoughness.baseColor.gamma;
                material.ApplyShaderProperty(MRTKShaderUtils.Metallic, gltfMaterial.pbrMetallicRoughness.metallicFactor);
                material.ApplyShaderProperty(MRTKShaderUtils.Smoothness, 1 - gltfMaterial.pbrMetallicRoughness.roughnessFactor);

            }

            if (gltfMaterial.emissive != null && gltfMaterial.emissive != Color.black)
            {
                material.ApplyShaderProperty(MRTKShaderUtils.EmissiveColor, gltfMaterial.emissive);
            }

            Texture2D emissiveTexture = TryGetTexture(gltfMaterial.emissiveTexture, ref textures, ref imageVariants);
            Texture2D normalTexture = TryGetTexture(gltfMaterial.normalTexture, ref textures, ref imageVariants);
            Texture2D occlusionTexture = TryGetTexture(gltfMaterial.occlusionTexture, ref textures, ref imageVariants);
            Texture2D baseColorTexture = TryGetTexture(gltfMaterial.pbrMetallicRoughness.baseColorTexture, ref textures, ref imageVariants);
            Texture2D metallicRoughnessTexture = TryGetTexture(gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture, ref textures, ref imageVariants);

            ApplyTexture(material, gltfMaterial.normalTexture, MRTKShaderUtils.NormalMap, normalTexture, ref textures);
            material.ApplyShaderProperty(MRTKShaderUtils.BumpScale, gltfMaterial.normalTexture.scale);

            if (gltfMaterial.pbrMetallicRoughness != null)
            {
                ApplyTexture(material, gltfMaterial.pbrMetallicRoughness.baseColorTexture, MRTKShaderUtils.MainTex, baseColorTexture, ref textures);

                if (emissiveTexture != null || occlusionTexture != null || metallicRoughnessTexture != null)
                {
                    Texture2D chanelMap = TextureCombiner.CombineFromGltfStandard(metallicRoughnessTexture, occlusionTexture, emissiveTexture);
                    ApplyTexture(material, gltfMaterial.pbrMetallicRoughness.metallicRoughnessTexture, MRTKShaderUtils.ChannelMap, chanelMap, ref textures);
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
        protected void ApplyTexture(UnityEngine.Material matToApply, TextureInfo mapInfo, MRTKShaderUtils.ShaderProperty<Texture2D> property, Texture2D newValue, ref GLTFast.Schema.Texture[] textures)
        {
            if (newValue != null)
            {
                matToApply.ApplyShaderProperty(property, newValue);
                int propertyId = matToApply.shader.FindPropertyIndex(property.propertyName);

                if (propertyId > -1 && property == MRTKShaderUtils.MainTex)
                {
                    bool isKtx = false;
                    if (mapInfo.index < textures.Length && mapInfo.index > -1 && textures[mapInfo.index] != null)
                        isKtx = textures[mapInfo.index].isKtx;
                    TrySetTextureTransform(mapInfo, matToApply, propertyId, isKtx);
                }
            }
        }

        ///<inheritdoc/>
        protected override void TrySetTextureTransform(
            GLTFast.Schema.TextureInfo textureInfo,
            UnityEngine.Material material,
            int propertyId,
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
                    Debug.LogError("Multiple UV sets are not supported!");
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
                    material.SetTextureScale(propertyId, scale);
                }
                if (tt.rotation != 0)
                {
                    /* cos = Mathf.Cos(tt.rotation);
                     sin = Mathf.Sin(tt.rotation);
                     material.SetVector(StandardShaderHelper.mainTexRotatePropId, new Vector4(cos, sin, -sin, cos));
                     material.EnableKeyword(StandardShaderHelper.KW_UV_ROTATION);*/
                    Debug.LogWarning("Texture rotation is not supported");
                    offset.x += scale.y * sin;
                }
                offset.y -= scale.y * cos;
                material.SetTextureOffset(propertyId, offset);
            }

            if (flipY)
            {
                offset.y = 1 - offset.y;
                scale.y = -scale.y;
            }
            material.SetTextureOffset(propertyId, offset);
            material.SetTextureScale(propertyId, scale);
        }

        ///<inheritdoc/>
        public override UnityEngine.Material GetPbrMetallicRoughnessMaterial(bool doubleSided = false)
        {
            UnityEngine.Material res = UMI3DEnvironmentLoader.Instance.GetBaseMaterial();
            if (doubleSided)
            {
                // Turn of back-face culling
                res.SetFloat(StandardShaderHelper.cullModePropId, 0);
            }
            return res;
        }

        private Texture2D TryGetTexture(TextureInfo textureInfo,
            //UnityEngine.Material material,
            //MrtkShader.MRTKShaderUtils.ShaderProperty<UnityEngine.Texture> shaderProperty,
            ref GLTFast.Schema.Texture[] textures,
            ref Dictionary<int, Texture2D>[] imageVariants
            )
        {
            if (textureInfo != null && textureInfo.index >= 0)
            {
                int bcTextureIndex = textureInfo.index;
                if (textures != null && textures.Length > bcTextureIndex)
                {
                    GLTFast.Schema.Texture txt = textures[bcTextureIndex];
                    int imageIndex = txt.GetImageIndex();

                    if (imageVariants != null
                        && imageIndex >= 0
                        && imageVariants.Length > imageIndex
                        && imageVariants[imageIndex] != null
                        && imageVariants[imageIndex].TryGetValue(txt.sampler, out Texture2D img)
                        )
                    {
                        //            int propertyId = material.shader.FindPropertyIndex(shaderProperty.propertyName);
                        //             material.SetTexture(propertyId, img);
                        //             var isKtx = txt.isKtx;
                        //            TrySetTextureTransform(textureInfo, material, propertyId, isKtx);
                        return img;
                    }
                    else
                    {
                        Debug.LogErrorFormat("Image #{0} not found", imageIndex);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("Texture #{0} not found", bcTextureIndex);
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