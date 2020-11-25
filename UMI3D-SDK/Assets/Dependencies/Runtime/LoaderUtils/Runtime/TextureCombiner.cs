using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MrtkShader
{
    public enum Channel
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        Alpha = 3,
        RGBAverage = 4
    }
    public class TextureCombiner : MonoBehaviour
    {
        private const float defaultUniformValue = -0.01f;

        private static float metallicUniform = defaultUniformValue;
        private static float occlusionUniform = defaultUniformValue;
        private static float emissionUniform = defaultUniformValue;
        private static float smoothnessUniform = defaultUniformValue;

        private static Channel gltfMetallicMapChannel = Channel.Blue;
        private static Channel gltfOcclusionMapChannel = Channel.Red;
        private static Channel emissionMapChannel = Channel.RGBAverage;
        private static Channel gltfSmoothnessMapChannel = Channel.Green;

        public static Texture2D CombineFromUnityStandard(Texture2D metallicMap, Texture2D occlusionMap, Texture2D emissionMap, Texture2D smoothnessMap)
        {
            return Combine(metallicMap, occlusionMap, emissionMap, smoothnessMap);
        }
        
        public static Texture2D CombineFromMrtkStandard(Texture2D metallicMap, Texture2D occlusionMap, Texture2D emissionMap, Texture2D smoothnessMap)
        {
            return Combine(metallicMap, occlusionMap, emissionMap, smoothnessMap,Channel.Red,Channel.Green,Channel.Blue,Channel.Alpha);
        }


        public static Texture2D CombineFromGltfStandard(Texture2D metallicMap, Texture2D occlusionMap, Texture2D emissionMap, Texture2D roughnessMap)
        {
            Texture2D smoothnessMap = InverseMap(roughnessMap);
                return Combine(metallicMap, occlusionMap, emissionMap, smoothnessMap,gltfMetallicMapChannel,gltfOcclusionMapChannel,emissionMapChannel,gltfSmoothnessMapChannel);
        }

        public static Texture2D CombineFromGltfStandard(Texture2D metallicRoughnessMap, Texture2D emissionMap)
        {
            Texture2D smoothnessMap = InverseMap(metallicRoughnessMap);
            return Combine(metallicRoughnessMap, metallicRoughnessMap, emissionMap, smoothnessMap, gltfMetallicMapChannel, gltfOcclusionMapChannel, emissionMapChannel, gltfSmoothnessMapChannel);
        }
        public static Texture2D CombineFromGltfStandard(Texture2D metallicRoughnessMap,Texture2D occlusionMap, Texture2D emissionMap)
        {
            Texture2D smoothnessMap = InverseMap(metallicRoughnessMap);
            return Combine(metallicRoughnessMap, occlusionMap, emissionMap, smoothnessMap, gltfMetallicMapChannel, gltfOcclusionMapChannel, emissionMapChannel, gltfSmoothnessMapChannel);
        }

        /// <summary>
        /// probably never used because gltf standard uses Roughness map
        /// </summary>
        /// <returns></returns>
        public static Texture2D CombineFromGltfStandard_Smoothness(Texture2D metallicMap, Texture2D occlusionMap, Texture2D emissionMap, Texture2D smoothnessMap)
        {
            return Combine(metallicMap, occlusionMap, emissionMap, smoothnessMap, gltfMetallicMapChannel, gltfOcclusionMapChannel, emissionMapChannel, gltfSmoothnessMapChannel);
        }
        
        /// <summary>
        /// probably never used because gltf standard uses Roughness map
        /// </summary>
        /// <returns></returns>
        public static Texture2D CombineFromGltfStandard_Smoothness(Texture2D metallicSmoothnessMap, Texture2D emissionMap)
        {
            return Combine(metallicSmoothnessMap, metallicSmoothnessMap, emissionMap, metallicSmoothnessMap, gltfMetallicMapChannel, gltfOcclusionMapChannel, emissionMapChannel, gltfSmoothnessMapChannel);
        }

        private static Texture2D InverseMap(Texture2D map)
        {
            if (map == null)
                return null;
            Texture2D res = new Texture2D(map.width, map.height);
            Color[] colors = map.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white - colors[i];
            }
            res.SetPixels(colors);
            //Destroy(map);
            return res;
        }

        public static Texture2D Combine(Texture2D metallicMap, Texture2D occlusionMap, Texture2D emissionMap, Texture2D smoothnessMap, Channel metallicMapChannel = Channel.Red, Channel occlusionMapChannel = Channel.Green, Channel emissionMapChannel = Channel.RGBAverage, Channel smoothnessMapChannel = Channel.Alpha)
        {
            if (metallicMap == null && occlusionMap == null && emissionMap == null && smoothnessMap == null)
                return null;
            int width;
            int height;
            Texture[] textures = new Texture[] { metallicMap, occlusionMap, emissionMap, smoothnessMap };
            CalculateChannelMapSize(textures, out width, out height);

            Texture2D channelMap = new Texture2D(width, height);
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            // Use the GPU to pack the various texture maps into a single texture.
            Material channelPacker = new Material(Shader.Find("Hidden/ChannelPacker"));
            channelPacker.SetTexture("_MetallicMap", metallicMap);
            channelPacker.SetInt("_MetallicMapChannel", (int)metallicMapChannel);
            channelPacker.SetFloat("_MetallicUniform", metallicUniform);
            channelPacker.SetTexture("_OcclusionMap", occlusionMap);
            channelPacker.SetInt("_OcclusionMapChannel", (int)occlusionMapChannel);
            channelPacker.SetFloat("_OcclusionUniform", occlusionUniform);
            channelPacker.SetTexture("_EmissionMap", emissionMap);
            channelPacker.SetInt("_EmissionMapChannel", (int)emissionMapChannel);
            channelPacker.SetFloat("_EmissionUniform", emissionUniform);
            channelPacker.SetTexture("_SmoothnessMap", smoothnessMap);
            channelPacker.SetInt("_SmoothnessMapChannel", (int)smoothnessMapChannel);
            channelPacker.SetFloat("_SmoothnessUniform", smoothnessUniform);
            Graphics.Blit(null, renderTexture, channelPacker);
            DestroyImmediate(channelPacker);

            // Save the last render texture to a texture.
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            channelMap.ReadPixels(new Rect(0.0f, 0.0f, width, height), 0, 0);
            channelMap.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);

            return channelMap; 
        }


        private static void CalculateChannelMapSize(Texture[] textures, out int width, out int height)
        {
            width = 4;
            height = 4;

            // Find the max extents of all texture maps.
            foreach (Texture texture in textures)
            {
                width = texture != null ? Mathf.Max(texture.width, width) : width;
                height = texture != null ? Mathf.Max(texture.height, height) : height;
            }
        }

    }
}