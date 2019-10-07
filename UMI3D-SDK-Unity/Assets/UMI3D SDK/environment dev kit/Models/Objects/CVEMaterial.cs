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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;


namespace umi3d.edk
{

    [CreateAssetMenu(fileName = "CVEMaterial", menuName = "UMI3D/Material", order = 1)]
    [System.Serializable]
    public class CVEMaterial : ScriptableObject, IHasAsyncProperties
    {

        private static CVEMaterial defaultMaterial;
        public static CVEMaterial DefaultMaterial
        {
            get
            {
                if (!defaultMaterial)
                {
                    defaultMaterial = (CVEMaterial)ScriptableObject.CreateInstance("CVEMaterial");
                    defaultMaterial.name = "Default";
                    defaultMaterial.AlbedoColor = Color.white;
                    defaultMaterial.ShaderChosen = ShaderType.STANDARD;
                    defaultMaterial.Transparent = false;
                }

                return defaultMaterial;
            }
        }

        protected bool inited = false;
        protected bool updated = false;
        protected List<UMI3DUser> updatedFor = new List<UMI3DUser>();

        protected List<IHasAsyncProperties> lstListeners = new List<IHasAsyncProperties>();


        public ShaderType ShaderChosen;
        public bool Transparent = false;
        public Color AlbedoColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        public Color SpecularColor = new Color(0f, 0f, 0f, 0f);
        public Color EmissiveColor = new Color(0f, 0f, 0f, 0f);
        public float Shininess = 0.5f;
        [RangeAttribute(0.005f, 0.08f)]
        public float Height = 0.02f;
        [RangeAttribute(0f, 1f)]
        public float Glossiness = 0.02f;
        public float NormalMapScale = 1;
        public CVEResource AlbedoTextureResource = new CVEResource();
        public CVEResource NormalMapResource = new CVEResource();
        public CVEResource HeightMapResource = new CVEResource();
        public CVEResource MetallicMapResource = new CVEResource();
        public Vector2 MainMapsTiling = new Vector2(1, 1);
        public Vector2 MainMapsOffset = new Vector2(1, 1);


        public UMI3DAsyncProperty<ShaderType> objectMaterialShader;
        public UMI3DAsyncProperty<bool> objectMaterialTransparent;
        public UMI3DAsyncProperty<Color> objectMaterialColor;
        public UMI3DAsyncProperty<Color> objectMaterialSpecularColor;
        public UMI3DAsyncProperty<Color> objectMaterialEmissiveColor;
        public UMI3DAsyncProperty<float> objectMaterialShininess;
        public UMI3DAsyncProperty<float> objectMaterialHeight;
        public UMI3DAsyncProperty<float> objectMaterialMetallic;
        public UMI3DAsyncProperty<float> objectMaterialNormalMapScale;
        public UMI3DAsyncProperty<Vector2> objectMaterialMainMapsTiling;
        public UMI3DAsyncProperty<Vector2> objectMaterialMainMapsOffset;


        public void initDefinition()
        {
            if (!hasBeenInit())
            {
                objectMaterialShader = new UMI3DAsyncProperty<ShaderType>(this, ShaderChosen);
                objectMaterialShader.OnValueChanged += (ShaderType value) => ShaderChosen = value;


                objectMaterialTransparent = new UMI3DAsyncProperty<bool>(this, Transparent);
                objectMaterialTransparent.OnValueChanged += (bool value) => Transparent = value;

                objectMaterialColor = new UMI3DAsyncProperty<Color>(this, AlbedoColor);
                objectMaterialColor.OnValueChanged += (Color value) => AlbedoColor = value;

                objectMaterialEmissiveColor = new UMI3DAsyncProperty<Color>(this, EmissiveColor);
                objectMaterialEmissiveColor.OnValueChanged += (Color value) => EmissiveColor = value;

                objectMaterialSpecularColor = new UMI3DAsyncProperty<Color>(this, SpecularColor);
                objectMaterialSpecularColor.OnValueChanged += (Color value) => SpecularColor = value;

                objectMaterialShininess = new UMI3DAsyncProperty<float>(this, Shininess);
                objectMaterialShininess.OnValueChanged += (float value) => Shininess = value;

                objectMaterialHeight = new UMI3DAsyncProperty<float>(this, Height);
                objectMaterialHeight.OnValueChanged += (float value) => Height = value;

                objectMaterialMetallic = new UMI3DAsyncProperty<float>(this, Glossiness);
                objectMaterialMetallic.OnValueChanged += (float value) => Glossiness = value;

                objectMaterialNormalMapScale = new UMI3DAsyncProperty<float>(this, NormalMapScale);
                objectMaterialNormalMapScale.OnValueChanged += (float value) => NormalMapScale = value;

                objectMaterialMainMapsTiling = new UMI3DAsyncProperty<Vector2>(this, MainMapsTiling);
                objectMaterialMainMapsTiling.OnValueChanged += (Vector2 value) => MainMapsTiling = value;

                objectMaterialMainMapsOffset = new UMI3DAsyncProperty<Vector2>(this, MainMapsOffset);
                objectMaterialMainMapsOffset.OnValueChanged += (Vector2 value) => MainMapsOffset = value;

                AlbedoTextureResource.initDefinition();
                AlbedoTextureResource.addListener(this);

                NormalMapResource.initDefinition();
                NormalMapResource.addListener(this);

                HeightMapResource.initDefinition();
                HeightMapResource.addListener(this);

                MetallicMapResource.initDefinition();
                MetallicMapResource.addListener(this);
            }
        }

