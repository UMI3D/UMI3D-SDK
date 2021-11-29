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
using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    public class AbstractRenderedNodeLoader : UMI3DNodeLoader
    {
        const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        ///<inheritdoc/>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base.SetUMI3DProperty(entity, property)) return true;
            if (entity == null) return false;
            var extension = (entity?.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DRenderedNodeDto;
            if (extension == null) return false;
            switch (property.property)
            {

                case UMI3DPropertyKeys.ApplyCustomMaterial:
                    extension.applyCustomMaterial = (bool)property.value;

                    if (!(bool)property.value) //revert original materials
                    {
                        RevertToOriginalMaterial((UMI3DNodeInstance)entity);
                    }
                    else
                    {
                        SetMaterialOverided(extension, (UMI3DNodeInstance)entity);

                        //apply submodel overrider if subojects are tracked
                        if (extension is UMI3DMeshNodeDto && ((UMI3DMeshNodeDto)extension).areSubobjectsTracked)
                        {
                            foreach (UMI3DNodeInstance subNode in ((UMI3DNodeInstance)entity).subNodeInstances)
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

                    switch (property)
                    {
                        case SetEntityListAddPropertyDto addProperty:
                            if (((SetEntityListAddPropertyDto)property).index == extension.overridedMaterials.Count)
                            {
                                if (extension.applyCustomMaterial)
                                {
                                    var node = ((UMI3DNodeInstance)entity);
                                    ulong newMatId = ((UMI3DRenderedNodeDto.MaterialOverrideDto)addProperty.value).newMaterialId;
                                    bool shouldAdd = ((UMI3DRenderedNodeDto.MaterialOverrideDto)addProperty.value).addMaterialIfNotExists;
                                    UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(newMatId, ((UMI3DRenderedNodeDto.MaterialOverrideDto)addProperty.value).overridedMaterialsId, node, null, shouldAdd));

                                }
                                extension.overridedMaterials.Add((UMI3DRenderedNodeDto.MaterialOverrideDto)addProperty.value);
                            }
                            else
                            {
                                extension.overridedMaterials.Insert(((SetEntityListAddPropertyDto)property).index, (UMI3DRenderedNodeDto.MaterialOverrideDto)addProperty.value);
                                if (extension.applyCustomMaterial)
                                {
                                    var node = ((UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(property.entityId));
                                    SetMaterialOverided(extension, (UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(property.entityId));
                                }

                            }
                            //       
                            break;
                        case SetEntityListRemovePropertyDto removeProperty:

                            if (extension.applyCustomMaterial)
                            {
                                RevertOneOverrider((UMI3DNodeInstance)entity, (UMI3DRenderedNodeDto.MaterialOverrideDto)removeProperty.value);
                                extension.overridedMaterials.RemoveAt(removeProperty.index);

                                SetMaterialOverided(extension, (UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(property.entityId)); // necessary if multiples overriders override the same removed material

                            }
                            else
                            {
                                extension.overridedMaterials.RemoveAt(removeProperty.index);
                            }
                            break;
                        case SetEntityListPropertyDto changeProperty:
                            var propertValue = (UMI3DRenderedNodeDto.MaterialOverrideDto)changeProperty.value;
                            if (extension.applyCustomMaterial)
                            {
                                //Remove old overrider
                                RevertOneOverrider((UMI3DNodeInstance)entity, propertValue);

                                //Change overriders list
                                extension.overridedMaterials[changeProperty.index] = propertValue;

                                bool shouldAdd = ((UMI3DRenderedNodeDto.MaterialOverrideDto)changeProperty.value).addMaterialIfNotExists;

                                //Apply new overrider (Apply again the list from the new element to then end of the list)
                                var node = ((UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(property.entityId));
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
                                RevertToOriginalMaterial((UMI3DNodeInstance)entity);
                                extension.overridedMaterials = (prop.value as List<object>)?.Select(i => i as UMI3DRenderedNodeDto.MaterialOverrideDto).Where(i => i != null).ToList();

                                SetMaterialOverided(extension, (UMI3DNodeInstance)entity);

                            }
                            else
                            {
                                extension.overridedMaterials = (prop.value as List<object>)?.Select(i => i as UMI3DRenderedNodeDto.MaterialOverrideDto).Where(i => i != null).ToList();
                            }
                            break;
                        default:
                            UMI3DLogger.LogWarning("wrong type in AsyncProperty list ", scope);
                            break;
                    }

                    break;

                case UMI3DPropertyKeys.CastShadow:
                    extension.castShadow = (bool)property.value;
                    if (entity is UMI3DNodeInstance)
                    {
                        var node = entity as UMI3DNodeInstance;
                        foreach (Renderer renderer in node.renderers)
                            renderer.shadowCastingMode = extension.castShadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                    else
                    {
                        return false;
                    }

                    break;
                case UMI3DPropertyKeys.ReceiveShadow:
                    extension.receiveShadow = (bool)property.value;
                    if (entity is UMI3DNodeInstance)
                    {
                        var node = entity as UMI3DNodeInstance;
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

        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (base.SetUMI3DProperty(entity, operationId, propertyKey, container)) return true;
            if (entity == null) return false;
            var extension = (entity?.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DRenderedNodeDto;
            if (extension == null) return false;
            var node = entity as UMI3DNodeInstance;
            switch (propertyKey)
            {

                case UMI3DPropertyKeys.ApplyCustomMaterial:
                    extension.applyCustomMaterial = UMI3DNetworkingHelper.Read<bool>(container);

                    if (!extension.applyCustomMaterial) //revert original materials
                    {
                        RevertToOriginalMaterial((UMI3DNodeInstance)entity);
                    }
                    else
                    {
                        SetMaterialOverided(extension, (UMI3DNodeInstance)entity);

                        //apply submodel overrider if subojects are tracked
                        if (extension is UMI3DMeshNodeDto && ((UMI3DMeshNodeDto)extension).areSubobjectsTracked)
                        {
                            foreach (UMI3DNodeInstance subNode in ((UMI3DNodeInstance)entity).subNodeInstances)
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
                    UMI3DRenderedNodeDto.MaterialOverrideDto mat;
                    switch (operationId)
                    {
                        case UMI3DOperationKeys.SetEntityListAddProperty:
                            index = UMI3DNetworkingHelper.Read<int>(container);
                            mat = UMI3DNetworkingHelper.Read<UMI3DRenderedNodeDto.MaterialOverrideDto>(container);
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
                            index = UMI3DNetworkingHelper.Read<int>(container);
                            if (extension.applyCustomMaterial)
                            {
                                mat = UMI3DNetworkingHelper.Read<UMI3DRenderedNodeDto.MaterialOverrideDto>(container);
                                RevertOneOverrider((UMI3DNodeInstance)entity, mat);
                                extension.overridedMaterials.RemoveAt(index);

                                SetMaterialOverided(extension, node); // necessary if multiples overriders override the same removed material

                            }
                            else
                            {
                                extension.overridedMaterials.RemoveAt(index);
                            }
                            break;
                        case UMI3DOperationKeys.SetEntityListProperty:
                            index = UMI3DNetworkingHelper.Read<int>(container);
                            mat = UMI3DNetworkingHelper.Read<UMI3DRenderedNodeDto.MaterialOverrideDto>(container);
                            if (extension.applyCustomMaterial)
                            {
                                //Remove old overrider
                                RevertOneOverrider((UMI3DNodeInstance)entity, mat);

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
                            List<UMI3DRenderedNodeDto.MaterialOverrideDto> list = UMI3DNetworkingHelper.ReadList<UMI3DRenderedNodeDto.MaterialOverrideDto>(container);
                            if (extension.applyCustomMaterial)
                            {
                                RevertToOriginalMaterial((UMI3DNodeInstance)entity);
                                extension.overridedMaterials = list;

                                SetMaterialOverided(extension, (UMI3DNodeInstance)entity);

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
                    extension.castShadow = UMI3DNetworkingHelper.Read<bool>(container);
                    if (entity is UMI3DNodeInstance)
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
                    extension.receiveShadow = UMI3DNetworkingHelper.Read<bool>(container);
                    if (entity is UMI3DNodeInstance)
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
                foreach (UMI3DRenderedNodeDto.MaterialOverrideDto mat in dto.overridedMaterials)
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
                    if (filter(renderer.sharedMaterials[i].name) || (oldMats != null && oldMats.oldMats[i] != null && filter(oldMats.oldMats[i].name)))
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
                        renderer.materials = ((IEnumerable<Material>)matsToApply).Take(oldMats.Length).ToArray();
                    else
                        renderer.materials = matsToApply;
                }
            }
        }

        protected virtual void RevertOneOverrider(UMI3DNodeInstance entity, UMI3DRenderedNodeDto.MaterialOverrideDto matToRemoveDto)
        {
            var matToRemove = (Material)UMI3DEnvironmentLoader.GetEntity(matToRemoveDto.newMaterialId).Object;
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
                                matsToApply[i] = (oldMats[i]);
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
            Dictionary<string, object> shaderProperties = null;
            if (newMat == null)
            {
                // apply shader properties 
                shaderProperties = (matEntity.dto as GlTFMaterialDto)?.extensions.umi3d.shaderProperties;
            }
            if (listToOverride.Contains("ANY_mat"))
            {
                OverrideMaterial(newMatId, node, newMat, (s) => true, matEntity, shaderProperties);
            }
            else
            {
                foreach (string matKey in listToOverride)
                {
                    OverrideMaterial(newMatId, node, newMat, (s) => s.Equals(matKey) || (s.Equals(matKey + " (Instance)")), matEntity, shaderProperties, addIfNotExists);
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
                UMI3DLogger.LogWarning("That should not happen", scope);
                return node.renderers;
            }

            UMI3DLogger.LogError("RendererNodeLoader used for non rendered node", scope);
            return new List<Renderer>();
        }



    }
}

