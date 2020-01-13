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

namespace umi3d.cdk
{
    public class PrimitiveDtoLoader : AbstractObjectDTOLoader<PrimitiveDto>
    {

        public override void LoadDTO(PrimitiveDto dto, Action<GameObject> callback)
        {
            try
            {
                //Primitive creation
                GameObject res = null;
                switch (dto.Primitive)
                {
                    case (int)MeshPrimitive.Capsule:
                        res = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        break;
                    case (int)MeshPrimitive.Cube:
                        res = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        break;
                    case (int)MeshPrimitive.Cylinder:
                        res = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        var c = res.GetComponent<Collider>();
                        if (c != null)
                            Destroy(c);
                        var cc = res.AddComponent<MeshCollider>();
                        cc.convex = true;
                        break;
                    case (int)MeshPrimitive.Plane:
                        res = GameObject.CreatePrimitive(PrimitiveType.Plane);
                        Destroy( res.GetComponent<MeshCollider>());
                        res.AddComponent<BoxCollider>();
                        break;
                    case (int)MeshPrimitive.Sphere:
                        res = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        break;
                }

                //Collider creation
                CreateCollider(res, dto);


                if (res != null)
                {
                    var matdef = res.AddComponent<MeshMaterial>();
                    matdef.dtoid = dto.Id;

                    MaterialDtoLoader materialDTOLoader = GetComponentInParent<MaterialDtoLoader>();

                    materialDTOLoader.LoadDTO(dto.material, (MeshMaterial meshMaterial) =>
                    {
                        matdef.Set(meshMaterial);
                    });

                    if (dto.video != null)
                    {


                        var videoDef = res.gameObject.AddComponent<Video>();
                        videoDef.dtoid = dto.Id;
                        VideoDtoLoader videoDtoLoader = GetComponentInParent<VideoDtoLoader>();
                        videoDtoLoader.LoadDTO(dto.video, (Video video) =>
                        {
                            videoDef.Set(video);
                            videoDef.LoadVideo(res.GetComponent<MeshRenderer>());
                        });

                    }
                    matdef.LoadMaterial();
                }
                InitObjectFromDto(res, dto);
                callback(res);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }




        public override void UpdateFromDTO(GameObject go, PrimitiveDto olddto, PrimitiveDto newdto)
        {
            base.UpdateFromDTO(go, olddto, newdto);
            try
            {
                if ((olddto != null) && (newdto != null))
                {
                    if (newdto.colliderType != olddto.colliderType)
                    {
                        CreateCollider(go, newdto);
                    }
                }

                var mats = go.GetComponentsInChildren<MeshMaterial>(true).Where(mat => mat.dtoid == newdto.Id);
                foreach (MeshMaterial mat in mats)
                {
                    MaterialDtoLoader materialDTOLoader = GetComponentInParent<MaterialDtoLoader>();
                    materialDTOLoader.LoadDTO(newdto.material, (MeshMaterial meshMaterial) =>
                    {
                        mat.Set(meshMaterial);
                        mat.LoadMaterial();
                    });

                    var renderer = mat.gameObject.GetComponent<MeshRenderer>();
                    renderer.material = mat.material;

                    if (renderer != null)
                    {
                        renderer.enabled = !(mat.Transparent && mat.MainColor.a == 0);
                    }
                }
                if (newdto.video != null)
                {
                    var videos = go.GetComponentsInChildren<Video>(true).Where(mat => mat.dtoid == newdto.Id);
                    foreach (var video in videos)
                    {
                        var renderer = video.gameObject.GetComponent<MeshRenderer>();
                        VideoDtoLoader videoDTOLoader = GetComponentInParent<VideoDtoLoader>();
                        videoDTOLoader.UpdateFromDTO(video, (olddto != null)? olddto.video : null, newdto.video);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }
        }

        /// <summary>
        /// Create collider for a primitive
        /// </summary>
        /// <param name="go"></param>
        /// <param name="colType"></param>
        /// <param name="primitive"></param>
        private void CreateCollider(GameObject go, PrimitiveDto dto)
        {
            switch (dto.colliderType)
            {
                case ColliderType.None:
                    foreach (Collider col in go.GetComponents<Collider>())
                    {
                        Destroy(col);
                    }
                    break;

                case ColliderType.Auto:
                    switch ((MeshPrimitive) dto.Primitive)
                    {
                        case MeshPrimitive.Cylinder:

                            foreach (Collider col in go.GetComponents<Collider>())
                            {
                                Destroy(col);
                            }
                            var cc = go.AddComponent<MeshCollider>();
                            cc.convex = true;
                            break;

                        case MeshPrimitive.Plane:
                            foreach (Collider col in go.GetComponents<Collider>())
                            {
                                Destroy(col);
                            }
                            go.AddComponent<BoxCollider>();
                            break;

                        case MeshPrimitive.Cube:
                            foreach (Collider col in go.GetComponents<Collider>())
                            {
                                Destroy(col);
                            }
                            go.AddComponent<BoxCollider>();
                            break;

                        case MeshPrimitive.Capsule:
                            foreach (Collider col in go.GetComponents<Collider>())
                            {
                                Destroy(col);
                            }
                            go.AddComponent<CapsuleCollider>();
                            break;

                        case MeshPrimitive.Sphere:
                            foreach (Collider col in go.GetComponents<Collider>())
                            {
                                Destroy(col);
                            }
                            go.AddComponent<SphereCollider>();
                            break;
                    }
                    break;

                case ColliderType.Box:
                    foreach (Collider col in go.GetComponents<Collider>())
                    {
                        Destroy(col);
                    }
                    BoxCollider bc = go.AddComponent<BoxCollider>();
                    bc.size = go.GetComponent<MeshRenderer>().bounds.size;
                    break;

                case ColliderType.Sphere:
                    foreach (Collider col in go.GetComponents<Collider>())
                    {
                        Destroy(col);
                    }

                    SphereCollider sc = go.AddComponent<SphereCollider>();
                    break;

                case ColliderType.Capsule:
                    foreach (Collider col in go.GetComponents<Collider>())
                    {
                        Destroy(col);
                    }
                    go.AddComponent<CapsuleCollider>();
                    break;

                case ColliderType.Mesh:
                    foreach (Collider col in go.GetComponents<Collider>())
                    {
                        Destroy(col);
                    }
                    var mc = go.AddComponent<MeshCollider>();
                    mc.convex = dto.convex;
                    break;
            }
        }

    }
}