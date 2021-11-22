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
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    public class UMI3DServer : Singleton<UMI3DServer>
    {
        [SerializeField]
        protected string ip = "localhost";

        /// <summary>
        /// Initialize the server.
        /// </summary>
        public virtual void Init()
        {
            dataFullPath = Path.Combine(Application.dataPath, dataPath);
            dataFullPath = System.IO.Path.GetFullPath(dataFullPath);

            publicDataFullPath = Path.Combine(Application.dataPath, dataPath, publicDataPath);
            publicDataFullPath = System.IO.Path.GetFullPath(publicDataFullPath);

            privateDataFullPath = Path.Combine(Application.dataPath, dataPath, privateDataPath);
            privateDataFullPath = System.IO.Path.GetFullPath(privateDataFullPath);
        }

        #region ressources
        public static string dataPath = "../data/";
        public static string publicDataPath = "/public";
        public static string privateDataPath = "/private";
        private string publicDataFullPath;
        private string privateDataFullPath;
        private string dataFullPath;

        public static string publicRepository => Instance == null ? null : Instance.publicDataFullPath;
        public static string privateRepository => Instance == null ? null : Instance.privateDataFullPath;
        public static string dataRepository => Instance == null ? null : Instance.dataFullPath;

        public static bool IsInDataRepository(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            return fullPath.StartsWith(dataRepository);
        }

        public static bool IsInPrivateRepository(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            return fullPath.StartsWith(privateRepository);
        }

        public static bool IsInPublicRepository(string path)
        {
            string fullPath = System.IO.Path.GetFullPath(path);
            return fullPath.StartsWith(publicRepository);
        }

        #endregion

        /// <summary>
        /// Return the Url of the Http Server.
        /// </summary>
        /// <returns></returns>
        public static string GetHttpUrl()
        {
            return Instance._GetHttpUrl();
        }

        protected virtual string _GetHttpUrl()
        {
            return ip;
        }

        /*
        /// <summary>
        /// Return the Url of the websocket server.
        /// </summary>
        /// <returns></returns>
        static public string GetWebsocketUrl()
        {
            return Instance._GetWebsocketUrl();
        }

        protected virtual string _GetWebsocketUrl()
        {
            return ip;
        }*/

        public virtual ForgeConnectionDto ToDto()
        {
            return null;
        }

        public virtual void NotifyUserChanged(UMI3DUser user)
        {
        }

        public virtual HashSet<UMI3DUser> UserSet()
        {
            return new HashSet<UMI3DUser>(UMI3DEnvironment.GetEntities<UMI3DUser>());
        }
        public virtual HashSet<UMI3DUser> UserSetWhenHasJoined()
        {
            return new HashSet<UMI3DUser>(UMI3DEnvironment.GetEntities<UMI3DUser>().Where((u) => u.hasJoined));
        }
        public virtual IEnumerable<UMI3DUser> Users()
        {
            return UMI3DEnvironment.GetEntities<UMI3DUser>();
        }

        /// <summary>
        /// Call To Notify a user status change.
        /// </summary>
        /// <param name="user">user that get its staus updated</param>
        /// <param name="status">new status</param>
        public virtual void NotifyUserStatusChanged(UMI3DUser user, StatusType status)
        {
            switch (status)
            {
                case StatusType.CREATED:
                    OnUserCreated.Invoke(user);
                    break;
                case StatusType.READY:
                    OnUserReady.Invoke(user);
                    break;
                case StatusType.AWAY:
                    OnUserAway.Invoke(user);
                    break;
                case StatusType.MISSING:
                    LookForMissing(user);
                    OnUserMissing.Invoke(user);
                    break;
                case StatusType.ACTIVE:
                    OnUserActive.Invoke(user);
                    break;
            }
        }

        protected virtual void LookForMissing(UMI3DUser user) { }


        public static void Dispatch(Transaction transaction)
        {
            if (Exists) Instance._Dispatch(transaction);
        }

        public static void Dispatch(DispatchableRequest dispatchableRequest)
        {
            if (Exists) Instance._Dispatch(dispatchableRequest);
        }

        protected virtual void _Dispatch(Transaction transaction)
        {
        }
        protected virtual void _Dispatch(DispatchableRequest dispatchable)
        {
        }

        #region session
        public UMI3DUserEvent OnUserJoin = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserCreated = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserReady = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserAway = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserMissing = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserActive = new UMI3DUserEvent();
        public UMI3DUserEvent OnUserLeave = new UMI3DUserEvent();
        #endregion
    }
}