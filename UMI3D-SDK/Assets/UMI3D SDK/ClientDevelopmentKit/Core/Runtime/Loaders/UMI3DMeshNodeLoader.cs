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
                failed.Invoke(new Umi3dException(0, "dto should be an  UMI3DAbstractNodeDto"));
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
                Vector3 offset = Vector3.zero;
                if (loader is AbstractMeshDtoLoader)
                    offset = ((AbstractMeshDtoLoader)loader).GetRotationOffset();
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
                                CallbackAfterLoadingForMesh(g, meshDto, node.transform, offset);
                                finished.Invoke();
                            }
                            else
                            {
                                failed?.Invoke(new Umi3dException(0, $"Cast not valid for {o.GetType()} into GameObject or {dto.GetType()} into UMI3DMeshNodeDto"));
                            }
                        },
                        failed,
                        loader.DeleteObject
                        );
                }
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
            string url = UMI3DEnvironmentLoader.Parameters.ChooseVariante(dto.mesh.variants).url;
            if (!UMI3DResourcesManager.Instance.subModelsCache.ContainsKey(url))
            {
                var copy = GameObject.Instantiate(goInCache, UMI3DResourcesManager.Instance.gameObject.transform);// goInCache.transform.parent);
                foreach (LODGroup lodgroup in copy.GetComponentsInChildren<LODGroup>())
                    GameObject.Destroy(lodgroup);
                var subObjectsReferences = new Dictionary<string, Transform>();
                foreach (Transform child in copy.GetComponentsInChildren<Transform>())
                {
                    if (!ignoredPrimitiveNameForSubObjectsLoading.Contains(child.name)) // ignore game objects created by the gltf importer or other importer 
                    {
                        child.SetParent(copy.transform.parent);
                        subObjectsReferences.Add(child.name, child);
                        child.transform.localEulerAngles = rotationOffsetByLoader;
                        child.transform.localPosition = Vector3.zero;
                        child.transform.localScale = Vector3.one;
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

        private void CallbackAfterLoadingForMesh(GameObject go, UMI3DMeshNodeDto dto, Transform parent, Vector3 rotationOffsetByLoader)
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
            ColliderDto colliderDto = (dto).colliderDto;
            SetCollider(dto.id, nodeInstance, colliderDto);
            SetMaterialOverided(dto, nodeInstance);

        }



    }

}