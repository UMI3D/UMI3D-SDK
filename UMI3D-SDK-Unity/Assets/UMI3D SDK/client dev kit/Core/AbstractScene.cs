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
using UnityEngine;
using umi3d.common;
using System.Collections;
using System.Collections.Generic;


namespace umi3d.cdk
{
    /// <summary>
    /// Abstract class for UMI3D scene.
    /// </summary>
    public abstract class AbstractScene : MonoBehaviour
    {
        /// <summary>
        /// Objects stored in the scene.
        /// </summary>
        private Dictionary<string, GameObject> objects;

        /// <summary>
        /// Objects stored in the scene dtos.
        /// </summary>
        private Dictionary<string, AbstractObject3DDto> dtos;
        
        /// <summary>
        /// Avatars in the scene.
        /// </summary>
        private Dictionary<string, AvatarDto> avatars;

        protected GenericObject3DDtoLoader genericObject3DDtoLoader;
        protected ARTrackerDtoLoader ARTrackerDtoLoader;
        protected LightDtoLoader lightDtoLoader;
        protected LineDtoLoader lineDtoLoader;
        protected ModelDtoLoader modelDtoLoader;
        protected AvatarPartDtoLoader avatarPartDtoLoader;
        protected PrimitiveDtoLoader primitiveDtoLoader;
        protected MaterialDtoLoader materialDtoLoader;
        protected VideoDtoLoader videoDtoLoader;
        protected AudioSourceDtoLoader audioSourceDtoLoader;
        protected UIRectDtoLoader uiRectDtoLoader;
        protected UICanvasDtoLoader uiCanvasDtoLoader;
        protected UITextDtoLoader uiTextDtoLoader;
        protected UIImageDtoLoader uiImageDtoLoader;
        protected CubeMapDtoLoader cubeMabDtoLoader;
        protected AvatarMappingDtoLoader userMappingDtoLoader;

        /// <summary>
        /// Is this device a full 3D media displayer (sush as Computer or Virtual reality headset).
        /// </summary>
        static public bool IsImmersiveDevice = true;
        [SerializeField]
        bool _IsImmersiveDevice = true;

        /// <summary>
        /// Skybox material.
        /// </summary>
        public Material skyboxMaterial;

        /// <summary>
        /// Default line shader.
        /// </summary>
        public Shader defaultLineShader;

        /// <summary>
        /// Default mesh shader.
        /// </summary>
        public Shader defaultMeshShader;


        // Use this for initialization
        protected void Start()
        {
            objects = new Dictionary<string, GameObject>();
            dtos = new Dictionary<string, AbstractObject3DDto>();
            avatars = new Dictionary<string, AvatarDto>();

            IsImmersiveDevice = _IsImmersiveDevice;



            //creating default loaders
            genericObject3DDtoLoader = GetOrAddComponent<GenericObject3DDtoLoader>();
            ARTrackerDtoLoader = GetOrAddComponent<ARTrackerDtoLoader>();
            lightDtoLoader = GetOrAddComponent<LightDtoLoader>();
            lineDtoLoader = GetOrAddComponent<LineDtoLoader>();
            modelDtoLoader = GetOrAddComponent<ModelDtoLoader>();
            avatarPartDtoLoader = GetOrAddComponent<AvatarPartDtoLoader>();
            primitiveDtoLoader = GetOrAddComponent<PrimitiveDtoLoader>();
            materialDtoLoader = GetOrAddComponent<MaterialDtoLoader>();
            videoDtoLoader = GetOrAddComponent<VideoDtoLoader>();
            audioSourceDtoLoader = GetOrAddComponent<AudioSourceDtoLoader>();
            uiRectDtoLoader = GetOrAddComponent<UIRectDtoLoader>();
            uiCanvasDtoLoader = GetOrAddComponent<UICanvasDtoLoader>();
            uiTextDtoLoader = GetOrAddComponent<UITextDtoLoader>();
            uiImageDtoLoader = GetOrAddComponent<UIImageDtoLoader>();
            cubeMabDtoLoader = GetOrAddComponent<CubeMapDtoLoader>();
            userMappingDtoLoader = GetOrAddComponent<AvatarMappingDtoLoader>();
        }

        protected A GetOrAddComponent<A>() where A : Component
        {
            var type = typeof(A);
            var _comp = GetComponent(type);
            if (_comp == null)
                _comp = gameObject.AddComponent(type);
            return _comp as A;
        }




        /// <summary>
        /// Load skybox
        /// </summary>
        /// <param name="id">Id of the object to get</param>
        public void SetSkybox(ResourceDto cubeMap)
        {
            if (skyboxMaterial != null)
                cubeMabDtoLoader.LoadDTO(cubeMap, (Cubemap result) => {
                    skyboxMaterial.SetTexture("_Tex", result);
                    RenderSettings.skybox = skyboxMaterial;
                });
        }

        /// <summary>
        /// Get scene object by id.
        /// </summary>
        /// <param name="id">Id of the object to get</param>
        public GameObject GetObject(string id)
        {
            return objects.ContainsKey(id) ? objects[id] : null;
        }

