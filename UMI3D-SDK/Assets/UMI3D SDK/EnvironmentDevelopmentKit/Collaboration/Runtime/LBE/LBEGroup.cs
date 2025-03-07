/*
Copyright 2019 - 2024 Inetum

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

using umi3d.common;
using umi3d.common.lbe.description;
using UnityEngine;
using umi3d.edk;
using System;
using System.Collections.Generic;

namespace umi3d.edk.collaboration
{
    public class LBEGroup
    {
        public Vector3 spawnPoint { get; private set; }

        public ulong groupId { get; private set; }

        public ulong leaderId { get; private set; }

        List<ulong> userIds = new List<ulong>();

        public LBEGroup(ulong newGroupId, Vector3 initSpawnPos)
        {
            groupId = newGroupId;
            spawnPoint = initSpawnPos;
        }     

        public void SetSpawnPoint(Vector3 position)
        {
            spawnPoint = position;
        }

        public void SetNewLeader(ulong userId)
        {
            leaderId = userId;
        }

        public bool TryAddUser(ulong userId)
        {
            //naive
            this.userIds.Add(userId);

            return true;
        }

        public bool TryRemoveUser(ulong userId)
        {
            int i;
            if ((i = userIds.FindIndex(id => id == userId)) != -1)
            {
                userIds.RemoveAt(i);

                if (userId == leaderId)
                    leaderId = 0;

                return true;
            }
            return false;
        }

        public bool ContainsUser(ulong userId)
        {
            return userIds.Contains(userId);
        }
    }
}
