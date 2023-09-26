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

using System;
using System.Collections.Generic;
using umi3d.cdk.userCapture.animation;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace umi3d.cdk.userCapture
{
    /// <summary>
    /// Representation of a human user, controlled by a player.
    /// </summary>
    public interface ISkeleton
    {
        UserTrackingFrameDto LastFrame { get; }

        /// <summary>
        /// Position and rotation of each bone, indexed by UMI3D <see cref="BoneType"/>.
        /// </summary>
        IDictionary<uint, Transformation> Bones { get; }

        /// <summary>
        /// Subskeletons that compose the final skeleton.
        /// </summary>
        IReadOnlyList<ISubskeleton> Subskeletons { get; }

        /// <summary>
        /// Skeleton hierarchy used, with relative position between each bone.
        /// </summary>
        UMI3DSkeletonHierarchy SkeletonHierarchy { get; }

        /// <summary>
        /// Anchor of the skeleton hierarchy.
        /// </summary>
        UnityEngine.Transform HipsAnchor { get; }

        /// <summary>
        /// Id of the user represented by this skeleton.
        /// </summary>
        ulong UserId { get; set; }

        /// <summary>
        /// Subskeleton updated from tracked controllers.
        /// </summary>
        ITrackedSubskeleton TrackedSubskeleton { get; }

        /// <summary>
        /// Susbskeleton for body poses.
        /// </summary>
        IPoseSubskeleton PoseSubskeleton { get; }

        /// <summary>
        /// Update the positions/rotation of bone of subskeletons based on the received frame.
        /// </summary>
        /// <param name="frame"></param>
        void UpdateBones(UserTrackingFrameDto frame);

        /// <summary>
        /// Update the skeleton bones based on subskeletons data.
        /// </summary>
        /// Merge the bones of each subskeleton to find a final state for each bone.
        /// <returns></returns>
        ISkeleton Compute();

        /// <summary>
        /// Get subskeleton camera parameters.
        /// </summary>
        /// <returns></returns>
        UserCameraPropertiesDto GetCameraDto();

        /// <summary>
        /// Add a subskeleton to the skeleton.
        /// </summary>
        /// <param name="subskeleton"></param>
        void AddSubskeleton(IAnimatedSubskeleton subskeleton);

        /// <summary>
        /// Add a subskeleton to the skeleton.
        /// </summary>
        /// <param name="subskeleton"></param>
        void RemoveSubskeleton(IAnimatedSubskeleton subskeleton);

        #region Data struture

        public class Transformation
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Quaternion LocalRotation;
        }

        #endregion Data struture
    }
}