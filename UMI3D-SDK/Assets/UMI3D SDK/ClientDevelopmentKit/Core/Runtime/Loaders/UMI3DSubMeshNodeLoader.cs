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

        ///<inheritdoc/>
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<string> failed)
        {

            base.ReadUMI3DExtension(dto, node, () =>
            {
                var nodeDto = dto as SubModelDto;
                if (nodeDto != null)
                {


                    UMI3DNodeInstance modelNodeInstance = UMI3DEnvironmentLoader.GetNode(nodeDto.modelId);
                    GlTFNodeDto modelDto = (GlTFNodeDto)modelNodeInstance.dto;
                    UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode(nodeDto.id);

                    string modelInCache = UMI3DEnvironmentLoader.Parameters.ChooseVariante(((UMI3DMeshNodeDto)modelDto.extensions.umi3d).mesh.variants).url;

                    UMI3DMeshNodeDto rootDto = (UMI3DMeshNodeDto)modelDto.extensions.umi3d;
                    var rootGO = UMI3DEnvironmentLoader.GetNode((nodeDto).pid).gameObject;
                    GameObject instance = null;

                    try
                    {
                        string sub = nodeDto.subModelName;
                        if (UMI3DResourcesManager.Instance.subModelsCache.ContainsKey(modelInCache))
                        {
                            instance = GameObject.Instantiate(UMI3DResourcesManager.Instance.subModelsCache[modelInCache][sub].gameObject, node.gameObject.transform);

                            UMI3DEnvironmentLoader.GetNode(nodeDto.modelId).subNodeInstances.Add(nodeInstance);
                            AbstractMeshDtoLoader.ShowModelRecursively(instance);

                            var renderers = instance.GetComponentsInChildren<Renderer>();
                            if (renderers != null)
                                UMI3DEnvironmentLoader.GetNode(nodeDto.modelId).renderers.AddRange(renderers);
                            if (rootDto.applyCustomMaterial)
                            {
                                // apply root model override
                                SetMaterialOverided(rootDto, nodeInstance);
                            }
                            if (nodeDto.applyCustomMaterial)
                            {
                                SetMaterialOverided(nodeDto, nodeInstance);
                                // apply sub model overrider
                            }
                            foreach (var renderer in renderers)
                            {
                                renderer.shadowCastingMode = nodeDto.castShadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                                renderer.receiveShadows = nodeDto.receiveShadow;
                            }

                            SetCollider(nodeDto.id, UMI3DEnvironmentLoader.GetNode(nodeDto.id), ((UMI3DNodeDto)dto).colliderDto);
                        }
                        else
                        {
                            UMI3DResourcesManager.Instance.GetSubModel(modelInCache, sub, (o) =>
                            {

                                instance = GameObject.Instantiate((GameObject)o, node.gameObject.transform, false);

                                AbstractMeshDtoLoader.ShowModelRecursively(instance);
                                if (!rootDto.isRightHanded)
                                {
                                    instance.transform.localEulerAngles += new Vector3(0, 180, 0);
                                }
                                /*         instance.transform.localPosition = Vector3.zero;
                                         instance.transform.localEulerAngles = Vector3.zero; //new Vector3(0, 180, 0);
                                         instance.transform.localScale = Vector3.one;*/
                                SetCollider(nodeDto.id, UMI3DEnvironmentLoader.GetNode(nodeDto.id), ((UMI3DNodeDto)dto).colliderDto);

                                UMI3DEnvironmentLoader.GetNode(nodeDto.modelId).subNodeInstances.Add(nodeInstance);
                                var renderers = instance.GetComponentsInChildren<Renderer>();
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

                                foreach (var renderer in renderers)
                                {
                                    renderer.shadowCastingMode = nodeDto.castShadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                                    renderer.receiveShadows = nodeDto.receiveShadow;
                                }

                            });
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        Debug.LogError("SubModels names of " + rootDto.id + " are different from environment names. " + nodeDto.id + " not found");
                    }
                    finished?.Invoke();
                }
                else failed?.Invoke("nodeDto should not be null");
            }, failed);
        }


        protected override void RevertToOriginalMaterial(UMI3DNodeInstance entity)
        {

            //     Renderer[] renderers = entity.gameObject.GetComponentsInChildren<Renderer>();
            List<Renderer> renderers = GetChildRenderersWhithoutOtherModel(entity);
            if (renderers == null || renderers.Count == 0)
                return;
            SubModelDto subDto = (SubModelDto)((GlTFNodeDto)entity.dto).extensions.umi3d;

            UMI3DMeshNodeDto parentDto = (UMI3DMeshNodeDto)((GlTFNodeDto)UMI3DEnvironmentLoader.GetNode(subDto.modelId).dto).extensions.umi3d;
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
                            matsToApply[i] = (oldMats[i]);
                        }
                    }
                    if (oldMats.Length != matsToApply.Length)
                        renderer.materials = ((IEnumerable<Material>)matsToApply).Take(oldMats.Length).ToArray();
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
                            UMI3DMeshNodeDto parentDto = (UMI3DMeshNodeDto)((GlTFNodeDto)UMI3DEnvironmentLoader.GetNode(extension.modelId).dto).extensions.umi3d;
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
                return false;
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
                            UMI3DMeshNodeDto parentDto = (UMI3DMeshNodeDto)((GlTFNodeDto)UMI3DEnvironmentLoader.GetNode(extension.modelId).dto).extensions.umi3d;
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
                return false;
        }
    }
}
