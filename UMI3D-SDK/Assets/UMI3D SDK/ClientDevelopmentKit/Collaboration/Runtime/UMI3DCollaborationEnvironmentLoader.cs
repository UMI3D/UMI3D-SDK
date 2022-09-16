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
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    public class UMI3DCollaborationEnvironmentLoader : UMI3DEnvironmentLoader
    {
        public static new UMI3DCollaborationEnvironmentLoader Instance { get => UMI3DEnvironmentLoader.Instance as UMI3DCollaborationEnvironmentLoader; set => UMI3DEnvironmentLoader.Instance = value; }

        public List<UMI3DUser> UserList;
        public static event Action OnUpdateUserList;

        public List<UMI3DUser> JoinnedUserList => UserList.Where(u => u.status >= StatusType.AWAY || (UMI3DCollaborationClientServer.Exists && u.id == UMI3DCollaborationClientServer.Instance.GetUserId())).ToList();
        public static event Action OnUpdateJoinnedUserList;

        public UMI3DUser GetClientUser()
        {
            return UserList.FirstOrDefault(u => UMI3DCollaborationClientServer.Exists && u.id == UMI3DCollaborationClientServer.Instance.GetUserId());
        }

        private UMI3DUser GetUser(UserDto dto)
        {
            return UserList.FirstOrDefault(u => u.id == dto.id);
        }

        ///<inheritdoc/>
        public override void ReadUMI3DExtension(GlTFEnvironmentDto _dto, GameObject node)
        {
            base.ReadUMI3DExtension(_dto, node);
            var dto = (_dto?.extensions)?.umi3d as UMI3DCollaborationEnvironmentDto;
            if (dto == null) return;
            UserList = dto.userList.Select(u => new UMI3DUser(u)).ToList();
            OnUpdateUserList?.Invoke();
            OnUpdateJoinnedUserList?.Invoke();
            AudioManager.Instance.OnUserSpeaking.AddListener(OnUserSpeaking);
        }

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

        private void StartAnim(ulong id)
        {
            UMI3DAbstractAnimation.Get(id)?.Start();
        }


        ///<inheritdoc/>
        protected override bool _SetUMI3DPorperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base._SetUMI3DPorperty(entity, property)) return true;
            if (entity == null) return false;

            switch (property.property)
            {
                case UMI3DPropertyKeys.UserList:
                    var dto = ((entity.dto as GlTFEnvironmentDto)?.extensions)?.umi3d as UMI3DCollaborationEnvironmentDto;
                    return SetUserList(dto, property);

                case UMI3DPropertyKeys.UserMicrophoneStatus:
                case UMI3DPropertyKeys.UserAttentionRequired:
                case UMI3DPropertyKeys.UserAvatarStatus:
                case UMI3DPropertyKeys.UserAudioFrequency:
                case UMI3DPropertyKeys.UserAudioLogin:
                case UMI3DPropertyKeys.UserAudioPassword:
                case UMI3DPropertyKeys.UserAudioServer:
                case UMI3DPropertyKeys.UserAudioUseMumble:
                case UMI3DPropertyKeys.UserAudioChannel:
                    return UpdateUser(property.property, entity, property.value);

                default:
                    return false;
            }
        }

        protected override bool _SetUMI3DPorperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (base._SetUMI3DPorperty(entity, operationId, propertyKey, container)) return true;
            if (entity == null) return false;

            switch (propertyKey)
            {
                case UMI3DPropertyKeys.UserList:
                    var dto = ((entity.dto as GlTFEnvironmentDto)?.extensions)?.umi3d as UMI3DCollaborationEnvironmentDto;
                    return SetUserList(dto, operationId, propertyKey, container);

                case UMI3DPropertyKeys.UserMicrophoneStatus:
                case UMI3DPropertyKeys.UserAttentionRequired:
                case UMI3DPropertyKeys.UserAvatarStatus:
                case UMI3DPropertyKeys.UserAudioUseMumble:
                    {
                        bool value = UMI3DNetworkingHelper.Read<bool>(container);
                        return UpdateUser(propertyKey, entity, value);
                    }
                case UMI3DPropertyKeys.UserAudioFrequency:
                    {
                        int value = UMI3DNetworkingHelper.Read<int>(container);
                        return UpdateUser(propertyKey, entity, value);
                    }
                case UMI3DPropertyKeys.UserAudioLogin:
                case UMI3DPropertyKeys.UserAudioPassword:
                case UMI3DPropertyKeys.UserAudioServer:
                case UMI3DPropertyKeys.UserAudioChannel:
                    {
                        string value = UMI3DNetworkingHelper.Read<string>(container);
                        return UpdateUser(propertyKey, entity, value);
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
            switch (operationId)
            {
                case UMI3DOperationKeys.SetEntityListAddProperty:
                    InsertUser(dto, UMI3DNetworkingHelper.Read<int>(container), UMI3DNetworkingHelper.Read<UserDto>(container));
                    break;
                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    RemoveUserAt(dto, UMI3DNetworkingHelper.Read<int>(container));
                    break;
                case UMI3DOperationKeys.SetEntityListProperty:
                    ReplaceUser(dto, UMI3DNetworkingHelper.Read<int>(container), UMI3DNetworkingHelper.Read<UserDto>(container));
                    break;
                default:
                    ReplaceAllUser(dto, UMI3DNetworkingHelper.ReadList<UserDto>(container));
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
            if (UserList.Exists((user) => user.id == userDto.id)) return;
            UserList.Insert(index, new UMI3DUser(userDto));
            dto.userList.Insert(index, userDto);
            OnUpdateUserList?.Invoke();
            OnUpdateJoinnedUserList?.Invoke();
        }

        private void RemoveUserAt(UMI3DCollaborationEnvironmentDto dto, int index)
        {
            if (UserList.Count <= index) return;
            UMI3DUser Olduser = UserList[index];
            UserList.RemoveAt(index);
            dto.userList.RemoveAt(index);
            Olduser.Destroy();
            OnUpdateUserList?.Invoke();
            OnUpdateJoinnedUserList?.Invoke();
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
            dto.userList = usersNew;
            UserList = dto.userList.Select(u => new UMI3DUser(u)).ToList();
            OnUpdateUserList?.Invoke();
            OnUpdateJoinnedUserList?.Invoke();
        }
    }
}