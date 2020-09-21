using GLTFast.Materials;
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class UMI3DPbrMaterialLoader 
    {
        public void LoadMaterialFromExtension(GlTFMaterialDto dto, Action<Material> callback)
        {

            UMI3DMaterialDto ext = (UMI3DMaterialDto)(dto.extensions.umi3d);
            KHR_texture_transform KhrTT = dto.extensions.KHR_texture_transform;
            if (ext is UMI3DMaterialDto)
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

                newMat.SetFloat("_BumpScale", dto.extensions.umi3d.normalTexture.scale);

                Dictionary<string, object> shaderAdditionalProperties = ext.shaderProperties;
                foreach (KeyValuePair<string, object> item in shaderAdditionalProperties)
                {
                    if ((!string.IsNullOrEmpty(item.Key)) && item.Value != null)
                    {
                        //Type type = item.Value.GetType();
                        switch (item.Value)
                        {
                            case float f:
                                newMat.SetFloat(item.Key, f);
                                break;
                            case Vector4 v:
                                newMat.SetVector(item.Key, v);
                                break;
                            case Vector3 v:
                                newMat.SetVector(item.Key, new Vector4(v.x, v.y, v.z));
                                break;
                            case Vector2 v:
                                newMat.SetVector(item.Key, new Vector4(v.x, v.y));
                                break;
                            case Color c:
                                newMat.SetColor(item.Key, c);
                                break;
                            case int i:
                                newMat.SetInt(item.Key, i);
                                break;
                            case TextureDto t:
                                //newMat.SetTexture(item.Key, t);
                                LoadTextureInMaterial(t, item.Key, newMat);
                                //ApplyTiling(KhrTT.offset, KhrTT.scale, newMat);

                                break;
                            default:
                                Debug.LogWarning("unsupported type for shader property");
                                break;
                        }
                    }
                }
                ApplyTiling(KhrTT.offset, KhrTT.scale, newMat);

                //   return newMat;
                callback.Invoke(newMat);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static void ApplyTiling(Vector2 offset, Vector2 scale, Material newMat)
        {
            if (offset.magnitude > 0.0001 && (scale - Vector2.one).magnitude > 0.0001)
            {
                foreach (string textureName in newMat.GetTexturePropertyNames())
                {
                    newMat.SetTextureOffset(textureName, offset);
                    newMat.SetTextureScale(textureName, scale);
                }
            }
            else
            {
                if (offset.magnitude > 0.0001)
                {
                    foreach (string textureName in newMat.GetTexturePropertyNames())
                    {
                        newMat.SetTextureOffset(textureName, offset);
                    }
                }
                if ((scale - Vector2.one).magnitude > 0.0001)
                {
                    foreach (string textureName in newMat.GetTexturePropertyNames())
                    {
                        newMat.SetTextureScale(textureName, scale);
                    }
                }
            }
        }


        public static void LoadTextureInMaterial (TextureDto textureDto, string materialKey, Material mat)
        {
            if (textureDto == null || textureDto.variants == null || textureDto.variants.Count<1) return;

            FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(textureDto.variants);  // Peut etre ameliore

            string url = fileToLoad.url;
            string ext = fileToLoad.extension;
            string authorization = fileToLoad.authorization;
            IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
            if (loader != null)
                UMI3DResourcesManager.LoadFile(
                    fileToLoad.url,
                    fileToLoad,
                    loader.UrlToObject,
                    loader.ObjectFromCache,
                    (o) =>
                    {
                        var tex = (Texture2D)o;
                        if (tex != null)
                        {
                          /*  if (textureDto is ScalableTextureDto)
                            {
                                tex.Resize((int)(tex.width * ((ScalableTextureDto)textureDto).scale), (int)(tex.height * ((ScalableTextureDto)textureDto).scale));
                            }*/
                            try
                            {
                                mat.SetTexture(materialKey, tex);
                            }
                            catch
                            {
                                Debug.LogError("invalid textur key : " + materialKey);
                            }
                        }
                        else
                            Debug.LogWarning($"invalid cast from {o.GetType()} to {typeof(Texture2D)}");
                    },
                    Debug.LogWarning,
                    loader.DeleteObject
                    );
        }
    }

}

