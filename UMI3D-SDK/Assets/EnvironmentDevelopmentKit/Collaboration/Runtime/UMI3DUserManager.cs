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

using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class UMI3DUserManager
    {

        /// <summary>
        /// Contain the users connected to the scene.
        /// </summary>
        Dictionary<string, UMI3DCollaborationUser> users = new Dictionary<string, UMI3DCollaborationUser>();
        Dictionary<string, string> loginMap = new Dictionary<string, string>();

        UMI3DAsyncListProperty<UMI3DCollaborationUser> _objectUserList;

        public UMI3DAsyncListProperty<UMI3DCollaborationUser> objectUserList
        {
            get {
                if (_objectUserList == null) _objectUserList = new UMI3DAsyncListProperty<UMI3DCollaborationUser>(UMI3DGlobalID.EnvironementId, UMI3DPropertyKeys.UserList, new List<UMI3DCollaborationUser>(), (u, user) => u.ToUserDto());
                return _objectUserList;
            }
        }

        /// <summary>
        /// get all userDto collection
        /// </summary>
        /// <returns></returns>
        public List<UserDto> ToDto()
        {
            return objectUserList.GetValue().Select(u => u.ToUserDto()).ToList();
        }


        /// <summary>
        /// Return the UMI3D user associated with an identifier.
        /// </summary>
        public UMI3DCollaborationUser GetUser(string id)
        {
            return users.ContainsKey(id) ? users[id] : null;
        }

        /// <summary>
        /// Return the UMI3D user associated with an identifier.
        /// </summary>
        public UMI3DCollaborationUser GetUserByToken(string authorization)
        {
            foreach (UMI3DCollaborationUser u in users.Values)
            {
                if (UMI3DNetworkingKeys.bearer + u.token == authorization)
                    return u;
            }
            return null;
        }

        /// <summary>
        /// Return all UMI3D users.
        /// </summary>
        public IEnumerable<UMI3DCollaborationUser> Users
        {
            get {
                return users.Values;
            }
        }

        /// <summary>
        /// logout a user
        /// </summary>
        /// <param name="user"></param>
        public void Logout(UMI3DCollaborationUser user)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(RemoveUserOnLeave(user));
            if (users.ContainsKey(user.Id()))
                users.Remove(user.Id());
            if (loginMap.ContainsKey(user.login))
                loginMap.Remove(user.login);
            user.SetStatus(StatusType.NONE);
            user.Logout();
        }

        /// <summary>
        /// Mark a user as missing.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerator ConnectionClose(string id)
        {
            Debug.Log($"connection close {id}");
            if (users.ContainsKey(id))
            {
                var user = users[id];
                user.SetStatus(StatusType.MISSING);
            }
            else Debug.Log($"{id} not found");
            yield break;
        }

        /// <summary>
        /// Create a User.
        /// </summary>
        /// <param name="Login">Login of the user.</param>
        /// <param name="connection">Websoket connection of the user.</param>
        /// <param name="Callback">Callback called when the user has been created.</param>
        public void CreateUser(string Login, UMI3DWebSocketConnection connection, Action<UMI3DCollaborationUser, bool> Callback)
        {
            UMI3DCollaborationUser user;
            bool reconnection = false;
            if (loginMap.ContainsKey(Login) && users.ContainsKey(loginMap[Login]))
            {
                user = users[loginMap[Login]];
                user.connection = connection;
                reconnection = true;
            }
            else
            {
                user = new UMI3DCollaborationUser(Login, connection);

                users.Add(user.Id(), user);
            }
            Callback.Invoke(user, reconnection);
        }

        /// <summary>
        /// Notify that a user ended connection and join.
        /// </summary>
        /// <param name="user"></param>
        public void UserJoin(UMI3DCollaborationUser user)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(AddUserOnJoin(user));
        }

        /// <summary>
        /// Notify a user status change.
        /// </summary>
        /// <param name="user"></param>
        public void NotifyUserStatusChanged(UMI3DCollaborationUser user)
        {
            if (user != null)
                UnityMainThreadDispatcher.Instance().Enqueue(UpdateUser(user));
        }


        IEnumerator AddUserOnJoin(UMI3DCollaborationUser user)
        {
            yield return new WaitForFixedUpdate();
            UMI3DCollaborationServer.Dispatch(new Transaction() { reliable = true, Operations = new List<Operation>() { objectUserList.Add(user) } });
        }

        IEnumerator RemoveUserOnLeave(UMI3DCollaborationUser user)
        {
            yield return new WaitForFixedUpdate();
            UMI3DCollaborationServer.Dispatch(new Transaction() { reliable = true, Operations = new List<Operation>() { objectUserList.Remove(user) } });
        }

        IEnumerator UpdateUser(UMI3DCollaborationUser user)
        {
            yield return new WaitForFixedUpdate();
            int index = objectUserList.GetValue().IndexOf(user);
            var operation = new SetEntityListProperty()
            {
                users = new HashSet<UMI3DUser>() { },
                entityId = UMI3DGlobalID.EnvironementId,
                property = UMI3DPropertyKeys.UserList,
                index = index,
                value = user.ToUserDto()
            };
            operation += UMI3DEnvironment.GetEntities<UMI3DUser>();
            UMI3DCollaborationServer.Dispatch(new Transaction() { reliable = true, Operations = new List<Operation>() { operation } });
        }


        /// <summary>
        /// Called when a status update is received by the real-time connection.
        /// </summary>
        public void OnStatusUpdate(string id, StatusType status)
        {
            if (users.ContainsKey(id))
            {
                users[id].SetStatus(status);
            }
        }
    }
}