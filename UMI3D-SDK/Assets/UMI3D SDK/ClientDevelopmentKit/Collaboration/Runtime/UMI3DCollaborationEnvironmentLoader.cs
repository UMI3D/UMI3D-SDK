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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration.dto;
using umi3d.common.collaboration.dto.signaling;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Loader for <see cref="UMI3DCollaborationEnvironmentDto"/>.
    /// </summary>
    public class UMI3DCollaborationEnvironmentLoader : UMI3DEnvironmentLoader, ICollaborationEnvironmentManager
    {
        public static new UMI3DCollaborationEnvironmentLoader Instance
        {
            get
            {
                if (ApplicationIsQuitting)
                    return null;
                if (!Exists)
                    instance = new UMI3DCollaborationEnvironmentLoader();
                if (UMI3DEnvironmentLoader.Instance is UMI3DCollaborationEnvironmentLoader collabEnvironmentLoader)
                    return collabEnvironmentLoader;
                else
                    throw new Umi3dException("EnvironmentLoader instance is no UMI3DCollaborationEnvironmentLoader");
            }
        }

        public IReadOnlyList<UMI3DUser> UserList => userList.SelectMany(kp => kp.Value).ToList();
        private Dictionary<ulong,List<UMI3DUser>> userList = new();

        public event Action OnUpdateUserList;

        public IReadOnlyList<UMI3DUser> JoinnedUserList => UserList.Where(u => u.status >= StatusType.AWAY || (UMI3DCollaborationClientServer.Exists && u.id == UMI3DCollaborationClientServer.Instance.GetUserId())).ToList();
        public event Action OnUpdateJoinnedUserList;

        private Dictionary<ulong,ulong> lastTimeUserMessageListReceived = new();

        public UMI3DUser GetClientUser()
        {
            return UserList.FirstOrDefault(u => UMI3DCollaborationClientServer.Exists && u.id == UMI3DCollaborationClientServer.Instance.GetUserId());
        }

        private UMI3DUser GetUser(UserDto dto)
        {
            return UserList.FirstOrDefault(u => u.id == dto.id);
        }

        /// <inheritdoc/>
        protected override async Task WaitForFirstTransaction()
        {
            while (UMI3DCollaborationClientServer.transactionPending != null
                && (UMI3DCollaborationClientServer.transactionPending.areTransactionPending))
                await UMI3DAsyncManager.Yield();
        }

        ///<inheritdoc/>
        public override async Task ReadUMI3DExtension(ulong environmentId, GlTFEnvironmentDto _dto, GameObject node)
        {
            await base.ReadUMI3DExtension(environmentId, _dto, node);

            var dto = (_dto?.extensions)?.umi3d as UMI3DCollaborationEnvironmentDto;
            if (dto == null)
                return;

            if (userList != null)
            {
                if (userList.ContainsKey(environmentId))
                {
                    userList[environmentId].ForEach(u => { if (u != null) DeleteEntityInstance(u.EnvironmentId, u.id); });
                    userList.Remove(environmentId);
                }
            }
            else
                userList = new();

            var users = dto.userList.Select(u => (dto: u, entity: new UMI3DUser(environmentId, u)));

            userList[environmentId] = users.Select(u => u.entity).ToList();
            users.ForEach(u =>
            {
                UMI3DEnvironmentLoader.Instance.RegisterEntity(UMI3DGlobalID.EnvironmentId, u.entity.id, u.dto, u.entity, () => { UMI3DUser.OnRemoveUser.Invoke(u.entity); }).NotifyLoaded();
                UMI3DUser.OnNewUser.Invoke(u.entity);
            });
            OnUpdateUserList?.Invoke();
            OnUpdateJoinnedUserList?.Invoke();

            AudioManager.Instance.OnUserSpeaking.AddListener(OnUserSpeaking);
        }

        /// <summary>
        /// Called when a user starts to speak.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isSpeaking"></param>
        void OnUserSpeaking(UMI3DUser user, bool isSpeaking)
        {
            if (isSpeaking)
            {
                if (user != null && user.onStartSpeakingAnimationId != 0)
                    StartAnim(UMI3DGlobalID.EnvironmentId,user.onStartSpeakingAnimationId);
            }
            else
            {
                if (user != null && user.onStopSpeakingAnimationId != 0)
                    StartAnim(UMI3DGlobalID.EnvironmentId,user.onStopSpeakingAnimationId);
            }
        }

        private async void StartAnim(ulong environmentId,ulong id)
        {
            var anim = UMI3DAbstractAnimation.Get(environmentId, id);

            if (anim != null)
            {
                await anim.SetUMI3DProperty(
                    new SetUMI3DPropertyData(
                        environmentId,
                        new SetEntityPropertyDto()
                        {
                            entityId = id,
                            property = UMI3DPropertyKeys.AnimationPlaying,
                            value = true
                        },
                        GetEntityInstance(environmentId,id)
                    )
                );
                anim.Start();
            }
        }


        /// <inheritdoc/>
        protected override async Task<bool> _SetUMI3DProperty(SetUMI3DPropertyData data)
        {
            if (await base._SetUMI3DProperty(data)) return true;
            if (data.entity == null) return false;

            switch (data.property.property)
            {
                case UMI3DPropertyKeys.UserList:
                    var dto = ((data.entity.dto as GlTFEnvironmentDto)?.extensions)?.umi3d as UMI3DCollaborationEnvironmentDto;
                    return SetUserList(data.environmentId, dto, data.property);

                case UMI3DPropertyKeys.UserMicrophoneStatus:
                case UMI3DPropertyKeys.UserAttentionRequired:
                case UMI3DPropertyKeys.UserAvatarStatus:
                case UMI3DPropertyKeys.UserAudioFrequency:
                case UMI3DPropertyKeys.UserAudioLogin:
                case UMI3DPropertyKeys.UserAudioPassword:
                case UMI3DPropertyKeys.UserAudioServer:
                case UMI3DPropertyKeys.UserAudioUseMumble:
                case UMI3DPropertyKeys.UserAudioChannel:
                    return UpdateUser(data.property.property, data.entity, data.property.value);
                case UMI3DPropertyKeys.UserOnStartSpeakingAnimationId:
                case UMI3DPropertyKeys.UserOnStopSpeakingAnimationId:
                    return UpdateUser(data.property.property, data.entity, (ulong)(long)data.property.value);

                default:
                    return false;
            }
        }

        /// <inheritdoc/>
        protected override async Task<bool> _SetUMI3DProperty(SetUMI3DPropertyContainerData data)
        {
            if (await base._SetUMI3DProperty(data)) return true;
            if (data.entity == null) return false;
            switch (data.propertyKey)
            {
                case UMI3DPropertyKeys.UserList:
                    var dto = ((data.entity.dto as GlTFEnvironmentDto)?.extensions)?.umi3d as UMI3DCollaborationEnvironmentDto;
                    return SetUserList(data.environmentId, dto, data.operationId, data.propertyKey, data.container);

                case UMI3DPropertyKeys.UserMicrophoneStatus:
                case UMI3DPropertyKeys.UserAttentionRequired:
                case UMI3DPropertyKeys.UserAvatarStatus:
                case UMI3DPropertyKeys.UserAudioUseMumble:
                    {
                        bool value = UMI3DSerializer.Read<bool>(data.container);
                        return UpdateUser(data.propertyKey, data.entity, value);
                    }
                case UMI3DPropertyKeys.UserAudioFrequency:
                    {
                        int value = UMI3DSerializer.Read<int>(data.container);
                        return UpdateUser(data.propertyKey, data.entity, value);
                    }
                case UMI3DPropertyKeys.UserAudioLogin:
                case UMI3DPropertyKeys.UserAudioPassword:
                case UMI3DPropertyKeys.UserAudioServer:
                case UMI3DPropertyKeys.UserAudioChannel:
                    {
                        string value = UMI3DSerializer.Read<string>(data.container);
                        return UpdateUser(data.propertyKey, data.entity, value);
                    }

                case UMI3DPropertyKeys.UserOnStartSpeakingAnimationId:
                case UMI3DPropertyKeys.UserOnStopSpeakingAnimationId:
                    {
                        ulong value = UMI3DSerializer.Read<ulong>(data.container);
                        return UpdateUser(data.propertyKey, data.entity, value);
                    }
                default:
                    return false;
            }
        }


        /// <summary>
        /// Update Userlist
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool SetUserList(ulong environmentId, UMI3DCollaborationEnvironmentDto dto, SetEntityPropertyDto property)
        {
            if (dto == null) return false;

            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    InsertUser(environmentId, dto, add.index, add.value as UserDto);
                    break;
                case SetEntityListRemovePropertyDto rem:
                    RemoveUserAt(environmentId, dto, rem.index);
                    break;
                case SetEntityListPropertyDto set:
                    ReplaceUser(environmentId, dto, set.index, set.value as UserDto);
                    break;
                default:
                    ReplaceAllUser(environmentId, dto, property.value as List<UserDto>);
                    break;
            }
            return true;
        }

        /// <summary>
        /// Update Userlist
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool SetUserList(ulong environmentId, UMI3DCollaborationEnvironmentDto dto, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (dto == null) return false;

            if (!lastTimeUserMessageListReceived.ContainsKey(environmentId) || lastTimeUserMessageListReceived[environmentId] < container.timeStep)
            {
                lastTimeUserMessageListReceived[environmentId] = container.timeStep;
            }
            else
            {
                return true;
            }

            switch (operationId)
            {
                case UMI3DOperationKeys.SetEntityListAddProperty:
                    InsertUser(container.environmentId, dto, UMI3DSerializer.Read<int>(container), UMI3DSerializer.Read<UserDto>(container));
                    break;
                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    RemoveUserAt(container.environmentId, dto, UMI3DSerializer.Read<int>(container));
                    break;
                case UMI3DOperationKeys.SetEntityListProperty:
                    ReplaceUser(container.environmentId, dto, UMI3DSerializer.Read<int>(container), UMI3DSerializer.Read<UserDto>(container));
                    break;
                default:
                    ReplaceAllUser(container.environmentId, dto, UMI3DSerializer.ReadList<UserDto>(container));
                    break;
            }
            return true;
        }

        private bool UpdateUser(ulong property, UMI3DEntityInstance userInstance, object value)
        {
            if (!(userInstance.dto is UserDto dto)) return false;

            UMI3DUser user = GetUser(dto);
            return user?.UpdateUser(property, value) ?? false;
        }



        private void InsertUser(ulong environmentId,UMI3DCollaborationEnvironmentDto dto, int index, UserDto userDto)
        {
            if (!userList.ContainsKey(environmentId))
                userList[environmentId] = new();

            if (userList[environmentId].Exists((user) => user.id == userDto.id)) return;

            if (index >= 0 && index <= userList[environmentId].Count)
            {
                userList[environmentId].Insert(index, new UMI3DUser(environmentId, userDto));
                OnUpdateUserList?.Invoke();
                OnUpdateJoinnedUserList?.Invoke();
            }
            else
            {
                UMI3DLogger.LogWarning("Impossible to insert new user into user list, index out of range " + index, DebugScope.CDK);
            }
        }

        private void RemoveUserAt(ulong environmentId, UMI3DCollaborationEnvironmentDto dto, int index)
        {
            if (UserList.Count <= index) return;

            if (index >= 0 && index < UserList.Count)
            {
                UMI3DUser Olduser = UserList[index];
                userList[environmentId].RemoveAt(index);
                DeleteEntityInstance(Olduser.EnvironmentId, Olduser.id);
                OnUpdateUserList?.Invoke();
                OnUpdateJoinnedUserList?.Invoke();
            }
            else
            {
                UMI3DLogger.LogWarning("Impossible to remove user from user list, index out of range " + index, DebugScope.CDK);
            }
        }

        private void ReplaceUser(ulong environmentId, UMI3DCollaborationEnvironmentDto dto, int index, UserDto userNew)
        {
            if (index < 0 || index > UserList.Count) return;

            if (index < UserList.Count)
            {
                bool userWasReady = UserList[index].status < StatusType.AWAY;
                bool userIsReady = userNew.status < StatusType.AWAY;
                bool userReadyListUpdated = userWasReady != userIsReady;

                UserList[index].Update(userNew);
                dto.userList[index] = userNew;

                if (userReadyListUpdated)
                    OnUpdateJoinnedUserList?.Invoke();
            }
            else if (index == UserList.Count) InsertUser(environmentId, dto, index, userNew);
        }

        private void ReplaceAllUser(ulong environmentId, UMI3DCollaborationEnvironmentDto dto, List<UserDto> usersNew)
        {
            if (userList != null)
            {
                if (userList.ContainsKey(environmentId))
                {
                    userList[environmentId].ForEach(u => DeleteEntityInstance(u.EnvironmentId, u.id, null));
                    userList.Remove(environmentId);
                }
            }
            else
                userList = new();

            var users = usersNew.Select(u => (dto: u, entity: new UMI3DUser(environmentId, u)));
            userList[environmentId] = users.Select(u => u.entity).ToList();
            users.ForEach(u =>
            {
                UMI3DEnvironmentLoader.Instance.RegisterEntity(UMI3DGlobalID.EnvironmentId, u.entity.id, u.dto, u.entity, () => { UMI3DUser.OnRemoveUser.Invoke(u.entity); }).NotifyLoaded();
                UMI3DUser.OnNewUser.Invoke(u.entity);
            });

            OnUpdateUserList?.Invoke();
            OnUpdateJoinnedUserList?.Invoke();
        }

        protected override void InternalClear()
        {
            base.InternalClear();

            userList.SelectMany(u => u.Value).ForEach(u => { if (u != null) DeleteEntityInstance(u.EnvironmentId, u.id); });
            userList.Clear();

            lastTimeUserMessageListReceived.Clear();
        }
    }
}