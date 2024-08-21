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
        protected Dictionary<NetworkingPlayer, (int nbOfFrames, byte[] rawFrames)> tempRawFramesPerPlayers;

        /// <summary>
        /// Temp list to contain all bytes to snd to a user.
        /// </summary>
        private List<byte> tempMessage = new();

        private byte[] tempBytesArray;

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
                                tempBytesArray = UMI3DSerializer.Write(frames[0]).ToBytes();
                            else if (frames.Count > 1)
                                tempBytesArray = UMI3DSerializer.Write(frames).ToBytes()[5..]; // 5 = 1 (one byte for array type) + 4 (four bytes for array length)

                            tempRawFramesPerPlayers[player] = (frames.Count, tempBytesArray);
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
            try
            {
                if (UMI3DEnvironment.Instance.useDto)
                {
                    return (new UMI3DDtoListDto<UserTrackingFrameDto>() { values = fromPlayers.Select(player => framesPerSource[player][0]).ToList() }).ToBson();
                }
                else
                {
                    tempMessage.Clear();
                    tempMessage.Add(UMI3DObjectKeys.CountArray);
                    int nbOfFrames = fromPlayers.Sum(p =>
                    {
                        if (tempRawFramesPerPlayers.ContainsKey(p))
                            return tempRawFramesPerPlayers[p].nbOfFrames;
                        else return 0;
                    });
                    tempMessage.AddRange(UMI3DSerializer.Write(nbOfFrames).ToBytes());
                    UnityEngine.Debug.Assert(tempMessage.Count == 5, tempMessage.Count);

                    foreach (NetworkingPlayer player in fromPlayers)
                    {
                        if (tempRawFramesPerPlayers.ContainsKey(player))
                            tempMessage.AddRange(tempRawFramesPerPlayers[player].rawFrames);
                        else
                            UnityEngine.Debug.LogError($"Impossible to find tracking frame from {player.NetworkId}");
                    }

                    return tempMessage.ToArray();
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
                return new byte[0];
            }
        }

        public void SetFrame(NetworkingPlayer source, UserTrackingFrameDto frame)
        {
            SetFrame(source, new List<UserTrackingFrameDto> { frame });
        }
    }
}