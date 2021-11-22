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
using inetum.unityUtils;
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
        const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        private readonly UMI3DEnvironmentLoader EnvironementLoader;

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
            var go = new GameObject(dto.name);
            UMI3DEnvironmentLoader.RegisterNodeInstance(dto.extensions.umi3d.id, dto, go,
                () =>
                {
                    UMI3DSceneNodeDto sceneDto = dto.extensions.umi3d;
                    foreach (string library in sceneDto.LibrariesId)
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
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<Umi3dException> failed)
        {
            base.ReadUMI3DExtension(dto, node, () =>
            {
                var sceneDto = dto as UMI3DSceneNodeDto;
                if (sceneDto == null) return;
                node.transform.localPosition = sceneDto.position;
                node.transform.localRotation = sceneDto.rotation;
                node.transform.localScale = sceneDto.scale;
                foreach (string library in sceneDto.LibrariesId)
                    UMI3DResourcesManager.LoadLibrary(library, null, sceneDto.id);
                int count = 0;
                if (sceneDto.otherEntities != null)
                {
                    foreach (IEntity entity in sceneDto.otherEntities)
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
            var dto = (node.dto as GlTFSceneDto)?.extensions?.umi3d as UMI3DSceneNodeDto;
            if (dto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.Position:
                    dto.position = (SerializableVector3)property.value;
                    if (node.updatePose)
                    {
                        node.transform.localPosition = dto.position;
                        node.SendOnPoseUpdated();
                    }
                    break;
                case UMI3DPropertyKeys.Rotation:
                    dto.rotation = (SerializableVector4)property.value;
                    if (node.updatePose)
                    {
                        node.transform.localRotation = dto.rotation;
                        node.SendOnPoseUpdated();
                    }
                    break;
                case UMI3DPropertyKeys.Scale:
                    dto.scale = (SerializableVector3)property.value;
                    if (node.updatePose)
                    {
                        node.transform.localScale = dto.scale;
                        node.SendOnPoseUpdated();
                    }
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
            var dto = (node.dto as GlTFSceneDto)?.extensions?.umi3d as UMI3DSceneNodeDto;
            if (dto == null) return false;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.Position:
                    dto.position = UMI3DNetworkingHelper.Read<SerializableVector3>(container); ;
                    if (node.updatePose)
                    {
                        node.transform.localPosition = dto.position;
                        node.SendOnPoseUpdated();
                    }
                    break;
                case UMI3DPropertyKeys.Rotation:
                    dto.rotation = UMI3DNetworkingHelper.Read<SerializableVector4>(container); ;
                    if (node.updatePose)
                    {
                        node.transform.localRotation = dto.rotation;
                        node.SendOnPoseUpdated();
                    }
                    break;
                case UMI3DPropertyKeys.Scale:
                    dto.scale = UMI3DNetworkingHelper.Read<SerializableVector3>(container); ;
                    if (node.updatePose)
                    {
                        node.transform.localScale = dto.scale;
                        node.SendOnPoseUpdated();
                    }
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
                    UMI3DLogger.LogError("this material failed to load : " + material.name,scope);
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
            ulong id = (glTFMaterialDto?.extensions?.umi3d as AbstractEntityDto).id;

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

                case UMI3DPropertyKeys.HeightTexture:
                    UMI3DLogger.LogWarning("Height Texture not supported",scope);
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

                case UMI3DPropertyKeys.ShaderProperties:
                    UMI3DLogger.LogWarning("not totaly implemented",scope);
                    IMaterialDto extension = glTFMaterialDto.extensions.umi3d;
                    switch (property)
                    {
                        case SetEntityDictionaryAddPropertyDto p:
                            //  string key = (string)p.key;
                            if (extension.shaderProperties.ContainsKey((string)p.key))
                            {
                                extension.shaderProperties[(string)p.key] = ((UMI3DShaderPropertyDto)p.value).value;
                                UMI3DLogger.LogWarning("this key (" + p.key.ToString() + ") already exists. Update old value",scope);
                            }
                            else
                            {
                                extension.shaderProperties.Add((string)p.key, ((UMI3DShaderPropertyDto)p.value).value);
                            }

                            break;
                        case SetEntityDictionaryRemovePropertyDto p:
                            extension.shaderProperties.Remove((string)p.key);
                            UMI3DLogger.LogWarning("Warning a property is removed but it cannot be applied",scope);
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
                        AbstractUMI3DMaterialLoader.ReadAdditionalShaderProperties(id, extension.shaderProperties, materialToModify);
                    break;

                default:
                    var uMI3DMaterialDto = glTFMaterialDto?.extensions?.umi3d as UMI3DMaterialDto;
                    if (uMI3DMaterialDto == null)
                        return false;

                    switch (property.property)
                    {
                        case UMI3DPropertyKeys.Maintexture:
                            AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, (TextureDto)property.value, MRTKShaderUtils.MainTex, materialToModify);
                            uMI3DMaterialDto.baseColorTexture = (TextureDto)property.value;
                            break;

                        case UMI3DPropertyKeys.NormalTexture:
                            AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, (ScalableTextureDto)property.value, MRTKShaderUtils.NormalMap, materialToModify);
                            uMI3DMaterialDto.normalTexture = (ScalableTextureDto)property.value;
                            break;

                        case UMI3DPropertyKeys.EmissiveTexture:
                            AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, (TextureDto)property.value, MRTKShaderUtils.EmissionMap, materialToModify);
                            uMI3DMaterialDto.emissiveTexture = (TextureDto)property.value;
                            break;

                        case UMI3DPropertyKeys.RoughnessTexture:
                            AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, (TextureDto)property.value, MRTKShaderUtils.RoughnessMap, materialToModify);
                            uMI3DMaterialDto.roughnessTexture = (TextureDto)property.value;
                            break;

                        case UMI3DPropertyKeys.MetallicTexture:
                            AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, (TextureDto)property.value, MRTKShaderUtils.MetallicMap, materialToModify);
                            uMI3DMaterialDto.metallicTexture = (TextureDto)property.value;
                            break;

                        case UMI3DPropertyKeys.ChannelTexture:
                            AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, (TextureDto)property.value, MRTKShaderUtils.ChannelMap, materialToModify);
                            uMI3DMaterialDto.channelTexture = (TextureDto)property.value;
                            break;

                        case UMI3DPropertyKeys.MetallicRoughnessTexture:
                            AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, (TextureDto)property.value, MRTKShaderUtils.MetallicMap, materialToModify);
                            AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, (TextureDto)property.value, MRTKShaderUtils.RoughnessMap, materialToModify);
                            uMI3DMaterialDto.metallicRoughnessTexture = (TextureDto)property.value;

                            break;

                        case UMI3DPropertyKeys.OcclusionTexture:
                            AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, (TextureDto)property.value, MRTKShaderUtils.OcclusionMap, materialToModify);
                            uMI3DMaterialDto.occlusionTexture = (TextureDto)property.value;
                            break;

                        case UMI3DPropertyKeys.NormalTextureScale:
                            materialToModify.ApplyShaderProperty(MRTKShaderUtils.NormalMapScale, (float)(double)property.value);
                            uMI3DMaterialDto.normalTexture.scale = (float)(double)property.value;
                            break;

                        case UMI3DPropertyKeys.HeightTextureScale:
                            //UMI3DLogger.LogWarning("Height Texture not supported",scope);
                            AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, (TextureDto)property.value, MRTKShaderUtils.BumpMap, materialToModify);
                            uMI3DMaterialDto.heightTexture = (ScalableTextureDto)property.value;
                            break;
                    }
                    break;
            }

            return true;
        }


        private bool SwitchOnMaterialProperties(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container, object materialToModify)
        {
            var glTFMaterialDto = entity?.dto as GlTFMaterialDto;
            ulong id = (glTFMaterialDto?.extensions?.umi3d as AbstractEntityDto)?.id ?? 0;

            switch (propertyKey)
            {
                case UMI3DPropertyKeys.RoughnessFactor:
                    //        ((Material)entity.Object).SetFloat("_Roughness", (float)(double)property.value);
                    //      ((Material)entity.Object).SetFloat("_Smoothness", RoughnessToSmoothness((float)(double)property.value)); 
                    float rf = UMI3DNetworkingHelper.Read<float>(container);
                    if (materialToModify is Material)
                    {
                        (materialToModify as Material).ApplyShaderProperty(MRTKShaderUtils.Smoothness, RoughnessToSmoothness(rf));
                    }
                    else if (materialToModify is List<Material>)
                    {
                        foreach (Material itemToModify in materialToModify as List<Material>)
                        {
                            itemToModify.ApplyShaderProperty(MRTKShaderUtils.Smoothness, RoughnessToSmoothness(rf));
                        }
                    }
                    else
                    {
                        return false;
                    }

                    glTFMaterialDto.pbrMetallicRoughness.roughnessFactor = rf;
                    break;

                case UMI3DPropertyKeys.MetallicFactor:
                    float mf = UMI3DNetworkingHelper.Read<float>(container);
                    if (materialToModify is Material)
                    {
                        (materialToModify as Material).ApplyShaderProperty(MRTKShaderUtils.Metallic, mf);
                    }
                    else if (materialToModify is List<Material>)
                    {
                        foreach (Material itemToModify in materialToModify as List<Material>)
                        {
                            itemToModify.ApplyShaderProperty(MRTKShaderUtils.Metallic, mf);
                        }
                    }
                    else
                    {
                        return false;
                    }

                    glTFMaterialDto.pbrMetallicRoughness.metallicFactor = mf;
                    break;

                case UMI3DPropertyKeys.BaseColorFactor:
                    Color bc = UMI3DNetworkingHelper.Read<Color>(container);
                    if (materialToModify is Material)
                    {
                        (materialToModify as Material).color = bc;
                    }
                    else if (materialToModify is List<Material>)
                    {
                        foreach (Material itemToModify in materialToModify as List<Material>)
                        {
                            itemToModify.color = bc;
                        }
                    }
                    else
                    {
                        return false;
                    }

                    glTFMaterialDto.pbrMetallicRoughness.baseColorFactor = bc;
                    break;

                case UMI3DPropertyKeys.EmissiveFactor:
                    Color ef = UMI3DNetworkingHelper.Read<Color>(container);
                    if (materialToModify is Material)
                    {
                        (materialToModify as Material).ApplyShaderProperty(MRTKShaderUtils.EmissiveColor, ef);
                    }
                    else if (materialToModify is List<Material>)
                    {
                        foreach (Material itemToModify in materialToModify as List<Material>)
                        {
                            itemToModify.ApplyShaderProperty(MRTKShaderUtils.EmissiveColor, ef);
                        }
                    }
                    else
                    {
                        return false;
                    }

                    glTFMaterialDto.emissiveFactor = (Vector3)(Vector4)ef;
                    break;

                case UMI3DPropertyKeys.HeightTexture:
                    UMI3DLogger.LogWarning("Height Texture not supported",scope);
                    break;

                case UMI3DPropertyKeys.TextureTilingOffset:
                    Vector2 offset = UMI3DNetworkingHelper.Read<Vector2>(container);
                    if (materialToModify is Material)
                    {
                        foreach (string textureName in (materialToModify as Material).GetTexturePropertyNames())
                        {
                            (materialToModify as Material).SetTextureOffset(textureName, offset);
                        }
                    }
                    else if (materialToModify is List<Material>)
                    {
                        foreach (Material itemToModify in materialToModify as List<Material>)
                        {
                            foreach (string textureName in itemToModify.GetTexturePropertyNames())
                            {
                                itemToModify.SetTextureOffset(textureName, offset);
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }

                    glTFMaterialDto.extensions.KHR_texture_transform.offset = offset;
                    break;

                case UMI3DPropertyKeys.TextureTilingScale:
                    Vector2 scale = UMI3DNetworkingHelper.Read<Vector2>(container);
                    if (materialToModify is Material)
                    {
                        foreach (string textureName in (materialToModify as Material).GetTexturePropertyNames())
                        {
                            (materialToModify as Material).SetTextureScale(textureName, scale);
                        }
                    }
                    else if (materialToModify is List<Material>)
                    {
                        foreach (Material itemToModify in materialToModify as List<Material>)
                        {
                            foreach (string textureName in itemToModify.GetTexturePropertyNames())
                            {
                                itemToModify.SetTextureScale(textureName, scale);
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }

                    glTFMaterialDto.extensions.KHR_texture_transform.scale = scale;
                    break;

                case UMI3DPropertyKeys.ShaderProperties:
                    UMI3DLogger.LogWarning("not totaly implemented",scope);
                    IMaterialDto extension = glTFMaterialDto.extensions.umi3d;
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
                                UMI3DLogger.LogWarning($"this key [{key}] already exists. Update old value",scope);
                            }
                            else
                            {
                                extension.shaderProperties.Add(key, value);
                            }

                            break;
                        case UMI3DOperationKeys.SetEntityDictionnaryRemoveProperty:
                            key = UMI3DNetworkingHelper.Read<string>(container);
                            extension.shaderProperties.Remove((string)key);
                            UMI3DLogger.LogWarning("Warning a property is removed but it cannot be applied",scope);
                            break;
                        case UMI3DOperationKeys.SetEntityDictionnaryProperty:
                            key = UMI3DNetworkingHelper.Read<string>(container);
                            value = UMI3DNetworkingHelper.Read<UMI3DShaderPropertyDto>(container).value;
                            extension.shaderProperties[key] = value;
                            break;
                        case UMI3DOperationKeys.SetEntityProperty:
                            extension.shaderProperties = UMI3DNetworkingHelper.ReadDictionary<string, UMI3DShaderPropertyDto>(container).Select(k => new KeyValuePair<string, object>(k.Key, k.Value.value)).ToDictionary();
                            break;
                        default:
                            break;
                    }
                    if (materialToModify != null)
                    {
                        if (materialToModify is Material)
                        {
                            AbstractUMI3DMaterialLoader.ReadAdditionalShaderProperties(id, extension.shaderProperties, materialToModify as Material);
                        }
                        else if (materialToModify is List<Material>)
                        {
                            foreach (Material itemToModify in materialToModify as List<Material>)
                            {
                                AbstractUMI3DMaterialLoader.ReadAdditionalShaderProperties(id, extension.shaderProperties, itemToModify);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;

                default:
                    var uMI3DMaterialDto = glTFMaterialDto?.extensions?.umi3d as UMI3DMaterialDto;
                    if (uMI3DMaterialDto == null)
                        return false;
                    switch (propertyKey)
                    {
                        case UMI3DPropertyKeys.Maintexture:
                            TextureDto mt = UMI3DNetworkingHelper.Read<TextureDto>(container);
                            if (materialToModify is Material)
                            {
                                AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, mt, MRTKShaderUtils.MainTex, materialToModify as Material);
                            }
                            else if (materialToModify is List<Material>)
                            {
                                foreach (Material itemToModify in materialToModify as List<Material>)
                                {
                                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, mt, MRTKShaderUtils.MainTex, itemToModify);
                                }
                            }
                            else
                            {
                                return false;
                            }

                            uMI3DMaterialDto.baseColorTexture = mt;
                            break;

                        case UMI3DPropertyKeys.NormalTexture:
                            ScalableTextureDto nt = UMI3DNetworkingHelper.Read<ScalableTextureDto>(container);
                            if (materialToModify is Material)
                            {
                                AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, nt, MRTKShaderUtils.NormalMap, materialToModify as Material);
                            }
                            else if (materialToModify is List<Material>)
                            {
                                foreach (Material itemToModify in materialToModify as List<Material>)
                                {
                                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, nt, MRTKShaderUtils.NormalMap, itemToModify);
                                }
                            }
                            else
                            {
                                return false;
                            }

                            uMI3DMaterialDto.normalTexture = nt;
                            break;

                        case UMI3DPropertyKeys.EmissiveTexture:
                            TextureDto et = UMI3DNetworkingHelper.Read<TextureDto>(container);
                            if (materialToModify is Material)
                            {
                                AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, et, MRTKShaderUtils.EmissionMap, materialToModify as Material);
                            }
                            else if (materialToModify is List<Material>)
                            {
                                foreach (Material itemToModify in materialToModify as List<Material>)
                                {
                                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, et, MRTKShaderUtils.EmissionMap, itemToModify);
                                }
                            }
                            else
                            {
                                return false;
                            }

                            uMI3DMaterialDto.emissiveTexture = et;
                            break;

                        case UMI3DPropertyKeys.RoughnessTexture:
                            TextureDto rt = UMI3DNetworkingHelper.Read<TextureDto>(container);
                            if (materialToModify is Material)
                            {
                                AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, rt, MRTKShaderUtils.RoughnessMap, materialToModify as Material);
                            }
                            else if (materialToModify is List<Material>)
                            {
                                foreach (Material itemToModify in materialToModify as List<Material>)
                                {
                                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, rt, MRTKShaderUtils.RoughnessMap, itemToModify);
                                }
                            }
                            else
                            {
                                return false;
                            }

                            uMI3DMaterialDto.roughnessTexture = rt;
                            break;

                        case UMI3DPropertyKeys.MetallicTexture:
                            TextureDto met = UMI3DNetworkingHelper.Read<TextureDto>(container);
                            if (materialToModify is Material)
                            {
                                AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, met, MRTKShaderUtils.MetallicMap, materialToModify as Material);
                            }
                            else if (materialToModify is List<Material>)
                            {
                                foreach (Material itemToModify in materialToModify as List<Material>)
                                {
                                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, met, MRTKShaderUtils.MetallicMap, itemToModify);
                                }
                            }
                            else
                            {
                                return false;
                            }

                            uMI3DMaterialDto.metallicTexture = met;
                            break;

                        case UMI3DPropertyKeys.ChannelTexture:
                            TextureDto ct = UMI3DNetworkingHelper.Read<TextureDto>(container);
                            if (materialToModify is Material)
                            {
                                AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, ct, MRTKShaderUtils.ChannelMap, materialToModify as Material);
                            }
                            else if (materialToModify is List<Material>)
                            {
                                foreach (Material itemToModify in materialToModify as List<Material>)
                                {
                                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, ct, MRTKShaderUtils.ChannelMap, itemToModify);
                                }
                            }
                            else
                            {
                                return false;
                            }

                            uMI3DMaterialDto.channelTexture = ct;
                            break;

                        case UMI3DPropertyKeys.MetallicRoughnessTexture:
                            TextureDto mrt = UMI3DNetworkingHelper.Read<TextureDto>(container);
                            if (materialToModify is Material)
                            {
                                AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, mrt, MRTKShaderUtils.MetallicMap, materialToModify as Material);
                                AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, mrt, MRTKShaderUtils.RoughnessMap, materialToModify as Material);
                            }
                            else if (materialToModify is List<Material>)
                            {
                                foreach (Material itemToModify in materialToModify as List<Material>)
                                {
                                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, mrt, MRTKShaderUtils.MetallicMap, itemToModify);
                                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, mrt, MRTKShaderUtils.RoughnessMap, itemToModify);
                                }
                            }
                            else
                            {
                                return false;
                            }

                            uMI3DMaterialDto.metallicRoughnessTexture = mrt;

                            break;

                        case UMI3DPropertyKeys.OcclusionTexture:
                            TextureDto ot = UMI3DNetworkingHelper.Read<TextureDto>(container);
                            if (materialToModify is Material)
                            {
                                AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, ot, MRTKShaderUtils.OcclusionMap, materialToModify as Material);
                            }
                            else if (materialToModify is List<Material>)
                            {
                                foreach (Material itemToModify in materialToModify as List<Material>)
                                {
                                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, ot, MRTKShaderUtils.OcclusionMap, itemToModify);
                                }
                            }
                            else
                            {
                                return false;
                            }

                            uMI3DMaterialDto.occlusionTexture = ot;
                            break;

                        case UMI3DPropertyKeys.NormalTextureScale:
                            float nts = UMI3DNetworkingHelper.Read<float>(container);
                            if (materialToModify is Material)
                            {
                                (materialToModify as Material).ApplyShaderProperty(MRTKShaderUtils.NormalMapScale, nts);
                            }
                            else if (materialToModify is List<Material>)
                            {
                                foreach (Material itemToModify in materialToModify as List<Material>)
                                {
                                    (materialToModify as Material).ApplyShaderProperty(MRTKShaderUtils.NormalMapScale, nts);
                                }
                            }
                            else
                            {
                                return false;
                            }

                            uMI3DMaterialDto.normalTexture.scale = nts;
                            break;

                        case UMI3DPropertyKeys.HeightTextureScale:
                            ScalableTextureDto hts = UMI3DNetworkingHelper.Read<ScalableTextureDto>(container);
                            //UMI3DLogger.LogWarning("Height Texture not supported");
                            if (materialToModify is Material)
                            {
                                AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, hts, MRTKShaderUtils.BumpMap, materialToModify as Material);
                            }
                            else if (materialToModify is List<Material>)
                            {
                                foreach (Material itemToModify in materialToModify as List<Material>)
                                {
                                    AbstractUMI3DMaterialLoader.LoadTextureInMaterial(id, hts, MRTKShaderUtils.BumpMap, itemToModify);
                                }
                            }
                            else
                            {
                                return false;
                            }

                            uMI3DMaterialDto.heightTexture = hts;
                            break;
                        default:
                            return false;
                    }
                    break;
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
            if (entity != null && entity.Object != null && (entity.Object is Material || entity.Object is List<Material>))
            {
                return SwitchOnMaterialProperties(entity, operationId, propertyKey, container, entity.Object);
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
                    UMI3DLogger.LogWarning("Height Texture not supported",scope);
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
                    UMI3DLogger.LogWarning("not totaly implemented",scope);
                    break;

                default:
                    return false;

            }

            return true;
        }

    }

}