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

using UnityEngine;

namespace umi3d.edk.userCapture
{
    /// <summary>
    /// Bone of a user.
    /// </summary>
    public class UMI3DUserEmbodimentBone
    {
        /// <summary>
        /// Structure for bone positionning. Based on rotations.
        /// </summary>
        public struct SpatialPosition
        {
            /// <summary>
            /// Rotation relative to its parent.
            /// </summary>
            public Quaternion localRotation;
        }

        /// <summary>
        /// User id in <see cref="UMI3DEmbodimentManager"/>.
        /// </summary>
        public ulong userId { get; protected set; }

        /// <summary>
        /// Bone type in UMI3D standards in <see cref="common.userCapture.BoneType"/>
        /// </summary>
        public uint boneType { get; protected set; }

        /// <summary>
        /// Current spatial position of the bone relative to its parent.
        /// </summary>
        public SpatialPosition spatialPosition;

        /// <summary>
        /// Is the bone tracked?
        /// </summary>
        public bool isTracked;

        public UMI3DUserEmbodimentBone(ulong userId, uint boneType)
        {
            this.userId = userId;
            this.boneType = boneType;
            spatialPosition = new SpatialPosition();
        }
    }
}
