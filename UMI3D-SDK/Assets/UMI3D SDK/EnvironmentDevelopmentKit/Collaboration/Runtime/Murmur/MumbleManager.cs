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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using umi3d.common;

namespace umi3d.edk.collaboration.murmur
{
    public class MumbleManager
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.Mumble;

        public readonly string ip;
        public readonly string httpIp;
        private MurmurAPI m;
        private MurmurAPI.Server serv;
        private readonly string guid;
        private Regex roomRegex;
        private Regex userRegex;

        int localRoomIndex = 0;

        public string GetGUID() => guid;

        class Room
        {
            public int roomId;
            public int id;
            public string name;

            public Room(int roomId, int id, string name) : this(roomId, name)
            {
                this.id = id;
            }

            public Room(int roomId, string name)
            {
                this.roomId = roomId;
                this.name = name;
            }
        }
        public class User
        {
            public int id;
            public string userId;
            public string login;
            public string password;

            public User(int id, string userId, string login, string password) : this(id, userId, login)
            {
                this.password = password;
            }

            public User(int id, string userId, string login) : this(userId, login)
            {
                this.id = id;
            }

            public User(string userId, string login)
            {

                this.login = login;
                this.userId = userId;
            }
        }

        List<Room> roomList;
        Room defaultRoom;
        public List<User> userList;

        bool refreshing = false;
        bool running = false;
        float RefreshTime = 0;
        const float MaxRefreshTimeSecond = 30f;

        public static MumbleManager Create(string ip, string http = null, string guid = null)
        {
            if (string.IsNullOrEmpty(ip))
                return null;
            if (string.IsNullOrEmpty(guid))
                guid = System.Guid.NewGuid().ToString();

            var mm = new MumbleManager(ip, http, guid);
            mm._Create();
            mm.HeartBeat();
            QuittingManager.OnApplicationIsQuitting.AddListener(mm.Delete);
            return mm;
        }

        async void HeartBeat()
        {
            running = true;
            while (running)
            {
                await UMI3DAsyncManager.Yield();
                if (RefreshTime - UnityEngine.Time.time > 0)
                    continue;
                await Refresh();
            }
        }


        async Task WaitWhileRefreshing()
        {
            while (refreshing)
                await UMI3DAsyncManager.Yield();
        }

        void _Create()
        {
            RefreshAsync();
            defaultRoom = _CreateRoom();
        }


        public void SwitchDefaultRoom(string name, IEnumerable<UMI3DCollaborationAbstractUser> users, bool force = false)
        {
            if(name == null)
                name = roomList.FirstOrDefault()?.name;
            
            var room = roomList.FirstOrDefault(r => r.name == name) ?? _CreateRoom(name);
            if (room == null)
                return;

            var old = defaultRoom;
            defaultRoom = room;
            if (old == defaultRoom)
                return;

            foreach(var user in users)
                if (force || user.audioChannel.GetValue(user) == old.name)
                    SwitchUserRoom(user); 

        }

        public async void RefreshAsync()
        {
            if (!refreshing)
                await Refresh();
        }

        private MumbleManager(string ip, string http, string guid = null)
        {
            this.guid = guid;
            this.ip = ip;
            this.httpIp = (string.IsNullOrEmpty(http)) ? ip.Split(':')[0] : http;
            m = new MurmurAPI(httpIp);
            roomList = new List<Room>();
            userList = new List<User>();
            roomRegex = new Regex(@"Room([0-9]*)_\[" + guid + @"\]");
            userRegex = new Regex(@"User((.*))_\[" + guid + @"\]");
        }

        public string GenerateUserName(UMI3DCollaborationUser user, string userID)
        {
            return RemoveSpace(@"User_" + user.displayName + "_" + userID + @"_[" + guid + @"]");
        }

        public string RemoveSpace(string value)
        {
            return new string(value.Where(c => !Char.IsWhiteSpace(c)).ToArray());
        }

        private string GenerateRoomName(int i)
        {
            return RemoveSpace(@"Room" + i.ToString() + @"_[" + guid + @"]");
        }