        /// <summary>
        /// Store a object dto in the local dto cache.
        /// </summary>
        /// <param name="id">dto id</param>
        /// <param name="dto">object dto to store</param>
        void CacheDto(string id, AbstractObject3DDto dto)
        {
            bool exists = dtos.ContainsKey(id);
            if (exists && dtos[id].Time <= dto.Time)
                dtos[id] = dto;
            else if (!exists)
                dtos.Add(id, dto);
        }

        /// <summary>
        /// Store an avatar in the local avatar cache.
        /// </summary>
        /// <param name="id">avatar id</param>
        /// <param name="dto">avatar to store</param>
        void CacheAvatars(string id, AvatarDto dto)
        {
            if (avatars.ContainsKey(id))
                avatars[id] = dto;
            else
                avatars.Add(id, dto);
        }

        /// <summary>
        /// Get object dto by id.
        /// </summary>
        /// <param name="id">id of the dto to get</param>
        public AbstractObject3DDto GetDto(string id)
        {
            return dtos.ContainsKey(id) ? dtos[id] : null;
        }

        /// <summary>
        /// Get avatar by id.
        /// </summary>
        /// <param name="id">id of the avatar to get</param>
        public AvatarDto GetAvatarDto(string id)
        {
            return avatars.ContainsKey(id) ? avatars[id] : null;
        }

        /// <summary>
        /// Reset scene.
        /// </summary>
        public virtual void ResetModule()
        {
            if (objects != null)
                objects.Clear();
            if (dtos != null)
                dtos.Clear();
            if (avatars != null)
                avatars.Clear();
            foreach (Transform t in gameObject.transform)
                Destroy(t.gameObject);
        }

        /// <summary>
        /// Set an object and its children layers recursively.
        /// </summary>
        /// <param name="obj">obj to set layer</param>
        /// <param name="layer">layer to set objects to</param>
        private static void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }


        /// <summary>
        /// Load objects to the scene.
        /// </summary>
        /// <param name="data"></param>
        public void Load(LoadDto data)
        {
            StartCoroutine(load(data));
            /*
            foreach (var obj in data.Entities)
            Load(obj);
            */
        }

        bool isloading = false;
        int countLoading = 0;
        int maxLoading = 200;
        /// <summary>
        /// Load objects to the scene.
        /// </summary>
        /// <param name="data"></param>
        IEnumerator load(LoadDto data)
        {
            while (isloading)
                yield return new WaitForEndOfFrame();
            isloading = true;
            foreach (var def in data.Entities)
            {
                bool finished = false;
                CacheDto(def.Id, def);
                Load(def, (GameObject result) =>
                {
                    if (result == null)
                    {
                        finished = true;
                        return;
                    }
                    if (objects.ContainsKey(def.Id))
                    {
                        Destroy(result);
                        finished = true;
                        return;
                    }
                    objects.Add(def.Id, result);
                    SetLayerRecursively(result, gameObject.layer);
                    finished = true;
                });
                while (!finished)
                    yield return new WaitForEndOfFrame();
            }
            foreach (var def in data.Entities)
            {
                countLoading++;
                UMI3DHttpClient.LoadSubObjects(def.Id, () => { UMI3DHttpClient.loadingObjectsCount--; countLoading--; });
                while (countLoading >= maxLoading)
                    yield return new WaitForEndOfFrame();
            }
            isloading = false;
            yield return null;
        }

        /// <summary>
        /// Load object from dto to the scene and pass the gameobject to a given callback.
        /// </summary>
        /// <param name="def">Dto to load from</param>
        /// <param name="callback">Callback to execute</param>
        protected void Load(AbstractObject3DDto def, Action<GameObject> callback)
        {
            if (def == null)
                return;

            else if (def is AvatarPartDto && (def as AvatarPartDto).UserId == UMI3DBrowser.UserId)
                avatarPartDtoLoader.LoadDTO(def as AvatarPartDto, callback);          

            else if (def is ModelDto)
                modelDtoLoader.LoadDTO(def as ModelDto, callback);

            else if (def is PrimitiveDto)
                primitiveDtoLoader.LoadDTO(def as PrimitiveDto, callback);

            else if (def is LightDto)
                lightDtoLoader.LoadDTO(def as LightDto, callback);

            else if (def is LineDto)
                lineDtoLoader.LoadDTO(def as LineDto, callback);

            else if (def is AudioSourceDto)
                audioSourceDtoLoader.LoadDTO(def as AudioSourceDto, callback);

            else if (def is UICanvasDto)
                uiCanvasDtoLoader.LoadDTO(def as UICanvasDto, callback);

            else if (def is UIImageDto)
                uiImageDtoLoader.LoadDTO(def as UIImageDto, callback);

            else if (def is UITextDto)
                uiTextDtoLoader.LoadDTO(def as UITextDto, callback);

            else if (def is UIRectDto)
                uiRectDtoLoader.LoadDTO(def as UIRectDto, callback);

            else if (def is GenericObject3DDto)
                genericObject3DDtoLoader.LoadDTO(def as GenericObject3DDto, callback);

            else if (def is AvatarMappingDto)
                userMappingDtoLoader.LoadDTO(def as AvatarMappingDto, callback);

            else
                Debug.LogError("Unsupported ObjectType: " + def.GetType());

            if (def != null)
                CacheDto(def.Id, def);
        }




