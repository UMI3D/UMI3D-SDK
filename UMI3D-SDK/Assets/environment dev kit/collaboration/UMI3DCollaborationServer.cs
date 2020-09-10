/*
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

using MainThreadDispatcher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using umi3d.common;
using umi3d.common.collaboration;
using Unity.WebRTC;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class UMI3DCollaborationServer : UMI3DServer
    {
        public static new UMI3DCollaborationServer Instance { get { return UMI3DServer.Instance as UMI3DCollaborationServer; } set { UMI3DServer.Instance = value; } }

        UMI3DHttp http;
        UMI3DWebsocket websocket;
        UMI3DWebRTC webRTC;

        static public UMI3DWebRTC WebRTC { get => Exists ? Instance.webRTC : null; }

        public float tokenLifeTime = 10f;

        public IdentifierApi Identifier;

        private void OnAudioFilterRead(float[] data, int channels)
        {
            Audio.Update(data, data.Length);
        }

        public bool useRandomWebsocketPort;
        public int websocketPort;

        public bool useRandomHttpPort;
        public int httpPort;

        protected AuthenticationType Authentication;

        /// <summary>
        /// Return the Authentication type.
        /// </summary>
        /// <returns></returns>

        protected override AuthenticationType _GetAuthentication()
        {
            return Instance.Authentication;
        }

        protected override string _GetWebsocketUrl()
        {
            return "http://" + ip + ":" + websocketPort;
        }
        protected override string _GetHttpUrl()
        {
            return "http://" + ip + ":" + httpPort;
        }

        /// <summary>
        /// Get the WebsocketConnectionDto.
        /// </summary>
        /// <returns></returns>
        public override UMI3DDto ToDto()
        {
            var dto = new WebsocketConnectionDto();
            dto.IP = ip;
            dto.Port = httpPort;
            dto.Postfix = UMI3DNetworkingKeys.websocket;
            dto.websocketUrl = "ws://" + ip + ":" + websocketPort + UMI3DNetworkingKeys.websocket;
            return dto;
        }

        /// <summary>
        /// Initialize the server.
        /// </summary>
        public override void Init()
        {
            base.Init();

            ip = GetLocalIPAddress();

            httpPort = FreeTcpPort(useRandomHttpPort ? 0 : httpPort);
            websocketPort = FreeTcpPort(useRandomWebsocketPort ? 0 : websocketPort);

            http = new UMI3DHttp();
            websocket = new UMI3DWebsocket();
            webRTC = new UMI3DWebRTC(this);
        }

        /// <summary>
        /// Transmit RTCMessage to the RtcServer.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public void WebRtcMessage(string id, RTCDto dto)
        {
            Instance.webRTC.HandleMessage(dto);
        }

        /// <summary>
        /// Send a Message via The RTCServer to all peers.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="reliable"></param>
        public static void sendRTC(string message, bool reliable)
        {
            Instance.webRTC.Send(message, reliable);
        }

        public static void sendRTC(UMI3DUser user, UMI3DDto dto, bool reliable)
        {
            Instance.webRTC.Send(dto, reliable, user.Id());
        }

        /// <summary>
        /// Create new peers connection for a new user
        /// </summary>
        /// <param name="user"></param>
        public static void newUser(UMI3DCollaborationUser user)
        {
            Instance.webRTC.newUser(user);
            MainThreadDispatcher.UnityMainThreadDispatcher.Instance().Enqueue(Instance.NotifyUserJoin(user));
        }

        /// <summary>
        /// Call To Notify a user join.
        /// </summary>
        /// <param name="user">user that join</param>
        public IEnumerator NotifyUserJoin(UMI3DUser user)
        {
            OnUserJoin.Invoke(user);
            yield break;
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
            http?.Stop();
            websocket?.Stop();
            webRTC?.Stop();
        }

        void Clear()
        {
            http?.Stop();
            websocket?.Stop();
            webRTC?.Clear();
        }

        public static void Stop()
        {
            Instance.userManager = new UMI3DUserManager();
            Instance._Stop();
        }


        static int FreeTcpPort(int port = 0)
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
            UnityMainThreadDispatcher.Instance().Enqueue(Instance._Logout(user));
        }

        IEnumerator _Logout(UMI3DCollaborationUser user)
        {
            Collaboration.Logout(user);
            Instance.webRTC.UserLeave(user);
            OnUserLeave.Invoke(user);
            yield break;
        }

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

                var transactionDto = new TransactionDto();
                transactionDto.operations = new List<AbstractOperationDto>(transaction.Operations.Where((op) => { return op.users.Contains(user); }).Select((op) => { return op.ToOperationDto(user); }));
                if (transactionDto.operations.Count > 0)
                {
                    UMI3DCollaborationServer.sendRTC(user, transactionDto, transaction.reliable);
                }
            }
        }

        Dictionary<UMI3DCollaborationUser, Transaction> TransactionToBeSend = new Dictionary<UMI3DCollaborationUser, Transaction>();
        private void Update()
        {
            foreach(var kp in TransactionToBeSend.ToList())
            {
                var user = kp.Key;
                var transaction = kp.Value;
                if(user.status == StatusType.NONE)
                {
                    TransactionToBeSend.Remove(user);
                    continue;
                }
                if (user.status == StatusType.MISSING || user.status == StatusType.CREATED || user.status == StatusType.READY) continue;
                transaction.Simplify();
                var transactionDto = new TransactionDto();
                transactionDto.operations = new List<AbstractOperationDto>(transaction.Operations.Where((op) => { return op.users.Contains(user); }).Select((op) => { return op.ToOperationDto(user); }));
                if (transactionDto.operations.Count > 0)
                {
                    UMI3DCollaborationServer.sendRTC(user, transactionDto, transaction.reliable);
                }
                TransactionToBeSend.Remove(user);
            }

        }
    }
}