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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;
using UnityEngine.Rendering;

namespace umi3d.edk
{
    public class UMI3DEnvironment : Singleton<UMI3DEnvironment>
    {
        #region initialization

        protected override void Awake()
        {
            base.Awake();
        }
        #endregion

        #region environment description
        /// <summary>
        /// Environment's name.
        /// </summary>
        public string environmentName = "test";

        [HideInInspector]
        public List<UMI3DScene> scenes;

        public List<AssetLibrary> globalLibraries;

        [HideInInspector]
        private Vector3 defaultStartPosition;
        [HideInInspector]
        private Vector3 defaultStartOrentation;
        static public UMI3DAsyncProperty<Vector3> objectStartPosition;
        static public UMI3DAsyncProperty<Quaternion> objectStartQuaternion;



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
            res.websocketUrl = UMI3DServer.GetWebsocketUrl();
            res.httpUrl = UMI3DServer.GetHttpUrl();
            res.Authentication = UMI3DServer.GetAuthentication();
            res.name = environmentName;
            res.connection = UMI3DServer.Instance.ToDto();
            res.versionMajor = UMI3DVersion.major;
            res.versionMinor = UMI3DVersion.minor;
            res.versionStatus = UMI3DVersion.status;
            res.versionDate = UMI3DVersion.date;

            return res;
        }

        public GlTFEnvironmentDto ToDto(UMI3DUser user)
        {
            GlTFEnvironmentDto env = new GlTFEnvironmentDto();
            env.id = UMI3DGlobalID.EnvironementId;
            env.scenes.AddRange(scenes.Select(s => s.ToGlTFNodeDto(user)));
            env.extensions.umi3d = new UMI3DEnvironementDto();
            env.extensions.umi3d.LibrariesId = globalLibraries.Select(l => l.id).ToList();
            env.extensions.umi3d.preloadedScenes = objectPreloadedScenes.GetValue(user).Select( r => new PreloadedSceneDto() { scene = r.ToDto() }).ToList();
            env.extensions.umi3d.ambientType = (AmbientType)objectAmbientType.GetValue(user);
            env.extensions.umi3d.skyColor = objectSkyColor.GetValue(user);
            env.extensions.umi3d.horizontalColor = objectHorizonColor.GetValue(user);
            env.extensions.umi3d.groundColor = objectGroundColor.GetValue(user);
            env.extensions.umi3d.ambientIntensity = objectAmbientIntensity.GetValue(user);
            env.extensions.umi3d.skybox = objectAmbientSkyboxImage.GetValue(user)?.ToDto();
            return env;
        }

        public static EnterDto ToEnterDto(UMI3DUser user)
        {
            return new EnterDto();
        }


        public LibrariesDto ToLibrariesDto(UMI3DUser user)
        {
            List<AssetLibraryDto> libraries = globalLibraries.Select(l => l.ToDto()).ToList();
            libraries.AddRange(scenes.SelectMany(s => s.libraries).GroupBy(l=>l.id).Select(l=>l.First().ToDto()));
            return new LibrariesDto() { libraries = libraries };
        }

        static public bool UseLibrary()
        {
            return Exists ? Instance.globalLibraries.Any() || Instance.scenes.Any(s=>s.libraries.Any()): false;
        }
        #endregion

        #region AsyncProperties

        void InitDefinition()
        {
            var id = UMI3DGlobalID.EnvironementId;

            objectStartPosition = new UMI3DAsyncProperty<Vector3>(id, null, defaultStartPosition);
            objectStartQuaternion = new UMI3DAsyncProperty<Quaternion>(id, null, Quaternion.Euler(defaultStartOrentation));

            objectPreloadedScenes = new UMI3DAsyncListProperty<UMI3DResource>(id, UMI3DPropertyKeys.PreloadedScenes, preloadedScenes, (UMI3DResource r, UMI3DUser user) => new PreloadedSceneDto() { scene = r.ToDto() });
            objectAmbientType = new UMI3DAsyncProperty<AmbientMode>(id, UMI3DPropertyKeys.AmbientType, mode, (mode, user) => (AmbientType)mode);
            objectSkyColor = new UMI3DAsyncProperty<Color>(id, UMI3DPropertyKeys.AmbientSkyColor, skyColor, (c, u) => (SerializableColor)c);
            objectHorizonColor = new UMI3DAsyncProperty<Color>(id, UMI3DPropertyKeys.AmbientSkyColor, horizontalColor, (c, u) => (SerializableColor)c);
            objectGroundColor = new UMI3DAsyncProperty<Color>(id, UMI3DPropertyKeys.AmbientSkyColor, groundColor, (c, u) => (SerializableColor)c);
            objectAmbientIntensity = new UMI3DAsyncProperty<float>(id, UMI3DPropertyKeys.AmbientIntensity, ambientIntensity);
            objectAmbientSkyboxImage = new UMI3DAsyncProperty<UMI3DResource>(id, UMI3DPropertyKeys.AmbientSkyboxImage, skyboxImage, (r, u) => r.ToDto());
        }


        [SerializeField]
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
        [SerializeField]
        UMI3DResource skyboxImage = null;
        public UMI3DAsyncProperty<UMI3DResource> objectAmbientSkyboxImage;

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
                return Instance.entities.Values?.ToList().Where(entities => entities is E).Select(e => e as E);
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
        public static E GetEntity<E>(string id) where E : class, UMI3DEntity
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
        public static string Register(UMI3DEntity entity)
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
        /// Remove an object from the scene. 
        /// Supported Types: AbstractObject3D, GenericInteraction, Tool, Toolbox
        /// </summary>
        /// <param name="obj">Object to remove</param>
        public static void Remove(UMI3DEntity obj)
        {
            if (obj != null && Exists)
                Instance?.entities?.Remove(obj.Id());
        }

        public class DictionaryGenerator<A>
        {

            /// <summary>
            /// Contains the  stored objects.
            /// </summary>
            Dictionary<string, A> objects = new Dictionary<string, A>();

            public Dictionary<string, A>.ValueCollection Values { get { return objects.Values; } }

            public A this[string key]
            {
                get
                {
                    if (key == null || key.Length == 0)
                        return default;
                    else if (objects.ContainsKey(key))
                        return objects[key];
                    else return default;
                }
            }

            public string Register(A obj)
            {
                byte[] key = Guid.NewGuid().ToByteArray();
                string guid = Convert.ToBase64String(key);
                objects.Add(guid, obj);
                return guid;
            }

            public void Remove(string key)
            {
                objects.Remove(key);
            }

        }

        #endregion
    }
}
