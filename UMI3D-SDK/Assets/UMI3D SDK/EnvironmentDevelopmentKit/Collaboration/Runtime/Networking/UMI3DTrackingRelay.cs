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

using System.Collections.Generic;
using umi3d.common.collaboration;
using umi3d.common;
using umi3d.common.userCapture.tracking;
using System.Linq;
using BeardedManStudios.Forge.Networking;

namespace umi3d.edk.collaboration.tracking
{
    public class UMI3DTrackingRelay : UMI3DToUserRelay<List<UserTrackingFrameDto>>
    {
        private const DebugScope debugScope = DebugScope.EDK | DebugScope.UserCapture | DebugScope.Collaboration | DebugScope.User;

        /// <summary>
        /// Contains all user tracking frames serialized for an update.
        /// </summary>
        protected Dictionary<NetworkingPlayer, byte[]> tempRawFramesPerPlayers;

        /// <summary>
        /// Temp list to contain all bytes to snd to a user.
        /// </summary>
        private List<byte> tempMessage = new();

        public UMI3DTrackingRelay(IForgeServer server) : base(server)
        {
            dataChannel = DataChannelTypes.Tracking;
            tempRawFramesPerPlayers = new();
        }

        protected override void Update()
        {
            try
            {
                if (!UMI3DEnvironment.Instance.useDto)
                {
                    lock (framesPerSourceLock)
                    {
                        // Serialize all tracking frames once for all players.
                        foreach ((NetworkingPlayer player, List<UserTrackingFrameDto> frames) in framesPerSource)
                        {
                            if (frames.Count == 1)
                                tempRawFramesPerPlayers[player] = UMI3DSerializer.Write(frames[0]).ToBytes();
                            else
                            {
                                UnityEngine.Debug.LogError("Tracking frame list should only contains one entry");
                                tempRawFramesPerPlayers[player] = new byte[0];
                            }
                        }
                    }
                }

                base.Update();
            }
            catch (System.Exception ex)
            {
                UMI3DLogger.LogError("Error while sending tracking frame", debugScope);
                UMI3DLogger.LogException(ex, debugScope);
            }
        }

        /// <inheritdoc/>
        protected override byte[] GetMessage(List<NetworkingPlayer> fromPlayers)
        {
            if (UMI3DEnvironment.Instance.useDto)
            {
                return (new UMI3DDtoListDto<UserTrackingFrameDto>() { values = fromPlayers.Select(player => framesPerSource[player][0]).ToList() }).ToBson();
            }
            else
            {
                tempMessage.Clear();
                tempMessage.Add(UMI3DObjectKeys.CountArray);
                tempMessage.AddRange(UMI3DSerializer.Write(fromPlayers.Count).ToBytes());
                UnityEngine.Debug.Assert(tempMessage.Count == 5, tempMessage.Count);

                foreach (NetworkingPlayer player in fromPlayers)
                {
                    if (tempRawFramesPerPlayers.ContainsKey(player))
                        tempMessage.AddRange(tempRawFramesPerPlayers[player]);
                    else
                        UnityEngine.Debug.LogError($"Impossible to find tracking frame from {player.NetworkId}");
                }

                return tempMessage.ToArray();
            }
        }

        public void SetFrame(NetworkingPlayer source, UserTrackingFrameDto frame)
        {
            SetFrame(source, new List<UserTrackingFrameDto> { frame });
        }
    }
}