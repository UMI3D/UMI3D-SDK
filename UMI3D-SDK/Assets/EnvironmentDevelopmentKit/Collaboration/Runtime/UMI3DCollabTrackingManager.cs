/*
Copyright 2019 Gfi Informatique

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

using System.Linq;
using umi3d.common;
using umi3d.common.collaboration;
using umi3d.edk.userCapture;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class UMI3DCollabTrackingManager : UMI3DEmbodimentManager
    {
        public static new UMI3DCollabTrackingManager Instance { get { return UMI3DEmbodimentManager.Instance as UMI3DCollabTrackingManager; } set { UMI3DEmbodimentManager.Instance = value; } }

        ///<inheritdoc/>
        protected override void Start()
        {
            base.Start();
            UMI3DCollaborationServer.Instance.OnUserJoin.AddListener(NewUser);
        }

        void NewUser(UMI3DUser user)
        {
            if (user is UMI3DCollaborationUser _user) NewUser(_user);
        }

        void NewUser(UMI3DCollaborationUser user)
        {
            Debug.Log("Create Tracking channel with server");
            user.dataChannels.Add(UMI3DCollaborationServer.WebRTC.CreateChannel(user, false, DataType.Tracking));
            UMI3DCollaborationServer.Collaboration.Users.Where(u => u != user)
                .Where(u => u.dataChannels.FirstOrDefault(dc => dc is BridgeChannel bridge && bridge.Equals(u, user, false, DataType.Tracking)) == default)
                .ForEach(u =>
                {
                    var dc = UMI3DCollaborationServer.WebRTC.CreateChannel(user, u, false, DataType.Tracking);
                    user.dataChannels.Add(dc);
                    u.dataChannels.Add(dc);
                });
        }

        void closeChannel(UMI3DCollaborationUser user1, UMI3DCollaborationUser user2)
        {

            var dc = user1.dataChannels.FirstOrDefault(d => d is BridgeChannel bridge && bridge.Equals(user1, user2, false, DataType.Tracking));
            if(dc != default)
            {
                user1.dataChannels.Remove(dc);
                user2.dataChannels.Remove(dc);
            }
        }
    }
}
