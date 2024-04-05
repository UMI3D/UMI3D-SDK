/*
Copyright 2019 - 2024 Inetum

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

namespace umi3d.cdk.userCapture.tracking.constraint
{
    /// <summary>
    /// Manages tracker simulation for all skeletons.
    /// </summary>
    public interface ISkeletonConstraintService
    {
        /// <summary>
        /// Collection of all constraints registered for each bone of the user's skeleton.
        /// </summary>
        IReadOnlyDictionary<uint, IReadOnlyList<IBoneConstraint>> Constraints { get; }

        /// <summary>
        /// Register a constraint for a skeleton.
        /// </summary>
        /// <param name="constraint"></param>
        void RegisterConstraint(IBoneConstraint constraint);

        /// <summary>
        /// Unregister a constraint from a skeleton.
        /// </summary>
        /// <param name="constraint"></param>
        void UnregisterConstraint(IBoneConstraint constraint);

        /// <summary>
        /// Apply constraints that could be applied and stops constraints that could not for all bones.
        /// </summary>
        void UpdateConstraints();

        /// <summary>
        /// Apply constraints that could be applied and stops constraints that could not for one bone.
        /// </summary>
        /// <param name="boneType">Bone to update constraint for.</param>
        void UpdateConstraints(uint boneType);

        /// <summary>
        /// Force a constraint to be marked as should apply.
        /// </summary>
        /// <param name="constraint"></param>
        /// Use this method to apply constraint directly from the client without the server knowing.
        void ForceActivateConstraint(IBoneConstraint constraint);

        /// <summary>
        /// Force a constraint to be marked as should apply.
        /// </summary>
        /// <param name="constraint"></param>
        /// Use this method to stop a constraint directly from the client without the server knowing.
        void ForceDeactivateConstraint(IBoneConstraint constraint);
    }
}