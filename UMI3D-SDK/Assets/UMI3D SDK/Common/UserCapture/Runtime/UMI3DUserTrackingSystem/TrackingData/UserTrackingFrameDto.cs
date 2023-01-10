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

namespace umi3d.common.userCapture
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
        public ulong userId;

        /// <summary>
        /// Id of the parent.
        /// </summary>
        public ulong parentId;

        /// <summary>
        /// Bones information of the user
        /// </summary>
        public List<BoneDto> bones;

        /// <summary>
        /// Current jump height of the avatar.
        /// </summary>
        /// Probably obsolete with the new navigation system.
        public float skeletonHighOffset;

        /// <summary>
        /// Current position of the user.
        /// </summary>
        public SerializableVector3 position;

        /// <summary>
        /// Current rotation of the user as a quaternion.
        /// </summary>
        public SerializableVector4 rotation;

        /// <summary>
        /// Frequency in frame per second (FPS) at which the user tracking is sent to the server.
        /// </summary>
        public float refreshFrequency;

        /// <inheritdoc/>
        protected override uint GetOperationId() { return UMI3DOperationKeys.UserTrackingFrame; }

        /// <inheritdoc/>
        public override Bytable ToBytableArray(params object[] parameters)
        {
            return base.ToBytableArray(parameters)
                + UMI3DSerializer.Write(userId)
                + UMI3DSerializer.Write(parentId)
                + UMI3DSerializer.Write(skeletonHighOffset)
                + UMI3DSerializer.Write(position)
                + UMI3DSerializer.Write(rotation)
                + UMI3DSerializer.Write(refreshFrequency)
                + UMI3DSerializer.WriteIBytableCollection(bones);
        }
    }
}
