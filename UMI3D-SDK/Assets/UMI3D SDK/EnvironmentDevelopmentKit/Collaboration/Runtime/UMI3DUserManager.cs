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
using System.Net;
using umi3d.common;
using umi3d.common.collaboration.dto.signaling;
using umi3d.worldController;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class UMI3DUserManager
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.User;

        /// <summary>
        /// Contain the users connected to the scene.
        /// </summary>
        private readonly Dictionary<ulong, UMI3DCollaborationAbstractContentUser> users = new();
        private readonly List<UMI3DCollaborationDistantUser> resourcesOnlyUsers = new();
        private readonly Dictionary<ulong, UMI3DCollaborationAbstractContentUser> lostUsers = new();
        private readonly Dictionary<string, UMI3DCollaborationAbstractContentUser> guidMap = new();
        private readonly Dictionary<uint, ulong> forgeMap = new Dictionary<uint, ulong>();

        private readonly List<string> oldTokenOfUpdatedUser = new List<string>();

        private UMI3DAsyncListProperty<UMI3DCollaborationAbstractContentUser> _objectUserList;
        private DateTime lastUpdate = new DateTime();

        public event Action<UMI3DCollaborationUser, UserActionDto> OnActionClicked;

        public void SetLastUpdate(UMI3DCollaborationAbstractContentUser user) { if (users.ContainsValue(user)) SetLastUpdate(); }

        private void SetLastUpdate() { lastUpdate = DateTime.UtcNow; }
        public UMI3DAsyncListProperty<UMI3DCollaborationAbstractContentUser> objectUserList
        {
            get
            {
                if (_objectUserList == null) _objectUserList = new UMI3DAsyncListProperty<UMI3DCollaborationAbstractContentUser>(UMI3DGlobalID.EnvironmentId, UMI3DPropertyKeys.UserList, new List<UMI3DCollaborationAbstractContentUser>(), (u, user) => UMI3DEnvironment.Instance.useDto ? u.ToUserDto(user) : (object)u);
                return _objectUserList;
            }
        }

        /// <summary>
        /// get all userDto collection
        /// </summary>
        /// <returns></returns>
        public List<UserDto> ToDto(UMI3DUser user)
        {
            return objectUserList.GetValue().Select(u => u.ToUserDto(user)).ToList();
        }

        /// <summary>
        /// Get the current number of connected users in the environment as a DTO.
        /// </summary>
        /// <returns></returns>
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
        public UMI3DCollaborationAbstractContentUser GetUser(ulong id)
        {
            lock (users)
            {
                return id != 0 && users.ContainsKey(id) ? users[id] : null;
            }
        }

        /// <summary>
        /// Return the UMI3D user associated with a ForgeNetworkingRemastered Id.
        /// </summary>
        public UMI3DCollaborationAbstractContentUser GetUserByNetworkId(uint id)
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
        public (UMI3DCollaborationAbstractContentUser user, bool oldToken, bool resourcesOnly) GetUserByToken(string authorization)
        {
            if (authorization.StartsWith(UMI3DNetworkingKeys.bearer))
            {
                string token = authorization.Remove(0, UMI3DNetworkingKeys.bearer.Length);
                return GetUserByNakedToken(token);
            }
            return (null, false, false);
        }

        private (UMI3DCollaborationAbstractContentUser user, bool oldToken, bool resourcesOnly) GetUserByNakedToken(string token)
        {
            lock (users)
            {
                foreach (UMI3DCollaborationAbstractContentUser u in users.Values)
                    if (u.token == token)
                        return (u, true, false);

                if (oldTokenOfUpdatedUser.Contains(token))
                    return (null, true, false);

                var du = resourcesOnlyUsers.FirstOrDefault(u => u.token == token);
                if (du is not null)
                    return (null, false, true);

            }
            return (null, false, false);
        }

        public (UMI3DCollaborationAbstractContentUser user, bool oldToken, bool oldUser, bool resourcesOnly) GetUserByNakedTokenForConnection(string token)
        {
            (UMI3DCollaborationAbstractContentUser user, bool oldToken, bool resourcesOnly) connected = GetUserByNakedToken(token);

            if (connected.oldToken || connected.user != null)
                return (connected.user, connected.oldToken, false, connected.resourcesOnly);

            foreach (UMI3DCollaborationAbstractContentUser user in lostUsers.Values)
                if (user.token == token)
                    return (user, true, true, false);

            return (null, false, false, false);

        }

        /// <summary>
        /// Return all UMI3D users.
        /// </summary>
        public IEnumerable<UMI3DCollaborationAbstractContentUser> Users
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
        public async void Logout(UMI3DCollaborationAbstractContentUser user, bool notifiedByUser)
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
                if (!lostUsers.ContainsKey(user.Id()))
                    lostUsers.Add(user.Id(), user);
                else
                    UMI3DLogger.LogError($"Lost users already contains a key with {user.Id()}", scope);

                await UMI3DAsyncManager.Delay(600000); // wait 10 min for reco
                if (user.status == StatusType.NONE)
                {
                    lostUsers.Remove(user.Id());
                    UMI3DCollaborationServer.Instance.NotifyUnregistered(user);
                    return;
                }
            }

            user.Logout();
            UMI3DCollaborationServer.Instance.NotifyUnregistered(user);
        }

        public void reconnectUser(UMI3DCollaborationAbstractContentUser user)
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
        public void ConnectionClose(UMI3DCollaborationAbstractContentUser user, uint networkId)
        {
            if (user.networkPlayer.NetworkId == networkId)
                user?.SetStatus(StatusType.MISSING);
        }

        public void ConnectUser(NetworkingPlayer player, string token, Action<bool> acceptUser, Action<UMI3DCollaborationAbstractContentUser, bool> onUserCreated)
        {
            (UMI3DCollaborationAbstractContentUser user, bool oldToken, bool oldUser, bool resourcesOnly) res = GetUserByNakedTokenForConnection(token);
            UMI3DCollaborationAbstractContentUser user = res.user;
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


        public void CreateUserResourcesOnly(RegisterIdentityDto LoginDto, Action<UMI3DUser, bool> onUserCreated)
        {
            var user = new UMI3DCollaborationDistantUser(LoginDto);

            resourcesOnlyUsers.Add(user);/// = DateTime.UtcNow + TimeSpan.FromHours(1);

            onUserCreated.Invoke(user, false);
        }

        /// <summary>
        /// Create a User.
        /// </summary>
        /// <param name="LoginDto">Login of the user.</param>
        /// <param name="onUserCreated">Callback called when the user has been created.</param>
        public void CreateUser(RegisterIdentityDto LoginDto, Action<UMI3DCollaborationAbstractContentUser, bool> onUserCreated)
        {
            UMI3DLogger.Log($"CreateUser() begins", scope);

            UMI3DCollaborationAbstractContentUser user;
            bool reconnection = false;
            if (guidMap.ContainsKey(LoginDto.guid))
            {
                user = guidMap[LoginDto.guid];

                UMI3DLogger.Log($"CreateUser() : {user.Id()} {user.login} already contained in guidMap", scope);

                oldTokenOfUpdatedUser.Add(user.token);
                forgeMap.Remove(user.networkPlayer.NetworkId);
                (UMI3DCollaborationServer.ForgeServer.GetNetWorker() as UDPServer).Disconnect(user.networkPlayer, true);
                user.Update(LoginDto);
                user.SetStatus(StatusType.CREATED);
                reconnection = true;
            }
            else
            {
                user =
                    LoginDto.isServer ?
                        new UMI3DServerUser(LoginDto) :
                        new UMI3DCollaborationUser(LoginDto);

                UMI3DLogger.Log($"CreateUser() : {user.Id()} {user.login} new, create lock [{user.GetType()}]", scope);
                lock (users)
                {
                    users.Add(user.Id(), user);
                }
                UMI3DLogger.Log($"CreateUser() : {user.Id()} {user.login} added to users, release lock", scope);

                guidMap.Add(LoginDto.guid, user);
                SetLastUpdate();
            }

            onUserCreated.Invoke(user, reconnection);

            UMI3DLogger.Log($"CreateUser() ends", scope);
        }

        /// <summary>
        /// Notify that a user ended connection and join.
        /// </summary>
        /// <param name="user"></param>
        public void UserJoin(UMI3DCollaborationAbstractContentUser user)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(AddUserOnJoin(user));
        }

        /// <summary>
        /// Notify a user status change.
        /// </summary>
        /// <param name="user"></param>
        public void NotifyUserStatusChanged(UMI3DCollaborationAbstractContentUser user)
        {
            if (user != null && user is UMI3DCollaborationUser)
                UnityMainThreadDispatcher.Instance().Enqueue(UpdateUser(user));
        }

        private IEnumerator AddUserOnJoin(UMI3DCollaborationAbstractContentUser user)
        {
            yield return new WaitForFixedUpdate();

            if (user is UMI3DCollaborationUser)
                objectUserList.Add(user);

            SetEntityProperty op = objectUserList.GetSetEntityOperationForUsers(u => u.hasJoined);

            var tr = new Transaction() { reliable = true };
            tr.AddIfNotNull(op);
            UMI3DServer.Dispatch(tr);
        }

        private IEnumerator RemoveUserOnLeave(UMI3DCollaborationAbstractContentUser user)
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

        private IEnumerator UpdateUser(UMI3DCollaborationAbstractContentUser user)
        {
            yield return new WaitForFixedUpdate();

            Transaction transaction = new Transaction() { reliable = true };

            var operation = objectUserList.GetSetEntityOperationForUsers(u => u.hasJoined);

            transaction.AddIfNotNull(operation);
            transaction.Dispatch();
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

        public void CollaborationRequest(UMI3DUser user, ConferenceBrowserRequestDto dto)
        {
            var tr = new Transaction
            {
                reliable = true
            };
            switch (dto.operation)
            {
                case UMI3DOperationKeys.UserMicrophoneStatus:
                    if (users.ContainsKey(dto.id) && (!dto.value || (user.Id() == dto.id)))
                        tr.AddIfNotNull((users[dto.id] as UMI3DCollaborationUser)?.microphoneStatus.SetValue(dto.value));

                    if (tr.Count() > 0 && user.Id() != dto.id)
                    {
                        tr.AddIfNotNull(GetMuteNotification(user).GetLoadEntity(new HashSet<UMI3DUser> { users[dto.id] }));
                    }
                    break;

                case UMI3DOperationKeys.UserAvatarStatus:
                    if (users.ContainsKey(dto.id) && (!dto.value || (user.Id() == dto.id)))
                        tr.AddIfNotNull((users[dto.id] as UMI3DCollaborationUser)?.avatarStatus.SetValue(dto.value));
                    break;

                case UMI3DOperationKeys.UserAttentionStatus:
                    if (users.ContainsKey(dto.id) && user.Id() == dto.id)
                        tr.AddIfNotNull((users[dto.id] as UMI3DCollaborationUser)?.attentionRequired.SetValue(dto.value));
                    break;

                case UMI3DOperationKeys.MuteAllMicrophoneStatus:
                    tr.AddIfNotNull(MuteAll(user));
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
                    id = UMI3DSerializer.Read<ulong>(container);
                    value = UMI3DSerializer.Read<bool>(container);

                    if (users.ContainsKey(id) && (!value || (user.Id() == id)))
                        tr.AddIfNotNull((users[id] as UMI3DCollaborationUser)?.microphoneStatus.SetValue(value));

                    if (tr.Count() > 0 && user.Id() != id)
                    {
                        tr.AddIfNotNull(GetMuteNotification(user).GetLoadEntity(new HashSet<UMI3DUser> { users[id] }));
                    }
                    break;

                case UMI3DOperationKeys.UserAvatarStatus:
                    id = UMI3DSerializer.Read<ulong>(container);
                    value = UMI3DSerializer.Read<bool>(container);
                    if (users.ContainsKey(id) && (!value || (user.Id() == id)))
                        tr.AddIfNotNull((users[id] as UMI3DCollaborationUser)?.avatarStatus.SetValue(value));
                    break;

                case UMI3DOperationKeys.UserAttentionStatus:
                    id = UMI3DSerializer.Read<ulong>(container);
                    value = UMI3DSerializer.Read<bool>(container);
                    if (users.ContainsKey(id) && id == user.Id())
                        tr.AddIfNotNull((users[id] as UMI3DCollaborationUser)?.attentionRequired.SetValue(value));
                    break;

                case UMI3DOperationKeys.MuteAllMicrophoneStatus:
                    tr.AddIfNotNull(MuteAll(user));
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

        /// <summary>
        /// Mute everyone except <see cref="user"/>
        /// </summary>
        /// <param name="user">User who muted everyone</param>
        /// <returns></returns>
        private List<Operation> MuteAll(UMI3DUser user)
        {
            List<Operation> ops = new List<Operation>();
            HashSet<UMI3DUser> usersMuted = new HashSet<UMI3DUser>();

            foreach (UMI3DCollaborationUser u in users.Values)
            {
                if (u != user && u.microphoneStatus.GetValue())
                {
                    ops.Add(u.microphoneStatus.SetValue(false));
                    usersMuted.Add(u);
                }
            }

            ops.Add(GetMuteNotification(user).GetLoadEntity(usersMuted));

            return ops;
        }

        /// <summary>
        /// Returns a notification to display to users they were muted by <paramref name="user"/>.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private UMI3DNotification GetMuteNotification(UMI3DUser user)
        {
            UMI3DNotification notif;

            if (user is UMI3DCollaborationUser collabUser)
                notif = new UMI3DNotification(NotificationPriority.Low, "Microphone",
                    "You were muted by " + (string.IsNullOrEmpty(collabUser.login) ? "user " + collabUser.Id() : collabUser.login), 2.5f, null, null);
            else
                notif = new UMI3DNotification(NotificationPriority.Low, "Microphone", "You were muted.", 2.5f, null, null);

            return notif;
        }

        public void HandleUserActionRequest(UMI3DCollaborationAbstractContentUser user, UserActionRequestDto userActionRequest)
        {
            if (user is UMI3DCollaborationUser cUser)
                OnActionClicked?.Invoke(cUser, cUser.userActions.GetValue(user)?.FirstOrDefault(a => a.id == userActionRequest.actionId));
            else
                UnityEngine.Debug.Log($"User action not found {userActionRequest.environmentId} {userActionRequest.actionId}");
        }

    }
}