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
using UnityEngine.Events;

namespace umi3d.cdk
{
    /// <summary>
    /// Abstract class for UMI3D scene.
    /// </summary>
    public abstract class AbstractScene : MonoBehaviour
    {

        [System.Serializable]
        public class LoadEvent : UnityEvent<EmptyObject3DDto> { }
        public LoadEvent onObjectLoaded = new LoadEvent();

        /// <summary>
        /// Objects stored in the scene.
        /// </summary>
        private Dictionary<string, GameObject> objects;

        /// <summary>
        /// Objects stored in the scene dtos.
        /// </summary>
        private Dictionary<string, EmptyObject3DDto> dtos;

        /// <summary>
        /// Avatars in the scene.
        /// </summary>
        private Dictionary<string, AvatarDto> avatars;

        protected EmptyObject3DDTOLoader emptyObject3DDtoLoader;
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

        /// <summary>
        /// Is this device a full 3D media displayer (sush as Computer or Virtual reality headset).
        /// </summary>
        static public bool isImmersiveDevice = true;
        [SerializeField]
        bool _isImmersiveDevice = true;

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

        /// <summary>
        /// True if the first scene load has been done.
        /// </summary>
        private bool firstLoadDone = false;

        /// <summary>
        /// Event raised when the scene has been fully loaded.
        /// </summary>
        public UnityEvent onSceneLoaded = new UnityEvent();

        public void SendOnSceneLoaded()
        {
            onSceneLoaded.Invoke();
        }

