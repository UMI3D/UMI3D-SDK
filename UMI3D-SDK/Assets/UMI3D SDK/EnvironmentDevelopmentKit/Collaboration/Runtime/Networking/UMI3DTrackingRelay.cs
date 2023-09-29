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
using System.Threading;
using BeardedManStudios.Forge.Networking;
using System.Linq;
using inetum.unityUtils;
using System;
using umi3d.common.userCapture.tracking;

namespace umi3d.edk.collaboration.tracking
{
    public class UMI3DTrackingRelay : UMI3DToUserRelay<UserTrackingFrameDto>
    {
        public UMI3DTrackingRelay(IForgeServer server) : base(server)
        {
            dataChannel = DataChannelTypes.Tracking;
        }

        /// <inheritdoc/>
        protected override byte[] GetMessage(List<UserTrackingFrameDto> frames)
        {
            if (UMI3DEnvironment.Instance.useDto)
                return (new UMI3DDtoListDto<UserTrackingFrameDto>() { values = frames }).ToBson();
            else
                return UMI3DSerializer.WriteCollection(frames).ToBytes();

        }
    }

    public abstract class ThreadLoop
    {
        protected bool running { get; private set; } = false;
        private Thread sendAvatarFramesThread = null;
        private int millisecondsTimeOut;
        protected int MillisecondsTimeOut
        {
            get => millisecondsTimeOut;
            set
            {
                if(value > 0)
                    millisecondsTimeOut = value;
            }
        }

        protected void StartLoop()
        {
            OnLoopStart();
            sendAvatarFramesThread = new Thread(new ThreadStart(Looper));
            sendAvatarFramesThread.Start();
        }

        protected void StopLoop()
        {
            running = false;
            sendAvatarFramesThread = null;
        }

        /// <summary>
        /// Sends <see cref="UserTrackingFrameDto"/> every tick.
        /// </summary>
        private void Looper()
        {
            running = true;
            while (running)
            {
                try
                {
                    Update();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                Thread.Sleep(MillisecondsTimeOut);
            }
            OnLoopStop();
        }

        protected abstract void Update();
        protected virtual void OnLoopStart() { }
        protected virtual void OnLoopStop() { }
    }

    public abstract class UMI3DRelay<To,Source,Frame> : ThreadLoop where To : class where Source : class where Frame : class
    {
        protected readonly object framesPerSourceLock = new();
        protected readonly object lastFrameSentToLock = new();

        protected Dictionary<Source, Frame> framesPerSource = new();
        protected Dictionary<To, Dictionary<Source, Frame>> lastFrameSentTo = new();

        public void RemoveSource(Source source)
        {
            lock(framesPerSourceLock)
                framesPerSource.Remove(source);
            lock(lastFrameSentToLock)
                lastFrameSentTo.Select(k => k.Value).ForEach(d =>d.Remove(source));
        }

        public void RemoveTo(To to)
        {
            lock (lastFrameSentToLock)
                lastFrameSentTo.Remove(to);
        }

        public void Clear()
        {
            lock (framesPerSourceLock)
                framesPerSource.Clear();
            lock (lastFrameSentToLock)
                lastFrameSentTo.Clear();
        }

        public void SetFrame(Source source, Frame frame) {
            if(source != null)
                framesPerSource[source] = frame;
        }

        protected abstract IEnumerable<To> GetTargets();
        protected abstract ulong GetTime();
        protected abstract void Send(To to, List<Frame> frames, bool force);

        public bool forceSendToAll;

        protected UMI3DRelay()
        {
            UMI3DCollaborationServer.Instance.OnServerStart.AddListener(() => {
                StartLoop();
            });

            UMI3DCollaborationServer.Instance.OnServerStop.AddListener(() => {
                StopLoop();
            });

            QuittingManager.OnApplicationIsQuitting.AddListener(() => {
                StopLoop();
            });

#if UNITY_EDITOR
            Application.quitting += () =>
            {
                StopLoop();
            };
#endif
        }

        protected override void Update()
        {
            ulong time = GetTime(); //introduce wrong time. TB tested with frame.timestep

            KeyValuePair<Source,Frame>[] _framesPerSource;


            var r = new System.Random();
            lock (framesPerSourceLock)
                _framesPerSource = framesPerSource.OrderBy(s => r.Next()).ToArray();

            var targets = GetTargets();
            foreach(var target in targets)
            {
                if (target != null)
                    continue;

                (List<Frame> frames, bool force) = GetFramesToSend(target, time, _framesPerSource);

                if (frames.Count == 0)
                    continue;

                Send(target, frames, forceSendToAll || force);
            }

            if (forceSendToAll)
                forceSendToAll = false;
        }


