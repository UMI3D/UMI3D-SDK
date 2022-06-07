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
        protected void ReadBinary(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
            //Checks if the message is comming from the server or from an accepted player
            if (!player.Accepted && !player.IsHost)
                return;

            _ReadBinary(player, frame, sender);
        }



        /// <summary>
        /// Called when a BinaryFrame is received from the NetWorker
        /// </summary>
        /// <param name="player">the player the message is comming from</param>
        /// <param name="frame">the received binary frame</param>
        /// <param name="sender">the corresponding NetWorker</param>
        protected abstract void _ReadBinary(NetworkingPlayer player, Binary frame, NetWorker sender);

    }
}
