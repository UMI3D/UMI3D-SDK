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
using System.Threading;
using inetum.unityUtils;

namespace umi3d.cdk
{
    /// <summary>
    /// Loader for <see cref="UMI3DEnvironmentDto"/>.
    /// </summary>
    public class UMI3DEnvironmentLoader : inetum.unityUtils.Singleton<UMI3DEnvironmentLoader>, IEnvironmentManager, ILoadingManager, INavMeshManager
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Core | DebugScope.Loading;

        /// <summary>
        /// Index of any 3D object loaded.
        /// </summary>
        private readonly Dictionary<ulong, UMI3DEntityInstance> entities = new Dictionary<ulong, UMI3DEntityInstance>();
        private readonly Dictionary<ulong, List<(Action<UMI3DEntityInstance>, Action)>> entitywaited = new Dictionary<ulong, List<(Action<UMI3DEntityInstance>, Action)>>();
        private readonly HashSet<ulong> entityToBeLoaded = new HashSet<ulong>();
        private readonly HashSet<ulong> entityFailedToBeLoaded = new HashSet<ulong>();

        public UMI3DEnvironmentLoader() : base()
        {
            sceneLoader = new UMI3DSceneLoader();
            nodeLoader = new GlTFNodeLoader();

            onEnvironmentLoaded.AddListener(() => InterpolationRoutine());
        }

        public UMI3DEnvironmentLoader(Material baseMaterial, IUMI3DAbstractLoadingParameters parameters)
            : this()
        {
            _baseMaterial = new Material(baseMaterial);
            this.parameters = parameters;
        }

        /// <summary>
        /// Anchor of loading.
        /// </summary>
        /// For backwards compatibility only.
        [Obsolete("UMI3DLoadingHandler.Instance.gameObject instead")]
        public GameObject gameObject => UMI3DLoadingHandler.Exists ? UMI3DLoadingHandler.Instance.gameObject : null;

        /// <summary>
        /// Anchor of loading transform.
        /// </summary>
        /// For backwards compatibility only.
        [Obsolete("UMI3DLoadingHandler.Instance.transform instead")]
        public Transform transform => UMI3DLoadingHandler.Instance.transform;

        /// <summary>
        /// Anchor of loading access to coroutine logic.
        /// </summary>
        /// For backwards compatibility only.
        [Obsolete("Use ICoroutineManager instead")]
        public static Coroutine StartCoroutine(IEnumerator enumerator) => CoroutineManager.Instance.AttachCoroutine(enumerator);

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

