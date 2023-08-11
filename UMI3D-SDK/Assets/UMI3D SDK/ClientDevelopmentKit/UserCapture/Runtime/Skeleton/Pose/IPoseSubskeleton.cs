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

namespace umi3d.cdk.userCapture.pose
{
    /// <summary>
    /// Subskeleton that receive poses on its bones.
    /// </summary>
    public interface IPoseSubskeleton : IWritableSubskeleton
    {
        /// <summary>
        /// List of poses that are currently applied on the subskeleton.
        /// </summary>
        IReadOnlyList<SkeletonPose> AppliedPoses { get; }

        /// <summary>
        /// Set poses for the calculation of the next tracking frame
        /// </summary>
        /// <param name="posesToAdd">Poses to start to apply</param>
        /// <param name="isOverriding">If true, all previous poses will be stopped.</param>
        void StartPose(IEnumerable<SkeletonPose> posesToAdd, bool isOverriding = false);

        /// <summary>
        /// Set a pose for the calculation of the next tracking frame
        /// </summary>
        /// <param name="posesToAdd">Poses to start to apply</param>
        /// <param name="isOverriding">If true, all previous poses will be stopped.</param>
        void StartPose(SkeletonPose poseToAdd, bool isOverriding = false);

        /// <summary>
        /// Remove all poses from computation.
        /// </summary>
        void StopAllPoses();

        /// <summary>
        /// Remove poses from computation.
        /// </summary>
        /// <param name="posesToStopIds">Ids of poses to stop</param>
        void StopPose(IEnumerable<int> posesToStopIds);

        /// <summary>
        /// Remove poses from computation.
        /// </summary>
        /// <param name="posesToStopIds">Poses to stop</param>
        void StopPose(IEnumerable<SkeletonPose> posesToStop);

        /// <summary>
        /// Remove a pose from computation.
        /// </summary>
        /// <param name="posesToStopIds">Id of the pose to stop</param>
        void StopPose(int poseToStopId);

        /// <summary>
        /// Remove a pose from computation.
        /// </summary>
        /// <param name="posesToStopIds">Pose to stop</param>
        void StopPose(SkeletonPose poseToStop);
    }
}