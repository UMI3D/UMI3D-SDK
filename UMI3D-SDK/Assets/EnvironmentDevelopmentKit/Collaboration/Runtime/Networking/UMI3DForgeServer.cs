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

using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.SimpleJSON;
using System;
using System.Collections.Generic;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.edk.interaction;
using umi3d.edk.userCapture;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// 
    /// </summary>
    public class UMI3DForgeServer : ForgeSocketBase
    {
        /// <summary>
        /// 
        /// </summary>
        public int maxNbPlayer = 64;

        /// <summary>
        /// 
        /// </summary>
        private UDPServer server;

        /// <inheritdoc/>
        public override NetWorker GetNetWorker()
        {
            return server;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="masterServerHost"></param>
        /// <param name="masterServerPort"></param>
        /// <param name="natServerHost"></param>
        /// <param name="natServerPort"></param>
        /// <param name="maxNbPlayer"></param>
        /// <returns></returns>
        public static UMI3DForgeServer Create(string ip = "127.0.0.1", ushort port = 15937, string masterServerHost = "", ushort masterServerPort = 15940, string natServerHost = "", ushort natServerPort = 15941, int maxNbPlayer = 64)
        {
            UMI3DForgeServer server = (new GameObject("UMI3DForgeServer")).AddComponent<UMI3DForgeServer>();
            server.ip = ip;
            server.port = port;
            server.masterServerHost = masterServerHost;
            server.masterServerPort = masterServerPort;
            server.natServerHost = natServerHost;
            server.natServerPort = natServerPort;
            server.maxNbPlayer = maxNbPlayer;
            return server;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticator"></param>
        public void Host(IUserAuthenticator authenticator = null)
        {
            server = new UDPServer(maxNbPlayer);

            if (authenticator != null)
                server.SetUserAuthenticator(authenticator);

            server.binaryMessageReceived += ReadBinary;
            server.onPingPong += SetRoundTripLatency;
            server.playerAccepted += PlayerAccepted;
            server.playerAuthenticated += PlayerAuthenticated;
            server.playerRejected += PlayerRejected;
            server.playerTimeout += PlayerTimeout;
            server.playerDisconnected += PlayerDisconnected;

            if (natServerHost.Trim().Length == 0)
                server.Connect(ip, port);
            else
                server.Connect(port: port, natHost: natServerHost, natPort: natServerPort);


            //When connected

            if (!server.IsBound)
            {
                Debug.LogError("NetWorker failed to bind");
                return;
            }

            if (mgr == null && networkManager == null)
            {
                Debug.LogWarning("A network manager was not provided, generating a new one instead");
                networkManager = new GameObject("Network Manager");
                mgr = networkManager.AddComponent<NetworkManager>();
            }
            else if (mgr == null)
                mgr = Instantiate(networkManager).GetComponent<NetworkManager>();

            // If we are using the master server we need to get the registration data
            JSONNode masterServerData = null;
            if (!string.IsNullOrEmpty(masterServerHost))
            {
                string serverId = "myGame";
                string serverName = "Forge Game";
                string type = "Deathmatch";
                string mode = "Teams";
                string comment = "Demo comment...";

                masterServerData = mgr.MasterServerRegisterData(server, serverId, serverName, type, mode, comment);
            }
            mgr.Initialize(server, masterServerHost, masterServerPort, masterServerData);
            NetworkObject.Flush(server); //Called because we are already in the correct scene!
            playerCount = server.Players.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (server != null) server.Disconnect(true);
        }

        #region signaling

        public void SendSignalingMessage(NetworkingPlayer player, UMI3DDto dto)
        {
            SendBinaryDataTo((int)DataChannelTypes.Signaling, player, dto.ToBson(), true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sender"></param>
        private void PlayerTimeout(NetworkingPlayer player, NetWorker sender)
        {
            Debug.Log("Player " + player.NetworkId + " timed out");
            //TODO
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sender"></param>
        private void PlayerAuthenticated(NetworkingPlayer player, NetWorker sender)
        {
            Debug.Log($"Player { player.NetworkId } {player.Name} authenticated");
            //TODO
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sender"></param>
        private void PlayerAccepted(NetworkingPlayer player, NetWorker sender)
        {
            playerCount = server.Players.Count;
            //TODO
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sender"></param>
        private void PlayerRejected(NetworkingPlayer player, NetWorker sender)
        {
            Debug.Log("Player rejected");
            //TODO
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sender"></param>
        private void PlayerDisconnected(NetworkingPlayer player, NetWorker sender)
        {
            Debug.Log("Player " + player.NetworkId + " disconnected");
            playerCount = server.Players.Count;
            var user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if(user != null)
                MainThreadManager.Run(() =>
                {
                    UMI3DCollaborationServer.Collaboration.ConnectionClose(user.Id());
                });
        }


        /// <inheritdoc/>
        protected override void OnSignalingFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);
            var user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (dto is StatusDto sts)
            {
                Debug.Log(sts.status);
                UMI3DCollaborationServer.Collaboration.OnStatusUpdate(user.Id(), sts.status);
            }
        }


        #endregion

        #region data



        public void SendData(NetworkingPlayer player, UMI3DDto dto, bool reliable)
        {
            SendBinaryDataTo((int)DataChannelTypes.Data, player, dto.ToBson(), reliable);
        }

        /// <inheritdoc/>
        protected override void OnDataFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);
            Debug.Log(dto);
            var user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (user == null)
                return;

            if (dto is common.userCapture.UserCameraPropertiesDto camera)
            {
                MainThreadManager.Run(() =>
                {
                    UMI3DEmbodimentManager.Instance.UserCameraReception(camera, user);
                });
            }
            else
                MainThreadManager.Run(() =>
                {
                    UMI3DBrowserRequestDispatcher.DispatchBrowserRequest(user, dto);
                });
        }

        #endregion

        #region avatar
        /// <inheritdoc/>
        protected override void OnAvatarFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);

            if (dto is common.userCapture.UserTrackingFrameDto trackingFrame)
            {
                MainThreadManager.Run(() =>
                {
                    UMI3DEmbodimentManager.Instance.UserTrackingReception(trackingFrame);
                });
                RelayMessage(player, frame,Receivers.OthersProximity);
            }
        }

        #endregion

        #region video

        /// <inheritdoc/>
        protected override void OnVideoFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            Debug.LogError("Video frame not implemented!");
        }

        #endregion

        #region VoIP

        /// <inheritdoc/>
        protected override void OnVoIPFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            //TODO: Audio rooms, Min distance for spatialized audio conf.
            MainThreadManager.Run(() =>
            {
                Debug.Log($"audio Frame from {player.NetworkId}");
            });
            RelayMessage(player, frame);
        }

        #endregion

        #region proximity_relay

        /// <summary>
        /// 
        /// </summary>
        static readonly List<Receivers> Proximity = new List<Receivers> { Receivers.AllProximity, Receivers.AllProximityGrid, Receivers.OthersProximity, Receivers.OthersProximityGrid };

        /// <summary>
        /// 
        /// </summary>
        protected ulong minProximityRelay = 200;

        /// <summary>
        /// 
        /// </summary>
        protected ulong maxProximityRelay = 1000;

        /// <summary>
        /// 
        /// </summary>
        public ulong minProximityRelayFPS
        {
            get { return maxProximityRelay == 0 ? 0 : 1000 / maxProximityRelay; }
            set { if (value <= 0) maxProximityRelay = 0; else maxProximityRelay = 1000 / value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ulong maxProximityRelayFPS
        {
            get { return minProximityRelay == 0 ? 0 : 1000 / minProximityRelay; }
            set { if (value <= 0) minProximityRelay = 0; else minProximityRelay = 1000 / value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool proximityRelay = true;

        /// <summary>
        /// 
        /// </summary>
        public float startProximityAt = 3f;

        /// <summary>
        /// 
        /// </summary>
        public float proximityCutout = 20f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="frame"></param>
        /// <param name="strategy"></param>
        protected void RelayMessage(NetworkingPlayer player, Binary frame, Receivers strategy = Receivers.Others)
        {
            ulong time = server.Time.Timestep; //introduce wrong time. TB tested with frame.timestep
            Binary message = new Binary(time, false, frame.StreamData, Receivers.Target, frame.GroupId, frame.IsReliable);
            lock (server.Players)
            {
                foreach (NetworkingPlayer p in server.Players)
                    if (ShouldRelay(frame.GroupId, player, p, time, strategy))
                    {
                        RememberRelay(player, p, frame.GroupId, time);
                        server.Send(p, message, frame.IsReliable);
                    }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="timestep"></param>
        /// <param name="strategy"></param>
        /// <returns></returns>
        protected bool ShouldRelay(int groupId, NetworkingPlayer from, NetworkingPlayer to, ulong timestep, Receivers strategy)
        {
            if (to.IsHost || from == to)
                return false;
            if (Proximity.Contains(strategy))
            {
                ulong last = GetLastRelay(from, to, groupId);
                if (last > 0)
                {
                    ulong diff = timestep - last;
                    if (diff < GetCurrentDelay(from, to))
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        protected ulong GetCurrentDelay(NetworkingPlayer from, NetworkingPlayer to)
        {
            var user1 = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(from.NetworkId);
            var user2 = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(to.NetworkId);
            float dist = Vector3.Distance(user1.Avatar.objectPosition.GetValue(user2), user2.Avatar.objectPosition.GetValue(user2));
            float coeff = 0f;
            if (dist > startProximityAt && dist < proximityCutout)
            {
                coeff = (dist - startProximityAt) / (proximityCutout - startProximityAt);
            }
            else if (dist >= proximityCutout)
                coeff = 0f;
            return (ulong)Mathf.RoundToInt((1f - coeff) * minProximityRelay + coeff * maxProximityRelay);
        }

        //relayMemory[p1][p2][gi] = a ulong corresponding to the last time player p1 sent a message to p2 in the gi channel
        /// <summary>
        /// 
        /// </summary>
        Dictionary<uint, Dictionary<uint, Dictionary<int, ulong>>> relayMemory = new Dictionary<uint, Dictionary<uint, Dictionary<int, ulong>>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="groupId"></param>
        /// <param name="time"></param>
        void RememberRelay(NetworkingPlayer from, NetworkingPlayer to, int groupId, ulong time)
        {
            uint p1 = from.NetworkId, p2 = to.NetworkId;
            if (!relayMemory.ContainsKey(p1))
                relayMemory.Add(p1, new Dictionary<uint, Dictionary<int, ulong>>());
            var dicP1 = relayMemory[p1];

            if (!dicP1.ContainsKey(p2))
                dicP1.Add(p2, new Dictionary<int, ulong>());
            var dicP2 = dicP1[p2];

            if (dicP2.ContainsKey(groupId))
                dicP2[groupId] = time;
            else
                dicP2.Add(groupId, time);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        ulong GetLastRelay(NetworkingPlayer from, NetworkingPlayer to, int groupId)
        {
            uint p1 = from.NetworkId, p2 = to.NetworkId;
            //no relay from p1
            if (!relayMemory.ContainsKey(p1))
                return 0;
            var dicP1 = relayMemory[p1];
            //no relay from p1 to P2
            if (!dicP1.ContainsKey(p2))
                return 0;
            var dicP2 = dicP1[p2];
            //last telay from p1 to p2 on channel groupId
            if (dicP2.ContainsKey(groupId))
                return dicP2[groupId];
            else
                return 0;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <param name="isReliable"></param>
        protected void SendBinaryDataTo(int channel, NetworkingPlayer player, byte[] data, bool isReliable)
        {
            ulong timestep = NetworkManager.Instance.Networker.Time.Timestep;
            bool isTcpClient = NetworkManager.Instance.Networker is TCPClient;
            bool isTcp = NetworkManager.Instance.Networker is BaseTCP;

            Binary bin = new Binary(timestep, isTcpClient, data, Receivers.Target, channel, isTcp);
            server.Send(player, bin, isReliable);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <param name="isReliable"></param>
        protected void SendBinaryDataToAll(int channel, byte[] data, bool isReliable)
        {
            ulong timestep = NetworkManager.Instance.Networker.Time.Timestep;
            bool isTcpClient = NetworkManager.Instance.Networker is TCPClient;
            bool isTcp = NetworkManager.Instance.Networker is BaseTCP;
            Binary bin = new Binary(timestep, isTcpClient, data, Receivers.Others, channel, isTcp);
            server.Send(bin, isReliable);
        }

        #region MonoBehaviour

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            // If not using TCP
            // Should it be done before Host() ???
            NetWorker.PingForFirewall(port);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnApplicationQuit()
        {
            Stop();
        }

        #endregion
    }

}
