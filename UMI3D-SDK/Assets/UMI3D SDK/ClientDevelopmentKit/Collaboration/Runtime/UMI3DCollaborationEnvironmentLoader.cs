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

using System;
using System.Collections.Generic;
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

        public IReadOnlyList<UMI3DUser> UserList => userList;
        private List<UMI3DUser> userList = new();

        public event Action OnUpdateUserList;

        public IReadOnlyList<UMI3DUser> JoinnedUserList => UserList.Where(u => u.status >= StatusType.AWAY || (UMI3DCollaborationClientServer.Exists && u.id == UMI3DCollaborationClientServer.Instance.GetUserId())).ToList();
        public event Action OnUpdateJoinnedUserList;

        private ulong lastTimeUserMessageListReceived = 0;

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
        public override async Task ReadUMI3DExtension(GlTFEnvironmentDto _dto, GameObject node)
        {
            await base.ReadUMI3DExtension(_dto, node);

            var dto = (_dto?.extensions)?.umi3d as UMI3DCollaborationEnvironmentDto;
            if (dto == null) 
                return;

            userList = dto.userList.Select(u => new UMI3DUser(u)).ToList();

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
                    StartAnim(user.onStartSpeakingAnimationId);
            }
            else
            {
                if (user != null && user.onStopSpeakingAnimationId != 0)
                    StartAnim(user.onStopSpeakingAnimationId);
            }
        }

        private async void StartAnim(ulong id)
        {
            var anim = UMI3DAbstractAnimation.Get(id);

            if (anim != null)
            {
                await anim.SetUMI3DProperty(
                    new SetUMI3DPropertyData(
                        new SetEntityPropertyDto()
                        {
                            entityId = id,
                            property = UMI3DPropertyKeys.AnimationPlaying,
                            value = true
                        },
                        GetEntity(id)
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
                    return SetUserList(dto, data.property);

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
                    return SetUserList(dto, data.operationId, data.propertyKey, data.container);

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
        private bool SetUserList(UMI3DCollaborationEnvironmentDto dto, SetEntityPropertyDto property)
        {
            if (dto == null) return false;

            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    InsertUser(dto, add.index, add.value as UserDto);
                    break;
                case SetEntityListRemovePropertyDto rem:
                    RemoveUserAt(dto, rem.index);
                    break;
                case SetEntityListPropertyDto set:
                    ReplaceUser(dto, set.index, set.value as UserDto);
                    break;
                default:
                    ReplaceAllUser(dto, property.value as List<UserDto>);
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
        private bool SetUserList(UMI3DCollaborationEnvironmentDto dto, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (dto == null) return false;

            if (lastTimeUserMessageListReceived < container.timeStep)
            {
                lastTimeUserMessageListReceived = container.timeStep;
            }
            else
            {
                return true;
            }

            switch (operationId)
            {
                case UMI3DOperationKeys.SetEntityListAddProperty:
                    InsertUser(dto, UMI3DSerializer.Read<int>(container), UMI3DSerializer.Read<UserDto>(container));
                    break;
                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    RemoveUserAt(dto, UMI3DSerializer.Read<int>(container));
                    break;
                case UMI3DOperationKeys.SetEntityListProperty:
                    ReplaceUser(dto, UMI3DSerializer.Read<int>(container), UMI3DSerializer.Read<UserDto>(container));
                    break;
                default:
                    ReplaceAllUser(dto, UMI3DSerializer.ReadList<UserDto>(container));
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



        private void InsertUser(UMI3DCollaborationEnvironmentDto dto, int index, UserDto userDto)
        {
            if (userList.Exists((user) => user.id == userDto.id)) return;

            if (index >= 0 && index <= UserList.Count)
            {
                userList.Insert(index, new UMI3DUser(userDto));
                OnUpdateUserList?.Invoke();
                OnUpdateJoinnedUserList?.Invoke();
            }
            else
            {
                UMI3DLogger.LogWarning("Impossible to insert new user into user list, index out of range " + index, DebugScope.CDK);
            }
        }

        private void RemoveUserAt(UMI3DCollaborationEnvironmentDto dto, int index)
        {
            if (UserList.Count <= index) return;

            if (index >= 0 && index < UserList.Count)
            {
                UMI3DUser Olduser = UserList[index];
                userList.RemoveAt(index);
                Olduser.Destroy();
                OnUpdateUserList?.Invoke();
                OnUpdateJoinnedUserList?.Invoke();
            }
            else
            {
                UMI3DLogger.LogWarning("Impossible to remove user from user list, index out of range " + index, DebugScope.CDK);
            }
        }

        private void ReplaceUser(UMI3DCollaborationEnvironmentDto dto, int index, UserDto userNew)
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
            else if (index == UserList.Count) InsertUser(dto, index, userNew);
        }

        private void ReplaceAllUser(UMI3DCollaborationEnvironmentDto dto, List<UserDto> usersNew)
        {
            foreach (UMI3DUser user in UserList) user.Destroy();
            userList = usersNew.Select(u => new UMI3DUser(u)).ToList();
            OnUpdateUserList?.Invoke();
            OnUpdateJoinnedUserList?.Invoke();
        }

        protected override void InternalClear()
        {
            base.InternalClear();

            lastTimeUserMessageListReceived = 0;
        }
    }
}