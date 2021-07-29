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
using System.Linq;
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

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            var node = entity as UMI3DNodeInstance;
            if (node == null)
            {
                return SetUMI3DMaterialProperty(entity, operationId, propertyKey, container); ;
            }
            if (base.SetUMI3DProperty(entity, operationId, propertyKey, container))
                return true;
            UMI3DSceneNodeDto dto = (node.dto as GlTFSceneDto)?.extensions?.umi3d as UMI3DSceneNodeDto;
            if (dto == null) return false;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.Position:
                    dto.position = UMI3DNetworkingHelper.Read<SerializableVector3>(container); ;
                    if (node.updatePose)
                        node.transform.localPosition = dto.position;
                    break;
                case UMI3DPropertyKeys.Rotation:
                    dto.rotation = UMI3DNetworkingHelper.Read<SerializableVector4>(container); ;
                    if (node.updatePose)
                        node.transform.localRotation = dto.rotation;
                    break;
                case UMI3DPropertyKeys.Scale:
                    dto.scale = UMI3DNetworkingHelper.Read<SerializableVector3>(container); ;
                    if (node.updatePose)
                        node.transform.localScale = dto.scale;
                    break;
                default:
                    return false;
            }
            return true;
        }

        public override bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            if (ReadUMI3DMaterialProperty(ref value, propertyKey, container))
                return true;
            if (base.ReadUMI3DProperty(ref value, propertyKey, container))
                return true;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.Position:
                    value = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                    break;
                case UMI3DPropertyKeys.Rotation:
                    value = UMI3DNetworkingHelper.Read<SerializableVector4>(container);
                    break;
                case UMI3DPropertyKeys.Scale:
                    value = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
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
            var glTFMaterialDto = entity?.dto as GlTFMaterialDto;
            var uMI3DMaterialDto = glTFMaterialDto?.extensions?.umi3d as UMI3DMaterialDto;

            switch (property.property)
            {
                case UMI3DPropertyKeys.RoughnessFactor:
                    //        ((Material)entity.Object).SetFloat("_Roughness", (float)(double)property.value);
                    //      ((Material)entity.Object).SetFloat("_Smoothness", RoughnessToSmoothness((float)(double)property.value)); 
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.Smoothness, RoughnessToSmoothness((float)(double)property.value));

                    glTFMaterialDto.pbrMetallicRoughness.roughnessFactor = (float)(double)property.value;
                    break;

                case UMI3DPropertyKeys.MetallicFactor:
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.Metallic, (float)(double)property.value);
                    glTFMaterialDto.pbrMetallicRoughness.metallicFactor = (float)(double)property.value;
                    break;

                case UMI3DPropertyKeys.BaseColorFactor:
                    materialToModify.color = ((SerializableColor)property.value);
                    glTFMaterialDto.pbrMetallicRoughness.baseColorFactor = (SerializableColor)property.value;
                    break;

                case UMI3DPropertyKeys.EmissiveFactor:
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.EmissiveColor, ((SerializableColor)property.value));
                    glTFMaterialDto.emissiveFactor = (Vector3)(Vector4)(Color)(SerializableColor)property.value;
                    break;

                case UMI3DPropertyKeys.Maintexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, (TextureDto)property.value, MRTKShaderUtils.MainTex, materialToModify);
                    uMI3DMaterialDto.baseColorTexture = (TextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.NormalTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, (ScalableTextureDto)property.value, MRTKShaderUtils.NormalMap, materialToModify);
                    uMI3DMaterialDto.normalTexture = (ScalableTextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.EmissiveTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, (TextureDto)property.value, MRTKShaderUtils.EmissionMap, materialToModify);
                    uMI3DMaterialDto.emissiveTexture = (TextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.RoughnessTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, (TextureDto)property.value, MRTKShaderUtils.RoughnessMap, materialToModify);
                    uMI3DMaterialDto.roughnessTexture = (TextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.MetallicTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, (TextureDto)property.value, MRTKShaderUtils.MetallicMap, materialToModify);
                    uMI3DMaterialDto.metallicTexture = (TextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.ChannelTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, (TextureDto)property.value, MRTKShaderUtils.ChannelMap, materialToModify);
                    uMI3DMaterialDto.channelTexture = (TextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.MetallicRoughnessTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, (TextureDto)property.value, MRTKShaderUtils.MetallicMap, materialToModify);
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, (TextureDto)property.value, MRTKShaderUtils.RoughnessMap, materialToModify);
                    uMI3DMaterialDto.metallicRoughnessTexture = (TextureDto)property.value;

                    break;

                case UMI3DPropertyKeys.OcclusionTexture:
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, (TextureDto)property.value, MRTKShaderUtils.OcclusionMap, materialToModify);
                    uMI3DMaterialDto.occlusionTexture = (TextureDto)property.value;
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
                    glTFMaterialDto.extensions.KHR_texture_transform.offset = offset;
                    break;

                case UMI3DPropertyKeys.TextureTilingScale:
                    var scale = (SerializableVector2)property.value;
                    foreach (string textureName in materialToModify.GetTexturePropertyNames())
                    {
                        materialToModify.SetTextureScale(textureName, scale);
                    }
                    glTFMaterialDto.extensions.KHR_texture_transform.scale = scale;
                    break;

                case UMI3DPropertyKeys.NormalTextureScale:
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.NormalMapScale, (float)(double)property.value);
                    uMI3DMaterialDto.normalTexture.scale = (float)(double)property.value;
                    break;

                case UMI3DPropertyKeys.HeightTextureScale:
                    //Debug.LogWarning("Height Texture not supported");
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, (TextureDto)property.value, MRTKShaderUtils.BumpMap, materialToModify);
                    uMI3DMaterialDto.heightTexture = (ScalableTextureDto)property.value;
                    break;

                case UMI3DPropertyKeys.ShaderProperties:
                    Debug.LogWarning("not totaly implemented");
                    var extension = glTFMaterialDto.extensions.umi3d;
                    switch (property)
                    {
                        case SetEntityDictionaryAddPropertyDto p:
                            //  string key = (string)p.key;
                            if (extension.shaderProperties.ContainsKey((string)p.key))
                            {
                                extension.shaderProperties[(string)p.key] = ((UMI3DShaderPropertyDto)p.value).value;
                                Debug.LogWarning("this key (" + p.key.ToString() + ") already exists. Update old value");
                            }
                            else
                                extension.shaderProperties.Add((string)p.key, ((UMI3DShaderPropertyDto)p.value).value);
                            break;
                        case SetEntityDictionaryRemovePropertyDto p:
                            extension.shaderProperties.Remove((string)p.key);
                            Debug.LogWarning("Warning a property is removed but it cannot be applied");
                            break;
                        case SetEntityDictionaryPropertyDto p:
                            extension.shaderProperties[(string)p.key] = ((UMI3DShaderPropertyDto)p.value).value;
                            break;
                        case SetEntityPropertyDto p:
                            extension.shaderProperties = ((Dictionary<string, UMI3DShaderPropertyDto>)p.value).Select(k => new KeyValuePair<string, object>(k.Key, k.Value.value)).ToDictionary();
                            break;

                        default:
                            break;
                    }
                    if (materialToModify != null)
                        AbstractUMI3DMaterialLoader.ReadAdditionalShaderProperties(uMI3DMaterialDto.id, extension.shaderProperties, materialToModify);

                    break;

                default:
                    return false;

            }

            return true;
        }


        private bool SwitchOnMaterialProperties(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container, Material materialToModify)
        {
            var glTFMaterialDto = entity?.dto as GlTFMaterialDto;
            var uMI3DMaterialDto = glTFMaterialDto?.extensions?.umi3d as UMI3DMaterialDto;

            switch (propertyKey)
            {
                case UMI3DPropertyKeys.RoughnessFactor:
                    //        ((Material)entity.Object).SetFloat("_Roughness", (float)(double)property.value);
                    //      ((Material)entity.Object).SetFloat("_Smoothness", RoughnessToSmoothness((float)(double)property.value)); 
                    var rf = UMI3DNetworkingHelper.Read<float>(container);
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.Smoothness, RoughnessToSmoothness(rf));

                    glTFMaterialDto.pbrMetallicRoughness.roughnessFactor = rf;
                    break;

                case UMI3DPropertyKeys.MetallicFactor:
                    var mf = UMI3DNetworkingHelper.Read<float>(container);
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.Metallic, mf);
                    glTFMaterialDto.pbrMetallicRoughness.metallicFactor = mf;
                    break;

                case UMI3DPropertyKeys.BaseColorFactor:
                    var bc = UMI3DNetworkingHelper.Read<Color>(container);
                    materialToModify.color = bc;
                    glTFMaterialDto.pbrMetallicRoughness.baseColorFactor = bc;
                    break;

                case UMI3DPropertyKeys.EmissiveFactor:
                    var ef = UMI3DNetworkingHelper.Read<Color>(container);
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.EmissiveColor, ef);
                    glTFMaterialDto.emissiveFactor = (Vector3)(Vector4)ef;
                    break;

                case UMI3DPropertyKeys.Maintexture:
                    var mt = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, mt, MRTKShaderUtils.MainTex, materialToModify);
                    uMI3DMaterialDto.baseColorTexture = mt;
                    break;

                case UMI3DPropertyKeys.NormalTexture:
                    var nt = UMI3DNetworkingHelper.Read<ScalableTextureDto>(container);
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, nt, MRTKShaderUtils.NormalMap, materialToModify);
                    uMI3DMaterialDto.normalTexture = nt;
                    break;

                case UMI3DPropertyKeys.EmissiveTexture:
                    var et = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, et, MRTKShaderUtils.EmissionMap, materialToModify);
                    uMI3DMaterialDto.emissiveTexture = et;
                    break;

                case UMI3DPropertyKeys.RoughnessTexture:
                    var rt = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, rt, MRTKShaderUtils.RoughnessMap, materialToModify);
                    uMI3DMaterialDto.roughnessTexture = rt;
                    break;

                case UMI3DPropertyKeys.MetallicTexture:
                    var met = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, met, MRTKShaderUtils.MetallicMap, materialToModify);
                    uMI3DMaterialDto.metallicTexture = met;
                    break;

                case UMI3DPropertyKeys.ChannelTexture:
                    var ct = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, ct, MRTKShaderUtils.ChannelMap, materialToModify);
                    uMI3DMaterialDto.channelTexture = ct;
                    break;

                case UMI3DPropertyKeys.MetallicRoughnessTexture:
                    var mrt = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, mrt, MRTKShaderUtils.MetallicMap, materialToModify);
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, mrt, MRTKShaderUtils.RoughnessMap, materialToModify);
                    uMI3DMaterialDto.metallicRoughnessTexture = mrt;

                    break;

                case UMI3DPropertyKeys.OcclusionTexture:
                    var ot = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, ot, MRTKShaderUtils.OcclusionMap, materialToModify);
                    uMI3DMaterialDto.occlusionTexture = ot;
                    break;

                case UMI3DPropertyKeys.HeightTexture:
                    Debug.LogWarning("Height Texture not supported");
                    break;

                case UMI3DPropertyKeys.TextureTilingOffset:
                    Vector2 offset = UMI3DNetworkingHelper.Read<Vector2>(container);
                    foreach (string textureName in materialToModify.GetTexturePropertyNames())
                    {
                        materialToModify.SetTextureOffset(textureName, offset);
                    }
                    glTFMaterialDto.extensions.KHR_texture_transform.offset = offset;
                    break;

                case UMI3DPropertyKeys.TextureTilingScale:
                    var scale = UMI3DNetworkingHelper.Read<Vector2>(container);
                    foreach (string textureName in materialToModify.GetTexturePropertyNames())
                    {
                        materialToModify.SetTextureScale(textureName, scale);
                    }
                    glTFMaterialDto.extensions.KHR_texture_transform.scale = scale;
                    break;

                case UMI3DPropertyKeys.NormalTextureScale:
                    var nts = UMI3DNetworkingHelper.Read<float>(container);
                    materialToModify.ApplyShaderProperty(MRTKShaderUtils.NormalMapScale, nts);
                    uMI3DMaterialDto.normalTexture.scale = nts;
                    break;

                case UMI3DPropertyKeys.HeightTextureScale:
                    var hts = UMI3DNetworkingHelper.Read<ScalableTextureDto>(container);
                    //Debug.LogWarning("Height Texture not supported");
                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(uMI3DMaterialDto.id, hts, MRTKShaderUtils.BumpMap, materialToModify);
                    uMI3DMaterialDto.heightTexture = hts;
                    break;

                case UMI3DPropertyKeys.ShaderProperties:
                    Debug.LogWarning("not totaly implemented");
                    var extension = glTFMaterialDto.extensions.umi3d;
                    string key;
                    object value;
                    //TODO
                    switch (operationId)
                    {
                        case UMI3DOperationKeys.SetEntityDictionnaryAddProperty:
                            key = UMI3DNetworkingHelper.Read<string>(container);
                            value = UMI3DNetworkingHelper.Read<UMI3DShaderPropertyDto>(container).value;
                            if (extension.shaderProperties.ContainsKey(key))
                            {
                                extension.shaderProperties[key] = value;
                                Debug.LogWarning($"this key [{key}] already exists. Update old value");
                            }
                            else
                                extension.shaderProperties.Add(key, value);
                            break;
                        case UMI3DOperationKeys.SetEntityDictionnaryRemoveProperty:
                            key = UMI3DNetworkingHelper.Read<string>(container);
                            extension.shaderProperties.Remove((string)key);
                            Debug.LogWarning("Warning a property is removed but it cannot be applied");
                            break;
                        case UMI3DOperationKeys.SetEntityDictionnaryProperty:
                            key = UMI3DNetworkingHelper.Read<string>(container);
                            value = UMI3DNetworkingHelper.Read<UMI3DShaderPropertyDto>(container).value;
                            extension.shaderProperties[key] = value;
                            break;
                        case UMI3DOperationKeys.SetEntityProperty:
                            extension.shaderProperties = UMI3DNetworkingHelper.ReadDictionary<string, UMI3DShaderPropertyDto>(container).Select(k=> new KeyValuePair<string,object>(k.Key,k.Value.value)).ToDictionary();
                            break;
                        default:
                            break;
                    }
                    if (materialToModify != null)
                        AbstractUMI3DMaterialLoader.ReadAdditionalShaderProperties(uMI3DMaterialDto.id, extension.shaderProperties, materialToModify);

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

        public bool SetUMI3DMaterialProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (entity != null && entity.Object is Material)
            {
                return SwitchOnMaterialProperties(entity, operationId, propertyKey, container, (Material)entity.Object);
            }

            if (entity != null && entity.Object is List<Material>)
            {
                bool res = false;
                foreach (Material item in (List<Material>)entity.Object)
                {

                    if (SwitchOnMaterialProperties(entity, operationId, propertyKey, container, item))
                        res = true;
                }
                return res;
            }
            return false;
        }

        public bool ReadUMI3DMaterialProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.RoughnessFactor:
                    value = UMI3DNetworkingHelper.Read<float>(container);
                    break;

                case UMI3DPropertyKeys.MetallicFactor:
                    value = UMI3DNetworkingHelper.Read<float>(container);
                    break;

                case UMI3DPropertyKeys.BaseColorFactor:
                    value = UMI3DNetworkingHelper.Read<SerializableColor>(container);
                    break;

                case UMI3DPropertyKeys.EmissiveFactor:
                    value = UMI3DNetworkingHelper.Read<SerializableColor>(container);
                    break;

                case UMI3DPropertyKeys.Maintexture:
                    value = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    break;

                case UMI3DPropertyKeys.NormalTexture:
                    value = UMI3DNetworkingHelper.Read<ScalableTextureDto>(container);
                    break;

                case UMI3DPropertyKeys.EmissiveTexture:
                    value = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    break;

                case UMI3DPropertyKeys.RoughnessTexture:
                    value = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    break;

                case UMI3DPropertyKeys.MetallicTexture:
                    value = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    break;

                case UMI3DPropertyKeys.ChannelTexture:
                    value = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    break;

                case UMI3DPropertyKeys.MetallicRoughnessTexture:
                    value = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    break;

                case UMI3DPropertyKeys.OcclusionTexture:
                    value = UMI3DNetworkingHelper.Read<TextureDto>(container);
                    break;

                case UMI3DPropertyKeys.HeightTexture:
                    Debug.LogWarning("Height Texture not supported");
                    break;

                case UMI3DPropertyKeys.TextureTilingOffset:
                    value = UMI3DNetworkingHelper.Read<SerializableVector2>(container);
                    break;

                case UMI3DPropertyKeys.TextureTilingScale:
                    value = UMI3DNetworkingHelper.Read<SerializableVector2>(container);
                    break;

                case UMI3DPropertyKeys.NormalTextureScale:
                    value = UMI3DNetworkingHelper.Read<float>(container);
                    break;

                case UMI3DPropertyKeys.HeightTextureScale:
                    value = UMI3DNetworkingHelper.Read<ScalableTextureDto>(container);
                    break;

                case UMI3DPropertyKeys.ShaderProperties:
                    Debug.LogWarning("not totaly implemented");
                    break;

                default:
                    return false;

            }

            return true;
        }

    }

}