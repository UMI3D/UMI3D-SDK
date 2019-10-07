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
using System.Linq;
using umi3d.common;
using UnityEngine;
using System.Collections.Generic;

namespace umi3d.cdk
{
    /// <summary>
    /// Loads 3D Models from Dto.
    /// </summary>
    public class ModelDtoLoader : AbstractObjectDTOLoader<ModelDto>
    {

        /// <summary>
        /// Load 3DModel from Dto.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="callback"></param>
        public override void LoadDTO(ModelDto dto, Action<GameObject> callback)
        {
            try
            {                
                Resource resource = new Resource();
                resource.Set(dto.objResource);

                GameObject obj = new GameObject();
                HDResourceCache.Download(resource, obj.transform, ( GameObject model) => {

                    var renderers = model.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var r in renderers)
                    {
                        var matdef = r.gameObject.AddComponent<MeshMaterial>();
                        matdef.dtoid = dto.Id;
                        matdef.previousMaterial = r.sharedMaterial;

                        MaterialDtoLoader materialDTOLoader = GetComponentInParent<MaterialDtoLoader>();

                        materialDTOLoader.LoadDTO(dto.material, (MeshMaterial meshMaterial) =>
                        {
                            matdef.Set(meshMaterial);
                        });

                        matdef.LoadMaterial();

                        if (dto.video != null)
                        {
                            var videoDef = r.gameObject.AddComponent<Video>();
                            VideoDtoLoader videoDtoLoader = GetComponentInParent<VideoDtoLoader>();
                            videoDtoLoader.LoadDTO(dto.video, (Video video) => {
                                videoDef.Set(video);
                                videoDef.LoadVideo(r.GetComponent<MeshRenderer>());
                            });
                        }
                    }

                    obj.SetActive(true);
                    callback(obj);
                    InitObjectFromDto(obj, dto);
                });


            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        /// <summary>
        /// Compute the bouding box around a collection of MeshRenderers.
        /// </summary>
        /// <param name="renderers"></param>
        /// <returns></returns>
        private Bounds ComputeBoundingBox(IEnumerable<MeshRenderer> renderers)
        {
            bool first = false;
            Vector3 globalMin = Vector3.zero;
            Vector3 globalMax = Vector3.zero;
            float nb = 0;

            foreach (MeshRenderer mrnd in renderers)
            {
                nb = nb + 1f;
                Bounds bounds = mrnd.bounds;
                if (!first)
                {
                    globalMin = bounds.min;
                    globalMax = bounds.max;
                    first = true;
                }
                else
                {
                    float localMinX = bounds.min.x;
                    float localMinY = bounds.min.y;
                    float localMinZ = bounds.min.z;

                    if (localMinX < globalMin.x)
                        globalMin.x = localMinX;
                    if (localMinY < globalMin.y)
                        globalMin.y = localMinY;
                    if (localMinZ < globalMin.z)
                        globalMin.z = localMinZ;

                    float localMaxX = bounds.max.x;
                    float localMaxY = bounds.max.y;
                    float localMaxZ = bounds.max.z;

                    if (localMaxX > globalMax.x)
                        globalMax.x = localMaxX;
                    if (localMaxY > globalMax.y)
                        globalMax.y = localMaxY;
                    if (localMaxZ > globalMax.z)
                        globalMax.z = localMaxZ;
                }

            }

            return new Bounds((globalMin + globalMax) / 2f, globalMax - globalMin);
        }

        /// <summary>
        /// Update model from dto.
        /// </summary>
        /// <param name="go">Model to update</param>
        /// <param name="olddto">Old dto</param>
        /// <param name="newdto">New dto to update to</param>
        public override void UpdateFromDTO(GameObject go, ModelDto olddto, ModelDto newdto)
        {
            base.UpdateFromDTO(go, olddto, newdto);
            try
            {
                IEnumerable<MeshMaterial> mats = go.GetComponentsInChildren<MeshMaterial>(true).Where(mat => mat.dtoid == newdto.Id);
                foreach (var mat in mats)
                {
                    var renderer = mat.gameObject.GetComponent<MeshRenderer>();
                    if (newdto.OverrideModelMaterial)
                    {
                        MaterialDtoLoader materialDTOLoader = GetComponentInParent<MaterialDtoLoader>();
                        materialDTOLoader.LoadDTO(newdto.material, (MeshMaterial meshMaterial) =>
                        {
                            mat.Set(meshMaterial);
                        });


                        mat.material = mat.LoadMaterial();

                        if (renderer != null && (newdto.OverrideModelMaterial))
                        {
                            renderer.enabled = !(mat.Transparent && mat.MainColor.a == 0);
                        }

                        renderer.material = mat.material;
                    }
                    else
                    {
                        renderer.material = mat.previousMaterial;
                    }
                }


                if (newdto.video != null)
                {
                    var videos = go.GetComponentsInChildren<Video>(true).Where(mat => mat.dtoid == newdto.Id);
                    foreach (var video in videos)
                    {
                        var renderer = video.gameObject.GetComponent<MeshRenderer>();
                        VideoDtoLoader VideoDTOLoader = GetComponentInParent<VideoDtoLoader>();
                        VideoDTOLoader.UpdateFromDTO(video, (olddto != null) ? olddto.video : null, newdto.video);
                    }
                }


                //Update collider if needed
                if ((olddto == null) || (newdto.colliderType != olddto.colliderType))
                {
                    //local variables
                    Collider c;
                    Quaternion rotation;
                    Bounds boundingBox;
                    List<MeshRenderer> meshChildren;

                    switch (newdto.colliderType)
                    {
                        case ColliderType.None:
                            #region None

                            c = go.GetComponent<Collider>();
                            if (c != null)
                                Destroy(c);
                            foreach (MeshMaterial mat in mats)
                            {
                                c = mat.GetComponent<Collider>();
                                if (c != null)
                                    Destroy(c);
                            }
                            #endregion
                            break;
                        case ColliderType.Auto:
                        case ColliderType.Box:
                            #region Box

                            //Destroy existing colliders
                            c = go.GetComponent<Collider>();
                            if (c != null)
                                Destroy(c);
                            foreach (MeshMaterial mat in mats)
                            {
                                c = mat.GetComponent<Collider>();
                                if (c != null)
                                    Destroy(c);
                            }

                            //Temporarily fix rotation to (0,0,0) to simplify calculations
                            rotation = go.transform.rotation;
                            go.transform.rotation = Quaternion.Euler(0, 0, 0);

                            //Compute global bounding box 
                            meshChildren = new List<MeshRenderer>();
                            foreach (var mat in mats)
                                meshChildren.Add(mat.GetComponent<MeshRenderer>());
                            boundingBox = ComputeBoundingBox(meshChildren);

                            //Create collider
                            BoxCollider bc = go.AddComponent<BoxCollider>();
                            bc.size = go.transform.InverseTransformVector(boundingBox.size);
                            bc.center = go.transform.InverseTransformPoint(boundingBox.center);

                            //Restore original rotation
                            go.transform.rotation = rotation;
                            #endregion
                            break;
                        case ColliderType.Sphere:
                            #region Sphere

                            //Destroy existing colliders
                            c = go.GetComponent<Collider>();
                            if (c != null)
                                Destroy(c);
                            foreach (MeshMaterial mat in mats)
                            {
                                c = mat.GetComponent<Collider>();
                                if (c != null)
                                    Destroy(c);
                            }

                            //Temporarily fix rotation to (0,0,0) and scale to (1,1,1) to simplify calculations
                            rotation = go.transform.rotation;
                            go.transform.rotation = Quaternion.Euler(0, 0, 0);

                            //Compute global bounding box 
                            meshChildren = new List<MeshRenderer>();
                            foreach (var mat in mats)
                                meshChildren.Add(mat.GetComponent<MeshRenderer>());
                            boundingBox = ComputeBoundingBox(meshChildren);

                            //Create collider
                            SphereCollider sc = go.AddComponent<SphereCollider>();
                            sc.radius = Mathf.Max(boundingBox.size.x, Mathf.Max(boundingBox.size.y, boundingBox.size.z)) /
                                        Mathf.Max(go.transform.lossyScale.x, Mathf.Max(go.transform.lossyScale.y, go.transform.lossyScale.z)) / 2f;
                            sc.center = go.transform.InverseTransformPoint(boundingBox.center);


                            //Restore original rotation
                            go.transform.rotation = rotation;
                            #endregion
                            break;

                        case ColliderType.Capsule:
                            #region Capsule

                            //Destroy existing colliders
                            c = go.GetComponent<Collider>();
                            if (c != null)
                                Destroy(c);
                            foreach (MeshMaterial mat in mats)
                            {
                                c = mat.GetComponent<Collider>();
                                if (c != null)
                                    Destroy(c);
                            }

                            //Temporarily fix rotation to (0,0,0) to simplify calculations
                            rotation = go.transform.rotation;
                            go.transform.rotation = Quaternion.Euler(0, 0, 0);

                            //Compute global bounding box 
                            meshChildren = new List<MeshRenderer>();
                            foreach (var mat in mats)
                                meshChildren.Add(mat.GetComponent<MeshRenderer>());
                            boundingBox = ComputeBoundingBox(meshChildren);

                            //Create collider
                            CapsuleCollider cc = go.AddComponent<CapsuleCollider>();
                            cc.center = go.transform.InverseTransformPoint(boundingBox.center);

                            if ((boundingBox.size.x >= boundingBox.size.y) && (boundingBox.size.x >= boundingBox.size.z))
                            {
                                cc.direction = 0;
                                cc.height = boundingBox.size.x / go.transform.lossyScale.x;

                                if (boundingBox.size.y >= boundingBox.size.z)
                                {
                                    cc.radius = boundingBox.size.y / go.transform.lossyScale.y / 2f;
                                }
                                else
                                {
                                    cc.radius = boundingBox.size.z / go.transform.lossyScale.z / 2f;
                                }
                            }

                            if ((boundingBox.size.y >= boundingBox.size.x) && (boundingBox.size.y >= boundingBox.size.z))
                            {
                                cc.direction = 1;
                                cc.height = boundingBox.size.y / go.transform.lossyScale.y;

                                if (boundingBox.size.x >= boundingBox.size.z)
                                {
                                    cc.radius = boundingBox.size.x / go.transform.lossyScale.x / 2f;
                                }
                                else
                                {
                                    cc.radius = boundingBox.size.z / go.transform.lossyScale.z / 2f;
                                }
                            }

                            if ((boundingBox.size.z >= boundingBox.size.x) && (boundingBox.size.z >= boundingBox.size.y))
                            {
                                cc.direction = 2;
                                cc.height = boundingBox.size.z / go.transform.lossyScale.z;

                                if (boundingBox.size.x >= boundingBox.size.y)
                                {
                                    cc.radius = boundingBox.size.x / go.transform.lossyScale.x / 2f;
                                }
                                else
                                {
                                    cc.radius = boundingBox.size.y / go.transform.lossyScale.y / 2f;
                                }
                            }


                            //Restore original rotation
                            go.transform.rotation = rotation;
                            #endregion
                            break;

                        case ColliderType.Mesh:
                            #region Mesh

                            //Destroy existing colliders
                            c = go.GetComponent<Collider>();
                            if (c != null)
                                Destroy(c);
                            foreach (MeshMaterial mat in mats)
                            {
                                c = mat.GetComponent<Collider>();
                                if (c != null)
                                    Destroy(c);
                            }

                            //Create mesh colliders
                            foreach (MeshMaterial m in mats)
                            {
                                var mc = m.gameObject.AddComponent<MeshCollider>();
                                mc.convex = newdto.convex;
                            }
                            #endregion
                            break;
                    }
                }
                

            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }
        }

    }
}