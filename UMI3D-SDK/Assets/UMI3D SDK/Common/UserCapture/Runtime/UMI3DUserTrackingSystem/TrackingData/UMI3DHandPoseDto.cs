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

using System.Collections.Generic;

namespace umi3d.common.userCapture
{
    /// <summary>
    /// DTO for hand poses
    /// </summary>
    /// Hand poses are predefined poses that are associated with specific interactions and constraint the hand of the avatar's skeleton.
    /// For example, grabbing a cup of tea will force the skeleton to place correctly its fingers if such a hand pose is designed.
    public class UMI3DHandPoseDto : AbstractEntityDto, IEntity
    {
        /// <summary>
        /// Hand pose name
        /// </summary>
        public string Name;

        /// <summary>
        /// Is the hand pose active?
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// Does the pose occurs at a precise place relative to the node?
        /// </summary>
        public bool isRelativeToNode;

        /// <summary>
        /// Is the hand pose triggered on hover?
        /// </summary>
        public bool HoverPose;

        /// <summary>
        /// Relative position of the right hand pose reference point to the object.
        /// </summary>
        public SerializableVector3 RightHandPosition;
        /// <summary>
        /// Relative rotation of the right hand pose reference point to the object.
        /// </summary>
        /// Rotation is expressed using Euler's coordinates.
        public SerializableVector3 RightHandEulerRotation;

        /// <summary>
        /// Relative position of the left hand pose reference point to the object.
        /// </summary>
        public SerializableVector3 LeftHandPosition;
        /// <summary>
        /// Relative rotation of the left hand pose reference point to the object.
        /// </summary>
        /// Rotation is expressed using Euler's coordinates.
        public SerializableVector3 LeftHandEulerRotation;

        /// <summary>
        /// Relative rotations of the phalanxes for the hand pose.
        /// </summary>
        public Dictionary<uint, SerializableVector3> PhalanxRotations = new Dictionary<uint, SerializableVector3>();
    }
}
