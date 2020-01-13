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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using umi3d.common;


namespace umi3d.cdk
{
    /// <summary>
    /// UMI3D Material appliable to 3D meshes.
    /// </summary>
    public class MeshMaterial : MonoBehaviour
    {
        
        /// <summary>
        /// Shader associated to the material.
        /// </summary>
        public ShaderType ShaderChosen;

        /// <summary>
        /// True if the material is transparent, false otherwise.
        /// </summary>
        public bool Transparent;

        /// <summary>
        /// Albedo main color.
        /// </summary>
        public Color MainColor;

        /// <summary>
        /// Emissive color.
        /// </summary>
        public Color EmissiveColor;

        /// <summary>
        /// Specular color.
        /// </summary>
        public Color SpecularColor;


        /// <summary>
        /// Height Map texture.
        /// </summary>
        public Resource HeightMapResource = new Resource();

        /// <summary>
        /// Albedo texture.
        /// </summary>
        public Resource TextureResource = new Resource();

        /// <summary>
        /// Normal Map texture.
        /// </summary>
        public Resource NormalMapResource = new Resource();

        /// <summary>
        /// Metallic Map texture.
        /// </summary>
        public Resource GlossinessMapResource = new Resource();


        /// <summary>
        /// Height coefficient.
        /// </summary>
        /// <see cref="HeightMapResource"/>
        public float Height;

        /// <summary>
        /// Metallic coefficient.
        /// </summary>
        /// <see cref="GlossinessMapResource"/>
        public float Glossiness;

        /// <summary>
        /// Normal map influence.
        /// </summary>
        /// <see cref="NormalMapResource"/>
        public float NormalMapScale;

        public float Shininess;

        /// <summary>
        /// Main textures tiling.
        /// </summary>
        public Vector2 MainMapsTiling;

        /// <summary>
        /// Main textures offset.
        /// </summary>
        public Vector2 MainMapsOffset;


        public Material previousMaterial;

        /// <summary>
        /// Unity material associated with this MeshMaterial.
        /// </summary>
        public Material material;

        /// <summary>
        /// Dto id associated to this MeshMaterial.
        /// </summary>
        public string dtoid = null;

        /// <summary>
        /// Current shader used.
        /// </summary>
        private string currentShader;



        /// <summary>
        /// Get complete shader name.
        /// </summary>
        public string GetNewParticularShaderName()
        {
            bool hasTexture = TextureResource.Url != null && TextureResource.Url.Length > 0;
            bool hasNormalMap = NormalMapResource.Url != null && NormalMapResource.Url.Length > 0;
            bool hasHeightMap = HeightMapResource.Url != null && HeightMapResource.Url.Length > 0;
            bool hasMetallicMap = GlossinessMapResource.Url != null && GlossinessMapResource.Url.Length > 0;

            switch (ShaderChosen)
            {
                case ShaderType.UNLIT:
                    if (Transparent && hasTexture)
                        return "Unlit/Transparent";
                    else if (hasTexture)
                        return "Unlit/Texture";
                    else if(Transparent)
                        return "Unlit/Transparent Colored";
                    else
                        return "Unlit/Color";
                //break;

                case ShaderType.STANDARD:
                    return "Standard";
                //break;

                case ShaderType.STANDARD_SPECULAR_SETUP:
                    return "Standard (Specular setup)";
                //break;

                default:
                    return null;
                    //break;
            }
        }

        /// <summary>
        /// Copy a given meshMaterial.
        /// </summary>
        /// <param name="meshMaterial">MeshMaterial to copy value from</param>
        public void Set(MeshMaterial meshMaterial)
        {
            ShaderChosen = meshMaterial.ShaderChosen;

            Height = meshMaterial.Height;
            Glossiness = meshMaterial.Glossiness;
            NormalMapScale = meshMaterial.NormalMapScale;
            MainMapsTiling = meshMaterial.MainMapsTiling;
            MainMapsOffset = meshMaterial.MainMapsOffset;
            Shininess = meshMaterial.Shininess;
            Transparent = meshMaterial.Transparent;
            MainColor = meshMaterial.MainColor;
            EmissiveColor = meshMaterial.EmissiveColor;
            SpecularColor = meshMaterial.SpecularColor;

            HeightMapResource.Set(meshMaterial.HeightMapResource);
            GlossinessMapResource.Set(meshMaterial.GlossinessMapResource);
            TextureResource.Set(meshMaterial.TextureResource);
            NormalMapResource.Set(meshMaterial.NormalMapResource);

        }

