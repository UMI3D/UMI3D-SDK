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
using umi3d.common.userCapture.description;

namespace umi3d.common.userCapture.tracking
{
    /// <summary>
    /// A request to inform about the current pose of the user.
    /// </summary>
    [Serializable]
    public class UserTrackingFrameDto : AbstractBrowserRequestDto
    {
        /// <summary>
        /// User id of the tracked user
        /// </summary>
        public ulong userId { get; set; }

        /// <summary>
        /// Id of the parent.
        /// </summary>
        public ulong parentId { get; set; }

        /// <summary>
        /// Bones information of the user
        /// </summary>
        public List<ControllerDto> trackedBones { get; set; }

        /// <summary>
        /// Applied pose clip Ids.
        /// </summary>
        public List<ulong> poses { get; set; } = new ();

        /// <summary>
        /// Current position of the user.
        /// </summary>
        public Vector3Dto position { get; set; }

        /// <summary>
        /// Current rotation of the user as a quaternion.
        /// </summary>
        public Vector4Dto rotation { get; set; }

        /// <summary>
        /// Speed of the user.
        /// </summary>
        public Vector3Dto speed { get; set; }

        /// <summary>
        /// True if user is on ground.
        /// </summary>
        public bool grounded { get; set; }

        /// <summary>
        /// True if user is jumping.
        /// </summary>
        public bool jumping { get; set; }

        /// <summary>
        /// True if user is crouching.
        /// </summary>
        public bool crouching { get; set; }

    }
}