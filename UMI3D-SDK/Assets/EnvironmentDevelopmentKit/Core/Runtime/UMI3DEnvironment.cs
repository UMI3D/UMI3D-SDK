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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;
using UnityEngine.Rendering;

namespace umi3d.edk
{
    public class UMI3DEnvironment : Singleton<UMI3DEnvironment>
    {
        [EditorReadOnly]
        public bool useDto = false;

        #region initialization
        ///<inheritdoc/>
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
        public string environmentName = "test";

        [HideInInspector]
        public List<UMI3DScene> scenes;

        [SerializeField, EditorReadOnly]
        public List<AssetLibrary> globalLibraries;

        [SerializeField, EditorReadOnly]
        private Vector3 defaultStartPosition = new Vector3(0, 0, 0);
        [SerializeField, EditorReadOnly]
        private Vector3 defaultStartOrientation = new Vector3(0, 0, 0);
        static public UMI3DAsyncProperty<Vector3> objectStartPosition { get; protected set; }
        static public UMI3DAsyncProperty<Quaternion> objectStartOrientation { get; protected set; }

        private void Start()
        {
            foreach (UMI3DAbstractNode node in GetComponentsInChildren<UMI3DAbstractNode>(true))
                node.Register();
            scenes = new List<UMI3DScene>(GetComponentsInChildren<UMI3DScene>(true));
            InitDefinition();
        }

        /// <summary>
        /// Get scene's information required for client connection.
        /// </summary>
        public MediaDto ToDto()
        {
            var res = new MediaDto();
            res.name = environmentName;
            res.connection = UMI3DServer.Instance.ToDto();
            res.versionMajor = UMI3DVersion.major;
            res.versionMinor = UMI3DVersion.minor;
            res.versionStatus = UMI3DVersion.status;
            res.versionDate = UMI3DVersion.date;

            return res;
        }

        public virtual GlTFEnvironmentDto ToDto(UMI3DUser user)
        {
            GlTFEnvironmentDto env = new GlTFEnvironmentDto();
            env.id = UMI3DGlobalID.EnvironementId;
            env.scenes.AddRange(scenes.Where(s => s.LoadOnConnection(user)).Select(s => s.ToGlTFNodeDto(user)));
            env.extensions.umi3d = CreateDto();
            WriteProperties(env.extensions.umi3d, user);
            return env;
        }

        /// <summary>
        /// Write Properties on a UMI3DEnvironmentDto.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="user"></param>
        protected virtual void WriteProperties(UMI3DEnvironmentDto dto, UMI3DUser user)
        {
            dto.LibrariesId = globalLibraries.Select(l => l.id).ToList();
            dto.preloadedScenes = objectPreloadedScenes.GetValue(user).Select(r => new PreloadedSceneDto() { scene = r.ToDto() }).ToList();
            dto.ambientType = (AmbientType)objectAmbientType.GetValue(user);
            dto.skyColor = objectSkyColor.GetValue(user);
            dto.horizontalColor = objectHorizonColor.GetValue(user);
            dto.groundColor = objectGroundColor.GetValue(user);
            dto.ambientIntensity = objectAmbientIntensity.GetValue(user);
            dto.skybox = objectAmbientSkyboxImage.GetValue(user)?.ToDto();
            dto.defaultMaterial = defaultMaterial?.ToDto();
        }

        /// <summary>
        /// Create a UMI3DEnvironmentDto.
        /// </summary>
        /// <returns></returns>
        protected virtual UMI3DEnvironmentDto CreateDto()
        {
            return new UMI3DEnvironmentDto();
        }

        public static EnterDto ToEnterDto(UMI3DUser user)
        {
            return new EnterDto() { userPosition = objectStartPosition.GetValue(user), userRotation = objectStartOrientation.GetValue(user), usedDto = Instance.useDto };
        }


