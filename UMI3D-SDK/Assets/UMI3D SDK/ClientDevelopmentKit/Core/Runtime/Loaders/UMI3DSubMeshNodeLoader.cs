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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="UMI3DMeshNodeDto"/>.
    /// </summary>
    public class UMI3DSubMeshNodeLoader : AbstractRenderedNodeLoader
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is SubModelDto && base.CanReadUMI3DExtension(data);
        }


        /// <inheritdoc/>
        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {

            await base.ReadUMI3DExtension(data);

            var nodeDto = data.dto as SubModelDto;
            if (nodeDto == null)
            {
                throw (new Umi3dException("nodeDto should not be null"));
            }

            var e = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(nodeDto.modelId,data.tokens);
            LoadSubModel(e, data.node, nodeDto);
        }


        private void LoadSubModel(UMI3DEntityInstance entity, GameObject node, SubModelDto subDto)
        {
            if (entity is UMI3DNodeInstance modelNodeInstance)
            {
                var modelDto = (GlTFNodeDto)modelNodeInstance.dto;
                UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode(subDto.id);

                string modelInCache = UMI3DEnvironmentLoader.Parameters.ChooseVariant(((UMI3DMeshNodeDto)modelDto.extensions.umi3d).mesh.variants).url;

                var rootDto = (UMI3DMeshNodeDto)modelDto.extensions.umi3d;
                GameObject instance = null;

                try
                {
                    string sub = subDto.subModelName;

                    UMI3DResourcesManager.Instance.GetSubModel(modelInCache, sub, subDto.subModelHierachyIndexes, subDto.subModelHierachyNames, (o) =>
                    {
                        instance = GameObject.Instantiate((GameObject)o, node.gameObject.transform, false);

                        AbstractMeshDtoLoader.ShowModelRecursively(instance);
                        if (!rootDto.isRightHanded)
                        {
                            instance.transform.localEulerAngles += new Vector3(0, 180, 0);
                        }

                        SetCollider(subDto.id, UMI3DEnvironmentLoader.GetNode(subDto.id), subDto.colliderDto);

                        UMI3DEnvironmentLoader.GetNode(subDto.modelId).subNodeInstances.Add(nodeInstance);
                        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();

                        if (renderers != null)
                        {
                            UMI3DEnvironmentLoader.GetNode(subDto.modelId).renderers.AddRange(renderers);
                            UMI3DEnvironmentLoader.GetNode(subDto.id).renderers.AddRange(renderers);
                        }

                        if (rootDto.applyCustomMaterial && !((SubModelDto)((GlTFNodeDto)UMI3DEnvironmentLoader.GetNode(subDto.id).dto).extensions.umi3d).ignoreModelMaterialOverride)
                        {
                            // apply root model override
                            SetMaterialOverided(rootDto, nodeInstance);
                        }

                        if (subDto.applyCustomMaterial)
                        {
                            SetMaterialOverided(subDto, nodeInstance);
                            // apply sub model overrider
                        }

                        foreach (Renderer renderer in renderers)
                        {
                            renderer.shadowCastingMode = subDto.castShadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                            renderer.receiveShadows = subDto.receiveShadow;
                        }

                        UpdateLightmapReferences(rootDto.id, renderers, o as GameObject);

                        nodeInstance.IsTraversable = subDto.isTraversable;
                        nodeInstance.IsPartOfNavmesh = subDto.isPartOfNavmesh;
                    });

                }
                catch (Exception ex)
                {
                    UMI3DLogger.LogError("SubModels names of " + rootDto.id + " are different from environment names. " + subDto.id + " not found", scope);
                    UMI3DLogger.LogException(ex, scope);
                }
                return;
            }
            else
                throw (new Umi3dException($"Model Entity [{subDto.modelId}] should be a nodeInstance"));
        }

        /// <summary>
        /// If root node has a <see cref="PrefabLightmapData"/>, updates its references
        /// </summary>
        /// <param name="rooId"></param>
        /// <param name="renderers"></param>
        protected void UpdateLightmapReferences(ulong rooId, Renderer[] renderers, GameObject o)
        {
            var lightmapData = UMI3DEnvironmentLoader.GetNode(rooId)?.prefabLightmapData;

            if (lightmapData == null)
                return;

            var renderersInfo = lightmapData.GetRenderersInfo();

            for (int i = 0; i < renderersInfo.Length; i++)
            {
                if (renderersInfo[i].renderer.gameObject == o)
                {
                    renderersInfo[i].renderer = renderers.FirstOrDefault();
                    break;
                }
            }
        }

        /// <inheritdoc/>
        protected override void RevertToOriginalMaterial(UMI3DNodeInstance entity)
        {

            //     Renderer[] renderers = entity.gameObject.GetComponentsInChildren<Renderer>();
            List<Renderer> renderers = GetChildRenderersWhithoutOtherModel(entity);
            if (renderers == null || renderers.Count == 0)
                return;
            var subDto = (SubModelDto)((GlTFNodeDto)entity.dto).extensions.umi3d;

            var parentDto = (UMI3DMeshNodeDto)((GlTFNodeDto)UMI3DEnvironmentLoader.GetNode(subDto.modelId).dto).extensions.umi3d;
            foreach (Renderer renderer in renderers)
            {
                OldMaterialContainer oldMaterialContainer = renderer.gameObject.GetComponent<OldMaterialContainer>();
                if (oldMaterialContainer != null)
                {
                    Material[] oldMats = oldMaterialContainer.oldMats;
                    Material[] matsToApply = renderer.sharedMaterials;
                    for (int i = 0; i < oldMats.Length; i++)
                    {
                        if (oldMats[i] != null)
                        {
                            matsToApply[i] = oldMats[i];
                        }
                    }
                    if (oldMats.Length != matsToApply.Length)
                        renderer.materials = matsToApply.Take(oldMats.Length).ToArray();
                    else
                        renderer.materials = matsToApply;
                }
            }

            if (parentDto.applyCustomMaterial /*&& !subDto.ignoreModelMaterialOverride */ /* && !subDto.applyCustomMaterial */&& !subDto.ignoreModelMaterialOverride)
            {
                SetMaterialOverided(parentDto, entity); //..
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            if ((data.entity?.dto as GlTFNodeDto)?.extensions?.umi3d is SubModelDto)
            {
                if (await base.SetUMI3DProperty(data))
                    return true;
                var extension = ((GlTFNodeDto)data.entity?.dto)?.extensions?.umi3d as SubModelDto;
                if (extension == null) return false;
                switch (data.property.property)
                {
                    case UMI3DPropertyKeys.IgnoreModelMaterialOverride:
                        extension.ignoreModelMaterialOverride = (bool)data.property.value;
                        if ((bool)data.property.value) //revert model override and apply only subModel overriders 
                        {
                            RevertToOriginalMaterial((UMI3DNodeInstance)data.entity);
                            SetMaterialOverided(extension, (UMI3DNodeInstance)data.entity);
                        }
                        else
                        {
                            RevertToOriginalMaterial((UMI3DNodeInstance)data.entity);
                            var parentDto = (UMI3DMeshNodeDto)((GlTFNodeDto)UMI3DEnvironmentLoader.GetNode(extension.modelId).dto).extensions.umi3d;
                            SetMaterialOverided(parentDto, (UMI3DNodeInstance)data.entity);
                            SetMaterialOverided(extension, (UMI3DNodeInstance)data.entity);
                        }
                        break;

                    case UMI3DPropertyKeys.IsPartOfNavmesh:
                        (data.entity as UMI3DNodeInstance).IsPartOfNavmesh = (bool)data.property.value;
                        return true;
                    case UMI3DPropertyKeys.IsTraversable:
                        (data.entity as UMI3DNodeInstance).IsTraversable = (bool)data.property.value;
                        return true;

                    default:
                        return false;
                }
                return true;

            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            if ((data.entity?.dto as GlTFNodeDto)?.extensions?.umi3d is SubModelDto)
            {
                if (await base.SetUMI3DProperty(data)) return true;
                var extension = ((GlTFNodeDto)data.entity?.dto)?.extensions?.umi3d as SubModelDto;
                if (extension == null) return false;
                switch (data.propertyKey)
                {
                    case UMI3DPropertyKeys.IgnoreModelMaterialOverride:
                        extension.ignoreModelMaterialOverride = UMI3DSerializer.Read<bool>(data.container);
                        if (extension.ignoreModelMaterialOverride) //revert model override and apply only subModel overriders 
                        {
                            RevertToOriginalMaterial((UMI3DNodeInstance)data.entity);
                            SetMaterialOverided(extension, (UMI3DNodeInstance)data.entity);
                        }
                        else
                        {
                            RevertToOriginalMaterial((UMI3DNodeInstance)data.entity);
                            var parentDto = (UMI3DMeshNodeDto)((GlTFNodeDto)UMI3DEnvironmentLoader.GetNode(extension.modelId).dto).extensions.umi3d;
                            SetMaterialOverided(parentDto, (UMI3DNodeInstance)data.entity);
                            SetMaterialOverided(extension, (UMI3DNodeInstance)data.entity);
                        }
                        break;

                    case UMI3DPropertyKeys.IsPartOfNavmesh:
                        (data.entity as UMI3DNodeInstance).IsPartOfNavmesh = UMI3DSerializer.Read<bool>(data.container);
                        return true;

                    case UMI3DPropertyKeys.IsTraversable:
                        (data.entity as UMI3DNodeInstance).IsTraversable = UMI3DSerializer.Read<bool>(data.container);
                        return true;

                    default:
                        return false;
                }
                return true;

            }
            else
            {
                return false;
            }
        }
    }
}
