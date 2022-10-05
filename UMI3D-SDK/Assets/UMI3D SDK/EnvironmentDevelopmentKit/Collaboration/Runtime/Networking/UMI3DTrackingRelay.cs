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
using umi3d.common.userCapture;
using umi3d.common;
using UnityEngine;
using System.Threading;
using BeardedManStudios.Forge.Networking;
using System.Linq;
using inetum.unityUtils;
using System;

namespace umi3d.edk.collaboration
{
    public class UMI3DTrackingRelay : UMI3DRelay<UserTrackingFrameDto>
    {
        public UMI3DTrackingRelay(UMI3DForgeServer server) : base(server)
        {
        }

        protected override byte[] GetMessage(List<UserTrackingFrameDto> frames)
        {
            if (UMI3DEnvironment.Instance.useDto)
                return (new UMI3DDtoListDto<UserTrackingFrameDto>() { values = frames }).ToBson();
            else
                return UMI3DNetworkingHelper.WriteCollection(frames).ToBytes();

        }
    }


    public abstract class UMI3DRelay<T> where T : class
    {

        #region Fields
        UMI3DForgeServer server;

        /// <summary>
        /// Stores all <see cref="T"/> received from all <see cref="UnityEngine.NetworkPlayer"/>.
        /// </summary>
        Dictionary<NetworkingPlayer, T> framesPerPlayer = new Dictionary<NetworkingPlayer, T>();

        /// <summary>
        /// Collection used to prevent sending the same <see cref="UserTrackingFrameDto"/> twice to the same <see cref="NetworkPlayer"/>.
        /// </summary>
        Dictionary<NetworkingPlayer, Dictionary<NetworkingPlayer, T>> lastFrameSentToAPlayer = new Dictionary<NetworkingPlayer, Dictionary<NetworkingPlayer, T>>();

        /// <summary>
        /// Thread used to send trackingFrames.
        /// </summary>
        private Thread sendAvatarFramesThread;

        /// <summary>
        /// If true, all <see cref="T"/> of <see cref="framesPerPlayer"/> must be sent to everyone.
        /// </summary>
        private bool forceSendtrackingFrames = false;

        bool running = false;

        #endregion

        public UMI3DRelay(UMI3DForgeServer server)
        {
            this.server = server;
            InitTrackingFrameThread();
        }

        /// <summary>
        /// Set last frame for a player
        /// </summary>
        /// <param name="from">the player</param>
        /// <param name="frame">last frame</param>
        public void SetFrame(NetworkingPlayer from, T frame)
        {
            lock (framesPerPlayer)
            {
                framesPerPlayer[from] = frame;
            }
        }

        ulong GetTime()
        {
            return server.time;
        }

        private void InitTrackingFrameThread()
        {
            UMI3DCollaborationServer.Instance.OnServerStart.AddListener(() => {
                sendAvatarFramesThread = new Thread(new ThreadStart(SendTrackingFramesLoop));
                sendAvatarFramesThread.Start();
            });
            UMI3DCollaborationServer.Instance.OnServerStop.AddListener(() => {
                running = false;
                sendAvatarFramesThread = null;
            });

            UMI3DCollaborationServer.Instance.OnUserLeave.AddListener((user) =>
            {
                lock (framesPerPlayer)
                {
                    var player = framesPerPlayer.Keys.ToList().Find(p => p.NetworkId == user.Id());

                    if (player != null && framesPerPlayer.ContainsKey(player))
                        framesPerPlayer.Remove(player);
                }
            });

            UMI3DCollaborationServer.Instance.OnUserActive.AddListener((user) => forceSendtrackingFrames = true);
        }

