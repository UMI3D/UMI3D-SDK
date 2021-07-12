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
using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using System.Collections;
using System.Linq;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.userCapture;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// 
    /// </summary>
    public class UMI3DForgeClient : ForgeSocketBase
    {
        uint Me { get { return UMI3DCollaborationClientServer.UserDto.networkId; } }
        bool useDto { get { return UMI3DCollaborationClientServer.useDto; } }

        UMI3DUser GetUserByNetWorkId(uint nid)
        {
            if (UMI3DCollaborationEnvironmentLoader.Exists && UMI3DCollaborationEnvironmentLoader.Instance.UserList != null)
                lock (UMI3DCollaborationEnvironmentLoader.Instance.UserList)
                {
                    return UMI3DCollaborationEnvironmentLoader.Instance.UserList?.Find(u => u?.networkId == nid);
                }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        private UDPClient client;


        public bool IsConnected { get => client != null && client.IsConnected; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override NetWorker GetNetWorker()
        {
            return client;
        }

        /// <summary>
        /// 
        /// </summary>
        public uint NetworkingId { get { return client.Me.NetworkId; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="masterServerHost"></param>
        /// <param name="masterServerPort"></param>
        /// <param name="natServerHost"></param>
        /// <param name="natServerPort"></param>
        /// <returns></returns>
        public static UMI3DForgeClient Create(string ip = "127.0.0.1", ushort port = 15937, string masterServerHost = "", ushort masterServerPort = 15940, string natServerHost = "", ushort natServerPort = 15941)
        {
            UMI3DForgeClient client = (new GameObject("UMI3DForgeClient")).AddComponent<UMI3DForgeClient>();
            client.ip = ip;
            client.port = port;
            client.masterServerHost = masterServerHost;
            client.masterServerPort = masterServerPort;
            client.natServerHost = natServerHost;
            client.natServerPort = natServerPort;
            return client;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authenticator"></param>
        public void Join(IUserAuthenticator authenticator = null)
        {
            client = new UDPClient();

            if (authenticator != null)
                client.SetUserAuthenticator(authenticator);

            client.binaryMessageReceived += ReadBinary;
            client.onPingPong += SetRoundTripLatency;
            client.disconnected += DisconnectedFromServer;
            client.serverAccepted += AcceptedByServer;

            if (natServerHost.Trim().Length == 0)
                client.Connect(ip, (ushort)port);
            else
                client.Connect(ip, (ushort)port, natServerHost, natServerPort);

            //When connected

            if (!client.IsBound)
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

            mgr.Initialize(client, masterServerHost, masterServerPort, null);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (client != null) client.Disconnect(true);
            client = null;
        }

        #region signaling



        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="frame"></param>
        /// <param name="sender"></param>
        protected override void OnAuthenticationFailure(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            MainThreadManager.Run(() =>
            {
                Debug.Log("AUTH FAILED !");
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void AcceptedByServer(NetWorker sender)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void DisconnectedFromServer(NetWorker sender)
        {
            NetworkManager.Instance.Networker.disconnected -= DisconnectedFromServer;
            MainThreadManager.Run(() =>
            {
                NetworkManager.Instance.Disconnect();
                if (client != null)
                    UMI3DCollaborationClientServer.Instance.ConnectionLost();
            });
        }



        /// <inheritdoc/>
        protected override void OnSignalingFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);
            switch (dto)
            {
                case TokenDto tokenDto:
                    UMI3DCollaborationClientServer.SetToken(tokenDto.token);
                    break;
                case StatusDto statusDto:
                    MainThreadManager.Run(() =>
                    {
                        UMI3DCollaborationClientServer.OnStatusChanged(statusDto);
                    });
                    break;
                case StatusRequestDto statusRequestDto:
                    MainThreadManager.Run(() =>
                    {
                        UMI3DCollaborationClientServer.Instance.HttpClient.SendPostUpdateStatus(null, null);
                    });
                    break;
            }
        }

        #endregion

        #region data

        public void SendSignalingData(UMI3DDto dto)
        {
            SendBinaryData((int)DataChannelTypes.Signaling, dto.ToBson(), true);
        }

        public void SendBrowserRequest(AbstractBrowserRequestDto dto, bool reliable)
        {
            if (useDto)
                SendBinaryData((int)DataChannelTypes.Data, dto.ToBson(), reliable);
            else
            {
                SendBinaryData((int)DataChannelTypes.Data, dto.ToBytableArray().ToBytes(), reliable);
            }
        }

        public void SendVOIP(int length, byte[] sample)
        {
            if (client == null || client.Me == null) return;
            Binary voice = null;
            if (useDto)
            {
                var dto = new VoiceDto()
                {
                    data = sample.Take(length).ToArray(),
                    senderId = Me
                };
                voice = new Binary(client.Time.Timestep, false, dto.ToBson(), Receivers.All, MessageGroupIds.VOIP, false);
            }
            else
            {
                var bytable = UMI3DNetworkingHelper.Write(Me) + UMI3DNetworkingHelper.WriteCollection(sample.Take(length));
                voice = new Binary(client.Time.Timestep, false, bytable.ToBytes(), Receivers.All, MessageGroupIds.VOIP, false);
            }
            client.Send(voice);
        }

        /// <inheritdoc/>
        protected override void OnDataFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            if (useDto)
            {
                var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);
                MainThreadManager.Run(() =>
                {
                    switch (dto)
                    {
                        case TransactionDto transaction:
                            StartCoroutine(UMI3DTransactionDispatcher.PerformTransaction(transaction));

                            break;
                        case NavigateDto navigate:
                            StartCoroutine(UMI3DNavigation.Navigate(navigate));

                            break;
                        case GetLocalInfoRequestDto requestGet:
                            UMI3DCollaborationClientServer.Instance.HttpClient.SendGetLocalInfo(
                                requestGet.key,
                                (bytes) => LocalInfoSender.SetLocalInfo(requestGet.key, bytes),
                                (error) => { Debug.Log("error on get local info : " + requestGet.key); }
                            );

                            break;
                        default:
                            Debug.Log($"Type not catch {dto.GetType()}");
                            break;
                    }
                });
            }
            else
            {
                var container = new ByteContainer(frame.StreamData.byteArr);
                var TransactionId = UMI3DNetworkingHelper.Read<uint>(container);
                switch (TransactionId)
                {
                    case UMI3DOperationKeys.Transaction:
                        MainThreadManager.Run(() =>
                        {
                            StartCoroutine(UMI3DTransactionDispatcher.PerformTransaction(container));
                        });
                        break;
                    case UMI3DOperationKeys.NavigationRequest:
                        {
                            var pos = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                            var nav = new NavigateDto() { position = pos };
                            MainThreadManager.Run(() =>
                            {
                                StartCoroutine(UMI3DNavigation.Navigate(nav));
                            });
                        }
                        break;
                    case UMI3DOperationKeys.TeleportationRequest:
                        {
                            var pos = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                            var rot = UMI3DNetworkingHelper.Read<SerializableVector4>(container);
                            var nav = new TeleportDto() { position = pos, rotation = rot };
                            MainThreadManager.Run(() =>
                            {
                                StartCoroutine(UMI3DNavigation.Navigate(nav));
                            });
                        }
                        break;

                    case UMI3DOperationKeys.GetLocalInfoRequest:
                        var key = UMI3DNetworkingHelper.Read<string>(container);
                        MainThreadManager.Run(() =>
                        {
                            UMI3DCollaborationClientServer.Instance.HttpClient.SendGetLocalInfo(
                            key,
                            (bytes) => LocalInfoSender.SetLocalInfo(key, bytes),
                            (error) => { Debug.Log("error on get local info : " + key); }
                            );
                        });
                        break;
                    default:
                        MainThreadManager.Run(() =>
                        {
                            Debug.Log($"Type not catch {TransactionId}");
                        });
                        break;
                }
            }
        }

        #endregion

        #region avatar

        public void SendTrackingFrame(AbstractBrowserRequestDto dto)
        {
            if (useDto)
                SendBinaryData((int)DataChannelTypes.Tracking, dto.ToBson(), false);
            else
                SendBinaryData((int)DataChannelTypes.Tracking, dto.ToBytableArray().ToBytes(), false);
        }

        /// <inheritdoc/>
        protected override void OnAvatarFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            if (useDto)
            {
                if (UMI3DDto.FromBson(frame.StreamData.byteArr) is UserTrackingFrameDto trackingFrame)
                {
                    if (UMI3DClientUserTracking.Instance.embodimentDict.TryGetValue(trackingFrame.userId, out UserAvatar userAvatar))
                        MainThreadManager.Run(() =>
                        {
                            if (client.Time.Timestep - frame.TimeStep < 500)
                                StartCoroutine((userAvatar as UMI3DCollaborativeUserAvatar).UpdateAvatarPosition(trackingFrame, frame.TimeStep));
                        });
                    else
                        MainThreadManager.Run(() =>
                        {
                            Debug.LogWarning("User Avatar not found.");
                        });
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

                    if (UMI3DClientUserTracking.Instance.embodimentDict.TryGetValue(trackingFrame.userId, out UserAvatar userAvatar))
                        MainThreadManager.Run(() =>
                        {
                            if (client.Time.Timestep - frame.TimeStep < 500)
                                StartCoroutine((userAvatar as UMI3DCollaborativeUserAvatar).UpdateAvatarPosition(trackingFrame, frame.TimeStep));
                        });
                    else
                        MainThreadManager.Run(() =>
                        {
                            Debug.LogWarning("User Avatar not found.");
                        });
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
                Debug.LogError("Video channels not implemented!");
            });
        }

        #endregion

        #region VoIP



        /// <inheritdoc/>
        protected override void OnVoIPFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            if (useDto)
            {
                VoiceDto dto = UMI3DDto.FromBson(frame.StreamData.byteArr) as VoiceDto;
                var id = dto.senderId;
                UMI3DUser source = GetUserByNetWorkId(id);
                if (source != null)
                    AudioManager.Instance.Read(source.id, dto.data, client.Time.Timestep);
            }
            else
            {
                var container = new ByteContainer(frame.StreamData.byteArr);
                var id = UMI3DNetworkingHelper.Read<uint>(container);
                UMI3DUser source = GetUserByNetWorkId(id);
                if (source != null)
                    AudioManager.Instance.Read(source.id, UMI3DNetworkingHelper.ReadByteArray(container), client.Time.Timestep);
            }


        }


        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        /// <param name="isReliable"></param>
        protected void SendBinaryData(int channel, byte[] data, bool isReliable)
        {
            ulong timestep = NetworkManager.Instance.Networker.Time.Timestep;
            bool isTcpClient = NetworkManager.Instance.Networker is TCPClient;
            bool isTcp = NetworkManager.Instance.Networker is BaseTCP;

            Binary bin = new Binary(timestep, isTcpClient, data, Receivers.All, channel, isTcp);
            client.Send(bin, isReliable);
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

        private void OnDestroy()
        {
            Stop();
            destroyed = true;
        }

        bool destroyed = false;
        new Coroutine StartCoroutine(IEnumerator enumerator)
        {
            if (!destroyed)
                return base.StartCoroutine(enumerator);
            return null;
        }

        #endregion
    }
}