        /// <summary>
        /// Translate this meshmaterial into a Unity Material.
        /// </summary>
        /// <param name="renderer"></param>
        /// <returns></returns>
        [ContextMenu("Load Material")]
        public Material LoadMaterial()
        {
            string shader = GetNewParticularShaderName();


            if ((currentShader != shader) || (material == null))
            {
                if (shader == null)
                    material = new Material(UMI3DBrowser.Scene.defaultMeshShader);
                else
                    material = new Material(Shader.Find(shader));
            }

            currentShader = shader;

            #region Keywords

            if (Transparent)
            {
                material.SetFloat("_Mode", 2.0f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
            else
            {
                material.SetFloat("_Mode", 0);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
            }

            #endregion

            LoadMap(material);
            LoadNormalMap(material);
            LoadHeightMap(material);
            LoadMetallicMap(material);

            #region Properties

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", MainColor);
            if (material.HasProperty("_SpecColor"))
                material.SetColor("_SpecColor", SpecularColor);
            if (material.HasProperty("_Emission"))
                material.SetColor("_Emission", EmissiveColor);
            if (material.HasProperty("_Shininess"))
                material.SetFloat("_Shininess", Shininess);
            if (material.HasProperty("_Parallax"))
                material.SetFloat("_Parallax", Height);
            if (material.HasProperty("_BumpScale"))
                material.SetFloat("_BumpScale", NormalMapScale);
            if (material.HasProperty("_Metallic"))
                material.SetFloat("_Metallic", Shininess);
            if (material.HasProperty("_MetallicGlossMap"))
                material.SetFloat("_MetallicGlossMap", Shininess);
            if (material.HasProperty("_Glossiness"))
                material.SetFloat("_Glossiness", Glossiness);
            if (material.HasProperty("_GlossMapScale"))
                material.SetFloat("_GlossMapScale", Glossiness);

            #endregion

            return material;
        }





        /// <summary>
        /// Load texture from ressource object into a given material and a given renderer.
        /// </summary>
        /// <param name="resource">Ressource to load from</param>
        /// <param name="prop">Texture name</param>
        /// <param name="material">Material to load texture to</param>
        /// <param name="renderer">Renderer to load material texture to</param>
        public void LoadTexture(Resource resource, string prop, Material material)
        {
            HDResourceCache.Download(resource, (Texture2D text) =>
            {
                material.SetTexture(prop, text);
            });
        }

        /// <summary>
        /// Load albedo texture into a given material and a given renderer.
        /// </summary>
        /// <param name="material">Material to load albedo texture to</param>
        /// <param name="renderer">Renderer to load material albedo texture to</param>
        public void LoadMap(Material material)
        {
            var hasTexture = TextureResource.Url != null && TextureResource.Url.Length > 0;
            if (material.HasProperty("_MainTex"))
            {

                material.mainTextureScale = MainMapsTiling;
                material.mainTextureOffset = MainMapsOffset;

                if (hasTexture)
                    LoadTexture(TextureResource, "_MainTex", material);
                else
                {
                    material.SetTexture("_MainTex", null);
                }
            }
        }

        /// <summary>
        /// Load normal map texture into a given material and a given renderer.
        /// </summary>
        /// <param name="material">Material to load normal map texture to</param>
        /// <param name="renderer">Renderer to load material normal map texture to</param>
        public void LoadNormalMap(Material material)
        {
            var hasNormalMap = NormalMapResource.Url != null && NormalMapResource.Url.Length > 0;
            if (material.HasProperty("_BumpMap"))
            {
                if (hasNormalMap)
                {
                    material.EnableKeyword("_NORMALMAP");
                    LoadTexture(NormalMapResource, "_BumpMap", material);
                }
                else
                {
                    material.DisableKeyword("_NORMALMAP");
                    material.SetTexture("_BumpMap", null);
                }
            }
        }

        /// <summary>
        /// Load height map texture into a given material and a given renderer.
        /// </summary>
        /// <param name="material">Material to load height map texture to</param>
        /// <param name="renderer">Renderer to load material height map texture to</param>
        public void LoadHeightMap(Material material)
        {
            var hasHeightMap = HeightMapResource.Url != null && HeightMapResource.Url.Length > 0;
            if (material.HasProperty("_ParallaxMap"))
            {
                if (hasHeightMap)
                {
                    material.EnableKeyword("_PARALLAXMAP");
                    LoadTexture(HeightMapResource, "_ParallaxMap", material);
                }
                else
                {
                    material.DisableKeyword("_PARALLAXMAP");
                    material.SetTexture("_ParallaxMap", null);
                }
            }
        }

        /// <summary>
        /// Load metallic map texture into a given material and a given renderer.
        /// </summary>
        /// <param name="material">Material to load metallic map texture to</param>
        /// <param name="renderer">Renderer to load material metallic map texture to</param>
        public void LoadMetallicMap(Material material)
        {
            bool hasMetallicMap = GlossinessMapResource.Url != null && GlossinessMapResource.Url.Length > 0;
            if (material.HasProperty("_MetallicGlossMap"))
            {
                if (hasMetallicMap)
                {
                    material.EnableKeyword("_METALLICGLOSSMAP");
                    LoadTexture(GlossinessMapResource, "_MetallicGlossMap", material);
                }
                else
                {
                    material.DisableKeyword("_METALLICGLOSSMAP");
                    material.SetTexture("_MetallicGlossMap", null);
                }
            }
        }
    }
}