            Instance.WaitUntilEntityLoaded(id, entityLoaded, entityFailedToLoad);
        }

        /// <summary>
        /// Call a callback when an entity is registerd.
        /// The entity might not be totaly loaded when the callback is called.
        /// all property of UMI3DEntityInstance and UMI3DNodeInstance are set.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entityLoaded"></param>
        /// <param name="entityFailedToLoad"></para
        public virtual void WaitUntilEntityLoaded(ulong id, Action<UMI3DEntityInstance> entityLoaded, Action entityFailedToLoad = null)
        {
            if (Instance.entitywaited == null) return;
            UMI3DEntityInstance node = TryGetEntityInstance(id);
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

            return;
        }

        public static async Task<UMI3DEntityInstance> WaitForAnEntityToBeLoaded(ulong id, List<CancellationToken> tokens)
        {
            if (!Exists)
                throw new Umi3dException("EnvironmentLoader does not exist");

            if (Instance.entitywaited == null)
                throw new Umi3dException("Entity waited to be loaded does not exist");

            return await Instance.WaitUntilEntityLoaded(id, tokens);
        }

        public virtual async Task<UMI3DEntityInstance> WaitUntilEntityLoaded(ulong id, List<CancellationToken> tokens)
        {
            if (entitywaited == null)
                throw new Umi3dException("Entity waited to be loaded does not exist");

            UMI3DEntityInstance node = TryGetEntityInstance(id);
            if (node != null && node.IsLoaded)
            {
                return (node);
            }
            UMI3DEntityInstance loaded = null;
            bool error = false;
            bool finished = false;

            Action<UMI3DEntityInstance> entityLoaded = (e) => { loaded = e; finished = true; };
            Action entityFailedToLoad = () => { error = true; finished = true; };

            WaitUntilEntityLoaded(id, entityLoaded, entityFailedToLoad);

            while (!finished)
                await UMI3DAsyncManager.Yield(tokens);
            if (error)
                throw new Umi3dException($"Failed to load entity. Entity id: {id}.");

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
        [Obsolete("Use GetEntityInstance or GetEntityObject<T> instead.")]
        public static UMI3DEntityInstance GetEntity(ulong id) { return id != 0 && Exists && Instance.entities.ContainsKey(id) ? Instance.entities[id] : null; }

        /// <summary>
        /// Get an entity with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public virtual UMI3DEntityInstance GetEntityInstance(ulong id)
        {
            if (id == 0 || !entities.ContainsKey(id))
                throw new ArgumentException(message: $"Entity {id} does not exist.");
            else if (entities[id] != null)
                return entities[id];
            else
                throw new Umi3dException($"Entity {id} is referenced but is null.");
        }

        /// <summary>
        /// Get an entity with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public virtual UMI3DEntityInstance TryGetEntityInstance(ulong id)
        {
            if (id == 0)
                throw new ArgumentException(message: $"Entity {id} does not exist.");
            else if (!entities.ContainsKey(id))
                return null;
            else if (entities[id] != null)
                return entities[id];
            else
                throw new Umi3dException($"Entity {id} is referenced but is null.");
        }

        /// <summary>
        /// Get an entity with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public virtual T GetEntityObject<T>(ulong id) where T : class
        {
            var entity = GetEntityInstance(id);
            if (entity.Object is T objEntity)
                return objEntity;
            else
                throw new Umi3dException($"Entity {id} [{entity}:{entity?.GetType()}] is not of type {nameof(T)}.");
        }

        /// <summary>
        /// Get a node with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public static UMI3DNodeInstance GetNode(ulong id) { return id != 0 && Exists && Instance.entities.ContainsKey(id) ? Instance.entities[id] as UMI3DNodeInstance : null; }

        /// <summary>
        /// Get a node with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns>Node instance or null if node does not exist.</returns>
        public virtual UMI3DNodeInstance GetNodeInstance(ulong id)
        {
            if (GetEntityInstance(id) is not UMI3DNodeInstance node)
                throw new Umi3dException($"Entity {id} is not an UMI3DNodeInstance.");
            return node;
        }

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
                    UnityEngine.Object.Destroy(instance);
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
        [Obsolete("Use Instance.RegisterEntity() instead")]
        public static UMI3DEntityInstance RegisterEntityInstance(ulong id, UMI3DDto dto, object Object, Action delete = null)
        {
            if (!Exists)
                return null;
            else
                return Instance.RegisterEntity(id, dto, Object, delete);
        }

        /// <summary>
        /// Register an entity without a gameobject.
        /// </summary>
        /// <param name="id">unique id of the node.</param>
        /// <param name="dto">dto of the node.</param>
        /// <returns></returns>
        public virtual UMI3DEntityInstance RegisterEntity(ulong id, UMI3DDto dto, object objectInstance, Action delete = null)
        {
            UMI3DEntityInstance node = null;
            if (!Exists)
                throw new Umi3dException("Cannot register entity. Loader does not exist.");

            else if (entities.ContainsKey(id))
                node = entities[id];

            else
            {
                node = new UMI3DEntityInstance(() => NotifyEntityLoad(id)) { dto = dto, Object = objectInstance, Delete = delete };
                entities.Add(id, node);
            }

            return node;
        }

        /// <summary>
        /// Index of any 3D object loaded.
        /// </summary>
        private GlTFEnvironmentDto environment;

        public UMI3DSceneLoader sceneLoader { get; private set; }
        public GlTFNodeLoader nodeLoader { get; private set; }

        private Material _baseMaterial;
        /// <summary>
        /// Basic material used to init a new material with the same properties
        /// </summary>
        public Material baseMaterial
        {
            get
            {
                if (_baseMaterial == null)
                    throw new Umi3dException("Base Material on UMI3DEnvironmentLoader should never be null");
                return _baseMaterial;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newBaseMat">A new material to override the baseMaterial used to initialise all materials</param>
        internal void SetBaseMaterial(Material newBaseMat)
        {
            _baseMaterial = new Material(newBaseMat);
        }

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

        #region workflow

        /// <summary>
        /// Indicates if the UMI3D environment has been fully loaded
        /// </summary>
        public bool loaded { get; private set; } = false;

        public UnityEvent onResourcesLoaded { get; protected set; } = new UnityEvent();
        public UnityEvent onEnvironmentLoaded { get; protected set; } = new UnityEvent();

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

                await UMI3DVideoPlayerLoader.LoadVideoPlayers();
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

        private IUMI3DAbstractLoadingParameters parameters = null;
        public static IUMI3DAbstractLoadingParameters AbstractParameters => Exists ? Instance.parameters : null;

        public static IUMI3DLoadingParameters Parameters => Exists ? Instance.parameters as IUMI3DLoadingParameters : null;
        public virtual IUMI3DAbstractLoadingParameters AbstractLoadingParameters => parameters;

        public IUMI3DLoadingParameters LoadingParameters => parameters as IUMI3DLoadingParameters;

        internal void SetParameters(IUMI3DAbstractLoadingParameters parameters)
        {
            this.parameters = parameters;
        }

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
                await sceneLoader.ReadUMI3DExtension(new ReadUMI3DExtensionData(umi3dScene, node.gameObject));
                progress1.AddComplete();
                node.gameObject.SetActive(umi3dScene.active);
            }));
        }

        #endregion

        /// <summary>
        /// Load IEntity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="performed"></param>
        public static async Task LoadEntity(IEntity entity, List<CancellationToken> tokens)
        {
            if (Exists) await Instance._LoadEntity(entity, tokens);
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
        private async Task _LoadEntity(IEntity entity, List<CancellationToken> tokens)
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
                        await AbstractParameters.ReadUMI3DExtension(new ReadUMI3DExtensionData(dto, tokens));
                        break;
                    case GlTFMaterialDto matDto:
                        AbstractParameters.SelectMaterialLoader(matDto).LoadMaterialFromExtension(matDto, (m) =>
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
            List<ulong> ids = UMI3DSerializer.ReadList<ulong>(container);
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
                            await LoadEntity(item, container.tokens);
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
        public static async Task DeleteEntity(ulong entityId, List<CancellationToken> tokens)
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
                    node.ClearBeforeDestroy();
                    UnityEngine.Object.Destroy(node.gameObject);
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
                var e = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(entityId, tokens);
                await DeleteEntity(entityId, tokens);
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
                DeleteEntity(entity, null);
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
                    _baseMaterial = null;
                    LoadDefaultMaterial(extension.defaultMaterial);
                }
                foreach (PreloadedSceneDto scene in extension.preloadedScenes)
                    await AbstractParameters.ReadUMI3DExtension(new ReadUMI3DExtensionData(scene, node));
                RenderSettings.ambientMode = (AmbientMode)extension.ambientType;
                RenderSettings.ambientSkyColor = extension.skyColor.Struct();
                RenderSettings.ambientEquatorColor = extension.horizontalColor.Struct();
                RenderSettings.ambientGroundColor = extension.groundColor.Struct();
                RenderSettings.ambientIntensity = extension.ambientIntensity;
                if (extension.skybox != null)
                {
                    AbstractParameters.LoadSkybox(extension.skybox, extension.skyboxType, extension.skyboxRotation, extension.ambientIntensity);
                }
            }
        }

        /// <summary>
        /// Load DefaultMaterial from matDto
        /// </summary>
        /// <param name="matDto"></param>
        private async void LoadDefaultMaterial(ResourceDto matDto)
        {
            FileDto fileToLoad = AbstractParameters.ChooseVariant(matDto.variants);
            if (fileToLoad == null) return;
            string ext = fileToLoad.extension;
            IResourcesLoader loader = AbstractParameters.SelectLoader(ext);
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
        public static async Task<bool> SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            if (Exists)
                return await Instance._SetUMI3DProperty(data);
            else
                return false;
        }

        public static async Task<bool> ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            if (Exists)
                return await Instance._ReadUMI3DProperty(data);
            else
                return false;
        }


        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="property">Property containing the new value.</param>
        /// <returns></returns>
        public static async Task<bool> SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            if (Exists)
                return await Instance._SetUMI3DProperty(data);
            else
                return false;
        }


        /// <summary>
        /// Update a property.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <param name="property">Property containing the new value.</param>
        /// <returns></returns>
        protected virtual async Task<bool> _SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            if (data.entity == null) return false;
            UMI3DEnvironmentDto dto = ((data.entity.dto as GlTFEnvironmentDto)?.extensions)?.umi3d;
            if (dto == null) return false;
            switch (data.property.property)
            {
                case UMI3DPropertyKeys.PreloadedScenes:
                    return await AbstractParameters.SetUMI3DProperty(data);
                case UMI3DPropertyKeys.AmbientType:
                    RenderSettings.ambientMode = (AmbientMode)data.property.value;
                    return AbstractParameters.SetSkyboxProperties(dto.skyboxType, dto.skyboxRotation, RenderSettings.ambientIntensity);
                case UMI3DPropertyKeys.AmbientSkyColor:
                    RenderSettings.ambientSkyColor = ((ColorDto)data.property.value).Struct();
                    return true;
                case UMI3DPropertyKeys.AmbientHorizontalColor:
                    RenderSettings.ambientEquatorColor = ((ColorDto)data.property.value).Struct();
                    return true;
                case UMI3DPropertyKeys.AmbientGroundColor:
                    RenderSettings.ambientGroundColor = ((ColorDto)data.property.value).Struct();
                    return true;
                case UMI3DPropertyKeys.AmbientIntensity:
                    RenderSettings.ambientIntensity = (float)data.property.value;
                    return AbstractParameters.SetSkyboxProperties(dto.skyboxType, dto.skyboxRotation, RenderSettings.ambientIntensity);
                case UMI3DPropertyKeys.AmbientSkyboxImage:
                    if (dto.skybox != null)
                    {
                        AbstractParameters.LoadSkybox(dto.skybox, dto.skyboxType, dto.skyboxRotation, RenderSettings.ambientIntensity);
                        AbstractParameters.SetSkyboxProperties(dto.skyboxType, dto.skyboxRotation, RenderSettings.ambientIntensity);
                    }
                    return true;
                case UMI3DPropertyKeys.AmbientSkyboxRotation:
                    return AbstractParameters.SetSkyboxProperties(dto.skyboxType, dto.skyboxRotation, RenderSettings.ambientIntensity);
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
        protected virtual async Task<bool> _SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            if (data.entity == null) return false;
            UMI3DEnvironmentDto dto = ((data.entity.dto as GlTFEnvironmentDto)?.extensions)?.umi3d;
            if (dto == null) return false;

            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.PreloadedScenes:
                    return await AbstractParameters.SetUMI3DProperty(data);
                case UMI3DPropertyKeys.AmbientType:
                    RenderSettings.ambientMode = (AmbientMode)UMI3DSerializer.Read<int>(data.container);
                    return AbstractParameters.SetSkyboxProperties(dto.skyboxType, dto.skyboxRotation, RenderSettings.ambientIntensity);
                case UMI3DPropertyKeys.AmbientSkyColor:
                    RenderSettings.ambientSkyColor = UMI3DSerializer.Read<ColorDto>(data.container).Struct();
                    return true;
                case UMI3DPropertyKeys.AmbientHorizontalColor:
                    RenderSettings.ambientEquatorColor = UMI3DSerializer.Read<ColorDto>(data.container).Struct();
                    return true;
                case UMI3DPropertyKeys.AmbientGroundColor:
                    RenderSettings.ambientGroundColor = UMI3DSerializer.Read<ColorDto>(data.container).Struct();
                    return true;
                case UMI3DPropertyKeys.AmbientIntensity:
                    RenderSettings.ambientIntensity = UMI3DSerializer.Read<float>(data.container);
                    return AbstractParameters.SetSkyboxProperties(dto.skyboxType, dto.skyboxRotation, RenderSettings.ambientIntensity);
                case UMI3DPropertyKeys.AmbientSkyboxImage:
                    dto.skybox = UMI3DSerializer.Read<ResourceDto>(data.container);
                    if (dto.skybox != null)
                    {
                        AbstractParameters.LoadSkybox(dto.skybox, dto.skyboxType, dto.skyboxRotation, RenderSettings.ambientIntensity);
                        AbstractParameters.SetSkyboxProperties(dto.skyboxType, dto.skyboxRotation, RenderSettings.ambientIntensity);
                    }
                    return true;
                case UMI3DPropertyKeys.AmbientSkyboxRotation:
                    dto.skyboxRotation = UMI3DSerializer.Read<float>(data.container);
                    return AbstractParameters.SetSkyboxProperties(dto.skyboxType, dto.skyboxRotation, RenderSettings.ambientIntensity);
                default:
                    return false;
            }
        }

        protected virtual async Task<bool> _ReadUMI3DProperty(ReadUMI3DPropertyData data)
        {
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.PreloadedScenes:
                    return await AbstractParameters.ReadUMI3DProperty(data);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public static async Task SetEntity(SetEntityPropertyDto dto, List<CancellationToken> tokens)
        {
            if (!Exists) return;
            try
            {
                var e = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(dto.entityId, tokens);
                if (!await SetEntity(new SetUMI3DPropertyData(dto, e, tokens)))
                    UMI3DLogger.LogWarning("SetEntity operation was not applied : entity : " + dto.entityId + "   propKey : " + dto.property, scope);
            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
            }
        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public static async Task SetEntity(uint operationId, ulong entityId, uint propertyKey, ByteContainer container)
        {
            var e = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(entityId, container.tokens);

            if (!await SetEntity(entityId, new SetUMI3DPropertyContainerData(e, operationId, propertyKey, container)))
                UMI3DLogger.LogWarning("SetEntity operation was not applied : entity : " + entityId + "  operation : " + operationId + "   propKey : " + propertyKey, scope);

        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="node">Node on which the dto should be applied.</param>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public static async Task<bool> SetEntity(SetUMI3DPropertyData data)
        {
            if (Instance.entityFilters.ContainsKey(data.property.entityId) && Instance.entityFilters[data.property.entityId].ContainsKey(data.property.property))
            {
                Instance.entityFilters[data.property.entityId][data.property.property].AddMeasure(data.property.value.Deserialize());
                return true;
            }
            else
            {
                if (await SetUMI3DProperty(data)) return true;
                if (UMI3DEnvironmentLoader.Exists && await UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(data)) return true;
                return await AbstractParameters.SetUMI3DProperty(data);
            }
        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="node">Node on which the dto should be applied.</param>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public static async Task<bool> SetEntity(ulong entityId, SetUMI3DPropertyContainerData data)
        {
            if (Instance.entityFilters.ContainsKey(entityId) && Instance.entityFilters[entityId].ContainsKey(data.propertyKey))
            {
                var value = new ReadUMI3DPropertyData(data.propertyKey, data.container);
                await ReadValueEntity(value);
                Instance.entityFilters[entityId][data.propertyKey].AddMeasure(value.result.Deserialize());
                return true;
            }
            else
            {
                if (await SetUMI3DProperty(data)) return true;
                if (UMI3DEnvironmentLoader.Exists && await UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(data)) return true;
                return await AbstractParameters.SetUMI3DProperty(data);
            }
        }

        private static async Task<bool> ReadValueEntity(ReadUMI3DPropertyData data)
        {
            if (await ReadUMI3DProperty(data)) return true;
            if (UMI3DEnvironmentLoader.Exists && await UMI3DEnvironmentLoader.Instance.sceneLoader.ReadUMI3DProperty(data)) return true;
            return await AbstractParameters.ReadUMI3DProperty(data);
        }


        private static async Task<bool> SimulatedSetEntity(SetUMI3DPropertyData data)
        {
            if (await SetUMI3DProperty(data)) return true;
            if (UMI3DEnvironmentLoader.Exists && await UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(data)) return true;
            return await AbstractParameters.SetUMI3DProperty(data);
        }


        /// <summary>
        /// Apply a setEntity to multiple entities
        /// </summary>
        /// <param name="dto">MultiSetEntityPropertyDto with the ids list to mofify</param>
        /// <returns></returns>
        public static async Task<bool> SetMultiEntity(MultiSetEntityPropertyDto dto, List<CancellationToken> tokens)
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

                    var e = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(id, tokens);
                    await SetEntity(new SetUMI3DPropertyData(entityPropertyDto, e));

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
        public static async Task<bool> SetMultiEntity(ByteContainer container)
        {
            if (!Exists) return false;
            List<ulong> idList = UMI3DSerializer.ReadList<ulong>(container);
            uint operationId = UMI3DSerializer.Read<uint>(container);
            uint propertyKey = UMI3DSerializer.Read<uint>(container);

            foreach (ulong id in idList)
            {
                try
                {
                    var e = await WaitForAnEntityToBeLoaded(id, container.tokens);

                    var newContainer = new ByteContainer(container);
                    if (!await SetEntity(id, new SetUMI3DPropertyContainerData(e, operationId, propertyKey, newContainer)))
                        UMI3DLogger.LogWarning($"A SetUMI3DProperty failed to match any loader {id} {operationId} {propertyKey} {newContainer}", scope | DebugScope.Bytes);

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



        private readonly Dictionary<ulong, Dictionary<ulong, IExtrapolator>> entityFilters = new Dictionary<ulong, Dictionary<ulong, IExtrapolator>>();

        protected async void InterpolationRoutine()
        {
            while (UMI3DClientServer.Exists && UMI3DClientServer.Instance.GetUserId() != 0)
            {
                foreach (ulong entityId in Instance.entityFilters.Keys)
                {
                    foreach (ulong property in Instance.entityFilters[entityId].Keys)
                    {
                        UMI3DEntityInstance node = UMI3DEnvironmentLoader.GetEntity(entityId);
                        IExtrapolator extrapolator = Instance.entityFilters[entityId][property];

                        extrapolator.ComputeExtrapolatedValue();

                        var entityPropertyDto = new SetEntityPropertyDto()
                        {
                            entityId = entityId,
                            property = property,
                            value = extrapolator.ExtrapolatedValue.ToSerializable()
                        };
                        //SimulatedSetEntity(new SetUMI3DPropertyData(entityPropertyDto, node));
                        Task.FromResult(SimulatedSetEntity(new SetUMI3DPropertyData(entityPropertyDto, node)));
                    }
                }

                await UMI3DAsyncManager.Yield();
            }
        }

        /// <summary>
        /// Handle StartInterpolationPropertyDto operation.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static async Task<bool> StartInterpolation(StartInterpolationPropertyDto dto, List<CancellationToken> tokens)
        {
            if (!Exists) return false;
            var e = await WaitForAnEntityToBeLoaded(dto.entityId, tokens);
            return await Instance.StartInterpolation(e, dto.entityId, dto.property, dto.startValue, tokens);
        }

        /// <summary>
        /// Handle StartInterpolationPropertyDto operation.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static async Task<bool> StartInterpolation(ByteContainer container)
        {
            if (!Exists) return false;
            ulong entityId = UMI3DSerializer.Read<ulong>(container);
            uint propertyKey = UMI3DSerializer.Read<uint>(container);
            var e = await WaitForAnEntityToBeLoaded(entityId, container.tokens);

            var value = new ReadUMI3DPropertyData(propertyKey, container);
            await ReadValueEntity(value);
            return await Instance.StartInterpolation(e, entityId, propertyKey, value.result.Deserialize(), container.tokens);
        }

        protected async Task<bool> StartInterpolation(UMI3DEntityInstance node, ulong entityId, ulong propertyKey, object startValue, List<CancellationToken> tokens)
        {
            if (!entityFilters.ContainsKey(entityId))
            {
                entityFilters.Add(entityId, new Dictionary<ulong, IExtrapolator>());
            }

            if (!entityFilters[entityId].ContainsKey(propertyKey))
            {
                IExtrapolator newExtrapolator;
                if (propertyKey == UMI3DPropertyKeys.Rotation)
                    newExtrapolator = new QuaternionLinearDelayedExtrapolator();
                else
                    newExtrapolator = new Vector3LinearDelayedExtrapolator();

                entityFilters[entityId].Add(propertyKey, newExtrapolator);

                newExtrapolator.AddMeasure(startValue);

                var entityPropertyDto = new SetEntityPropertyDto()
                {
                    entityId = entityId,
                    property = propertyKey,
                    value = startValue.ToSerializable()
                };
                return await SetEntity(new SetUMI3DPropertyData(entityPropertyDto, node, tokens));
            }
            return false;
        }

        /// <summary>
        /// Handle StopInterpolationPropertyDto operation.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static async Task<bool> StopInterpolation(StopInterpolationPropertyDto dto, List<CancellationToken> tokens)
        {
            if (!Exists) return false;
            var e = await WaitForAnEntityToBeLoaded(dto.entityId, tokens);
            await Instance.StopInterpolation(e, dto.entityId, dto.property, dto.stopValue, tokens);
            return true;
        }

        public static async Task<bool> StopInterpolation(ByteContainer container)
        {
            if (!Exists) return false;
            ulong entityId = UMI3DSerializer.Read<ulong>(container);
            uint propertyKey = UMI3DSerializer.Read<uint>(container);
            var e = await WaitForAnEntityToBeLoaded(entityId, container.tokens);

            var value = new ReadUMI3DPropertyData(propertyKey, container);
            await ReadValueEntity(value);
            return await Instance.StopInterpolation(e, entityId, propertyKey, value.result.Deserialize(), container.tokens);
        }


        protected async Task<bool> StopInterpolation(UMI3DEntityInstance node, ulong entityId, uint property, object stopValue, List<CancellationToken> tokens)
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

                return await SetEntity(new SetUMI3DPropertyData(entityPropertyDto, node, tokens));
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