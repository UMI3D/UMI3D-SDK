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

using inetum.unityUtils;
using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Abstract base for renderded node.
    /// </summary>
    public class AbstractRenderedNodeLoader : UMI3DNodeLoader
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        /// <inheritdoc/>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            if (await base.SetUMI3DProperty(data)) return true;
            if (data.entity == null) return false;
            var extension = (data.entity?.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DRenderedNodeDto;
            if (extension == null) return false;
            switch (data.property.property)
            {

                case UMI3DPropertyKeys.ApplyCustomMaterial:
                    extension.applyCustomMaterial = (bool)data.property.value;

                    if (!(bool)data.property.value) //revert original materials
                    {
                        RevertToOriginalMaterial((UMI3DNodeInstance)data.entity);
                    }
                    else
                    {
                        SetMaterialOverided(extension, (UMI3DNodeInstance)data.entity);

                        //apply submodel overrider if subojects are tracked
                        if (extension is UMI3DMeshNodeDto && ((UMI3DMeshNodeDto)extension).areSubobjectsTracked)
                        {
                            foreach (UMI3DNodeInstance subNode in ((UMI3DNodeInstance)data.entity).subNodeInstances)
                            {
                                var subMeshdto = ((GlTFNodeDto)subNode.dto)?.extensions?.umi3d as SubModelDto;
                                if (subMeshdto.ignoreModelMaterialOverride)
                                    RevertToOriginalMaterial(subNode);

                                SetMaterialOverided(subMeshdto, subNode);
                            }
                        }
                    }
                    break;
                case UMI3DPropertyKeys.OverideMaterialId:

                    switch (data.property)
                    {
                        case SetEntityListAddPropertyDto addProperty:
                            if (((SetEntityListAddPropertyDto)data.property).index == extension.overridedMaterials.Count)
                            {
                                if (extension.applyCustomMaterial)
                                {
                                    var node = (UMI3DNodeInstance)data.entity;
                                    ulong newMatId = ((MaterialOverrideDto)addProperty.value).newMaterialId;
                                    bool shouldAdd = ((MaterialOverrideDto)addProperty.value).addMaterialIfNotExists;
                                    UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(newMatId, ((MaterialOverrideDto)addProperty.value).overridedMaterialsId, node, null, shouldAdd));

                                }
                                extension.overridedMaterials.Add((MaterialOverrideDto)addProperty.value);
                            }
                            else
                            {
                                extension.overridedMaterials.Insert(((SetEntityListAddPropertyDto)data.property).index, (MaterialOverrideDto)addProperty.value);
                                if (extension.applyCustomMaterial)
                                {
                                    var node = (UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(data.property.entityId);
                                    SetMaterialOverided(extension, (UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(data.property.entityId));
                                }
                            }
                            //       
                            break;
                        case SetEntityListRemovePropertyDto removeProperty:

                            if (extension.applyCustomMaterial)
                            {
                                RevertOneOverrider((UMI3DNodeInstance)data.entity, (MaterialOverrideDto)removeProperty.value);
                                extension.overridedMaterials.RemoveAt(removeProperty.index);

                                SetMaterialOverided(extension, (UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(data.property.entityId)); // necessary if multiples overriders override the same removed material

                            }
                            else
                            {
                                extension.overridedMaterials.RemoveAt(removeProperty.index);
                            }
                            break;
                        case SetEntityListPropertyDto changeProperty:
                            var propertValue = (MaterialOverrideDto)changeProperty.value;
                            if (extension.applyCustomMaterial)
                            {
                                //Remove old overrider
                                RevertOneOverrider((UMI3DNodeInstance)data.entity, propertValue);

                                //Change overriders list
                                extension.overridedMaterials[changeProperty.index] = propertValue;

                                bool shouldAdd = ((MaterialOverrideDto)changeProperty.value).addMaterialIfNotExists;

                                //Apply new overrider (Apply again the list from the new element to then end of the list)
                                var node = (UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(data.property.entityId);
                                UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(propertValue.newMaterialId, propertValue.overridedMaterialsId, node, () =>
                                {
                                    for (int i = changeProperty.index + 1; i < extension.overridedMaterials.Count; i++)
                                    {
                                        ApplyMaterialOverrider(extension.overridedMaterials[i].newMaterialId, extension.overridedMaterials[i].overridedMaterialsId, node, null, shouldAdd);
                                    };
                                },
                                shouldAdd));
                            }
                            else
                            {
                                extension.overridedMaterials[changeProperty.index] = propertValue;
                            }

                            break;
                        case SetEntityPropertyDto prop:

                            if (extension.applyCustomMaterial)
                            {
                                RevertToOriginalMaterial((UMI3DNodeInstance)data.entity);
                                extension.overridedMaterials = (prop.value as List<object>)?.Select(i => i as MaterialOverrideDto).Where(i => i != null).ToList();

                                SetMaterialOverided(extension, (UMI3DNodeInstance)data.entity);

                            }
                            else
                            {
                                extension.overridedMaterials = (prop.value as List<object>)?.Select(i => i as MaterialOverrideDto).Where(i => i != null).ToList();
                            }
                            break;
                        default:
                            UMI3DLogger.LogWarning("wrong type in AsyncProperty list ", scope);
                            break;
                    }

                    break;

                case UMI3DPropertyKeys.CastShadow:
                    extension.castShadow = (bool)data.property.value;
                    if (data.entity is UMI3DNodeInstance)
                    {
                        var node = data.entity as UMI3DNodeInstance;
                        foreach (Renderer renderer in node.renderers)
                            renderer.shadowCastingMode = extension.castShadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                    else
                    {
                        return false;
                    }

                    break;
                case UMI3DPropertyKeys.ReceiveShadow:
                    extension.receiveShadow = (bool)data.property.value;
                    if (data.entity is UMI3DNodeInstance)
                    {
                        var node = data.entity as UMI3DNodeInstance;
                        foreach (Renderer renderer in node.renderers)
                            renderer.receiveShadows = extension.receiveShadow;
                    }
                    else
                    {
                        return false;
                    }

                    break;
                default:
                    return false;
            }
            return true;
        }

        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            if (await base.SetUMI3DProperty(data)) return true;
            if (data.entity == null) return false;
            var extension = (data.entity?.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DRenderedNodeDto;
            if (extension == null) return false;
            var node = data.entity as UMI3DNodeInstance;
            switch (data.propertyKey)
            {

                case UMI3DPropertyKeys.ApplyCustomMaterial:
                    extension.applyCustomMaterial = UMI3DSerializer.Read<bool>(data.container);

                    if (!extension.applyCustomMaterial) //revert original materials
                    {
                        RevertToOriginalMaterial((UMI3DNodeInstance)data.entity);
                    }
                    else
                    {
                        SetMaterialOverided(extension, (UMI3DNodeInstance)data.entity);

                        //apply submodel overrider if subojects are tracked
                        if (extension is UMI3DMeshNodeDto && ((UMI3DMeshNodeDto)extension).areSubobjectsTracked)
                        {
                            foreach (UMI3DNodeInstance subNode in ((UMI3DNodeInstance)data.entity).subNodeInstances)
                            {
                                var subMeshdto = ((GlTFNodeDto)subNode.dto)?.extensions?.umi3d as SubModelDto;
                                if (subMeshdto.ignoreModelMaterialOverride)
                                    RevertToOriginalMaterial(subNode);

                                SetMaterialOverided(subMeshdto, subNode);
                            }
                        }
                    }
                    break;
                case UMI3DPropertyKeys.OverideMaterialId:
                    int index;
                    MaterialOverrideDto mat;
                    switch (data.operationId)
                    {
                        case UMI3DOperationKeys.SetEntityListAddProperty:
                            index = UMI3DSerializer.Read<int>(data.container);
                            mat = UMI3DSerializer.Read<MaterialOverrideDto>(data.container);
                            if (index == extension.overridedMaterials.Count)
                            {
                                if (extension.applyCustomMaterial)
                                {
                                    ulong newMatId = mat.newMaterialId;
                                    bool shouldAdd = mat.addMaterialIfNotExists;
                                    UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(newMatId, mat.overridedMaterialsId, node, null, shouldAdd));

                                }
                                extension.overridedMaterials.Add(mat);
                            }
                            else
                            {
                                extension.overridedMaterials.Insert(index, mat);
                                if (extension.applyCustomMaterial)
                                    SetMaterialOverided(extension, node);
                            }
                            //       
                            break;
                        case UMI3DOperationKeys.SetEntityListRemoveProperty:
                            index = UMI3DSerializer.Read<int>(data.container);
                            if (extension.applyCustomMaterial)
                            {
                                mat = UMI3DSerializer.Read<MaterialOverrideDto>(data.container);
                                RevertOneOverrider((UMI3DNodeInstance)data.entity, mat);
                                extension.overridedMaterials.RemoveAt(index);

                                SetMaterialOverided(extension, node); // necessary if multiples overriders override the same removed material

                            }
                            else
                            {
                                extension.overridedMaterials.RemoveAt(index);
                            }
                            break;
                        case UMI3DOperationKeys.SetEntityListProperty:
                            index = UMI3DSerializer.Read<int>(data.container);
                            mat = UMI3DSerializer.Read<MaterialOverrideDto>(data.container);
                            if (extension.applyCustomMaterial)
                            {
                                //Remove old overrider
                                RevertOneOverrider((UMI3DNodeInstance)data.entity, mat);

                                //Change overriders list
                                extension.overridedMaterials[index] = mat;

                                bool shouldAdd = mat.addMaterialIfNotExists;

                                //Apply new overrider (Apply again the list from the new element to then end of the list)
                                UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(mat.newMaterialId, mat.overridedMaterialsId, node, () =>
                                {
                                    for (int i = index + 1; i < extension.overridedMaterials.Count; i++)
                                    {
                                        ApplyMaterialOverrider(extension.overridedMaterials[i].newMaterialId, extension.overridedMaterials[i].overridedMaterialsId, node, null, shouldAdd);
                                    };
                                },
                                shouldAdd));
                            }
                            else
                            {
                                extension.overridedMaterials[index] = mat;
                            }

                            break;
                        case UMI3DOperationKeys.SetEntityProperty:
                            List<MaterialOverrideDto> list = UMI3DSerializer.ReadList<MaterialOverrideDto>(data.container);
                            if (extension.applyCustomMaterial)
                            {
                                RevertToOriginalMaterial((UMI3DNodeInstance)data.entity);
                                extension.overridedMaterials = list;

                                SetMaterialOverided(extension, (UMI3DNodeInstance)data.entity);

                            }
                            else
                            {
                                extension.overridedMaterials = list;
                            }
                            break;
                        default:
                            UMI3DLogger.LogWarning("wrong type in AsyncProperty list ", scope);
                            break;
                    }

                    break;

                case UMI3DPropertyKeys.CastShadow:
                    extension.castShadow = UMI3DSerializer.Read<bool>(data.container);
                    if (data.entity is UMI3DNodeInstance)
                    {
                        foreach (Renderer renderer in node.renderers)
                            renderer.shadowCastingMode = extension.castShadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                    else
                    {
                        return false;
                    }

                    break;
                case UMI3DPropertyKeys.ReceiveShadow:
                    extension.receiveShadow = UMI3DSerializer.Read<bool>(data.container);
                    if (data.entity is UMI3DNodeInstance)
                    {
                        foreach (Renderer renderer in node.renderers)
                            renderer.receiveShadows = extension.receiveShadow;
                    }
                    else
                    {
                        return false;
                    }

                    break;
                default:
                    return false;
            }
            return true;
        }


        public void SetMaterialOverided(UMI3DRenderedNodeDto dto, UMI3DNodeInstance instance)
        {
            if (dto != null && dto.applyCustomMaterial && dto.overridedMaterials != null)
            {
                //TODO a améliorer 
                foreach (MaterialOverrideDto mat in dto.overridedMaterials)
                {
                    UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(mat.newMaterialId, mat.overridedMaterialsId, instance, null, mat.addMaterialIfNotExists));

                }
            }
        }

        private void OverrideMaterial(ulong id, UMI3DNodeInstance node, Material newMat, Func<string, bool> filter, UMI3DEntityInstance entity, Dictionary<string, object> additionalShaderProperties = null, bool addIfNotExists = false)
        {
            foreach (Renderer renderer in GetChildRenderersWhithoutOtherModel(node))
            {

                Material[] mats = renderer.sharedMaterials;
                bool modified = false;

                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    OldMaterialContainer oldMats = renderer.gameObject.GetOrAddComponent<OldMaterialContainer>();
                    if (filter(renderer.sharedMaterials[i].name) || (oldMats != null && oldMats.oldMats.Length > i && oldMats.oldMats[i] != null && filter(oldMats.oldMats[i].name)))
                    {
                        if (oldMats.oldMats[i] == null)
                            oldMats.oldMats[i] = renderer.sharedMaterials[i];

                        if (newMat != null)
                        {
                            mats[i] = newMat;
                        }
                        else
                        {
                            mats[i] = new Material(oldMats.oldMats[i]);
                            if (additionalShaderProperties != null)
                                AbstractUMI3DMaterialLoader.ReadAdditionalShaderProperties(id, additionalShaderProperties, mats[i]);
                            if (entity.Object == null)
                                entity.Object = new List<Material>();
                            var matList = entity.Object as List<Material>;
                            if (matList != null)
                            {
                                matList.Add(mats[i]);
                            }
                        }

                        modified = true;
                    }
                }
                if (modified)
                {
                    renderer.materials = mats;
                }
                else
                {
                    if (addIfNotExists)
                    {
                        Material[] matTab = renderer.sharedMaterials;

                        renderer.materials = matTab.Concat(new Material[] { newMat }).ToArray();

                    }
                }
            }
        }

        protected virtual void RevertToOriginalMaterial(UMI3DNodeInstance entity)
        {
            foreach (Renderer renderer in GetChildRenderersWhithoutOtherModel(entity))
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
        }

        protected virtual void RevertOneOverrider(UMI3DNodeInstance entity, MaterialOverrideDto matToRemoveDto)
        {
            var matToRemove = (Material)UMI3DEnvironmentLoader.GetEntity(matToRemoveDto.newMaterialId)?.Object;
            if (matToRemove == null) return;
            foreach (Renderer renderer in GetChildRenderersWhithoutOtherModel(entity))
            {
                OldMaterialContainer oldMaterialContainer = renderer.gameObject.GetComponent<OldMaterialContainer>();
                if (oldMaterialContainer != null)
                {
                    Material[] oldMats = oldMaterialContainer.oldMats;
                    Material[] matsToApply = renderer.sharedMaterials;
                    bool removed = false;
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        if (matsToApply[i] == matToRemove)
                        {
                            if (i < oldMats.Length)
                            {
                                matsToApply[i] = oldMats[i];
                            }
                            else
                            {
                                matsToApply[i] = null;
                                removed = true;
                            }
                        }
                    }
                    if (removed) // remove null element in materials
                    {
                        renderer.materials = matsToApply.Where((m) => m != null).ToArray();
                    }
                    else
                    {
                        renderer.materials = matsToApply;
                    }
                }
            }
        }


        protected IEnumerator ApplyMaterialOverrider(ulong newMatId, List<string> listToOverride, UMI3DNodeInstance node, Action callback = null, bool addIfNotExists = false)
        {
            UMI3DEntityInstance matEntity = UMI3DEnvironmentLoader.GetEntity(newMatId);
            if (matEntity == null) UMI3DLogger.LogWarning("Material not found : " + newMatId + " , that should not happen", scope);

            while (matEntity == null)
            {
                matEntity = UMI3DEnvironmentLoader.GetEntity(newMatId);

                yield return new WaitForSeconds(0.2f);
            }

            if (node == null || node.gameObject == null || matEntity == null)
            {
                UMI3DLogger.LogWarning("object has been removed during material loading ", scope);
                yield break;
            }

            var newMat = matEntity.Object as Material;
            // apply shader properties 
            Dictionary<string, object> shaderProperties = shaderProperties = (matEntity.dto as GlTFMaterialDto)?.extensions.umi3d.shaderProperties;

            if (listToOverride.Contains("ANY_mat"))
            {
                OverrideMaterial(newMatId, node, newMat, (s) => true, matEntity, shaderProperties);
            }
            else
            {
                foreach (string matKey in listToOverride)
                {
                    OverrideMaterial(newMatId, node, newMat, (s) => s.Equals(matKey) || s.Equals(matKey + " (Instance)") || matKey.Equals(s + " (Instance)"), matEntity, shaderProperties, addIfNotExists);
                }
            }

            if (callback != null)
                callback.Invoke();

        }


        protected List<Renderer> GetChildRenderersWhithoutOtherModel(UMI3DNodeInstance node)
        {
            if (((GlTFNodeDto)node.dto).extensions.umi3d is UMI3DMeshNodeDto)
                return node.renderers;
            if (((GlTFNodeDto)node.dto).extensions.umi3d is SubModelDto)
            {
                if (node.renderers != null && node.renderers.Count > 0)
                    return node.renderers;
                List<Renderer> modelMeshs = UMI3DEnvironmentLoader.GetNode(((SubModelDto)((GlTFNodeDto)node.dto).extensions.umi3d).modelId).renderers;

                //    Renderer[] childRenderers = node.gameObject.GetComponentsInChildren<Renderer>();
                node.renderers = node.gameObject.GetComponentsInChildren<Renderer>().Where((r) => modelMeshs.Contains(r)).ToList();
                UMI3DLogger.LogWarning("Renderers list was empty, That should not happen", scope);
                return node.renderers;
            }
            if (((GlTFNodeDto)node.dto).extensions.umi3d is UMI3DLineDto)
            {
                if (node.renderers != null && node.renderers.Count > 0)
                    return node.renderers;
                node.renderers = new List<Renderer>() { node.gameObject.GetComponent<LineRenderer>() };
                return node.renderers;

            }

            UMI3DLogger.LogError("RendererNodeLoader used for non rendered node", scope);
            return new List<Renderer>();
        }
    }
}

