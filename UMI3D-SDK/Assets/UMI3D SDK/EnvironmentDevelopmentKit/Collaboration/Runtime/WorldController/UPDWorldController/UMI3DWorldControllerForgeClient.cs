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
using umi3d.common;
using umi3d.common.collaboration;
using UnityEngine;

namespace umi3d.worldController
{
    public class UMI3DWorldControllerForgeClient : ForgeSocketBase
    {

        private const DebugScope scope = DebugScope.EDK | DebugScope.Collaboration | DebugScope.Networking;

        private UDPClient client;
        private UPDWorldControllerAPI api;

        public bool IsConnected => client != null && client.IsConnected;

        public override NetWorker GetNetWorker()
        {
            return client;
        }

        protected override void _ReadBinary(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            api?.OnMessage(frame.GroupId, frame?.StreamData?.byteArr);
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
        /// <returns></returns>
        public static UMI3DWorldControllerForgeClient Create(UPDWorldControllerAPI api, string ip = "127.0.0.1", ushort port = 15937, string masterServerHost = "", ushort masterServerPort = 15940, string natServerHost = "", ushort natServerPort = 15941)
        {
            UMI3DWorldControllerForgeClient client = new GameObject("UMI3DWorldControllerForgeClient").AddComponent<UMI3DWorldControllerForgeClient>();
            client.api = api;
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
            if (NetworkManager.Instance?.Networker != null)
                NetworkManager.Instance.Networker.disconnected -= DisconnectedFromServer;
            MainThreadManager.Run(() =>
            {
                NetworkManager.Instance?.Disconnect();
                if (client != null)
                    api?.ConnectionLost();
            });
        }

        public void Send(byte[] message)
        {
            var data = new Binary(client.Time.Timestep, false, message, Receivers.All, MessageGroupIds.START_OF_GENERIC_IDS, false);
            client.Send(data);
        }


        public void Stop()
        {
            if (client != null) client.Disconnect(true);
            client = null;
            if (NetworkManager.Instance?.Networker != null)
                NetworkManager.Instance.Networker.disconnected -= DisconnectedFromServer;
            NetworkManager.Instance?.Disconnect();
        }
    }
}