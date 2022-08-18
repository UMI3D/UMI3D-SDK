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
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Linq;
using inetum.unityUtils;

namespace umi3d.edk.collaboration.murmur
{
    public class MurmurAPI
    {
        string url = "";

        public MurmurAPI(string url)
        {
            this.url = url;
        }

        async Task Send(UnityWebRequest www)
        {
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();
            while (!operation.isDone)
                await UMI3DAsyncManager.Yield();
        }

        string RequestToString(UnityWebRequest www)
        {
            if (www.isHttpError || www.isNetworkError)
                throw new System.Exception("Error" + www.error);
            return www?.downloadHandler?.text;
        }

        async public Task<string> GetRequest(string url)
        {
            var www = UnityWebRequest.Get(url);
            await Send(www);
            return RequestToString(www);
        }

        #region Servers
        //GET /servers/ 	Get server list
        async public Task<string> GetServers()
        {
            return await GetRequest(url + "/servers/");
        }

        //POST /servers/ 	Create a new server, starts it, and returns details
        async public Task<string> AddServer()
        {

            var www = UnityWebRequest.Post(url + "/servers/", "");
            await Send(www);
            return RequestToString(www);
        }

        //GET /servers/:serverid Get server details
        async public Task<string> GetServerInfo(int i)
        {
            return await GetRequest(url + "/servers/" + i);
        }

        //POST /servers/:serverid/start Start server
        async public Task<string> StartServer(int server)
        {
            var www = UnityWebRequest.Post(url + "/servers/" + server + "/start", "");
            await Send(www);
            return RequestToString(www);
        }

        //POST /servers/:serverid/stop Stop server
        async public Task<string> StopServer(int server)
        {
            var www = UnityWebRequest.Post(url + "/servers/" + server + "/stop", "");
            await Send(www);
            return RequestToString(www);
        }

        //DELETE /servers/:serverid Delete server
        //DELETE /servers/delete? id = 1,2,3 	Delete multiple servers
        async public Task<string> DeleteServer(int serverId, params int[] ids)
        {
            if (ids.Length > 0)
            {
                string list = serverId.ToString();
                foreach (var id in ids)
                    list += ',' + id;

                var www = UnityWebRequest.Delete(url + "/servers/delete?id=" + list);
                await Send(www);
                return RequestToString(www);
            }
            else
            {
                var www = UnityWebRequest.Delete(url + "/servers/" + serverId);
                await Send(www);
                return RequestToString(www);
            }
        }

        //GET /servers/:serverid/logs Get server logs
        async public Task<string> GetServerLogs(int i)
        {
            return await GetRequest(url + "/servers/" + i + "/logs");
        }

        //GET /servers/:serverid/bans Get list of banned users
        async public Task<string> GetServerBans(int i)
        {
            return await GetRequest(url + "/servers/" + i + "/bans");
        }

        //GET /servers/:serverid/conf Get server configuration for specified id
        async public Task<string> GetServerConf(int i)
        {
            return await GetRequest(url + "/servers/" + i + "/conf");
        }

        //POST /servers/:serverid/conf? key = users & value = 100    Set configuration variable 'users' to 100
        //POST /servers/:serverid/sendmessage Send a message to all channels in a server.formdata: message
        //POST /servers/:serverid/setsuperuserpw Sets SuperUser password. formdata: password
        #endregion

        #region Stats
        //GET /stats/ 	Get all statistics
        async public Task<string> GetStats()
        {
            return await GetRequest(url + "/stats/");
        }
        #endregion

        #region Users
        //GET /servers/:serverid/user Get all users in a server
        async public Task<string> GetServerUsers(int i)
        {
            return await GetRequest(url + "/servers/" + i + "/user");
        }
        //GET /servers/:serverid/user/:userid Get User
        async public Task<string> GetServerUsers(int i, int user)
        {
            return await GetRequest(url + "/servers/" + i + "/user/" + user);
        }
        //POST /servers/:serverid/user Create User, formdata: username&password
        async public Task<string> AddUser(int server, string userName,string password)
        {
            var form = new List<IMultipartFormSection>();
            form.Add(new MultipartFormDataSection("username", userName));
            form.Add(new MultipartFormDataSection("password", password));
            var www = UnityWebRequest.Post(url + "/servers/" + server + "/user", form);
            await Send(www);
            return RequestToString(www);
        }

        //DELETE /servers/:serverid/user/:userid Delete User
        async public Task<string> DeleteUser(int server, int user)
        {
            var www = UnityWebRequest.Delete(url + "/servers/" + server + "/user/" + user);
            await Send(www);
            return RequestToString(www);
        }

