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
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace umi3d.cdk
{
    /// <summary>
    /// 
    /// </summary>
    public class UMI3DEnvironmentLoader : Singleton<UMI3DEnvironmentLoader>
    {

        /// <summary>
        /// Index of any 3D object loaded.
        /// </summary>
        Dictionary<ulong, UMI3DEntityInstance> entities = new Dictionary<ulong, UMI3DEntityInstance>();

        /// <summary>
        /// Return a list of all registered entities.
        /// </summary>
        /// <returns></returns>
        public static List<UMI3DEntityInstance> Entities() { return Exists ? Instance.entities.Values.ToList() : null; }

        /// <summary>
        /// Get an entity with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public static UMI3DEntityInstance GetEntity(ulong id) { return id != 0 && Exists && Instance.entities.ContainsKey(id) ? Instance.entities[id] : null; }

        /// <summary>
        /// Get a node with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public static UMI3DNodeInstance GetNode(ulong id) { return id != 0 && Exists && Instance.entities.ContainsKey(id) ? Instance.entities[id] as UMI3DNodeInstance : null; }

        /// <summary>
        /// Get a node id with a collider.
        /// </summary>
        /// <param name="collider">collider.</param>
        /// <returns></returns>
        public static ulong GetNodeID(Collider collider) { return Exists ? Instance.entities.Where(k => k.Value is UMI3DNodeInstance).FirstOrDefault(k => (k.Value as UMI3DNodeInstance).colliders.Any(c => c == collider)).Key : 0; }

        /// <summary>
        /// Register a node instance.
        /// </summary>
        /// <param name="id">unique id of the node.</param>
        /// <param name="dto">dto of the node.</param>
        /// <param name="instance">gameobject of the node.</param>
        /// <returns></returns>
        public static UMI3DNodeInstance RegisterNodeInstance(ulong id, UMI3DDto dto, GameObject instance, Action delete = null)
        {
            if (!Exists || instance == null)
                return null;
            else if (Instance.entities.ContainsKey(id))
            {
                UMI3DNodeInstance node = Instance.entities[id] as UMI3DNodeInstance;
                if (node == null)
                    throw new Exception($"id:{id} found but the value was of type {Instance.entities[id].GetType()}");
                if (node.gameObject != instance)
                    Destroy(instance);
                return node;
            }
            else
            {
                UMI3DNodeInstance node = new UMI3DNodeInstance() { gameObject = instance, dto = dto, Delete = delete };
                Instance.entities.Add(id, node);
                return node;
            }
        }

        /// <summary>
        /// Register an entity without a gameobject.
        /// </summary>
        /// <param name="id">unique id of the node.</param>
        /// <param name="dto">dto of the node.</param>
        /// <returns></returns>
        public static UMI3DEntityInstance RegisterEntityInstance(ulong id, UMI3DDto dto, object Object, Action delete = null)
        {
            if (!Exists)
                return null;
            else if (Instance.entities.ContainsKey(id))
            {
                return Instance.entities[id];
            }
            else
            {
                UMI3DEntityInstance node = new UMI3DEntityInstance() { dto = dto, Object = Object, Delete = delete };
                Instance.entities.Add(id, node);
                return node;
            }
        }

        /// <summary>
        /// Index of any 3D object loaded.
        /// </summary>
        GlTFEnvironmentDto environment;

        /// <summary>
        /// Number of UMI3D nodes.
        /// = Number of scenes + Number of glTF nodes
        /// </summary>
        int nodesToInstantiate = 0;

        /// <summary>
        /// Number of UMI3D nodes.
        /// = Number of scenes + Number of glTF nodes
        /// </summary>
        int instantiatedNodes = 0;

        /// <summary>
        /// Number of UMI3D nodes.
        /// = Number of scenes + Number of glTF nodes
        /// </summary>
        int resourcesToLoad = 0;

        /// <summary>
        /// Number of loaded resources.
        /// </summary>
        int loadedResources = 0;

        public UMI3DSceneLoader sceneLoader { get; private set; }
        public GlTFNodeLoader nodeLoader { get; private set; }

        /// <summary>
        /// Basic material used to init a new material with the same properties
        /// </summary>
        public Material baseMaterial;

        /// <summary>
        /// return a copy of the baseMaterial. it can be modified 
        /// </summary>
        /// <returns></returns>
        public Material GetBaseMaterial()
        {
            //Debug.Log("GetBaseMaterial");
            if (baseMaterial == null)
                return null;
            return new Material(baseMaterial);
        }

        /// <summary>
        /// wait initialization of baseMaterial then invoke callback
        /// </summary>
        /// <param name="callback">callback to invoke with the baseMaterial in argument</param>
        /// <returns></returns>
        public IEnumerator GetBaseMaterialBeforeAction(Action<Material> callback)
        {
            yield return new WaitWhile(() => baseMaterial == null);

            callback.Invoke(new Material(baseMaterial));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newBaseMat">A new material to override the baseMaterial used to initialise all materials</param>
        public void SetBaseMaterial(Material newBaseMat) { baseMaterial = new Material(newBaseMat); }

        ///<inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            sceneLoader = new UMI3DSceneLoader(this);
            nodeLoader = new GlTFNodeLoader();
        }

        #region workflow

        /// <summary>
        /// Indicates if a UMI3D environment has been loaded
        /// </summary>
        public bool started { get; private set; } = false;

        /// <summary>
        /// Indicates if the UMI3D environment's resources has been loaded
        /// </summary>
        public bool downloaded { get; private set; } = false;

        /// <summary>
        /// Indicates if the UMI3D environment has been fully loaded
        /// </summary>
        public bool loaded { get; private set; } = false;

        [System.Serializable]
        public class ProgressListener : UnityEvent<float> { }
        public ProgressListener onProgressChange = new ProgressListener();

        public UnityEvent onResourcesLoaded = new UnityEvent();
        public UnityEvent onEnvironmentLoaded = new UnityEvent();

        /// <summary>
        /// Load the Environment.
        /// </summary>
        /// <param name="dto">Dto of the environement.</param>
        /// <param name="onSuccess">Finished callback.</param>
        /// <param name="onError">Error callback.</param>
        /// <returns></returns>
        public IEnumerator Load(GlTFEnvironmentDto dto, Action onSuccess, Action<string> onError)
        {
            environment = dto;
            RegisterEntityInstance(UMI3DGlobalID.EnvironementId, dto, null);
            nodesToInstantiate = dto.scenes.Count;
            foreach (GlTFSceneDto sce in dto.scenes)
                nodesToInstantiate += sce.nodes.Count;

            //
            // Load resources
            //
            StartCoroutine(LoadResources(dto));
            while (!downloaded)
            {
                onProgressChange.Invoke(resourcesToLoad == 0 ? 1f : loadedResources / resourcesToLoad);
                yield return new WaitForEndOfFrame();
            }
            onProgressChange.Invoke(1f);
            onResourcesLoaded.Invoke();
            //
            // Instantiate nodes
            //

            ReadUMI3DExtension(dto, null);

            onProgressChange.Invoke(0f);
            InstantiateNodes();
            while (!loaded)
            {
                onProgressChange.Invoke(nodesToInstantiate == 0 ? 1f : instantiatedNodes / nodesToInstantiate);
                yield return new WaitForEndOfFrame();
            }
            onProgressChange.Invoke(1f);
            onEnvironmentLoaded.Invoke();
            yield return null;
            onSuccess.Invoke();
        }

        #endregion

        #region resources

        /// <summary>
        /// Load the environment's resources
        /// </summary>
        IEnumerator LoadResources(GlTFEnvironmentDto dto)
        {
            started = true;
            downloaded = false;
            List<string> ids = dto.extensions.umi3d.LibrariesId;
            foreach (var scene in dto.scenes)
                ids.AddRange(scene.extensions.umi3d.LibrariesId);
            yield return StartCoroutine(UMI3DResourcesManager.LoadLibraries(ids, (i) => { loadedResources = i; }, (i) => { resourcesToLoad = i; }));
            downloaded = true;
        }

        #endregion

        #region parameters

        [SerializeField]
        private AbstractUMI3DLoadingParameters parameters = null;
        public static AbstractUMI3DLoadingParameters Parameters { get { return Exists ? Instance.parameters : null; } }

        #endregion

        #region instantiation

        /// <summary>
        /// Load the environment's resources
        /// </summary>
        void InstantiateNodes()
        {
            Action finished = () => { loaded = true; };
            StartCoroutine(_InstantiateNodes(environment.scenes, finished));
        }

        /// <summary>
        /// Load scenes 
        /// </summary>
        /// <param name="scenes">scenes to loads</param>
        /// <returns></returns>
        IEnumerator _InstantiateNodes(List<GlTFSceneDto> scenes, Action finished)
        {
            //Load scenes without hierarchy
            foreach (var scene in scenes)
            {
                bool isFinished = false;
                sceneLoader.LoadGlTFScene(scene, () => isFinished = true, (i) => instantiatedNodes = i); ;
                yield return new WaitUntil(() => isFinished == true);
            }

            int count = 0;
            //Organize scenes
            foreach (var scene in scenes)
            {
                count += 1;
                UMI3DNodeInstance node = entities[scene.extensions.umi3d.id] as UMI3DNodeInstance;
                UMI3DSceneNodeDto umi3dScene = scene.extensions.umi3d;
                sceneLoader.ReadUMI3DExtension(umi3dScene, node.gameObject, () => { count -= 1; instantiatedNodes += 1; }, (s) => { count -= 1; Debug.LogWarning(s); });
                node.gameObject.SetActive(true);
            }
            yield return new WaitUntil(() => count <= 0);
            finished.Invoke();
            yield return null;
        }

        #endregion


        /// <summary>
        /// Load IEntity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="performed"></param>
        static public void LoadEntity(IEntity entity, Action performed)
        {
            if (Exists) Instance._LoadEntity(entity, performed);
        }

        static public void LoadEntity(ByteContainer container, Action performed)
        {
            if (Exists) Instance._LoadEntity(container, performed);
        }

        /// <summary>
        /// Load IEntity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="performed"></param>
        void _LoadEntity(IEntity entity, Action performed)
        {
            switch (entity)
            {
                case GlTFSceneDto scene:
                    StartCoroutine(_InstantiateNodes(new List<GlTFSceneDto>() { scene }, performed));
                    break;
                case GlTFNodeDto node:
                    StartCoroutine(nodeLoader.LoadNodes(new List<GlTFNodeDto>() { node }, performed));
                    break;
                case AssetLibraryDto library:
                    UMI3DResourcesManager.DownloadLibrary(library,
                        UMI3DClientServer.Media.name,
                        () =>
                        {
                            UMI3DResourcesManager.LoadLibrary(library.libraryId, performed);
                        });
                    break;
                case AbstractEntityDto dto:
                    Parameters.ReadUMI3DExtension(dto, null, performed, (s) => { Debug.Log(s); performed.Invoke(); });
                    break;
                case GlTFMaterialDto matDto:
                    Parameters.SelectMaterialLoader(matDto).LoadMaterialFromExtension(matDto, (m) =>
                    {

                        if (matDto.name != null && matDto.name.Length > 0)
                            m.name = matDto.name;
                        //register the material
                        RegisterEntityInstance(((AbstractEntityDto)matDto.extensions.umi3d).id, matDto, m);
                        performed.Invoke();
                    });
                    break;
                default:
                    Debug.Log($"load entity fail missing case {entity.GetType()}");
                    performed.Invoke();
                    break;

            }
        }

        /// <summary>
        /// Load IEntity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="performed"></param>
        void _LoadEntity(ByteContainer container, Action performed)
        {
            List<ulong> ids = UMI3DNetworkingHelper.ReadList<ulong>(container);
            int count = ids.Count;
            int performedCount = 0;
            Action performed2 = () => { performedCount++; if (performedCount == count) performed.Invoke(); };
            Action<LoadEntityDto> callback = (load) =>
            {
                foreach (IEntity item in load.entities)
                {
                    LoadEntity(item, performed2);
                }
            };
            Action<string> error = (s) =>
            {
                Debug.Log(s);
                performed2.Invoke();
            };
            UMI3DClientServer.GetEntity(ids, callback, error);
        }

        /// <summary>
        /// Delete IEntity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="performed"></param>
        static public void DeleteEntity(ulong entityId, Action performed)
        {
            if (Instance.entities.ContainsKey(entityId))
            {
                UMI3DEntityInstance entity = Instance.entities[entityId];
                if (entity is UMI3DNodeInstance)
                {
                    UMI3DNodeInstance node = entity as UMI3DNodeInstance;
                    Destroy(node.gameObject);
                }
                Instance.entities[entityId].Delete?.Invoke();
                Instance.entities.Remove(entityId);
            }
            else if (UMI3DResourcesManager.isKnowedLibrary(entityId))
            {
                UMI3DResourcesManager.UnloadLibrary(entityId);
            }
            else
                Debug.LogError($"Entity [{entityId}] To Destroy Not Found");
            performed?.Invoke();
        }

        /// <summary>
        /// Clear an environement and make the client ready to load a new environment.
        /// </summary>
        static public void Clear()
        {
            Instance.entityFilters.Clear();

            foreach (var entity in Instance.entities.ToList().Select(p => { return p.Key; }))
            {
                DeleteEntity(entity, null);
            }
            UMI3DResourcesManager.Instance.ClearCache();
        }

        /// <summary>
        /// Load environment.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="node"></param>
        public virtual void ReadUMI3DExtension(GlTFEnvironmentDto dto, GameObject node)
        {
            var extension = dto?.extensions?.umi3d;
            if (extension != null)
            {
                if (extension.defaultMaterial != null && extension.defaultMaterial.variants != null && extension.defaultMaterial.variants.Count > 0)
                {
                    baseMaterial = null;
                    LoadDefaultMaterial(extension.defaultMaterial);
                }
                foreach (var scene in extension.preloadedScenes)
                    Parameters.ReadUMI3DExtension(scene, node, null, null);
                RenderSettings.ambientMode = (AmbientMode)extension.ambientType;
                RenderSettings.ambientSkyColor = extension.skyColor;
                RenderSettings.ambientEquatorColor = extension.horizontalColor;
                RenderSettings.ambientGroundColor = extension.groundColor;
                RenderSettings.ambientIntensity = extension.ambientIntensity;
                if (extension.skybox != null)
                {
                    Parameters.loadSkybox(extension.skybox);
                }

            }
        }

        /// <summary>
        /// Load DefaultMaterial from matDto
        /// </summary>
        /// <param name="matDto"></param>
        private void LoadDefaultMaterial(ResourceDto matDto)
        {
            FileDto fileToLoad = Parameters.ChooseVariante(matDto.variants);
            if (fileToLoad == null) return;
            string url = fileToLoad.url;
            string ext = fileToLoad.extension;
            string authorization = fileToLoad.authorization;
            IResourcesLoader loader = Parameters.SelectLoader(ext);
            if (loader != null)
                UMI3DResourcesManager.LoadFile(
                    UMI3DGlobalID.EnvironementId,
                    fileToLoad,
                    loader.UrlToObject,
                    loader.ObjectFromCache,
                    (mat) => SetBaseMaterial((Material)mat),
                    (e)=>Debug.LogWarning(e.Message),
                    loader.DeleteObject
                    );

        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="property">Property containing the new value.</param>
        /// <returns></returns>
        public static bool SetUMI3DPorperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (Exists)
                return Instance._SetUMI3DPorperty(entity, property);
            else
                return false;
        }

        public static bool ReadUMI3DPorperty(ref object value, uint propertyKey, ByteContainer container)
        {
            if (Exists)
                return Instance._ReadUMI3DPorperty(ref value, propertyKey, container);
            else
                return false;
        }


        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="property">Property containing the new value.</param>
        /// <returns></returns>
        public static bool SetUMI3DPorperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (Exists)
                return Instance._SetUMI3DPorperty(entity, operationId, propertyKey, container);
            else
                return false;
        }


        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="property">Property containing the new value.</param>
        /// <returns></returns>
        protected virtual bool _SetUMI3DPorperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (entity == null) return false;
            var dto = ((entity.dto as GlTFEnvironmentDto)?.extensions as GlTFEnvironmentExtensions)?.umi3d;
            if (dto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.PreloadedScenes:
                    return Parameters.SetUMI3DProperty(entity, property);
                default:
                    return false;
            }
        }


        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="property">Property containing the new value.</param>
        /// <returns></returns>
        protected virtual bool _SetUMI3DPorperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (entity == null) return false;
            var dto = ((entity.dto as GlTFEnvironmentDto)?.extensions as GlTFEnvironmentExtensions)?.umi3d;
            if (dto == null) return false;

            switch (propertyKey)
            {
                case UMI3DPropertyKeys.PreloadedScenes:
                    return Parameters.SetUMI3DProperty(entity, operationId, propertyKey, container);
                default:
                    return false;
            }
        }

        protected virtual bool _ReadUMI3DPorperty(ref object value, uint propertyKey, ByteContainer container)
        {
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.PreloadedScenes:
                    return Parameters.ReadUMI3DProperty(ref value, propertyKey, container);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public static bool SetEntity(SetEntityPropertyDto dto)
        {
            if (!Exists) return false;
            var node = UMI3DEnvironmentLoader.GetEntity(dto.entityId);
            if (node == null)
            {
                Instance.StartCoroutine(Instance._SetEntity(dto));
                return false;
            }
            else
            {
                return SetEntity(node, dto);
            }
        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public static bool SetEntity(uint operationId, ulong entityId, uint propertyKey, ByteContainer container)
        {
            var node = UMI3DEnvironmentLoader.GetEntity(entityId);
            if (node == null)
            {
                Instance.StartCoroutine(Instance._SetEntity(operationId, entityId, propertyKey, container));
                return false;
            }
            else
            {
                return SetEntity(node, operationId, entityId, propertyKey, container);
            }
        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="node">Node on which the dto should be applied.</param>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public static bool SetEntity(UMI3DEntityInstance node, SetEntityPropertyDto dto)
        {
            if (Instance.entityFilters.ContainsKey(dto.entityId) && Instance.entityFilters[dto.entityId].ContainsKey(dto.property))
            {
                float now = Time.time;
                Instance.entityFilters[dto.entityId][dto.property].measuresPerSecond = 1 / (now - Instance.entityFilters[dto.entityId][dto.property].lastMessageTime);
                Instance.entityFilters[dto.entityId][dto.property].lastMessageTime = now;
                Instance.PropertyKalmanUpdate(Instance.entityFilters[dto.entityId][dto.property], dto.value);
                return true;
            }
            else
            {
                if (SetUMI3DPorperty(node, dto)) return true;
                if (UMI3DEnvironmentLoader.Exists && UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(node, dto)) return true;
                return Parameters.SetUMI3DProperty(node, dto);
            }
        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="node">Node on which the dto should be applied.</param>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public static bool SetEntity(UMI3DEntityInstance node, uint operationId, ulong entityId, uint propertyKey, ByteContainer container)
        {
            if (Instance.entityFilters.ContainsKey(entityId) && Instance.entityFilters[entityId].ContainsKey(propertyKey))
            {
                float now = Time.time;
                Instance.entityFilters[entityId][propertyKey].measuresPerSecond = 1 / (now - Instance.entityFilters[entityId][propertyKey].lastMessageTime);
                Instance.entityFilters[entityId][propertyKey].lastMessageTime = now;
                object value = null;
                ReadValueEntity(ref value, propertyKey, container);
                Instance.PropertyKalmanUpdate(Instance.entityFilters[entityId][propertyKey], value);
                return true;
            }
            else
            {
                if (SetUMI3DPorperty(node, operationId, propertyKey, container)) return true;
                if (UMI3DEnvironmentLoader.Exists && UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(node, operationId, propertyKey, container)) return true;
                return Parameters.SetUMI3DProperty(node, operationId, propertyKey, container);
            }
        }

        public static bool ReadValueEntity(ref object value, uint propertyKey, ByteContainer container)
        {
            if (ReadUMI3DPorperty(ref value, propertyKey, container)) return true;
            if (UMI3DEnvironmentLoader.Exists && UMI3DEnvironmentLoader.Instance.sceneLoader.ReadUMI3DProperty(ref value, propertyKey, container)) return true;
            return Parameters.ReadUMI3DProperty(ref value, propertyKey, container);
        }


        private static bool SimulatedSetEntity(UMI3DEntityInstance node, SetEntityPropertyDto dto)
        {
            if (SetUMI3DPorperty(node, dto)) return true;
            if (UMI3DEnvironmentLoader.Exists && UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(node, dto)) return true;
            return Parameters.SetUMI3DProperty(node, dto);
        }


        /// <summary>
        /// Apply a setEntity to multiple entities
        /// </summary>
        /// <param name="dto">MultiSetEntityPropertyDto with the ids list to mofify</param>
        /// <returns></returns>
        public static bool SetMultiEntity(MultiSetEntityPropertyDto dto)
        {
            if (!Exists) return false;
            foreach (ulong id in dto.entityIds)
            {
                try
                {
                    var node = UMI3DEnvironmentLoader.GetEntity(id);
                    SetEntityPropertyDto entityPropertyDto = new SetEntityPropertyDto()
                    {
                        entityId = id,
                        property = dto.property,
                        value = dto.value
                    };
                    if (node == null)
                    {
                        Instance.StartCoroutine(Instance._SetEntity(entityPropertyDto));
                    }
                    else
                    {
                        if (SetUMI3DPorperty(node, entityPropertyDto)) break;
                        if (UMI3DEnvironmentLoader.Exists && UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(node, entityPropertyDto)) break;
                        Parameters.SetUMI3DProperty(node, entityPropertyDto);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("SetEntity not apply on this object, id = " + id + " ,  property = " + dto.property);
                    Debug.LogWarning(e);
                }
            }
            return true;
        }

        /// <summary>
        /// Apply a setEntity to multiple entities
        /// </summary>
        /// <param name="dto">MultiSetEntityPropertyDto with the ids list to mofify</param>
        /// <returns></returns>
        public static bool SetMultiEntity(ByteContainer container)
        {
            if (!Exists) return false;
            var idList = UMI3DNetworkingHelper.ReadList<ulong>(container);
            var operationId = UMI3DNetworkingHelper.Read<uint>(container);
            var propertyKey = UMI3DNetworkingHelper.Read<uint>(container);

            foreach (ulong id in idList)
            {
                try
                {
                    var node = UMI3DEnvironmentLoader.GetEntity(id);
                    if (node == null)
                    {
                        Instance.StartCoroutine(Instance._SetEntity(operationId, id, propertyKey, container));
                    }
                    else
                    {
                        if (SetUMI3DPorperty(node, operationId, propertyKey, container)) break;
                        if (UMI3DEnvironmentLoader.Exists && UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(node, operationId, propertyKey, container)) break;
                        if (!Parameters.SetUMI3DProperty(node, operationId, propertyKey, container))
                        {
                            Debug.LogWarning($"A SetUMI3DProperty failed to match any loader {id} {operationId} {propertyKey} {container}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"SetEntity not apply on this object, id = {  id },  operation = { operationId } ,  property = { propertyKey }");
                    Debug.LogWarning(e);
                }
            }
            return true;
        }

        IEnumerator _SetEntity(SetEntityPropertyDto dto)
        {
            WaitForFixedUpdate wait = new WaitForFixedUpdate();
            UMI3DEntityInstance node = null;
            yield return wait;
            while ((node = UMI3DEnvironmentLoader.GetEntity(dto.entityId)) == null)
            {
                yield return wait;
            }
            SetEntity(node, dto);
        }

        IEnumerator _SetEntity(uint operationId, ulong entityId, uint propertyKey, ByteContainer container)
        {
            WaitForFixedUpdate wait = new WaitForFixedUpdate();
            UMI3DEntityInstance node = null;
            yield return wait;
            while ((node = UMI3DEnvironmentLoader.GetEntity(entityId)) == null)
            {
                yield return wait;
            }
            SetEntity(node, operationId, entityId, propertyKey, container);
        }

        #region interpolation

        private abstract class AbstractKalmanEntity
        {
            public float measuresPerSecond;
            public float lastMessageTime;
            public ulong entityId;
            public ulong property;
            public object regressed_value;

            public AbstractKalmanEntity(double q, double r) { }
        }

        private class KalmanEntity : AbstractKalmanEntity
        {
            public UMI3DUnscentedKalmanFilter KalmanFilter;
            public double[] estimations;
            public double[] previous_prediction;
            public double[] prediction;
            
            public KalmanEntity(double q, double r) : base(q, r)
            {
                KalmanFilter = new UMI3DUnscentedKalmanFilter(q, r);
                estimations = new double[] { };
                previous_prediction = new double[] { };
                prediction = new double[] { };
                lastMessageTime = 0;
            }
        }

        private class KalmanRotationEntity : AbstractKalmanEntity
        {
            // forward, up
            public Tuple<UMI3DUnscentedKalmanFilter, UMI3DUnscentedKalmanFilter> KalmanFilters;
            public Tuple<double[], double[]> estimations;
            public Tuple<double[], double[]> previous_prediction;
            public Tuple<double[], double[]> prediction;

            public KalmanRotationEntity(double q, double r) : base(q, r)
            {
                KalmanFilters = new Tuple<UMI3DUnscentedKalmanFilter, UMI3DUnscentedKalmanFilter>(new UMI3DUnscentedKalmanFilter(q, r), new UMI3DUnscentedKalmanFilter(q, r));
                estimations = new Tuple<double[], double[]>(new double[] { }, new double[] { });
                previous_prediction = new Tuple<double[], double[]>(new double[] { }, new double[] { });
                prediction = new Tuple<double[], double[]>(new double[] { }, new double[] { });
                lastMessageTime = 0;
            }
        }

        Dictionary<ulong, Dictionary<ulong, AbstractKalmanEntity>> entityFilters = new Dictionary<ulong, Dictionary<ulong, AbstractKalmanEntity>>();

        private void Update()
        {
            foreach (var entityId in Instance.entityFilters.Keys)
                foreach (var property in Instance.entityFilters[entityId].Keys)
                {
                    var node = UMI3DEnvironmentLoader.GetEntity(entityId);
                    AbstractKalmanEntity kalmanEntity = Instance.entityFilters[entityId][property];

                    Instance.PropertyRegression(kalmanEntity);

                    SetEntityPropertyDto entityPropertyDto = new SetEntityPropertyDto()
                    {
                        entityId = kalmanEntity.entityId,
                        property = kalmanEntity.property,
                        value = kalmanEntity.regressed_value
                    };

                    SimulatedSetEntity(node, entityPropertyDto);
                }
        }

        /// <summary>
        /// Handle StartInterpolationPropertyDto operation.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static bool StartInterpolation(StartInterpolationPropertyDto dto)
        {
            if (!Exists) return false;
            var node = UMI3DEnvironmentLoader.GetEntity(dto.entityId);
            if (node == null)
            {
                Instance.StartCoroutine(Instance._StartInterpolation(dto));
                return false;
            }
            else
            {
                StartInterpolation(node, dto);
            }
            return true;
        }

        /// <summary>
        /// Handle StartInterpolationPropertyDto operation.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static bool StartInterpolation(ByteContainer container)
        {
            if (!Exists) return false;
            var entityId = UMI3DNetworkingHelper.Read<ulong>(container);
            var propertyKey = UMI3DNetworkingHelper.Read<uint>(container);
            var frequence = UMI3DNetworkingHelper.Read<uint>(container);
            var node = UMI3DEnvironmentLoader.GetEntity(entityId);
            if (node == null)
            {
                Instance.StartCoroutine(Instance._StartInterpolation(entityId, propertyKey, frequence, container));
                return false;
            }
            else
            {
                StartInterpolation(node, entityId, propertyKey, frequence, container);
            }
            return true;
        }

        IEnumerator _StartInterpolation(StartInterpolationPropertyDto dto)
        {
            WaitForFixedUpdate wait = new WaitForFixedUpdate();
            UMI3DEntityInstance node = null;
            yield return wait;
            while ((node = UMI3DEnvironmentLoader.GetEntity(dto.entityId)) == null)
            {
                yield return wait;
            }
            StartInterpolation(node, dto);
        }

        IEnumerator _StartInterpolation(ulong id, uint property, uint frequence, ByteContainer container)
        {
            WaitForFixedUpdate wait = new WaitForFixedUpdate();
            UMI3DEntityInstance node = null;
            yield return wait;
            while ((node = UMI3DEnvironmentLoader.GetEntity(id)) == null)
            {
                yield return wait;
            }
            StartInterpolation(node, id, property, frequence, container);
        }

        /// <summary>
        /// Handle StartInterpolationPropertyDto operation.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static bool StartInterpolation(UMI3DEntityInstance node, StartInterpolationPropertyDto dto)
        {
            if (!Instance.entityFilters.ContainsKey(dto.entityId))
            {
                Instance.entityFilters.Add(dto.entityId, new Dictionary<ulong, AbstractKalmanEntity>());
            }

            if (!Instance.entityFilters[dto.entityId].ContainsKey(dto.property))
            {

                AbstractKalmanEntity newKalmanEntity;

                if (dto.property.Equals(UMI3DPropertyKeys.Rotation))
                    newKalmanEntity = new KalmanRotationEntity(50, 0.5)
                    {
                        lastMessageTime = Time.time,
                        entityId = dto.entityId,
                        property = dto.property
                    };

                else
                    newKalmanEntity = new KalmanEntity(50, 0.5)
                    {
                        lastMessageTime = Time.time,
                        entityId = dto.entityId,
                        property = dto.property
                    };

                Instance.entityFilters[dto.entityId].Add(dto.property, newKalmanEntity);

                Instance.PropertyKalmanUpdate(newKalmanEntity, dto.startValue);

                SetEntityPropertyDto entityPropertyDto = new SetEntityPropertyDto()
                {
                    entityId = dto.entityId,
                    property = dto.property,
                    value = dto.startValue
                };

                SetEntity(node, entityPropertyDto);
                return true;
            }
            return false;
        }

        public static bool StartInterpolation(UMI3DEntityInstance node, ulong entityId, uint property, uint frequence, ByteContainer container)
        {
            if (!Instance.entityFilters.ContainsKey(entityId))
            {
                Instance.entityFilters.Add(entityId, new Dictionary<ulong, AbstractKalmanEntity>());
            }

            if (!Instance.entityFilters[entityId].ContainsKey(property))
            {
                AbstractKalmanEntity newKalmanEntity;

                if (property.Equals(UMI3DPropertyKeys.Rotation))
                    newKalmanEntity = new KalmanRotationEntity(50, 0.5)
                    {
                        lastMessageTime = Time.time,
                        entityId = entityId,
                        property = property
                    };

                else
                    newKalmanEntity = new KalmanEntity(50, 0.5)
                    {
                        lastMessageTime = Time.time,
                        entityId = entityId,
                        property = property
                    };

                Instance.entityFilters[entityId].Add(property, newKalmanEntity);

                object value = null;
                ReadValueEntity(ref value, property, container);

                Instance.PropertyKalmanUpdate(newKalmanEntity, value);

                SetEntityPropertyDto entityPropertyDto = new SetEntityPropertyDto()
                {
                    entityId = entityId,
                    property = property,
                    value = value
                };

                SetEntity(node, entityPropertyDto);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handle StopInterpolationPropertyDto operation.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static bool StopInterpolation(StopInterpolationPropertyDto dto)
        {
            if (!Exists) return false;
            var node = UMI3DEnvironmentLoader.GetEntity(dto.entityId);
            if (node == null)
            {
                Instance.StartCoroutine(Instance._StopInterpolation(dto));
                return false;
            }
            else
            {
                StopInterpolation(node, dto);
            }
            return true;
        }

        public static bool StopInterpolation(ByteContainer container)
        {
            if (!Exists) return false;
            var entityId = UMI3DNetworkingHelper.Read<ulong>(container);
            var propertyKey = UMI3DNetworkingHelper.Read<uint>(container);
            var node = UMI3DEnvironmentLoader.GetEntity(entityId);
            if (node == null)
            {
                Instance.StartCoroutine(Instance._StopInterpolation(entityId, propertyKey, container));
                return false;
            }
            else
            {
                StopInterpolation(node, entityId, propertyKey, container);
            }
            return true;
        }

        IEnumerator _StopInterpolation(StopInterpolationPropertyDto dto)
        {
            WaitForFixedUpdate wait = new WaitForFixedUpdate();
            UMI3DEntityInstance node = null;
            yield return wait;
            while ((node = UMI3DEnvironmentLoader.GetEntity(dto.entityId)) == null)
            {
                yield return wait;
            }
            StopInterpolation(node, dto);
        }

        IEnumerator _StopInterpolation(ulong entityId, uint propertyKey, ByteContainer container)
        {
            WaitForFixedUpdate wait = new WaitForFixedUpdate();
            UMI3DEntityInstance node = null;
            yield return wait;
            while ((node = UMI3DEnvironmentLoader.GetEntity(entityId)) == null)
            {
                yield return wait;
            }
            StopInterpolation(node, entityId, propertyKey, container);
        }

        /// <summary>
        /// Handle StopInterpolationPropertyDto operation.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static bool StopInterpolation(UMI3DEntityInstance node, StopInterpolationPropertyDto dto)
        {
            if (Instance.entityFilters.ContainsKey(dto.entityId) && Instance.entityFilters[dto.entityId].ContainsKey(dto.property))
            {
                Instance.entityFilters[dto.entityId].Remove(dto.property);
                SetEntityPropertyDto entityPropertyDto = new SetEntityPropertyDto()
                {
                    entityId = dto.entityId,
                    property = dto.property,
                    value = dto.stopValue
                };

                SetEntity(node, entityPropertyDto);

                return true;
            }

            Debug.LogWarning("Need to determine what happens when not in interpolation");

            return false;
        }

        public static bool StopInterpolation(UMI3DEntityInstance node, ulong entityId, uint property, ByteContainer container)
        {
            if (Instance.entityFilters.ContainsKey(entityId) && Instance.entityFilters[entityId].ContainsKey(property))
            {
                object value = null;
                ReadValueEntity(ref value, property, container);

                Instance.entityFilters[entityId].Remove(property);
                SetEntityPropertyDto entityPropertyDto = new SetEntityPropertyDto()
                {
                    entityId = entityId,
                    property = property,
                    value = value
                };

                SetEntity(node, entityPropertyDto);

                return true;
            }
            return false;
        }

        void PropertyRegression(AbstractKalmanEntity kalmanEntity)
        {
            if (kalmanEntity.property.Equals(UMI3DPropertyKeys.Rotation))
            {
                if ((kalmanEntity as KalmanRotationEntity).previous_prediction.Item1.Length > 0)
                {
                    double check = kalmanEntity.lastMessageTime;
                    double now = Time.time;

                    double delta = now - check;

                    if (delta * kalmanEntity.measuresPerSecond <= 1)
                    {
                        var fw_value_x = ((kalmanEntity as KalmanRotationEntity).prediction.Item1[0] - (kalmanEntity as KalmanRotationEntity).previous_prediction.Item1[0]) * kalmanEntity.measuresPerSecond * delta + (kalmanEntity as KalmanRotationEntity).previous_prediction.Item1[0];
                        var fw_value_y = ((kalmanEntity as KalmanRotationEntity).prediction.Item1[1] - (kalmanEntity as KalmanRotationEntity).previous_prediction.Item1[1]) * kalmanEntity.measuresPerSecond * delta + (kalmanEntity as KalmanRotationEntity).previous_prediction.Item1[1];
                        var fw_value_z = ((kalmanEntity as KalmanRotationEntity).prediction.Item1[2] - (kalmanEntity as KalmanRotationEntity).previous_prediction.Item1[2]) * kalmanEntity.measuresPerSecond * delta + (kalmanEntity as KalmanRotationEntity).previous_prediction.Item1[2];

                        var up_value_x = ((kalmanEntity as KalmanRotationEntity).prediction.Item2[0] - (kalmanEntity as KalmanRotationEntity).previous_prediction.Item2[0]) * kalmanEntity.measuresPerSecond * delta + (kalmanEntity as KalmanRotationEntity).previous_prediction.Item2[0];
                        var up_value_y = ((kalmanEntity as KalmanRotationEntity).prediction.Item2[1] - (kalmanEntity as KalmanRotationEntity).previous_prediction.Item2[1]) * kalmanEntity.measuresPerSecond * delta + (kalmanEntity as KalmanRotationEntity).previous_prediction.Item2[1];
                        var up_value_z = ((kalmanEntity as KalmanRotationEntity).prediction.Item2[2] - (kalmanEntity as KalmanRotationEntity).previous_prediction.Item2[2]) * kalmanEntity.measuresPerSecond * delta + (kalmanEntity as KalmanRotationEntity).previous_prediction.Item2[2];

                        (kalmanEntity as KalmanRotationEntity).estimations = new Tuple<double[], double[]>(new double[] { fw_value_x, fw_value_y, fw_value_z }, new double[] { up_value_x, up_value_y, up_value_z });

                        Quaternion res = Quaternion.LookRotation(new Vector3((float)fw_value_x, (float)fw_value_y, (float)fw_value_z), new Vector3((float)up_value_x, (float)up_value_y, (float)up_value_z));

                        kalmanEntity.regressed_value = new SerializableVector4(res.x, res.y, res.z, res.w);
                    }
                }
            }

            else
            {
                if ((kalmanEntity as KalmanEntity).previous_prediction.Length > 0)
                {
                    double check = kalmanEntity.lastMessageTime;
                    double now = Time.time;

                    double delta = now - check;

                    double new_value_1;
                    double new_value_2;
                    double new_value_3;
                    double new_value_4;

                    if (delta * kalmanEntity.measuresPerSecond <= 1)
                    {
                        switch (kalmanEntity.regressed_value)
                        {
                            case int n:
                                new_value_1 = ((kalmanEntity as KalmanEntity).prediction[0] - (kalmanEntity as KalmanEntity).previous_prediction[0]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[0];

                                (kalmanEntity as KalmanEntity).estimations = new double[] { new_value_1 };
                                kalmanEntity.regressed_value = (int)new_value_1;

                                break;
                            case float f:
                                new_value_1 = ((kalmanEntity as KalmanEntity).prediction[0] - (kalmanEntity as KalmanEntity).previous_prediction[0]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[0];

                                (kalmanEntity as KalmanEntity).estimations = new double[] { new_value_1 };
                                kalmanEntity.regressed_value = (float)new_value_1;

                                break;
                            case SerializableVector2 v:
                                new_value_1 = ((kalmanEntity as KalmanEntity).prediction[0] - (kalmanEntity as KalmanEntity).previous_prediction[0]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[0];
                                new_value_2 = ((kalmanEntity as KalmanEntity).prediction[1] - (kalmanEntity as KalmanEntity).previous_prediction[1]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[1];

                                (kalmanEntity as KalmanEntity).estimations = new double[] { new_value_1, new_value_2 };
                                kalmanEntity.regressed_value = new SerializableVector2((float)new_value_1, (float)new_value_2);

                                break;
                            case SerializableVector3 v:
                                new_value_1 = ((kalmanEntity as KalmanEntity).prediction[0] - (kalmanEntity as KalmanEntity).previous_prediction[0]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[0];
                                new_value_2 = ((kalmanEntity as KalmanEntity).prediction[1] - (kalmanEntity as KalmanEntity).previous_prediction[1]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[1];
                                new_value_3 = ((kalmanEntity as KalmanEntity).prediction[2] - (kalmanEntity as KalmanEntity).previous_prediction[2]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[2];

                                (kalmanEntity as KalmanEntity).estimations = new double[] { new_value_1, new_value_2, new_value_3 };
                                kalmanEntity.regressed_value = new SerializableVector3((float)new_value_1, (float)new_value_2, (float)new_value_3);

                                break;
                            case SerializableVector4 v:
                                double[] estimations;
                                object regressed_value;

                                new_value_1 = ((kalmanEntity as KalmanEntity).prediction[0] - (kalmanEntity as KalmanEntity).previous_prediction[0]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[0];
                                new_value_2 = ((kalmanEntity as KalmanEntity).prediction[1] - (kalmanEntity as KalmanEntity).previous_prediction[1]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[1];
                                new_value_3 = ((kalmanEntity as KalmanEntity).prediction[2] - (kalmanEntity as KalmanEntity).previous_prediction[2]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[2];
                                new_value_4 = ((kalmanEntity as KalmanEntity).prediction[3] - (kalmanEntity as KalmanEntity).previous_prediction[3]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[3];

                                estimations = new double[] { new_value_1, new_value_2, new_value_3, new_value_4 };
                                regressed_value = new SerializableVector4((float)new_value_1, (float)new_value_2, (float)new_value_3, (float)new_value_4);

                                (kalmanEntity as KalmanEntity).estimations = estimations;
                                kalmanEntity.regressed_value = regressed_value;

                                break;
                            case SerializableColor v:
                                new_value_1 = ((kalmanEntity as KalmanEntity).prediction[0] - (kalmanEntity as KalmanEntity).previous_prediction[0]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[0];
                                new_value_2 = ((kalmanEntity as KalmanEntity).prediction[1] - (kalmanEntity as KalmanEntity).previous_prediction[1]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[1];
                                new_value_3 = ((kalmanEntity as KalmanEntity).prediction[2] - (kalmanEntity as KalmanEntity).previous_prediction[2]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[2];
                                new_value_4 = ((kalmanEntity as KalmanEntity).prediction[3] - (kalmanEntity as KalmanEntity).previous_prediction[3]) * delta * kalmanEntity.measuresPerSecond + (kalmanEntity as KalmanEntity).previous_prediction[3];

                                (kalmanEntity as KalmanEntity).estimations = new double[] { new_value_1, new_value_2, new_value_3, new_value_4 };
                                kalmanEntity.regressed_value = new SerializableVector4((float)new_value_1, (float)new_value_2, (float)new_value_3, (float)new_value_4);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }              
        }

        void PropertyKalmanUpdate(AbstractKalmanEntity abstractKalman, object value)
        {
            object measurement;

            switch (value)
            {
                case int n:
                    measurement = new double[] { n };
                    if (abstractKalman.regressed_value == null)
                        abstractKalman.regressed_value = n;
                    break;
                case float f:
                    measurement = new double[] { f };
                    if (abstractKalman.regressed_value == null)
                        abstractKalman.regressed_value = f;
                    break;
                case SerializableVector2 v:
                    measurement = new double[] { v.X, v.Y };
                    if (abstractKalman.regressed_value == null)
                        abstractKalman.regressed_value = v;
                    break;
                case SerializableVector3 v:
                    measurement = new double[] { v.X, v.Y, v.Z };
                    if (abstractKalman.regressed_value == null)
                        abstractKalman.regressed_value = v;
                    break;
                case SerializableVector4 v:
                    if (abstractKalman.property.Equals(UMI3DPropertyKeys.Rotation))
                    {
                        Quaternion quaternionMeasurment = new Quaternion(v.X, v.Y, v.Z, v.W);

                        Vector3 targetForward = quaternionMeasurment * Vector3.forward;
                        Vector3 targetUp = quaternionMeasurment * Vector3.up;

                        double[] targetForwardMeasurement = new double[] { targetForward.x, targetForward.y, targetForward.z };
                        double[] targetUpMeasurement = new double[] { targetUp.x, targetUp.y, targetUp.z };

                        measurement = new Tuple<double[], double[]>(targetForwardMeasurement, targetUpMeasurement);
                    }
                    else
                        measurement = new double[] { v.X, v.Y, v.Z, v.W };
                    if (abstractKalman.regressed_value == null)
                        abstractKalman.regressed_value = v;

                    break;
                case SerializableColor v:
                    measurement = new double[] { v.R, v.G, v.B, v.A };
                    if (abstractKalman.regressed_value == null)
                        abstractKalman.regressed_value = v;
                    break;
                default:
                    measurement = new double[0];
                    break;
            }

            if (abstractKalman.property.Equals(UMI3DPropertyKeys.Rotation))
            {
                (abstractKalman as KalmanRotationEntity).KalmanFilters.Item1.Update((measurement as Tuple<double[], double[]>).Item1); // forward
                (abstractKalman as KalmanRotationEntity).KalmanFilters.Item2.Update((measurement as Tuple<double[], double[]>).Item2);

                (abstractKalman as KalmanRotationEntity).prediction = new Tuple<double[], double[]>((abstractKalman as KalmanRotationEntity).KalmanFilters.Item1.getState(), (abstractKalman as KalmanRotationEntity).KalmanFilters.Item2.getState());

                if ((abstractKalman as KalmanRotationEntity).estimations.Item1.Length > 0)
                    (abstractKalman as KalmanRotationEntity).previous_prediction = (abstractKalman as KalmanRotationEntity).estimations;
                else
                    (abstractKalman as KalmanRotationEntity).previous_prediction = new System.Tuple<double[], double[]>((measurement as Tuple<double[], double[]>).Item1, (measurement as Tuple<double[], double[]>).Item2);
            }

            else if ((measurement as double[]).Length > 0)
            {
                (abstractKalman as KalmanEntity).KalmanFilter.Update((measurement as double[]));

                double[] newValueState = (abstractKalman as KalmanEntity).KalmanFilter.getState();

                (abstractKalman as KalmanEntity).prediction = newValueState;

                if ((abstractKalman as KalmanEntity).estimations.Length > 0)
                    (abstractKalman as KalmanEntity).previous_prediction = (abstractKalman as KalmanEntity).estimations;
                else
                    (abstractKalman as KalmanEntity).previous_prediction = (measurement as double[]);
            }
            else
            {
                throw new Exception("Datatype not filterable");
            }
        }

        #endregion
    }
}