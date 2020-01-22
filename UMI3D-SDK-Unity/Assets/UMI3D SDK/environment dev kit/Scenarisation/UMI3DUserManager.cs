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
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using System.Linq;

namespace umi3d.edk
{
    public class UMI3DUserManager : MonoBehaviour
    {
        /// <summary>
        /// Contain the users connected to the scene.
        /// </summary>
        Dictionary<string, UMI3DUser> UsersMap = new Dictionary<string, UMI3DUser>();

        /// <summary>
        /// Time before removing a user which has lost its connection.
        /// </summary>
        [Tooltip("Time before removing a user which has lost its connection.")]
        public float TimeOutTime = 10;

        /// <summary>
        /// The avatars display mode. Disabled by default.
        /// </summary>
        [SerializeField]
        [Tooltip("The avatars display mode. Disabled by default.")]
        public AvatarDisplayMode AvatarDisplay = AvatarDisplayMode.None;

        /// <summary>
        /// Contain the default avatar GameObject.
        /// </summary>
        [SerializeField]
        [Tooltip("Default avatar GameObject.")]
        public GameObject DefaultAvatar;

        /// <summary>
        /// Contain the default avatar spawn position.
        /// </summary>
        [SerializeField]
        [Tooltip("Default avatar spawn position.")]
        public Transform AvatarStartPosition;

        /// <summary>
        /// Contain an association BoneType - GameObject.
        /// </summary>
        [Serializable]
        public struct BonePrefab
        {
            public BoneType BoneType;
            public GameObject Prefab;
        }

        /// <summary>
        /// Contain the pairs BoneType - GameObject.
        /// </summary>
        [SerializeField]
        [Tooltip("To be completed to associate a type of bone with a GameObject to instantiate.")]
        public BonePrefab[] BonesPrefabs;

        private Dictionary<BoneType, GameObject> prefabsDictionary = new Dictionary<BoneType, GameObject>();

        /// <summary>
        /// Contain the BoneTypes not to use for self-representation. Set in the inspector.
        /// </summary>
        public List<BoneType> BonesToFilter;

        void Start()
        {
            this.pairsToDictionnary();
        }

        /// <summary>
        /// Return the UMI3D user associated with an identifier.
        /// </summary>
        public UMI3DUser Get(string id)
        {
            return UsersMap.ContainsKey(id) ? UsersMap[id] : null;
        }

        /// <summary>
        /// Convert the array of pairs BoneType - GameObject into a dictionary.
        /// </summary
        private void pairsToDictionnary()
        {
            if (BonesPrefabs != null && BonesPrefabs.Length != 0)
                prefabsDictionary = BonesPrefabs.ToDictionary(x => x.BoneType, x => x.Prefab);
        }

        /// <summary>
        /// Called when an Object is received by the real-time connection.
        /// </summary>
        public IEnumerator OnMessage(string id, System.Object data)
        {
            if (UsersMap.ContainsKey(id))
                UsersMap[id].onMessage(data);
            yield return null;
        }

        /// <summary>
        /// Called on a user connection.
        /// </summary>
        public IEnumerator OnConnection(string userId, IUMI3DRealtimeConnection connection)
        {
            var user = Get(userId);
            if (user != null)
                user.SetConnection(connection);

            yield return null;
        }

        /// <summary>
        /// Called on a user login.
        /// </summary>
        public EnterDto Login(ConnectionRequestDto connection)
        {
            GameObject userObj = new GameObject();
            GameObject avatar = new GameObject();
            GameObject viewpoint = new GameObject("viewpoint");
            GameObject Anchor = new GameObject("Anchor");

            viewpoint.transform.SetParent(avatar.transform);
            Anchor.transform.SetParent(avatar.transform);           

            avatar.AddComponent<EmptyObject3D>();

            UMI3DAvatar avt = this.setAvatar(avatar, viewpoint, Anchor);

            avatar.transform.SetParent(transform, false);
            userObj.transform.SetParent(UMI3D.Scene.transform, false);
            UMI3DUser user = userObj.AddComponent<UMI3DUser>();
            user.avatar = avatar.GetComponent<UMI3DAvatar>();

            if (avt != null)
                avt.user = user;

            var id = "u";
            int i = 0;
            while (UsersMap.ContainsKey(id + i))
                i++;
            id += i;

            EnterDto res = user.Create(id);
            res.UserPosition = Vector3.zero;
            if (AvatarStartPosition != null)
                res.UserPosition = (SerializableVector3)AvatarStartPosition.position - new Vector3(user.avatar.viewpoint.transform.position.x, 0, user.avatar.viewpoint.transform.position.z);
            UsersMap.Add(id, user);
            userObj.name = "user " + id;
            avatar.name = "avatar_user " + id;
            user.avatar.anchor.name = "Anchor_user " + id;
            user.ImmersiveDeviceUser = connection.IsImmersive;

            UMI3D.OnUserCreate.Invoke(user);
            return res;
        }

        /// <summary>
        /// Add a UMI3DAvatar component with the appropriate parameters.
        /// </summary>
        private UMI3DAvatar setAvatar(GameObject avatar, GameObject viewpoint, GameObject anchor)
        {
            UMI3DAvatar avt = avatar.AddComponent<UMI3DAvatar>();
            avt.viewpoint = viewpoint;
            avt.anchor = anchor;
            avt.listOfPrefabs = this.prefabsDictionary;
            avt.bonesToFilter = this.BonesToFilter;
            avt.defaultAvatar = this.DefaultAvatar;
            avt.displayMode = this.AvatarDisplay;
            return avt;
        }

        /// <summary>
        /// Called during a connection closure.
        /// </summary>
        public IEnumerator OnRealtimeConnectionClose(string Id)
        {
            var user = Get(Id);
            if (user != null)
            {
                user.SetConnection(null);
                /*
                UMI3D.OnUserQuit.Invoke(user);
                UsersMap.Remove(Id);
                Destroy(user.gameObject);
                */
            }
            yield return null;
        }

        /// <summary>
        /// Called during a connection closure.
        /// </summary>
        public IEnumerator OnConnectionClose(string Id)
        {
            var user = Get(Id);
            if (user != null)
            {
                UMI3D.OnUserQuit.Invoke(user);
                UsersMap.Remove(Id);
                UMI3DAvatarBone.instancesByUserId.Remove(Id);
                Destroy(user.gameObject);
            }
            yield return null;
        }

        /// <summary>
        /// Return the UserMap values.
        /// </summary>
        public IEnumerable<UMI3DUser> GetUsers()
        {
            return UsersMap.Values;
        }

        /// <summary>
        /// Return the lenght of the UserMap.
        /// </summary>
        public int UserCount()
        {
            return UsersMap.Count;
        }
    }
}