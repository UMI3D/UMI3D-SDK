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
using System.Collections;
using System.Linq;
using umi3d.cdk.interaction;
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
    public class UMI3DForgeClient : UMI3DForgeSocketBase
    {
        private const DebugScope scope = DebugScope.CDK | DebugScope.Collaboration | DebugScope.Networking;

        private UMI3DEnvironmentClient environmentClient;

        private uint Me => environmentClient?.UserDto.answerDto.networkId ?? 0;

        private bool useDto => environmentClient?.useDto ?? false;

        private bool pingReceived = false;
        private bool CheckForBandWidthRunning = false;

        private UMI3DUser GetUserByNetWorkId(uint nid)
        {
            if (UMI3DCollaborationEnvironmentLoader.Exists && UMI3DCollaborationEnvironmentLoader.Instance.UserList != null)
            {
                lock (UMI3DCollaborationEnvironmentLoader.Instance.UserList)
                {
                    return UMI3DCollaborationEnvironmentLoader.Instance.UserList?.Find(u => u?.networkId == nid);
                }
            }

            return null;
        }

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
        public static UMI3DForgeClient Create(UMI3DEnvironmentClient environmentClient, string ip = "127.0.0.1", ushort port = 15937, string masterServerHost = "", ushort masterServerPort = 15940, string natServerHost = "", ushort natServerPort = 15941)
        {
            UMI3DForgeClient client = new GameObject("UMI3DForgeClient").AddComponent<UMI3DForgeClient>();
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
            var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);
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
                SendBinaryData((int)DataChannelTypes.Data, dto.ToBytableArray().ToBytes(), reliable);
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
                Bytable bytable = UMI3DNetworkingHelper.Write(Me) + UMI3DNetworkingHelper.WriteCollection(sample.Take(length));
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

                switch (dto)
                {
                    case TransactionDto transaction:
                        MainThreadManager.Run(() =>
                        {
                            StartCoroutine(UMI3DTransactionDispatcher.PerformTransaction(transaction));
                        });

                        break;
                    case NavigateDto navigate:
                        MainThreadManager.Run(() =>
                        {
                            StartCoroutine(UMI3DNavigation.Navigate(navigate));
                        });

                        break;
                    case GetLocalInfoRequestDto requestGet:
                        MainThreadManager.Run(() =>
                        {
                            SendGetLocalInfo(requestGet.key);
                        });

                        break;
                    case RequestHttpUploadDto uploadFileRequest:
                        string token = uploadFileRequest.uploadToken;
                        string fileId = uploadFileRequest.fileId;

                        string fileName = FileUploader.GetFileName(fileId);
                        byte[] bytesToUpload = FileUploader.GetFileToUpload(fileId);
                        if (bytesToUpload != null)
                        {
                            MainThreadManager.Run(() =>
                            {
                                SendPostFile(token, fileName, bytesToUpload);
                            });
                        }
                        break;
                    case RedirectionDto redirection:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DCollaborationClientServer.Connect(redirection);
                        });
                        break;
                    case ForceLogoutDto forceLogout:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DCollaborationClientServer.ReceivedLogoutMessage(forceLogout.reason);
                        });
                        break;
                    default:
                        UMI3DLogger.Log($"Type not catch {dto.GetType()}", scope);
                        break;
                }
            }
            else
            {
                var container = new ByteContainer(frame.StreamData.byteArr);
                uint TransactionId = UMI3DNetworkingHelper.Read<uint>(container);
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
                            SerializableVector3 pos = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                            var nav = new NavigateDto() { position = pos };
                            MainThreadManager.Run(() =>
                            {
                                StartCoroutine(UMI3DNavigation.Navigate(nav));
                            });
                        }
                        break;
                    case UMI3DOperationKeys.TeleportationRequest:
                        {
                            SerializableVector3 pos = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                            SerializableVector4 rot = UMI3DNetworkingHelper.Read<SerializableVector4>(container);
                            var nav = new TeleportDto() { position = pos, rotation = rot };
                            MainThreadManager.Run(() =>
                            {
                                StartCoroutine(UMI3DNavigation.Navigate(nav));
                            });
                        }
                        break;
                    case UMI3DOperationKeys.VehicleRequest:
                        {
                            SerializableVector3 pos = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                            SerializableVector4 rot = UMI3DNetworkingHelper.Read<SerializableVector4>(container);
                            ulong vehicleId = UMI3DNetworkingHelper.Read<ulong>(container);
                            bool stopNavigation = UMI3DNetworkingHelper.Read<bool>(container);

                            var nav = new VehicleDto()
                            {
                                position = pos,
                                rotation = rot,
                                VehicleId = vehicleId,
                                StopNavigation = stopNavigation,
                            };

                            MainThreadManager.Run(() =>
                            {
                                StartCoroutine(UMI3DNavigation.Navigate(nav));
                            });
                        }
                        break;
                    case UMI3DOperationKeys.BoardedVehicleRequest:
                        {
                            SerializableVector3 pos = UMI3DNetworkingHelper.Read<SerializableVector3>(container);
                            SerializableVector4 rot = UMI3DNetworkingHelper.Read<SerializableVector4>(container);
                            ulong vehicleId = UMI3DNetworkingHelper.Read<ulong>(container);
                            bool stopNavigation = UMI3DNetworkingHelper.Read<bool>(container);
                            ulong bodyAnimationId = UMI3DNetworkingHelper.Read<ulong>(container);
                            bool changeBonesToStream = UMI3DNetworkingHelper.Read<bool>(container);
                            System.Collections.Generic.List<uint> bonesToStream = UMI3DNetworkingHelper.ReadList<uint>(container);

                            var nav = new BoardedVehicleDto()
                            {
                                position = pos,
                                rotation = rot,
                                VehicleId = vehicleId,
                                StopNavigation = stopNavigation,
                                BodyAnimationId = bodyAnimationId,
                                ChangeBonesToStream = changeBonesToStream,
                                BonesToStream = bonesToStream
                            };

                            MainThreadManager.Run(() =>
                            {
                                StartCoroutine(UMI3DNavigation.Navigate(nav));
                                UMI3DClientUserTracking.Instance.EmbarkVehicle(nav);
                            });
                        }
                        break;
                    case UMI3DOperationKeys.EmoteRequest:
                        {
                            ulong emoteId = UMI3DNetworkingHelper.Read<ulong>(container);
                            bool trigger = UMI3DNetworkingHelper.Read<bool>(container);
                            ulong sendingUserId = UMI3DNetworkingHelper.Read<ulong>(container);
                            MainThreadManager.Run(() =>
                            {
                                if (trigger)
                                    UMI3DClientUserTracking.Instance.PlayEmoteOnOtherAvatar(emoteId, sendingUserId);
                                else
                                    UMI3DClientUserTracking.Instance.StopEmoteOnOtherAvatar(emoteId, sendingUserId);
                            });
                        }
                        break;
                    case UMI3DOperationKeys.GetLocalInfoRequest:
                        string key = UMI3DNetworkingHelper.Read<string>(container);
                        MainThreadManager.Run(() =>
                        {
                            SendGetLocalInfo(key);
                        });
                        break;
                    case UMI3DOperationKeys.UploadFileRequest:
                        string token = UMI3DNetworkingHelper.Read<string>(container);
                        string fileId = UMI3DNetworkingHelper.Read<string>(container);
                        string name = FileUploader.GetFileName(fileId);
                        byte[] bytesToUpload = FileUploader.GetFileToUpload(fileId);
                        if (bytesToUpload != null)
                        {
                            MainThreadManager.Run(() =>
                            {
                                SendPostFile(token, name, bytesToUpload);
                            });
                        }
                        break;
                    case UMI3DOperationKeys.RedirectionRequest:
                        RedirectionDto redirection = UMI3DNetworkingHelper.Read<RedirectionDto>(container);
                        MainThreadManager.Run(() =>
                        {
                            UMI3DCollaborationClientServer.Connect(redirection);
                        });
                        break;
                    case UMI3DOperationKeys.ForceLogoutRequest:
                        ForceLogoutDto forceLogout = UMI3DNetworkingHelper.Read<ForceLogoutDto>(container);
                        MainThreadManager.Run(() =>
                        {
                            UMI3DCollaborationClientServer.ReceivedLogoutMessage(forceLogout.reason);
                        });
                        break;
                    default:
                        MainThreadManager.Run(() =>
                        {
                            UMI3DLogger.Log($"Type not catch {TransactionId}", scope);
                        });
                        break;
                }
            }
        }

        private async void SendGetLocalInfo(string key)
        {
            try
            {
                byte[] bytes = await environmentClient.HttpClient.SendGetLocalInfo(key);
                LocalInfoSender.SetLocalInfo(key, bytes);
            }
            catch
            {
                UMI3DLogger.Log("error on get local info : " + key, scope);
            }
        }

        private async void SendPostFile(string token, string fileName, byte[] bytesToUpload)
        {
            try
            {
                await environmentClient.HttpClient.SendPostFile(token, fileName, bytesToUpload);
            }
            catch
            {
                UMI3DLogger.Log("error on upload file : " + fileName, scope);
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
                    if (UMI3DClientUserTracking.Instance.trackingReception && UMI3DClientUserTracking.Instance.embodimentDict.TryGetValue(trackingFrame.userId, out UserAvatar userAvatar))
                    {
                        MainThreadManager.Run(() =>
                        {
                            if (client.Time.Timestep - frame.TimeStep < 500)
                                StartCoroutine((userAvatar as UMI3DCollaborativeUserAvatar).UpdateAvatarPosition(trackingFrame, frame.TimeStep));
                        });
                    }
                    else
                    {
                        MainThreadManager.Run(() =>
                        {
                            UMI3DLogger.LogWarning("Avatar Frame Dropped", scope);
                        });
                    }
                }
            }
            else
            {
                var trackingFrame = new common.userCapture.UserTrackingFrameDto();

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

                    if (UMI3DClientUserTracking.Instance.embodimentDict.TryGetValue(trackingFrame.userId, out UserAvatar userAvatar))
                    {
                        MainThreadManager.Run(() =>
                        {
                            if (client.Time.Timestep - frame.TimeStep < 500)
                                StartCoroutine((userAvatar as UMI3DCollaborativeUserAvatar).UpdateAvatarPosition(trackingFrame, frame.TimeStep));
                        });
                    }
                    else
                    {
                        MainThreadManager.Run(() =>
                        {
                            UMI3DLogger.LogWarning("User Avatar not found.", scope);
                        });
                    }
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
