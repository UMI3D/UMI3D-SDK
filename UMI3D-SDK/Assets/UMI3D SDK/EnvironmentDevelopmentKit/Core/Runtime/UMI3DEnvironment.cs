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

using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;
using UnityEngine.Rendering;

namespace umi3d.edk
{
    /// <summary>
    /// Root node of any UMI3D enviroment.
    /// </summary>
    /// As there is only one envionment node, it could be called as a manager.
    public class UMI3DEnvironment : SingleBehaviour<UMI3DEnvironment>, IUMI3DEnvironmentManager
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration;

        /// <summary>
        /// If true, the environment will use JSON Data Transfer Objects for network communications with browsers.
        /// </summary>
        /// Not that it is not recommended as JSON DTOs are way heavier, and thus slower, than the recent byte networking system.
        [EditorReadOnly]
        [Tooltip("If true, the environment will use JSON Data Transfer Objects for network communications with browsers.\n" +
                 "Negatively affects the performance of the networking system by increasing the size of exchanged messages.")]
        public bool useDto = false;

        #region initialization
        /// <inheritdoc/>
        protected override void Awake()
        {
            base.Awake();
        }
        #endregion

        #region environment description
        /// <summary>
        /// Environment's name.
        /// </summary>
        [EditorReadOnly]
        [Tooltip("Name of the environment. Will be seen by users when connecting to the environment.")]
        public string environmentName = "Environment Name";

        /// <summary>
        /// Scenes that are available in environment.
        /// </summary>
        [HideInInspector]
        public List<UMI3DScene> scenes;

        /// <summary>
        /// List of <see cref="AssetLibrary"/> required in the environment.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("List of asset libraries required to access the environment.")]
        public List<AssetLibrary> globalLibraries;

        /// <summary>
        /// Default spawn position in the environment.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Default position in the environment.")]
        private Vector3 defaultStartPosition = new Vector3(0, 0, 0);
        /// <summary>
        /// Default spawn rotation in the environment.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Default rotation in the environment.")]
        private Vector3 defaultStartOrientation = new Vector3(0, 0, 0);
        /// <summary>
        /// See <see cref="defaultStartPosition"/>.
        /// </summary>
        public static UMI3DAsyncProperty<Vector3> objectStartPosition { get; protected set; }
        /// <summary>
        /// See <see cref="defaultStartOrientation"/>.
        /// </summary>
        public static UMI3DAsyncProperty<Quaternion> objectStartOrientation { get; protected set; }

        private void Start()
        {
            foreach (UMI3DAbstractNode node in GetComponentsInChildren<UMI3DAbstractNode>(true))
                node.Register();
            scenes = new List<UMI3DScene>(GetComponentsInChildren<UMI3DScene>(true));
            InitDefinition();
        }

        /// <summary>
        /// Convert the environment to a <see cref="GlTFEnvironmentDto"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual GlTFEnvironmentDto ToDto(UMI3DUser user)
        {
            var env = new GlTFEnvironmentDto
            {
                id = UMI3DGlobalID.EnvironmentId
            };
            env.scenes.AddRange(scenes.Where(s => s.LoadOnConnection(user)).Select(s => s.ToGlTFNodeDto(user)));
            env.extensions.umi3d = CreateDto();
            WriteProperties(env.extensions.umi3d, user);
            return env;
        }

        /// <summary>
        /// Write Properties on a <see cref="UMI3DEnvironmentDto"/>.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="user"></param>
        protected virtual void WriteProperties(UMI3DEnvironmentDto dto, UMI3DUser user)
        {
            dto.LibrariesId = globalLibraries.Select(l => l.idVersion).ToList();
            dto.preloadedScenes = objectPreloadedScenes.GetValue(user).Select(r => new PreloadedSceneDto() { scene = r.ToDto() }).ToList();
            dto.ambientType = (AmbientType)objectAmbientType.GetValue(user);
            dto.skyColor = objectSkyColor.GetValue(user).Dto();
            dto.horizontalColor = objectHorizonColor.GetValue(user).Dto();
            dto.groundColor = objectGroundColor.GetValue(user).Dto();
            dto.ambientIntensity = objectAmbientIntensity.GetValue(user);
            dto.skyboxType = skyboxType;
            dto.skybox = objectAmbientSkyboxImage.GetValue(user)?.ToDto();
            dto.skyboxRotation = objectSkyboxRotation.GetValue(user);
            dto.defaultMaterial = defaultMaterial?.ToDto();
        }

