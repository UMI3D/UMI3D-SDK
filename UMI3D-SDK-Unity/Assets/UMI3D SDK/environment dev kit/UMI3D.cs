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
using UnityEngine.Events;
using System.Globalization;

namespace umi3d.edk
{
    public class UMI3D : Singleton<UMI3D>
    {

        /// <summary>
        /// Networking protocols availables.
        /// </summary>
        public enum Protocol { WebSocket, WebRTC };

        /// <summary>
        /// True if this environment has been fully initialized and is currently running.
        /// </summary>
        public bool isRunning { get; private set; } = false;

        /// <summary>
        /// True if this environment has been fully initialized.
        /// </summary>
        public static bool Ready
        {
            get { return Exist ?  Instance.ready : false; }
            set { }
        }

        /// <summary>
        /// Contains server to communicate with information.
        /// </summary>
        public static IUMI3DServer Server
        {
            get
            {
                if (!Exist)
                    return null;

                if (Instance.server == null)
                {
                    Instance.server = Instance.gameObject.GetComponent<IUMI3DServer>();
                    if ((Instance.server == null) && (Instance.protocol == Protocol.WebSocket))
                        Instance.server = Instance.gameObject.AddComponent<WebSocketUMI3DServer>();
                    else if ((Instance.server as WebSocketUMI3DServer) != null)
                        Instance.protocol = Protocol.WebSocket;
                }
                return Instance.server;
            }
            set { }
        }

        /// <summary>
        /// UMI3D Scene associated with this environment.
        /// </summary>
        public static UMI3DScene Scene
        {
            get { return Exist ? Instance.scene : null; }
            set { if (Exist && !Instance.isRunning) Instance.scene = value;}
        }

        /// <summary>
        /// User manager used for this environment.
        /// </summary>
        public static UMI3DUserManager UserManager
        {
            get { return Exist ? Instance.userManager : null; }
            set { if (Exist && !Instance.isRunning) Instance.userManager = value; }
        }

        /// <summary>
        /// Get scene origin.
        /// </summary>
        [System.Obsolete("This method is obsolete. Use UMI3DScene.Origin instead.")]
        public Transform Origin
        {
            get
            {
                if (scene == null)
                {
                    scene = new GameObject("environment").AddComponent<UMI3DScene>();
                    scene.transform.SetParent(transform, false);
                }
                return scene.transform;
            }
            set { }
        }

        /// <summary>
        /// Event invoked on user creation.
        /// </summary>
        public static UnityEvent<UMI3DUser> OnUserCreate
        {
            get { return Exist ? Instance.onUserCreate : null; }
        }

        /// <summary>
        /// Event called on user quit.
        /// </summary>
        public static UnityEvent<UMI3DUser> OnUserQuit
        {
            get { return Exist ? Instance.onUserQuit : null; }
        }

        /// <summary>
        /// Event invoked on user creation.
        /// </summary>
        public static UnityEvent OnRun
        {
            get { return Exist ? Instance.onRun : null; }
        }

        /// <summary>
        /// Event called on user quit.
        /// </summary>
        public static UnityEvent OnStop
        {
            get { return Exist ? Instance.onStop : null; }
            set { }
        }

        /// <summary>
        /// Target framerate for updates through network.
        /// </summary>
        public static int TargetFrameRate
        {
            get { return Exist ? Instance.targetFrameRate : -1; }
            set { if (Exist || !Instance.isRunning) Instance.targetFrameRate = value; }
        }

        /// <summary>
        /// If set to true, the environment will run itself automatically on application start (on awake).
        /// </summary>
        public static bool AutoRun
        {
            get { return Exist ? Instance.autoRun : false; }
            set { if (!Instance || !Instance.isRunning) Instance.autoRun = value; }
        }

        /// <summary>
        /// Network protocol used for real-time updates.
        /// </summary>
        public Protocol protocol = Protocol.WebSocket;

        [SerializeField] private int targetFrameRate = 40;
        [SerializeField] private bool autoRun = true;
        [SerializeField] private UMI3DScene scene;
        [SerializeField] private UMI3DUserManager userManager;
        private bool ready = false;
        private IUMI3DServer server;
        private UnityEvent<UMI3DUser> onUserCreate = new UMI3DUserEvent();
        private UnityEvent<UMI3DUser> onUserQuit = new UMI3DUserEvent();
        private UnityEvent onRun = new UnityEvent();
        private UnityEvent onStop = new UnityEvent();

        /// <summary>
        /// Start the environment and make it available for clients.
        /// </summary>
        public void Run()
        {
            Server.Init();
            isRunning = true;
            onRun.Invoke();
        }

        /// <summary>
        /// Stop the environment and make it available for clients.
        /// </summary>
        public void Stop()
        {
            server = gameObject.GetComponent<IUMI3DServer>();
            if (server != null)
                Destroy(server as MonoBehaviour);
            isRunning = false;
            onStop.Invoke();
        }

        /// <summary>
        /// Start the environment and make it available for clients.
        /// </summary>
        public static void RunUMI3D()
        {
            if (Instance != null)
                Instance.Run();
        }

        /// <summary>
        /// Start the environment and make it available for clients.
        /// </summary>
        public static void StopUMI3D()
        {
            if (Instance != null)
                Instance.Stop();
        }


        protected override void Awake()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            bool wasNull = !Exist;
            base.Awake();
            if (wasNull)
            {
                Application.runInBackground = true;
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = TargetFrameRate;
                ready = true;
                if (autoRun)
                    Run();
            }
        }

        static public string GetResourceRoot()
        {

            return System.IO.Path.GetFullPath(UnityEngine.Application.dataPath + @"/../Public/");
        }

        static public string GetDefaultRoot()
        {
            UMI3D instance = Exist ?  Instance : GameObject.Find("UMI3D").GetComponent<UMI3D>();
            if (instance == null)
                throw new System.Exception("No UMI3D node found");
            if(instance?.scene?.OSQualitycollection?.DefaultQuality?.path == null)
            {
                throw new System.Exception("Default path of OSQualitycollection in UMI3DScene not set");
            }
            instance.scene.OSQualitycollection.DefaultQuality.path = instance.scene.OSQualitycollection.DefaultQuality.path.Replace(@"\", "/");
            if (instance.scene.OSQualitycollection.DefaultQuality.path != null && instance.scene.OSQualitycollection.DefaultQuality.path != "" && !(instance.scene.OSQualitycollection.DefaultQuality.path.StartsWith("/") /*|| Path.StartsWith(@"\")*/))
            {
                instance.scene.OSQualitycollection.DefaultQuality.path = "/" + instance.scene.OSQualitycollection.DefaultQuality.path;
            }
            return Path.Combine(GetResourceRoot() , instance.scene.OSQualitycollection.DefaultQuality.path);
        }

    }
}