        /// <summary>
        /// Update a scene object from dto.
        /// </summary>
        public void UpdateObject(UpdateObjectDto data)
        {
            if (data == null || data.Entity == null)
                return;
            var obj = GetObject(data.Entity.Id);
            var dto = GetDto(data.Entity.Id);
            if (dto != null)
            {
                if(obj != null && dto.Time <= data.Entity.Time)
                    UpdateObject(obj, dto, data.Entity);
                CacheDto(dto.Id, dto);
            }

        }

        /// <summary>
        /// Update a scene object from dto.
        /// </summary>
        /// <param name="go">Gameobject to update</param>
        /// <param name="olddto">previous dto</param>
        /// <param name="newdto">dto to update to</param>
        private void UpdateObject(GameObject go, AbstractObject3DDto olddto, AbstractObject3DDto newdto)
        {
            if (olddto == null || newdto == null)
                return;

            if (olddto.Time > newdto.Time)
                return;
            
            if (olddto is GenericObject3DDto && newdto is GenericObject3DDto)
                genericObject3DDtoLoader.UpdateFromDTO(go, olddto as GenericObject3DDto, newdto as GenericObject3DDto);

            else if (olddto is AvatarPartDto && newdto is AvatarPartDto && (newdto as AvatarPartDto).UserId == UMI3DBrowser.UserId)
                avatarPartDtoLoader.UpdateFromDTO(go, olddto as AvatarPartDto, newdto as AvatarPartDto);

            else if (olddto is ModelDto && newdto is ModelDto)
                modelDtoLoader.UpdateFromDTO(go, olddto as ModelDto, newdto as ModelDto);

            else if (olddto is PrimitiveDto && newdto is PrimitiveDto)
                primitiveDtoLoader.UpdateFromDTO(go, olddto as PrimitiveDto, newdto as PrimitiveDto);

            else if (olddto is LightDto && newdto is LightDto)
                lightDtoLoader.UpdateFromDTO(go, olddto as LightDto, newdto as LightDto);

            else if (olddto is LineDto && newdto is LineDto)
                lineDtoLoader.UpdateFromDTO(go, olddto as LineDto, newdto as LineDto);

            else if (olddto is AudioSourceDto && newdto is AudioSourceDto)
                audioSourceDtoLoader.UpdateFromDTO(go, olddto as AudioSourceDto, newdto as AudioSourceDto);

            else if (olddto is UIImageDto && newdto is UIImageDto)
                uiImageDtoLoader.UpdateFromDTO(go, olddto as UIImageDto, newdto as UIImageDto);

            else if (olddto is UITextDto && newdto is UITextDto)
                uiTextDtoLoader.UpdateFromDTO(go, olddto as UITextDto, newdto as UITextDto);

            else if (olddto is UICanvasDto && newdto is UICanvasDto)
                uiCanvasDtoLoader.UpdateFromDTO(go, olddto as UICanvasDto, newdto as UICanvasDto);

            else if (olddto is UIRectDto && newdto is UIRectDto)
                uiRectDtoLoader.UpdateFromDTO(go, olddto as UIRectDto, newdto as UIRectDto);

            else if (olddto is AvatarMappingDto && newdto is AvatarMappingDto)
                userMappingDtoLoader.UpdateFromDTO(go, olddto as AvatarMappingDto, newdto as AvatarMappingDto);

            CacheDto(newdto.Id, newdto);
        }



        /// <summary>
        /// Remove an object from the scene.
        /// </summary>
        /// <param name="data">Object to remove dto</param>
        public void Remove(RemoveObjectDto data)
        {
            if (data == null)
                return;
            RemoveObject(data.Id);
        }

        /// <summary>
        /// Remove an object from the scene.
        /// </summary>
        /// <param name="id">Object to remove id</param>
        public void RemoveObject(string id)
        {
                GameObject obj = id == null ? null : GetObject(id);
                RemoveObject(obj);
        }

        /// <summary>
        /// Remove object from the scene.
        /// </summary>
        /// <param name="obj">Object to remove</param>
        private void RemoveObject(GameObject obj)
        {
            if (obj != null)
            {
                foreach (Transform t in obj.transform)
                    RemoveObject(t.gameObject);
                string id = null;
                foreach (var entry in objects)
                {
                    if (entry.Value.Equals(obj))
                    {
                        id = entry.Key;
                        break;
                    }
                }
                if (id != null)
                    objects.Remove(id);
                if (id != null && dtos.ContainsKey(id))
                    dtos.Remove(id);

                Destroy(obj);
            }
        }

        /// <summary>
        /// Update Media from dto.
        /// </summary>
        /// <param name="dto">dto describing the object</param>
        public abstract void UpdateFromDTO(MediaUpdateDto dto);

    }
}