        /// <summary>
        /// Create an empty <see cref="UMI3DEnvironmentDto"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual UMI3DEnvironmentDto CreateDto()
        {
            return new UMI3DEnvironmentDto();
        }

        /// <summary>
        /// Create an <see cref="EnterDto"/> describing the entrance point of the environment.
        /// </summary>
        public static EnterDto ToEnterDto(UMI3DUser user)
        {
            return new EnterDto() { userPosition = objectStartPosition.GetValue(user).Dto(), userRotation = objectStartOrientation.GetValue(user).Dto(), usedDto = Instance.useDto };
        }

        /// <summary>
        /// Export the required assets libraries as a <see cref="LibrariesDto"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public virtual LibrariesDto ToLibrariesDto(UMI3DUser user)
        {
            try
            {
                List<AssetLibraryDto> libraries = globalLibraries?.Select(l => l.ToDto()).ToList() ?? new List<AssetLibraryDto>();
                IEnumerable<AssetLibraryDto> sceneLib = scenes?.SelectMany(s => s.libraries)?.GroupBy(l => l.id)?.Where(l => !libraries.Any(l2 => l2.libraryId == l.Key))?.Select(l => l.First().ToDto());
                if (sceneLib != null)
                    libraries.AddRange(sceneLib);
                return new LibrariesDto() { libraries = libraries };
            } catch (Exception e) {
                Debug.LogException(e);
                return null;
            }
        }

        /// <summary>
        /// Is the environment requiring any asset library to be accessed?
        /// </summary>
        /// <returns></returns>
        public static bool UseLibrary()
        {
            return Exists && (Instance.globalLibraries.Any() || Instance.scenes.Any(s => s.libraries.Any()));
        }
        #endregion

        #region AsyncProperties

