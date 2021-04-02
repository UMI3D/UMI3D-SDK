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

    public class UMI3DSceneLoader : UMI3DAbstractNodeLoader
    {
        UMI3DEnvironmentLoader EnvironementLoader;

        public UMI3DSceneLoader(UMI3DEnvironmentLoader EnvironementLoader)
        {
            this.EnvironementLoader = EnvironementLoader;
        }

        /// <summary>
        /// Create a GLTFScene based on a GLTFSceneDto
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="finished"></param>
        public void LoadGlTFScene(GlTFSceneDto dto, System.Action finished, System.Action<int> LoadedNodesCount)
        {
            GameObject go = new GameObject(dto.name);
            UMI3DEnvironmentLoader.RegisterNodeInstance(dto.extensions.umi3d.id, dto, go,
                () =>
                    {
                        var sceneDto = dto.extensions.umi3d;
                        foreach (var library in sceneDto.LibrariesId)
                            UMI3DResourcesManager.UnloadLibrary(library, sceneDto.id);
                    });
            go.transform.SetParent(EnvironementLoader.transform);
            //Load Materials
            LoadSceneMaterials(dto, () => { EnvironementLoader.StartCoroutine(EnvironementLoader.nodeLoader.LoadNodes(dto.nodes, finished, LoadedNodesCount)); });
            //Load Nodes
            //     EnvironementLoader.StartCoroutine(EnvironementLoader.nodeLoader.LoadNodes(dto.nodes, finished, LoadedNodesCount));
        }

        /// <summary>
        /// Setup a scene node based on a UMI3DSceneNodeDto
        /// </summary>
        /// <param name="node"></param>
        /// <param name="dto"></param>
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<string> failed)
        {
            base.ReadUMI3DExtension(dto, node, () =>
             {
                 var sceneDto = dto as UMI3DSceneNodeDto;
                 if (sceneDto == null) return;
                 node.transform.localPosition = sceneDto.position;
                 node.transform.localRotation = sceneDto.rotation;
                 node.transform.localScale = sceneDto.scale;
                 foreach (var library in sceneDto.LibrariesId)
                     UMI3DResourcesManager.LoadLibrary(library, null, sceneDto.id);
                 int count = 0;
                 if (sceneDto.otherEntities != null)
                 {
                     foreach (var entity in sceneDto.otherEntities)
                     {
                         count++;
                         UMI3DEnvironmentLoader.LoadEntity(entity, () => { count--; if (count == 0) finished.Invoke(); });
                     }
                 }

                 if (count == 0)
                     finished.Invoke();
             }, failed);
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var node = entity as UMI3DNodeInstance;
            if (node == null)
            {
                return SetUMI3DMaterialProperty(entity, property);
            }
            if (base.SetUMI3DProperty(entity, property))
                return true;
            UMI3DSceneNodeDto dto = (node.dto as GlTFSceneDto)?.extensions?.umi3d as UMI3DSceneNodeDto;
            if (dto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.Position:
                    dto.position = (SerializableVector3)property.value;
                    if (node.updatePose)
                        node.transform.localPosition = dto.position;
                    break;
                case UMI3DPropertyKeys.Rotation:
                    dto.rotation = (SerializableVector4)property.value;
                    if (node.updatePose)
                        node.transform.localRotation = dto.rotation;
                    break;
                case UMI3DPropertyKeys.Scale:
                    dto.scale = (SerializableVector3)property.value;
                    if (node.updatePose)
                        node.transform.localScale = dto.scale;
                    break;
                default:
                    return false;
            }
            return true;
        }


        public void LoadSceneMaterials(GlTFSceneDto dto, Action callback)
        {
            foreach (GlTFMaterialDto material in dto.materials)
            {
                try
                {

                    UMI3DEnvironmentLoader.Parameters.SelectMaterialLoader(material).LoadMaterialFromExtension(material, (m) =>
                    {
                        if (material.name != null && material.name.Length > 0 && m != null)
                            m.name = material.name;
                        //register the material
                        UMI3DEntityInstance entity = UMI3DEnvironmentLoader.RegisterEntityInstance(((AbstractEntityDto)material.extensions.umi3d).id, material, m);
                    }
                    );

                }
                catch
                {
                    Debug.LogError("this material failed to load : " + material.name);
                }
            }
            callback.Invoke();
        }

        private float RoughnessToSmoothness(float f)
        {
            return 1 - f;
        }

        private bool SwitchOnMaterialProperties(UMI3DEntityInstance entity, SetEntityPropertyDto property, Material materialToModify)
        {

            switch (property.property)
            {
                case UMI3DPropertyKeys.RoughnessFactor:
                    //        ((Material)entity.Object).SetFloat("_Roughness", (float)(double)property.value);
                    //      ((Material)entity.Object).SetFloat("_Smoothness", RoughnessToSmoothness((float)(double)property.value)); 
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.Smoothness, RoughnessToSmoothness((float)(double)property.value));

                    ((GlTFMaterialDto)entity.dto).pbrMetallicRoughness.roughnessFactor = (float)(double)property.value;
                    break;

                case UMI3DPropertyKeys.MetallicFactor:
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.Metallic, (float)(double)property.value);
                    ((GlTFMaterialDto)entity.dto).pbrMetallicRoughness.metallicFactor = (float)(double)property.value;
                    break;

                case UMI3DPropertyKeys.BaseColorFactor:
                    materialToModify.color = ((SerializableColor)property.value);
                    ((GlTFMaterialDto)entity.dto).pbrMetallicRoughness.baseColorFactor = (SerializableColor)property.value;
                    break;

                case UMI3DPropertyKeys.EmissiveFactor:
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.EmissiveColor, ((SerializableColor)property.value));
                    ((GlTFMaterialDto)entity.dto).emissiveFactor = (Vector3)(Vector4)(Color)(SerializableColor)property.value;
                    break;

                case UMI3DPropertyKeys.Maintexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial((TextureDto)property.value, MRTKShaderUtils.MainTex, materialToModify);
                    ((UMI3DMaterialDto)((GlTFMaterialDto)entity.dto).extensions.umi3d).baseColorTexture = (TextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.NormalTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial((ScalableTextureDto)property.value, MRTKShaderUtils.NormalMap, materialToModify);
                    ((UMI3DMaterialDto)((GlTFMaterialDto)entity.dto).extensions.umi3d).normalTexture = (ScalableTextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.EmissiveTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial((TextureDto)property.value, MRTKShaderUtils.EmissionMap, materialToModify);
                    ((UMI3DMaterialDto)((GlTFMaterialDto)entity.dto).extensions.umi3d).emissiveTexture = (TextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.RoughnessTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial((TextureDto)property.value, MRTKShaderUtils.RoughnessMap, materialToModify);
                    ((UMI3DMaterialDto)((GlTFMaterialDto)entity.dto).extensions.umi3d).roughnessTexture = (TextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.MetallicTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial((TextureDto)property.value, MRTKShaderUtils.MetallicMap, materialToModify);
                    ((UMI3DMaterialDto)((GlTFMaterialDto)entity.dto).extensions.umi3d).metallicTexture = (TextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.ChannelTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial((TextureDto)property.value, MRTKShaderUtils.ChannelMap, materialToModify);
                    ((UMI3DMaterialDto)((GlTFMaterialDto)entity.dto).extensions.umi3d).channelTexture = (TextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.MetallicRoughnessTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial((TextureDto)property.value, MRTKShaderUtils.MetallicMap, materialToModify);
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial((TextureDto)property.value, MRTKShaderUtils.RoughnessMap, materialToModify);
                    ((UMI3DMaterialDto)((GlTFMaterialDto)entity.dto).extensions.umi3d).metallicRoughnessTexture = (TextureDto)property.value;

                    break;

                case UMI3DPropertyKeys.OcclusionTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial((TextureDto)property.value, MRTKShaderUtils.OcclusionMap, materialToModify);
                    ((UMI3DMaterialDto)((GlTFMaterialDto)entity.dto).extensions.umi3d).occlusionTexture = (TextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.HeightTexture:
                    Debug.LogWarning("Height Texture not supported");
                    break;

                case UMI3DPropertyKeys.TextureTilingOffset:
                    Vector2 offset = (SerializableVector2)property.value;
                    foreach (string textureName in materialToModify.GetTexturePropertyNames())
                    {
                        materialToModify.SetTextureOffset(textureName, offset);
                    }
                    ((GlTFMaterialDto)entity.dto).extensions.KHR_texture_transform.offset = offset;
                    break;

                case UMI3DPropertyKeys.TextureTilingScale:
                    var scale = (SerializableVector2)property.value;
                    foreach (string textureName in materialToModify.GetTexturePropertyNames())
                    {
                        materialToModify.SetTextureScale(textureName, scale);
                    }
                    ((GlTFMaterialDto)entity.dto).extensions.KHR_texture_transform.scale = scale;
                    break;

                case UMI3DPropertyKeys.NormalTextureScale:
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.NormalMapScale, (float)(double)property.value);
                    ((UMI3DMaterialDto)((GlTFMaterialDto)entity.dto).extensions.umi3d).normalTexture.scale = (float)(double)property.value;
                    break;

                case UMI3DPropertyKeys.HeightTextureScale:
                    //Debug.LogWarning("Height Texture not supported");
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial((TextureDto)property.value, MRTKShaderUtils.BumpMap, materialToModify);
                    ((UMI3DMaterialDto)((GlTFMaterialDto)entity.dto).extensions.umi3d).heightTexture = (ScalableTextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.ShaderProperties:
                    Debug.LogWarning("not totaly implemented");
                    var extension = ((GlTFMaterialDto)entity.dto).extensions.umi3d;
                    switch (property)
                    {
                        case SetEntityDictionaryAddPropertyDto p:
                            //  string key = (string)p.key;
                            if (extension.shaderProperties.ContainsKey((string)p.key))
                            {
                                extension.shaderProperties[(string)p.key] = p.value;
                                Debug.LogWarning("this key (" + p.key.ToString() + ") already exists. Update old value");
                            }
                            else
                                extension.shaderProperties.Add((string)p.key, p.value);
                            break;
                        case SetEntityDictionaryRemovePropertyDto p:
                            extension.shaderProperties.Remove((string)p.key);
                            Debug.LogWarning("Warning a property is removed but it cannot be applied");
                            break;
                        case SetEntityDictionaryPropertyDto p:
                            extension.shaderProperties[(string)p.key] = p.value;
                            break;
                        case SetEntityPropertyDto p:
                            extension.shaderProperties = (Dictionary<string, object>)p.value;
                            break;

                        default:
                            break;
                    }
                    if (materialToModify != null)
                        AbstractUMI3DMaterialLoader.ReadAdditionalShaderProperties(extension.shaderProperties, materialToModify);

                    break;

                default:
                    return false;

            }

            return true;
        }

        public bool SetUMI3DMaterialProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (entity != null && entity.Object is Material)
            {
                return SwitchOnMaterialProperties(entity, property, (Material)entity.Object);
            }

            if (entity != null && entity.Object is List<Material>)
            {
                bool res = false;
                foreach (Material item in (List<Material>)entity.Object)
                {

                    if (SwitchOnMaterialProperties(entity, property, item))
                        res = true;
                }
                return res;
            }
            return false;
        }
    }

}