        public async Task Refresh()
        {
            if (refreshing)
            {
                while (refreshing)
                    await UMI3DAsyncManager.Yield();
                return;
            }

            refreshing = true;
            RefreshTime = UnityEngine.Time.time + MaxRefreshTimeSecond;
            await ForceRefresh();
            RefreshTime = UnityEngine.Time.time + MaxRefreshTimeSecond;
            refreshing = false;
        }

        private async Task ForceRefresh()
        {
            try
            {
                if (serv == null)
                    serv = await MurmurAPI.Server.Create(m, 1);
                else
                    await serv.Refresh();
                CheckRoom();
                CheckUser();
            }
            catch (Exception e)
            {
                UMI3DLogger.LogError($"Error in mumble server refreshing [will try again in 1min] {e.Message} \n {e.StackTrace}", scope);
                UMI3DLogger.LogException(e, scope);
                await UMI3DAsyncManager.Delay(60000);
                await ForceRefresh();
            }
        }

        private void CheckUser()
        {
            List<User> toAdd = new List<User>(this.userList);
            List<User> toDelete = new List<User>();

            foreach (var user in serv.RegisteredUsers)
            {
                var match = userRegex.Match(user.name);
                if (match.Success)
                {

                    var lr = toAdd.FirstOrDefault(r => r.login == user.name);
                    if (lr != null)
                    {
                        toAdd.Remove(lr);
                        lr.id = user.id;
                    }
                    else
                    {
                        var id = match.Groups[1].Captures[0].Value;
                        toDelete.Add(new User(user.id, id, user.name));
                    }
                }
            }
            foreach (var user in toAdd)
                CreateUser(user);
            foreach (var user in toDelete)
                DeleteUser(user);
        }

        private void CheckRoom()
        {
            List<Room> toAdd = new List<Room>(this.roomList);
            List<Room> toDelete = new List<Room>();

            foreach (var room in serv.Channels)
            {
                var match = roomRegex.Match(room.data.name);
                if (match.Success)
                {
                    var lr = toAdd.FirstOrDefault(r => r.name == room.data.name);
                    if (lr != null)
                    {
                        toAdd.Remove(lr);
                        lr.id = room.data.id;
                    }
                    else
                    {
                        var id = int.Parse(match.Groups[1].Captures[0].Value);
                        toDelete.Add(new Room(id, room.data.id, room.data.name));
                    }
                }
            }

            foreach (var room in toAdd)
                CreateRoom(room);
            foreach (var room in toDelete)
                DeleteRoom(room);
        }

        private async void CreateRoom(Room room)
        {
            await WaitWhileRefreshing();
            try
            {
                MurmurAPI.Server.Channel c = await serv.CreateChannel(room.name);
                room.id = (c.data.id);
            }
            catch (Exception e)
            {
                UMI3DLogger.LogError($"Error in mumble create room", scope);
                UMI3DLogger.LogException(e, scope);
                await UMI3DAsyncManager.Delay(500);
                RefreshAsync();
            }
        }

        private async void DeleteRoom(Room room)
        {
            await WaitWhileRefreshing();
            try
            {
                await (serv.Channels.FirstOrDefault(c => c.data.id == room.id)?.DeleteChannel() ?? Task.CompletedTask);
            }

            catch (Exception e)
            {
                UMI3DLogger.LogError($"Error in mumble delete room", scope);
                UMI3DLogger.LogException(e, scope);
                await UMI3DAsyncManager.Delay(500);
                RefreshAsync();
            }
        }

        public async void CreateUser(User user)
        {
            await WaitWhileRefreshing();
            try
            {
                MurmurAPI.Server.User c = await serv.AddUser(user.login, user.password);
                user.id = (c.id);
            }
            catch (Exception e)
            {
                UMI3DLogger.LogError($"Error in mumble create user", scope);
                UMI3DLogger.LogException(e, scope);
                await UMI3DAsyncManager.Delay(500);
                RefreshAsync();
            }
        }

