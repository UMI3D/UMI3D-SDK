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

using inetum.unityUtils.lifeCycle;
using inetum.unityUtils.observation;
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

        private Regex generalRoomRegex;
        private Regex userRegex;

        int localRoomIndex = 0;

        public string GetGUID() => guid;

        public enum Permissions
        {
            /** Write access to channel control. Implies all other s (except Speak). */
            Write = 0x01,
            /** Traverse channel. Without this, a client cannot reach subchannels, no matter which privileges he has there. */
            Traverse = 0x02,
            /** Enter channel. */
            Enter = 0x04,
            /** Speak in channel. */
            Speak = 0x08,
            /** Listen in channel. */
            Listen = 0x800,
            /** Whisper to channel. This is different from Speak, so you can set up different s. */
            Whisper = 0x100,
            /** Mute and deafen other users in this channel. */
            MuteDeafen = 0x10,
            /** Move users from channel. You need this  in both the source and destination channel to move another user. */
            Move = 0x20,
            /** Make new channel as a subchannel of this channel. */
            MakeChannel = 0x40,
            /** Make new temporary channel as a subchannel of this channel. */
            MakeTempChannel = 0x400,
            /** Link this channel. You need this  in both the source and destination channel to link channels, or in either channel to unlink them. */
            LinkChannel = 0x80,
            /** Send text message to channel. */
            TextMessage = 0x200,
            /** Kick user from server. Only valid on root channel. */
            Kick = 0x10000,
            /** Ban user from server. Only valid on root channel. */
            Ban = 0x20000,
            /** Register and unregister users. Only valid on root channel. */
            Register = 0x40000,
            /** Register and unregister users. Only valid on root channel. */
            RegisterSelf = 0x80000
        }

        class Room
        {
            public bool root;
            public bool localRoom;
            public int id;
            public string name;
            public List<Room> children = new();

            public Room(int id, string name, bool localRoom)
            {
                this.id = id;
                this.name = name;
                this.localRoom = localRoom;
            }

            public override string ToString() => $"[AudioRoom roomId {id}; name {name}; local : {localRoom}]";
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
        Room rootRoom;
        Room defaultRoom;
        public List<User> userList;

        bool refreshing = false;
        bool running = false;
        float RefreshTime = 0;
        const float MaxRefreshTimeSecond = 30f;

        public event Action<int> OnRoomCreated, OnRoomDeleted;

        public static MumbleManager Create(string ip, string http = null, string guid = null)
        {
            if (string.IsNullOrEmpty(ip))
                return null;
            if (string.IsNullOrEmpty(guid))
                guid = System.Guid.NewGuid().ToString();

            var mm = new MumbleManager(ip, http, guid);
            mm.Init();
            mm.HeartBeat();

            Quitting.instance.SubscribeFor(
                Quitting.SubscriptionType.IsQuitting,
                typeof(MumbleManager).FullName,
                (Callback)mm.Delete
            );

            return mm;
        }

        /// <summary>
        /// Add a header that will be send on each MurmurApi Rest call
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool AddHeader(string key, string value)
        {
            return m.AddHeader(key, value);
        }

        /// <summary>
        /// Update a header that will be send on each MurmurApi Rest call
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void UpdateHeader(string key, string value)
        {
            m.UpdateHeader(key, value);
        }

        /// <summary>
        /// Remove a header that will be send on each MurmurApi Rest call
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool RemoveHeader(string key)
        {
            return m.RemoveHeader(key);
        }

        async void HeartBeat()
        {
            running = true;
            while (running)
            {
                try
                {
                    await UMI3DAsyncManager.Yield();
                    if (RefreshTime - UnityEngine.Time.time > 0)
                        continue;
                    await Refresh();
                }
                catch (Exception e)
                {
                    await UMI3DAsyncManager.Yield();
                    UnityEngine.Debug.LogError(e);
                }
            }
        }

        async Task WaitWhileRefreshing()
        {
            while (refreshing)
                await UMI3DAsyncManager.Yield();
        }

        async void Init()
        {
            RefreshAsync();

            try
            {
                rootRoom = await CreateRoomInternalAsync(null, true, true);
                defaultRoom = await CreateRoomInternalAsync();
            }
            catch (Exception ex)
            {
                UMI3DLogger.LogError("Impossible to create default Mumble Room", scope);
                UMI3DLogger.LogException(ex, scope);
            }
        }

        public List<Operation> SwitchDefaultRoom(string name, IEnumerable<UMI3DCollaborationAbstractContentUser> users, bool force = false)
        {
            var ops = new List<Operation>();

            bool localRoom = true;
            if (name != null)
            {
                var match = generalRoomRegex.Match(name);
                if (match.Success)
                {
                    var VmID = match.Groups[2].Captures[0].Value;
                    if (VmID == guid)
                        localRoom = false;
                }
            }

            if (name == null)
                name = roomList.FirstOrDefault()?.name;

            var room = roomList.FirstOrDefault(r => r.name == name) ?? _CreateRoom(name, localRoom);
            if (room == null)
                return ops;

            var old = defaultRoom;
            defaultRoom = room;
            if (old == defaultRoom)
                return ops;

            foreach (var user in users)
                if (force || user.audioChannel.GetValue(user) == old.name)
                    ops.AddRange(SwitchUserRoom(user) ?? new());

            return ops;
        }

        public async void RefreshAsync()
        {
            if (!refreshing)
                await Refresh();
        }

        private MumbleManager(string ip, string http, string guid = null)
        {
            if (string.IsNullOrEmpty(guid))
                guid = Guid.NewGuid().ToString();

            this.guid = guid;
            this.ip = ip;
            this.httpIp = (string.IsNullOrEmpty(http)) ? ip.Split(':')[0] : http;
            m = new MurmurAPI(httpIp);
            roomList = new List<Room>();
            userList = new List<User>();

            generalRoomRegex = new Regex(@"Room_\[(.*)\]_([0-9]*)");
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
            return RemoveSpace(@"Room_[" + guid + @"]_" + i.ToString());
        }

        public async Task Refresh()
        {
            if (refreshing)
            {
                while (refreshing)
                    await UMI3DAsyncManager.Yield();
                return;
            }

            try
            {
                refreshing = true;
                RefreshTime = UnityEngine.Time.time + MaxRefreshTimeSecond;
                await ForceRefresh();
                RefreshTime = UnityEngine.Time.time + MaxRefreshTimeSecond;
                refreshing = false;
            }
            catch (Exception e)
            {
                await UMI3DAsyncManager.Yield();
                UnityEngine.Debug.LogError(e);
                refreshing = false;
            }
        }

        private async Task ForceRefresh()
        {
            try
            {
                if (serv == null)
                {
                    await Task.Yield();
                    serv = await MurmurAPI.Server.Create(m, 1);
                }
                else
                    await serv.Refresh();
                await CheckRoom();
                await CheckUser();
            }
            catch (Exception e)
            {
                UMI3DLogger.LogError($"Error in mumble server refreshing [will try again in 1min] {e.Message} \n {e.StackTrace}", scope);
                UMI3DLogger.LogException(e, scope);
                await UMI3DAsyncManager.Delay(60000);
                await ForceRefresh();
            }
        }

        private async Task CheckUser()
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
                await CreateUser(user, true);

            foreach (var user in toDelete)
                await DeleteUser(user, true);
        }

        private async Task CheckRoom()
        {
            List<Room> toAdd = new List<Room>(this.roomList);
            List<Room> toDelete = new List<Room>();

            foreach (MurmurAPI.Server.Channel channel in serv.Channels)
            {
                var match = generalRoomRegex.Match(channel.data.name);

                if (match.Success)
                {
                    string VmID = match.Groups[2].Captures[0].Value;

                    Room lr = toAdd.FirstOrDefault(r => r.name == channel.data.name);

                    if (lr != null)
                    {
                        toAdd.Remove(lr);
                        lr.id = channel.data.id;

                        foreach (Room child in lr.children)
                            toAdd.Remove(child);
                    }
                    else if (VmID == guid)
                    {
                        var id = int.Parse(match.Groups[1].Captures[0].Value);
                        toDelete.Add(new Room(channel.data.id, channel.data.name, true));
                    }
                }
            }

            foreach (var room in toAdd)
                await CreateRoom(room, true);

            foreach (var room in toDelete)
                await DeleteRoom(room, true);
        }

        private async Task CreateRoom(Room room, bool ignoreWait = false)
        {
            if (!ignoreWait)
                await WaitWhileRefreshing();

            while (!room.root && rootRoom == null)
                await Task.Yield();

            try
            {
                var r = serv.Channels.FirstOrDefault(r => r.data.name == room.name);

                if (r is null)
                {
                    MurmurAPI.Server.Channel c = await serv.CreateChannel(room.name, rootRoom?.id);
                    room.id = (c.data.id);

                    rootRoom?.children.Add(room);
                }
                else
                    room.id = r.data.id;
            }
            catch (Exception e)
            {
                UMI3DLogger.LogError($"Error in mumble create room", scope);
                UMI3DLogger.LogException(e, scope);
                await UMI3DAsyncManager.Delay(500);
                RefreshAsync();
            }
        }

        private async Task DeleteRoom(Room room, bool ignoreWait = false)
        {
            if (!ignoreWait)
                await WaitWhileRefreshing();

            try
            {
                if (room.localRoom)
                {
                    await (serv.Channels.FirstOrDefault(c => c.data.id == room.id)?.DeleteChannel() ?? Task.CompletedTask);
                    rootRoom?.children.Remove(room);

                    try
                    {
                        OnRoomDeleted?.Invoke(room.id);
                    }
                    catch (Exception e)
                    {
                        UMI3DLogger.LogException(e, scope);
                    }
                }
            }

            catch (Exception e)
            {
                UMI3DLogger.LogError($"Error in mumble delete room", scope);
                UMI3DLogger.LogException(e, scope);
                await UMI3DAsyncManager.Delay(500);
                RefreshAsync();
            }
        }

        public async Task CreateUser(User user, bool ignoreWait = false)
        {
            if (!ignoreWait)
                await WaitWhileRefreshing();
            try
            {
                var r = serv.RegisteredUsers.FirstOrDefault(r => r.name == user.login);

                if (r is null)
                {
                    MurmurAPI.Server.User c = await serv.AddUser(user.login, user.password);
                    user.id = (c.id);
                }
                else
                    user.id = r.id;
            }
            catch (Exception e)
            {
                UMI3DLogger.LogError($"Error in mumble create user", scope);
                UMI3DLogger.LogException(e, scope);
                await UMI3DAsyncManager.Delay(500);
                RefreshAsync();
            }
        }

        private async Task DeleteUser(User user, bool ignoreWait = false)
        {
            if (!ignoreWait)
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

        [Obsolete("Use CreateRoomInternalAsync instead")]
        private Room _CreateRoom(string name = null, bool localRoom = true)
        {
            var roomId = localRoomIndex++;
            name = name ?? GenerateRoomName(roomId);
            var room = new Room(roomId, name, localRoom);
            __CreateRoom(room);

            try
            {
                OnRoomCreated?.Invoke(roomId);
            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
            }

            return room;
        }

        private async Task<Room> CreateRoomInternalAsync(string name = null, bool localRoom = true, bool root = false)
        {
            var roomId = localRoomIndex++;
            name = name ?? GenerateRoomName(roomId);
            var room = new Room(roomId, name, localRoom) { root = root };

            await SendCreateRoomRequestAsync(room);

            try
            {
                OnRoomCreated?.Invoke(roomId);
            }
            catch (Exception e)
            {
                UMI3DLogger.LogException(e, scope);
            }

            return room;
        }

        [Obsolete("Use SendCreateRoomRequestAsync instead")]
        private async void __CreateRoom(Room room)
        {
            await CreateRoom(room);
            roomList.Add(room);
        }

        private async Task SendCreateRoomRequestAsync(Room room)
        {
            await CreateRoom(room);

            roomList.Add(room);
        }

        [Obsolete("Use CreateRoomAsync instead")]
        public int CreateRoom()
        {
            Room room = _CreateRoom();
            return room.id;
        }

        public async Task<int> CreateRoomAsync()
        {
            Room room = await CreateRoomInternalAsync();
            return room.id;
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
            return roomList.Select(r => r.id).ToList();
        }

        public string GetRoomName(int roomId)
        {
            Room room = this.roomList.Find(r => r.id == roomId);

            return room?.name ?? string.Empty;
        }

        public void DeleteRoom(int roomId)
        {
            var room = roomList.FirstOrDefault(r => r.id == roomId);
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

        /// <summary>
        /// Create a link between two rooms.
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="otherRoomId"></param>
        /// <returns></returns>
        public async Task LinkRooms(int roomId, int otherRoomId) => await this.m.LinkChannels(this.serv.data.id, roomId, otherRoomId);

        /// <summary>
        /// Removes a link between two rooms.
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="otherRoomId"></param>
        /// <returns></returns>
        public async Task UnlinkRooms(int roomId, int otherRoomId) => await this.m.UnlinkChannels(this.serv.data.id, roomId, otherRoomId);

        /// <summary>
        /// Clear all links from a room.
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public async Task ClearRoomLinks(int roomId) => await this.m.ClearChannelLinks(this.serv.data.id, roomId);

        /// <summary>
        /// Adds ACLs (Access Control Lists) to a an audio room for a specific group. Use <see cref="Permissions"/> for allow and deny flags.
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="groupName"></param>
        /// <param name="allowFlags"></param>
        /// <param name="denyFlags"></param>
        /// <returns></returns>
        public async Task AddACLToChannel(int roomId, string groupName, int allowFlags, int denyFlags)
            => await this.m.AddACLToChannel(this.serv.data.id, roomId, groupName, allowFlags, denyFlags);

        /// <summary>
        /// Removes an ACLs (Access Control Lists) to a an audio room for a specific group.
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="groupName"></param>
        /// <param name="allowFlags"></param>
        /// <param name="denyFlags"></param>
        /// <returns></returns>
        public async Task RemoveACLFromChannel(int roomId, string groupName)
            => await this.m.RemoveACLFromChannel(this.serv.data.id, roomId, groupName);

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

            _CreateUser(_user);
            var ops = new List<Operation>();

            ops.Add(user.audioLogin.SetValue(_user.login));
            ops.Add(ToPrivate(user, user.audioPassword.SetValue(_user.password)));
            ops.Add(user.audioServerUrl.SetValue(ip));
            ops.Add(user.audioUseMumble.SetValue(true));
            ops.AddRange(SwitchUserRoom(user, room));

            return ops;
        }

        async void _CreateUser(User user)
        {
            await CreateUser(user);
            userList.Add(user);
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

        public List<Operation> SwitchUserRoom(UMI3DCollaborationAbstractContentUser user, int roomId = -1)
        {
            var ops = new List<Operation>();

            Room room = roomList.FirstOrDefault(r => r.id == roomId) ?? defaultRoom;
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

        public int GetDefaultRoomId()
        {
            return defaultRoom.id;
        }

        #region Message

        /// <summary>
        /// Sends a message to all users of a a room.
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="message"></param>
        /// <param name="from">If not null, the message will be sent by this user</param>
        /// <returns></returns>
        public async Task SendMessageToRoom(int roomId, string message, UMI3DCollaborationUser from = null)
        {
            User fromUser = null;

            if (from != null)
            {
                fromUser = this.userList.Find(u => u.login == from.audioLogin.GetValue());
                UnityEngine.Debug.Assert(fromUser != null, "No user found with audio login " + from.audioLogin.GetValue());
            }

            await this.m.SendMessageToChannel(this.serv.data.id, roomId, message, fromUser?.id ?? null);
        }

        /// <summary>
        /// Sends a message to a user.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="to">Destination user</param>
        /// <param name="from">If not null, the message will be sent by this user</param>
        /// <returns></returns>
        public async Task SendMessageToUser(string message, UMI3DCollaborationUser to, UMI3DCollaborationUser from = null)
        {
            User toUser = null, fromUser = null;

            if (from != null)
            {
                fromUser = this.userList.Find(u => u.login == from.audioLogin.GetValue());
                UnityEngine.Debug.Assert(fromUser != null, "No user found with audio login " + from.audioLogin.GetValue());
            }

            toUser = this.userList.Find(u => u.login == to.audioLogin.GetValue());

            if (toUser == null)
            {
                UMI3DLogger.LogError("Impossible to send a message to a user, not user found with audio login " + to.audioLogin.GetValue(), scope);
                return;
            }

            await this.m.SendMessageToUser(this.serv.data.id, toUser.id, message, fromUser?.id ?? null);
        }

        #endregion
    }
}