        public void SyncMaterialProperties()
        {
            objectMaterialShader.SetValue(ShaderChosen);
            objectMaterialTransparent.SetValue(Transparent);
            objectMaterialColor.SetValue(AlbedoColor);
            objectMaterialEmissiveColor.SetValue(EmissiveColor);
            objectMaterialSpecularColor.SetValue(SpecularColor);
            objectMaterialShininess.SetValue(Shininess);
            objectMaterialHeight.SetValue(Height);
            objectMaterialMetallic.SetValue(Glossiness);
            objectMaterialNormalMapScale.SetValue(NormalMapScale);
            objectMaterialMainMapsTiling.SetValue(MainMapsTiling);
            objectMaterialMainMapsOffset.SetValue(MainMapsOffset);
        }

        public MaterialDto ToDto(UMI3DUser user)
        {
            var dto = new MaterialDto();

            //dto.Unlit = objectMaterialUnlit.GetValue(user);
            dto.ShaderChosen = objectMaterialShader.GetValue(user);
            dto.Transparent = objectMaterialTransparent.GetValue(user);
            dto.MainColor = objectMaterialColor.GetValue(user);
            dto.SpecularColor = objectMaterialSpecularColor.GetValue(user);
            dto.EmissiveColor = objectMaterialEmissiveColor.GetValue(user);
            dto.Shininess = objectMaterialShininess.GetValue(user);
            dto.Height = objectMaterialHeight.GetValue(user);
            dto.Glossiness = objectMaterialMetallic.GetValue(user);
            dto.NormalMapScale = objectMaterialNormalMapScale.GetValue(user);
            dto.TextureResource = AlbedoTextureResource.ToDto(user);
            dto.NormalMapResource = NormalMapResource.ToDto(user);
            dto.HeightMapResource = HeightMapResource.ToDto(user);
            dto.MetallicMapResource = MetallicMapResource.ToDto(user);
            dto.MainMapsTiling = objectMaterialMainMapsTiling.GetValue(user);
            dto.MainMapsOffset = objectMaterialMainMapsOffset.GetValue(user);

            return dto;
        }

        public void OnValidate()
        {
            if (!hasBeenInit())
                initDefinition();

            SyncMaterialProperties();
            AlbedoTextureResource.SyncProperties();
            NormalMapResource.SyncProperties();
            HeightMapResource.SyncProperties();
            MetallicMapResource.SyncProperties();
        }

        public void SyncRenderer(Renderer renderer)
        {
            if (renderer != null)
            {
                Material material = renderer.sharedMaterial;
                if (material != null)
                {
                    SyncRendererMaterial(material);

                    SyncTextureMaterial(material);
                    SyncNormalMapMaterial(material);
                    SyncHeightMapMaterial(material);
                    SyncMetallicMapMaterial(material);
                }
            }
        }

        public void SyncRendererMaterial(Material material)
        {
            if (Transparent && AlbedoColor.a < 1)
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
            if (material.HasProperty("_Color"))
                material.SetColor("_Color", AlbedoColor);
            if (material.HasProperty("_SpecColor"))
                material.SetColor("_SpecColor", SpecularColor);
            if (material.HasProperty("_Emission"))
                material.SetColor("_Emission", EmissiveColor);
            if (material.HasProperty("_Shininess"))
                material.SetFloat("_Shininess", Shininess);
            if (material.HasProperty("_Parallax"))
                material.SetFloat("_Parallax", Height);
            if (material.HasProperty("_GlossMapScale"))
                material.SetFloat("_GlossMapScale", Glossiness);
            if (material.HasProperty("_BumpScale"))
                material.SetFloat("_BumpScale", NormalMapScale);



        }