        /// <summary>
        /// Sends <see cref="UserTrackingFrameDto"/> every tick.
        /// </summary>
        private void SendTrackingFramesLoop()
        {
            running = true;
            while (running)
            {
                try
                {
                    SendTrackingFrames();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Sends <see cref="UserTrackingFrameDto"/> to every player if they should received them.
        /// </summary>
        private void SendTrackingFrames()
        {
            ulong time = GetTime(); //introduce wrong time. TB tested with frame.timestep

            KeyValuePair<NetworkingPlayer, T>[] _framesPerPlayer;
            lock (framesPerPlayer)
            {
                var r = new System.Random();
                _framesPerPlayer = framesPerPlayer.OrderBy(s => r.Next()).ToArray();
            }
            foreach (var avatarFrameEntry in _framesPerPlayer)
            {
                UMI3DCollaborationUser user = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(avatarFrameEntry.Key.NetworkId);

                if (user == null)
                    continue;

                (List<T> frames, bool force) = GetTrackingFrameToSend(user, time, _framesPerPlayer);

                if (frames.Count == 0)
                    continue;

                server.RelayBinaryDataTo((int)DataChannelTypes.Tracking, avatarFrameEntry.Key, GetMessage(frames), force || forceSendtrackingFrames);
            }

            if (forceSendtrackingFrames)
                forceSendtrackingFrames = false;

        }

        abstract protected byte[] GetMessage(List<T> frames);

        /// <summary>
        /// Returns all <see cref="UserTrackingFrameDto"/> that <paramref name="to"/> should received.
        /// </summary>
        /// <param name="to"></param>
        private (List<T>, bool) GetTrackingFrameToSend(UMI3DCollaborationUser user, ulong time, KeyValuePair<NetworkingPlayer, T>[] framesPerPlayer)
        {
            bool forceRelay = false;
            NetworkingPlayer to = user?.networkPlayer;

            List<T> frames = new List<T>();

            if (to == null || user == null)
                return (frames, false);

            KeyValuePair<NetworkingPlayer, T>[] userFrameMap = null;
            RelayVolume relayVolume;
            if (user is UMI3DCollaborationUser cUser && cUser?.Avatar?.RelayRoom != null && RelayVolume.relaysVolumes.TryGetValue(cUser.Avatar.RelayRoom.Id(), out relayVolume) && relayVolume.HasStrategyFor(DataChannelTypes.Tracking))
            {
                var users = relayVolume.RelayTrackingRequest(null, null, user, Receivers.Others).Select(u => u as UMI3DCollaborationUser).ToList();
                userFrameMap = framesPerPlayer.Where(p => users.Any(u => u?.networkPlayer == p.Key)).ToArray();
                forceRelay = true;
            }
            else
            {
                userFrameMap = framesPerPlayer;
            }

            foreach (var other in userFrameMap)
            {
                if (to == other.Key)
                    continue;

                if (forceSendtrackingFrames || forceRelay)
                {
                    frames.Add(other.Value);
                }
                else if (ShouldRelay((int)(int)DataChannelTypes.Tracking, to, other.Key, time, BeardedManStudios.Forge.Networking.Receivers.Target))
                {

                    if (!lastFrameSentToAPlayer.ContainsKey(to))
                    {
                        lastFrameSentToAPlayer.Add(to, new Dictionary<NetworkingPlayer, T>());
                    }

                    if (!lastFrameSentToAPlayer[to].ContainsKey(other.Key))
                    {
                        lastFrameSentToAPlayer[to][other.Key] = other.Value;
                        frames.Add(other.Value);
                        RememberRelay(to, other.Key, time);
                    }
                    else
                    {
                        if (lastFrameSentToAPlayer[to][other.Key] != other.Value)
                        {
                            frames.Add(other.Value);
                            lastFrameSentToAPlayer[to][other.Key] = other.Value;
                            RememberRelay(to, other.Key, time);
                        }
                    }
                }
            }

            return (frames, forceSendtrackingFrames);
        }


        #region proximity_relay

        /// <summary>
        /// 
        /// </summary>
        private static readonly List<BeardedManStudios.Forge.Networking.Receivers> Proximity = new List<BeardedManStudios.Forge.Networking.Receivers> { BeardedManStudios.Forge.Networking.Receivers.AllProximity, BeardedManStudios.Forge.Networking.Receivers.AllProximityGrid, BeardedManStudios.Forge.Networking.Receivers.OthersProximity, BeardedManStudios.Forge.Networking.Receivers.OthersProximityGrid };

        /// <summary>
        /// 
        /// </summary>
        protected ulong minProximityRelay = 200;

        protected uint maxFPSRelay = 5;

        /// <summary>
        /// 
        /// </summary>
        protected ulong maxProximityRelay = 1000;

        protected uint minFPSRelay = 1;

        object proximityLock = new object();
        /// <summary>
        /// 
        /// </summary>
        public ulong minProximityRelayFPS
        {
            get
            {
                lock (proximityLock)
                    return maxProximityRelay == 0 ? 0 : 1000 / maxProximityRelay;
            }

            set
            {
                lock (proximityLock)
                    if (value <= 0) maxProximityRelay = 0;
                    else maxProximityRelay = 1000 / value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ulong maxProximityRelayFPS
        {
            get
            {
                lock (proximityLock)
                    return minProximityRelay == 0 ? 0 : 1000 / minProximityRelay;
            }

            set
            {
                lock (proximityLock)
                    if (value <= 0) minProximityRelay = 0;
                    else minProximityRelay = 1000 / value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool proximityRelay = true;

        /// <summary>
        /// 
        /// </summary>
        public float startProximityAt = 3f;

        /// <summary>
        /// 
        /// </summary>
        public float proximityCutout = 20f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="timestep"></param>
        /// <param name="strategy"></param>
        /// <returns></returns>
        protected bool ShouldRelay(int groupId, NetworkingPlayer from, NetworkingPlayer to, ulong timestep, BeardedManStudios.Forge.Networking.Receivers strategy)
        {
            if (to.IsHost || from == to || UMI3DCollaborationServer.Collaboration?.GetUserByNetworkId(to.NetworkId)?.status != StatusType.ACTIVE)
                return false;
            if (Proximity.Contains(strategy))
            {
                ulong last = GetLastRelay(from, to, groupId);
                if (last > 0)
                {
                    ulong diff = timestep - last;
                    ulong currentDelay = GetCurrentDelay(from, to);
                    if (diff < currentDelay)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        protected ulong GetCurrentDelay(NetworkingPlayer from, NetworkingPlayer to)
        {
            UMI3DCollaborationUser user1 = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(from.NetworkId);
            UMI3DCollaborationUser user2 = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(to.NetworkId);
            float dist = Vector3.Distance(user1.Avatar.objectPosition.GetValue(user2), user2.Avatar.objectPosition.GetValue(user2));
            float coeff = 0f;
            if (dist > startProximityAt && dist < proximityCutout)
            {
                coeff = (dist - startProximityAt) / (proximityCutout - startProximityAt);
            }
            else if (dist >= proximityCutout)
            {
                coeff = 1f;
            }

            return (ulong)Mathf.RoundToInt(1000 / Mathf.Floor(((1f - coeff) * maxFPSRelay) + (coeff * minFPSRelay)));
        }

        //relayMemory[p1][p2][gi] = a ulong corresponding to the last time player p1 sent a message to p2 in the gi channel
        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<uint, Dictionary<uint, ulong>> relayMemory = new Dictionary<uint, Dictionary<uint, ulong>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="groupId"></param>
        /// <param name="time"></param>
        private void RememberRelay(NetworkingPlayer from, NetworkingPlayer to, ulong time)
        {
            uint p1 = from.NetworkId, p2 = to.NetworkId;
            if (!relayMemory.ContainsKey(p1))
                relayMemory.Add(p1, new Dictionary<uint, ulong>());
            Dictionary<uint, ulong> dicP1 = relayMemory[p1];
            dicP1[p2] = time;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        private ulong GetLastRelay(NetworkingPlayer from, NetworkingPlayer to, int groupId)
        {
            uint p1 = from.NetworkId, p2 = to.NetworkId;
            //no relay from p1
            if (!relayMemory.ContainsKey(p1))
                return 0;
            Dictionary<uint, ulong> dicP1 = relayMemory[p1];
            //no relay from p1 to P2
            if (!dicP1.ContainsKey(p2))
                return 0;

            return dicP1[p2];
        }

        #endregion
    }



}