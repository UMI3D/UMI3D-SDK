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
            // ajouter une classe abs au dessu 

            UMI3DMaterialDto ext = (UMI3DMaterialDto)(dto.extensions.umi3d);
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

                //gltf shader
                newMat.color = (Vector4)(dto.pbrMetallicRoughness.baseColorFactor);
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
             //   return newMat;
                callback.Invoke(newMat);
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        private void LoadTextureInMaterial (TextureDto textureDto, string materialKey, Material mat)
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
                            if (textureDto is ScalableTextureDto)
                            {
                                tex.Resize((int)(tex.width * ((ScalableTextureDto)textureDto).scale), (int)(tex.height * ((ScalableTextureDto)textureDto).scale));
                            }
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

