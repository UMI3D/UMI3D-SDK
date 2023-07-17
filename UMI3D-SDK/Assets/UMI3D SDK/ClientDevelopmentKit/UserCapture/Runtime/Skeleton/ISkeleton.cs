/*
Copyright 2019 - 2023 Inetum

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
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.tracking;
using umi3d.common.userCapture;
using UnityEngine;
using umi3d.cdk.userCapture.tracking;
using umi3d.cdk.userCapture.pose;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Representation of a human user, controlled by a player.
    /// </summary>
    public interface ISkeleton
    {
        /// <summary>
        /// Position and rotation of each bone, indexed by UMI3D <see cref="BoneType"/>.
        /// </summary>
        Dictionary<uint, s_Transform> Bones { get; }

        /// <summary>
        /// Subskeletons that compose the final skeleton.
        /// </summary>
        List<ISubskeleton> Skeletons { get; }

        /// <summary>
        /// Skeleton hiearchy used, with relative position between each bone.
        /// </summary>
        UMI3DSkeletonHierarchy SkeletonHierarchy { get; }

        /// <summary>
        /// Anchor of the skeleton hierarchy.
        /// </summary>
        Transform HipsAnchor { get; }

        /// <summary>
        /// Id of the user represented by this skeleton.
        /// </summary>
        public ulong UserId { get; }

        /// <summary>
        /// Subskeleton updated from tracked controllers.
        /// </summary>
        public TrackedSkeleton TrackedSkeleton { get; }

        /// <summary>
        /// Susbskeleton for body poses.
        /// </summary>
        public PoseSkeleton PoseSkeleton { get; }

        #region Data struture

        public class s_Transform
        {
            public Vector3 s_Position;
            public Quaternion s_Rotation;
        }

        public struct SavedTransform
        {
            public Transform obj;
            public Vector3 savedPosition;
            public Quaternion savedRotation;
            public Vector3 savedLocalScale;
            public Vector3 savedLossyScale;
        }

        #endregion Data struture

        /// <summary>
        /// Update the positions/rotation of bone of subskeletons based on the received frame.
        /// </summary>
        /// <param name="frame"></param>
        public void UpdateFrame(UserTrackingFrameDto frame);

        /// <summary>
        /// Update the skeleton bones based on subskeletons data.
        /// </summary>
        /// Merge the bones of each subskeleton to find a final state for each bone.
        /// <returns></returns>
        public ISkeleton Compute();
    }
}