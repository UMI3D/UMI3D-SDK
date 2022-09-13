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
using inetum.unityUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using umi3d.cdk.collaboration;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.userCapture;
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
    public class UMI3DForgeServer : UMI3DForgeSocketBase
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.Networking;

        #region Fields

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

        #endregion

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
            UMI3DForgeServer server = new GameObject("UMI3DForgeServer").AddComponent<UMI3DForgeServer>();
            server.ip = ip;
            server.port = port;
            server.masterServerHost = masterServerHost;
            server.masterServerPort = masterServerPort;
            server.natServerHost = natServerHost;
            server.natServerPort = natServerPort;
            server.maxNbPlayer = maxNbPlayer;
            server.connectionPort = connectionPort;
            server.InitTrackingFrameThread();

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
                UMI3DLogger.LogError("NetWorker failed to bind", scope);
                return;
            }

            if (mgr == null && networkManager == null)
            {
                UMI3DLogger.LogWarning("A network manager was not provided, generating a new one instead", scope);
                networkManager = new GameObject("Network Manager");
                mgr = networkManager.AddComponent<UMI3DNetworkManager>();
            }
            else if (mgr == null)
            {
                mgr = Instantiate(networkManager).GetComponent<UMI3DNetworkManager>();
            }

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
                UMI3DLogger.Log($"Player [{player.NetworkId}] timeout", scope);
            });
            playerCount = server.Players.Count;
            UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (user != null)
            {
                MainThreadManager.Run(() =>
                {
                    UMI3DCollaborationServer.Collaboration.ConnectionClose(user, player.NetworkId);
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sender"></param>
        private void PlayerAuthenticated(NetworkingPlayer player, NetWorker sender)
        {
            UMI3DLogger.Log("Player Authenticated", scope);
            //UMI3DLogger.Log($"Player { player.NetworkId } {player.Name} authenticated",scope);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sender"></param>
        private void PlayerAccepted(NetworkingPlayer player, NetWorker sender)
        {
            UMI3DLogger.Log("Player Accepted", scope);
            playerCount = server.Players.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="sender"></param>
        private void PlayerRejected(NetworkingPlayer player, NetWorker sender)
        {
            UMI3DLogger.Log("Player rejected", scope);
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
                UMI3DLogger.Log($"Player [{player.NetworkId}] disconected", scope);
            });
            playerCount = server.Players.Count;
            UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration?.GetUserByNetworkId(player.NetworkId);
            if (user != null)
            {
                MainThreadManager.Run(() =>
                {
                    UMI3DCollaborationServer.Collaboration.ConnectionClose(user, player.NetworkId);
                });
            }
        }


        /// <inheritdoc/>
        protected override void OnSignalingFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);
            UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (dto is StatusDto sts)
            {
                MainThreadManager.Run(() =>
                {
                    UMI3DLogger.Log(sts.status, scope);
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
            UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (user == null)
                return;

            if (UMI3DEnvironment.Instance.useDto)
            {
                var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);

                switch (dto)
                {

                    case common.userCapture.UserCameraPropertiesDto cam:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DEmbodimentManager.Instance.UserCameraReception(cam, user);
                        });
                        break;

                    case common.volume.VolumeUserTransitDto vutdto:
                        MainThreadManager.Run(() =>
                        {
                            VolumeManager.DispatchBrowserRequest(user, vutdto.volumeId, vutdto.direction);
                        });
                        break;

                    case common.ConferenceBrowserRequest conferencedto:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DCollaborationServer.Collaboration.CollaborationRequest(user, conferencedto);
                        });
                        break;

                    default:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DBrowserRequestDispatcher.DispatchBrowserRequest(user, dto);
                        });
                        break;
                }
            }
            else
            {
                var container = new ByteContainer(frame.StreamData.byteArr);
                uint id = UMI3DNetworkingHelper.Read<uint>(container);
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

                    case UMI3DOperationKeys.UserMicrophoneStatus:
                    case UMI3DOperationKeys.UserAvatarStatus:
                    case UMI3DOperationKeys.UserAttentionStatus:
                    case UMI3DOperationKeys.MuteAllMicrophoneStatus:
                    case UMI3DOperationKeys.MuteAllAvatarStatus:
                    case UMI3DOperationKeys.MuteAllAttentionStatus:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DCollaborationServer.Collaboration.CollaborationRequest(user, id, container);
                        });
                        break;
                    case UMI3DOperationKeys.EmoteRequest:
                        MainThreadManager.Run(() =>
                        {
                            var emoteToTriggerId = UMI3DNetworkingHelper.Read<ulong>(container);
                            var trigger = UMI3DNetworkingHelper.Read<bool>(container);
                            UMI3DEmbodimentManager.Instance.DispatchChangeEmoteReception(emoteToTriggerId, user, trigger);
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

        #region Fields

        /// <summary>
        /// Stores all <see cref="UserTrackingFrameDto"/> received from all <see cref="UnityEngine.NetworkPlayer"/>.
        /// </summary>
        Dictionary<NetworkingPlayer, UserTrackingFrameDto> avatarFramesPerPlayer = new Dictionary<NetworkingPlayer, UserTrackingFrameDto>();

        /// <summary>
        /// Collection used to prevent sending the same <see cref="UserTrackingFrameDto"/> twice to the same <see cref="NetworkPlayer"/>.
        /// </summary>
        Dictionary<NetworkingPlayer, Dictionary<NetworkingPlayer, UserTrackingFrameDto>> lastFrameSentToAPlayer = new Dictionary<NetworkingPlayer, Dictionary<NetworkingPlayer, UserTrackingFrameDto>>();

        /// <summary>
        /// Thread used to send trackingFrames.
        /// </summary>
        private Thread sendAvatarFramesThread;

        /// <summary>
        /// If true, all <see cref="UserTrackingFrameDto"/> of <see cref="avatarFramesPerPlayer"/> must be sent to everyone.
        /// </summary>
        private bool forceSendtrackingFrames = false;

        #endregion

        #region Receive

        protected class AvatarFrameEvent : UnityEvent<UserTrackingFrameDto, ulong> { };

        protected static AvatarFrameEvent avatarFrameEvent = new AvatarFrameEvent();

        public static void RequestAvatarListener(UnityAction<common.userCapture.UserTrackingFrameDto, ulong> action, string reason)
        {
            // do something with reason

            avatarFrameEvent.AddListener(action);
        }

        /// <inheritdoc/>
        protected override void OnAvatarFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);
            if (user == null) return;


            UserTrackingFrameDto trackingFrame = null;

            if (UMI3DEnvironment.Instance.useDto)
            {
                var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);

                if (dto is UserTrackingFrameDto readFrame)
                {
                    trackingFrame = readFrame;
                }
            }
            else
            {
                trackingFrame = new UserTrackingFrameDto();

                var container = new ByteContainer(frame.StreamData.byteArr);
                uint id = UMI3DNetworkingHelper.Read<uint>(container);
                if (id == UMI3DOperationKeys.UserTrackingFrame)
                {
                    trackingFrame.userId = UMI3DNetworkingHelper.Read<ulong>(container);
                    trackingFrame.skeletonHighOffset = UMI3DNetworkingHelper.Read<float>(container);
                    trackingFrame.position = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                    trackingFrame.rotation = UMI3DNetworkingHelper.Read<SerializableVector4>(container);
                    trackingFrame.refreshFrequency = UMI3DNetworkingHelper.Read<float>(container);
                    trackingFrame.bones = UMI3DNetworkingHelper.ReadList<common.userCapture.BoneDto>(container);
                }
            }

            if (trackingFrame == null)
                return;

            avatarFrameEvent.Invoke(trackingFrame, server.Time.Timestep);
            MainThreadManager.Run(() =>
            {
                UMI3DEmbodimentManager.Instance.UserTrackingReception(trackingFrame, user.Id());
            });

            lock (avatarFramesPerPlayer)
            {
                if (avatarFramesPerPlayer.ContainsKey(player))
                {
                    avatarFramesPerPlayer[player] = trackingFrame;
                }
                else
                {
                    avatarFramesPerPlayer.Add(player, trackingFrame);
                }
            }
        }

        #endregion

        #region Send

        private void InitTrackingFrameThread()
        {
            UMI3DCollaborationServer.Instance.OnServerStart.AddListener(() => {
                sendAvatarFramesThread = new Thread(new ThreadStart(SendTrackingFramesLoop));
                sendAvatarFramesThread.Start();
            });
            UMI3DCollaborationServer.Instance.OnServerStop.AddListener(() => {
                sendAvatarFramesThread.Abort();
            });

            UMI3DCollaborationServer.Instance.OnUserLeave.AddListener((user) =>
            {
                lock (avatarFramesPerPlayer)
                {
                    var player = avatarFramesPerPlayer.Keys.ToList().Find(p => p.NetworkId == user.Id());
                    Debug.Assert(player != null, "Player null");

                    if (avatarFramesPerPlayer.ContainsKey(player))
                        avatarFramesPerPlayer.Remove(player);
                }
            });

            UMI3DCollaborationServer.Instance.OnUserActive.AddListener((user) => forceSendtrackingFrames = true);
        }

        /// <summary>
        /// Sends <see cref="UserTrackingFrameDto"/> every tick.
        /// </summary>
        private void SendTrackingFramesLoop()
        {
            while (true)
            {
                SendTrackingFrames();

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Sends <see cref="UserTrackingFrameDto"/> to every player if they should received them.
        /// </summary>
        private void SendTrackingFrames()
        {
            lock (avatarFramesPerPlayer)
            {
                foreach (var avatarFrameEntry in avatarFramesPerPlayer)
                {
                    UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(avatarFrameEntry.Key.NetworkId);

                    if (user == null)
                        continue;

                    List<UserTrackingFrameDto> frames = GetTrackingFrameToSend(avatarFrameEntry.Key, user);

                    if (frames.Count == 0)
                        continue;

                    Binary message = null;
                    if (UMI3DEnvironment.Instance.useDto)
                    {
                        message = new Binary(server.Time.Timestep, false, (new UMI3DDtoListDto<UserTrackingFrameDto>() { values = frames}).ToBson(),
                        BeardedManStudios.Forge.Networking.Receivers.Target, (int)DataChannelTypes.Tracking, false);
                    }
                    else
                    {
                        message = new Binary(server.Time.Timestep, false, UMI3DNetworkingHelper.WriteCollection(frames).ToBytes(),
                        BeardedManStudios.Forge.Networking.Receivers.Target, (int)DataChannelTypes.Tracking, false);
                    }
                    server.Send(avatarFrameEntry.Key, message, forceSendtrackingFrames);
                }

                if (forceSendtrackingFrames)
                    forceSendtrackingFrames = false;
            }
        }

        /// <summary>
        /// Returns all <see cref="UserTrackingFrameDto"/> that <paramref name="to"/> should received.
        /// </summary>
        /// <param name="to"></param>
        private List<UserTrackingFrameDto> GetTrackingFrameToSend(NetworkingPlayer to, UMI3DUser user)
        {
            ulong time = server.Time.Timestep; //introduce wrong time. TB tested with frame.timestep

            List<UserTrackingFrameDto> frames = new List<UserTrackingFrameDto>();

            if (to == null || user == null)
                return frames;

            foreach (var other in avatarFramesPerPlayer)
            {
                if (user.Id() == other.Key.NetworkId)
                    continue;

                Debug.LogError("TODO : check relay");

                if (forceSendtrackingFrames)
                {
                    frames.Add(other.Value);
                }
                else if (ShouldRelay((int)(int)DataChannelTypes.Tracking, to, other.Key, time, BeardedManStudios.Forge.Networking.Receivers.OthersProximity))
                {
                    if (!lastFrameSentToAPlayer.ContainsKey(to))
                    {
                        lastFrameSentToAPlayer.Add(to, new Dictionary<NetworkingPlayer, UserTrackingFrameDto>());
                    }

                    if (!lastFrameSentToAPlayer[to].ContainsKey(other.Key))
                    {
                        lastFrameSentToAPlayer[to][other.Key] = other.Value;
                        frames.Add(other.Value);
                    }
                    else
                    {
                        if (lastFrameSentToAPlayer[to][other.Key] != other.Value)
                        {
                            frames.Add(other.Value);
                            lastFrameSentToAPlayer[to][other.Key] = other.Value;
                        }
                    }
                }
            }

            return frames;
        }

        #endregion

        #endregion

        #region video

        /// <inheritdoc/>
        protected override void OnVideoFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            MainThreadManager.Run(() =>
            {
                UMI3DLogger.LogError("Video frame not implemented!", scope);
            });
        }

        #endregion

        #region VoIP

        private static readonly List<UMI3DCollaborationUser> VoipInterceptionList = new List<UMI3DCollaborationUser>();

        public delegate void AudioFrame(UMI3DCollaborationUser user, Binary frame);
        public static AudioFrame OnAudioFrame;

        /// <inheritdoc/>
        protected override void OnVoIPFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(player.NetworkId);

            if (VoipInterceptionList.Contains(user))
            {
                OnAudioFrame(user, frame);
                return;
            }

            if (user.Avatar != null && user.Avatar.RelayRoom != null)
            {
                RelayVolume relayVolume = RelayVolume.relaysVolumes[user.Avatar.RelayRoom.Id()];

                if (relayVolume != null && relayVolume.HasStrategyFor(DataChannelTypes.VoIP))
                {
                    MainThreadManager.Run(() =>
                    {
                        relayVolume.RelayVoIPRequest(user.Avatar, user, frame.StreamData.byteArr, user, Receivers.Others);
                    });
                }
                else
                {
                    RelayMessage(player, frame, false);
                }
            }
            else
            {
                RelayMessage(player, frame, false);
            }
        }

        #endregion

        #region proximity_relay

        /// <summary>
        /// 
        /// </summary>
        private static readonly List<BeardedManStudios.Forge.Networking.Receivers> Proximity = new List<BeardedManStudios.Forge.Networking.Receivers> { BeardedManStudios.Forge.Networking.Receivers.AllProximity, BeardedManStudios.Forge.Networking.Receivers.AllProximityGrid, BeardedManStudios.Forge.Networking.Receivers.OthersProximity, BeardedManStudios.Forge.Networking.Receivers.OthersProximityGrid };

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
            get => maxProximityRelay == 0 ? 0 : 1000 / maxProximityRelay;
            set { if (value <= 0) maxProximityRelay = 0; else maxProximityRelay = 1000 / value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ulong maxProximityRelayFPS
        {
            get => minProximityRelay == 0 ? 0 : 1000 / minProximityRelay;
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
        protected void RelayMessage(NetworkingPlayer player, Binary frame, bool forceSending, BeardedManStudios.Forge.Networking.Receivers strategy = BeardedManStudios.Forge.Networking.Receivers.Others)
        {
            ulong time = server.Time.Timestep; //introduce wrong time. TB tested with frame.timestep
            var message = new Binary(time, false, frame.StreamData, BeardedManStudios.Forge.Networking.Receivers.Target, frame.GroupId, frame.IsReliable);
            //message.SetSender(player);
            if (UMI3DCollaborationServer.Collaboration?.GetUserByNetworkId(player.NetworkId)?.status == StatusType.ACTIVE)
            {
                lock (server.Players)
                {
                    foreach (NetworkingPlayer p in server.Players)
                    {
                        if (forceSending || ShouldRelay(frame.GroupId, player, p, time, strategy))
                        {
                            RememberRelay(player, p, frame.GroupId, time);
                            server.Send(p, message, frame.IsReliable || forceSending);
                        }
                    }
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
                    ulong currentDelay = GetCurrentDelay(from, to);
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
            UMI3DCollaborationUser user1 = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(from.NetworkId);
            UMI3DCollaborationUser user2 = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(to.NetworkId);
            float dist = Vector3.Distance(user1.Avatar.objectPosition.GetValue(user2), user2.Avatar.objectPosition.GetValue(user2));
            float coeff = 0f;
            if (dist > startProximityAt && dist < proximityCutout)
            {
                coeff = (dist - startProximityAt) / (proximityCutout - startProximityAt);
            }
            else if (dist >= proximityCutout)
            {
                coeff = 1f;
            }

            return (ulong)Mathf.RoundToInt(1000 / Mathf.Floor(((1f - coeff) * maxFPSRelay) + (coeff * minFPSRelay)));
        }

        //relayMemory[p1][p2][gi] = a ulong corresponding to the last time player p1 sent a message to p2 in the gi channel
        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<uint, Dictionary<uint, Dictionary<int, ulong>>> relayMemory = new Dictionary<uint, Dictionary<uint, Dictionary<int, ulong>>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="groupId"></param>
        /// <param name="time"></param>
        private void RememberRelay(NetworkingPlayer from, NetworkingPlayer to, int groupId, ulong time)
        {
            uint p1 = from.NetworkId, p2 = to.NetworkId;
            if (!relayMemory.ContainsKey(p1))
                relayMemory.Add(p1, new Dictionary<uint, Dictionary<int, ulong>>());
            Dictionary<uint, Dictionary<int, ulong>> dicP1 = relayMemory[p1];

            if (!dicP1.ContainsKey(p2))
                dicP1.Add(p2, new Dictionary<int, ulong>());
            Dictionary<int, ulong> dicP2 = dicP1[p2];

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
        private ulong GetLastRelay(NetworkingPlayer from, NetworkingPlayer to, int groupId)
        {
            uint p1 = from.NetworkId, p2 = to.NetworkId;
            //no relay from p1
            if (!relayMemory.ContainsKey(p1))
                return 0;
            Dictionary<uint, Dictionary<int, ulong>> dicP1 = relayMemory[p1];
            //no relay from p1 to P2
            if (!dicP1.ContainsKey(p2))
                return 0;
            Dictionary<int, ulong> dicP2 = dicP1[p2];
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

            var bin = new Binary(timestep, isTcpClient, data, BeardedManStudios.Forge.Networking.Receivers.Target, channel, isTcp);
            try
            {
                server.Send(player, bin, isReliable);
            }
            catch (Exception e)
            {
                MainThreadManager.Run(() =>
                {
                    UMI3DLogger.Log($"Error on send binary to {player?.NetworkId} (from {bin?.Sender?.NetworkId}) on channel {channel} [{e}]", scope);
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
            var bin = new Binary(timestep, isTcpClient, data, BeardedManStudios.Forge.Networking.Receivers.Others, channel, isTcp);
            server.Send(bin, isReliable);
        }

        #region MonoBehaviour

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            inetum.unityUtils.QuittingManager.OnApplicationIsQuitting.AddListener(ApplicationQuit);
            // If not using TCP
            // Should it be done before Host() ???
            NetWorker.PingForFirewall(port);
        }

        public static void SetUserVOIPInterception(UMI3DCollaborationUser user, bool intercept)
        {
            if (intercept)
            {
                if (!VoipInterceptionList.Contains(user))
                {
                    VoipInterceptionList.Add(user);
                }
            }
            else
                VoipInterceptionList.Remove(user);
        }


        /// <summary>
        /// 
        /// </summary>
        private void ApplicationQuit()
        {
            Stop();
        }

        #endregion
    }
}
