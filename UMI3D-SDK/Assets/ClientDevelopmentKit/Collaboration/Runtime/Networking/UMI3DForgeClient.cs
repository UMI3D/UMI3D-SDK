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

using BeardedManStudios;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Frame;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Threading;
using System.Collections.Generic;
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

        UMI3DUser GetUserByNetWorkId(uint nid)
        {
            return UMI3DCollaborationEnvironmentLoader.Instance.UserList.Find(u => u.networkId == nid);
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
            StopVoip();
            if (client != null) client.Disconnect(true);
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
            //TODO
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
            //TODO
            StartVOIP();
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

                Debug.Log("DisconnectedFromServer !");
                //TODO
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
            SendBinaryData((int)DataChannelTypes.Data, dto.ToBson(), reliable);
        }

        /// <inheritdoc/>
        protected override void OnDataFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            
            var dto = UMI3DDto.FromBson(frame.StreamData.byteArr);
            Debug.Log(dto);
            switch (dto)
            {
                case TransactionDto transaction:
                    MainThreadManager.Run(() => {
                        StartCoroutine(UMI3DTransactionDispatcher.PerformTransaction(transaction));
                    });
                    break;
                case NavigateDto navigate:
                    MainThreadManager.Run(() => {
                        StartCoroutine(UMI3DNavigation.Navigate(navigate));
                    });
                    break;
                default:
                    Debug.Log($"Type not catch {dto.GetType()}");
                    break;
            }
        }

        #endregion

        #region avatar

        public void SendTrackingFrame(AbstractBrowserRequestDto dto)
        {
            SendBinaryData((int)DataChannelTypes.Tracking, dto.ToBson(), false);
        }

        /// <inheritdoc/>
        protected override void OnAvatarFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            if (UMI3DDto.FromBson(frame.StreamData.byteArr) is UserTrackingFrameDto trackingFrame)
            {
                if (UMI3DClientUserTracking.Instance.embodimentDict.TryGetValue(trackingFrame.userId, out UserAvatar userAvatar))
                    MainThreadManager.Run(() => {
                        StartCoroutine(userAvatar.UpdateBonePosition(trackingFrame));
                    });
                else
                    Debug.LogWarning("User Avatar not found.");
            }
        }

        #endregion

        #region video

        /// <inheritdoc/>
        protected override void OnVideoFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            Debug.LogError("Video channels not implemented!");
        }

        #endregion

        #region VoIP

        /// <inheritdoc/>
        protected override void OnVoIPFrame(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            VoiceDto dto = UMI3DDto.FromBson(frame.StreamData.byteArr) as VoiceDto;
            UMI3DUser source = GetUserByNetWorkId(dto.senderId);
            if (source != null)
                AudioManager.Instance.Read(source.id, dto);
        }

        /// <summary>
        /// 
        /// </summary>
        private int lastSample = 0;

        /// <summary>
        /// 
        /// </summary>
        private AudioClip mic = null;

        /// <summary>
        /// 
        /// </summary>
        private int channels = 1;

        /// <summary>
        /// 
        /// </summary>
        private int frequency = 8000;

        /// <summary>
        /// 
        /// </summary>
        private float[] samples = null;

        /// <summary>
        /// 
        /// </summary>
        private List<float> writeSamples = null;

        /// <summary>
        /// 
        /// </summary>
        private float WRITE_FLUSH_TIME = 0.5f;

        /// <summary>
        /// 
        /// </summary>
        private float writeFlushTimer = 0.0f;

        /// <summary>
        /// 
        /// </summary>
        public bool muted = false;

        /// <summary>
        /// 
        /// </summary>
        private void StartVOIP()
        {
            writeSamples = new List<float>(1024);
            MainThreadManager.Run(() =>
            {
                mic = Microphone.Start(null, true, 100, frequency);
                channels = mic.channels;
                if (mic == null)
                {
                    Debug.LogError("A default microphone was not found or plugged into the system");
                    return;
                }
                Task.Queue(VOIPWorker);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopVoip()
        {
            Microphone.End(null);
            mic = null;
        }

        /// <summary>
        /// 
        /// </summary>
        BMSByte writeBuffer = new BMSByte();

        /// <summary>
        /// 
        /// </summary>
        private void VOIPWorker()
        {
            while (client.IsConnected)
            {
                if (writeFlushTimer >= WRITE_FLUSH_TIME && writeSamples.Count > 0)
                {
                    writeFlushTimer = 0.0f;
                    lock (writeSamples)
                    {
                        writeBuffer.Clone(ToByteArray(writeSamples));
                        writeSamples.Clear();
                    }

                    if (!muted)
                    {
                        var dto = new VoiceDto()
                        {
                            length = writeBuffer.Size,
                            data = writeBuffer.byteArr,
                            senderId = client.Me.NetworkId
                        };
                        Binary voice = new Binary(client.Time.Timestep, false, dto.ToBson(), Receivers.All, MessageGroupIds.VOIP, false);
                        client.Send(voice);
                    }

                }
                MainThreadManager.ThreadSleep(10);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReadMic()
        {
            if (mic != null)
            {
                writeFlushTimer += Time.deltaTime;
                int pos = Microphone.GetPosition(null);
                int diff = pos - lastSample;

                if (diff > 0)
                {
                    samples = new float[diff * channels];
                    mic.GetData(samples, lastSample);

                    lock (writeSamples)
                    {
                        writeSamples.AddRange(samples);
                    }
                }
                lastSample = pos;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sampleList"></param>
        /// <returns></returns>
        private byte[] ToByteArray(List<float> sampleList)
        {
            int len = sampleList.Count * 4;
            byte[] byteArray = new byte[len];
            int pos = 0;

            for (int i = 0; i < sampleList.Count; i++)
            {
                byte[] data = System.BitConverter.GetBytes(sampleList[i]);
                System.Array.Copy(data, 0, byteArray, pos, 4);
                pos += 4;
            }

            return byteArray;
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
        private void FixedUpdate()
        {
            ReadMic();
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
