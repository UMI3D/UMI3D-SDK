﻿/*
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
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MrtkShader

{
    public static class MRTKShaderUtils
    {

        public class ShaderProperty<T>
        {
            public string propertyName = "";
            public string keyword = null;
            public T defaultValue;
            public Action<Material, ShaderProperty<T>, T> action;

            /// <summary>
            /// Create a new ShaderProperty
            /// </summary>
            /// <param name="propertyName">The name used in the shader for this property</param>
            /// <param name="defaultValue"></param>
            /// <param name="keyword">The keyword in the shader (if needed) in order to use the propety </param>
            /// <param name="action">if not null this action is called after active keyword and the action is called instead of applying the property in the shader</param>
            public ShaderProperty(string propertyName, T defaultValue, string keyword = null, Action<Material, ShaderProperty<T>, T> action = null)
            {
                this.propertyName = propertyName;
                this.keyword = keyword;
                this.defaultValue = defaultValue;
                this.action = action;
            }



        }

        /// <summary>
        /// Apply the value in mat, the property to update is identified by the shaderProperty
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mat"></param>
        /// <param name="shaderProperty"></param>
        /// <param name="value"></param>
        public static void ApplyShaderProperty<T>(this Material mat, ShaderProperty<T> shaderProperty, T value)
        {
            if (shaderProperty.keyword != null)
            {
                mat.EnableKeyword(shaderProperty.keyword);
            }
            if (shaderProperty.action != null)
            {
                shaderProperty.action.Invoke(mat, shaderProperty, value);
                return;
            }

            // else apply default action 
            switch (value)
            {
                case Color c:
                    mat.SetColor(shaderProperty.propertyName, c);
                    break;
                case Vector4 v:
                    mat.SetVector(shaderProperty.propertyName, v);
                    break;
                case Vector3 v:
                    mat.SetVector(shaderProperty.propertyName, v);
                    break;
                case float f:
                    mat.SetFloat(shaderProperty.propertyName, f);
                    break;
                case int i:
                    mat.SetInt(shaderProperty.propertyName, i);
                    break;
                case Texture t:
                    mat.SetTexture(shaderProperty.propertyName, t);
                    break;

                default:
                    Debug.LogWarning("unsupported shader property");
                    break;
            }
        }
#if USING_URP
        public static ShaderProperty<Color> Tilling = new ShaderProperty<Color>("_Tilling", new Color(1, 1, 1, 1));
        public static ShaderProperty<Color> Offset = new ShaderProperty<Color>("_Offset", new Color(0, 0, 0, 0));

        public static ShaderProperty<Color> MainColor = new ShaderProperty<Color>("_BaseColor", Color.white, null,
            (m, s, v) =>
            {
                m.color = v;
                //m.SetColor(s.propertyName, v);
                //If color is not applied properly :
                //Check in the shader if the 'MainColor' attribute is set to the main color of the shader in the shader file :
                //[MainColor] _BaseColor

                if (v.a < 1)
                    SetMaterialTransparent(m);
                else
                    SetMaterialOpaque(m);
            });

        public static ShaderProperty<Texture2D> MainTex = new ShaderProperty<Texture2D>("_BaseMap", null, null,
            (m, s, v) =>
            {
                m.DisableKeyword("_DISABLE_ALBEDO_MAP");
                m.SetTexture(s.propertyName, v);
            });

        public static ShaderProperty<Color> EmissiveColor = new ShaderProperty<Color>("_EmissionColor", Color.black, "_EMISSION");
        public static ShaderProperty<Texture2D> ChannelMap = new ShaderProperty<Texture2D>("_ChannelMap", null, "",
            (m, s, v) =>
            {
                m.SetTexture("_ChannelMap", v);
                m.SetFloat("_ChannelMapONOFF", 1);
            });

        public static ShaderProperty<Texture2D> MetallicMap = new ShaderProperty<Texture2D>("_MetallicGlossMap", null, "");

        public static ShaderProperty<Texture2D> RoughnessMap = new ShaderProperty<Texture2D>("_Smoothness", null, null,
            (m, s, v) =>
            {
                m.SetTexture("_SmoothnessTextureChannel", v);
                m.SetFloat("_RoughnesstoSmoothness", 0f);
                if(v is not null)
                    m.SetFloat("_Smoothness", 1f);
            });

        public static ShaderProperty<Texture2D> EmissionMap = new ShaderProperty<Texture2D>("_EmissionMap", null, "_EMISSION");
        public static ShaderProperty<Texture2D> NormalMap = new ShaderProperty<Texture2D>("_BumpMap", null, "_NORMALMAP");
        public static ShaderProperty<Texture2D> OcclusionMap = new ShaderProperty<Texture2D>("_OcclusionMap", null, "", (m, s, v) =>
        {
            m.SetTexture("_OcclusionMap", v);
            m.SetFloat("_OcclusionStrength", 1f);
        });

        /// <summary>
        /// Makes a URP Lit shader opaque.
        /// </summary>
        /// <param name="m"></param>
        private static void SetMaterialOpaque(Material m)
        {
            m.SetFloat("_Surface", 0); // Opaque
            m.SetInt("_SrcBlend", (int)BlendMode.One);
            m.SetInt("_DstBlend", (int)BlendMode.Zero);
            m.SetInt("_ZWrite", 1);
            m.renderQueue = (int)RenderQueue.Geometry;
        }

        /// <summary>
        /// Makes a URP Lit shader transparent.
        /// </summary>
        /// <param name="m"></param>
        private static void SetMaterialTransparent(Material m)
        {
            m.SetFloat("_Surface", 1); // Transparent
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.renderQueue = (int)RenderQueue.Transparent + 2;
        }

#else

    public static ShaderProperty<Color> MainColor = new ShaderProperty<Color>("_Color", Color.white);

        public static ShaderProperty<Texture2D> MainTex = new ShaderProperty<Texture2D>("_MainTex", null, null,
            (m, s, v) =>
            {
                m.DisableKeyword("_DISABLE_ALBEDO_MAP");

                m.SetTexture(s.propertyName, v);
            }

            );

        public static ShaderProperty<Color> EmissiveColor = new ShaderProperty<Color>("_EmissiveColor", Color.black, "_EMISSION");
        public static ShaderProperty<Texture2D> ChannelMap = new ShaderProperty<Texture2D>("_ChannelMap", null, null,
            (m, s, v) =>
            {
                m.EnableKeyword("_CHANNEL_MAP");
               // m.EnableKeyword("_EMISSION");
                m.SetTexture(s.propertyName, v);
            });

    public static ShaderProperty<Texture2D> MetallicMap = new ShaderProperty<Texture2D>("_ChannelMap", null, null,
            (m, s, v) =>
            {
                Texture2D chanelMap = (Texture2D)m.GetTexture(s.propertyName);
                Texture2D combined = TextureCombiner.Combine(v, chanelMap, chanelMap, chanelMap, Channel.Red, Channel.Green, Channel.Blue, Channel.Alpha);

                //m.SetTexture(s.propertyName, v);
                m.ApplyShaderProperty(ChannelMap, combined);

            }
            );
             public static ShaderProperty<Texture2D> RoughnessMap = new ShaderProperty<Texture2D>("_ChannelMap", null, null,
            (m, s, v) =>
            {
                Texture2D chanelMap = (Texture2D)m.GetTexture(s.propertyName);
                Texture2D combined = TextureCombiner.Combine( chanelMap, chanelMap, chanelMap, v, Channel.Red, Channel.Green, Channel.Blue, Channel.Alpha);

                //m.SetTexture(s.propertyName, v);
                m.ApplyShaderProperty(ChannelMap, combined);

            }
            );

        public static ShaderProperty<Texture2D> EmissionMap = new ShaderProperty<Texture2D>("_ChannelMap", null, "_EMISSION",
            (m, s, v) =>
            {
                Texture2D chanelMap = (Texture2D)m.GetTexture(s.propertyName);
                Texture2D combined = TextureCombiner.Combine(chanelMap, chanelMap, v, chanelMap, Channel.Red, Channel.Green, Channel.RGBAverage, Channel.Alpha);

                //m.SetTexture(s.propertyName, v);
                m.ApplyShaderProperty(ChannelMap, combined);

            }
            );

        public static ShaderProperty<Texture2D> NormalMap = new ShaderProperty<Texture2D>("_NormalMap", null, "_NORMAL_MAP");
        public static ShaderProperty<Texture2D> OcclusionMap = new ShaderProperty<Texture2D>("_OcclusionMap", null,null,
            (m, s, v) =>
            {
                Texture2D chanelMap = (Texture2D)m.GetTexture(s.propertyName);
                Texture2D combined = TextureCombiner.Combine(chanelMap, v, chanelMap, chanelMap, Channel.Red, Channel.Green, Channel.RGBAverage, Channel.Alpha) ;

                //m.SetTexture(s.propertyName, v);
                m.ApplyShaderProperty(ChannelMap, combined);

            });
#endif

        public static ShaderProperty<Color> ClippingBorderColor = new ShaderProperty<Color>("_ClippingBorderColor", Color.white);
        public static ShaderProperty<Texture2D> BumpMap = new ShaderProperty<Texture2D>("_BumpMap", null);

        public static ShaderProperty<float> Metallic = new ShaderProperty<float>("_Metallic", 0f);
        public static ShaderProperty<float> Smoothness = new ShaderProperty<float>("_Smoothness", 0.5f);
        public static ShaderProperty<float> NormalMapScale = new ShaderProperty<float>("_NormalMapScale", 1f);
        public static ShaderProperty<float> BumpScale = new ShaderProperty<float>("_BumpScale", 1f, null, (m, s, v) =>
        {
            m.SetFloat("_BumpScale", Mathf.Clamp(v, 0, 1));
        });
    }
}
