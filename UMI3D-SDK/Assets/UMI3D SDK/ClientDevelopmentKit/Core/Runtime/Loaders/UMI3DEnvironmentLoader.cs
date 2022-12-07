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
using umi3d.cdk.utils.extrapolation;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.utils.serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="UMI3DEnvironmentDto"/>.
    /// </summary>
    public class UMI3DEnvironmentLoader : inetum.unityUtils.SingleBehaviour<UMI3DEnvironmentLoader>
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        /// <summary>
        /// Index of any 3D object loaded.
        /// </summary>
        private readonly Dictionary<ulong, UMI3DEntityInstance> entities = new Dictionary<ulong, UMI3DEntityInstance>();
        private readonly Dictionary<ulong, List<(Action<UMI3DEntityInstance>, Action)>> entitywaited = new Dictionary<ulong, List<(Action<UMI3DEntityInstance>, Action)>>();
        private readonly HashSet<ulong> entityToBeLoaded = new HashSet<ulong>();
        private readonly HashSet<ulong> entityFailedToBeLoaded = new HashSet<ulong>();

        /// <summary>
        /// Call a callback when an entity is registerd.
        /// The entity might not be totaly loaded when the callback is called.
        /// all property of UMI3DEntityInstance and UMI3DNodeInstance are set.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entityLoaded"></param>
        /// <param name="entityFailedToLoad"></param>
        public static void WaitForAnEntityToBeLoaded(ulong id, Action<UMI3DEntityInstance> entityLoaded, Action entityFailedToLoad = null)
        {
            if (!Exists) return;
            if (Instance.entitywaited == null) return;

            UMI3DEntityInstance node = GetEntity(id);
            if (node != null && node.IsLoaded)
            {
                entityLoaded?.Invoke(node);
            }
            else
            {
                if (Instance.entitywaited.ContainsKey(id))
                    Instance.entitywaited[id].Add((entityLoaded, entityFailedToLoad));
                else
                    Instance.entitywaited[id] = new List<(Action<UMI3DEntityInstance>, Action)>() { (entityLoaded, entityFailedToLoad) };
            }
        }

        public static async Task<UMI3DEntityInstance> WaitForAnEntityToBeLoaded(ulong id)
        {
            if (!Exists)
                throw new Umi3dException("Do Not Exist");
            if (Instance.entitywaited == null)
                throw new Umi3dException("Do Not Exist");

            UMI3DEntityInstance node = GetEntity(id);
            if (node != null && node.IsLoaded)
            {
                return (node);
            }
            UMI3DEntityInstance loaded = null;
            bool error = false;
            bool finished = false;

            Action<UMI3DEntityInstance> entityLoaded = (e) => { loaded = e; finished = true; };
            Action entityFailedToLoad = () => { error = true; finished = true; };

            WaitForAnEntityToBeLoaded(id, entityLoaded, entityFailedToLoad);

            while (!finished)
                await UMI3DAsyncManager.Yield();
            if (error)
                throw new Umi3dException("Entity Failed to be loaded");

            return loaded;
        }

        private static bool NotifyEntityToBeLoaded(ulong id)
        {
            return Exists && Instance.entityToBeLoaded.Add(id);
        }

        private static bool IsEntityToBeLoaded(ulong id)
        {
            return Exists && Instance.entityToBeLoaded.Contains(id);
        }
        private static bool IsEntityToFailedBeLoaded(ulong id)
        {
            return Exists && Instance.entityFailedToBeLoaded.Contains(id);
        }

        private static bool RemoveEntityToFailedBeLoaded(ulong id)
        {
            return Exists && Instance.entityFailedToBeLoaded.Remove(id);
        }

        private static void NotifyEntityLoad(ulong id)
        {
            if (Exists)
            {
                UMI3DEntityInstance node = GetEntity(id);
                if (node != null)
                {
                    if (Instance.entitywaited.ContainsKey(id))
                    {
                        Instance.entitywaited[id].ForEach(a => a.Item1?.Invoke(node));
                        Instance.entitywaited.Remove(id);
                    }
                    Instance.entityToBeLoaded.Remove(id);
                }
            }
        }

        private static void NotifyEntityFailedToLoad(ulong id)
        {
            if (Exists)
            {
                if (Instance.entitywaited.ContainsKey(id))
                {
                    Instance.entitywaited[id].ForEach(a => a.Item2?.Invoke());
                    Instance.entitywaited.Remove(id);
                }
                Instance.entityToBeLoaded.Remove(id);
                Instance.entityFailedToBeLoaded.Add(id);
            }
        }

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
        /// Get node id associated to <paramref name="t"/>.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static ulong GetNodeID(Transform t) { return Exists ? Instance.entities.Where(k => k.Value is UMI3DNodeInstance).FirstOrDefault(k => (k.Value as UMI3DNodeInstance).transform == t).Key : 0; }

        /// <summary>
        /// Register a node instance.
        /// </summary>
        /// <param name="id">unique id of the node.</param>
        /// <param name="dto">dto of the node.</param>
        /// <param name="instance">gameobject of the node.</param>
        /// <returns></returns>
        public static UMI3DNodeInstance RegisterNodeInstance(ulong id, UMI3DDto dto, GameObject instance, Action delete = null)
        {
            UMI3DNodeInstance node = null;
            if (!Exists || instance == null)
            {
                return null;
            }
            else if (Instance.entities.ContainsKey(id))
            {
                node = Instance.entities[id] as UMI3DNodeInstance;
                if (node == null)
                    throw new Exception($"id:{id} found but the value was of type {Instance.entities[id].GetType()}");
                if (node.gameObject != instance)
                    Destroy(instance);
                return node;
            }
            else
            {
                node = new UMI3DNodeInstance(() => NotifyEntityLoad(id)) { gameObject = instance, dto = dto, Delete = delete };
                Instance.entities.Add(id, node);
            }

            return node;
        }

        /// <summary>
        /// Register an entity without a gameobject.
        /// </summary>
        /// <param name="id">unique id of the node.</param>
        /// <param name="dto">dto of the node.</param>
        /// <returns></returns>
        public static UMI3DEntityInstance RegisterEntityInstance(ulong id, UMI3DDto dto, object Object, Action delete = null)
        {
            UMI3DEntityInstance node = null;
            if (!Exists)
            {
                return null;
            }
            else if (Instance.entities.ContainsKey(id))
            {
                node = Instance.entities[id];
            }
            else
            {
                node = new UMI3DEntityInstance(() => NotifyEntityLoad(id)) { dto = dto, Object = Object, Delete = delete };
                Instance.entities.Add(id, node);
            }
            return node;
        }

        /// <summary>
        /// Index of any 3D object loaded.
        /// </summary>
        private GlTFEnvironmentDto environment;

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
            //UMI3DLogger.Log("GetBaseMaterial",scope);
            if (baseMaterial == null)
            {
                throw new Exception("Base Material on UMI3DEnvironmentLoader should never be null");
            }
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

        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
            sceneLoader = new UMI3DSceneLoader();
            nodeLoader = new GlTFNodeLoader();
        }

        #region workflow

        /// <summary>
        /// Indicates if the UMI3D environment has been fully loaded
        /// </summary>
        public bool loaded { get; private set; } = false;

        public UnityEvent onResourcesLoaded = new UnityEvent();
        public UnityEvent onEnvironmentLoaded = new UnityEvent();

        /// <summary>
        /// Is environement (except videos) loaded ?
        /// </summary>
        public bool isEnvironmentLoaded = false;

        /// <summary>
        /// Load the Environment.
        /// </summary>
        /// <param name="dto">Dto of the environement.</param>
        /// <param name="onSuccess">Finished callback.</param>
        /// <param name="onError">Error callback.</param>
        /// <returns></returns>
        public async Task Load(GlTFEnvironmentDto dto, MultiProgress LoadProgress)
        {
            Progress downloadingProgress = new Progress(0, "Downloading");
            Progress ReadingDataProgress = new Progress(2, "Reading Data");
            MultiProgress loadingProgress = new MultiProgress("Loading");
            Progress endProgress = new Progress(6, "Cleaning the room");
            LoadProgress.Add(downloadingProgress);
            LoadProgress.Add(ReadingDataProgress);
            LoadProgress.Add(loadingProgress);
            LoadProgress.Add(endProgress);

            if (baseMaterial == null)
            {
                throw new Exception("Base Material on UMI3DEnvironmentLoader should never be null");
            }

            isEnvironmentLoaded = false;

            environment = dto;
            RegisterEntityInstance(UMI3DGlobalID.EnvironementId, dto, null).NotifyLoaded();
            //
            // Load resources
            //
            await LoadResources(dto, downloadingProgress);

            onResourcesLoaded.Invoke();

            ReadingDataProgress.AddComplete();
            //
            // Instantiate nodes
            //
            await ReadUMI3DExtension(dto, null);
            ReadingDataProgress.AddComplete();
            await InstantiateNodes(loadingProgress);

            endProgress.AddComplete();
            await UMI3DAsyncManager.Delay(200);

            endProgress.AddComplete();
            if (UMI3DVideoPlayerLoader.HasVideoToLoad)
            {
                endProgress.SetStatus("Loading videos");
                Debug.Log("wait for video");
                await UMI3DVideoPlayerLoader.LoadVideoPlayers();
                Debug.Log("wait for video end");
            }
            endProgress.AddComplete();

            endProgress.SetStatus("Rendering Probes");
            if (QualitySettings.realtimeReflectionProbes)
            {
                await RenderProbes();
            }
            else
            {
                Debug.Log("Rendering probes not enabled on this browser.");
            }
            endProgress.AddComplete();
            await UMI3DAsyncManager.Yield();

            endProgress.SetStatus("Updating the world");
            await WaitForFirstTransaction();
            endProgress.AddComplete();
            await UMI3DAsyncManager.Yield();

            isEnvironmentLoaded = true;
            onEnvironmentLoaded.Invoke();

            await UMI3DAsyncManager.Yield();
            endProgress.AddComplete();
        }

        protected virtual Task WaitForFirstTransaction()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Renders all <see cref="ReflectionProbe"/> set to <see cref=" ReflectionProbeMode.Realtime"/> 
        /// and <see cref="ReflectionProbeRefreshMode.OnAwake"/> of the environment.
        /// </summary>
        private async Task RenderProbes()
        {
            List<(ReflectionProbe probe, int id)> probeList = new List<(ReflectionProbe, int)>();

            foreach (var entity in entities)
            {
                if (entity.Value.dto is GlTFSceneDto && entity.Value is UMI3DNodeInstance scene)
                {
                    foreach (var probe in scene.gameObject.GetComponentsInChildren<ReflectionProbe>())
                    {
                        if (probe.mode == ReflectionProbeMode.Realtime && probe.refreshMode == ReflectionProbeRefreshMode.OnAwake)
                        {
                            var id = probe.RenderProbe();
                            probeList.Add((probe, id));
                        }
                    }
                }
            }
            await Task.WhenAll(probeList.Select(
                async p =>
                {
                    while (!p.probe.IsFinishedRendering(p.id))
                        await UMI3DAsyncManager.Yield();
                }));
        }

        #endregion

        #region resources

        /// <summary>
        /// Load the environment's resources
        /// </summary>
        private async Task LoadResources(GlTFEnvironmentDto dto, Progress progress)
        {
            List<string> ids = dto.extensions.umi3d.LibrariesId;
            foreach (GlTFSceneDto scene in dto.scenes)
                ids.AddRange(scene.extensions.umi3d.LibrariesId);
            await UMI3DResourcesManager.LoadLibraries(ids, progress);
        }

        #endregion

        #region parameters

        [SerializeField]
        private AbstractUMI3DLoadingParameters parameters = null;
        public static AbstractUMI3DLoadingParameters Parameters => Exists ? Instance.parameters : null;

        #endregion

        #region instantiation

        /// <summary>
        /// Load the environment's resources
        /// </summary>
        private async Task InstantiateNodes(MultiProgress progress)
        {
            await _InstantiateNodes(environment.scenes, progress);
            loaded = true;
        }

        /// <summary>
        /// Load scenes 
        /// </summary>
        /// <param name="scenes">scenes to loads</param>
        /// <returns></returns>
        private async Task _InstantiateNodes(List<GlTFSceneDto> scenes, MultiProgress progress)
        {

            //Load scenes without hierarchy
            await Task.WhenAll(scenes.Select(async scene =>
            {
                Progress progress1 = new Progress(0, $"Load scene {scene.name}");
                progress.Add(progress1);
                await sceneLoader.LoadGlTFScene(scene, progress1);

            }));
            //Organize scenes
            await Task.WhenAll(scenes.Select(async scene =>
            {
                Progress progress1 = new Progress(2, $"Generate scene {scene.name}");
                progress.Add(progress1);
                progress1.AddComplete();
                var node = entities[scene.extensions.umi3d.id] as UMI3DNodeInstance;
                UMI3DSceneNodeDto umi3dScene = scene.extensions.umi3d;
                await sceneLoader.ReadUMI3DExtension(umi3dScene, node.gameObject);
                progress1.AddComplete();
                node.gameObject.SetActive(true);
            }));
        }

        #endregion

        /// <summary>
        /// Load IEntity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="performed"></param>
        public static async Task LoadEntity(IEntity entity)
        {
            if (Exists) await Instance._LoadEntity(entity);
        }

        public static async Task LoadEntity(ByteContainer container)
        {
            if (Exists)
                await Instance._LoadEntity(container);
        }

        /// <summary>
        /// Load IEntity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="performed"></param>
        private async Task _LoadEntity(IEntity entity)
        {
            try
            {
                switch (entity)
                {
                    case GlTFSceneDto scene:
                        await _InstantiateNodes(new List<GlTFSceneDto>() { scene }, new MultiProgress("Load Entity"));
                        break;
                    case GlTFNodeDto node:
                        await nodeLoader.LoadNodes(new List<GlTFNodeDto>() { node }, new Progress(0, "Load Entity"));
                        break;
                    case AssetLibraryDto library:
                        await UMI3DResourcesManager.DownloadLibrary(library, UMI3DClientServer.Media.name, new MultiProgress("Load Entity"));
                        await UMI3DResourcesManager.LoadLibrary(library.libraryId);
                        break;
                    case AbstractEntityDto dto:
                        await Parameters.ReadUMI3DExtension(dto, null);
                        break;
                    case GlTFMaterialDto matDto:
                        Parameters.SelectMaterialLoader(matDto).LoadMaterialFromExtension(matDto, (m) =>
                        {

                            if (matDto.name != null && matDto.name.Length > 0)
                                m.name = matDto.name;
                            //register the material
                            if (m == null)
                            {
                                RegisterEntityInstance(((AbstractEntityDto)matDto.extensions.umi3d).id, matDto, new List<Material>()).NotifyLoaded();
                            }
                            else
                            {
                                RegisterEntityInstance(((AbstractEntityDto)matDto.extensions.umi3d).id, matDto, m).NotifyLoaded();
                            }
                        });
                        break;
                    default:
                        UMI3DLogger.Log($"load entity fail missing case {entity.GetType()}", scope);
                        break;
                }
            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
            }
        }

        /// <summary>
        /// Load IEntity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="performed"></param>
        private async Task _LoadEntity(ByteContainer container)
        {
            List<ulong> ids = UMI3DNetworkingHelper.ReadList<ulong>(container);
            ids.ForEach(id => NotifyEntityToBeLoaded(id));

            try
            {
                var load = await UMI3DClientServer.GetEntity(ids);

                await Task.WhenAll(
                    load.entities.Select(async item =>
                    {
                        if (item is MissingEntityDto missing)
                        {
                            NotifyEntityFailedToLoad(missing.id);
                            UMI3DLogger.Log($"Get entity [{missing.id}] failed : {missing.reason}", scope);
                        }
                        else
                            await LoadEntity(item);
                    }));

            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
            }
        }

        /// <summary>
        /// Delete IEntity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="performed"></param>
        public static async Task DeleteEntity(ulong entityId)
        {
            if (Instance.entities.ContainsKey(entityId))
            {
                UMI3DEntityInstance entity = Instance.entities[entityId];

                if (entity.Object is UMI3DAbstractAnimation animation)
                {
                    animation.Stop();
                }

                if (entity is UMI3DNodeInstance)
                {
                    var node = entity as UMI3DNodeInstance;
                    Destroy(node.gameObject);
                }
                Instance.entities[entityId].Delete?.Invoke();
                Instance.entities.Remove(entityId);
            }
            else if (UMI3DResourcesManager.isKnowedLibrary(entityId))
            {
                UMI3DResourcesManager.UnloadLibrary(entityId);
            }
            else if (UMI3DEnvironmentLoader.IsEntityToBeLoaded(entityId))
            {
                var e = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(entityId);
                await DeleteEntity(entityId);
            }
            else if (UMI3DEnvironmentLoader.IsEntityToFailedBeLoaded(entityId))
            {
                UMI3DEnvironmentLoader.RemoveEntityToFailedBeLoaded(entityId);
            }
            else
            {
                UMI3DLogger.LogError($"Entity [{entityId}] To Destroy Not Found And Not in Entities to be loaded", scope);
            }
        }

        /// <summary>
        /// Clear an environement and make the client ready to load a new environment.
        /// </summary>
        public static void Clear(bool clearCache = true)
        {
            Instance.entityFilters.Clear();

            foreach (ulong entity in Instance.entities.ToList().Select(p => { return p.Key; }))
            {
                DeleteEntity(entity);
            }
            if (clearCache)
                UMI3DResourcesManager.Instance.ClearCache();

            Instance.InternalClear();
        }

        protected virtual void InternalClear()
        {
            UMI3DVideoPlayerLoader.Clear();

            entities.Clear();
            entitywaited.Clear();
            entityToBeLoaded.Clear();
            Instance.entityFailedToBeLoaded.Clear();

            isEnvironmentLoaded = false;

            environment = null;
            loaded = false;
        }

        /// <summary>
        /// Load environment.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="node"></param>
        public virtual async Task ReadUMI3DExtension(GlTFEnvironmentDto dto, GameObject node)
        {
            UMI3DEnvironmentDto extension = dto?.extensions?.umi3d;
            if (extension != null)
            {
                if (extension.defaultMaterial != null && extension.defaultMaterial.variants != null && extension.defaultMaterial.variants.Count > 0)
                {
                    baseMaterial = null;
                    LoadDefaultMaterial(extension.defaultMaterial);
                }
                foreach (PreloadedSceneDto scene in extension.preloadedScenes)
                    await Parameters.ReadUMI3DExtension(scene, node);

                RenderSettings.ambientMode = (AmbientMode)extension.ambientType;
                RenderSettings.ambientSkyColor = extension.skyColor;
                RenderSettings.ambientEquatorColor = extension.horizontalColor;
                RenderSettings.ambientGroundColor = extension.groundColor;
                RenderSettings.ambientIntensity = extension.ambientIntensity;
                if (extension.skybox != null)
                {
                    Parameters.LoadSkybox(extension.skybox, extension.skyboxType, extension.skyboxRotation, extension.skyboxExposure);
                }
            }
        }

        /// <summary>
        /// Load DefaultMaterial from matDto
        /// </summary>
        /// <param name="matDto"></param>
        private async void LoadDefaultMaterial(ResourceDto matDto)
        {
            FileDto fileToLoad = Parameters.ChooseVariant(matDto.variants);
            if (fileToLoad == null) return;
            string ext = fileToLoad.extension;
            IResourcesLoader loader = Parameters.SelectLoader(ext);
            if (loader != null)
            {
                var mat = await UMI3DResourcesManager.LoadFile(UMI3DGlobalID.EnvironementId, fileToLoad, loader);
                SetBaseMaterial((Material)mat);
            }
        }

        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="property">Property containing the new value.</param>
        /// <returns></returns>
        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (Exists)
                return Instance._SetUMI3DProperty(entity, property);
            else
                return false;
        }

        public static bool ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
        {
            if (Exists)
                return Instance._ReadUMI3DProperty(ref value, propertyKey, container);
            else
                return false;
        }


        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="property">Property containing the new value.</param>
        /// <returns></returns>
        public static bool SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (Exists)
                return Instance._SetUMI3DProperty(entity, operationId, propertyKey, container);
            else
                return false;
        }


        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="property">Property containing the new value.</param>
        /// <returns></returns>
        protected virtual bool _SetUMI3DProperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (entity == null) return false;
            UMI3DEnvironmentDto dto = ((entity.dto as GlTFEnvironmentDto)?.extensions)?.umi3d;
            if (dto == null) return false;

            switch (property.property)
            {
                case UMI3DPropertyKeys.PreloadedScenes:
                    return Parameters.SetUMI3DProperty(entity, property);
                case UMI3DPropertyKeys.AmbientType:
                    RenderSettings.ambientMode = (AmbientMode)property.value;
                    return true;
                case UMI3DPropertyKeys.AmbientSkyColor:
                    RenderSettings.ambientSkyColor = (SerializableColor)property.value;
                    return true;
                case UMI3DPropertyKeys.AmbientHorizontalColor:
                    RenderSettings.ambientEquatorColor = (SerializableColor)property.value;
                    return true;
                case UMI3DPropertyKeys.AmbientGroundColor:
                    RenderSettings.ambientGroundColor = (SerializableColor)property.value;
                    return true;
                case UMI3DPropertyKeys.AmbientIntensity:
                    RenderSettings.ambientIntensity = (float)property.value;
                    return true;
                case UMI3DPropertyKeys.AmbientSkyboxImage:
                    if (dto.skybox != null)
                    {
                        Parameters.LoadSkybox(dto.skybox, dto.skyboxType, dto.skyboxRotation, dto.skyboxExposure);
                    }
                    return true;
                case UMI3DPropertyKeys.AmbientSkyboxRotation:
                case UMI3DPropertyKeys.AmbientSkyboxExposure:
                    return Parameters.SetSkyboxProperties(dto.skyboxType, dto.skyboxRotation, dto.skyboxExposure);
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
        protected virtual bool _SetUMI3DProperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (entity == null) return false;
            UMI3DEnvironmentDto dto = ((entity.dto as GlTFEnvironmentDto)?.extensions)?.umi3d;
            if (dto == null) return false;

            switch (propertyKey)
            {
                case UMI3DPropertyKeys.PreloadedScenes:
                    return Parameters.SetUMI3DProperty(entity, operationId, propertyKey, container);
                case UMI3DPropertyKeys.AmbientType:
                    RenderSettings.ambientMode = (AmbientMode)UMI3DNetworkingHelper.Read<int>(container);
                    return true;
                case UMI3DPropertyKeys.AmbientSkyColor:
                    RenderSettings.ambientSkyColor = UMI3DNetworkingHelper.Read<SerializableColor>(container);
                    return true;
                case UMI3DPropertyKeys.AmbientHorizontalColor:
                    RenderSettings.ambientEquatorColor = UMI3DNetworkingHelper.Read<SerializableColor>(container);
                    return true;
                case UMI3DPropertyKeys.AmbientGroundColor:
                    RenderSettings.ambientGroundColor = UMI3DNetworkingHelper.Read<SerializableColor>(container);
                    return true;
                case UMI3DPropertyKeys.AmbientIntensity:
                    RenderSettings.ambientIntensity = UMI3DNetworkingHelper.Read<float>(container);
                    return true;
                case UMI3DPropertyKeys.AmbientSkyboxImage:
                    dto.skybox = UMI3DNetworkingHelper.Read<ResourceDto>(container);
                    Parameters.LoadSkybox(dto.skybox, dto.skyboxType, dto.skyboxRotation, dto.skyboxExposure);
                    return true;
                case UMI3DPropertyKeys.AmbientSkyboxRotation:
                    dto.skyboxRotation = UMI3DNetworkingHelper.Read<float>(container);
                    return Parameters.SetSkyboxProperties(dto.skyboxType, dto.skyboxRotation, dto.skyboxExposure);
                case UMI3DPropertyKeys.AmbientSkyboxExposure:
                    dto.skyboxExposure = UMI3DNetworkingHelper.Read<float>(container);
                    return Parameters.SetSkyboxProperties(dto.skyboxType, dto.skyboxRotation, dto.skyboxExposure);
                default:
                    return false;
            }
        }

        protected virtual bool _ReadUMI3DProperty(ref object value, uint propertyKey, ByteContainer container)
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
        public static void SetEntity(SetEntityPropertyDto dto)
        {
            if (!Exists) return;
            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.entityId, (e) =>
            {
                if (!SetEntity(e, dto))
                    UMI3DLogger.LogWarning("SetEntity operation was not applied : entity : " + dto.entityId + "   propKey : " + dto.property, scope);
            });


        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public static void SetEntity(uint operationId, ulong entityId, uint propertyKey, ByteContainer container)
        {
            UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(entityId, (e) =>
            {
                if (!SetEntity(e, operationId, entityId, propertyKey, container))
                    UMI3DLogger.LogWarning("SetEntity operation was not applied : entity : " + entityId + "  operation : " + operationId + "   propKey : " + propertyKey, scope);
            }
            );
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
                Instance.entityFilters[dto.entityId][dto.property].AddMeasure(dto.value.Deserialize());
                return true;
            }
            else
            {
                if (SetUMI3DProperty(node, dto)) return true;
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
                object value = null;
                ReadValueEntity(ref value, propertyKey, container);
                Instance.entityFilters[entityId][propertyKey].AddMeasure(value.Deserialize());
                return true;
            }
            else
            {
                if (SetUMI3DProperty(node, operationId, propertyKey, container)) return true;
                if (UMI3DEnvironmentLoader.Exists && UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(node, operationId, propertyKey, container)) return true;
                return Parameters.SetUMI3DProperty(node, operationId, propertyKey, container);
            }
        }

        private static bool ReadValueEntity(ref object value, uint propertyKey, ByteContainer container)
        {
            if (ReadUMI3DProperty(ref value, propertyKey, container)) return true;
            if (UMI3DEnvironmentLoader.Exists && UMI3DEnvironmentLoader.Instance.sceneLoader.ReadUMI3DProperty(ref value, propertyKey, container)) return true;
            return Parameters.ReadUMI3DProperty(ref value, propertyKey, container);
        }


        private static bool SimulatedSetEntity(UMI3DEntityInstance node, SetEntityPropertyDto dto)
        {
            if (SetUMI3DProperty(node, dto)) return true;
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
                    var entityPropertyDto = new SetEntityPropertyDto()
                    {
                        entityId = id,
                        property = dto.property,
                        value = dto.value
                    };

                    UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(id, (e) =>
                    {
                        SetEntity(e, entityPropertyDto);
                    });
                }
                catch (Exception e)
                {
                    UMI3DLogger.LogWarning("SetEntity not apply on this object, id = " + id + " ,  property = " + dto.property, scope);
                    UMI3DLogger.LogException(e, scope);
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
            List<ulong> idList = UMI3DNetworkingHelper.ReadList<ulong>(container);
            uint operationId = UMI3DNetworkingHelper.Read<uint>(container);
            uint propertyKey = UMI3DNetworkingHelper.Read<uint>(container);

            foreach (ulong id in idList)
            {
                try
                {
                    WaitForAnEntityToBeLoaded(id, (e) =>
                    {
                        var newContainer = new ByteContainer(container);
                        if (!SetEntity(e, operationId, id, propertyKey, newContainer))
                            UMI3DLogger.LogWarning($"A SetUMI3DProperty failed to match any loader {id} {operationId} {propertyKey} {newContainer}", scope | DebugScope.Bytes);
                    });
                }
                catch (Exception e)
                {
                    UMI3DLogger.LogWarning($"SetEntity not apply on this object, id = {id},  operation = {operationId} ,  property = {propertyKey}", scope | DebugScope.Bytes);
                    UMI3DLogger.LogException(e, scope);
                }
            }
            return true;
        }

        #region interpolation



        private readonly Dictionary<ulong, Dictionary<ulong, AbstractExtrapolator>> entityFilters = new Dictionary<ulong, Dictionary<ulong, AbstractExtrapolator>>();

        private void Update()
        {
            foreach (ulong entityId in Instance.entityFilters.Keys)
            {
                foreach (ulong property in Instance.entityFilters[entityId].Keys)
                {
                    UMI3DEntityInstance node = UMI3DEnvironmentLoader.GetEntity(entityId);
                    AbstractExtrapolator extrapolator = Instance.entityFilters[entityId][property];

                    extrapolator.UpdateRegressedValue();

                    var entityPropertyDto = new SetEntityPropertyDto()
                    {
                        entityId = extrapolator.entityId,
                        property = extrapolator.property,
                        value = extrapolator.GetRegressedValue().ToSerializable()
                    };

                    SimulatedSetEntity(node, entityPropertyDto);
                }
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
            WaitForAnEntityToBeLoaded(dto.entityId, (e) =>
            {
                Instance.StartInterpolation(e, dto.entityId, dto.property, dto.startValue);
            }
            );
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
            ulong entityId = UMI3DNetworkingHelper.Read<ulong>(container);
            uint propertyKey = UMI3DNetworkingHelper.Read<uint>(container);
            WaitForAnEntityToBeLoaded(entityId, (e) =>
            {
                object value = null;
                ReadValueEntity(ref value, propertyKey, container);
                Instance.StartInterpolation(e, entityId, propertyKey, value.Deserialize());
            }
            );
            return true;
        }

        protected bool StartInterpolation(UMI3DEntityInstance node, ulong entityId, ulong propertyKey, object startValue)
        {
            if (!entityFilters.ContainsKey(entityId))
            {
                entityFilters.Add(entityId, new Dictionary<ulong, AbstractExtrapolator>());
            }

            if (!entityFilters[entityId].ContainsKey(propertyKey))
            {
                AbstractExtrapolator newExtrapolator;
                if (propertyKey == UMI3DPropertyKeys.Rotation)
                {
                    newExtrapolator = new QuaternionLinearDelayedExtrapolator()
                    {
                        entityId = entityId,
                        property = propertyKey
                    };
                }
                else
                {
                    newExtrapolator = new Vector3LinearDelayedExtrapolator()
                    {
                        entityId = entityId,
                        property = propertyKey
                    };
                }

                entityFilters[entityId].Add(propertyKey, newExtrapolator);

                newExtrapolator.AddMeasure(startValue);

                var entityPropertyDto = new SetEntityPropertyDto()
                {
                    entityId = entityId,
                    property = propertyKey,
                    value = startValue.ToSerializable()
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
            WaitForAnEntityToBeLoaded(dto.entityId, e =>
            {
                Instance.StopInterpolation(e, dto.entityId, dto.property, dto.stopValue);
            }
             );
            return true;
        }

        public static bool StopInterpolation(ByteContainer container)
        {
            if (!Exists) return false;
            ulong entityId = UMI3DNetworkingHelper.Read<ulong>(container);
            uint propertyKey = UMI3DNetworkingHelper.Read<uint>(container);
            WaitForAnEntityToBeLoaded(entityId, (e) =>
            {
                object value = null;
                ReadValueEntity(ref value, propertyKey, container);
                Instance.StopInterpolation(e, entityId, propertyKey, value.Deserialize());
            });

            return true;
        }


        protected bool StopInterpolation(UMI3DEntityInstance node, ulong entityId, uint property, object stopValue)
        {
            if (entityFilters.ContainsKey(entityId) && entityFilters[entityId].ContainsKey(property))
            {
                Instance.entityFilters[entityId].Remove(property);
                var entityPropertyDto = new SetEntityPropertyDto()
                {
                    entityId = entityId,
                    property = property,
                    value = stopValue.ToSerializable()
                };

                SetEntity(node, entityPropertyDto);

                return true;
            }
            return false;
        }

        #endregion

        #region Navmesh

        public delegate void NodeNavmeshModifiedDelegate(UMI3DNodeInstance node);

        public event NodeNavmeshModifiedDelegate onNodePartOfNavmeshSet;

        /// <summary>
        /// Notify browser that a <see cref="UMI3DNodeInstance"/> has changed its part of navmesh status.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="isPartOfNavmesh"></param>
        public void SetNodePartOfNavmesh(UMI3DNodeInstance node)
        {
            onNodePartOfNavmeshSet?.Invoke(node);
        }

        public event NodeNavmeshModifiedDelegate onNodeTraversableSet;

        /// <summary>
        /// Notify browser that a <see cref="UMI3DNodeInstance"/> has changed its traversable status.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="isTraversable"></param>
        public void SetNodeTraversable(UMI3DNodeInstance node)
        {
            onNodeTraversableSet?.Invoke(node);
        }

        #endregion
    }
}