        private void InitDefinition()
        {
            ulong id = UMI3DGlobalID.EnvironmentId;

            objectStartPosition = new UMI3DAsyncProperty<Vector3>(id, 0, defaultStartPosition);
            objectStartOrientation = new UMI3DAsyncProperty<Quaternion>(id, 0, Quaternion.Euler(defaultStartOrientation));

            objectPreloadedScenes = new UMI3DAsyncListProperty<UMI3DResource>(id, UMI3DPropertyKeys.PreloadedScenes, preloadedScenes, (UMI3DResource r, UMI3DUser user) => new PreloadedSceneDto() { scene = r.ToDto() });
            objectAmbientType = new UMI3DAsyncProperty<AmbientMode>(id, UMI3DPropertyKeys.AmbientType, mode, (mode, user) => (int)(AmbientType)mode);
            objectSkyColor = new UMI3DAsyncProperty<Color>(id, UMI3DPropertyKeys.AmbientSkyColor, skyColor, (c, u) => c.Dto());
            objectHorizonColor = new UMI3DAsyncProperty<Color>(id, UMI3DPropertyKeys.AmbientHorizontalColor, horizontalColor, (c, u) => c.Dto());
            objectGroundColor = new UMI3DAsyncProperty<Color>(id, UMI3DPropertyKeys.AmbientGroundColor, groundColor, (c, u) => c.Dto());
            objectAmbientIntensity = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AmbientIntensity, ambientIntensity);
            objectAmbientSkyboxImage = new UMI3DAsyncProperty<UMI3DResource>(id, UMI3DPropertyKeys.AmbientSkyboxImage, skyboxImage, (r, u) => r.ToDto());
            objectSkyboxRotation = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AmbientSkyboxRotation, skyboxRotation);
        }

        /// <summary>
        /// Scene that are loaded with the environment the first time.
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Scene that are loaded with the environment the first time.")]
        private List<UMI3DResource> preloadedScenes = new List<UMI3DResource>();
        /// <summary>
        /// See <see cref="objectPreloadedScenes"/>.
        /// </summary>
        public UMI3DAsyncListProperty<UMI3DResource> objectPreloadedScenes;

        /// <summary>
        /// AsyncProperties of the ambient type. See <see cref="RenderSettings.ambientMode"/>.
        /// </summary>
        private AmbientMode mode => RenderSettings.ambientMode;
        /// <summary>
        /// See <see cref="mode"/>.
        /// </summary>
        public UMI3DAsyncProperty<AmbientMode> objectAmbientType;

        /// <summary>
        /// AsyncProperties of the ambient color. See <see cref="RenderSettings.ambientSkyColor"/>.
        /// </summary>
        private Color skyColor => RenderSettings.ambientSkyColor;
        /// <summary>
        /// See <see cref="skyColor"/>.
        /// </summary>
        public UMI3DAsyncProperty<Color> objectSkyColor;

        /// <summary>
        /// AsyncProperties of the ambient color. See <see cref="RenderSettings.ambientEquatorColor"/>.
        /// </summary>
        private Color horizontalColor => RenderSettings.ambientEquatorColor;
        /// <summary>
        /// See <see cref="horizontalColor"/>.
        /// </summary>
        public UMI3DAsyncProperty<Color> objectHorizonColor;

        /// <summary>
        /// AsyncProperties of the ambient color. See <see cref="RenderSettings.ambientGroundColor"/>.
        /// </summary>
        private Color groundColor => RenderSettings.ambientGroundColor;
        /// <summary>
        /// See <see cref="groundColor"/>.
        /// </summary>
        public UMI3DAsyncProperty<Color> objectGroundColor;

        /// <summary>
        /// AsyncProperties of the ambient Intensity. See <see cref="RenderSettings.ambientIntensity"/>.
        /// </summary>
        private float ambientIntensity => RenderSettings.ambientIntensity;
        /// <summary>
        /// See <see cref="ambientIntensity"/>.
        /// </summary>
        /// 
        public UMI3DAsyncProperty<float> objectAmbientIntensity;

        #region Skybox

        [Header("Skybox")]

        [SerializeField, Tooltip("Image format of skybox image")]
        public SkyboxType skyboxType;

        /// <summary>
        /// AsyncProperties of the Skybox Image
        /// </summary>
        [SerializeField, EditorReadOnly, Tooltip("Image of the sybox as a resource.")]
        private UMI3DResource skyboxImage = null;

        /// <summary>
        /// See <see cref="skyboxImage"/>.
        /// </summary>
        public UMI3DAsyncProperty<UMI3DResource> objectAmbientSkyboxImage;

        [SerializeField, Tooltip("Rotation for skybox, only works with equirectangular format"), Range(0, 360)]
        private float skyboxRotation = 0;

        /// <summary>
        /// AsyncProperties for <see cref="skyboxRotation"/>.
        /// </summary>
        public UMI3DAsyncProperty<float> objectSkyboxRotation;

        #endregion

        /// <summary>
        /// Properties of the default Material, it is used to initialise loaded materials in clients. 
        /// </summary>
        [SerializeField, Tooltip("Properties of the default Material, it is used to initialise loaded materials in clients.")]
        private UMI3DResource defaultMaterial = null;

        #endregion

        #region entities


        /// <summary>
        /// Contains the objects stored in the scene.
        /// </summary>
        private readonly DictionaryGenerator<UMI3DEntity> entities = new DictionaryGenerator<UMI3DEntity>();

        /// <summary>
        /// Access to all entities of a given type.
        /// </summary>
        public static IEnumerable<E> GetEntities<E>() where E : class, UMI3DEntity
        {
            if (Exists)
                return Instance.entities?.Values?.ToList()?.Where(entities => entities is E)?.Select(e => e as E);
            else if (QuittingManager.ApplicationIsQuitting)
                return new List<E>();
            else
                throw new System.NullReferenceException("UMI3DEnvironment doesn't exists !");
        }

        /// <summary>
        /// Access to all entities of a given type.
        /// </summary>
        public virtual IEnumerable<E> _GetEntities<E>() where E : class, UMI3DEntity
        {
            return entities.Values.Where(entities => entities is E).Select(e => e as E);
        }

        /// <summary>
        /// Get the collection of all <see cref="UMI3DUser"/> instances in the environment.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<UMI3DUser> GetUsers()
        {
            return _GetEntities<UMI3DUser>();
        }

        /// <summary>
        /// Get the set of all <see cref="UMI3DUser"/> instances in the environment.
        /// </summary>
        /// <returns></returns>
        public virtual HashSet<UMI3DUser> GetUserSet()
        {
            return new(_GetEntities<UMI3DUser>());
        }

        /// <summary>
        /// Get the set of all <see cref="UMI3DUser"/> instances in the environment that have already joined.
        /// </summary>
        /// <returns></returns>
        public virtual HashSet<UMI3DUser> GetJoinedUserSet()
        {
            return new(_GetEntities<UMI3DUser>().Where((u) => u.hasJoined));
        }

        /// <summary>
        /// Get the collection of all <see cref="UMI3DUser"/> instances in the environment.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<UMI3DUser> Users()
        {
            return UMI3DEnvironment.GetEntities<UMI3DUser>();
        }


        /// <summary>
        /// Return all id that have been registered and remove.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ulong> GetUnregisteredEntitiesId()
        {
            if (Exists)
                return Instance.entities?.old.ToList();
            else if (QuittingManager.ApplicationIsQuitting)
                return new List<ulong>();
            else
                throw new System.NullReferenceException("UMI3DEnvironment doesn't exists !");
        }

        /// <summary>
        /// Access to all entities of a given type that validate a predicate.
        /// </summary>
        /// <param name="predicate">Your selection condition</param>
        public static IEnumerable<E> GetEntitiesWhere<E>(System.Func<E, bool> predicate) where E : class, UMI3DEntity
        {
            if (Exists)
                return Instance.entities.Values.ToList().Where(entities => entities is E).Select(e => e as E).Where(predicate);
            else if (QuittingManager.ApplicationIsQuitting)
                return new List<E>();
            else
                throw new System.NullReferenceException("UMI3DEnvironment doesn't exists !");
        }

        /// <summary>
        /// Get entity by id.
        /// </summary>
        /// <param name="id">Entity to get id</param>
        public static E GetEntityInstance<E>(ulong id) where E : class, UMI3DEntity
        {
            if (Exists)
                return Instance._GetEntityInstance<E>(id);
            else if (QuittingManager.ApplicationIsQuitting)
                return null;
            else
                throw new System.NullReferenceException("UMI3DEnvironment doesn't exists !");
        }

        /// <summary>
        /// Get entity by id.
        /// </summary>
        /// <param name="id">Entity to get id</param>
        public E _GetEntityInstance<E>(ulong id) where E : class, UMI3DEntity
        {
            return entities[id] as E;
        }

        /// <summary>
        /// Get entity by id.
        /// </summary>
        /// <param name="id">Entity to get id</param>
        public static (E entity, bool exist, bool found) GetEntityIfExist<E>(ulong id) where E : class, UMI3DEntity
        {
            if (Exists)
            {
                if (Instance.entities.IsOldId(id))
                    return (null, false, true);
                else
                {
                    var e = Instance.entities[id] as E;
                    return (e, true, true);
                }
            }
            else if (QuittingManager.ApplicationIsQuitting)
                return (null, false, false);
            else
                throw new System.NullReferenceException("UMI3DEnvironment doesn't exists !");
        }

        /// <summary>
        /// Register an entity to the environment, and return it's id. 
        /// </summary>
        /// <param name="entity">Entity to register</param>
        /// <returns>Registered object's id.</returns>
        public static ulong Register(UMI3DEntity entity)
        {
            if (Exists)
            {
                if (entity != null)
                    return Instance.entities.Register(entity);
                else
                    throw new System.NullReferenceException("Trying to register null entity !");
            }
            else if (QuittingManager.ApplicationIsQuitting)
            {
                return 0;
            }
            else
            {
                throw new System.NullReferenceException("UMI3DEnvironment doesn't exists !");
            }
        }

        /// <summary>
        /// Register an entity to the environment with an id, and return it's id. 
        /// </summary>
        /// <param name="entity">Entity to register</param>
        /// <returns>Registered object's id</returns>
        public virtual ulong RegisterEntity(UMI3DEntity entity)
        {
            return Register(entity);
        }

        /// <summary>
        /// Register an entity to the environment with an id, and return it's id. 
        /// </summary>
        /// <param name="entity">Entity to register</param>
        /// <param name="id">id to use</param>
        /// <returns>Registered object's id (same as id field if the id wasn't already used).</returns>
        public static ulong Register(UMI3DEntity entity, ulong id)
        {
            if (Exists)
            {
                if (entity != null)
                    return Instance.entities.Register(entity, id);
                else
                    throw new System.NullReferenceException("Trying to register null entity !");
            }
            else if (QuittingManager.ApplicationIsQuitting)
            {
                return 0;
            }
            else
            {
                throw new System.NullReferenceException("UMI3DEnvironment doesn't exists !");
            }
        }

        /// <summary>
        /// Register an entity to the environment with an id, and return it's id. 
        /// </summary>
        /// <param name="entity">Entity to register</param>
        /// <param name="id">id to use</param>
        /// <returns>Registered object's id (same as id field if the id wasn't already used).</returns>
        public virtual ulong RegisterEntity(UMI3DEntity entity, ulong id)
        {
            return Register(entity, id);
        }

        /// <summary>
        /// Remove an entity from the scene. 
        /// Supported Types: AbstractObject3D, GenericInteraction, Tool, Toolbox
        /// </summary>
        /// <param name="obj">Object to remove</param>
        public static void Remove(UMI3DEntity obj)
        {
            if (obj != null && Exists)
                Instance?.entities?.Remove(obj.Id());
        }

        /// <summary>
        /// Remove an entity from the scene by id. 
        /// Supported Types: AbstractObject3D, GenericInteraction, Tool, Toolbox
        /// </summary>
        /// <param name="id">UMI3D id of the object to remove</param>
        public static void Remove(ulong id)
        {
            if (id != 0 && Exists)
            {
                Instance.entities?.Remove(id);
            }
        }

        /// <summary>
        /// Remove an entity from the scene by id. 
        /// Supported Types: AbstractObject3D, GenericInteraction, Tool, Toolbox
        /// </summary>
        /// <param name="id">UMI3D id of the object to remove</param>
        public void RemoveEntity(ulong id)
        {
            if (id != 0)
            {
                entities?.Remove(id);
            }
        }

        public class DictionaryGenerator<A>
        {
            private readonly HashSet<ulong> unRegisteredIds = new HashSet<ulong>();
            /// <summary>
            /// Contains the  stored objects.
            /// </summary>
            private readonly Dictionary<ulong, A> objects = new Dictionary<ulong, A>();

            public Dictionary<ulong, A>.ValueCollection Values => objects.Values;
            public IEnumerable<ulong> old => unRegisteredIds;

            public A this[ulong key]
            {
                get
                {
                    if (key == 0)
                        return default;
                    else if (objects.ContainsKey(key))
                        return objects[key];
                    else return default;
                }
            }

            public bool IsOldId(ulong guid)
            {
                return unRegisteredIds.Contains(guid);
            }

            private readonly System.Random random = new System.Random();

            private ulong NewID()
            {
                ulong value = LongRandom(100010);
                while (objects.ContainsKey(value)) value = LongRandom(100010);
                return value;
            }

            private ulong lastId = 1;

            /// <summary>
            /// return a random ulong with a min value;
            /// </summary>
            /// <param name="min">min value for this ulong. this should be inferior to 4,294,967,295/2</param>
            /// <returns></returns>
            private ulong LongRandom(ulong min)
            {
                //byte[] buf = new byte[64];
                //random.NextBytes(buf);
                //ulong longRand = (ulong)Mathf.Abs(BitConverter.ToInt64(buf, 0));
                //if (longRand < min) return longRand + min;
                //return longRand;
                if (lastId < min)
                    lastId = min;
                ulong longRand = lastId++;
                return longRand;
            }

            public ulong Register(A obj)
            {
                byte[] key = Guid.NewGuid().ToByteArray();
                ulong guid = NewID();
                objects.Add(guid, obj);
                if (unRegisteredIds.Contains(guid))
                    unRegisteredIds.Remove(guid);
                return guid;
            }

            public ulong Register(A obj, ulong guid)
            {
                if (objects.ContainsKey(guid))
                {
                    ulong old = guid;
                    guid = NewID();
                    UMI3DLogger.LogWarning($"Guid [{old}] was already used node register with another id [{guid}]", scope);
                }
                objects.Add(guid, obj);
                if (unRegisteredIds.Contains(guid))
                    unRegisteredIds.Remove(guid);
                return guid;
            }

            public void Remove(ulong guid)
            {
                objects.Remove(guid);
                unRegisteredIds.Add(guid);
            }
        }

        #endregion
    }
}
