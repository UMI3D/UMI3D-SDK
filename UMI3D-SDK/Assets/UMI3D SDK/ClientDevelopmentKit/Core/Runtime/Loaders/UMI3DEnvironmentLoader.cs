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

        private Dictionary<ulong, UMI3DEntities> entitiesCollection = new();

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


        public static void DeclareNewEnvironment(ulong id, string url)
        {
            Instance.entitiesCollection[id] = new(id,url);
        }

        public IReadOnlyList<string> GetResourcesUrls()
        {
            return entitiesCollection?.Values?.Select(v => v.ReourcesUrl).ToList();
        }

        /// <summary>
        /// Call a callback when an entity is registerd.
        /// The entity might not be totaly loaded when the callback is called.
        /// all property of UMI3DEntityInstance and UMI3DNodeInstance are set.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entityLoaded"></param>
        /// <param name="entityFailedToLoad"></param>
        public static void WaitForAnEntityToBeLoaded(ulong environmentid, ulong id, Action<UMI3DEntityInstance> entityLoaded, Action entityFailedToLoad = null)
        {
            if (!Exists) return;
            Instance.WaitUntilEntityLoaded(environmentid, id, entityLoaded, entityFailedToLoad);
        }

        /// <summary>
        /// Call a callback when an entity is registerd.
        /// The entity might not be totaly loaded when the callback is called.
        /// all property of UMI3DEntityInstance and UMI3DNodeInstance are set.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entityLoaded"></param>
        /// <param name="entityFailedToLoad"></para
        public virtual void WaitUntilEntityLoaded(ulong environmentid, ulong id, Action<UMI3DEntityInstance> entityLoaded, Action entityFailedToLoad = null)
        {
            if (entitiesCollection == null) return;
            entitiesCollection[environmentid].WaitUntilEntityLoaded(id,entityLoaded,entityFailedToLoad);
        }

        public static async Task<UMI3DEntityInstance> WaitForAnEntityToBeLoaded(ulong environmentid, ulong id, List<CancellationToken> tokens)
        {
            if (!Exists)
                throw new Umi3dException("EnvironmentLoader does not exist");
            return await Instance.WaitUntilEntityLoaded(environmentid, id, tokens);
        }

        public virtual Task<UMI3DEntityInstance> WaitUntilEntityLoaded(ulong environmentid, ulong id, List<CancellationToken> tokens)
        {
            if (entitiesCollection == null)
                throw new Umi3dException("Entity waited to be loaded does not exist");
            return entitiesCollection[environmentid].WaitUntilEntityLoaded(id, tokens);
        }


        /// <summary>
        /// Return a list of all registered entities in every environment.
        /// </summary>
        /// <returns></returns>
        public static List<UMI3DEntityInstance> AllEntities() { return Exists ? Instance.entitiesCollection.SelectMany(e => e.Value.Entities()).ToList() : null; }

        /// <summary>
        /// Return a list of all registered entities.
        /// </summary>
        /// <returns></returns>
        public static List<UMI3DEntityInstance> Entities(ulong environmentid) { return Exists ? Instance.entitiesCollection[environmentid].Entities() : null; }

        /// <summary>
        /// Get an entity with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        [Obsolete("Use GetEntityInstance or GetEntityObject<T> instead.")]
        public static UMI3DEntityInstance GetEntity(ulong environmentid, ulong id) { return id != 0 && Exists ? Instance.GetEntityInstance(environmentid, id) : null; }

        /// <summary>
        /// Get an entity with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public virtual UMI3DEntityInstance GetEntityInstance(ulong environmentid, ulong id)
        {
            return entitiesCollection[environmentid].GetEntityInstance(id);
        }

        /// <summary>
        /// Get an entity with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public virtual UMI3DEntityInstance TryGetEntityInstance(ulong environmentid, ulong id)
        {
            return entitiesCollection[environmentid].TryGetEntityInstance(id);
        }

        /// <summary>
        /// Get an entity with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns></returns>
        public virtual T GetEntityObject<T>(ulong environmentid, ulong id) where T : class
        {
            var entity = GetEntityInstance(environmentid, id);
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
        public static UMI3DNodeInstance GetNode(ulong environmentid, ulong id) { return id != 0 && Exists ? Instance.entitiesCollection[environmentid].GetNode(id) : null; }

        /// <summary>
        /// Get a node with an id.
        /// </summary>
        /// <param name="id">unique id of the entity.</param>
        /// <returns>Node instance or null if node does not exist.</returns>
        public virtual UMI3DNodeInstance GetNodeInstance(ulong environmentid, ulong id)
        {
            if (GetEntityInstance(environmentid, id) is not UMI3DNodeInstance node)
                throw new Umi3dException($"Entity {id} is not an UMI3DNodeInstance.");
            return node;
        }

        /// <summary>
        /// Get a node id with a collider.
        /// </summary>
        /// <param name="collider">collider.</param>
        /// <returns></returns>
        public static ulong GetNodeID(Collider collider) { return Exists ? Instance.entitiesCollection.Select( e => e.Value.GetNodeID(collider)).FirstOrDefault( id => id != 0) : 0; }

        /// <summary>
        /// Get node id associated to <paramref name="t"/>.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static ulong GetNodeID(Transform t) { return Exists ? Instance.entitiesCollection.Select(e => e.Value.GetNodeID(t)).FirstOrDefault(id => id != 0) : 0; }

        /// <summary>
        /// Register a node instance.
        /// </summary>
        /// <param name="id">unique id of the node.</param>
        /// <param name="dto">dto of the node.</param>
        /// <param name="instance">gameobject of the node.</param>
        /// <returns></returns>
        public static UMI3DNodeInstance RegisterNodeInstance(ulong environmentid, ulong id, UMI3DDto dto, GameObject instance, Action delete = null)
        {
            if (!Exists)
                return null;
            return Instance.entitiesCollection[environmentid].RegisterNodeInstance(id, dto, instance, delete);
        }

        /// <summary>
        /// Register an entity without a gameobject.
        /// </summary>
        /// <param name="id">unique id of the node.</param>
        /// <param name="dto">dto of the node.</param>
        /// <returns></returns>
        [Obsolete("Use Instance.RegisterEntity() instead")]
        public static UMI3DEntityInstance RegisterEntityInstance(ulong environmentid, ulong id, UMI3DDto dto, object Object, Action delete = null)
        {
            if (!Exists)
                return null;
            else
                return Instance.RegisterEntity(environmentid, id, dto, Object, delete);
        }

        /// <summary>
        /// Register an entity without a gameobject.
        /// </summary>
        /// <param name="id">unique id of the node.</param>
        /// <param name="dto">dto of the node.</param>
        /// <returns></returns>
        public virtual UMI3DEntityInstance RegisterEntity(ulong environmentid, ulong id, UMI3DDto dto, object objectInstance, Action delete = null)
        {
            if (!Exists)
                throw new Umi3dException("Cannot register entity. Loader does not exist.");

            return entitiesCollection[environmentid].RegisterEntity(id, dto, objectInstance, delete);
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
            ulong mainEnvironmentId = 0;
            DeclareNewEnvironment(mainEnvironmentId, UMI3DClientServer.Environement.resourcesUrl);

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
            RegisterEntity(mainEnvironmentId, UMI3DGlobalID.EnvironementId, dto, null).NotifyLoaded();
            //
            // Load resources
            //
            await LoadResources(dto, downloadingProgress);

            onResourcesLoaded.Invoke();

            ReadingDataProgress.AddComplete();
            //
            // Instantiate nodes
            //
            await ReadUMI3DExtension(mainEnvironmentId, dto, null);
            ReadingDataProgress.AddComplete();
            await InstantiateNodes(mainEnvironmentId, loadingProgress);

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
                await RenderProbes(mainEnvironmentId);
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
        private async Task RenderProbes(ulong environmentid)
        {
            List<(ReflectionProbe probe, int id)> probeList = new List<(ReflectionProbe, int)>();

            foreach (var entity in entitiesCollection[environmentid].Entities())
            {
                if (entity.dto is GlTFSceneDto && entity is UMI3DNodeInstance scene)
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
        private async Task InstantiateNodes(ulong environmentid, MultiProgress progress)
        {
            await _InstantiateNodes(environmentid, environment.scenes, progress);
            loaded = true;
        }

        public async Task InstantiateNodes(ulong environmentid, List<GlTFSceneDto> scenes)
        {
            await _InstantiateNodes(environmentid, scenes, null);
        }

        /// <summary>
        /// Load scenes 
        /// </summary>
        /// <param name="scenes">scenes to loads</param>
        /// <returns></returns>
        private async Task _InstantiateNodes(ulong environmentid, List<GlTFSceneDto> scenes, MultiProgress progress)
        {

            //Load scenes without hierarchy
            await Task.WhenAll(scenes.Select(async scene =>
            {
                Progress progress1 = new Progress(0, $"Load scene {scene.name}");
                if(progress != null)
                    progress.Add(progress1);
                await sceneLoader.LoadGlTFScene(environmentid,scene, progress1);

            }));
            //Organize scenes
            await Task.WhenAll(scenes.Select(async scene =>
            {
                Progress progress1 = new Progress(2, $"Generate scene {scene.name}");
                if (progress != null)
                    progress.Add(progress1);
                progress1.AddComplete();
                var node = GetNodeInstance(environmentid, scene.extensions.umi3d.id);
                UMI3DSceneNodeDto umi3dScene = scene.extensions.umi3d;
                await sceneLoader.ReadUMI3DExtension(new ReadUMI3DExtensionData(environmentid,umi3dScene, node.gameObject));
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
        public static async Task LoadEntity(ulong environmentid, IEntity entity, List<CancellationToken> tokens)
        {
            if (Exists) await Instance._LoadEntity(environmentid, entity, tokens);
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
        private async Task _LoadEntity(ulong environmentId, IEntity entity, List<CancellationToken> tokens)
        {
            try
            {
                switch (entity)
                {
                    case GlTFSceneDto scene:
                        await _InstantiateNodes(environmentId, new List<GlTFSceneDto>() { scene }, new MultiProgress("Load Entity"));
                        break;
                    case GlTFNodeDto node:
                        await nodeLoader.LoadNodes(environmentId, new List<GlTFNodeDto>() { node }, new Progress(0, "Load Entity"));
                        break;
                    case AssetLibraryDto library:
                        await UMI3DResourcesManager.DownloadLibrary(library, UMI3DClientServer.Media.name, new MultiProgress("Load Entity"));
                        await UMI3DResourcesManager.LoadLibrary(library.libraryId);
                        break;
                    case AbstractEntityDto dto:
                        await AbstractParameters.ReadUMI3DExtension(new ReadUMI3DExtensionData(environmentId, dto, tokens));
                        break;
                    case GlTFMaterialDto matDto:
                        AbstractParameters.SelectMaterialLoader(matDto).LoadMaterialFromExtension(matDto, (m) =>
                        {

                            if (matDto.name != null && matDto.name.Length > 0)
                                m.name = matDto.name;
                            //register the material
                            if (m == null)
                            {
                                RegisterEntity(environmentId,((AbstractEntityDto)matDto.extensions.umi3d).id, matDto, new List<Material>()).NotifyLoaded();
                            }
                            else
                            {
                                RegisterEntity(environmentId,((AbstractEntityDto)matDto.extensions.umi3d).id, matDto, m).NotifyLoaded();
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
            Task InternalLoadEntityTask(IEntity item, List<CancellationToken> tokens)
            {
                return LoadEntity(container.environmentId, item, tokens);
            }
            await entitiesCollection[container.environmentId]._LoadEntity(container, InternalLoadEntityTask);

        }


        private Task _LoadEntityTask(ulong environmentid, IEntity item, List<CancellationToken> tokens)
        {
            return LoadEntity(environmentid, item, tokens);
        }

        /// <summary>
        /// Delete IEntity
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="performed"></param>
        public static async Task DeleteEntity(ulong environmentid, ulong entityId, List<CancellationToken> tokens)
        {
            if (UMI3DResourcesManager.isKnowedLibrary(entityId))
                UMI3DResourcesManager.UnloadLibrary(entityId);
            else
                await Instance.entitiesCollection[environmentid].DeleteEntity(entityId, tokens);
        }

        /// <summary>
        /// Clear an environement and make the client ready to load a new environment.
        /// </summary>
        public static void Clear(bool clearCache = true)
        {
            Instance.entitiesCollection.Clear();

            if (clearCache)
                UMI3DResourcesManager.Instance.ClearCache();

            Instance.InternalClear();
        }

        protected virtual void InternalClear()
        {
            UMI3DVideoPlayerLoader.Clear();

            foreach(var entity in Instance.entitiesCollection)
                entity.Value.InternalClear();

            isEnvironmentLoaded = false;

            environment = null;
            loaded = false;
        }

        /// <summary>
        /// Load environment.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="node"></param>
        public virtual async Task ReadUMI3DExtension(ulong environmentId, GlTFEnvironmentDto dto, GameObject node)
        {
            UMI3DEnvironmentDto extension = dto?.extensions?.umi3d;
            if (extension != null && environmentId == 0)
            {
                if (extension.defaultMaterial != null && extension.defaultMaterial.variants != null && extension.defaultMaterial.variants.Count > 0)
                {
                    _baseMaterial = null;
                    LoadDefaultMaterial(extension.defaultMaterial);
                }
                foreach (PreloadedSceneDto scene in extension.preloadedScenes)
                    await AbstractParameters.ReadUMI3DExtension(new ReadUMI3DExtensionData(environmentId, scene, node));
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
        public static async Task SetEntity(ulong environmentId, SetEntityPropertyDto dto, List<CancellationToken> tokens)
        {
            if (!Exists) return;
            try
            {
                var e = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(environmentId, dto.entityId, tokens);
                if (!await SetEntity(environmentId, new SetUMI3DPropertyData(environmentId, dto, e, tokens)))
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
        public static async Task SetEntity(ulong environmentId, uint operationId, ulong entityId, uint propertyKey, ByteContainer container)
        {
            var e = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(environmentId, entityId, container.tokens);
            if (!await SetEntity(environmentId, entityId, new SetUMI3DPropertyContainerData(environmentId, e, operationId, propertyKey, container)))
                UMI3DLogger.LogWarning("SetEntity operation was not applied : entity : " + entityId + "  operation : " + operationId + "   propKey : " + propertyKey, scope);

        }

        /// <summary>
        /// Handle SetEntityPropertyDto operation.
        /// </summary>
        /// <param name="node">Node on which the dto should be applied.</param>
        /// <param name="dto">Set operation to handle.</param>
        /// <returns></returns>
        public static async Task<bool> SetEntity(ulong environmentId, SetUMI3DPropertyData data)
        {
            if (instance.entitiesCollection[environmentId].SetEntity(data))
                return true;
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
        public static async Task<bool> SetEntity(ulong environmentId, ulong entityId, SetUMI3DPropertyContainerData data)
        {
           // UnityMainThreadDispatcher.Instance().Enqueue(() => UnityEngine.Debug.Log($"SetEntityProperty {environmentId} {entityId}"));
            if (await Instance.entitiesCollection[environmentId].SetEntity(entityId,data, ReadValueEntity))
                return true;
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


        private static async void SimulatedSetEntity(SetUMI3DPropertyData data)
        {
            if (await SetUMI3DProperty(data)) return;
            if (UMI3DEnvironmentLoader.Exists && await UMI3DEnvironmentLoader.Instance.sceneLoader.SetUMI3DProperty(data)) return;
            await AbstractParameters.SetUMI3DProperty(data);
        }


        /// <summary>
        /// Apply a setEntity to multiple entities
        /// </summary>
        /// <param name="dto">MultiSetEntityPropertyDto with the ids list to mofify</param>
        /// <returns></returns>
        public static async Task<bool> SetMultiEntity(ulong environmentId, MultiSetEntityPropertyDto dto, List<CancellationToken> tokens)
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

                    var e = await UMI3DEnvironmentLoader.WaitForAnEntityToBeLoaded(environmentId, id, tokens);
                    await SetEntity(environmentId, new SetUMI3DPropertyData(environmentId, entityPropertyDto, e));

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
        public static async Task<bool> SetMultiEntity(ulong environmentId, ByteContainer container)
        {
            if (!Exists) return false;
            List<ulong> idList = UMI3DSerializer.ReadList<ulong>(container);
            uint operationId = UMI3DSerializer.Read<uint>(container);
            uint propertyKey = UMI3DSerializer.Read<uint>(container);

            foreach (ulong id in idList)
            {
                try
                {
                    var e = await WaitForAnEntityToBeLoaded(environmentId, id, container.tokens);

                    var newContainer = new ByteContainer(container);
                    if (!await SetEntity(environmentId, id, new SetUMI3DPropertyContainerData(environmentId, e, operationId, propertyKey, newContainer)))
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


        protected async void InterpolationRoutine()
        {
            while (UMI3DClientServer.Exists && UMI3DClientServer.Instance.GetUserId() != 0)
            {
                foreach (var entities in entitiesCollection.Values)
                    entities.InterpolationRoutine(SimulatedSetEntity);
                await UMI3DAsyncManager.Yield();
            }
        }



        /// <summary>
        /// Handle StartInterpolationPropertyDto operation.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static async Task<bool> StartInterpolation(ulong environmentId, StartInterpolationPropertyDto dto, List<CancellationToken> tokens)
        {
            if (!Exists) return false;
            var e = await WaitForAnEntityToBeLoaded(environmentId, dto.entityId, tokens);
            return Instance.entitiesCollection[environmentId].StartInterpolation(e, dto.entityId, dto.property, dto.startValue, tokens);
        }

        /// <summary>
        /// Handle StartInterpolationPropertyDto operation.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static async Task<bool> StartInterpolation(ulong environmentId, ByteContainer container)
        {
            if (!Exists) return false;
            ulong entityId = UMI3DSerializer.Read<ulong>(container);
            uint propertyKey = UMI3DSerializer.Read<uint>(container);
            var e = await WaitForAnEntityToBeLoaded(environmentId, entityId, container.tokens);

            var value = new ReadUMI3DPropertyData(environmentId, propertyKey, container);
            await ReadValueEntity(value);
            return Instance.entitiesCollection[environmentId].StartInterpolation(e, entityId, propertyKey, value.result.Deserialize(), container.tokens);
        }



        /// <summary>
        /// Handle StopInterpolationPropertyDto operation.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static async Task<bool> StopInterpolation(ulong environmentId, StopInterpolationPropertyDto dto, List<CancellationToken> tokens)
        {
            if (!Exists) return false;
            var e = await WaitForAnEntityToBeLoaded(environmentId, dto.entityId, tokens);
            Instance.entitiesCollection[environmentId].StopInterpolation(e, dto.entityId, dto.property, dto.stopValue, tokens);
            return true;
        }

        public static async Task<bool> StopInterpolation(ulong environmentId, ByteContainer container)
        {
            if (!Exists) return false;
            ulong entityId = UMI3DSerializer.Read<ulong>(container);
            uint propertyKey = UMI3DSerializer.Read<uint>(container);
            var e = await WaitForAnEntityToBeLoaded(environmentId, entityId, container.tokens);

            var value = new ReadUMI3DPropertyData(environmentId, propertyKey, container);
            await ReadValueEntity(value);
            return Instance.entitiesCollection[environmentId].StopInterpolation(e, entityId, propertyKey, value.result.Deserialize(), container.tokens);
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