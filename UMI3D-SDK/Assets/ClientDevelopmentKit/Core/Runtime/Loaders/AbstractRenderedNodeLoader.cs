using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
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
                        SetMaterialOverided(extension, ((UMI3DNodeInstance)entity).gameObject);
                    }
                    break;
                case UMI3DPropertyKeys.OverideMaterialId:
                    //  Debug.Log("override mat id");

                    switch (property)
                    {
                        case SetEntityListAddPropertyDto addProperty:
                            Debug.Log("SetEntityListAddPropertyDto");
                            if (extension.applyCustomMaterial)
                            {
                                GameObject gameObjectToOverride = ((UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(property.entityId)).gameObject;
                                string newMatId = ((UMI3DRenderedNodeDto.MaterialOverrideDto)addProperty.value).newMaterialId;
                                UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(newMatId, ((UMI3DRenderedNodeDto.MaterialOverrideDto)addProperty.value).overridedMaterialsId, gameObjectToOverride));

                            }
                            extension.overridedMaterials.Add((UMI3DRenderedNodeDto.MaterialOverrideDto)addProperty.value);
                            break;
                        case SetEntityListRemovePropertyDto removeProperty:

                            if (extension.applyCustomMaterial)
                            {
                                RevertOneOverrider((UMI3DNodeInstance)entity, (UMI3DRenderedNodeDto.MaterialOverrideDto)removeProperty.value);
                                extension.overridedMaterials.RemoveAt(removeProperty.index);
                                if (extension.applyCustomMaterial)
                                {
                                    SetMaterialOverided(extension, ((UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(property.entityId)).gameObject); // necessary if multiples overriders override the same removed material
                                }
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
                                GameObject go = ((UMI3DNodeInstance)UMI3DEnvironmentLoader.GetEntity(property.entityId)).gameObject;
                                UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(propertValue.newMaterialId, propertValue.overridedMaterialsId, go, () =>
                                {
                                    for (int i = changeProperty.index + 1; i < extension.overridedMaterials.Count; i++)
                                    {
                                        ApplyMaterialOverrider(extension.overridedMaterials[i].newMaterialId, extension.overridedMaterials[i].overridedMaterialsId, go);
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
                                SetMaterialOverided(extension, ((UMI3DNodeInstance)entity).gameObject);

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

        public void SetMaterialOverided(UMI3DRenderedNodeDto dto, GameObject instance)
        {
            if (dto.applyCustomMaterial && dto.overridedMaterials != null)
            {
                //TODO a améliorer 
                foreach (UMI3DRenderedNodeDto.MaterialOverrideDto mat in dto.overridedMaterials)
                {
                    UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(mat.newMaterialId, mat.overridedMaterialsId, instance));

                }
            }
        }

        private void OverrideMaterial(GameObject go, Material newMat, Func<string, bool> filter)
        {
            foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>())
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
            foreach (Renderer renderer in entity.gameObject.GetComponentsInChildren<Renderer>())
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
            foreach (Renderer renderer in (entity).gameObject.GetComponentsInChildren<Renderer>())
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


        protected IEnumerator ApplyMaterialOverrider(string newMatId, List<string> listToOverride, GameObject gameObject, Action callback = null)
        {
            UMI3DEntityInstance matEntity = UMI3DEnvironmentLoader.GetEntity(newMatId);
            if (matEntity == null) Debug.LogWarning("Material not found : " + newMatId + " , that should not append");

            while (matEntity == null)
            {
                matEntity = UMI3DEnvironmentLoader.GetEntity(newMatId);

                yield return new WaitForSeconds(0.2f);
            }

            if (gameObject == null || matEntity == null)
            {
                Debug.LogWarning("object has been removed during material loading ");
                yield break;
            }

            if (listToOverride.Contains("ANY_mat"))
            {
                OverrideMaterial(gameObject, (Material)matEntity.Object, (s) => true);
            }
            else
            {
                foreach (string matKey in listToOverride)
                {
                    OverrideMaterial(gameObject, (Material)matEntity.Object, (s) => s.Equals(matKey) || (s.Equals(matKey + " (Instance)")));
                }
            }
            if (callback != null)
                callback.Invoke();

        }

    }
}

