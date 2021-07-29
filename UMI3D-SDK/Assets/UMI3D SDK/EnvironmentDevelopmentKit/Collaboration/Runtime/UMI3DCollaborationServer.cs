﻿/*
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
using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;
using UnityEngine.Events;

namespace umi3d.edk.collaboration
{
    public class UMI3DCollaborationServer : UMI3DServer
    {
        public static new UMI3DCollaborationServer Instance { get { return UMI3DServer.Instance as UMI3DCollaborationServer; } set { UMI3DServer.Instance = value; } }

        public bool isRunning { get; protected set; } = false;

        [SerializeField, ReadOnly]
        bool useIp = false;

        UMI3DHttp http;

        static public UMI3DHttp HttpServer { get => Exists ? Instance.http : null; }

        UMI3DForgeServer forgeServer;

        static public UMI3DForgeServer ForgeServer { get => Exists ? Instance.forgeServer : null; }

        public float tokenLifeTime = 10f;

        public IdentifierApi Identifier;


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

        public AuthenticationType Authentication;

        ///<inheritdoc/>
        protected override string _GetHttpUrl()
        {
            return "http://" + ip + ":" + httpPort;
        }

        /// <summary>
        /// Get the ForgeConnectionDto.
        /// </summary>
        /// <returns></returns>
        public override ForgeConnectionDto ToDto()
        {
            var dto = new ForgeConnectionDto();
            dto.host = ip;
            dto.httpUrl = _GetHttpUrl();
            dto.forgeServerPort = forgePort;
            dto.forgeMasterServerHost = forgeMasterServerHost;
            dto.forgeMasterServerPort = forgeMasterServerPort;
            dto.forgeNatServerHost = forgeNatServerHost;
            dto.forgeNatServerPort = forgeNatServerPort;
            return dto;
        }

        internal void UpdateStatus(UMI3DCollaborationUser user, StatusDto dto)
        {
            user.SetStatus(dto.status);
        }

        List<Umi3dNetworkingHelperModule> collaborativeModule;

        /// <summary>
        /// Initialize the server.
        /// </summary>
        public override void Init()
        {
            base.Init();
            if (collaborativeModule == null)
                collaborativeModule = new List<Umi3dNetworkingHelperModule>() { new UMI3DEnvironmentNetworkingCollaborationModule(), new common.collaboration.UMI3DCollaborationNetworkingModule() };
            UMI3DNetworkingHelper.AddModule(collaborativeModule);

            if (!useIp)
                ip = GetLocalIPAddress();

            httpPort = (ushort)FreeTcpPort(useRandomHttpPort ? 0 : httpPort);
            forgePort = (ushort)FreeTcpPort(useRandomForgePort ? 0 : forgePort);
            //websocketPort = FreeTcpPort(useRandomWebsocketPort ? 0 : websocketPort);

            http = new UMI3DHttp();

            forgeServer = UMI3DForgeServer.Create(
                ip, httpPort,
                forgePort,//UDPServer config
                forgeMasterServerHost, forgeMasterServerPort, //ForgeMasterServer
                forgeNatServerHost, forgeNatServerPort, //Forge Nat Hole Punching Server,
                forgeMaxNbPlayer //MAX NB of Players
                );
            var auth = Identifier?.GetAuthenticator(ref Authentication);
            if (auth != null)
                auth.shouldAccdeptPlayer = ShouldAcceptPlayer;
            forgeServer.Host(auth);

            isRunning = true;
            OnServerStart.Invoke();
        }

        void ShouldAcceptPlayer(IdentityDto identity, NetworkingPlayer player, Action<bool> action)
        {
            UMI3DCollaborationServer.Collaboration.CreateUser(player, identity, action, UserCreatedCallback);
        }

        protected void UserCreatedCallback(UMI3DCollaborationUser user, bool reconnection)
        {
            OnUserCreated.Invoke(user);
            user.InitConnection(forgeServer);
            forgeServer.SendSignalingMessage(user.networkPlayer, user.ToStatusDto());
        }


        /// <summary>
        /// Create new peers connection for a new user
        /// </summary>
        /// <param name="user"></param>
        public static void NotifyUserJoin(UMI3DCollaborationUser user)
        {
            Collaboration.UserJoin(user);
            MainThreadManager.Run(() =>
            {
                Debug.Log($"<color=magenta>User Join [{user.Id()}] [{user.login}]</color>");
                Instance.NotifyUserJoin(user);
            });
        }

        /// <summary>
        /// Call To Notify a user join.
        /// </summary>
        /// <param name="user">user that join</param>
        public void NotifyUserJoin(UMI3DUser user)
        {
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

        static string GetLocalIPAddress()
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !ip.ToString().EndsWith(".1"))
                {
                    return ip.ToString();
                }
            }
            //if offline. 
            Debug.LogWarning("No public IP found. This computer seems to be offline.");
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        void OnApplicationQuit()
        {
            Clear();
        }

        void _Stop()
        {
            if (collaborativeModule != null)
                UMI3DNetworkingHelper.RemoveModule(collaborativeModule);
            http?.Stop();
            forgeServer?.Stop();
            isRunning = false;
            OnServerStop.Invoke();
        }

        void Clear()
        {
            http?.Stop();
            forgeServer?.Stop();
            isRunning = false;
            OnServerStop.Invoke();
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
                TcpListener l = new TcpListener(IPAddress.Loopback, port);
                l.Start();
                port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                return port;
            }
            catch (Exception)
            {
                TcpListener l = new TcpListener(IPAddress.Loopback, 0);
                l.Start();
                port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                return port;
            }
        }


        #region security

        public static bool IsAuthenticated(WebSocketSharp.Net.HttpListenerRequest request)
        {
            if (!Exists)
                return false;
            UMI3DCollaborationUser user = GetUserFor(request);
            if (user == null)
                return false;
            else
            {
                byte[] data = Convert.FromBase64String(user.token);
                DateTime when = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
                if (when < DateTime.UtcNow)
                {
                    user.RenewToken();
                    return false;
                }
                return true;
            }
        }

        public static UMI3DCollaborationUser GetUserFor(WebSocketSharp.Net.HttpListenerRequest request)
        {
            string authorization = request.Headers[UMI3DNetworkingKeys.Authorization];
            if (authorization == null)
                return null;
            else
            {
                return Collaboration.GetUserByToken(authorization);
            }
        }

        #endregion

        #region collaboration

        UMI3DUserManager userManager = new UMI3DUserManager();
        public static UMI3DUserManager Collaboration { get { return Exists ? Instance.userManager : null; } }

        #endregion

        public static void Logout(UMI3DCollaborationUser user)
        {
            if (user == null)
                return;
            (user.networkPlayer.Networker as IServer).Disconnect(user.networkPlayer, true);
            Collaboration.Logout(user);
            MainThreadManager.Run(() => Instance._Logout(user));
        }

        void _Logout(UMI3DCollaborationUser user)
        {
            Debug.Log($"Logout {user.login} {user.Id()}");
            OnUserLeave.Invoke(user);
        }


        public float WaitTimeForPingAnswer = 3f;
        public int MaxPingingTry = 5;

        ///<inheritdoc/>
        protected override void LookForMissing(UMI3DUser user)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(_lookForMissing(user as UMI3DCollaborationUser));
        }

        IEnumerator _lookForMissing(UMI3DCollaborationUser user)
        {
            if (user == null) yield break;
            yield return new WaitForFixedUpdate();
            int count = 0;
            while (count++ < MaxPingingTry)
            {
                if (user.status == StatusType.MISSING)
                {
                    Ping(user);
                }
                else
                    break;
                yield return new WaitForSecondsRealtime(WaitTimeForPingAnswer);
            }
            Logout(user);
        }

        public virtual void Ping(UMI3DCollaborationUser user)
        {
            Debug.Log($"Ping {user.Id()} {user.login}");
            user.networkPlayer.Ping();
            var sr = new StatusRequestDto { CurrentStatus = user.status };
            ForgeServer.SendSignalingMessage(user.networkPlayer, sr);
        }

        ///<inheritdoc/>
        protected override void _Dispatch(Transaction transaction)
        {
            base._Dispatch(transaction);
            foreach (var user in UMI3DCollaborationServer.Collaboration.Users)
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
            foreach (var u in dispatchableRequest.users)
            {
                if (u is UMI3DCollaborationUser user)
                {
                    if (user.status == StatusType.NONE)
                    {
                        continue;
                    }
                    if (user.status == StatusType.MISSING || user.status == StatusType.CREATED || user.status == StatusType.READY)
                    {
                        NavigationToBeSend[user] = dispatchableRequest;
                        continue;
                    }

                    SendNavigationRequest(user, dispatchableRequest);
                }
            }
        }

        void SendTransaction(UMI3DCollaborationUser user, Transaction transaction)
        {
            var c = UMI3DEnvironment.Instance.useDto ? transaction.ToBson(user) : transaction.ToBytes(user);
            if (c.Item2)
                ForgeServer.SendData(user.networkPlayer, c.Item1, transaction.reliable);
        }

        void SendNavigationRequest(UMI3DCollaborationUser user, DispatchableRequest dispatchableRequest)
        {
            var data = UMI3DEnvironment.Instance.useDto ? dispatchableRequest.ToBson() : dispatchableRequest.ToBytes();
            ForgeServer.SendData(user.networkPlayer, data, dispatchableRequest.reliable);
        }

        Dictionary<UMI3DCollaborationUser, Transaction> TransactionToBeSend = new Dictionary<UMI3DCollaborationUser, Transaction>();
        Dictionary<UMI3DCollaborationUser, DispatchableRequest> NavigationToBeSend = new Dictionary<UMI3DCollaborationUser, DispatchableRequest>();
        private void Update()
        {
            foreach (var kp in TransactionToBeSend.ToList())
            {
                var user = kp.Key;
                var transaction = kp.Value;
                if (user.status == StatusType.NONE)
                {
                    TransactionToBeSend.Remove(user);
                    continue;
                }
                if (user.status == StatusType.MISSING || user.status == StatusType.CREATED || user.status == StatusType.READY) continue;
                transaction.Simplify();
                SendTransaction(user, transaction);
                TransactionToBeSend.Remove(user);
            }
            foreach (var kp in NavigationToBeSend.ToList())
            {
                var user = kp.Key;
                var navigation = kp.Value;
                if (user.status == StatusType.NONE)
                {
                    NavigationToBeSend.Remove(user);
                    continue;
                }
                if (user.status == StatusType.MISSING || user.status == StatusType.CREATED || user.status == StatusType.READY) continue;
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

        #region session
        public UnityEvent OnServerStart = new UnityEvent();
        public UnityEvent OnServerStop = new UnityEvent();
        #endregion
    }
}