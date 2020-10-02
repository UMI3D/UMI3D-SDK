/*
Copyright 2019 Gfi Informatique

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
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using MainThreadDispatcher;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for UMI3D Mesh
    /// </summary>
    public class UMI3DMeshNodeLoader : UMI3DNodeLoader
    {
        public List<string> ignoredPrimitiveNameForSubObjectsLoading = new List<string>() { "Gltf_Primitive" };
        public UMI3DMeshNodeLoader(List<string> ignoredPrimitiveNameForSubObjectsLoading)
        {
            this.ignoredPrimitiveNameForSubObjectsLoading = ignoredPrimitiveNameForSubObjectsLoading;
        }
        public UMI3DMeshNodeLoader() { }


        /// <summary>
        /// Load a mesh node.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the abstract node will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<string> failed)
        {
            var nodeDto = dto as UMI3DAbstractNodeDto;
            if (node == null)
            {
                failed.Invoke("dto should be an  UMI3DAbstractNodeDto");
                return;
            }

            base.ReadUMI3DExtension(dto, node, () =>
            {

                //MeshRenderer nodeMesh = node.AddComponent<MeshRenderer>();
                FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(((UMI3DMeshNodeDto)dto).mesh.variants);  // Peut etre ameliore

                string url = fileToLoad.url;
                string ext = fileToLoad.extension;
                string authorization = fileToLoad.authorization;
                string pathIfInBundle = fileToLoad.pathIfInBundle;
                IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
                if (loader != null)
                    UMI3DResourcesManager.LoadFile(
                        nodeDto.id,
                        fileToLoad,
                        loader.UrlToObject,
                        loader.ObjectFromCache,
                        (o) =>
                        {
                            CallbackAfterLoadingForMesh((GameObject)o, (UMI3DMeshNodeDto)dto, node.transform);
                            finished.Invoke();
                        },
                        failed,
                        loader.DeleteObject
                        );
            }, failed);
        }


        /// <summary>
        ///  Set Sub Objects References.
        /// </summary>
        /// <param name="goInCache"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        private GameObject SetSubObjectsReferences(GameObject goInCache, UMI3DMeshNodeDto dto)
        {
            string url = UMI3DEnvironmentLoader.Parameters.ChooseVariante(dto.mesh.variants).url;
            if (!UMI3DResourcesManager.Instance.subModelsCache.ContainsKey(url))
            {
                GameObject copy = GameObject.Instantiate(goInCache, UMI3DResourcesManager.Instance.gameObject.transform);// goInCache.transform.parent);
                Dictionary<string, Transform> subObjectsReferences = new Dictionary<string, Transform>();
                foreach (Transform child in copy.GetComponentsInChildren<Transform>())
                {
                    if (!ignoredPrimitiveNameForSubObjectsLoading.Contains(child.name)) // ignore game objects created by the gltf importer or other importer 
                    {
                        child.SetParent(copy.transform.parent);
                        subObjectsReferences.Add(child.name, child);
                    }
                }
                UMI3DResourcesManager.Instance.subModelsCache.Add(url, subObjectsReferences);
                return copy;
            }
            else
            {
                return UMI3DResourcesManager.Instance.subModelsCache[url][goInCache.name + "(Clone)"].gameObject;
            }
        }

        private void CallbackAfterLoadingForMesh(GameObject go, UMI3DMeshNodeDto dto, Transform parent)
        {
            GameObject root = null;
            if (dto.areSubobjectsTracked)
            {
                root = SetSubObjectsReferences(go, dto);
            }
            else
            {
                root = go;
            }
            var instance = GameObject.Instantiate(root, parent, true);
            AbstractMeshDtoLoader.ShowModelRecursively(instance);
            instance.transform.localPosition = root.transform.localPosition;
            instance.transform.localScale = root.transform.localScale;
            instance.transform.localEulerAngles = root.transform.localEulerAngles;
            UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode(dto.id);
            ColliderDto colliderDto = (dto).colliderDto;
            SetCollider(nodeInstance, colliderDto);
            SetMaterialOverided(dto, instance);
           
        }

        public void SetMaterialOverided(UMI3DMeshNodeDto dto, GameObject instance)
        {
            if (dto.applyCustomMaterial && dto.overridedMaterials != null )
            {
                //TODO a améliorer 
                foreach (UMI3DMeshNodeDto.MaterialOverrideDto mat in dto.overridedMaterials)
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
                    if (filter(renderer.sharedMaterials[i].name) || (oldMats!=null && oldMats.oldMats[i]!=null && filter(oldMats.oldMats[i].name)))
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

        private void RevertToOriginalMaterial(UMI3DNodeInstance entity)
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

        private void RevertOneOverrider(UMI3DNodeInstance entity, UMI3DMeshNodeDto.MaterialOverrideDto matToRemove)
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

        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base.SetUMI3DProperty(entity, property)) return true;
            if (entity == null) return false;
            var extension = (UMI3DMeshNodeDto)((GlTFNodeDto)entity?.dto)?.extensions?.umi3d;
            if (extension == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.Model:
                    extension.mesh = (ResourceDto)property.value;
                    ReadUMI3DExtension(extension, ((UMI3DNodeInstance)entity).transform.parent.gameObject, null, null);
                    break;
                case UMI3DPropertyKeys.ApplyCustomMaterial:
                    if (!(bool)property.value) //revert original materials
                    {
                        RevertToOriginalMaterial((UMI3DNodeInstance)entity);
                    }
                    else
                    {
                        SetMaterialOverided(extension, ((UMI3DNodeInstance)entity).gameObject);
                    }
                    extension.applyCustomMaterial = (bool)property.value;
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
                                string newMatId = ((UMI3DMeshNodeDto.MaterialOverrideDto)addProperty.value).newMaterialId;
                                UnityMainThreadDispatcher.Instance().StartCoroutine(ApplyMaterialOverrider(newMatId, ((UMI3DMeshNodeDto.MaterialOverrideDto)addProperty.value).overridedMaterialsId, gameObjectToOverride));

                            }
                            extension.overridedMaterials.Add((UMI3DMeshNodeDto.MaterialOverrideDto)addProperty.value);
                            break;
                        case SetEntityListRemovePropertyDto removeProperty:

                            if (extension.applyCustomMaterial)
                            {
                                RevertOneOverrider((UMI3DNodeInstance)entity, (UMI3DMeshNodeDto.MaterialOverrideDto)removeProperty.value);
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
                            var propertValue = (UMI3DMeshNodeDto.MaterialOverrideDto)changeProperty.value;
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
                                extension.overridedMaterials = (List<UMI3DMeshNodeDto.MaterialOverrideDto>)prop.value;
                                SetMaterialOverided(extension, ((UMI3DNodeInstance)entity).gameObject);

                            }
                            else
                            { 
                                extension.overridedMaterials = (List<UMI3DMeshNodeDto.MaterialOverrideDto>)prop.value;
                            }
                            break;
                        default:
                            Debug.LogWarning("wrong type in AsyncProperty list ");
                            break;
                    }

                   break;

                default:
                    return false;
            }
            return true;
        }

        private IEnumerator ApplyMaterialOverrider (string newMatId, List<string> listToOverride, GameObject gameObject, Action callback = null)
        {
            UMI3DEntityInstance matEntity = UMI3DEnvironmentLoader.GetEntity(newMatId);
            if(matEntity == null) Debug.LogWarning("Material not found : " + newMatId +" , that should not append");

            while (matEntity == null)
            {
                matEntity = UMI3DEnvironmentLoader.GetEntity(newMatId);

                yield return new WaitForSeconds(0.2f);
            }

            if(gameObject == null || matEntity == null)
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
            if(callback != null)
                callback.Invoke();

        }

    }

}