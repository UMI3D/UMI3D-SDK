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

using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using inetum.unityUtils;
using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.collaboration
{
    public class UMI3DCollaborationServer : UMI3DServer, IEnvironment
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.Networking;
        public static new UMI3DCollaborationServer Instance { get => UMI3DServer.Instance as UMI3DCollaborationServer; set => UMI3DServer.Instance = value; }

        public bool isRunning { get; protected set; } = false;

        [SerializeField, ReadOnly]
        private bool useIp = false;
        private UMI3DHttp http;

        public static UMI3DHttp HttpServer => Exists ? Instance.http : null;

        private UMI3DForgeServer forgeServer;

        public static UMI3DForgeServer ForgeServer => Exists ? Instance.forgeServer : null;

        private murmur.MumbleManager mumbleManager;

        public static murmur.MumbleManager MumbleManager => Exists ? Instance.mumbleManager : null;

        public static bool NeedToWaitForCallBackAtUserJoin = false;

        public float tokenLifeTime = 10f;

        public IdentifierApi Identifier;

        [EditorReadOnly, Tooltip("World controller for stand alone api.")]
        public worldController.WorldControllerAPI WorldController;


        [EditorReadOnly]
        public string mumbleIp = "";

        [EditorReadOnly]
        public string guid = "";

        [EditorReadOnly]
        public bool useRandomForgePort;
        [EditorReadOnly]
        public ushort forgePort;
        [EditorReadOnly]
        public string forgeMasterServerHost;
        [EditorReadOnly]
        public ushort forgeMasterServerPort;
        [EditorReadOnly]
        public string forgeNatServerHost;
        [EditorReadOnly]
        public ushort forgeNatServerPort;
        [EditorReadOnly]
        public int forgeMaxNbPlayer = 32;

        [EditorReadOnly]
        public bool useRandomHttpPort;
        [EditorReadOnly]
        public ushort httpPort;

        [EditorReadOnly]
        [Tooltip("set to this HttpUrl if empty")]
        /// <summary>
        /// Url of the default resources server. Set to this HttpUrl if empty.
        /// </summary>
        public string resourcesUrl;

        /// <summary>
        /// /Returns true if <see cref="resourcesUrl"/> is set, which means a resource server is used.
        /// </summary>
        public bool IsResourceServerSetup => !string.IsNullOrEmpty(this.resourcesUrl);

        /// <summary>
        /// url of an image that could be displayed by browser to show different awailable environments.
        /// </summary>
        public string iconServerUrl;

        /// <summary>
        /// Forge server session id
        /// </summary>
        [HideInInspector]
        public string sessionId = "";

        /// <summary>
        /// Forge server description scene (comment)
        /// </summary>
        public string descriptionComment = "";

        ///<inheritdoc/>
        protected override string _GetHttpUrl()
        {
            return "http://" + ip + ":" + httpPort;
        }

        protected override string _GetResourcesUrl()
        {
            return !IsResourceServerSetup ? _GetHttpUrl() : this.resourcesUrl;
        }

        /// <summary>
        /// Get the ForgeConnectionDto.
        /// </summary>
        /// <returns></returns>
        public override ForgeConnectionDto ToDto()
        {
            var dto = new ForgeConnectionDto
            {
                forgeHost = ip,
                httpUrl = _GetHttpUrl(),
                forgeServerPort = forgePort,
                forgeMasterServerHost = forgeMasterServerHost,
                forgeMasterServerPort = forgeMasterServerPort,
                forgeNatServerHost = forgeNatServerHost,
                forgeNatServerPort = forgeNatServerPort,
                resourcesUrl = _GetResourcesUrl(),
                authorizationInHeader = !IsResourceServerSetup
            };
            return dto;
        }

        public async Task Register(RegisterIdentityDto identityDto)
        {
            UMI3DLogger.Log($"User to be Created {identityDto.login} {identityDto.guid} {identityDto.userId} {identityDto.localToken}", scope);
            UMI3DCollaborationServer.Collaboration.CreateUser(identityDto, UserRegisteredCallback);
            await Task.CompletedTask;
        }

        Task<ForgeConnectionDto> IEnvironment.ToDto()
        {
            return Task.FromResult(ToDto());
        }


        internal void UpdateStatus(UMI3DCollaborationUser user, StatusDto dto)
        {
            user.SetStatus(dto.status);
        }

        private List<Umi3dNetworkingHelperModule> collaborativeModule;

        private void Start()
        {
            QuittingManager.OnApplicationIsQuitting.AddListener(ApplicationQuit);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!Exists)
                UMI3DHttp.Destroy();
        }

        /// <summary>
        /// Initialize the server.
        /// </summary>
        public override void Init()
        {
            UMI3DLogger.Log($"Server Init", scope);
            base.Init();

            if (string.IsNullOrEmpty(guid))
                guid = System.Guid.NewGuid().ToString();

            mumbleManager = murmur.MumbleManager.Create(mumbleIp, guid);

            if (collaborativeModule == null)
                collaborativeModule = new List<Umi3dNetworkingHelperModule>() { new UMI3DEnvironmentNetworkingCollaborationModule(), new common.collaboration.UMI3DCollaborationNetworkingModule() };
            UMI3DNetworkingHelper.AddModule(collaborativeModule);

            if (!useIp)
                ip = GetLocalIPAddress();

            httpPort = (ushort)FreeTcpPort(useRandomHttpPort ? 0 : httpPort);
            forgePort = (ushort)FreeTcpPort(useRandomForgePort ? 0 : forgePort);
            //websocketPort = FreeTcpPort(useRandomWebsocketPort ? 0 : websocketPort);

            UMI3DHttp.Destroy();
            http = new UMI3DHttp(httpPort);
            UMI3DHttp.Instance.AddRoot(new UMI3DEnvironmentApi());

            WorldController.Setup();

            forgeServer = UMI3DForgeServer.Create(
                ip, httpPort,
                forgePort,//UDPServer config
                forgeMasterServerHost, forgeMasterServerPort, //ForgeMasterServer
                forgeNatServerHost, forgeNatServerPort, //Forge Nat Hole Punching Server,
                forgeMaxNbPlayer //MAX NB of Players
                );

            var auth = new UMI3DAuthenticator();

            if (auth != null)
                auth.shouldAccdeptPlayer = ShouldAcceptPlayer;

            forgeServer.Host(auth);

            isRunning = true;


            WorldController.SetupAfterServerStart();
            OnServerStart.Invoke();
        }

        private void ShouldAcceptPlayer(string identity, NetworkingPlayer player, Action<bool> action)
        {
            UMI3DLogger.Log($"Should accept player", scope);
            UMI3DCollaborationServer.Collaboration.ConnectUser(player, identity, action, UserCreatedCallback);
        }

        protected void UserRegisteredCallback(UMI3DCollaborationUser user, bool reconnection)
        {
            user.SetStatus(StatusType.REGISTERED);
            if (!reconnection)
            {
                WorldController.NotifyUserRegister(user);
                UMI3DLogger.Log($"User Registered", scope);
                OnUserRegistered.Invoke(user);
            }
        }

        protected void UserCreatedCallback(UMI3DCollaborationUser user, bool reconnection)
        {
            UMI3DLogger.Log($"User Created", scope);
            user.SetStatus(StatusType.CREATED);
            AddUserAudio(user);
            if (!reconnection)
            {
                OnUserCreated.Invoke(user);
            }
            else
            {
                OnUserRecreated.Invoke(user);
            }
            user.InitConnection(forgeServer);
            forgeServer.SendSignalingMessage(user.networkPlayer, user.ToStatusDto());
        }

        private void AddUserAudio(UMI3DCollaborationUser user)
        {
            if (mumbleManager == null)
                return;
            List<Operation> op = mumbleManager.AddUser(user);
            var t = new Transaction() { reliable = true };
            t.AddIfNotNull(op);
            t.Dispatch();
        }


        /// <summary>
        /// Create new peers connection for a new user
        /// </summary>
        /// <param name="user"></param>
        public static async Task NotifyUserJoin(UMI3DCollaborationUser user)
        {
            user.hasJoined = true;
            Collaboration.UserJoin(user);
            bool finished = !NeedToWaitForCallBackAtUserJoin;
            MainThreadManager.Run(async () =>
            {
                UMI3DLogger.Log($"<color=magenta>User Join [{user.Id()}] [{user.login}]</color>", scope);
                await Instance.NotifyUserJoin(user);
                finished = true;
            });

            while (!finished)
                await UMI3DAsyncManager.Yield();
        }

        /// <summary>
        /// Call To Notify a user join.
        /// </summary>
        /// <param name="user">user that join</param>
        public async Task NotifyUserJoin(UMI3DUser user)
        {
            if (user is UMI3DCollaborationUser _user)
                await WorldController.NotifyUserJoin(_user);
            OnUserJoin.Invoke(user);
        }

        public void ClearIP()
        {
            ip = "localhost";
            useIp = false;
        }

        public void SetIP(string ip)
        {
            this.ip = ip;
            useIp = true;
        }

        private static string GetLocalIPAddress()
        {
            IPHostEntry host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ip.ToString().EndsWith(".1"))
                {
                    return ip.ToString();
                }
            }
            //if offline. 
            UMI3DLogger.LogWarning("No public IP found. This computer seems to be offline.", scope);
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        private void ApplicationQuit()
        {
            Clear();
        }

        private void _Stop()
        {
            if (collaborativeModule != null)
                UMI3DNetworkingHelper.RemoveModule(collaborativeModule);
            http?.Stop();
            forgeServer?.Stop();
            if (isRunning)
            {
                isRunning = false;
                OnServerStop.Invoke();
            }
            if (mumbleManager != null)
                mumbleManager.Delete();
        }

        private async void Clear()
        {
            http?.Stop();
            forgeServer?.Stop();
            if (isRunning)
            {
                isRunning = false;
                OnServerStop.Invoke();
            }
            if (mumbleManager != null)
                mumbleManager.Delete();
        }

        public static void Stop()
        {
            Instance.userManager = new UMI3DUserManager();
            Instance._Stop();
        }


        public static int FreeTcpPort(int port = 0)
        {
            try
            {
                var l = new TcpListener(IPAddress.Loopback, port);
                l.Start();
                port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                return port;
            }
            catch (Exception)
            {
                var l = new TcpListener(IPAddress.Loopback, 0);
                l.Start();
                port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                return port;
            }
        }


        #region security

        public static bool IsAuthenticated(WebSocketSharp.Net.HttpListenerRequest request, bool allowOldToken = false)
        {
            if (!Exists)
                return false;
            (UMI3DCollaborationUser user, bool oldToken) c = GetUserFor(request);
            if (c.user == null && !(c.oldToken && allowOldToken))
            {
                return false;
            }
            else
            {
                //byte[] data = Convert.FromBase64String(user.token);
                //var when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
                //if (when < DateTime.UtcNow)
                //{
                //    user.RenewToken();
                //    return false;
                //}
                return true;
            }
        }

        public static (UMI3DCollaborationUser user, bool oldToken) GetUserFor(WebSocketSharp.Net.HttpListenerRequest request)
        {
            string authorization = request.Headers[UMI3DNetworkingKeys.Authorization];
            if (authorization == null)
            {
                return (null, false);
            }
            else
            {
                return Collaboration.GetUserByToken(authorization);
            }
        }

        #endregion

        #region collaboration

        private UMI3DUserManager userManager = new UMI3DUserManager();
        public static UMI3DUserManager Collaboration => Exists ? Instance.userManager : null;

        #endregion

        public static void Logout(UMI3DCollaborationUser user, bool notifiedByUser = true)
        {
            if (user == null)
                return;
            user.hasJoined = false;
            (user.networkPlayer.Networker as IServer).Disconnect(user.networkPlayer, true);
            lock (user.networkPlayer.Networker.Players)
                user.networkPlayer.Networker.Players.Remove(user.networkPlayer);
            Collaboration.Logout(user, notifiedByUser);
            MainThreadManager.Run(() => Instance._Logout(user));
        }

        private void _Logout(UMI3DCollaborationUser user)
        {
            UMI3DLogger.Log($"Logout {user.login} {user.Id()}", scope);
            RemoveUserAudio(user);
            WorldController.NotifyUserLeave(user);
            OnUserLeave.Invoke(user);
        }

        public void NotifyUnregistered(UMI3DCollaborationUser user)
        {
            UMI3DLogger.Log($"Unregistered {user.login} {user.Id()}", scope);
            WorldController.NotifyUserUnregister(user);
            OnUserUnregistered.Invoke(user);
        }

        private void RemoveUserAudio(UMI3DCollaborationUser user)
        {
            if (mumbleManager == null)
                return;
            List<Operation> op = mumbleManager.RemoveUser(user);
            var t = new Transaction() { reliable = true };
            t.AddIfNotNull(op);
            t.Dispatch();
        }

        public float WaitTimeForPingAnswer = 3f;
        public int MaxPingingTry = 5;

        ///<inheritdoc/>
        protected override void LookForMissing(UMI3DUser user)
        {
            if (user is UMI3DCollaborationUser _user && _user?.networkPlayer?.NetworkId != null)
                UnityMainThreadDispatcher.Instance().Enqueue(_lookForMissing(_user, _user.networkPlayer.NetworkId));
        }

        private IEnumerator _lookForMissing(UMI3DCollaborationUser user, uint networkId)
        {
            UMI3DLogger.Log($"look For missing", scope);
            if (user == null) yield break;
            yield return new WaitForFixedUpdate();
            int count = 0;
            while (count++ < MaxPingingTry)
            {
                if (user.status == StatusType.MISSING && user.networkPlayer.NetworkId == networkId)
                {
                    Ping(user);
                }
                else
                {
                    break;
                }

                yield return new WaitForSecondsRealtime(WaitTimeForPingAnswer);
            }
            Logout(user, false);
        }

        public virtual void Ping(UMI3DCollaborationUser user)
        {
            UMI3DLogger.Log($"Ping {user.Id()} {user.login}", scope);
            try
            {
                user.networkPlayer?.Networker?.Ping();
            }
            catch { }
            var sr = new StatusRequestDto { CurrentStatus = user.status };
            ForgeServer.SendSignalingMessage(user.networkPlayer, sr);
        }

        ///<inheritdoc/>
        protected override void _Dispatch(Transaction transaction)
        {
            base._Dispatch(transaction);
            foreach (UMI3DCollaborationUser user in UMI3DCollaborationServer.Collaboration.Users)
            {
                if (user.status == StatusType.NONE)
                {
                    continue;
                }
                if (user.status == StatusType.MISSING || user.status == StatusType.CREATED || user.status == StatusType.READY)
                {
                    if (!TransactionToBeSend.ContainsKey(user))
                    {
                        TransactionToBeSend[user] = new Transaction();
                    }
                    
                    TransactionToBeSend[user] += transaction;
                    continue;
                }

                SendTransaction(user, transaction);
            }
        }

        protected override void _Dispatch(DispatchableRequest dispatchableRequest)
        {
            base._Dispatch(dispatchableRequest);
            foreach (UMI3DUser u in dispatchableRequest.users)
            {
                if (u is UMI3DCollaborationUser user)
                {
                    if (user.status == StatusType.NONE)
                    {
                        continue;
                    }

                    if (user.status == StatusType.MISSING || user.status == StatusType.CREATED || user.status == StatusType.READY)
                    {

                        if (!NavigationToBeSend.ContainsKey(user))
                        {
                            NavigationToBeSend[user] = new List<DispatchableRequest>();
                        }

                        NavigationToBeSend[user].Add(dispatchableRequest);
                        continue;
                    }

                    SendNavigationRequest(user, dispatchableRequest);
                }
            }
        }

        private void SendTransaction(UMI3DCollaborationUser user, Transaction transaction)
        {
            (byte[], bool) c = UMI3DEnvironment.Instance.useDto ? transaction.ToBson(user) : transaction.ToBytes(user);
            if (c.Item2)
                ForgeServer.SendData(user.networkPlayer, c.Item1, transaction.reliable);
        }

        private void SendNavigationRequest(UMI3DCollaborationUser user, DispatchableRequest dispatchableRequest)
        {
            byte[] data = UMI3DEnvironment.Instance.useDto ? dispatchableRequest.ToBson() : dispatchableRequest.ToBytes();
            ForgeServer.SendData(user.networkPlayer, data, dispatchableRequest.reliable);
        }

        private readonly Dictionary<UMI3DCollaborationUser, Transaction> TransactionToBeSend = new Dictionary<UMI3DCollaborationUser, Transaction>();
        private readonly Dictionary<UMI3DCollaborationUser, List<DispatchableRequest>> NavigationToBeSend = new Dictionary<UMI3DCollaborationUser, List<DispatchableRequest>>();
        private void Update()
        {
            foreach (KeyValuePair<UMI3DCollaborationUser, Transaction> kp in TransactionToBeSend.ToList())
            {
                UMI3DCollaborationUser user = kp.Key;
                Transaction transaction = kp.Value;
                if (user.status == StatusType.NONE)
                {
                    TransactionToBeSend.Remove(user);
                    continue;
                }
                if (user.status < StatusType.ACTIVE) continue;
                transaction.Simplify();
                SendTransaction(user, transaction);
                TransactionToBeSend.Remove(user);
            }
            foreach (KeyValuePair<UMI3DCollaborationUser, List<DispatchableRequest>> kp in NavigationToBeSend.ToList())
            {
                UMI3DCollaborationUser user = kp.Key;
                List<DispatchableRequest> navigations = kp.Value;
                if (user.status == StatusType.NONE)
                {
                    NavigationToBeSend.Remove(user);
                    continue;
                }
                if (user.status < StatusType.ACTIVE) continue;
                foreach (var navigation in navigations)
                    SendNavigationRequest(user, navigation);
                TransactionToBeSend.Remove(user);
            }
        }

        /// <summary>
        /// Call To Notify a user status change.
        /// </summary>
        /// <param name="user">user that get its staus updated</param>
        /// <param name="status">new status</param>
        public override void NotifyUserStatusChanged(UMI3DUser user, StatusType status)
        {
            base.NotifyUserStatusChanged(user, status);
            if (user is UMI3DCollaborationUser cUser)
                Collaboration.SetLastUpdate(cUser);
            Collaboration.NotifyUserStatusChanged(user as UMI3DCollaborationUser);
        }

        ///<inheritdoc/>
        public override void NotifyUserChanged(UMI3DUser user)
        {
            Collaboration.NotifyUserStatusChanged(user as UMI3DCollaborationUser);
        }

        public override HashSet<UMI3DUser> UserSet()
        {
            return Collaboration.UsersSet();
        }
        public override IEnumerable<UMI3DUser> Users()
        {
            return Collaboration.Users;
        }
        public override HashSet<UMI3DUser> UserSetWhenHasJoined()
        {
            return new HashSet<UMI3DUser>(Collaboration.Users.Where((u) => u.hasJoined));
        }

        public override float ReturnServerTime()
        {
            return NetworkManager.Instance?.Networker?.Time?.Timestep ?? 0;
        }

        #region session
        public UnityEvent OnServerStart = new UnityEvent();
        public UnityEvent OnServerStop = new UnityEvent();
        #endregion
    }
}