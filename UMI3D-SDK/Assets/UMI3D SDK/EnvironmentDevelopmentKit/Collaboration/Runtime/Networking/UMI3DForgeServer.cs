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

        UMI3DTrackingRelay trackingRelay;

        object timeLock = new object();
        public ulong time {
            get
            {
                lock (timeLock)
                    return server.Time.Timestep;
            }
        }

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
            server.trackingRelay = new UMI3DTrackingRelay(server);

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

                    case common.VehicleConfirmation vConfirmation:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DEmbodimentManager.Instance.ConfirmEmbarkment(vConfirmation, user);
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
                var container = new ByteContainer(frame);
                uint id = UMI3DNetworkingHelper.Read<uint>(container);
                switch (id)
                {
                    case UMI3DOperationKeys.UserCameraProperties:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DEmbodimentManager.Instance.UserCameraReception(id, container, user);
                        });
                        break;

                    case UMI3DOperationKeys.VehicleConfirmation:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DEmbodimentManager.Instance.ConfirmEmbarkment(id, container, user);
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

                var container = new ByteContainer(frame);
                uint id = UMI3DNetworkingHelper.Read<uint>(container);
                if (id == UMI3DOperationKeys.UserTrackingFrame)
                {
                    trackingFrame.userId = UMI3DNetworkingHelper.Read<ulong>(container);
                    trackingFrame.parentId = UMI3DNetworkingHelper.Read<ulong>(container);
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
                UMI3DEmbodimentManager.Instance.UserTrackingReception(trackingFrame, user.Id(), server.Time.Timestep);
            });

            trackingRelay.SetFrame(player, trackingFrame);
        }

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
            MainThreadManager.Run(() =>
            {
                UMI3DLogger.Log($"Received a VoIP frame", scope);
            });
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
