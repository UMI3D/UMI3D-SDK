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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public abstract class AbstractUMI3DMaterialLoader
    {
        const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading |DebugScope.Material;

        public abstract void LoadMaterialFromExtension(GlTFMaterialDto dto, Action<Material> callback);

        public abstract bool IsSuitableFor(GlTFMaterialDto gltfMatDto);

        /// <summary>
        /// Apply the tiling (scale and offset) in the material shader propety 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="scale"></param>
        /// <param name="newMat"></param>
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

        /*     public static void LoadTextureInMaterial(TextureDto textureDto, MRTKShaderUtils.ShaderProperty<Texture2D> materialKey, Material mat)
             {
                 LoadTextureInMaterial(textureDto, (MRTKShaderUtils.ShaderProperty<Texture> )materialKey, mat);
             }*/

        /// <summary>
        /// Load a texture from file or from cache and add it in the material
        /// </summary>
        /// <param name="textureDto">The texture dto with variants</param>
        /// <param name="materialKey">The Shader property, it contains the id/name used to change the good texture in the material</param>
        /// <param name="mat">the material to modify</param>
        /// <param name="alternativeCallback">The basic callback is juste apply the new shader property in the shader but you can overide it to do some other action and then apply the property in the shader</param>
        public static void LoadTextureInMaterial(ulong id, TextureDto textureDto, MRTKShaderUtils.ShaderProperty<Texture2D> materialKey, Material mat, Action<Texture2D> alternativeCallback = null)
        {
            if (textureDto == null || textureDto.variants == null || textureDto.variants.Count < 1)
            {
                if (alternativeCallback != null)
                    alternativeCallback.Invoke(null);
                return;
            }
            FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(textureDto.variants);  // Peut etre ameliore

            string url = fileToLoad.url;
            string ext = fileToLoad.extension;
            string authorization = fileToLoad.authorization;
            IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
            if (loader != null)
            {
                UMI3DResourcesManager.LoadFile(
                    id,
                    fileToLoad,
                    loader.UrlToObject,
                    loader.ObjectFromCache,
                    (o) =>
                    {
                        var tex = (Texture2D)o;
                        if (tex != null)
                        {

                            try
                            {
                                if (alternativeCallback == null)
                                    mat.ApplyShaderProperty(materialKey, tex);
                                else
                                    alternativeCallback.Invoke(tex);
                            }
                            catch
                            {
                                UMI3DLogger.LogError("invalid texture key : " + materialKey,scope);
                            }
                        }
                        else
                        {
                            UMI3DLogger.LogWarning($"invalid cast from {o.GetType()} to {typeof(Texture2D)}",scope);
                        }
                    },
                    e=>UMI3DLogger.LogWarning(e,scope),
                    loader.DeleteObject
                    );
            }
        }

        [System.Obsolete("This is an obsolete method, you should use LoadTextureInMaterial(TextureDto textureDto, MRTKShaderUtils.ShaderProperty<Texture2D> materialKey, Material mat)")]
        protected static void LoadTextureInMaterial(ulong id, TextureDto textureDto, string materialKey, Material mat)
        {
            if (textureDto == null || textureDto.variants == null || textureDto.variants.Count < 1) return;

            FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(textureDto.variants);  // Peut etre ameliore

            string url = fileToLoad.url;
            string ext = fileToLoad.extension;
            string authorization = fileToLoad.authorization;
            IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
            if (loader != null)
            {
                UMI3DResourcesManager.LoadFile(
                    id,
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
                                UMI3DLogger.LogError("invalid texture key : " + materialKey,scope);
                            }
                        }
                        else
                        {
                            UMI3DLogger.LogWarning($"invalid cast from {o.GetType()} to {typeof(Texture2D)}",scope);
                        }
                    },
                    e=>UMI3DLogger.LogWarning(e,scope),
                    loader.DeleteObject
                    );
            }
        }


        /// <summary>
        /// Set all properties of shaderAdditionalProperties in the material shader
        /// </summary>
        /// <param name="shaderAdditionalProperties">A dictionary containing in keys the name to identify the shader property and in value the new value to apply</param>
        /// <param name="newMat">The material to modify</param>
        public static void ReadAdditionalShaderProperties(ulong id, Dictionary<string, object> shaderAdditionalProperties, Material newMat)
        {
            if (shaderAdditionalProperties == null)
                return;
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
                        case double f:
                            newMat.SetFloat(item.Key, (float)f);
                            break;
                        case Vector4 v:
                            newMat.SetVector(item.Key, v);
                            break;
                        case SerializableVector4 v:
                            newMat.SetVector(item.Key, v);
                            break;
                        case Vector3 v:
                            newMat.SetVector(item.Key, new Vector4(v.x, v.y, v.z));
                            break;
                        case SerializableVector3 v:
                            newMat.SetVector(item.Key, new Vector4(v.X, v.Y, v.Z));
                            break;
                        case Vector2 v:
                            newMat.SetVector(item.Key, new Vector4(v.x, v.y));
                            break;
                        case SerializableVector2 v:
                            newMat.SetVector(item.Key, new Vector4(v.X, v.Y));
                            break;
                        case Color c:
                            newMat.SetColor(item.Key, c);
                            break;
                        case SerializableColor c:
                            newMat.SetColor(item.Key, c);
                            break;
                        case int i:
                            newMat.SetInt(item.Key, i);
                            break;
                        case Int64 i:
                            newMat.SetInt(item.Key, (int)i);
                            break;
                        case TextureDto t:
                            //newMat.SetTexture(item.Key, t);
                            LoadTextureInMaterial(id, t, item.Key, newMat);
                            //ApplyTiling(KhrTT.offset, KhrTT.scale, newMat);
                            break;
                        case bool b:
                            if (b)
                            {
                                newMat.EnableKeyword(item.Key);
                            }
                            else
                            {
                                newMat.DisableKeyword(item.Key);
                            }
                            break;
                        default:
                            UMI3DLogger.LogWarning("unsupported type for shader property",scope);
                            break;
                    }
                }
            }
        }


    }
}