        public void SyncTextureMaterial(Material material)
        {
            if (material.HasProperty("_MainTex"))
            {
                if (AlbedoTextureResource.GetUrl() != null && AlbedoTextureResource.GetUrl().Length > 0)
                    material.SetTexture("_MainTex", LoadImage(AlbedoTextureResource));
                else
                    material.SetTexture("_MainTex", null);

                if (material.mainTexture != null)
                {
                    material.mainTextureScale = MainMapsTiling;
                    material.mainTextureOffset = MainMapsOffset;
                }
            }
        }

        public void SyncNormalMapMaterial(Material material)
        {
            if ((ShaderChosen == ShaderType.STANDARD) || (ShaderChosen == ShaderType.STANDARD_SPECULAR_SETUP))
            {
                if (material.HasProperty("_BumpMap"))
                {
                    if (NormalMapResource.GetUrl() != null && NormalMapResource.GetUrl().Length > 0)
                        material.SetTexture("_BumpMap", LoadImage(NormalMapResource));
                    else
                        material.SetTexture("_BumpMap", null);
                }
            }
        }


        public void SyncHeightMapMaterial(Material material)
        {
            if ((ShaderChosen == ShaderType.STANDARD) || (ShaderChosen == ShaderType.STANDARD_SPECULAR_SETUP))
            {
                if (material.HasProperty("_ParallaxMap"))
                {
                    if (HeightMapResource.GetUrl() != null && HeightMapResource.GetUrl().Length > 0)
                        material.SetTexture("_ParallaxMap", LoadImage(HeightMapResource));
                    else
                        material.SetTexture("_ParallaxMap", null);
                }
            }
        }


        public void SyncMetallicMapMaterial(Material material)
        {
            if ((ShaderChosen == ShaderType.STANDARD) || (ShaderChosen == ShaderType.STANDARD_SPECULAR_SETUP))
            {
                if (material.HasProperty("_MetallicGlossMap"))
                {
                    if (MetallicMapResource.GetUrl() != null && MetallicMapResource.GetUrl().Length > 0)
                        material.SetTexture("_MetallicGlossMap", LoadImage(MetallicMapResource));
                    else
                        material.SetTexture("_MetallicGlossMap", null);
                }
            }
        }

        public Material GetMaterial()
        {
            Material material;

            switch (ShaderChosen)
            {
                case ShaderType.UNLIT:
                    if (Transparent && AlbedoTextureResource.GetUrl() != null && AlbedoTextureResource.GetUrl().Length > 0)
                        material = new Material(Shader.Find("Unlit/Transparent"));
                    else if (AlbedoTextureResource.GetUrl() != null && AlbedoTextureResource.GetUrl().Length > 0)
                        material = new Material(Shader.Find("Unlit/Texture"));
                    else if(Transparent)
                        material = new Material(Shader.Find("Unlit/Transparent Colored"));
                    else
                        material = new Material(Shader.Find("Unlit/Color"));
                    break;

                case ShaderType.STANDARD:
                    material = new Material(Shader.Find("Standard"));
                    break;

                case ShaderType.STANDARD_SPECULAR_SETUP:
                    material = new Material(Shader.Find("Standard (Specular setup)"));
                    break;

                default:
                    material = new Material(Shader.Find("Standard"));
                    break;
            }

            return material;
        }

        Texture2D LoadImage(CVEResource resource)
        {
            Texture2D tex = null;

            //HDResourceCache.Download(resource, (Texture2D t) => { tex = t; });

            return tex;
        }



        public void addListener(IHasAsyncProperties listener)
        {
            if (!lstListeners.Contains(listener))
                lstListeners.Add(listener);
        }

        public void removeListener(IHasAsyncProperties listener)
        {
            if (lstListeners.Contains(listener))
                lstListeners.Remove(listener);
        }

        public void NotifyUpdate()
        {
            updated = true;
            if (lstListeners != null)
                foreach (var listener in lstListeners)
                {
                    if (listener != null)
                        listener.NotifyUpdate();
                }
        }

        public void NotifyUpdate(UMI3DUser u)
        {
            if (!updatedFor.Contains(u))
                updatedFor.Add(u);

            foreach (var listener in lstListeners)
            {
                listener.NotifyUpdate(u);
            }
        }

        /// <summary>
        /// Return true if the CVEMaterial has been init, false otherwise.
        /// </summary>
        /// <returns></returns>
        private bool hasBeenInit()
        {
            return objectMaterialShader != null;
        }
    }



}