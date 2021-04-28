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

using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.collaboration;
using umi3d.edk;
using umi3d.edk.userCapture;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class RelayVolume : MonoBehaviour, ICollaborationRoom
    {
        public static Dictionary<string, RelayVolume> relaysVolumes = new Dictionary<string, RelayVolume>(); 

        [Serializable]
        public struct RelayAssociation
        {
            public DataChannelTypes channel;
            public RelayDescription relay;
        }

        [SerializeField]
        private RelayAssociation[] relays;

        protected Dictionary<DataChannelTypes, RelayDescription> DicoRelays = new Dictionary<DataChannelTypes, RelayDescription>();

        protected string volumeId;

        protected Dictionary<string, Dictionary<string, float>> relayDataMemory = new Dictionary<string, Dictionary<string, float>>();
        protected Dictionary<string, Dictionary<string, float>> relayTrackingMemory = new Dictionary<string, Dictionary<string, float>>();
        protected Dictionary<string, Dictionary<string, float>> relayVoIPMemory = new Dictionary<string, Dictionary<string, float>>();
        protected Dictionary<string, Dictionary<string, float>> relayVideoMemory = new Dictionary<string, Dictionary<string, float>>();

        private void Awake()
        {
            RelayVolume.relaysVolumes.Add(this.VolumeId(), this);
            DicoRelays = relays.ToDictionary(p => p.channel, p => p.relay);
        }

        public string VolumeId()
        {
            if (volumeId == null)
            {
                byte[] key = Guid.NewGuid().ToByteArray();
                volumeId = Convert.ToBase64String(key);
            }
            return volumeId;
        }

        public RelayDescription RelayDescription(DataChannelTypes channel)
        {
            return DicoRelays[channel];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">Node associated to the request</param>
        /// <param name="data"></param>
        /// <param name="target"></param>
        /// <param name="receiverSetting"></param>
        /// <param name="isReliable"></param>
        public void RelayDataRequest(UMI3DAbstractNode sender, byte[] data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false)
        {
            float now = Time.time;

            HashSet<UMI3DCollaborationUser> targetHashSet = GetTargetHashSet(target, receiverSetting);
            
            if (targetHashSet != null)
            {
                foreach (UMI3DCollaborationUser user in targetHashSet)
                {
                    if (ShouldRelay(sender, user, DataChannelTypes.Data, now))
                    {
                        RememberRelay(sender, user, DataChannelTypes.Data, now);
                        DispatchTransaction(user, data, DataChannelTypes.Data, isReliable);
                    }
                }
            }
        }

        public void RelayTrackingRequest(UMI3DAbstractNode sender, byte[] data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false)
        {
            float now = Time.time;

            HashSet<UMI3DCollaborationUser> targetHashSet = GetTargetHashSet(target, receiverSetting);

            if (targetHashSet != null)
            {
                foreach (UMI3DCollaborationUser user in targetHashSet)
                {
                    if (ShouldRelay(sender, user, DataChannelTypes.Tracking, now))
                    {
                        RememberRelay(sender, user, DataChannelTypes.Tracking, now);
                        DispatchTransaction(user, data, DataChannelTypes.Tracking, isReliable);
                    }
                }
            }
        }

        public void RelayVoIPRequest(UMI3DAbstractNode sender, byte[] data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false)
        {
            float now = Time.time;

            HashSet<UMI3DCollaborationUser> targetHashSet = GetTargetHashSet(target, receiverSetting);

            if (targetHashSet != null)
            {
                foreach (UMI3DCollaborationUser user in targetHashSet)
                {
                    if (ShouldRelay(sender, user, DataChannelTypes.VoIP, now))
                    {
                        RememberRelay(sender, user, DataChannelTypes.VoIP, now);
                        DispatchTransaction(user, data, DataChannelTypes.VoIP, isReliable);
                    }
                }
            }
        }

        public void RelayVideoRequest(UMI3DAbstractNode sender, byte[] data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false)
        {
            float now = Time.time;

            HashSet<UMI3DCollaborationUser> targetHashSet = GetTargetHashSet(target, receiverSetting);

            if (targetHashSet != null)
            {
                foreach (UMI3DCollaborationUser user in targetHashSet)
                {
                    if (ShouldRelay(sender, user, DataChannelTypes.Video, now))
                    {
                        RememberRelay(sender, user, DataChannelTypes.Video, now);
                        DispatchTransaction(user, data, DataChannelTypes.Video, isReliable);
                    }
                }
            }
        }
        
        protected HashSet<UMI3DCollaborationUser> GetTargetHashSet(UMI3DUser target, Receivers receiverSetting)
        {
            switch (receiverSetting)
            {
                case Receivers.All:
                    return (HashSet<UMI3DCollaborationUser>)UMI3DCollaborationServer.Collaboration.Users;
                case Receivers.Others:
                    return (HashSet<UMI3DCollaborationUser>)(UMI3DCollaborationServer.Collaboration.Users.Where(u => u.Id() != target.Id()));
                case Receivers.Target:
                    return new HashSet<UMI3DCollaborationUser>() { target as UMI3DCollaborationUser };
                default:
                    return null;
            }
        }

        protected bool ShouldRelay(UMI3DAbstractNode sender, UMI3DCollaborationUser to, DataChannelTypes channel, float now)
        {
            if (to.status != common.StatusType.ACTIVE)
                return false;

            RelayDescription relay = DicoRelays[channel];
            RelayDescription.Strategy strategy = sender.room.Equals(this) ? relay.InsideVolume : relay.OutsideVolume;

            if (strategy.sendData)
            {
                Dictionary<string, Dictionary<string, float>> relayMemory;

                switch (strategy.sendingStrategy)
                {
                    case collaboration.RelayDescription.SendingStrategy.AlwaysSend:
                        return true;

                    case collaboration.RelayDescription.SendingStrategy.Fixed:
                        relayMemory = GetRelayMemory(channel);

                        if (relayMemory != null)
                        {
                            if (!relayMemory.ContainsKey(sender.Id()) || !relayMemory[sender.Id()].ContainsKey(to.Id()))
                                return true;
                            else
                            {
                                float StrategyDelay = 1 / strategy.constantFPS;
                                float CurrentDelay = now - relayMemory[sender.Id()][to.Id()];

                                return StrategyDelay <= CurrentDelay;
                            }
                        }
                        return false;

                    case collaboration.RelayDescription.SendingStrategy.Proximity:
                        relayMemory = GetRelayMemory(channel);

                        if (relayMemory != null)
                        {
                            if (!relayMemory.ContainsKey(sender.Id()) || !relayMemory[sender.Id()].ContainsKey(to.Id()))
                                return true;
                            else
                            {
                                float dist = 0f;
                                if (channel == DataChannelTypes.Tracking)
                                {
                                    UMI3DCollaborationUser userSender = UMI3DCollaborationServer.Collaboration.GetUser((sender as UMI3DAvatarNode).userId);
                                    dist = Vector3.Distance(to.Avatar.objectPosition.GetValue(userSender), sender.objectPosition.GetValue(to));
                                }
                                else
                                    dist = Vector3.Distance(to.Avatar.objectPosition.GetValue(), sender.objectPosition.GetValue(to));

                                float coeff = 0f;
                                if (dist > strategy.startingProximityDistance && dist < strategy.stoppingProximityDistance)
                                {
                                    coeff = (dist - strategy.startingProximityDistance) / (strategy.stoppingProximityDistance - strategy.startingProximityDistance);
                                }
                                else if (dist >= strategy.stoppingProximityDistance)
                                    coeff = 0f;

                                float StrategyDelay = Mathf.RoundToInt((1f - coeff) * (1 / strategy.maxProximityFPS) + coeff * (1 / strategy.minProximityFPS));
                                float CurrentDelay = now - relayMemory[sender.Id()][to.Id()];

                                return StrategyDelay <= CurrentDelay;
                            }
                        }
                        return false;

                    default:
                        return true;
                }
            }
            else
                return false;
        }

        protected void RememberRelay(UMI3DAbstractNode sender, UMI3DCollaborationUser to, DataChannelTypes channel, float now)
        {
            Dictionary<string, Dictionary<string, float>> relayMemory = GetRelayMemory(channel);

            if (relayMemory != null)
            {
                if (!relayMemory.ContainsKey(sender.Id()))
                    relayMemory.Add(sender.Id(), new Dictionary<string, float>());

                if (!relayMemory[sender.Id()].ContainsKey(to.Id()))
                    relayMemory[sender.Id()].Add(to.Id(), now);
                else
                    relayMemory[sender.Id()][to.Id()] = now;
            }
        }

        protected Dictionary<string, Dictionary<string, float>> GetRelayMemory(DataChannelTypes channel)
        {
            switch (channel)
            {
                case DataChannelTypes.Tracking:
                    return relayTrackingMemory;
                case DataChannelTypes.Data:
                    return relayDataMemory;
                case DataChannelTypes.Video:
                    return relayVideoMemory;
                case DataChannelTypes.VoIP:
                    return relayVoIPMemory;
                default:
                    return null;
            }
        }

        protected void DispatchTransaction(UMI3DCollaborationUser to, byte[] data, DataChannelTypes channel, bool isReliable)
        {
            UMI3DCollaborationServer.ForgeServer.RelayBinaryDataTo((int)channel, to.networkPlayer, data, isReliable);
        }
    }
}