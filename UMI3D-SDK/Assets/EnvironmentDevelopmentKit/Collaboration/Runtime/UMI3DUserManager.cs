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

using BeardedManStudios.Forge.Networking;
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
        Dictionary<uint, string> forgeMap = new Dictionary<uint, string>();

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
            lock (users)
            {
                return id != null && users.ContainsKey(id) ? users[id] : null;
            }
        }

        /// <summary>
        /// Return the UMI3D user associated with a ForgeNetworkingRemastered Id.
        /// </summary>
        public UMI3DCollaborationUser GetUserByNetworkId(uint id)
        {
            lock (forgeMap)
            {
                string uid = forgeMap.ContainsKey(id) ? forgeMap[id] : null;
                lock (users)
                {
                    return (uid != null && users.ContainsKey(uid)) ? users[uid] : null;
                }
            }
        }

        /// <summary>
        /// Return the UMI3D user associated with an identifier.
        /// </summary>
        public UMI3DCollaborationUser GetUserByToken(string authorization)
        {
            lock (users)
            {
                foreach (UMI3DCollaborationUser u in users.Values)
                {
                    if (UMI3DNetworkingKeys.bearer + u.token == authorization)
                        return u;
                }
            }
            return null;
        }

        /// <summary>
        /// Return all UMI3D users.
        /// </summary>
        public IEnumerable<UMI3DCollaborationUser> Users
        {
            get {
                lock (users)
                {
                    return users.Values.ToList();
                }
            }
        }

        /// <summary>
        /// logout a user
        /// </summary>
        /// <param name="user"></param>
        public void Logout(UMI3DCollaborationUser user)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(RemoveUserOnLeave(user));
            lock (users)
            {
                users.Remove(user.Id());
            }
            loginMap.Remove(user.login);
            forgeMap.Remove(user.networkPlayer.NetworkId);
            user.SetStatus(StatusType.NONE);
            user.Logout();
        }

        /// <summary>
        /// Mark a user as missing.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public void ConnectionClose(string id)
        {
            if (users.ContainsKey(id))
            {
                var user = users[id];
                user.SetStatus(StatusType.MISSING);
            }
            else Debug.Log($"{id} not found");
        }

        /// <summary>
        /// Create a User.
        /// </summary>
        /// <param name="LoginDto">Login of the user.</param>
        /// <param name="onUserCreated">Callback called when the user has been created.</param>
        public void CreateUser(NetworkingPlayer player, IdentityDto LoginDto, Action<bool> acceptUser, Action<UMI3DCollaborationUser, bool> onUserCreated)
        {
            lock (users)
            {
                UMI3DCollaborationUser user;
                bool reconnection = false;
                if (LoginDto == null)
                {
                    Debug.LogWarning("user try to use empty login");
                    acceptUser(false);
                    return;
                }
                if (LoginDto.login == null || LoginDto.login == "")
                {
                    LoginDto.login = player.NetworkId.ToString();
                }

                if (loginMap.ContainsKey(LoginDto.login))
                {
                    if (loginMap[LoginDto.login] != LoginDto.userId || LoginDto.userId != null && users.ContainsKey(LoginDto.userId))
                    {
                        Debug.LogWarning($"Login [{LoginDto.login}] already us by an other user");
                        acceptUser(false);
                        return;
                    }
                    else
                    {
                        user = users[LoginDto.userId];
                        forgeMap.Remove(user.networkPlayer.NetworkId);
                        reconnection = true;
                    }
                }
                else
                {
                    user = new UMI3DCollaborationUser(LoginDto.login);
                    loginMap[LoginDto.login] = user.Id();
                    users.Add(user.Id(), user);
                }
                user.networkPlayer = player;
                forgeMap.Add(player.NetworkId, user.Id());
                acceptUser(true);
                onUserCreated.Invoke(user, reconnection);
            }
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
            var op = objectUserList.Add(user);
            op.users.Remove(user);
            UMI3DCollaborationServer.Dispatch(new Transaction() { reliable = true, Operations = new List<Operation>() { op } });
        }

        IEnumerator RemoveUserOnLeave(UMI3DCollaborationUser user)
        {
            yield return new WaitForFixedUpdate();
            var op = objectUserList.Remove(user);
            if (op == null)
                yield break;
            if(user != null)
                op.users.Remove(user);
            UMI3DCollaborationServer.Dispatch(new Transaction() { reliable = true, Operations = new List<Operation>() { op } });
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