        //POST /servers/:serverid/kickuser? usersession = 1  Kick user with session #1
        //POST /servers/:serverid/user/:userid/mute Mute User
        //POST /servers/:serverid/user/:userid/unmute Unmute User
        //POST /servers/:serverid/user/:userid/update Update registered username.formdata: username
        #endregion

        #region Channels
        //GET /servers/:serverid/channels Get all channels in a server
        async public Task<string> GetServerChannels(int i)
        {
            return await GetRequest(url + "/servers/" + i + "/channels");
        }

        //GET /servers/:serverid/channels/:channelid Get a channel from a server by ID
        async public Task<string> GetServerChannelsInfo(int i, int channel)
        {
            return await GetRequest(url + "/servers/" + i + "/channels/" + channel);
        }

        //POST /servers/:serverid/channels Create Channel, formdata: name&parent
        async public Task<string> CreateChannel(int server, string name, int parent)
        {
            var form = new List<IMultipartFormSection>();
            form.Add(new MultipartFormDataSection("name", name));
            form.Add(new MultipartFormDataSection($"parent", parent.ToString()));
            var www = UnityWebRequest.Post(url + "/servers/" + server + "/channels", form);
            await Send(www);
            return RequestToString(www);
        }

        //GET /servers/:serverid/channels/:channelid/acl Get ACL list for channel ID
        async public Task<string> GetServerChannelACL(int i, int channel)
        {
            return await GetRequest(url + "/servers/" + i + "/channels/" + channel + "/acl");
        }

        //DELETE /servers/:serverid/channels/:channelid Delete Channel
        async public Task<string> DeleteChannel(int server, int channel)
        {
            var www = UnityWebRequest.Delete(url + "/servers/" + server + "/channels/" + channel);
            await Send(www);
            return RequestToString(www);
        }
        #endregion

        public class MurmurClass
        {
            protected MurmurAPI murmur;

            public void SetApi(MurmurAPI api) { murmur = api; }

