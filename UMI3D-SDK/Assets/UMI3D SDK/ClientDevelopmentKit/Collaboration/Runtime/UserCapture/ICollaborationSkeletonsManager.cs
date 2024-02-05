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
using umi3d.cdk.userCapture;
using umi3d.common.userCapture.tracking;

namespace umi3d.cdk.collaboration.userCapture
{
    /// <summary>
    /// Skeleton manager that handles collaborative skeletons and personal skeleton in a collaborative context.
    /// </summary>
    public interface ICollaborationSkeletonsManager : ISkeletonManager
    {
        /// <summary>
        /// Collection of all skeletons indexed by user id.
        /// </summary>
        IReadOnlyDictionary<ulong, ISkeleton> Skeletons { get; }

        /// <summary>
        /// Scene where <see cref="CollaborativeSkeleton"/> are put.
        /// </summary>
        CollaborativeSkeletonsScene CollabSkeletonsScene { get; }

        /// <summary>
        /// Try to get a skeleton from a <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">User UMI3D id.</param>
        /// <returns>Return null if no skeleton is found.</returns>
        ISkeleton TryGetSkeletonById(ulong userId);

        /// <summary>
        /// Frequency of bone sending indexed by bone id.
        /// </summary>
        /// Contains only bones that have a different sending period than <see cref="TargetTrackingFPS"/>.
        IReadOnlyDictionary<uint, float> BonesAsyncFPS { get; }

        /// <summary>
        /// Number of <see cref="UserTrackingFrameDto"/> per second that are sent to the server.
        /// </summary>
        float TargetTrackingFPS { get; set; }

        /// <summary>
        /// If true, camera dto are sent as well.
        /// </summary>
        bool ShouldSendCameraProperties { get; set; }

        /// <summary>
        /// Set the period of a bone update in tracking.
        /// </summary>
        /// <param name="boneType">UMI3D Bone type</param>
        /// <param name="newFPSTarget">New period in seconds</param>
        void SetBoneFPSTarget(uint boneType, float newFPSTarget);

        /// <summary>
        /// Make a bone sent again at the <see cref="BonesAsyncFPS"/>.
        /// </summary>
        /// <param name="boneType"></param>
        void SyncBoneFPS(uint boneType);

        /// <summary>
        /// Update skeletons from tracking frames.
        /// </summary>
        /// <param name="frames"></param>
        void UpdateSkeleton(IEnumerable<UserTrackingFrameDto> frames);

        /// <summary>
        /// Update a collaborative skeleton from a tracking frame.
        /// </summary>
        /// <param name="frame"></param>
        void UpdateSkeleton(UserTrackingFrameDto frame);
    }
}