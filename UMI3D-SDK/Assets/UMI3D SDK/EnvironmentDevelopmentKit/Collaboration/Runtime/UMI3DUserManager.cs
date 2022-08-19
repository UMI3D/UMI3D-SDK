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
        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.User;

        /// <summary>
        /// Contain the users connected to the scene.
        /// </summary>
        private readonly Dictionary<ulong, UMI3DCollaborationUser> users = new Dictionary<ulong, UMI3DCollaborationUser>();
        private readonly Dictionary<ulong, UMI3DCollaborationUser> lostUsers = new Dictionary<ulong, UMI3DCollaborationUser>();
        private readonly Dictionary<string, UMI3DCollaborationUser> guidMap = new Dictionary<string, UMI3DCollaborationUser>();
        private readonly Dictionary<uint, ulong> forgeMap = new Dictionary<uint, ulong>();

        private readonly List<string> oldTokenOfUpdatedUser = new List<string>();


        private UMI3DAsyncListProperty<UMI3DCollaborationUser> _objectUserList;
        private DateTime lastUpdate = new DateTime();

        public void SetLastUpdate(UMI3DCollaborationUser user) { if (users.ContainsValue(user)) SetLastUpdate(); }

        private void SetLastUpdate() { lastUpdate = DateTime.UtcNow; }
        public UMI3DAsyncListProperty<UMI3DCollaborationUser> objectUserList
        {
            get
            {
                if (_objectUserList == null) _objectUserList = new UMI3DAsyncListProperty<UMI3DCollaborationUser>(UMI3DGlobalID.EnvironementId, UMI3DPropertyKeys.UserList, new List<UMI3DCollaborationUser>(), (u, user) => UMI3DEnvironment.Instance.useDto ? u.ToUserDto() : (object)u);
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

        public PlayerCountDto GetPlayerCount()
        {
            var pc = new PlayerCountDto
            {
                count = users.Count(k => k.Value.status == StatusType.ACTIVE || k.Value.status == StatusType.AWAY),
                lastUpdate = lastUpdate.ToString("MM:dd:yyyy:HH:mm:ss")
            };
            return pc;
        }



        /// <summary>
        /// Return the UMI3D user associated with an identifier.
        /// </summary>
        public UMI3DCollaborationUser GetUser(ulong id)
        {
            lock (users)
            {
                return id != 0 && users.ContainsKey(id) ? users[id] : null;
            }
        }

        /// <summary>
        /// Return the UMI3D user associated with a ForgeNetworkingRemastered Id.
        /// </summary>
        public UMI3DCollaborationUser GetUserByNetworkId(uint id)
        {
            lock (forgeMap)
            {
                ulong uid = forgeMap.ContainsKey(id) ? forgeMap[id] : 0;
                lock (users)
                {
                    return (uid != 0 && users.ContainsKey(uid)) ? users[uid] : null;
                }
            }
        }

        /// <summary>
        /// Return the UMI3D user associated with an identifier.
        /// </summary>
        public (UMI3DCollaborationUser user, bool oldToken) GetUserByToken(string authorization)
        {
            if (authorization.StartsWith(UMI3DNetworkingKeys.bearer))
            {
                string token = authorization.Remove(0, UMI3DNetworkingKeys.bearer.Length);
                return GetUserByNakedToken(token);
            }
            return (null, false);
        }

        private (UMI3DCollaborationUser user, bool oldToken) GetUserByNakedToken(string token)
        {
            lock (users)
            {
                foreach (UMI3DCollaborationUser u in users.Values)
                {
                    if (u.token == token)
                        return (u, true);
                }
                if (oldTokenOfUpdatedUser.Contains(token))
                {
                    return (null, true);
                }
            }
            return (null, false);
        }

        public (UMI3DCollaborationUser user, bool oldToken, bool oldUser) GetUserByNakedTokenForConnection(string token)
        {
            (UMI3DCollaborationUser user, bool oldToken) connected = GetUserByNakedToken(token);
            if (connected.oldToken || connected.user != null)
                return (connected.user, connected.oldToken, false);
            foreach (UMI3DCollaborationUser user in lostUsers.Values)
            {
                if (user.token == token)
                    return (user, true, true);
            }
            return (null, false, false);

        }

        /// <summary>
        /// Return all UMI3D users.
        /// </summary>
        public IEnumerable<UMI3DCollaborationUser> Users
        {
            get
            {
                lock (users)
                {
                    return users.Values.ToList();
                }
            }
        }

        /// <summary>
        /// Return a hashset copy of all UMI3D users.
        /// </summary>
        public HashSet<UMI3DUser> UsersSet()
        {
            lock (users)
            {
                return new HashSet<UMI3DUser>(users.Values);
            }
        }

        /// <summary>
        /// logout a user
        /// </summary>
        /// <param name="user"></param>
        public async void Logout(UMI3DCollaborationUser user, bool notifiedByUser)
        {
            UMI3DLogger.Log($"logout {user.Id()} {user.networkPlayer.NetworkId} {notifiedByUser}", scope);
            UnityMainThreadDispatcher.Instance().Enqueue(RemoveUserOnLeave(user));

            lock (users)
            {
                users.Remove(user.Id());
                SetLastUpdate();
            }
            guidMap.Remove(user.guid);
            forgeMap.Remove(user.networkPlayer.NetworkId);

            user.SetStatus(StatusType.NONE);

            if (!notifiedByUser)
            {
                lostUsers.Add(user.Id(), user);
                await UMI3DAsyncManager.Delay(600000); // wait 10 min for reco
                if (user.status != StatusType.NONE)
                {
                    lostUsers.Remove(user.Id());
                    UMI3DCollaborationServer.Instance.NotifyUnregistered(user);
                    return;
                }
            }

            user.Logout();
            UMI3DCollaborationServer.Instance.NotifyUnregistered(user);
        }

        public void reconnectUser(UMI3DCollaborationUser user)
        {
            lock (users)
            {
                users.Add(user.Id(), user);
                SetLastUpdate();
            }
            lostUsers.Remove(user.Id());
            guidMap.Add(user.guid, user);
            forgeMap.Add(user.networkPlayer.NetworkId, user.Id());
        }

        /// <summary>
        /// Mark a user as missing.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public void ConnectionClose(UMI3DCollaborationUser user, uint networkId)
        {
            if (user.networkPlayer.NetworkId == networkId)
                user?.SetStatus(StatusType.MISSING);
        }

        public void ConnectUser(NetworkingPlayer player, string token, Action<bool> acceptUser, Action<UMI3DCollaborationUser, bool> onUserCreated)
        {
            (UMI3DCollaborationUser user, bool oldToken, bool oldUser) res = GetUserByNakedTokenForConnection(token);
            UMI3DCollaborationUser user = res.user;
            UMI3DLogger.Log($"Connect User {user != null} {res}", scope);
            if (user != null)
            {
                if (res.oldUser)
                {
                    UMI3DLogger.Log($"Reconnection of lost user ", scope);
                    reconnectUser(user);
                }

                bool reconnection = user.networkPlayer != null || res.oldUser;
                if (reconnection)
                    forgeMap.Remove(user.networkPlayer.NetworkId);
                UMI3DLogger.Log($"Reconnection {reconnection}", scope);

                user.networkPlayer = player;
                forgeMap.Add(player.NetworkId, user.Id());

                acceptUser(true);
                onUserCreated.Invoke(user, reconnection);
            }
            else
                acceptUser(false);
        }

        /// <summary>
        /// Create a User.
        /// </summary>
        /// <param name="LoginDto">Login of the user.</param>
        /// <param name="onUserCreated">Callback called when the user has been created.</param>
        public void CreateUser(RegisterIdentityDto LoginDto, Action<UMI3DCollaborationUser, bool> onUserCreated)
        {
            lock (users)
            {
                UMI3DCollaborationUser user;
                bool reconnection = false;
                if (guidMap.ContainsKey(LoginDto.guid))
                {
                    user = guidMap[LoginDto.guid];
                    oldTokenOfUpdatedUser.Add(user.token);
                    forgeMap.Remove(user.networkPlayer.NetworkId);
                    (UMI3DCollaborationServer.ForgeServer.GetNetWorker() as UDPServer).Disconnect(user.networkPlayer, true);
                    user.Update(LoginDto);
                    user.SetStatus(StatusType.CREATED);
                    reconnection = true;
                }
                else
                {
                    user = new UMI3DCollaborationUser(LoginDto);
                    users.Add(user.Id(), user);
                    guidMap.Add(LoginDto.guid, user);
                    SetLastUpdate();
                }
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

        private IEnumerator AddUserOnJoin(UMI3DCollaborationUser user)
        {
            yield return new WaitForFixedUpdate();
            SetEntityProperty op = objectUserList.Add(user);
            op.users = new HashSet<UMI3DUser>(op.users.Where((u) => u.hasJoined));
            var tr = new Transaction() { reliable = true };
            tr.AddIfNotNull(op);
            UMI3DServer.Dispatch(tr);
        }

        private IEnumerator RemoveUserOnLeave(UMI3DCollaborationUser user)
        {
            yield return new WaitForFixedUpdate();
            SetEntityProperty op = objectUserList.Remove(user);
            if (op == null)
                yield break;
            if (user != null)
                op.users.Remove(user);
            var tr = new Transaction() { reliable = true };
            tr.AddIfNotNull(op);
            UMI3DServer.Dispatch(tr);
        }

        private IEnumerator UpdateUser(UMI3DCollaborationUser user)
        {
            yield return new WaitForFixedUpdate();
            int index = objectUserList.GetValue().IndexOf(user);
            var operation = new SetEntityListProperty()
            {
                users = new HashSet<UMI3DUser>() { },
                entityId = UMI3DGlobalID.EnvironementId,
                property = UMI3DPropertyKeys.UserList,
                index = index,
                value = UMI3DEnvironment.Instance.useDto ? user.ToUserDto() : (object)user,
            };
            operation += UMI3DCollaborationServer.Collaboration.Users.Where((u) => u.hasJoined);
            var tr = new Transaction() { reliable = true };
            tr.AddIfNotNull(operation);
            UMI3DServer.Dispatch(tr);
        }


        /// <summary>
        /// Called when a status update is received by the real-time connection.
        /// </summary>
        public void OnStatusUpdate(ulong id, StatusType status)
        {
            if (users.ContainsKey(id))
            {
                users[id].SetStatus(status);
            }
        }

        public void CollaborationRequest(UMI3DUser user, ConferenceBrowserRequest dto)
        {
            var tr = new Transaction
            {
                reliable = true
            };
            switch (dto.operation)
            {
                case UMI3DOperationKeys.UserMicrophoneStatus:
                    if (users.ContainsKey(dto.id) && (!dto.value || (user.Id() == dto.id)))
                        tr.AddIfNotNull(users[dto.id].microphoneStatus.SetValue(dto.value));
                    break;

                case UMI3DOperationKeys.UserAvatarStatus:
                    if (users.ContainsKey(dto.id) && (!dto.value || (user.Id() == dto.id)))
                        tr.AddIfNotNull(users[dto.id].avatarStatus.SetValue(dto.value));
                    break;

                case UMI3DOperationKeys.UserAttentionStatus:
                    if (users.ContainsKey(dto.id) && user.Id() == dto.id)
                        tr.AddIfNotNull(users[dto.id].attentionRequired.SetValue(dto.value));
                    break;

                case UMI3DOperationKeys.MuteAllMicrophoneStatus:
                    foreach (UMI3DCollaborationUser u in users.Values)
                        tr.AddIfNotNull(u.microphoneStatus.SetValue(false));
                    break;

                case UMI3DOperationKeys.MuteAllAvatarStatus:
                    foreach (UMI3DCollaborationUser u in users.Values)
                        tr.AddIfNotNull(u.avatarStatus.SetValue(false));
                    break;

                case UMI3DOperationKeys.MuteAllAttentionStatus:
                    foreach (UMI3DCollaborationUser u in users.Values)
                        tr.AddIfNotNull(u.attentionRequired.SetValue(false));
                    break;
            }
            tr.Dispatch();
        }

        public void CollaborationRequest(UMI3DUser user, uint operationKey, ByteContainer container)
        {
            bool value;
            ulong id;
            var tr = new Transaction
            {
                reliable = true
            };
            switch (operationKey)
            {
                case UMI3DOperationKeys.UserMicrophoneStatus:
                    id = UMI3DNetworkingHelper.Read<ulong>(container);
                    value = UMI3DNetworkingHelper.Read<bool>(container);
                    if (users.ContainsKey(id) && (!value || (user.Id() == id)))
                        tr.AddIfNotNull(users[id].microphoneStatus.SetValue(value));
                    break;

                case UMI3DOperationKeys.UserAvatarStatus:
                    id = UMI3DNetworkingHelper.Read<ulong>(container);
                    value = UMI3DNetworkingHelper.Read<bool>(container);
                    if (users.ContainsKey(id) && (!value || (user.Id() == id)))
                        tr.AddIfNotNull(users[id].avatarStatus.SetValue(value));
                    break;

                case UMI3DOperationKeys.UserAttentionStatus:
                    id = UMI3DNetworkingHelper.Read<ulong>(container);
                    value = UMI3DNetworkingHelper.Read<bool>(container);
                    if (users.ContainsKey(id) && id == user.Id())
                        tr.AddIfNotNull(users[id].attentionRequired.SetValue(value));
                    break;

                case UMI3DOperationKeys.MuteAllMicrophoneStatus:
                    foreach (UMI3DCollaborationUser u in users.Values)
                        tr.AddIfNotNull(u.microphoneStatus.SetValue(false));
                    break;

                case UMI3DOperationKeys.MuteAllAvatarStatus:
                    foreach (UMI3DCollaborationUser u in users.Values)
                        tr.AddIfNotNull(u.avatarStatus.SetValue(false));
                    break;

                case UMI3DOperationKeys.MuteAllAttentionStatus:
                    foreach (UMI3DCollaborationUser u in users.Values)
                        tr.AddIfNotNull(u.attentionRequired.SetValue(false));
                    break;
            }
            tr.Dispatch();
        }
    }
}