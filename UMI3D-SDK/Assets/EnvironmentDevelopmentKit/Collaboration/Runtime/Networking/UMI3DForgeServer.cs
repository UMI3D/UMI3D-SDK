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
using UnityEngine.Events;

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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sender"></param>
        private void PlayerAuthenticated(NetworkingPlayer player, NetWorker sender)
        {
            //Debug.Log($"Player { player.NetworkId } {player.Name} authenticated");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sender"></param>
        private void PlayerAccepted(NetworkingPlayer player, NetWorker sender)
        {
            playerCount = server.Players.Count;
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
            MainThreadManager.Run(() =>
            {
                Debug.Log("Player " + player.NetworkId + " disconnected");
            });
            playerCount = server.Players.Count;
            var user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (user != null)
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

        protected class AvatarFrameEvent : UnityEvent<common.userCapture.UserTrackingFrameDto, ulong> { };

        protected static AvatarFrameEvent avatarFrameEvent = new AvatarFrameEvent();

        public static void requestAvatarListener(UnityAction<common.userCapture.UserTrackingFrameDto, ulong> action, string reason)
        {
            // do something with reason

            avatarFrameEvent.AddListener(action);
        }

        /// <inheritdoc/>
        protected override void OnAvatarFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);

            if (dto is common.userCapture.UserTrackingFrameDto trackingFrame)
            {
                avatarFrameEvent.Invoke((dto as common.userCapture.UserTrackingFrameDto), server.Time.Timestep);
                MainThreadManager.Run(() =>
                {
                    UMI3DEmbodimentManager.Instance.UserTrackingReception(trackingFrame);
                });

                UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);

                if (user.Avatar != null && user.Avatar.room != null)
                {
                    RelayVolume relayVolume = RelayVolume.relaysVolumes[user.Avatar.room.VolumeId()];

                    if (relayVolume != null)
                        MainThreadManager.Run(() =>
                        {
                            relayVolume.RelayTrackingRequest(user.Avatar, frame.StreamData.byteArr, user, Receivers.Others);
                        });
                }
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
            UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (user.Avatar != null && user.Avatar.room != null)
            {
                RelayVolume relayVolume = RelayVolume.relaysVolumes[user.Avatar.room.VolumeId()];

                if (relayVolume != null)
                    MainThreadManager.Run(() =>
                    {
                        relayVolume.RelayVoIPRequest(user.Avatar, frame.StreamData.byteArr, user, Receivers.Others);
                    });
            }
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

            Binary bin = new Binary(timestep, isTcpClient, data, BeardedManStudios.Forge.Networking.Receivers.Target, channel, isTcp);

            try
            {
                server.Send(player, bin, isReliable);
            }
            catch (Exception e)
            {
                MainThreadManager.Run(() =>
                {
                    Debug.Log($"Error on send binary to {player.NetworkId} on channel {channel} [{e}]");
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <param name="isRealiable"></param>
        public void RelayBinaryDataTo(int channel, NetworkingPlayer player, byte[] data, bool isRealiable)
        {
            SendBinaryDataTo(channel, player, data, isRealiable);
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
            Binary bin = new Binary(timestep, isTcpClient, data, BeardedManStudios.Forge.Networking.Receivers.Others, channel, isTcp);
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
