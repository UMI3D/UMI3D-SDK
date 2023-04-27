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

using umi3d.common.userCapture;
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System.Linq;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// <see cref="UMI3DUser"/> with a UMI3D Avatar attached to it.
    /// </summary>
    public class UMI3DTrackedUser : UMI3DUser
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.UserCapture | DebugScope.User;

        /// <summary>
        /// User's tracking current state description
        /// </summary>
        public UserTrackingFrameDto CurrentTrackingFrame;

        UMI3DAsyncProperty<Vector3> userSize;

        public UMI3DTrackedUser(ulong id) : base()
        {
            base.userId = id;
        }

        static object joinLock = new object();
        public async Task JoinDtoReception( SerializableVector3 userSize, PoseDto[] userPoses)
        {
            lock (joinLock)
            {
                UMI3DLogger.Log("PoseManager.JoinDtoReception before " + userId, scope);

                if (this.userSize.GetValue() == userSize)
                    UMI3DLogger.LogWarning("Internal error : the user size is already registered", scope);
                else
                    this.userSize.SetValue(userSize);
            }

            await PoseManager.Instance.InitNewUserPoses(this, userPoses.ToList());
            await UMI3DAsyncManager.Yield();

            UMI3DLogger.Log("PoseManager.JoinDtoReception end " + userId, scope);
        }
    }
}