        public LibrariesDto ToLibrariesDto(UMI3DUser user)
        {
            List<AssetLibraryDto> libraries = globalLibraries?.Select(l => l.ToDto())?.ToList() ?? new List<AssetLibraryDto>();
            var sceneLib = scenes?.SelectMany(s => s.libraries)?.GroupBy(l => l.id)?.Where(l => !libraries.Any(l2 => l2.libraryId == l.Key))?.Select(l => l.First().ToDto());
            if (sceneLib != null)
                libraries.AddRange(sceneLib);
            return new LibrariesDto() { libraries = libraries };
        }

        static public bool UseLibrary()
        {
            return Exists ? Instance.globalLibraries.Any() || Instance.scenes.Any(s => s.libraries.Any()) : false;
        }
        #endregion

        #region AsyncProperties

        void InitDefinition()
        {
            var id = UMI3DGlobalID.EnvironementId;

            objectStartPosition = new UMI3DAsyncProperty<Vector3>(id, 0, defaultStartPosition);
            objectStartOrientation = new UMI3DAsyncProperty<Quaternion>(id, 0, Quaternion.Euler(defaultStartOrientation));

            objectPreloadedScenes = new UMI3DAsyncListProperty<UMI3DResource>(id, UMI3DPropertyKeys.PreloadedScenes, preloadedScenes, (UMI3DResource r, UMI3DUser user) => new PreloadedSceneDto() { scene = r.ToDto() });
            objectAmbientType = new UMI3DAsyncProperty<AmbientMode>(id, UMI3DPropertyKeys.AmbientType, mode, (mode, user) => (AmbientType)mode);
            objectSkyColor = new UMI3DAsyncProperty<Color>(id, UMI3DPropertyKeys.AmbientSkyColor, skyColor, (c, u) => (SerializableColor)c);
            objectHorizonColor = new UMI3DAsyncProperty<Color>(id, UMI3DPropertyKeys.AmbientHorizontalColor, horizontalColor, (c, u) => (SerializableColor)c);
            objectGroundColor = new UMI3DAsyncProperty<Color>(id, UMI3DPropertyKeys.AmbientGroundColor, groundColor, (c, u) => (SerializableColor)c);
            objectAmbientIntensity = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AmbientIntensity, ambientIntensity);
            objectAmbientSkyboxImage = new UMI3DAsyncProperty<UMI3DResource>(id, UMI3DPropertyKeys.AmbientSkyboxImage, skyboxImage, (r, u) => r.ToDto());

        }


        [SerializeField, EditorReadOnly]
        List<UMI3DResource> preloadedScenes = new List<UMI3DResource>();
        public UMI3DAsyncListProperty<UMI3DResource> objectPreloadedScenes;

        /// <summary>
        /// AsyncProperties of the ambient Type.
        /// </summary>
        AmbientMode mode { get => RenderSettings.ambientMode; }
        public UMI3DAsyncProperty<AmbientMode> objectAmbientType;

        /// <summary>
        /// AsyncProperties of the ambient color.
        /// </summary>
        Color skyColor { get => RenderSettings.ambientSkyColor; }
        public UMI3DAsyncProperty<Color> objectSkyColor;
        /// <summary>
        /// AsyncProperties of the ambient color.
        /// </summary>
        Color horizontalColor { get => RenderSettings.ambientEquatorColor; }
        public UMI3DAsyncProperty<Color> objectHorizonColor;
        /// <summary>
        /// AsyncProperties of the ambient color.
        /// </summary>
        Color groundColor { get => RenderSettings.ambientGroundColor; }
        public UMI3DAsyncProperty<Color> objectGroundColor;
        /// <summary>
        /// AsyncProperties of the ambient Intensity.
        /// </summary>
        float ambientIntensity { get => RenderSettings.ambientIntensity; }
        public UMI3DAsyncProperty<float> objectAmbientIntensity;
        /// <summary>
        /// AsyncProperties of the Skybox Image
        /// </summary>
        [SerializeField, EditorReadOnly]
        UMI3DResource skyboxImage = null;
        public UMI3DAsyncProperty<UMI3DResource> objectAmbientSkyboxImage;
        /// <summary>
        /// Properties of the default Material, it is used to initialise loaded materials in clients. 
        /// </summary>
        [SerializeField]
        UMI3DResource defaultMaterial = null;