        protected virtual (List<Frame> frames, bool force) GetFramesToSend(To to, ulong time, KeyValuePair<Source, Frame>[] framesPerSource)
        {
            List<Frame> frames = new();
            lock (lastFrameSentToLock)
            {
                if (!lastFrameSentTo.ContainsKey(to))
                    lastFrameSentTo.Add(to, new Dictionary<Source, Frame>());

                foreach (var kFrame in framesPerSource)
                {
                    if (!lastFrameSentTo[to].ContainsKey(kFrame.Key) || lastFrameSentTo[to][kFrame.Key] != kFrame.Value)
                    {
                        lastFrameSentTo[to][kFrame.Key] = kFrame.Value;
                        frames.Add(kFrame.Value);
                    }
                }
            }
            return (frames, false);
        }
    }

    public abstract class UMI3DToUserRelay<Frame> : UMI3DRelay<UMI3DCollaborationUser, NetworkingPlayer, Frame> where Frame : class
    {
        IForgeServer server;
        protected DataChannelTypes dataChannel = DataChannelTypes.Data;

        protected UMI3DToUserRelay(IForgeServer server) : base()
        {
            this.server = server;

            UMI3DCollaborationServer.Instance.OnUserLeave.AddListener(u => 
            {
                if (u is UMI3DCollaborationUser uc)
                {
                    RemoveTo(uc);
                    RemoveSource(uc.networkPlayer);
                }
            });

            UMI3DCollaborationServer.Instance.OnUserActive.AddListener((user) => forceSendToAll = true);
        }

        protected override IEnumerable<UMI3DCollaborationUser> GetTargets()
        {
            var r = new System.Random();
            return UMI3DCollaborationServer.Collaboration.Users.OrderBy(s => r.Next());
        }

        protected override ulong GetTime()
        {
            return server.Time;
        }

        protected override void Send(UMI3DCollaborationUser to, List<Frame> frames, bool force)
        {
            server.RelayBinaryDataTo((int)dataChannel, to.networkPlayer, GetMessage(frames), force);
        }

        abstract protected byte[] GetMessage(List<Frame> frames);

        /// <summary>
        /// Returns all <see cref="UserTrackingFrameDto"/> that <paramref name="to"/> should received.
        /// </summary>
        /// <param name="to"></param>
        protected override (List<Frame> frames, bool force) GetFramesToSend(UMI3DCollaborationUser user, ulong time, KeyValuePair<NetworkingPlayer, Frame>[] framesPerSource)
        {
            bool forceRelay = false;

            List<Frame> frames = new List<Frame>();

            if (user == null)
                return (frames, false);

            KeyValuePair<NetworkingPlayer, Frame>[] userFrameMap = null;
            RelayVolume relayVolume;
            if (user is UMI3DCollaborationUser cUser && cUser?.RelayRoom != null && RelayVolume.relaysVolumes.TryGetValue(cUser.RelayRoom.Id(), out relayVolume) && relayVolume.HasStrategyFor(DataChannelTypes.Tracking))
            {
                var users = relayVolume.RelayTrackingRequest(null, null, user, Receivers.Others).Select(u => u as UMI3DCollaborationUser).ToList();
                userFrameMap = framesPerSource.Where(p => users.Any(u => u?.networkPlayer == p.Key)).ToArray();
                forceRelay = true;
            }
            else
            {
                userFrameMap = framesPerSource;
            }

            foreach (var other in userFrameMap)
            {
                if (user.networkPlayer == other.Key)
                    continue;

                if (forceSendToAll || forceRelay)
                {
                    frames.Add(other.Value);
                }
                else if (ShouldRelay((int)(int)DataChannelTypes.Tracking, user.networkPlayer, other.Key, time, BeardedManStudios.Forge.Networking.Receivers.Target))
                {

                    if (!lastFrameSentTo.ContainsKey(user))
                    {
                        lastFrameSentTo.Add(user, new());
                    }

                    if (!lastFrameSentTo[user].ContainsKey(other.Key))
                    {
                        lastFrameSentTo[user][other.Key] = other.Value;
                        frames.Add(other.Value);
                        RememberRelay(user.networkPlayer, other.Key, time);
                    }
                    else
                    {
                        if (lastFrameSentTo[user][other.Key] != other.Value)
                        {
                            frames.Add(other.Value);
                            lastFrameSentTo[user][other.Key] = other.Value;
                            RememberRelay(user.networkPlayer, other.Key, time);
                        }
                    }
                }
            }

            return (frames, forceSendToAll);
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