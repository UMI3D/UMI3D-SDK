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

using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class UMI3DSubMeshNodeLoader : AbstractRenderedNodeLoader
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        ///<inheritdoc/>
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<Umi3dException> failed)
        {

            base.ReadUMI3DExtension(dto, node, () =>
            {
                var nodeDto = dto as SubModelDto;
                if (nodeDto != null)
                {
                    void LoadSubModel(UMI3DEntityInstance entity)
                    {
                        if (entity is UMI3DNodeInstance modelNodeInstance)
                        {
                            var modelDto = (GlTFNodeDto)modelNodeInstance.dto;
                            UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode(nodeDto.id);

                            string modelInCache = UMI3DEnvironmentLoader.Parameters.ChooseVariant(((UMI3DMeshNodeDto)modelDto.extensions.umi3d).mesh.variants).url;

                            var rootDto = (UMI3DMeshNodeDto)modelDto.extensions.umi3d;
                            GameObject instance = null;

                            try
                            {
                                string sub = nodeDto.subModelName;

                                UMI3DResourcesManager.Instance.GetSubModel(modelInCache, sub, nodeDto.subModelHierachyIndexes, nodeDto.subModelHierachyNames, (o) =>
                                {

                                    instance = GameObject.Instantiate((GameObject)o, node.gameObject.transform, false);

                                    AbstractMeshDtoLoader.ShowModelRecursively(instance);
                                    if (!rootDto.isRightHanded)
                                    {
                                        instance.transform.localEulerAngles += new Vector3(0, 180, 0);
                                    }

                                    SetCollider(nodeDto.id, UMI3DEnvironmentLoader.GetNode(nodeDto.id), ((UMI3DNodeDto)dto).colliderDto);

                                    UMI3DEnvironmentLoader.GetNode(nodeDto.modelId).subNodeInstances.Add(nodeInstance);
                                    Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();

                                    if (renderers != null)
                                    {
                                        UMI3DEnvironmentLoader.GetNode(nodeDto.modelId).renderers.AddRange(renderers);
                                        UMI3DEnvironmentLoader.GetNode(nodeDto.id).renderers.AddRange(renderers);
                                    }

                                    if (rootDto.applyCustomMaterial && !((SubModelDto)((GlTFNodeDto)UMI3DEnvironmentLoader.GetNode(nodeDto.id).dto).extensions.umi3d).ignoreModelMaterialOverride)
                                    {
                                        // apply root model override
                                        SetMaterialOverided(rootDto, nodeInstance);
                                    }

                                    if (nodeDto.applyCustomMaterial)
                                    {
                                        SetMaterialOverided(nodeDto, nodeInstance);
                                        // apply sub model overrider
                                    }

                                    foreach (Renderer renderer in renderers)
                                    {
                                        renderer.shadowCastingMode = nodeDto.castShadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                                        renderer.receiveShadows = nodeDto.receiveShadow;
                                    }

                                    UpdateLightmapReferences(rootDto.id, renderers, o as GameObject);
                                });

                            }
                            catch (Exception e)
                            {
                                UMI3DLogger.LogError("SubModels names of " + rootDto.id + " are different from environment names. " + nodeDto.id + " not found", scope);
                                UMI3DLogger.LogExcetion(e, scope);
                            }
                            finished?.Invoke();
                        }
                        else
                            failed?.Invoke(new Umi3dException($"Model Entity [{nodeDto.modelId}] should be a nodeInstance"));
                    }
                    UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(
                        nodeDto.modelId,
                        LoadSubModel,
                        () => failed?.Invoke(new Umi3dException($"Model [{nodeDto.modelId}] of this sub model [{nodeDto.id}] failed to load"))
                    );

                }
                else
                {
                    failed?.Invoke(new Umi3dException("nodeDto should not be null"));
                }
            }, failed);
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


        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if ((entity?.dto as GlTFNodeDto)?.extensions?.umi3d is SubModelDto)
            {
                if (base.SetUMI3DProperty(entity, property)) return true;
                var extension = ((GlTFNodeDto)entity?.dto)?.extensions?.umi3d as SubModelDto;
                if (extension == null) return false;
                switch (property.property)
                {
                    case UMI3DPropertyKeys.IgnoreModelMaterialOverride:
                        extension.ignoreModelMaterialOverride = (bool)property.value;
                        if ((bool)property.value) //revert model override and apply only subModel overriders 
                        {
                            RevertToOriginalMaterial((UMI3DNodeInstance)entity);
                            SetMaterialOverided(extension, (UMI3DNodeInstance)entity);
                        }
                        else
                        {
                            RevertToOriginalMaterial((UMI3DNodeInstance)entity);
                            var parentDto = (UMI3DMeshNodeDto)((GlTFNodeDto)UMI3DEnvironmentLoader.GetNode(extension.modelId).dto).extensions.umi3d;
                            SetMaterialOverided(parentDto, (UMI3DNodeInstance)entity);
                            SetMaterialOverided(extension, (UMI3DNodeInstance)entity);
                        }
                        break;

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

        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if ((entity?.dto as GlTFNodeDto)?.extensions?.umi3d is SubModelDto)
            {
                if (base.SetUMI3DProperty(entity, operationId, propertyKey, container)) return true;
                var extension = ((GlTFNodeDto)entity?.dto)?.extensions?.umi3d as SubModelDto;
                if (extension == null) return false;
                switch (propertyKey)
                {
                    case UMI3DPropertyKeys.IgnoreModelMaterialOverride:
                        extension.ignoreModelMaterialOverride = UMI3DNetworkingHelper.Read<bool>(container);
                        if (extension.ignoreModelMaterialOverride) //revert model override and apply only subModel overriders 
                        {
                            RevertToOriginalMaterial((UMI3DNodeInstance)entity);
                            SetMaterialOverided(extension, (UMI3DNodeInstance)entity);
                        }
                        else
                        {
                            RevertToOriginalMaterial((UMI3DNodeInstance)entity);
                            var parentDto = (UMI3DMeshNodeDto)((GlTFNodeDto)UMI3DEnvironmentLoader.GetNode(extension.modelId).dto).extensions.umi3d;
                            SetMaterialOverided(parentDto, (UMI3DNodeInstance)entity);
                            SetMaterialOverided(extension, (UMI3DNodeInstance)entity);
                        }
                        break;

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