        #endregion

        #region entities

        /// <summary>
        /// Scene's preview icon.
        /// </summary>
        /*[SerializeField]
        protected CVEResource skybox = new CVEResource()
        {
            IsLocalFile = true
        };*/

        /// <summary>
        /// Contains the objects stored in the scene.
        /// </summary>
        DictionaryGenerator<UMI3DEntity> entities = new DictionaryGenerator<UMI3DEntity>();

        /// <summary>
        /// Access to all entities of a given type.
        /// </summary>
        public static IEnumerable<E> GetEntities<E>() where E : class, UMI3DEntity
        {
            if (Exists)
                return Instance.entities?.Values?.ToList()?.Where(entities => entities is E)?.Select(e => e as E);
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
            else
                throw new System.NullReferenceException("UMI3DEnvironment doesn't exists !");
        }

        /// <summary>
        /// Get entity by id.
        /// </summary>
        /// <param name="id">Entity to get id</param>
        public static E GetEntity<E>(ulong id) where E : class, UMI3DEntity
        {
            if (Exists)
                return Instance.entities[id] as E;
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
            else
                throw new System.NullReferenceException("UMI3DEnvironment doesn't exists !");
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
            else
                throw new System.NullReferenceException("UMI3DEnvironment doesn't exists !");
        }

        /// <summary>
        /// Remove an object from the scene. 
        /// Supported Types: AbstractObject3D, GenericInteraction, Tool, Toolbox
        /// </summary>
        /// <param name="obj">Object to remove</param>
        public static void Remove(UMI3DEntity obj)
        {
            if (obj != null && Exists)
                Instance?.entities?.Remove(obj.Id());
        }

        public static void Remove(ulong id)
        {
            if (id != 0 && Exists)
                Instance?.entities?.Remove(id);
        }

        public class DictionaryGenerator<A>
        {

            /// <summary>
            /// Contains the  stored objects.
            /// </summary>
            Dictionary<ulong, A> objects = new Dictionary<ulong, A>();

            public Dictionary<ulong, A>.ValueCollection Values { get { return objects.Values; } }

            public A this[ulong key]
            {
                get {
                    if (key == 0)
                        return default;
                    else if (objects.ContainsKey(key))
                        return objects[key];
                    else return default;
                }
            }

            System.Random random = new System.Random();

            ulong NewID()
            {
                ulong value = LongRandom(100010);
                while(objects.ContainsKey(value)) value = LongRandom(100010);
                return value;
            }

            /// <summary>
            /// return a random ulong with a min value;
            /// </summary>
            /// <param name="min">min value for this ulong. this should be inferior to 4,294,967,295/2</param>
            /// <returns></returns>
            ulong LongRandom(ulong min)
            {
                byte[] buf = new byte[64];
                random.NextBytes(buf);
                ulong longRand = (ulong)Mathf.Abs(BitConverter.ToInt64(buf, 0));
                if (longRand < min) return longRand + min;
                return longRand;
            }


            public ulong Register(A obj)
            {
                byte[] key = Guid.NewGuid().ToByteArray();
                ulong guid = NewID();
                objects.Add(guid, obj);
                return guid;
            }

            public ulong Register(A obj, ulong guid)
            {
                if (objects.ContainsKey(guid))
                {
                    ulong old = guid;
                    guid = NewID();
                    Debug.LogWarning($"Guid [{old}] was already used node register with another id [{guid}]");
                }
                objects.Add(guid, obj);
                return guid;
            }

            public void Remove(ulong key)
            {
                objects.Remove(key);
            }

        }

        #endregion
    }
}
