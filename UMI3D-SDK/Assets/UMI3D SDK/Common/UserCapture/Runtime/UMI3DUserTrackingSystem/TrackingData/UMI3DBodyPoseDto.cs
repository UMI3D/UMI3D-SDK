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
    /// DTO for body poses.
    /// </summary>
    /// Body poses are predefined poses that are associated with specific interactions and constraint the skeleton of the avatar.
    /// For example, entering in a car will force the skeleton to be sat if such a body pose is designed.
    public class UMI3DBodyPoseDto : AbstractEntityDto, IEntity
    {
        /// <summary>
        /// Body pose name
        /// </summary>
        public string Name;

        /// <summary>
        /// Is the pose currently active?
        /// </summary>
        public bool IsActive;
        /// <summary>
        /// Does the pose occurs at a precise place relative to the node?
        /// </summary>
        public bool IsRelativeToNode;
        public bool AllowOverriding;

        /// <summary>
        /// Relative position of the body pose reference point to the object.
        /// </summary>
        public SerializableVector3 BodyPosition;
        /// <summary>
        /// Relative rotation of the body pose reference point to the object.
        /// </summary>
        /// Rotation is expressed using Euler's coordinates.
        public SerializableVector3 BodyEulerRotation;

        //public Dictionary<uint, SerializableVector3> JointRotations = new Dictionary<uint, SerializableVector3>();

        public Dictionary<uint, KeyValuePair<SerializableVector3, SerializableVector3>> TargetTransforms = new Dictionary<uint, KeyValuePair<SerializableVector3, SerializableVector3>>();
    }
}