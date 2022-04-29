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
using UnityEngine;

namespace umi3d.common.collaboration
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class ForgeSocketBase : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public string ip = "127.0.0.1";

        /// <summary>
        /// 
        /// </summary>
        public ushort port = 15937;

        /// <summary>
        /// 
        /// </summary>
        public string masterServerHost = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public ushort masterServerPort = 15940;

        /// <summary>
        /// 
        /// </summary>
        public string natServerHost = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public ushort natServerPort = 15941;

        /// <summary>
        /// 
        /// </summary>
        public GameObject networkManager = null; //should be a prefab is setted

        /// <summary>
        /// 
        /// </summary>
        protected NetworkManager mgr = null;

        /// <summary>
        /// The server can get the player count from the networker but the client
        /// currently can not, so we will track player counts in this variable for
        /// both the client and the server
        /// </summary>
        public int playerCount = 0;

        /// <summary>
        /// The amount of time in milliseconds for the round trip latency to/from the server
        /// </summary>
        public double RoundTripLatency { get; private set; }

        /// <summary>
        /// Time since the connection started.
        /// </summary>
        public static ulong Timestep
        {
            get
            {
                if (NetworkManager.Instance == null || NetworkManager.Instance.Networker == null)
                    return 0;
                return NetworkManager.Instance.Networker.Time.Timestep;
            }
        }

        /// <summary>
        /// Used to determine how much bandwidth (in bytes) hass been read
        /// </summary>
        public static ulong BandwidthIn
        {
            get
            {
                if (NetworkManager.Instance == null || NetworkManager.Instance.Networker == null)
                    return 0;
                return NetworkManager.Instance.Networker.BandwidthIn;
            }
        }

        /// <summary>
        /// Used to determine how much bandwidth (in bytes) hass been written
        /// </summary>
        public static ulong BandwidthOut
        {
            get
            {
                if (NetworkManager.Instance == null || NetworkManager.Instance.Networker == null)
                    return 0;
                return NetworkManager.Instance.Networker.BandwidthOut;
            }
        }

        /// <summary>
        /// Should info pannel should be displyed in top left corner.
        /// </summary>
        public bool DisplayForgeInfo = false;

        /// <summary>
        /// Getter for the Forge NetWorker
        /// </summary>
        /// <return> a Forge NetWorker.</return>
        public abstract NetWorker GetNetWorker();


        /// <summary>
        /// Update RoundTripLatency
        /// </summary>
        /// <param name="latency">time between ping and pong</param>
        /// <param name="sender">the corresponding NetWorker</param>
        protected void SetRoundTripLatency(double latency, NetWorker sender)
        {
            RoundTripLatency = latency;
        }

        /// <summary>
        /// Called when a BinaryFrame is received from the NetWorker
        /// </summary>
        /// <param name="player">the player the message is comming from</param>
        /// <param name="frame">the received binary frame</param>
        /// <param name="sender">the corresponding NetWorker</param>
        protected virtual void ReadBinary(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            //Checks if the message is comming from the server or from an accepted player
            if (!player.Accepted && !player.IsHost)
                return;

            //Dispatches the frame to the appropriated listener
            switch (frame.GroupId)
            {
                case (int)DataChannelTypes.Signaling:
                    OnSignalingFrame(player, frame, sender);
                    break;
                case (int)DataChannelTypes.Tracking:
                    OnAvatarFrame(player, frame, sender);
                    break;
                case (int)DataChannelTypes.Data:
                    OnDataFrame(player, frame, sender);
                    break;
                case (int)DataChannelTypes.VoIP:
                    OnVoIPFrame(player, frame, sender);
                    break;
                case (int)DataChannelTypes.Video:
                    OnVideoFrame(player, frame, sender);
                    break;
                case MessageGroupIds.AUTHENTICATION_FAILURE:
                    OnAuthenticationFailure(player, frame, sender);
                    break;
            }
        }

        /// <summary>
        /// Called when a UMI3D Signaling frame is received from the NetWorker
        /// </summary>
        /// <param name="player">the player the message is comming from</param>
        /// <param name="frame">the received binary frame</param>
        /// <param name="sender">the corresponding NetWorker</param>
        protected abstract void OnSignalingFrame(NetworkingPlayer player, Binary frame, NetWorker sender);

        /// <summary>
        /// Called when a UMI3D User Tracking frame is received from the NetWorker
        /// </summary>
        /// <param name="player">the player the message is comming from</param>
        /// <param name="frame">the received binary frame</param>
        /// <param name="sender">the corresponding NetWorker</param>
        protected abstract void OnAvatarFrame(NetworkingPlayer player, Binary frame, NetWorker sender);

        /// <summary>
        /// Called when a UMI3D Data (Transaction or Browser request) frame is received from the NetWorker
        /// </summary>
        /// <param name="player">the player the message is comming from</param>
        /// <param name="frame">the received binary frame</param>
        /// <param name="sender">the corresponding NetWorker</param>
        protected abstract void OnDataFrame(NetworkingPlayer player, Binary frame, NetWorker sender);

        /// <summary>
        /// Called when a VoIP frame is received from the NetWorker
        /// </summary>
        /// <param name="player">the player the message is comming from</param>
        /// <param name="frame">the received binary frame</param>
        /// <param name="sender">the corresponding NetWorker</param>
        protected abstract void OnVoIPFrame(NetworkingPlayer player, Binary frame, NetWorker sender);

        /// <summary>
        /// Called when a Video frame is received from the NetWorker
        /// </summary>
        /// <param name="player">the player the message is comming from</param>
        /// <param name="frame">the received binary frame</param>
        /// <param name="sender">the corresponding NetWorker</param>
        protected abstract void OnVideoFrame(NetworkingPlayer player, Binary frame, NetWorker sender);

        /// <summary>
        /// Called when the player authentication is rejected by the Forge server.
        /// </summary>
        /// <param name="player">the player the message is comming from</param>
        /// <param name="frame">the received binary frame</param>
        /// <param name="sender">the corresponding NetWorker</param>
        protected virtual void OnAuthenticationFailure(NetworkingPlayer player, Binary frame, NetWorker sender) { }

        //MONITORING

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="message"></param>
        private void WriteLabel(Rect rect, string message)
        {
            GUI.color = Color.black;
            GUI.Label(rect, message);
            // Do the same thing as above but make the above UI look like a solid
            // shadow so that the text is readable on any contrast screen
            GUI.color = Color.white;
            GUI.Label(rect, message);
        }

        private void OnGUI()
        {
            if (!DisplayForgeInfo || NetworkManager.Instance == null || NetworkManager.Instance.Networker == null)
                return;

            // If there are no players, then the scene is currently being loaded, otherwise
            // show the current count of players in the game
            if (NetworkManager.Instance.Networker.IsServer && playerCount > 0)
                WriteLabel(new Rect(14, 14, 100, 25), "Players: " + (playerCount - 1));

            if (!GetNetWorker().IsServer)
                WriteLabel(new Rect(14, 14, 100, 25), "Connected: " + (GetNetWorker().Me != null ? GetNetWorker().Me.Connected : false));

            WriteLabel(new Rect(14, 28, 100, 25), "Time: " + NetworkManager.Instance.Networker.Time.Timestep);
            WriteLabel(new Rect(14, 42, 256, 25), "Bandwidth In: " + NetworkManager.Instance.Networker.BandwidthIn);
            WriteLabel(new Rect(14, 56, 256, 25), "Bandwidth Out: " + NetworkManager.Instance.Networker.BandwidthOut);
            WriteLabel(new Rect(14, 70, 256, 25), "Round Trip Latency (ms): " + RoundTripLatency);
        }

    }
}
