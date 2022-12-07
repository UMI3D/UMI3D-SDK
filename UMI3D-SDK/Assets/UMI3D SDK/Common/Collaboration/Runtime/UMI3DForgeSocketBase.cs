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
    /// Forge server configuration and UMI3D data communication.
    /// </summary>
    public abstract class UMI3DForgeSocketBase : ForgeSocketBase
    {
        /// <summary>
        /// Called when a BinaryFrame is received from the NetWorker
        /// </summary>
        /// <param name="player">the player the message is comming from</param>
        /// <param name="frame">the received binary frame</param>
        /// <param name="sender">the corresponding NetWorker</param>
        protected override void _ReadBinary(NetworkingPlayer player, Binary frame, NetWorker sender)
        {
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
                WriteLabel(new Rect(14, 14, 100, 25), "Connected: " + (GetNetWorker().Me != null && GetNetWorker().Me.Connected));

            WriteLabel(new Rect(14, 28, 100, 25), "Time: " + NetworkManager.Instance.Networker.Time.Timestep);
            WriteLabel(new Rect(14, 42, 256, 25), "Bandwidth In: " + NetworkManager.Instance.Networker.BandwidthIn);
            WriteLabel(new Rect(14, 56, 256, 25), "Bandwidth Out: " + NetworkManager.Instance.Networker.BandwidthOut);
            WriteLabel(new Rect(14, 70, 256, 25), "Round Trip Latency (ms): " + RoundTripLatency);
        }
    }
}
