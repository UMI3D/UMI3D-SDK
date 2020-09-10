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

using umi3d.common;
using umi3d.edk.userCapture;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    public class UMI3DCollabTrackingManager : UMI3DEmbodimentManager
    {
        public static new UMI3DCollabTrackingManager Instance { get { return UMI3DEmbodimentManager.Instance as UMI3DCollabTrackingManager; } set { UMI3DEmbodimentManager.Instance = value; } }

        protected override void Start()
        {
            base.Start();
            UMI3DCollaborationServer.Instance.OnUserJoin.AddListener(NewUser);
        }

        void NewUser(UMI3DUser user)
        {
            Debug.Log("Create Tracking channel with server");
            UMI3DCollaborationServer.WebRTC.OpenChannel(user, "Tracking", DataType.Tracking, false);

            foreach (var userA in UMI3DCollaborationServer.Collaboration.Users)
            {
                if (userA == user) continue;
                Debug.Log($"Trying create Tracking {userA.Id()}->{user.Id()}");
                if (!UMI3DCollaborationServer.WebRTC.ContainsChannel(userA, user, "Tracking"))
                {
                    Debug.Log("Creating Tracking channel");
                    UMI3DCollaborationServer.WebRTC.OpenChannel(userA, user, "Tracking", DataType.Tracking, false);
                }

            }
        }

        void closeChannel(UMI3DUser user1, UMI3DUser user2)
        {
            Debug.Log("Trying to close Tracking channel between two users");
            if (!UMI3DCollaborationServer.WebRTC.ContainsChannel(user1, user2, "Tracking"))
            {
                Debug.Log("Closing Tracking channel between two users");
                UMI3DCollaborationServer.WebRTC.CloseChannel(user1, user2, "Tracking");
            }
        }
    }
}
