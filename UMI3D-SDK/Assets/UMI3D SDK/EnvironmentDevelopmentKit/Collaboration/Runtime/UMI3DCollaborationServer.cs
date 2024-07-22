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
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using System.Net.NetworkInformation;
using WebSocketSharp;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// Manager for the UMI3D server in a collaborative context.
    /// </summary>
    public class UMI3DCollaborationServer : UMI3DServer, IEnvironment, IUMI3DCollaborationServer
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.Networking;
        public static new UMI3DCollaborationServer Instance { get => UMI3DServer.Instance as UMI3DCollaborationServer; set => UMI3DServer.Instance = value; }

        /// <summary>
        /// Is the server active?
        /// </summary>
        public bool isRunning { get; protected set; } = false;

        [EditorReadOnly]
        public bool useLoopback = false;

        [SerializeField, ReadOnly]
        private bool useIp = false;
        private UMI3DHttp http;
        private UMI3DHttp _httpForWC;
        private UMI3DHttp httpForWC => _httpForWC ?? http;

        public static UMI3DHttp HttpServer => Exists ? Instance.http : null;

        public static UMI3DHttp HttpServerForWorldController => Exists ? Instance.httpForWC : null;

        private UMI3DForgeServer forgeServer;

        public static UMI3DForgeServer ForgeServer => Exists ? Instance.forgeServer : null;

        private murmur.MumbleManager mumbleManager;

        public static murmur.MumbleManager MumbleManager => Exists ? Instance.mumbleManager : null;

        public float tokenLifeTime = 10f;

        public IdentifierApi Identifier;

        [EditorReadOnly, Tooltip("World controller for standalone API.")]
        public worldController.WorldControllerAPI WorldController;

        /// <summary>
        /// IP adress of the Murmur server.
        /// </summary>
        [EditorReadOnly, Tooltip("IP adress of the Mumur server.")]
        public string mumbleIp = "";

        [EditorReadOnly]
        public string mumbleHttpIp = "";

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
        public bool useWorldControllerSpecificHttpPort;
        [EditorReadOnly]
        public bool useRandomPortForWorldController;
        [EditorReadOnly]
        public ushort httpPortForWorldController;

        [EditorReadOnly]
        [Tooltip("URL of the default resources server. Set to this HttpUrl if empty")]
        /// <summary>
        /// URL of the default resources server. Set to this HttpUrl if empty.
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
        /// Forge server session id.
        /// </summary>
        [HideInInspector]
        public string sessionId = "";

        /// <summary>
        /// Forge server description scene (comment).
        /// </summary>
        [Tooltip("Forge server description scene (comment).")]
        public string descriptionComment = "";

        /// <inheritdoc/>
        protected override string _GetHttpUrl()
        {
            return "http://" + ip + ":" + httpPort;
        }

        protected override string _GetResourcesUrl()
        {
            return !IsResourceServerSetup ? _GetHttpUrl() : this.resourcesUrl;
        }

        /// <summary>
        /// Get the <see cref="EnvironmentConnectionDto"/>.
        /// </summary>
        /// <returns></returns>
        public override EnvironmentConnectionDto ToDto()
        {
            var dto = new EnvironmentConnectionDto
            {
                name = UMI3DEnvironment.Instance.environmentName,
                forgeHost = ip,
                httpUrl = _GetHttpUrl(),
                forgeServerPort = forgePort,
                forgeMasterServerHost = forgeMasterServerHost,
                forgeMasterServerPort = forgeMasterServerPort,
                forgeNatServerHost = forgeNatServerHost,
                forgeNatServerPort = forgeNatServerPort,
                resourcesUrl = _GetResourcesUrl(),
                authorizationInHeader = !IsResourceServerSetup,
                version = UMI3DVersion.version
            };
            return dto;
        }

        public async Task Register(RegisterIdentityDto identityDto, bool resourcesOnly = false)
        {
            if (resourcesOnly)
            {
                UnityEngine.Debug.Log($"Register resourcesOnly : {identityDto.displayName} {identityDto.localToken}");
                UMI3DCollaborationServer.Collaboration.CreateUserResourcesOnly(identityDto, UserRegisteredCallback);
                return;
            }

            UMI3DLogger.Log($"User to be Created {identityDto.login} {identityDto.guid} {identityDto.userId} {identityDto.localToken}", scope);
            UMI3DCollaborationServer.Collaboration.CreateUser(identityDto, UserRegisteredCallback);
            await Task.CompletedTask;
        }

        Task<EnvironmentConnectionDto> IEnvironment.ToDto()
        {
            return Task.FromResult(ToDto());
        }


        internal void UpdateStatus(UMI3DCollaborationAbstractContentUser user, StatusDto dto)
        {
            user.SetStatus(dto.status);
        }

        private List<UMI3DSerializerModule> collaborativeModule;

        private void Start()
        {
            Debug.Assert(Identifier != null, "Identifier cannot be null");
            Debug.Assert(WorldController != null, "WorldController cannot be null");

            QuittingManager.OnApplicationIsQuitting.AddListener(ApplicationQuit);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (!Exists)
                http.Dispose();
        }

        /// <inheritdoc/>
        public override void Init()
        {
            UMI3DLogger.Log($"Server Init", scope);
            base.Init();

            if (string.IsNullOrEmpty(guid))
                guid = System.Guid.NewGuid().ToString();

            mumbleManager = murmur.MumbleManager.Create(mumbleIp, mumbleHttpIp, guid);

            if (collaborativeModule == null)
                collaborativeModule = UMI3DSerializerModuleUtils.GetModules().ToList();

            UMI3DSerializer.AddModule(collaborativeModule);

            if (!useIp)
                ip = GetLocalIPAddress(useLoopback);

            UnityEngine.Debug.Log(ip);

            httpPort = (ushort)FreeTcpPort(useRandomHttpPort ? 0 : httpPort);
            forgePort = (ushort)FreeTcpPort(useRandomForgePort ? 0 : forgePort);
            httpPortForWorldController = useWorldControllerSpecificHttpPort ? (ushort)FreeTcpPort(useRandomPortForWorldController ? 0 : httpPortForWorldController) : httpPort;

            http?.Dispose();
            _httpForWC?.Dispose();

            http = new UMI3DHttp(httpPort);
            _httpForWC = useWorldControllerSpecificHttpPort ? new UMI3DHttp(httpPortForWorldController) : null;

            http.AddRoot(new UMI3DEnvironmentApi());
            httpForWC.AddRoot(new UMI3DEnvironmentFromWorldControllerApi());
            http.AddRoot(new UMI3DResourcesServerApi());
            UnityEngine.Debug.Log("Add it only when needed on its port");

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

        protected void UserRegisteredCallback(UMI3DUser user, bool reconnection)
        {
            user.SetStatus(StatusType.REGISTERED);
            if (!reconnection)
            {
                if (user is UMI3DCollaborationAbstractContentUser collaborationUser)
                    WorldController.NotifyUserRegister(collaborationUser);
                UMI3DLogger.Log($"User Registered", scope);
                OnUserRegistered.Invoke(user);
            }
        }

        protected void UserCreatedCallback(UMI3DCollaborationAbstractContentUser user, bool reconnection)
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

        private void AddUserAudio(UMI3DCollaborationAbstractContentUser user)
        {
            if (mumbleManager == null)
                return;
            if (user is UMI3DCollaborationUser collaborationUser)
            {
                List<Operation> op = mumbleManager.AddUser(collaborationUser);
                var t = new Transaction() { reliable = true };
                t.AddIfNotNull(op);
                t.Dispatch();
            }
            else if (user is UMI3DServerUser serv)
                mumbleManager.AddUser(serv);
        }


        /// <summary>
        /// Create new peers connection for a new user
        /// </summary>
        /// <param name="user"></param>
        public static async Task NotifyUserJoin(UMI3DCollaborationAbstractContentUser user)
        {
            user.hasJoined = true;

            Collaboration.UserJoin(user);

            MainThreadManager.Run(async () =>
            {
                UMI3DLogger.Log($"<color=magenta>User Join [{user.Id()}] [{user.login}]</color>", scope);
                await Instance.NotifyUserJoin(user);
            });
        }

        /// <summary>
        /// Call To Notify a user join.
        /// </summary>
        /// <param name="user">user that join</param>
        public async Task NotifyUserJoin(UMI3DUser user)
        {
            if (user is UMI3DCollaborationAbstractContentUser _user)
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


        private static string DebugGetLocalIPAddress(Func<NetworkInterface, bool> networkPredicate, Func<NetworkInterface, IPAddressInformation, (bool, int)> AddressPredicate)
        {
            return NetworkInterface.GetAllNetworkInterfaces().SelectMany(n => n.GetIPProperties().UnicastAddresses.Select(a => (n, a))).Select(c => $"{c.n.Name} : {c.a.Address} [networkPredicate :{networkPredicate?.Invoke(c.n)}|AddressPredicate :{AddressPredicate?.Invoke(c.n, c.a)}]").Aggregate("", (a, b) => $"{a}{Environment.NewLine}{b}");
        }

        private static IPAddressInformation GetLocalIPAddress(Func<NetworkInterface, bool> networkPredicate, Func<NetworkInterface, IPAddressInformation, (bool, int)> AddressPredicate)
        {
            if (networkPredicate == null)
                networkPredicate = (NetworkInterface n) => true;
            if (AddressPredicate == null)
                AddressPredicate = (NetworkInterface n, IPAddressInformation a) => (true, 0);

            (IPAddressInformation, bool, int) SelectAdress((NetworkInterface n, UnicastIPAddressInformation a) c)
            {
                var r = AddressPredicate(c.n, c.a);
                return (c.a, r.Item1, r.Item2);
            }

            return NetworkInterface.GetAllNetworkInterfaces().Where(networkPredicate).SelectMany(n => n.GetIPProperties().UnicastAddresses.Select(a => (n, a))).Select(SelectAdress).Where(t => t.Item2).OrderBy(t => t.Item3).FirstOrDefault().Item1;
        }

        private static string GetLocalIPAddress(bool loopback)
        {
            bool NetworkPredicate(NetworkInterface network)
            {
                if (loopback)
                    return network.NetworkInterfaceType == NetworkInterfaceType.Loopback;

                return network.NetworkInterfaceType == NetworkInterfaceType.Ethernet || network.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || network.NetworkInterfaceType == NetworkInterfaceType.Loopback;
            }

            static (bool, int) AdressPredicate(NetworkInterface network, IPAddressInformation ip)
            {
                return ((ip.Address.AddressFamily == AddressFamily.InterNetwork) && ip.Address.IsLocal(), network.NetworkInterfaceType == NetworkInterfaceType.Loopback ? 10 : 0);
            }

            var ip = GetLocalIPAddress(NetworkPredicate, AdressPredicate);
            if (ip == null)
                throw new Exception("Local IP Address Not Found!");
            return ip.Address.ToString();
        }

        private void ApplicationQuit()
        {
            Clear();
        }

        private void _Stop()
        {
            if (collaborativeModule != null)
                UMI3DSerializer.RemoveModule(collaborativeModule);
            http?.Stop();
            forgeServer?.Stop();
            if (isRunning)
            {
                isRunning = false;
                OnServerStop.Invoke();
            }
            if (mumbleManager != null)
                mumbleManager.Delete();

            if (forgeServer != null)
            {
                GameObject.Destroy(forgeServer.gameObject);
                forgeServer = null;
            }
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

            if (forgeServer != null)
            {
                GameObject.Destroy(forgeServer.gameObject);
                forgeServer = null;
            }
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

        public static bool IsAuthenticated(WebSocketSharp.Net.HttpListenerRequest request, bool allowOldToken = false, bool allowResourceOnly = false)
        {
            if (!Exists)
                return false;
            (UMI3DCollaborationAbstractContentUser user, bool oldToken, bool resourceOnly) c = GetUserFor(request);
            if (c.user == null && !(c.oldToken && allowOldToken) && !(c.resourceOnly && allowResourceOnly))
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

        public static (UMI3DCollaborationAbstractContentUser user, bool oldToken, bool resourcesOnly) GetUserFor(WebSocketSharp.Net.HttpListenerRequest request)
        {
            string authorization = request.Headers[UMI3DNetworkingKeys.Authorization];
            if (authorization == null)
            {
                return (null, false, false);
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

        public static void Logout(UMI3DCollaborationAbstractContentUser user, bool notifiedByUser = true)
        {
            if (user == null)
                return;
            user.hasJoined = false;

            if (user.networkPlayer != null)
            {
                (user.networkPlayer.Networker as IServer).Disconnect(user.networkPlayer, true);
                lock (user.networkPlayer.Networker.Players)
                    user.networkPlayer.Networker.Players.Remove(user.networkPlayer);
            }

            Collaboration.Logout(user, notifiedByUser);
            MainThreadManager.Run(() => Instance._Logout(user));
        }

        private void _Logout(UMI3DCollaborationAbstractContentUser user)
        {
            UMI3DLogger.Log($"Logout {user.login} {user.Id()}", scope);
            if (user is UMI3DCollaborationUser cUser)
                RemoveUserAudio(cUser);
            WorldController.NotifyUserLeave(user);
            OnUserLeave.Invoke(user);
        }

        public void NotifyUnregistered(UMI3DCollaborationAbstractContentUser user)
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

        /// <inheritdoc/>
        protected override void LookForMissing(UMI3DUser user)
        {
            if (user is UMI3DCollaborationAbstractContentUser _user && _user?.networkPlayer?.NetworkId != null)
                UnityMainThreadDispatcher.Instance().Enqueue(_lookForMissing(_user, _user.networkPlayer.NetworkId));
        }

        private IEnumerator _lookForMissing(UMI3DCollaborationAbstractContentUser user, uint networkId)
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

        public virtual void Ping(UMI3DCollaborationAbstractContentUser user)
        {
            UMI3DLogger.Log($"Ping {user.Id()} {user.login}", scope);
            try
            {
                user.networkPlayer?.Networker?.Ping();
            }
            catch { }
            var sr = new StatusRequestDto { status = user.status };
            ForgeServer.SendSignalingMessage(user.networkPlayer, sr);
        }

        /// <inheritdoc/>
        protected override void _Dispatch(Transaction transaction)
        {
            base._Dispatch(transaction);
            foreach (UMI3DCollaborationAbstractContentUser user in UMI3DCollaborationServer.Collaboration.Users.Where(u => u is UMI3DCollaborationAbstractContentUser))
            {
                switch (user.status)
                {
                    case StatusType.NONE:
                    case StatusType.REGISTERED:
                        continue;
                    case StatusType.MISSING:
                    case StatusType.CREATED:
                    case StatusType.READY:
                        if (!transaction.reliable)
                            continue;

                        if (!TransactionToBeSend.ContainsKey(user))
                        {
                            TransactionToBeSend[user] = new Transaction();
                        }

                        TransactionToBeSend[user] += transaction;
                        continue;
                }

                if (user.networkPlayer == null)
                {
                    UMI3DLogger.LogWarning($"Network player null, user : id {user.Id()}, display name {user.displayName}", scope);
                    continue;
                }

                SendTransaction(user, transaction);
            }
        }

        private void SendTransaction(UMI3DCollaborationAbstractContentUser user, Transaction transaction)
        {
            (byte[], bool) c = UMI3DEnvironment.Instance.useDto ? transaction.ToBson(user) : transaction.ToBytes(user);
            if (c.Item2)
                ForgeServer.SendData(user.networkPlayer, c.Item1, transaction.reliable);
        }

        private readonly Dictionary<UMI3DCollaborationAbstractContentUser, Transaction> TransactionToBeSend = new Dictionary<UMI3DCollaborationAbstractContentUser, Transaction>();

        public PendingTransactionDto IsThereTransactionPending(UMI3DCollaborationAbstractContentUser user) => new PendingTransactionDto()
        {
            areTransactionPending = (TransactionToBeSend.ContainsKey(user) && TransactionToBeSend[user].Any(o => o.users.Contains(user)))
        };

        private void Update()
        {
            if (TransactionToBeSend.Count == 0)
                return;

            foreach (KeyValuePair<UMI3DCollaborationAbstractContentUser, Transaction> kp in TransactionToBeSend.ToList())
            {
                UMI3DCollaborationAbstractContentUser user = kp.Key;
                Transaction transaction = kp.Value;
                if (user.status == StatusType.NONE)
                {
                    TransactionToBeSend.Remove(user);
                    continue;
                }
                if (user.status < StatusType.ACTIVE) continue;
                transaction.Simplify();
                try
                {
                    SendTransaction(user, transaction);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
                TransactionToBeSend.Remove(user);
            }
        }


        public override void NotifyUserRefreshed(UMI3DUser user)
        {
            Collaboration.NotifyUserStatusChanged(user as UMI3DCollaborationAbstractContentUser);
            base.NotifyUserRefreshed(user);
        }

        /// <summary>
        /// Call To Notify a user status change.
        /// </summary>
        /// <param name="user">user that get its staus updated</param>
        /// <param name="status">new status</param>
        public override void NotifyUserStatusChanged(UMI3DUser user, StatusType status)
        {
            base.NotifyUserStatusChanged(user, status);
            if (user is UMI3DCollaborationAbstractContentUser cUser)
                Collaboration.SetLastUpdate(cUser);
            Collaboration.NotifyUserStatusChanged(user as UMI3DCollaborationAbstractContentUser);
        }

        /// <inheritdoc/>
        public override void NotifyUserChanged(UMI3DUser user)
        {
            Collaboration.NotifyUserStatusChanged(user as UMI3DCollaborationAbstractContentUser);
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