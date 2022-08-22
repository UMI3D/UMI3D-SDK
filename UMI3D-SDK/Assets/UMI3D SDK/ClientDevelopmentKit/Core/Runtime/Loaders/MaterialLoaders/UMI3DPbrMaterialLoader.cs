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
using MrtkShader;
using System;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class UMI3DPbrMaterialLoader : AbstractUMI3DMaterialLoader
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading | DebugScope.Material;

        /// <inheritdoc/>
        public override bool IsSuitableFor(GlTFMaterialDto gltfMatDto)
        {
            if (gltfMatDto.extensions.umi3d is UMI3DMaterialDto)
                return true;
            return false;
        }

        /// <inheritdoc/>
        public override void LoadMaterialFromExtension(GlTFMaterialDto dto, Action<Material> callback, Material oldMaterial = null)
        {
            var ext = dto.extensions.umi3d as UMI3DMaterialDto;
            KHR_texture_transform KhrTT = dto.extensions.KHR_texture_transform;
            if (ext != null)
            {
                void SetMaterial(Material newMat)
                {
                    newMat.color = (Color)dto.pbrMetallicRoughness.baseColorFactor;
                    if (newMat.color.a < 1 || dto.alphaMode == "BLEND")
                    {
                        newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        newMat.SetInt("_ZWrite", 0);
                        newMat.DisableKeyword("_ALPHATEST_ON");
                        newMat.DisableKeyword("_ALPHABLEND_ON");
                        newMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        newMat.renderQueue = 3000;
#if USING_URP
                        newMat.SetFloat("_Surface", 1);
#endif
                    }
                    newMat.ApplyShaderProperty(MRTKShaderUtils.EmissiveColor, (Vector4)(Vector3)dto.emissiveFactor);
                    newMat.ApplyShaderProperty(MRTKShaderUtils.Metallic, dto.pbrMetallicRoughness.metallicFactor);
                    newMat.ApplyShaderProperty(MRTKShaderUtils.Smoothness, 1 - dto.pbrMetallicRoughness.roughnessFactor);

                    LoadTextureInMaterial(ext.id, ext.baseColorTexture, MRTKShaderUtils.MainTex, newMat);
                    LoadTextureInMaterial(ext.id, ext.normalTexture, MRTKShaderUtils.NormalMap, newMat);
                    LoadTextureInMaterial(ext.id, ext.heightTexture, MRTKShaderUtils.BumpMap, newMat);

                    LoadTextureInMaterial(ext.id, ext.emissiveTexture, MRTKShaderUtils.EmissionMap, newMat);

                    // TODO optimise combine chanel map 
                    if (ext.channelTexture != null && ext.channelTexture.variants.Count > 0)
                    {
                        LoadTextureInMaterial(ext.id, ext.channelTexture, MRTKShaderUtils.ChannelMap, newMat);
                    }
                    else if (ext.emissiveTexture != null || ext.occlusionTexture != null || ext.metallicRoughnessTexture != null || ext.metallicTexture != null || ext.roughnessTexture != null)
                    {
#if USING_URP
                        LoadTextureInMaterial(ext.id, ext.occlusionTexture, MRTKShaderUtils.OcclusionMap, newMat);

                        LoadTextureInMaterial(ext.id, ext.metallicTexture, MRTKShaderUtils.MetallicMap, newMat);
                        LoadTextureInMaterial(ext.id, ext.metallicRoughnessTexture, MRTKShaderUtils.MetallicMap, newMat);
#else
                        Texture2D channelMap;
                        if (ext.metallicRoughnessTexture != null)
                        {
                            LoadTextureInMaterial(ext.id, ext.metallicRoughnessTexture, null, newMat, (mrTexture) =>
                            {
                                LoadTextureInMaterial(ext.id, ext.emissiveTexture, null, newMat, (eTexture) =>
                                {
                                    LoadTextureInMaterial(ext.id, ext.occlusionTexture, null, newMat, (oTexture) =>
                                    {
                                        channelMap = TextureCombiner.CombineFromGltfStandard(mrTexture, oTexture, eTexture);
                                        if (channelMap != null)
                                            newMat.ApplyShaderProperty(MRTKShaderUtils.ChannelMap, channelMap);
                                    });
                                });
                            });
                        }
                        else
                        {
                            LoadTextureInMaterial(ext.id, ext.metallicTexture, null, newMat, (mTexture) =>
                            {
                                LoadTextureInMaterial(ext.id, ext.emissiveTexture, null, newMat, (eTexture) =>
                                {
                                    LoadTextureInMaterial(ext.id, ext.occlusionTexture, null, newMat, (oTexture) =>
                                    {
                                        LoadTextureInMaterial(ext.id, ext.roughnessTexture, null, newMat, (rTexture) =>
                                        {
                                            channelMap = TextureCombiner.CombineFromGltfStandard(mTexture, oTexture, eTexture, rTexture);
                                            if (channelMap != null)
                                                newMat.ApplyShaderProperty(MRTKShaderUtils.ChannelMap, channelMap);
                                        });
                                    });
                                });
                            });
                        }
#endif
                    }

                    newMat.ApplyShaderProperty(MRTKShaderUtils.NormalMapScale, ext.normalTexture.scale);

                    ReadAdditionalShaderProperties(ext.id, ext.shaderProperties, newMat);
                    ApplyTiling(KhrTT.offset, KhrTT.scale, newMat);

                    callback.Invoke(newMat);
                }

                if (oldMaterial != null)
                    SetMaterial(oldMaterial);
                else
                    MainThreadDispatcher.UnityMainThreadDispatcher.Instance().StartCoroutine(
                            UMI3DEnvironmentLoader.Instance.GetBaseMaterialBeforeAction(SetMaterial)
                        );
            }
            else
            {
                UMI3DLogger.LogWarning("extension is null", scope);
            }
        }
    }
}

