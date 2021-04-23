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
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class RelayVolume : MonoBehaviour, ICollaborationRoom
    {
        public RelayDescription Relay;

        protected string volumeId;

        public string VolumeId()
        {
            if (volumeId == null)
            {
                byte[] key = Guid.NewGuid().ToByteArray();
                volumeId = Convert.ToBase64String(key);
            }
            return volumeId;
        }

        public RelayDescription RelayDescription()
        {
            return Relay;
        }

        public void RelayDataRequest(byte[] sender, byte[] data, UMI3DCollaborationUser target, TargetEnum targetSetting)
        {
            HashSet<UMI3DCollaborationUser> targetHashSet = GetTargetHashSet(target, targetSetting);
            
            if (targetHashSet != null)
            {

            }
        }

        public void RelayTrackingRequest(byte[] sender, byte[] data, UMI3DCollaborationUser target, TargetEnum targetSetting)
        {
            HashSet<UMI3DCollaborationUser> targetHashSet = GetTargetHashSet(target, targetSetting);

            if (targetHashSet != null)
            {

            }
        }

        public void RelayVoIPRequest(byte[] sender, byte[] data, UMI3DCollaborationUser target, TargetEnum targetSetting)
        {
            HashSet<UMI3DCollaborationUser> targetHashSet = GetTargetHashSet(target, targetSetting);

            if (targetHashSet != null)
            {

            }
        }

        public void RelayVideoRequest(byte[] sender, byte[] data, UMI3DCollaborationUser target, TargetEnum targetSetting)
        {
            HashSet<UMI3DCollaborationUser> targetHashSet = GetTargetHashSet(target, targetSetting);

            if (targetHashSet != null)
            {

            }
        }
        
        protected HashSet<UMI3DCollaborationUser> GetTargetHashSet(UMI3DCollaborationUser target, TargetEnum targetSetting)
        {
            switch (targetSetting)
            {
                case TargetEnum.All:
                    return (HashSet<UMI3DCollaborationUser>)UMI3DCollaborationServer.Collaboration.Users;
                case TargetEnum.Other:
                    return (HashSet<UMI3DCollaborationUser>)(UMI3DCollaborationServer.Collaboration.Users.Where(u => u.Id() != target.Id()));
                case TargetEnum.Target:
                    return new HashSet<UMI3DCollaborationUser>() { target };
                default:
                    return null;
            }
        }

        protected void RelayMemory()
        {

        }

        protected void ShouldRelay()
        {

        }

        protected void DispatchTransaction()
        {

        }
    }
}