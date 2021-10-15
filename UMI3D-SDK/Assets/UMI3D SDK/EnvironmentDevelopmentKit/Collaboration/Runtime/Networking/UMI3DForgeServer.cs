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
using umi3d.edk.volume;
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



        /// <summary>
        /// Forge server environmentType
        /// </summary>
        public string environmentType = "";

        /// <summary>
        /// Port given to clients if they find this server via a master server.
        /// </summary>
        public ushort connectionPort;

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
        public static UMI3DForgeServer Create(string ip = "127.0.0.1", ushort connectionPort = 50043, ushort port = 15937, string masterServerHost = "", ushort masterServerPort = 15940, string natServerHost = "", ushort natServerPort = 15941, int maxNbPlayer = 64)
        {
            UMI3DForgeServer server = (new GameObject("UMI3DForgeServer")).AddComponent<UMI3DForgeServer>();
            server.ip = ip;
            server.port = port;
            server.masterServerHost = masterServerHost;
            server.masterServerPort = masterServerPort;
            server.natServerHost = natServerHost;
            server.natServerPort = natServerPort;
            server.maxNbPlayer = maxNbPlayer;
            server.connectionPort = connectionPort;
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
                mgr = networkManager.AddComponent<UMI3DNetworkManager>();
            }
            else if (mgr == null)
                mgr = Instantiate(networkManager).GetComponent<UMI3DNetworkManager>();

            // If we are using the master server we need to get the registration data
            JSONNode masterServerData = null;
            if (!string.IsNullOrEmpty(masterServerHost))
            {
                string serverId = UMI3DCollaborationServer.Instance.sessionId;
                string serverName = UMI3DCollaborationEnvironment.Instance.environmentName;// ok
                string type = string.IsNullOrEmpty(environmentType) ? UMI3DCollaborationEnvironment.Instance.environmentName : environmentType;
                string mode = string.IsNullOrEmpty(UMI3DCollaborationServer.Instance.iconServerUrl) ? "public/picture.png" : UMI3DCollaborationServer.Instance.iconServerUrl;
                string comment = UMI3DCollaborationServer.Instance.descriptionComment;

                masterServerData = (mgr as UMI3DNetworkManager).MasterServerRegisterData(server, connectionPort.ToString(), serverId, serverName, type, mode, comment);
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
            MainThreadManager.Run(() =>
            {
                Debug.Log($"Player [{player.NetworkId}] timeout");
            });
            playerCount = server.Players.Count;
            var user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (user != null)
                MainThreadManager.Run(() =>
                {
                    UMI3DCollaborationServer.Collaboration.ConnectionClose(user);
                });
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
                Debug.Log($"Player [{player.NetworkId}] disconected");
            });
            playerCount = server.Players.Count;
            var user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (user != null)
                MainThreadManager.Run(() =>
                {
                    UMI3DCollaborationServer.Collaboration.ConnectionClose(user);
                });
        }


        /// <inheritdoc/>
        protected override void OnSignalingFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);
            var user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (dto is StatusDto sts)
            {
                MainThreadManager.Run(() =>
                {
                    Debug.Log(sts.status);
                });
                UMI3DCollaborationServer.Collaboration.OnStatusUpdate(user.Id(), sts.status);
            }
        }
        #endregion

        #region data



        public void SendData(NetworkingPlayer player, byte[] data, bool reliable)
        {
            SendBinaryDataTo((int)DataChannelTypes.Data, player, data, reliable);
        }

        /// <inheritdoc/>
        protected override void OnDataFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            var user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (user == null)
                return;

            if (UMI3DEnvironment.Instance.useDto)
            {
                var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);

                if (dto is common.userCapture.UserCameraPropertiesDto cam)
                {
                    MainThreadManager.Run(() =>
                    {
                        UMI3DEmbodimentManager.Instance.UserCameraReception(cam, user);
                    });
                }
                else if (dto is common.volume.VolumeUserTransitDto vutdto) 
                {
                    MainThreadManager.Run(() =>
                    {
                        VolumeManager.DispatchBrowserRequest(user, vutdto.volumeId, vutdto.direction);
                    });
                }
                else
                {
                    MainThreadManager.Run(() =>
                    {
                        UMI3DBrowserRequestDispatcher.DispatchBrowserRequest(user, dto);
                    });
                }
            }
            else
            {
                var container = new ByteContainer(frame.StreamData.byteArr);
                var id = UMI3DNetworkingHelper.Read<uint>(container);
                switch (id)
                {
                    case UMI3DOperationKeys.UserCameraProperties:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DEmbodimentManager.Instance.UserCameraReception(id, container, user);
                        });
                        break;

                    case UMI3DOperationKeys.VolumeUserTransit: //add here future other volume related keys.
                        MainThreadManager.Run(() =>
                        {
                            VolumeManager.DispatchBrowserRequest(user, id, container);
                        });
                        break;

                    default:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DBrowserRequestDispatcher.DispatchBrowserRequest(user, id, container);
                        });
                        break;
                }
                
            }
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
            UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (UMI3DEnvironment.Instance.useDto)
            {
                var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);

                if (dto is common.userCapture.UserTrackingFrameDto trackingFrame)
                {
                    avatarFrameEvent.Invoke(trackingFrame, server.Time.Timestep);
                    MainThreadManager.Run(() =>
                    {
                        UMI3DEmbodimentManager.Instance.UserTrackingReception(trackingFrame, user.Id());
                    });

                    if (user.Avatar != null && user.Avatar.RelayRoom != null)
                    {
                        RelayVolume relayVolume = RelayVolume.relaysVolumes[user.Avatar.RelayRoom.Id()];

                        if (relayVolume != null)
                            MainThreadManager.Run(() =>
                            {
                                relayVolume.RelayTrackingRequest(user.Avatar, user, frame.StreamData.byteArr, user, Receivers.Others);
                            });
                        else
                            RelayMessage(player, frame, BeardedManStudios.Forge.Networking.Receivers.OthersProximity);
                    }
                    else
                        RelayMessage(player, frame, BeardedManStudios.Forge.Networking.Receivers.OthersProximity);
                }
            }
            else
            {
                var trackingFrame = new common.userCapture.UserTrackingFrameDto();

                var container = new ByteContainer(frame.StreamData.byteArr);
                var id = UMI3DNetworkingHelper.Read<uint>(container);
                if (id == UMI3DOperationKeys.UserTrackingFrame)
                {
                    trackingFrame.userId = UMI3DNetworkingHelper.Read<ulong>(container);
                    trackingFrame.position = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                    trackingFrame.rotation = UMI3DNetworkingHelper.Read<SerializableVector4>(container);
                    trackingFrame.refreshFrequency = UMI3DNetworkingHelper.Read<float>(container);
                    trackingFrame.bones = UMI3DNetworkingHelper.ReadList<common.userCapture.BoneDto>(container);

                    avatarFrameEvent.Invoke(trackingFrame, server.Time.Timestep);

                    MainThreadManager.Run(() =>
                    {
                        UMI3DEmbodimentManager.Instance.UserTrackingReception(trackingFrame, user.Id());
                    });

                    if (user.Avatar != null && user.Avatar.RelayRoom != null)
                    {
                        RelayVolume relayVolume = RelayVolume.relaysVolumes[user.Avatar.RelayRoom.Id()];

                        if (relayVolume != null)
                            MainThreadManager.Run(() =>
                            {
                                relayVolume.RelayTrackingRequest(user.Avatar, user, frame.StreamData.byteArr, user, Receivers.Others);
                            });
                        else
                            RelayMessage(player, frame, BeardedManStudios.Forge.Networking.Receivers.OthersProximity);
                    }
                    else
                        RelayMessage(player, frame, BeardedManStudios.Forge.Networking.Receivers.OthersProximity);

                }
            }
        }

        #endregion

        #region video

        /// <inheritdoc/>
        protected override void OnVideoFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            MainThreadManager.Run(() =>
            {
                Debug.LogError("Video frame not implemented!");
            });
        }

        #endregion

        #region VoIP

        /// <inheritdoc/>
        protected override void OnVoIPFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {

            UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (user.Avatar != null && user.Avatar.RelayRoom != null)
            {
                RelayVolume relayVolume = RelayVolume.relaysVolumes[user.Avatar.RelayRoom.Id()];

                if (relayVolume != null)
                    MainThreadManager.Run(() =>
                    {
                        relayVolume.RelayVoIPRequest(user.Avatar, user, frame.StreamData.byteArr, user, Receivers.Others);
                    });
                else
                    RelayMessage(player, frame);
            }
            else
                RelayMessage(player, frame);
        }

        #endregion

        #region proximity_relay

        /// <summary>
        /// 
        /// </summary>
        static readonly List<BeardedManStudios.Forge.Networking.Receivers> Proximity = new List<BeardedManStudios.Forge.Networking.Receivers> { BeardedManStudios.Forge.Networking.Receivers.AllProximity, BeardedManStudios.Forge.Networking.Receivers.AllProximityGrid, BeardedManStudios.Forge.Networking.Receivers.OthersProximity, BeardedManStudios.Forge.Networking.Receivers.OthersProximityGrid };

        /// <summary>
        /// 
        /// </summary>
        protected ulong minProximityRelay = 200;

        protected uint maxFPSRelay = 5;

        /// <summary>
        /// 
        /// </summary>
        protected ulong maxProximityRelay = 1000;

        protected uint minFPSRelay = 1;

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
        protected void RelayMessage(NetworkingPlayer player, Binary frame, BeardedManStudios.Forge.Networking.Receivers strategy = BeardedManStudios.Forge.Networking.Receivers.Others)
        {
            ulong time = server.Time.Timestep; //introduce wrong time. TB tested with frame.timestep
            Binary message = new Binary(time, false, frame.StreamData, BeardedManStudios.Forge.Networking.Receivers.Target, frame.GroupId, frame.IsReliable);
            //message.SetSender(player);
            if (UMI3DCollaborationServer.Collaboration?.GetUserByNetworkId(player.NetworkId)?.status == StatusType.ACTIVE)
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
        protected bool ShouldRelay(int groupId, NetworkingPlayer from, NetworkingPlayer to, ulong timestep, BeardedManStudios.Forge.Networking.Receivers strategy)
        {
            if (to.IsHost || from == to || UMI3DCollaborationServer.Collaboration?.GetUserByNetworkId(to.NetworkId)?.status != StatusType.ACTIVE)
                return false;
            if (Proximity.Contains(strategy))
            {
                ulong last = GetLastRelay(from, to, groupId);
                if (last > 0)
                {
                    ulong diff = timestep - last;
                    var currentDelay = GetCurrentDelay(from, to);
                    if (diff < currentDelay)
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
                coeff = 1f;
            return (ulong)Mathf.RoundToInt(1000 / Mathf.Floor((1f - coeff) * maxFPSRelay + coeff * minFPSRelay));
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

            Binary bin = new Binary(timestep, isTcpClient, data, BeardedManStudios.Forge.Networking.Receivers.Target, channel, isTcp);
            try
            {
                server.Send(player, bin, isReliable);
            }
            catch (Exception e)
            {
                MainThreadManager.Run(() =>
                {
                    Debug.Log($"Error on send binary to {player.NetworkId} (from {bin.Sender?.NetworkId}) on channel {channel} [{e}]");
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
