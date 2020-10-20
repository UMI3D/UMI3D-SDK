﻿/*
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class UMI3DCollaborationEnvironmentLoader : UMI3DEnvironmentLoader
    {

        public static new UMI3DCollaborationEnvironmentLoader Instance { get => UMI3DEnvironmentLoader.Instance as UMI3DCollaborationEnvironmentLoader; set => UMI3DEnvironmentLoader.Instance = value; }

        public List<UMI3DUser> UserList;

        public override void ReadUMI3DExtension(GlTFEnvironmentDto _dto, GameObject node)
        {
            base.ReadUMI3DExtension(_dto, node);
            var dto = (_dto?.extensions as GlTFEnvironmentExtensions)?.umi3d as UMI3DCollaborationEnvironmentDto;
            if (dto == null) return;
            UserList = dto.userList.Select(u=>new UMI3DUser(u)).ToList();
        }

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

        /// <summary>
        /// Update Userlist
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        bool SetUserList(UMI3DCollaborationEnvironmentDto dto, SetEntityPropertyDto property)
        {
            switch (property)
            {
                case SetEntityListAddPropertyDto add:
                    var user = add.value as UserDto;
                    UserList.Add(new UMI3DUser(user));
                    dto.userList.Add(user);
                    break;
                case SetEntityListRemovePropertyDto rem:
                    UserList.RemoveAt(rem.index);
                    dto.userList.RemoveAt(rem.index);
                    break;
                case SetEntityListPropertyDto set:
                    var user2 = set.value as UserDto;
                    if (UserList.Count < set.index)
                    {
                        UserList[set.index].Update(user2);
                        dto.userList[set.index] = user2;
                    }
                    else if(UserList.Count == set.index)
                    {
                        UserList.Add(new UMI3DUser(user2));
                        dto.userList.Add(user2);
                    }
                    break;
                default:
                    dto.userList = property.value as List<UserDto>;
                    UserList = dto.userList.Select(u => new UMI3DUser(u)).ToList();
                    break;
            }
            return true;
        }

    }
}