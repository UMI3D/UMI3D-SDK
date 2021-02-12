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
using umi3d.common;
using UnityEngine;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader fot UMI3D Node
    /// </summary>
    public class UMI3DNodeLoader : UMI3DAbstractNodeLoader
    {
        /// <summary>
        /// Load an umi3d node.
        /// </summary>
        /// <param name="dto">dto.</param>
        /// <param name="node">gameObject on which the abstract node will be loaded.</param>
        /// <param name="finished">Finish callback.</param>
        /// <param name="failed">error callback.</param>
        public override void ReadUMI3DExtension(UMI3DDto dto, GameObject node, Action finished, Action<string> failed)
        {

            base.ReadUMI3DExtension(dto, node, () =>
             {
                 var nodeDto = dto as UMI3DNodeDto;
                 if (nodeDto != null)
                 {
                     if (nodeDto.colliderDto != null && !(nodeDto is UMI3DMeshNodeDto))
                     {
                         SetCollider(UMI3DEnvironmentLoader.GetNode(nodeDto.id), nodeDto.colliderDto);
                     }

                     if (nodeDto.xBillboard || nodeDto.yBillboard)
                     {
                         var b = node.AddComponent<Billboard>();
                         b.X = nodeDto.xBillboard;
                         b.Y = nodeDto.yBillboard;
                         node.gameObject.GetComponent<Billboard>().rotation = node.transform.rotation;
                     }

                     if (nodeDto.lodDto != null)
                     {
                         MainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(LoadLod(nodeDto.lodDto, node));
                     }


                     finished?.Invoke();
                 }
                 else failed?.Invoke("nodeDto should not be null");
             }, failed);
        }


        IEnumerator LoadLod(UMI3DLodDto dto, GameObject node)
        {
            var lg = node.GetOrAddComponent<LODGroup>();
            var ls = new List<LOD>();
            foreach (var lod in dto.lods)
            {
                var rend = new List<Renderer>();

                foreach (var id in lod.nodes)
                {
                    UMI3DNodeInstance n = null;
                    yield return new WaitUntil(() => (n = UMI3DEnvironmentLoader.GetNode(id)) != null);
                    var r = n.gameObject.GetComponentInChildren<Renderer>();
                    if (r != null)
                        rend.Add(r);
                }
                var l = new LOD(lod.screenSize, rend.ToArray());
                l.fadeTransitionWidth = lod.fadeTransition;
                ls.Add(l);
            }

            lg.SetLODs(ls.ToArray());
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">entity to be updated.</param>
        /// <param name="property">property containing the new value.</param>
        /// <returns></returns>
        public override bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            var node = entity as UMI3DNodeInstance;
            if (node == null) return false;

            if (!node.updatePose && (property.property == UMI3DPropertyKeys.Position || property.property == UMI3DPropertyKeys.Rotation || property.property == UMI3DPropertyKeys.Scale))
            {
                GlTFNodeDto gltfDto = (node.dto as GlTFNodeDto);
                if (gltfDto == null) return false;
                switch (property.property)
                {
                    case UMI3DPropertyKeys.Position:
                        gltfDto.position = (SerializableVector3)property.value;
                        break;
                    case UMI3DPropertyKeys.Rotation:
                        gltfDto.rotation = (SerializableVector4)property.value;
                        break;
                    case UMI3DPropertyKeys.Scale:
                        gltfDto.scale = (SerializableVector3)property.value;
                        break;
                    default:
                        break;
                }
                return true;
            }

            if (base.SetUMI3DProperty(entity, property)) return true;

            UMI3DNodeDto dto = (node.dto as GlTFNodeDto)?.extensions?.umi3d as UMI3DNodeDto;
            if (dto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.XBillboard:
                    dto.xBillboard = (bool)property.value;
                    node.gameObject.GetOrAddComponent<Billboard>().X = (bool)property.value;
                    if (dto.xBillboard || dto.yBillboard)
                    {
                        node.gameObject.GetComponent<Billboard>().enabled = true;
                        node.gameObject.GetComponent<Billboard>().rotation = node.transform.rotation;
                    }
                    else
                    {
                        node.gameObject.GetComponent<Billboard>().enabled = false;
                        node.transform.localRotation = (node.dto as GlTFNodeDto).rotation;
                    }
                    break;

                case UMI3DPropertyKeys.YBillboard:
                    dto.yBillboard = (bool)property.value;
                    node.gameObject.GetOrAddComponent<Billboard>().Y = (bool)property.value;
                    if (dto.xBillboard || dto.yBillboard)
                    {
                        node.gameObject.GetComponent<Billboard>().enabled = true;
                        node.gameObject.GetComponent<Billboard>().rotation = node.transform.rotation;
                    }
                    else
                    {
                        node.gameObject.GetComponent<Billboard>().enabled = false;
                        node.transform.localRotation = (node.dto as GlTFNodeDto).rotation;
                    }
                    break;

                case UMI3DPropertyKeys.Convex:
                    {
                        if (dto.colliderDto == null)
                            dto.colliderDto = new ColliderDto();
                        dto.colliderDto.convex = (bool)property.value;

                        foreach (Collider item in node.colliders)
                        {
                            if (item is MeshCollider)
                                (item as MeshCollider).convex = dto.colliderDto.convex = (bool)property.value;
                        }
                    }
                    break;

                case UMI3DPropertyKeys.ColliderCenter:
                    {
                        if (dto.colliderDto == null)
                            dto.colliderDto = new ColliderDto();
                        dto.colliderDto.colliderCenter = (SerializableVector3)property.value;
                        Collider c = node.gameObject.GetComponent<Collider>();
                        if (c != null && !(c is MeshCollider))
                        {
                            if (c is BoxCollider)
                            {
                                (c as BoxCollider).center = (SerializableVector3)property.value;
                            }
                            else if (c is SphereCollider)
                            {
                                (c as SphereCollider).center = (SerializableVector3)property.value;
                            }
                            else if (c is CapsuleCollider)
                            {
                                (c as CapsuleCollider).center = (SerializableVector3)property.value;
                            }
                        }
                    }
                    break;
                case UMI3DPropertyKeys.ColliderRadius:
                    {
                        if (dto.colliderDto == null)
                            dto.colliderDto = new ColliderDto();
                        dto.colliderDto.colliderRadius = (float)((double)property.value);
                        Collider c = node.gameObject.GetComponent<Collider>();
                        if (c is SphereCollider)
                        {
                            (c as SphereCollider).radius = (float)(double)property.value;
                        }
                        if (c is CapsuleCollider)
                        {
                            (c as CapsuleCollider).radius = (float)(double)property.value;
                        }

                    }
                    break;
                case UMI3DPropertyKeys.ColliderBoxSize:
                    {
                        if (dto.colliderDto == null)
                            dto.colliderDto = new ColliderDto();
                        dto.colliderDto.colliderBoxSize = (SerializableVector3)property.value;
                        Collider c = node.gameObject.GetComponent<Collider>();
                        if (c is BoxCollider)
                        {
                            (c as BoxCollider).size = (SerializableVector3)property.value;
                        }
                    }
                    break;
                case UMI3DPropertyKeys.ColliderHeight:
                    {
                        if (dto.colliderDto == null)
                            dto.colliderDto = new ColliderDto();
                        dto.colliderDto.colliderHeight = (float)(double)property.value;
                        Collider c = node.gameObject.GetComponent<Collider>();
                        if (c is CapsuleCollider)
                        {
                            (c as CapsuleCollider).height = (float)(double)property.value;
                        }
                    }
                    break;
                case UMI3DPropertyKeys.ColliderDirection:
                    {
                        if (dto.colliderDto == null)
                            dto.colliderDto = new ColliderDto();
                        dto.colliderDto.colliderDirection = (DirectionalType)(Int64)property.value;
                        Collider c = node.gameObject.GetComponent<Collider>();
                        if (c is CapsuleCollider)
                        {
                            (c as CapsuleCollider).direction = (int)((DirectionalType)(Int64)property.value);
                        }
                    }
                    break;
                case UMI3DPropertyKeys.IsMeshColliderCustom:
                    {
                        if (dto.colliderDto == null)
                            dto.colliderDto = new ColliderDto();
                        dto.colliderDto.isMeshCustom = (bool)property.value;
                        //  Collider c = node.gameObject.GetComponent<Collider>();
                        if ((dto.colliderDto.isMeshCustom && dto.colliderDto.customMeshCollider != null) || !dto.colliderDto.isMeshCustom)
                        {
                            SetCollider(node, dto.colliderDto);
                            // SetCustomCollider(node.gameObject, dto.colliderDto.customMeshCollider);
                        }
                        else if (!dto.colliderDto.isMeshCustom)
                        {
                            RemoveColliders(UMI3DEnvironmentLoader.GetNode(dto.id));

                        }
                    }
                    break;
                case UMI3DPropertyKeys.ColliderCustomResource:
                    {
                        if (dto.colliderDto == null)
                            dto.colliderDto = new ColliderDto();
                        dto.colliderDto.customMeshCollider = (ResourceDto)property.value;
                        // Collider c = node.gameObject.GetComponent<Collider>();
                        if (dto.colliderDto.isMeshCustom && dto.colliderDto.customMeshCollider != null)
                        {
                            SetCustomCollider(node.gameObject, dto.colliderDto.customMeshCollider);
                        }
                        else if (dto.colliderDto.isMeshCustom && dto.colliderDto.customMeshCollider == null)
                        {
                            //SetCustomCollider(node.gameObject, dto.colliderDto.customMeshCollider);
                            RemoveColliders(UMI3DEnvironmentLoader.GetNode(dto.id));
                        }
                    }
                    break;
                case UMI3DPropertyKeys.ColliderType:
                    {

                        if (dto.colliderDto == null)
                        {
                            dto.colliderDto = new ColliderDto();
                            dto.colliderDto.colliderType = (ColliderType)(Int64)property.value;
                        }
                        else
                        {
                            dto.colliderDto.colliderType = (ColliderType)(Int64)property.value;

                            SetCollider(node, dto.colliderDto);
                        }

                    }
                    break;
                case UMI3DPropertyKeys.HasCollider:
                    {
                        if ((bool)property.value)
                        {
                            if (dto.colliderDto == null)
                                dto.colliderDto = new ColliderDto();

                            SetCollider(node, dto.colliderDto);
                        }
                        else
                        {
                            RemoveColliders(node);
                        }
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }


        #region Collider
        protected void SetCustomCollider(GameObject node, ResourceDto resourceDto)
        {
            if (resourceDto == null) return;

            FileDto fileToLoad = UMI3DEnvironmentLoader.Parameters.ChooseVariante(resourceDto.variants);  // Peut etre ameliore
            if (fileToLoad == null) return;
            string url = fileToLoad.url;
            string ext = fileToLoad.extension;
            string authorization = fileToLoad.authorization;
            IResourcesLoader loader = UMI3DEnvironmentLoader.Parameters.SelectLoader(ext);
            if (loader != null)
                UMI3DResourcesManager.LoadFile(
                    url,
                    fileToLoad,
                    loader.UrlToObject,
                    loader.ObjectFromCache,
                    (o) =>
                    {
                        CallbackAfterLoadingCollider((GameObject)o, node.GetComponent<MeshCollider>());
                    },
                    Debug.LogWarning,
                    loader.DeleteObject
                    );
        }

        private void CallbackAfterLoadingCollider(GameObject modelCollider, MeshCollider collider)
        {
            collider.convex = false;
            var mesh = modelCollider.GetComponentInChildren<MeshFilter>();
            if (mesh != null)
                collider.sharedMesh = mesh.sharedMesh;
            else
            {
                Debug.LogError("Collider not found");
            }
        }

        private void RemoveColliders(UMI3DNodeInstance nodeInstance)
        {
            if (nodeInstance != null)
            {
                foreach (Collider item in nodeInstance.colliders)
                {
                    GameObject.Destroy(item);
                }
                nodeInstance.colliders.RemoveAll((x) => true);
            }
        }

        protected virtual void SetCollider(UMI3DNodeInstance nodeInstance, ColliderDto dto)
        {
            //UMI3DNodeInstance nodeInstance = UMI3DEnvironmentLoader.GetNode(pid);// go.GetComponent<UMI3DNodeInstance>();
            GameObject go = nodeInstance.gameObject;

            //Remove old oliders
            RemoveColliders(nodeInstance);

            if (dto == null)
                return;

            switch (dto.colliderType)
            {

                case ColliderType.Mesh:
                    if (dto.isMeshCustom)
                    {
                        try
                        {

                            MeshCollider mesh = go.AddComponent<MeshCollider>();

                            if (mesh.sharedMesh.isReadable)
                            {
                                mesh.convex = false;
                                SetCustomCollider(go, dto.customMeshCollider);
                                if (nodeInstance != null)
                                    nodeInstance.colliders.Add(mesh);
                                else
                                    Debug.LogWarning("This object has no UMI3DNodeInstance yet. Collider is not registered");
                            }
                            else
                                Debug.LogWarning("the mesh has been marked as non-accessible. Collider is not registered");
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"the mesh failed to be added, collider is not registered. Collider is not accessible [{e}]");
                        }

                    }
                    else
                    {
                        foreach (MeshFilter mesh in go.GetComponentsInChildren<MeshFilter>())
                        {
                            try
                            {


                                MeshCollider mc = mesh.gameObject.AddComponent<MeshCollider>();
                                mc.sharedMesh = mesh.sharedMesh;
                                mc.convex = dto.convex;
                                if (nodeInstance != null)
                                    nodeInstance.colliders.Add(mc);
                                else
                                    Debug.LogWarning("This object has no UMI3DNodeInstance yet. Collider is not registered");
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"the mesh failed to be added, collider is not registered. Collider is not accessible [{e}]");
                            }
                        }

                        foreach (SkinnedMeshRenderer mesh in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            try
                            {


                                MeshCollider mc = mesh.gameObject.AddComponent<MeshCollider>();
                                mc.sharedMesh = mesh.sharedMesh;
                                mc.convex = dto.convex;
                                if (nodeInstance != null)
                                    nodeInstance.colliders.Add(mc);
                                else
                                    Debug.LogWarning("This object has no UMI3DNodeInstance yet. Collider is not registered");
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"the mesh failed to be added, collider is not registered. Collider is not accessible [{e}]");
                            }
                        }
                    }
                    break;
                case ColliderType.Box:
                    BoxCollider bc = go.AddComponent<BoxCollider>();
                    bc.center = dto.colliderCenter;
                    bc.size = dto.colliderBoxSize;
                    if (nodeInstance != null)
                        nodeInstance.colliders.Add(bc);
                    else
                        Debug.LogWarning("This object has no UMI3DNodeInstance yet. Collider is not registered");
                    break;
                case ColliderType.Sphere:
                    SphereCollider sc = go.AddComponent<SphereCollider>();
                    sc.center = dto.colliderCenter;
                    sc.radius = dto.colliderRadius;
                    if (nodeInstance != null)
                        nodeInstance.colliders.Add(sc);
                    else
                        Debug.LogWarning("This object has no UMI3DNodeInstance yet. Collider is not registered");
                    break;
                case ColliderType.Capsule:
                    CapsuleCollider cc = go.AddComponent<CapsuleCollider>();
                    cc.center = dto.colliderCenter;
                    cc.radius = dto.colliderRadius;
                    cc.height = dto.colliderHeight;
                    cc.direction = (int)dto.colliderDirection;
                    if (nodeInstance != null)
                        nodeInstance.colliders.Add(cc);
                    else
                        Debug.LogWarning("This object has no UMI3DNodeInstance yet. Collider is not registered");
                    break;
                default:
                    break;
            }
        }
        #endregion


    }
}