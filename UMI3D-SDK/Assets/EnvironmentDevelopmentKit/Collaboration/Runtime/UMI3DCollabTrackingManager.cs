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
            UMI3DCollaborationServer.WebRTC.OpenChannel(user, "NonReliableTracking", DataType.Tracking, false);
            UMI3DCollaborationServer.WebRTC.OpenChannel(user, "ReliableTracking", DataType.Tracking, true);

            foreach (var userA in UMI3DCollaborationServer.Collaboration.Users)
            {
                if (userA == user) continue;
                if (!UMI3DCollaborationServer.WebRTC.ContainsChannel(userA, user, "Tracking"))
                {
                    UMI3DCollaborationServer.WebRTC.OpenChannel(userA, user, "NonReliableTracking", DataType.Tracking, false);
                    UMI3DCollaborationServer.WebRTC.OpenChannel(userA, user, "ReliableTracking", DataType.Tracking, true);
                }

            }
        }

        void closeChannel(UMI3DUser user1, UMI3DUser user2)
        {
            if (!UMI3DCollaborationServer.WebRTC.ContainsChannel(user1, user2, "Tracking"))
                UMI3DCollaborationServer.WebRTC.CloseChannel(user1, user2, "Tracking");
        }
    }
}
