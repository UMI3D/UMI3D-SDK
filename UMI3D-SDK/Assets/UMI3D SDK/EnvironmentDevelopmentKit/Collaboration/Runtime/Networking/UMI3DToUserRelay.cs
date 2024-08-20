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
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using System.Linq;

namespace umi3d.edk.collaboration
{
    public abstract class UMI3DToUserRelay<Frame> : UMI3DRelay<UMI3DCollaborationAbstractContentUser, NetworkingPlayer, Frame> where Frame : class
    {
        IForgeServer server;
        protected DataChannelTypes dataChannel = DataChannelTypes.Data;
        private System.Random random;
        private List<KeyValuePair<NetworkingPlayer, Frame>> tempUserFrameMap = new();

        protected UMI3DToUserRelay(IForgeServer server) : base()
        {
            this.server = server;
            this.random = new();

            UMI3DCollaborationServer.Instance.OnUserLeave.AddListener(u =>
            {
                if (u is UMI3DCollaborationAbstractContentUser uc)
                {
                    RemoveTo(uc);
                    RemoveSource(uc.networkPlayer);
                }
            });

            UMI3DCollaborationServer.Instance.OnUserActive.AddListener((user) => forceSendToAll = true);
        }

        protected override IEnumerable<UMI3DCollaborationAbstractContentUser> GetTargets()
        {
            return UMI3DCollaborationServer.Collaboration.Users.Where(u => u.networkPlayer != null).OrderBy(s => random.Next());
        }

        protected override ulong GetTime()
        {
            return server.Time;
        }

        protected override void Send(UMI3DCollaborationAbstractContentUser to, List<NetworkingPlayer> sources, bool force)
        {
            server.RelayBinaryDataTo((int)dataChannel, to.networkPlayer, GetMessage(sources), force);
        }

        abstract protected byte[] GetMessage(List<NetworkingPlayer> fromPlayers);

        /// <summary>
        /// Returns all <see cref="UserTrackingFrameDto"/> that <paramref name="to"/> should received.
        /// </summary>
        /// <param name="to"></param>
        protected override bool GetSourcesToSendTo(UMI3DCollaborationAbstractContentUser userTo, ulong time, List<NetworkingPlayer> sources)
        {
            bool forceRelay = false;

            if (userTo == null)
                return false;

            RelayVolume relayVolume;

            tempUserFrameMap.Clear();

            lock (framesPerSourceLock)
            {
                if (userTo is UMI3DCollaborationAbstractContentUser cUser && cUser?.RelayRoom != null && RelayVolume.relaysVolumes.TryGetValue(cUser.RelayRoom.Id(), out relayVolume) && relayVolume.HasStrategyFor(DataChannelTypes.Tracking))
                {
                    var users = relayVolume.RelayTrackingRequest(null, null, userTo, Receivers.Others).Select(u => u as UMI3DCollaborationAbstractContentUser).ToList();
                    tempUserFrameMap.AddRange(framesPerSource.Where(p => users.Any(u => u?.networkPlayer == p.Key)));
                    forceRelay = true;
                }
                else
                {
                    tempUserFrameMap.AddRange(framesPerSource);
                }
            }

            foreach (var other in tempUserFrameMap)
            {
                if (userTo.networkPlayer == other.Key)
                    continue;

                if (forceSendToAll || forceRelay)
                {
                    sources.Add(other.Key);
                }
                else if (ShouldRelay((int)DataChannelTypes.Tracking, other.Key, userTo.networkPlayer, time, BeardedManStudios.Forge.Networking.Receivers.Target))
                {
                    if (!lastFrameSentTo.ContainsKey(userTo))
                    {
                        lastFrameSentTo.Add(userTo, new());
                    }

                    if (!lastFrameSentTo[userTo].ContainsKey(other.Key))
                    {
                        lastFrameSentTo[userTo][other.Key] = other.Value;
                        sources.Add(other.Key);
                        RememberRelay(userTo.networkPlayer, other.Key, time);
                    }
                    else
                    {
                        if (lastFrameSentTo[userTo][other.Key] != other.Value)
                        {
                            sources.Add(other.Key);
                            lastFrameSentTo[userTo][other.Key] = other.Value;
                            RememberRelay(userTo.networkPlayer, other.Key, time);
                        }
                    }
                }
            }

            return forceSendToAll;
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
            UMI3DCollaborationAbstractContentUser user1 = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(from.NetworkId);
            UMI3DCollaborationAbstractContentUser user2 = UMI3DCollaborationServer.Collaboration.GetUserByNetworkId(to.NetworkId);
            float dist = Vector3.Distance(user1.CurrentTrackingFrame.position.Struct(), user2.CurrentTrackingFrame.position.Struct());
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