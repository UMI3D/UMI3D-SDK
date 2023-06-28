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
using umi3d.common.userCapture.tracking;

namespace umi3d.edk.userCapture.tracking
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
    }
}