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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for UMI3D Mesh
    /// </summary>
    public class UMI3DMeshNodeLoader : AbstractRenderedNodeLoader
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
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<Umi3dException> failed)
        {
            var nodeDto = dto as UMI3DAbstractNodeDto;
            if (node == null)
            {
                failed.Invoke(new Umi3dException("Dto should be an UMI3DAbstractNodeDto"));
                return;
            }

            base.ReadUMI3DExtension(dto, node, () =>
            {

                //MeshRenderer nodeMesh = node.AddComponent<MeshRenderer>();
                FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariant(((UMI3DMeshNodeDto)dto).mesh.variants);  // Peut etre ameliore
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
                    UMI3DResourcesManager.LoadFile(
                        nodeDto.id,
                        fileToLoad,
                        loader.UrlToObject,
                        loader.ObjectFromCache,
                        (o) =>
                        {
                            if (o is GameObject g && dto is UMI3DMeshNodeDto meshDto)
                            {
                                CallbackAfterLoadingForMesh(g, meshDto, node.transform, offset, finished);
                            }
                            else
                            {
                                failed?.Invoke(new Umi3dException($"Cast not valid for {o.GetType()} into GameObject or {dto.GetType()} into UMI3DMeshNodeDto"));
                            }
                        },
                        failed,
                        loader.DeleteObject
                        );
                }
                else
                    failed.Invoke(new Umi3dException($"No loader found for {ext}"));
            }, failed);
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

        private void CallbackAfterLoadingForMesh(GameObject go, UMI3DMeshNodeDto dto, Transform parent, Vector3 rotationOffsetByLoader, Action finished)
        {
            GameObject root = null;
            if (dto.areSubobjectsTracked)
            {
                root = SetSubObjectsReferences(go, dto, rotationOffsetByLoader);
            }
            else
            {
                root = go;
            }

            var instance = GameObject.Instantiate(root, parent, true);
            UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode(dto.id);
            AbstractMeshDtoLoader.ShowModelRecursively(instance);
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            nodeInstance.renderers = renderers.ToList();

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

            finished?.Invoke();
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