            protected T Convert<T>(string json)
            {
                return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None
                });
            }
        }

        public class ChannelData : MurmurClass
        {
            public string description = "";
            public int id = -3;
            // public string[] links;
            public string name = "";
            public int parent = 0;
            public int position = 0;
            public bool temporary = false;

            public async Task Delete()
            {
                await murmur.DeleteChannel(1, id);
            }
        }

        public class SubChannelData : MurmurClass
        {
            public ChannelData c = null;
            public User[] users = null;
            

            public async Task Delete()
            {
                await c.Delete();
            }
        }

        public class ServerDataShort : MurmurClass
        {
            public string address = "";
            public int channels = 1;
            public string host = "";
            public int id = 1;
            public int log_length = 181;
            public int maxusers = 1000;
            public string name = "";
            public int port = 64738;
            public bool running = true;
            public string uptime = "";
            public int uptime_seconds = 603;
            public int users = 1;
        }

        public class User : MurmurClass
        {
            public int[] address;
            public int bytespersec = 0;
            public int channel = 0;
            public string comment = "";
            public string context = "";
            public bool deaf = false;
            public string identity = "";
            public int idlesecs = 447;
            public bool mute = false;
            public string name = "";
            public int onlinesecs = 447;
            public string os = "";
            public string osversion = "";
            public bool prioritySpeaker = false;
            public bool recording = false;
            public string release = "";
            public bool selfDeaf = false;
            public bool selfMute = false;
            public int session = 1;
            public bool suppress = false;
            public double tcpPing = 6.7390899658203125;
            public bool tcponly = false;
            public double udpPing = 6.472775459289551;
            public int userid = -1;
            public int version = 66790;
        }

        public class ServerData : MurmurClass
        {
            public string address = "";
            public string host = "";
            public string humanize_uptime = "0:10:03";
            public int id = 1;
            public int log_length = 181;
            public int maxusers = 1000;
            public string name = "";
            public ChannelData parent_channel;
            public string password = "";
            public int port = 64738;
            public Dictionary<string, string> registered_users;
            bool running = true;
            public SubChannelData[] sub_channels;
            public int uptime = 603;
            public int user_count = 1;
            public User[] users;
            public string welcometext = "";
        }

        public class NewUserData : MurmurClass
        {
            public string last_active = "";
            public int user_id = -1;
            public string username = "";
        }

        public class UserData : MurmurClass
        {
            public string last_active = "";
            public string user_id = "";
            public string username = "";
        }

        public class Server : MurmurClass
        {
            public ServerData data { get; private set; }

            public List<Channel> Channels;
            public List<User> RegisteredUsers;
            public List<User> ConnectedUsers;

            private Server()
            {
                Channels = new List<Channel>();
                RegisteredUsers = new List<User>();
                ConnectedUsers = new List<User>();
            }

            static public async Task<Server> Create(MurmurAPI murmur, int id)
            {
                var server = new Server();
                server.SetApi(murmur);
                await server.Refresh(id);
                return server;
            }

            async Task Refresh(int id)
            {
                var info = await murmur.GetServerInfo(id);
                data = Convert<ServerData>(info);

                Debug.Log(info);

                Channels.Where(c => !data.sub_channels.Any(d => d.c.id == c.data.id)).ForEach(c =>Channels.Remove(c));

                var toAdd = data.sub_channels.Where(d => !Channels.Any(c => d.c.id == c.data.id));

                if (toAdd.Count() > 0)
                    Channels.AddRange(toAdd.Select(d => Channel.Create(murmur, this, d.c)));

                RegisteredUsers.Clear();
                if(data.registered_users != null)
                    RegisteredUsers.AddRange(data.registered_users.Where(r=>r.Key != "0").Select(r=>new User(r.Key, r.Value)));
                await Users();
            }

            public Channel GetChannel(int id)
            {
                return Channels.FirstOrDefault(c => c.data.id == id);
            }

            public async Task Refresh()
            {
                await Refresh(data.id);
            }

            public class User
            {
                public int id;
                public string name;
                public Channel channel;

                public User(string id, string name)
                {
                    if(int.TryParse(id,out int intId))
                        this.id =  intId;
                    else
                        this.id = -1;
                    this.name = name;
                    channel = null;
                }

                public User(int id, string name, Channel channel)
                {
                    this.id = id;
                    this.name = name;
                    this.channel = channel;
                }
            }

            public class Channel : MurmurClass
            {
                public ChannelData data { get; private set; }
                Server server;

                private Channel()
                {
                }

                static public async Task<Channel> Create(MurmurAPI murmur, Server server, int id)
                {
                    var channel = new Channel();
                    channel.server = server;
                    channel.SetApi(murmur);
                    await channel.Refresh(id);

                    server.Channels.Add(channel);

                    return channel;
                }

                static public Channel Create(MurmurAPI murmur, Server server, ChannelData data)
                {
                    var channel = new Channel();
                    channel.server = server;
                    channel.SetApi(murmur);
                    channel.data = data;

                    server.Channels.Add(channel);

                    return channel;
                }

                public async Task<Channel> CreateChannel(string name)
                {
                    return Create(
                        murmur,
                        server,
                        Convert<ChannelData>(await murmur.CreateChannel(data.id, name, data.id)));
                }

                async Task Refresh(int id)
                {
                    var info = await murmur.GetServerChannelsInfo(server.data.id, id);
                    Debug.Log(info);
                    data = Convert<ChannelData>(info);
                }

                public async Task Refresh()
                {
                    await Refresh(data.id);
                }

                public async Task DeleteChannel()
                {
                    await murmur.DeleteChannel(server.data.id, data.id);
                    server.Channels.Remove(this);
                }

            }

            public async Task<Channel> CreateChannel(string name)
            {
                return Channel.Create(
                    murmur,
                    this,
                    Convert<ChannelData>(await murmur.CreateChannel(data.id, name, data.parent_channel.id)));
            }

            public async Task<List<User>> Users()
            {
                var info = await murmur.GetServerUsers(data.id);
                var dt = Convert<Dictionary<string, MurmurAPI.User>>(info);
                ConnectedUsers.Clear();
                if (data.users != null)
                    ConnectedUsers.AddRange(dt.Values.Where(r => r.userid != 0).Select(r => new User(r.userid, r.name, GetChannel(r.channel))));
                return ConnectedUsers;
            }

            public async Task<User> Users(int id)
            {
                try
                {
                    var info = await murmur.GetServerUsers(data.id, id);
                    var dt = Convert<UserData>(info);
                    var r = new User(dt.user_id.ToString(), dt.username);
                    return r;
                }
                catch
                {
                    return null;
                }
            }

            public async Task<User> AddUser(string name, string password)
            {
                var info = await murmur.AddUser(data.id, name, password);
                try
                {
                    var dt = Convert<NewUserData>(info);
                    var r = new User(dt.user_id.ToString(), dt.username);
                    RegisteredUsers.Add(r);
                    return r;
                }
                catch
                {
                    return null;
                }
            }

            public async Task<bool> RemoveUser(int user)
            {
                if (user == 0) return false;
                var r = RegisteredUsers.FirstOrDefault(u=>u.id == user);
                if(r != null)
                    RegisteredUsers.Remove(r);
                var info = await murmur.DeleteUser(data.id, user);
                return info == null;
            }
        }

    }
}