        // Use this for initialization
        protected void Start()
        {
            objects = new Dictionary<string, GameObject>();
            dtos = new Dictionary<string, EmptyObject3DDto>();
            avatars = new Dictionary<string, AvatarDto>();

            isImmersiveDevice = _isImmersiveDevice;

            firstLoadDone = false;


            //creating default loaders
            emptyObject3DDtoLoader = GetOrAddComponent<EmptyObject3DDTOLoader>();
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
        /// Load skybox.
        /// </summary>
        /// <param name="id">Id of the object to get</param>
        public void SetSkybox(ResourceDto cubeMap)
        {
            if (skyboxMaterial != null)
                cubeMabDtoLoader.LoadDTO(cubeMap, (Cubemap result) => {
                    skyboxMaterial.SetTexture("_Tex", result);
                    RenderSettings.skybox = skyboxMaterial;
                }, 
                (e) => Debug.LogError("Failed to load skybox ("+e+")"));
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
        void CacheDto(string id, EmptyObject3DDto dto)
        {
            bool exists = dtos.ContainsKey(id);
            if (exists && dtos[id].time <= dto.time)
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
        public EmptyObject3DDto GetDto(string id)
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
            firstLoadDone = false;
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
        public void Load(LoadDto data, Action<IEnumerable<GameObject>> onSucess, Action<string> onFailure)
        {
            StartCoroutine(load(data, onSucess, onFailure));
        }

        int countLoading = 0;
        int maxLoading = 200;
        /// <summary>
        /// Load objects to the scene.
        /// </summary>
        /// <param name="data"></param>
        IEnumerator load(LoadDto data, Action<IEnumerable<GameObject>> onSucess, Action<string> onFailure)
        {
            int innerCountLoading = 0;
            
            List<GameObject> loadedObjects = new List<GameObject>();
            List<EmptyObject3DDto> loadedObjectsDto = new List<EmptyObject3DDto>();
            List<EmptyObject3DDto> failedObjectsDto = new List<EmptyObject3DDto>();
            List<EmptyObject3DDto> canceledObjectsDto = new List<EmptyObject3DDto>();

            foreach (var def in data.Entities)
            {
                CacheDto(def.id, def);
                Load(def, (GameObject result) =>
                {
                    if (result == null)
                    {
                        canceledObjectsDto.Add(def);
                        return;
                    }
                    if (objects.ContainsKey(def.id))
                    {
                        canceledObjectsDto.Add(def);
                        Destroy(result);
                        return;
                    }
                    objects.Add(def.id, result);
                    SetLayerRecursively(result, gameObject.layer);
                    onObjectLoaded.Invoke(def);
                    loadedObjects.Add(result);
                    loadedObjectsDto.Add(def);
                },
                (e) =>
                {
                    Debug.LogError("Failed to load object : " + def.name + "(id : " + def.id+" "+e+") ");
                    failedObjectsDto.Add(def);
                });

            }

            while (loadedObjectsDto.Count + failedObjectsDto.Count + canceledObjectsDto.Count < data.Entities.Count)
            {
                yield return new WaitForEndOfFrame();
            }

            foreach (var def in loadedObjectsDto)
            {
                countLoading++;
                innerCountLoading++;
                UMI3DHttpClient.LoadSubObjects(
                    def.id,
                    list => 
                    {
                        countLoading--;
                        innerCountLoading--;
                        loadedObjects.AddRange(list);
                    },
                    (e) =>
                    {
                        countLoading--;
                        innerCountLoading--;
                        Debug.LogError("Failed to load sub object : " + def.name + "(id : " + def.id + " "+e+")");
                    });
                while (countLoading >= maxLoading)
                {
                    yield return new WaitForEndOfFrame();
                }
            }

            foreach (var def in failedObjectsDto)
            {
                //TODO
                onFailure.Invoke("fail to load obj dto");

                yield break;
            }


            while (innerCountLoading > 0)
            {
                yield return new WaitForEndOfFrame();
            }
            onSucess.Invoke(loadedObjects);

        }

        /// <summary>
        /// Load object from dto to the scene and pass the gameobject to a given callback.
        /// </summary>
        /// <param name="def">Dto to load from</param>
        /// <param name="onSuccess">Callback to execute</param>
        protected void Load(EmptyObject3DDto def, Action<GameObject> onSuccess, Action<string> onError)
        {
            if (def == null)
                return;
            else
                CacheDto(def.id, def);

            if (def is AvatarPartDto && (def as AvatarPartDto).UserId == UMI3DBrowser.UserId)
                avatarPartDtoLoader.LoadDTO(def as AvatarPartDto, onSuccess, onError);

            else if (def is ModelDto)
                modelDtoLoader.LoadDTO(def as ModelDto, onSuccess, onError);

            else if (def is PrimitiveDto)
                primitiveDtoLoader.LoadDTO(def as PrimitiveDto, onSuccess, onError);

            else if (def is LightDto)
                lightDtoLoader.LoadDTO(def as LightDto, onSuccess, onError);

            else if (def is LineDto)
                lineDtoLoader.LoadDTO(def as LineDto, onSuccess, onError);

            else if (def is AudioSourceDto)
                audioSourceDtoLoader.LoadDTO(def as AudioSourceDto, onSuccess, onError);

            else if (def is UICanvasDto)
                uiCanvasDtoLoader.LoadDTO(def as UICanvasDto, onSuccess, onError);

            else if (def is UIImageDto)
                uiImageDtoLoader.LoadDTO(def as UIImageDto, onSuccess, onError);

            else if (def is UITextDto)
                uiTextDtoLoader.LoadDTO(def as UITextDto, onSuccess, onError);

            else if (def is UIRectDto)
                uiRectDtoLoader.LoadDTO(def as UIRectDto, onSuccess, onError);

            else if (def is EmptyObject3DDto)
                emptyObject3DDtoLoader.LoadDTO(def as EmptyObject3DDto, onSuccess, onError);

            else
            {
                Debug.LogError("Unsupported ObjectType: " + def.GetType());
                onError("Unsupported ObjectType: " + def.GetType());
            }

        }




        /// <summary>
        /// Update a scene object from dto.
        /// </summary>
        public void UpdateObject(UpdateObjectDto data)
        {
            if (data == null || data.Entity == null)
                return;
            var obj = GetObject(data.Entity.id);
            var dto = GetDto(data.Entity.id);
            if (dto != null)
            {
                if (obj != null && dto.time <= data.Entity.time)
                    UpdateObject(obj, dto, data.Entity);
                CacheDto(dto.id, dto);
            }

        }

        /// <summary>
        /// Update a scene object from dto.
        /// </summary>
        /// <param name="go">Gameobject to update</param>
        /// <param name="olddto">previous dto</param>
        /// <param name="newdto">dto to update to</param>
        private void UpdateObject(GameObject go, EmptyObject3DDto olddto, EmptyObject3DDto newdto)
        {
            if (olddto == null || newdto == null)
                return;

            if (olddto.time > newdto.time)
                return;


            if (olddto is AvatarPartDto && newdto is AvatarPartDto && (newdto as AvatarPartDto).UserId == UMI3DBrowser.UserId)
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

            else if (olddto is EmptyObject3DDto && newdto is EmptyObject3DDto)
                emptyObject3DDtoLoader.UpdateFromDTO(go, olddto as EmptyObject3DDto, newdto as EmptyObject3DDto);

            CacheDto(newdto.id, newdto);
        }



        /// <summary>
        /// Remove an object from the scene.
        /// </summary>
        /// <param name="data">Object to remove dto</param>
        public void Remove(RemoveObjectDto data)
        {
            if (data == null)
                return;
            RemoveObject(data.id);
        }

        /// <summary>
        /// Remove an object from the scene.
        /// </summary>
        /// <param name="id">Object to remove id</param>
        public void RemoveObject(string id)
        {
            GameObject obj = id == null ? null : GetObject(id);

            if (obj != null)
            {
                string interId = GetDto(id)?.interactable?.id;
                if (interId != null)
                    AbstractInteractionMapper.Instance.DeleteTool(interId);

                RemoveObject(obj);
            }
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