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

        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base.SetUMI3DProperty(entity, property)) return true;
            if (entity == null) return false;
            var extension = ((GlTFNodeDto)entity?.dto)?.extensions?.umi3d as UMI3DRenderedNodeDto;
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
                                SubModelDto subMeshdto = ((GlTFNodeDto)subNode.dto)?.extensions?.umi3d as SubModelDto;
                                if (subMeshdto.ignoreModelMaterialOverride)
                                    RevertToOriginalMaterial(subNode);

                                SetMaterialOverided(subMeshdto, subNode);
                            }
                        }
                    }
                    break;
                case UMI3DPropertyKeys.OverideMaterialId:
                    //  Debug.Log("override mat id");

                    switch (property)
                    {
                        case SetEntityListAddPropertyDto addProperty:
                            //  Debug.Log("SetEntityListAddPropertyDto");
                            if (((SetEntityListAddPropertyDto)property).index == extension.overridedMaterials.Count)
                            {
                                if (extension.applyCustomMaterial)
                                {
                                    var node = ((UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(property.entityId));
                                    string newMatId = ((UMI3DRenderedNodeDto.MaterialOverrideDto)addProperty.value).newMaterialId;
                                    UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(newMatId, ((UMI3DRenderedNodeDto.MaterialOverrideDto)addProperty.value).overridedMaterialsId, node));

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

                                //Apply new overrider (Apply again the list from the new element to then end of the list)
                                UMI3DNodeInstance node = ((UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(property.entityId));
                                UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(propertValue.newMaterialId, propertValue.overridedMaterialsId, node, () =>
                                {
                                    for (int i = changeProperty.index + 1; i < extension.overridedMaterials.Count; i++)
                                    {
                                        ApplyMaterialOverrider(extension.overridedMaterials[i].newMaterialId, extension.overridedMaterials[i].overridedMaterialsId, node);
                                    };
                                }));
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
                                extension.overridedMaterials = (List<UMI3DRenderedNodeDto.MaterialOverrideDto>)prop.value;
                                SetMaterialOverided(extension, (UMI3DNodeInstance)entity);

                            }
                            else
                            {
                                extension.overridedMaterials = (List<UMI3DRenderedNodeDto.MaterialOverrideDto>)prop.value;
                            }
                            break;
                        default:
                            Debug.LogWarning("wrong type in AsyncProperty list ");
                            break;
                    }

                    break;

                case UMI3DPropertyKeys.CastShadow:
                    extension.castShadow = (bool)property.value;
                    if (entity is UMI3DNodeInstance)
                    {
                        var node = entity as UMI3DNodeInstance;
                        foreach (var renderer in node.renderers)
                            renderer.shadowCastingMode = extension.castShadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                    else return false;
                    break;
                case UMI3DPropertyKeys.ReceiveShadow:
                    extension.receiveShadow = (bool)property.value;
                    if (entity is UMI3DNodeInstance)
                    {
                        var node = entity as UMI3DNodeInstance;
                        foreach (var renderer in node.renderers)
                            renderer.receiveShadows = extension.receiveShadow;
                    }
                    else return false;
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
                    UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(mat.newMaterialId, mat.overridedMaterialsId, instance));

                }
            }
        }

        private void OverrideMaterial(UMI3DNodeInstance node, Material newMat, Func<string, bool> filter)
        {
            foreach (Renderer renderer in GetChildRenderersWhithoutOtherModel(node))
            {

                Material[] mats = renderer.sharedMaterials;
                bool modified = false;

                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    var oldMats = renderer.gameObject.GetOrAddComponent<OldMaterialContainer>();
                    if (filter(renderer.sharedMaterials[i].name) || (oldMats != null && oldMats.oldMats[i] != null && filter(oldMats.oldMats[i].name)))
                    {
                        if (oldMats.oldMats[i] == null)
                            oldMats.oldMats[i] = renderer.sharedMaterials[i];

                        mats[i] = newMat;

                        modified = true;
                    }

                }
                if (modified)
                {
                    renderer.materials = mats;
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
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        if (oldMats[i] != null)
                        {
                            matsToApply[i] = (oldMats[i]);
                        }
                    }
                    renderer.materials = matsToApply;
                }
            }
        }

        protected virtual void RevertOneOverrider(UMI3DNodeInstance entity, UMI3DRenderedNodeDto.MaterialOverrideDto matToRemove)
        {
            foreach (Renderer renderer in GetChildRenderersWhithoutOtherModel(entity))
            {
                OldMaterialContainer oldMaterialContainer = renderer.gameObject.GetComponent<OldMaterialContainer>();
                if (oldMaterialContainer != null)
                {
                    Material[] oldMats = oldMaterialContainer.oldMats;
                    Material[] matsToApply = renderer.sharedMaterials;
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        if (matsToApply[i] == (Material)UMI3DEnvironmentLoader.GetEntity(matToRemove.newMaterialId).Object)
                        {
                            matsToApply[i] = (oldMats[i]);
                        }
                    }
                    renderer.materials = matsToApply;
                }
            }
        }


        protected IEnumerator ApplyMaterialOverrider(string newMatId, List<string> listToOverride, UMI3DNodeInstance node, Action callback = null)
        {
            UMI3DEntityInstance matEntity = UMI3DEnvironmentLoader.GetEntity(newMatId);
            if (matEntity == null) Debug.LogWarning("Material not found : " + newMatId + " , that should not append");

            while (matEntity == null)
            {
                matEntity = UMI3DEnvironmentLoader.GetEntity(newMatId);

                yield return new WaitForSeconds(0.2f);
            }

            if (node == null || node.gameObject == null || matEntity == null)
            {
                Debug.LogWarning("object has been removed during material loading ");
                yield break;
            }

            if (listToOverride.Contains("ANY_mat"))
            {
                OverrideMaterial(node, (Material)matEntity.Object, (s) => true);
            }
            else
            {
                foreach (string matKey in listToOverride)
                {
                    OverrideMaterial(node, (Material)matEntity.Object, (s) => s.Equals(matKey) || (s.Equals(matKey + " (Instance)")));
                }
            }
            if (callback != null)
                callback.Invoke();

        }


        private List<Renderer> GetChildRenderersWhithoutOtherModel(UMI3DNodeInstance node)
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
                Debug.LogWarning("that should not append");
                return node.renderers;
            }

            Debug.LogError("RendererNodeLoader used for non rendered node");
            return new List<Renderer>();
        }



    }
}

