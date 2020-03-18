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
using UnityEngine;
using UnityEngine.Rendering;
using umi3d.common;
using System.Globalization;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace umi3d.cdk
{
    /// <summary>
    /// Main class for UMI3D Browser.
    /// </summary>
    public class UMI3DBrowser : Singleton<UMI3DBrowser>
    {

        /// <summary>
        /// User id on server.
        /// </summary>
        private string userId = null;

        /// <summary>
        /// User id on server.
        /// </summary>
        public static string UserId
        {
            get
            {
                return Exist ? Instance.userId : null;
            }
            set
            {
                if (Exist)
                    Instance.userId = value;
            }
        }

        /// <summary>
        /// Username on server.
        /// </summary>
        public static string UserName;

        /// <summary>
        /// Environment connected to.
        /// </summary>
        private MediaDto environment;

        /// <summary>
        /// Environment connected to.
        /// </summary>
        public static MediaDto Media
        {
            get
            {
                return Exist ? Instance.environment : null;
            }
            set
            {
                if (Exist)
                    Instance.environment = value;
            }
        }

        /// <summary>
        /// Is the keyboard azerty or qwerty ?
        /// </summary>
        public static bool useQwerty = false;

        /// <summary>
        /// Navigation x-axis.
        /// </summary>
        public static string navigationAxis_X { get { return useQwerty ? "NavX_qwerty" : "NavX"; } }

        /// <summary>
        /// Navigation y-axis.
        /// </summary>
        public static string navigationAxis_Y { get { return useQwerty ? "NavY_qwerty" : "NavY"; } }

        /// <summary>
        /// Navigation z-axis.
        /// </summary>
        public static string navigationAxis_Z { get { return useQwerty ? "NavZ_qwerty" : "NavZ"; } }

        public QualityType quality;
        public string os;

        private string signaling = "";
        public static string Signaling
        {
            get
            {
                return Exist ? Instance.signaling : null;
            }
            set
            {
                if (Exist)
                    Instance.signaling = value;
            }
        }

        /// <summary>
        /// Environment url.
        /// </summary>
        private string dataRoot = "";
        public static string DataRoot
        {
            get
            {
                return Exist ? Instance.dataRoot : null;
            }
            set
            {
                if (Exist)
                    Instance.dataRoot = value;
            }
        }

        [System.Obsolete("Will be deleted soon")]
        public bool connectOnStart = true;

        /// <summary>
        /// Raised on connection to an environment.
        /// </summary>
        public UnityEvent onStartLoading;

        #region modules


        /// <summary>
        /// Player associated to the browser.
        /// </summary>
        public static AbstractInteractionMapper interactionMapper
        {
            get
            {
                return Exist ? AbstractInteractionMapper.Instance : null;
            }
        }

        /// <summary>
        /// UMI3D Scene.
        /// </summary>
        [SerializeField]
        private AbstractScene scene = null;
        /// <summary>
        /// UMI3D Scene.
        /// </summary>
        public static AbstractScene Scene
        {
            get
            {
                return Exist ? Instance.scene : null;
            }
        }

        /// <summary>
        /// Navigation method currently used.
        /// </summary>
        [SerializeField]
        private AbstractNavigation navigation;
        /// <summary>
        /// Navigation method currently used.
        /// </summary>
        public static AbstractNavigation Navigation
        {
            get
            {
                return Exist ? Instance.navigation : null;
            }
            set
            {
                if (Exist)
                    Instance.navigation = value;
            }
        }

        [SerializeField]
        private AbstractNotificationManager notificationManager_ = null;
        public static AbstractNotificationManager notificationManager
        {
            get { return Exist ? Instance.notificationManager_ : null; }
        }

        #endregion

        /// <summary>
        /// Media loading position for media environment.
        /// </summary>
        public Transform MediaAnchor;

        [SerializeField]
        private Transform worldAnchor = null;
        /// <summary>
        /// World position.
        /// </summary>
        public static Transform WorldAnchor { get { return Exist ? Instance.worldAnchor : null; } }

        private List<AssetBundle> loadedBundles = new List<AssetBundle>();


        #region Connection/Disconnection public events

        /// <summary>
        /// Unity event for string.
        /// </summary>
        private class StringEvent : UnityEvent<string> { }

        /// <summary>
        /// Raised on connection to an environment (the argument is the environment's name).
        /// </summary>
        private StringEvent onConnectionToEnvironment = new StringEvent();

        /// <summary>
        /// Raised on disconnection from an environment (the argument is the environment's name).
        /// </summary>
        private StringEvent onDisconnectionFromEnvironment = new StringEvent();


        /// <summary>
        /// Subscribe a given callback to the event raised on connection to an environment.
        /// </summary>
        /// <param name="callback">Callback to subscribe (the argument is the environment's url)</param>
        public static void SubscribeToConnectionToEnvironmentEvent(UnityAction<string> callback)
        {
            Instance.onConnectionToEnvironment.AddListener(callback);
        }

        /// <summary>
        /// Subscribe a given callback to the event raised on disconnection from an environment.
        /// </summary>
        /// <param name="callback">Callback to subscribe (the argument is the environment's url)</param>
        public static void SubscribeToDisconnectionFromEnvironmentEvent(UnityAction<string> callback)
        {
            Instance.onDisconnectionFromEnvironment.AddListener(callback);
        }

        #endregion



        /// <summary>
        /// Return true if the browser is connected to an environment.
        /// </summary>
        /// <returns></returns>
        public bool Connected()
        {
            return UMI3DWebSocketClient.Connected();
        }

        protected override void Awake()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            base.Awake();
        }

        void Start()
        {
            Application.runInBackground = true;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = Color.white;
            RenderSettings.ambientIntensity = 0.1f;

            ResetModules();
        }

        protected override void OnDestroy()
        {
            UMI3DHttpClient.Logout();
            base.OnDestroy();
        }

        /// <summary>
        /// Reset networking, player and scene.
        /// </summary>
        public static void ResetModules()
        {
            Instance.userId = null;
            UMI3DHttpClient.StopUpdatesPooling();
            if (interactionMapper != null)
                interactionMapper.ResetModule();
            if (Instance.scene != null)
                Instance.scene.ResetModule();
            foreach (AssetBundle bundle in Instance.loadedBundles)
            {
                bundle.Unload(false);
            }
        }

        /// <summary>
        /// Enter in an environment.
        /// </summary>
        /// <param name="data"></param>
        public static void OpenEnvironment(EnterDto data)
        {
            UserId = data.UserID;

            Instance.StartCoroutine(Instance.LoadRequiredResources(Media.requiredResources,
                () =>
                {
                    UMI3DHttpClient.PoolUpdates();
                    Scene.SetSkybox(Media.Skybox);
                    UMI3DWebSocketClient.Init();
                    Instance.StartCoroutine(Instance.EnterTeleportationAfterSetup(data.UserPosition));
                    Instance.onStartLoading.Invoke();
                },
                err =>
                {
                    Debug.LogError(err);
                }));


        }


        private IEnumerator LoadRequiredResources(List<ResourceDto> resources, System.Action onSuccess, System.Action<string> onError)
        {
            foreach (ResourceDto resource in resources)
            {
                UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(resource.Url);
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    onError?.Invoke(www.error);
                    yield break;
                }
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                if (bundle != null)
                {
                    bundle.LoadAllAssets();
                    loadedBundles.Add(bundle);
                }

                yield return null;
            }

            onSuccess.Invoke();
        }


        public static bool isEnterTeleportationAllowed = false;
        public static Vector3 viewpointOffsetForEnterTeleportation;
        private IEnumerator EnterTeleportationAfterSetup(Vector3 positionToTeleport)
        {
            yield return new WaitUntil(() => isEnterTeleportationAllowed);
            TeleportDto tpData = new TeleportDto();
            if (viewpointOffsetForEnterTeleportation != null)
            {
                tpData.Position = positionToTeleport - viewpointOffsetForEnterTeleportation;
            }
            else
            {
                tpData.Position = positionToTeleport;
            }
            Navigation.Teleport(tpData);
        }

        /// <summary>
        /// State if this client is compatible with an environement.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="extensionNotFound"></param>
        /// <returns></returns>
        static public bool IsCompatible(MediaDto dto, out List<string> extensionNotFound)
        {
            extensionNotFound = new List<string>();
            if (dto.extensionNeeded != null)
                foreach (string extension in dto.extensionNeeded)
                {
                    if (!HDResourceCache.ExtensionReadable.Contains(extension))
                        extensionNotFound.Add(extension);
                }
            return extensionNotFound.Count == 0;
        }

        static public bool IsCompatible(MediaDto dto, out string error)
        {
            error = "[Client version :" + UMI3DVersion.version + "| Environement version :" + dto.VersionMajor + "." + dto.VersionMinor + "." + dto.VersionStatus + "." + dto.VersionDate + "]";
            if (dto.VersionMajor != UMI3DVersion.major)
            {
                error = "Major version is different " + error;
                return false;
            }
            if (dto.VersionMinor != UMI3DVersion.minor)
            {
                error = "Minor version is different " + error;
                return false;
            }
            if (dto.VersionStatus != UMI3DVersion.status)
            {
                error = "Status version is different " + error;
                return false;
            }
            if (dto.VersionDate != UMI3DVersion.date)
            {
                error = "Status version is different " + error;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Change environment using only ip adress with port
        /// </summary>
        /// <param name="url">the ip adress  with the port</param>
        public static void ChangeEnvironment(string url)
        {
            UMI3DHttpClient.GetMedia(url,
                (MediaDto media) =>
                {
                    ChangeEnvironment(media);
                });
        }



        /// <summary>
        /// Change environment.
        /// </summary>
        /// <param name="dto">Environement to enter in</param>
        public static void ChangeEnvironment(MediaDto dto)
        {
            isEnterTeleportationAllowed = false;
            CloseEnvironment();

            UserId = null;
            Media = dto;

            Instance.dataRoot = dto.Url;

            Instance.onConnectionToEnvironment.Invoke(dto.Url);

            Scene.transform.SetParent(dto.EnvironmentType == EnvironmentType.Media ? Instance.MediaAnchor : WorldAnchor, false);

            UMI3DHttpClient.Login(new ConnectionRequestDto()
            {
                UserName = UserName,
                IsImmersive = AbstractScene.isImmersiveDevice,
                Quality = Instance.quality,
                OS = Instance.os,
            });
        }

        /// <summary>
        /// Exit from the current environment.
        /// </summary>
        public static void CloseEnvironment()
        {
            if (Media != null)
                Instance.onDisconnectionFromEnvironment.Invoke(Media.Url);

            UMI3DHttpClient.Logout();
            UMI3DWebSocketClient.Close();
            UserId = null;
            Media = null;
            Instance.dataRoot = null;
            ResetModules();
        }
    }
}