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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="UMI3DMeshNodeDto"/>.
    /// </summary>
    public class UMI3DMeshNodeLoader : AbstractRenderedNodeLoader
    {
        public List<string> ignoredPrimitiveNameForSubObjectsLoading = new List<string>() { "Gltf_Primitive" };
        public UMI3DMeshNodeLoader(List<string> ignoredPrimitiveNameForSubObjectsLoading)
        {
            this.ignoredPrimitiveNameForSubObjectsLoading = ignoredPrimitiveNameForSubObjectsLoading;
        }
        public UMI3DMeshNodeLoader() { }

        public override bool CanReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            return data.dto is UMI3DMeshNodeDto && base.CanReadUMI3DExtension(data);
        }

        /// <summary>
        /// Load a mesh node.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the abstract node will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override async Task ReadUMI3DExtension(ReadUMI3DExtensionData data)
        {
            var nodeDto = data.dto as UMI3DMeshNodeDto;
            if (data.node == null)
            {
                throw (new Umi3dException("Dto should be an UMI3DAbstractNodeDto"));
            }

            await base.ReadUMI3DExtension(data);

            //MeshRenderer nodeMesh = node.AddComponent<MeshRenderer>();
            FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariant(nodeDto.mesh.variants);  // Peut etre ameliore
            string url = fileToLoad.url;
            string ext = fileToLoad.extension;
            string authorization = fileToLoad.authorization;
            string pathIfInBundle = fileToLoad.pathIfInBundle;
            IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
            Vector3 offset = Vector3.zero;
            if (loader is AbstractMeshDtoLoader meshLoader)
                offset = meshLoader.GetRotationOffset();
            if (loader != null)
            {
                var o = await UMI3DResourcesManager.LoadFile(nodeDto.id, fileToLoad, loader);

                if (data.dto is UMI3DMeshNodeDto meshDto)
                {
                    if (o is GameObject g)
                    {
                        await CallbackAfterLoadingForMesh(g, meshDto, data.node.transform, offset, null);
                    }
                    else if (o is (GameObject go, Scene scene))
                    {
                        /*Debug.LogError("TODO : to improve");
                        var transforms = new List<GameObject>();
                        for (int i = 0; i < go.transform.childCount; i++)
                        {
                            transforms.Add(go.transform.GetChild(i).gameObject);
                        }*/

                        await CallbackAfterLoadingForMesh(go, meshDto, data.node.transform, offset, scene);

                        /*foreach (var goo in transforms.ToArray())
                        {
                            GameObject.Destroy(goo);
                        }*/
                    }
                }
                else
                    throw (new Umi3dException($"Cast not valid for {o.GetType()} into GameObject or {data.dto.GetType()} into UMI3DMeshNodeDto"));
            }
            else
                throw (new Umi3dException($"No loader found for {ext}"));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            if (await base.SetUMI3DProperty(data))
                return true;

            var extension = (data.entity?.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DMeshNodeDto;
            if (extension == null || data.entity as UMI3DNodeInstance == null) return false;

            switch (data.property.property)
            {
                case UMI3DPropertyKeys.IsPartOfNavmesh:
                    (data.entity as UMI3DNodeInstance).IsPartOfNavmesh = (bool)data.property.value;
                    return true;
                case UMI3DPropertyKeys.IsTraversable:
                    (data.entity as UMI3DNodeInstance).IsTraversable = (bool)data.property.value;
                    return true;
                default:
                    return false;
            }

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="operationId"></param>
        /// <param name="propertyKey"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            if (await base.SetUMI3DProperty(data))
                return true;

            var extension = (data.entity?.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DMeshNodeDto;
            if (extension == null) return false;
            var node = data.entity as UMI3DNodeInstance;

            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.IsPartOfNavmesh:
                    (data.entity as UMI3DNodeInstance).IsPartOfNavmesh = UMI3DSerializer.Read<bool>(data.container);
                    return true;
                case UMI3DPropertyKeys.IsTraversable:
                    (data.entity as UMI3DNodeInstance).IsTraversable = UMI3DSerializer.Read<bool>(data.container);
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        ///  Set Sub Objects References.
        /// </summary>
        /// <param name="goInCache"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        private GameObject SetSubObjectsReferences(GameObject goInCache, UMI3DMeshNodeDto dto, Vector3 rotationOffsetByLoader)
        {
            string url = UMI3DEnvironmentLoader.Parameters.ChooseVariant(dto.mesh.variants).url;
            if (!UMI3DResourcesManager.Instance.IsSubModelsSetFor(url))
            {
                var copy = GameObject.Instantiate(goInCache, UMI3DResourcesManager.Instance.gameObject.transform);// goInCache.transform.parent);
                foreach (LODGroup lodgroup in copy.GetComponentsInChildren<LODGroup>())
                    GameObject.Destroy(lodgroup);

                var subObjectsReferences = new UMI3DResourcesManager.SubmodelDataCollection();
                subObjectsReferences.SetRoot(copy.transform);

                SetSubObjectsReferencesAux(copy.transform, copy.transform, rotationOffsetByLoader, subObjectsReferences);
                UMI3DResourcesManager.Instance.AddSubModels(url, subObjectsReferences);
                return copy;
            }
            else
            {
                return UMI3DResourcesManager.Instance.GetSubModelRoot(url).gameObject;
            }
        }

        private class ChildRef
        {
            public string refByName;
            public List<int> refByIndex;
            public List<string> refByNames;
            public Transform transform;

            public ChildRef(string refByName, List<int> refByIndex, List<string> refByNames, Transform transform)
            {
                this.refByName = refByName;
                this.refByIndex = refByIndex;
                this.refByNames = refByNames;
                this.transform = transform;
            }
        }

        private void SetSubObjectsReferencesAux(Transform root, Transform Parent, Vector3 rotationOffsetByLoader, UMI3DResourcesManager.SubmodelDataCollection collection)
        {
            var childs = new List<ChildRef>() { new ChildRef(Parent.name, new List<int>(), new List<string>(), Parent) };
            GetChild(Parent, new List<int>(), new List<string>(), childs);

            foreach (ChildRef childRef in childs)
            {
                Transform child = childRef.transform;
                if (!ignoredPrimitiveNameForSubObjectsLoading.Contains(child.name)) // ignore game objects created by the gltf importer or other importer 
                {
                    child.SetParent(root.transform.parent);
                    collection.AddSubModel(childRef.refByName, childRef.refByIndex, childRef.refByNames, child);
                    child.transform.localEulerAngles = rotationOffsetByLoader;
                    child.transform.localPosition = Vector3.zero;
                    child.transform.localScale = Vector3.one;
                }
            }
        }

        private void GetChild(Transform Parent, List<int> indexes, List<string> names, List<ChildRef> collection)
        {
            for (int i = 0; i < Parent.childCount; i++)
            {
                Transform child = Parent.GetChild(i);
                var nIndexes = new List<int>(indexes) { i };
                var nNames = new List<string>(names) { child.name };
                collection.Add(new ChildRef(child.name, nIndexes, nNames, child));
                GetChild(child, nIndexes, nNames, collection);
            }
        }

        private async Task CallbackAfterLoadingForMesh(GameObject go, UMI3DMeshNodeDto dto, Transform parent, Vector3 rotationOffsetByLoader, object data)
        {
            var modelTracker = parent.gameObject.AddComponent<ModelTracker>();
            GameObject root = null;
            if (dto.areSubobjectsTracked)
            {
                root = SetSubObjectsReferences(go, dto, rotationOffsetByLoader);
                modelTracker.areSubObjectTracked = true;
            }
            else
            {
                root = go;
            }

            GameObject instance = null;
            UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode(dto.id);

            instance = GameObject.Instantiate(root, parent, true);

            if (data is Scene scene)
            {
                GameObject.Destroy(go);
                nodeInstance.scene = scene;
            }

            AbstractMeshDtoLoader.ShowModelRecursively(instance);
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            nodeInstance.renderers = renderers.ToList();

            if (dto.areSubobjectsTracked)
            {
                nodeInstance.mainInstance = instance;
                if (instance.GetComponent<Animator>())
                {
                    modelTracker.animatorsToRebind.Add(instance.GetComponent<Animator>());
                }
            }

            foreach (Renderer renderer in renderers)
            {
                renderer.shadowCastingMode = dto.castShadow ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = dto.receiveShadow;
            }

            instance.transform.localPosition = root.transform.localPosition;
            instance.transform.localScale = root.transform.localScale;
            instance.transform.localEulerAngles = root.transform.localEulerAngles;
            ColliderDto colliderDto = dto.colliderDto;
            SetCollider(dto.id, nodeInstance, colliderDto);
            SetMaterialOverided(dto, nodeInstance);
            SetLightMap(instance, nodeInstance);

            nodeInstance.IsPartOfNavmesh = dto.isPartOfNavmesh;
            nodeInstance.IsTraversable = dto.isTraversable;
        }

        /// <summary>
        /// If the node has a <see cref="PrefabLightmapData"/>, makes sure to refresh once its references are updated.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="nodeInstance"></param>
        private void SetLightMap(GameObject instance, UMI3DNodeInstance nodeInstance)
        {
            PrefabLightmapData data = instance.GetComponentInChildren<PrefabLightmapData>();
            if (data != null)
            {
                nodeInstance.prefabLightmapData = data;

                UMI3DEnvironmentLoader.StartCoroutine(RefreshLightmapData(data));
            }
        }

        /// <summary>
        /// Coroutine for <see cref="SetLightMap"/>.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        IEnumerator RefreshLightmapData(PrefabLightmapData data)
        {
            while (!UMI3DEnvironmentLoader.Instance.loaded)
                yield return null;

            data.Init();
        }


    }
}