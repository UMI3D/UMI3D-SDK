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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.common.collaboration.dto.networking;
using umi3d.common.collaboration.dto.signaling;
using umi3d.common.collaboration.dto.voip;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace umi3d.cdk.collaboration
{
    /// <summary>
    /// Client for the Forge server, handling most of the transactions coming from the environment.
    /// </summary>
    /// The Forge client retrieve all the UDP messages sent by the Forge server.
    public class UMI3DForgeClient1 : UMI3DForgeSocketBase
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Networking;

        private UMI3DEnvironmentClient1 environmentClient;

        private uint Me => environmentClient?.UserDto.answerDto.networkId ?? 0;

        private bool useDto => environmentClient?.useDto ?? false;

        private bool pingReceived = false;
        private bool CheckForBandWidthRunning = false;

        /// <summary>
        /// 
        /// </summary>
        private UDPClient client;


        public bool IsConnected => client != null && client.IsConnected;

        private NetworkManager networkManagerComponent = null;

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
        public uint NetworkingId => client.Me.NetworkId;

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
        public static UMI3DForgeClient1 Create(UMI3DEnvironmentClient1 environmentClient, string ip = "127.0.0.1", ushort port = 15937, string masterServerHost = "", ushort masterServerPort = 15940, string natServerHost = "", ushort natServerPort = 15941)
        {
            UMI3DForgeClient1 client = new GameObject("UMI3DForgeClient").AddComponent<UMI3DForgeClient1>();
            client.environmentClient = environmentClient;
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
            client.connectAttemptFailed += RejectedByServer;
            client.bindFailure += BindFailed;
            client.bindSuccessful += BindSucceded;
            client.forcedDisconnect += (n) => { UMI3DLogger.Log("Force disconnect", scope); };
            client.playerAccepted += (n, p) => { UMI3DLogger.Log("Player Accepted", scope); };

            if (natServerHost.Trim().Length == 0)
                client.Connect(ip, port);
            else
                client.Connect(ip, port, natServerHost, natServerPort);

            //When connected

            if (!client.IsBound)
            {
                UMI3DLogger.LogError("NetWorker failed to bind", scope);
                return;
            }

            if (mgr == null && networkManager == null)
            {
                UMI3DLogger.LogWarning("A network manager was not provided, generating a new one instead", scope);
                networkManager = new GameObject("Network Manager");
                mgr = networkManager.AddComponent<NetworkManager>();
            }
            else if (mgr == null)
            {
                mgr = Instantiate(networkManager).GetComponent<NetworkManager>();
            }

            mgr.Initialize(client, masterServerHost, masterServerPort, null);

            networkManagerComponent = NetworkManager.Instance;

            CheckForBandWidth();
        }

        protected override void SetRoundTripLatency(double latency, NetWorker sender)
        {
            pingReceived = true;
            base.SetRoundTripLatency(latency, sender);
        }

        private async void CheckForBandWidth()
        {
            if (CheckForBandWidthRunning) return;
            CheckForBandWidthRunning = true;

            while ((networkManagerComponent?.Networker != null && networkManagerComponent.Networker.BandwidthIn <= 0) || !IsConnected)
                await UMI3DAsyncManager.Delay(1000);

            float lastBand = -1;
            while (networkManagerComponent?.Networker != null && IsConnected && lastBand != networkManagerComponent.Networker.BandwidthIn)
            {

                lastBand = networkManagerComponent.Networker.BandwidthIn;
                await UMI3DAsyncManager.Delay(1000);
            }

            CheckForBandWidthRunning = false;
            if (IsConnected && networkManagerComponent?.Networker != null)
            {
                CheckForPing();
            }
        }

        private async void CheckForPing()
        {

            int count = 10;
            float lastBand = -1;

            while (count > 0)
            {
                pingReceived = false;
                lastBand = networkManagerComponent.Networker.BandwidthIn;
                client.Ping();
                UMI3DLogger.Log($"Send Ping", scope);
                await UMI3DAsyncManager.Delay(10000);

                if (environmentClient == null || !environmentClient.IsConnected() || networkManagerComponent?.Networker == null || !IsConnected)
                    return;

                if (pingReceived || lastBand != networkManagerComponent.Networker.BandwidthIn)
                {
                    CheckForBandWidth();
                    return;
                }
                count--;
            }

            DisconnectedFromServer(networkManagerComponent.Networker);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (client != null) client.Disconnect(true);
            client = null;

            if (networkManagerComponent?.Networker != null)
                networkManagerComponent.Networker.disconnected -= DisconnectedFromServer;

            networkManagerComponent?.Disconnect();
            networkManagerComponent = null;
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
                UMI3DLogger.Log("AUTH FAILED !", scope);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void AcceptedByServer(NetWorker sender)
        {
            UMI3DLogger.Log("Accepted by server", scope);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void RejectedByServer(NetWorker sender)
        {
            UMI3DLogger.Log("rejected by server", scope);
        }

        private void BindFailed(NetWorker sender)
        {
            UMI3DLogger.Log("Bind Failed", scope);
        }

        private void BindSucceded(NetWorker sender)
        {
            UMI3DLogger.Log("Bind Succeded", scope);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void DisconnectedFromServer(NetWorker sender)
        {
            if (networkManagerComponent?.Networker != null)
                networkManagerComponent.Networker.disconnected -= DisconnectedFromServer;

            MainThreadManager.Run(() =>
            {
                networkManagerComponent?.Disconnect();
                networkManagerComponent = null;

                if (client != null)
                    environmentClient?.ConnectionDisconnected();
            });
        }

        /// <inheritdoc/>
        protected override void OnSignalingFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            var dto = UMI3DDtoSerializer.FromBson(frame.StreamData.byteArr);
            switch (dto)
            {
                case TokenDto tokenDto:
                    environmentClient.SetToken(tokenDto.token);
                    break;
                case StatusDto statusDto:
                    MainThreadManager.Run(() =>
                    {
                        environmentClient.OnStatusChanged(statusDto);
                    });
                    break;
                case StatusRequestDto statusRequestDto:
                    MainThreadManager.Run(() =>
                    {
                        environmentClient.HttpClient.SendPostUpdateStatusAsync(environmentClient.UserDto.answerDto.status);
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
            {
                SendBinaryData((int)DataChannelTypes.Data, dto.ToBson(), reliable);
            }
            else
            {
                SendBinaryData((int)DataChannelTypes.Data, UMI3DSerializer.Write(dto).ToBytes(), reliable);
            }
        }

        public void SendVOIP(int length, byte[] sample)
        {
            if (client == null || client.Me == null) return;
            Binary voice = null;
            if (useDto)
            {
                var dto = new VoiceDataDto()
                {
                    data = sample.Take(length).ToArray(),
                    senderId = Me
                };
                voice = new Binary(client.Time.Timestep, false, dto.ToBson(), Receivers.All, MessageGroupIds.VOIP, false);
            }
            else
            {
                Bytable bytable = UMI3DSerializer.Write(Me) + UMI3DSerializer.WriteCollection(sample.Take(length));
                voice = new Binary(client.Time.Timestep, false, bytable.ToBytes(), Receivers.All, MessageGroupIds.VOIP, false);
            }
            client.Send(voice);
        }

        /// <inheritdoc/>
        protected override void OnDataFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            //if(UMI3DClientServer.Exists && UMI3DClientServer.transactionDispatcher == null)
            //{
            //    UMI3DClientServer.transactionDispatcher = new UMI3DTransactionDispatcher((dto) => PerformOperation(dto),(id,c)=>PerformOperation(id,c));

            //}

            //if (UMI3DClientServer.transactionDispatcher == null)
            //    throw new Exception("Transaction Dispatcher should not be null");

            if (useDto)
            {
                var dto = UMI3DDtoSerializer.FromBson(frame.StreamData.byteArr);

                switch (dto)
                {
                    case TransactionDto transaction:
                        MainThreadManager.Run(async () =>
                        {
                            UMI3DLogger.Log($"On Frame", scope);
                            await Task.Yield();
                            environmentClient.node.OnData(frame);
                            //await UMI3DClientServer.transactionDispatcher.PerformTransaction(transaction);
                            //if(UMI3DCollaborationClientServer.transactionPending != null)
                            //    UMI3DCollaborationClientServer.transactionPending.areTransactionPending = false;
                        });

                        break;

                    default:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DLogger.Log($"type not catch {dto.GetType()}", scope);
                        });
                        break;
                }
            }
            else
            {
                var container = new ByteContainer(0, frame);
                uint TransactionId = UMI3DSerializer.Read<uint>(container);
                switch (TransactionId)
                {
                    case UMI3DOperationKeys.Transaction:
                        MainThreadManager.Run(async () =>
                        {
                            UMI3DLogger.Log($"On Frame", scope);
                            await Task.Yield();
                            environmentClient.node.OnData(frame);

                        });
                        break;

                    default:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DLogger.Log($"type not catch {TransactionId}", scope);
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
                SendBinaryData((int)DataChannelTypes.Tracking, UMI3DSerializer.Write(dto).ToBytes(), false);
        }

        /// <inheritdoc/>
        protected override void OnAvatarFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            MainThreadManager.Run(() =>
            {
                UMI3DLogger.Log($"On T Frame", scope);
                //CollaborationSkeletonsManager.Instance.UpdateSkeleton(frames.values);
            });
            if (useDto)
            {
                if (UMI3DDtoSerializer.FromBson(frame.StreamData.byteArr) is UMI3DDtoListDto<UserTrackingFrameDto> frames)
                {
                    MainThreadManager.Run(() =>
                    {
                        UMI3DLogger.Log($"On T Frame", scope);
                        //CollaborationSkeletonsManager.Instance.UpdateSkeleton(frames.values);
                    });
                }
            }
            else
            {
                var container = new ByteContainer(0, frame);
                try
                {
                    System.Collections.Generic.List<UserTrackingFrameDto> frames = UMI3DSerializer.ReadList<UserTrackingFrameDto>(container);
                    MainThreadManager.Run(() =>
                    {
                        UMI3DLogger.Log($"On T Frame", scope);
                        //CollaborationSkeletonsManager.Instance.UpdateSkeleton(frames);
                    });
                }
                catch (Exception e)
                {
                    UMI3DLogger.LogError("Impossible to read tracking frames from server " + e.Message, scope);
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
                UMI3DLogger.LogError("Video channels not implemented!", scope);
            });
        }

        #endregion

        #region VoIP



        /// <inheritdoc/>
        protected override void OnVoIPFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {

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
            if (IsConnected)
            {
                ulong timestep = NetworkManager.Instance.Networker.Time.Timestep;
                bool isTcpClient = NetworkManager.Instance.Networker is TCPClient;
                bool isTcp = NetworkManager.Instance.Networker is BaseTCP;

                var bin = new Binary(timestep, isTcpClient, data, Receivers.All, channel, isTcp);
                client.Send(bin, isReliable);
            }
        }

        #region MonoBehaviour
        private static bool HasBeenSet = false;
        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            //if (UMI3DClientServer.Exists && UMI3DClientServer.transactionDispatcher == null)
            //{
            //    UMI3DClientServer.transactionDispatcher = new UMI3DTransactionDispatcher((dto) => PerformOperation(dto), (id, c) => PerformOperation(id, c));

            //}
            // If not using TCP
            // Should it be done before Host() ???
            NetWorker.PingForFirewall(port);
            if (!HasBeenSet) inetum.unityUtils.QuittingManager.OnApplicationIsQuitting.AddListener(ApplicationQuit);
            HasBeenSet = true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void ApplicationQuit()
        {
            if (!inetum.unityUtils.QuittingManager.ApplicationIsQuitting) return;
            NetworkManager.Instance.ApplicationQuit();
            Stop();
        }

        private void OnDestroy()
        {
            Stop();
            destroyed = true;
            HasBeenSet = false;
        }

        private bool destroyed = false;

        private new Coroutine StartCoroutine(IEnumerator enumerator)
        {
            if (!destroyed)
                return base.StartCoroutine(enumerator);
            return null;
        }

        #endregion
    }
}