        private async void DeleteUser(User user)
        {
            await WaitWhileRefreshing();
            try
            {
                await serv.RemoveUser(user.id);
            }
            catch (Exception e)
            {
                UMI3DLogger.LogError($"Error in mumble delete user", scope);
                UMI3DLogger.LogException(e, scope);
                await UMI3DAsyncManager.Delay(500);
                RefreshAsync();
            }
        }

        private Room _CreateRoom(string name = null)
        {
            var roomId = localRoomIndex++;
            name = name ?? GenerateRoomName(roomId);
            var room = new Room(roomId, name);
            roomList.Add(room);
            CreateRoom(room);
            return room;
        }

        public int CreateRoom()
        {
            var room = _CreateRoom();
            return room.roomId;
        }

        public List<int> CreateRoom(int count)
        {
            var roomIds = new List<int>();
            for (int i = 0; i < count; i++)
            {
                roomIds.Add(CreateRoom());
            }
            return roomIds;
        }

        public List<int> GetRooms()
        {
            return roomList.Select(r => r.roomId).ToList();
        }

        public void DeleteRoom(int roomId)
        {
            var room = roomList.FirstOrDefault(r => r.roomId == roomId);
            if (room != null)
            {
                roomList.Remove(room);
                DeleteRoom(room);
            }
        }

        public void DeleteRoom(List<int> rooms)
        {
            foreach (var room in rooms)
                DeleteRoom(room);
        }


        public List<Operation> AddUser(UMI3DServerUser user, int room = -1)
        {
            var ops = new List<Operation>();
            ops.Add(user.audioServerUrl.SetValue(ip));
            ops.Add(user.audioUseMumble.SetValue(true));
            ops.AddRange(SwitchUserRoom(user, room));

            return ops;
        }

        public List<Operation> AddUser(UMI3DCollaborationUser user, int room = -1)
        {
            var userId = System.Guid.NewGuid().ToString();
            var _user = new User(userId, GenerateUserName(user, userId));
            _user.password = System.Guid.NewGuid().ToString();
            userList.Add(_user);

            CreateUser(_user);
            var ops = new List<Operation>();

            ops.Add(user.audioLogin.SetValue(_user.login));
            ops.Add(ToPrivate(user, user.audioPassword.SetValue(_user.password)));
            ops.Add(user.audioServerUrl.SetValue(ip));
            ops.Add(user.audioUseMumble.SetValue(true));
            ops.AddRange(SwitchUserRoom(user, room));

            return ops;
        }

        public Operation ToPrivate(UMI3DCollaborationUser user, Operation op)
        {
            if (op == null) return null;

            op.users = new HashSet<UMI3DUser>() { user };
            return op;
        }

        public List<Operation> RemoveUser(UMI3DCollaborationUser user)
        {
            var ops = new List<Operation>();
            var _user = userList.FirstOrDefault(us => us.login == user.audioLogin.GetValue());
            if (_user != null)
            {
                userList.Remove(_user);
                DeleteUser(_user);
            }

            ops.Add(user.audioLogin.SetValue(""));
            ops.Add(ToPrivate(user, user.audioPassword.SetValue("")));
            ops.Add(user.audioUseMumble.SetValue(false));
            ops.Add(user.audioServerUrl.SetValue(""));
            ops.Add(user.audioChannel.SetValue(""));

            return ops;
        }


        public List<Operation> SwitchUserRoom(UMI3DCollaborationAbstractUser user, int roomId = -1)
        {
            var ops = new List<Operation>();

            var room = roomList.FirstOrDefault(r => r.roomId == roomId) ?? defaultRoom;
            ops.Add(user.audioUseMumble.SetValue(true));
            ops.Add(user.audioServerUrl.SetValue(ip));
            ops.Add(user.audioChannel.SetValue(room.name));

            return ops;
        }

        public void Delete()
        {
            running = false;
            foreach (var room in roomList)
            {
                DeleteRoom(room);
            }
            foreach (var user in userList)
            {
                DeleteUser(user);
            }
            roomList.Clear();
            userList.Clear();
        }

        public string GetDefaultRoomName()
        {
            return defaultRoom.name;
        }
    }
}