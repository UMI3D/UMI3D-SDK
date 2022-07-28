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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using umi3d.edk.collaboration;

namespace umi3d.edk.collaboration.murmur
{
    public class MumbleManager
    {
        readonly public string ip;
        MurmurAPI m;
        MurmurAPI.Server serv;
        int lastRoomguid = 0;
        int defaultRoom;
        List<int> localRoom;

        public static async Task<MumbleManager> Create(string ip)
        {
            var mm = new MumbleManager(ip);
            mm.serv = await MurmurAPI.Server.Create(mm.m, 1);
            mm.localRoom = new List<int>();
            mm.defaultRoom = await mm.CreateRoom();
            return mm;
        }

        private MumbleManager(string ip)
        {
            this.ip = ip;
            m = new MurmurAPI(ip);
        }

        public async Task Refresh()
        {
            await serv.Refresh();
        }

        public async Task<int> CreateRoom()
        {
            var roomName = "Room" + lastRoomguid++;
            var c = await serv.CreateChannel(roomName);
            localRoom.Add(c.data.id);
            return c.data.id;
        }

        public async Task<List<int>> CreateRoom(int count)
        {
            List<int> roomIds = new List<int>();
            for(int i = 0; i < count; i++)
            {
                roomIds.Add(await CreateRoom());
            }
            return roomIds;
        }

        public List<int> GetRooms()
        {
            return serv.Channels.Select(c=>c.data.id).Where(c=> localRoom.Contains(c)).ToList();
        }
        public async Task DeleteRoom(int room)
        {
            await DeleteRoom(room,false);
        }

        private async Task DeleteRoom(int room, bool local)
        {
            try
            {
                if (!local && room == defaultRoom) return;
                localRoom.Remove(room);
                await serv.Channels.FirstOrDefault(c => c.data.id == room)?.DeleteChannel();
            }
            catch { };
        }

        public async Task DeleteRoom(List<int> rooms)
        {
            await Task.WhenAll(rooms.Select(async roomName => await DeleteRoom(roomName)));
        }

        public async Task<List<Operation>> AddUser(UMI3DCollaborationUser user,int room = -1)
        {
            if (room == -1)
                room = defaultRoom;
            var ops = new List<Operation>();
            
            ops.Add(ToPrivate(user, user.audioLogin.SetValue(user.guid)));
            ops.Add(ToPrivate(user, user.audioPassword.SetValue(System.Guid.NewGuid().ToString())));
            ops.Add(user.audioServerUrl.SetValue(ip));
            var ch =  serv.Channels.FirstOrDefault(c => c.data.id == room)?.data.name;
            ops.Add(user.audioChannel.SetValue(ch));

            await serv.AddUser(user.audioLogin.GetValue(), user.audioPassword.GetValue());

            return ops;
        }

        public async Task<List<Operation>> RemoveUser(UMI3DCollaborationUser user)
        {
            var ops = new List<Operation>();
            var u = serv.RegisteredUsers.FirstOrDefault(us => us.name == user.audioLogin.GetValue());
            if (u != null)
                await serv.RemoveUser(u.id);

            ops.Add(ToPrivate(user,user.audioLogin.SetValue("")));
            ops.Add(ToPrivate(user, user.audioPassword.SetValue("")));
            ops.Add(user.audioServerUrl.SetValue(""));
            ops.Add(user.audioChannel.SetValue(""));

            return ops;
        }

        Operation ToPrivate(UMI3DCollaborationUser user, Operation op)
        {
            if (op == null) return null;

            op.users = new HashSet<UMI3DUser>() { user };
            return op;
        }


        public List<Operation> SwitchUserRoom(UMI3DCollaborationUser user, int room = -1)
        {
            if (room == -1)
                room = defaultRoom;
            var ops = new List<Operation>();

            ops.Add(user.audioServerUrl.SetValue(ip));
            var ch = serv.Channels.FirstOrDefault(c => c.data.id == room)?.data.name ;
            ops.Add(user.audioChannel.SetValue(ch));

            return ops;
        }


        public async void Delete()
        {
            await Task.WhenAll(localRoom.Select(async (room) => await DeleteRoom(room, true)));
        }

    }
}