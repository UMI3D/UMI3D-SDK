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

        ///<inheritdoc/>
        public override void ReadUMI3DExtension(GlTFEnvironmentDto _dto, GameObject node)
        {
            base.ReadUMI3DExtension(_dto, node);
            var dto = (_dto?.extensions as GlTFEnvironmentExtensions)?.umi3d as UMI3DCollaborationEnvironmentDto;
            if (dto == null) return;
            UserList = dto.userList.Select(u => new UMI3DUser(u)).ToList();
        }

        ///<inheritdoc/>
        protected override bool _SetUMI3DPorperty(UMI3DEntityInstance entity, SetEntityPropertyDto property)
        {
            if (base._SetUMI3DPorperty(entity, property)) return true;
            if (entity == null) return false;
            var dto = ((entity.dto as GlTFEnvironmentDto)?.extensions as GlTFEnvironmentExtensions)?.umi3d as UMI3DCollaborationEnvironmentDto;
            if (dto == null) return false;
            switch (property.property)
            {
                case UMI3DPropertyKeys.UserList:
                    return SetUserList(dto, property);
                default:
                    return false;
            }
        }

        protected override bool _SetUMI3DPorperty(UMI3DEntityInstance entity, uint operationId, uint propertyKey, ByteContainer container)
        {
            if (base._SetUMI3DPorperty(entity, operationId, propertyKey, container)) return true;
            if (entity == null) return false;
            var dto = ((entity.dto as GlTFEnvironmentDto)?.extensions as GlTFEnvironmentExtensions)?.umi3d as UMI3DCollaborationEnvironmentDto;
            if (dto == null) return false;
            switch (propertyKey)
            {
                case UMI3DPropertyKeys.UserList:
                    return SetUserList(dto, operationId, propertyKey, container);
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
            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    {
                        var user = add.value as UserDto;
                        var _user = new UMI3DUser(user);
                        UserList.Insert(add.index, _user);
                        dto.userList.Insert(add.index, user);
                        break;
                    }
                case SetEntityListRemovePropertyDto rem:
                    {
                        if (UserList.Count > rem.index)
                        {
                            UMI3DUser Olduser = UserList[rem.index];
                            UserList.RemoveAt(rem.index);
                            dto.userList.RemoveAt(rem.index);
                            Olduser.Destroy();
                        }
                        break;
                    }
                case SetEntityListPropertyDto set:
                    {
                        if (0 > set.index)
                            break;
                        var user2 = set.value as UserDto;
                        if (UserList.Count > set.index)
                        {
                            UserList[set.index].Update(user2);
                            dto.userList[set.index] = user2;
                        }
                        else if (UserList.Count == set.index)
                        {
                            var _user2 = new UMI3DUser(user2);
                            UserList.Add(_user2);
                            dto.userList.Add(user2);
                        }
                        break;
                    }
                default:
                    {
                        foreach (UMI3DUser user in UserList)
                            user.Destroy();
                        dto.userList = property.value as List<UserDto>;
                        UserList = dto.userList.Select(u => new UMI3DUser(u)).ToList();
                        break;
                    }
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
            int index;
            UserDto user;

            switch (operationId)
            {
                case UMI3DOperationKeys.SetEntityListAddProperty:
                    {
                        index = UMI3DNetworkingHelper.Read<int>(container);
                        user = UMI3DNetworkingHelper.Read<UserDto>(container);
                        var _user = new UMI3DUser(user);
                        UserList.Insert(index, _user);
                        dto.userList.Insert(index, user);
                        break;
                    }
                case UMI3DOperationKeys.SetEntityListRemoveProperty:
                    {
                        index = UMI3DNetworkingHelper.Read<int>(container);
                        if (UserList.Count > index)
                        {
                            UMI3DUser Olduser = UserList[index];
                            UserList.RemoveAt(index);
                            dto.userList.RemoveAt(index);
                            Olduser.Destroy();
                        }
                        break;
                    }
                case UMI3DOperationKeys.SetEntityListProperty:
                    {
                        index = UMI3DNetworkingHelper.Read<int>(container);
                        user = UMI3DNetworkingHelper.Read<UserDto>(container);
                        if (0 > index)
                            break;
                        if (UserList.Count > index)
                        {
                            UserList[index].Update(user);
                            dto.userList[index] = user;
                        }
                        else if (UserList.Count == index)
                        {
                            var _user = new UMI3DUser(user);
                            UserList.Add(_user);
                            dto.userList.Add(user);
                        }
                        break;
                    }
                default:
                    {
                        foreach (UMI3DUser ouser in UserList)
                            ouser.Destroy();
                        dto.userList = UMI3DNetworkingHelper.ReadList<UserDto>(container);
                        UserList = dto.userList.Select(u => new UMI3DUser(u)).ToList();
                        break;
                    }
            }
            return true;
        }
    }
}