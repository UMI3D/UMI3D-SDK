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

using UnityEngine;

namespace umi3d.cdk.userCapture.tracking.constraint
{
    /// <summary>
    /// Restrict or controls a bone movement.
    /// </summary>
    public interface IBoneConstraint
    {
        /// <summary>
        /// Bone constrained that should have its movement restricted.
        /// </summary>
        uint ConstrainedBone { get; }

        /// <summary>
        /// If true, the bone is constrained.
        /// </summary>
        bool IsApplied { get; }

        /// <summary>
        /// If true, the bone should be constrained.
        /// </summary>
        /// Can be false even if <see cref="IsApplied"/> is true, e.g. another constraint is applied.
        bool ShouldBeApplied { get; set; }

        /// <summary>
        /// Offset to apply from the constraining object position.
        /// </summary>
        Vector3 PositionOffset { get; }

        /// <summary>
        /// Offset to apply from the constraining object rotation.
        /// </summary>
        Quaternion RotationOffset { get; }

        /// <summary>
        /// Tracker responsible of the application of the constraint within the tracking system.
        /// </summary>
        ConstrainedSimulatedTracker Tracker { get; }

        /// <summary>
        /// Name of the tracker in the editor.
        /// </summary>
        string TrackerLabel { get; }

        /// <summary>
        /// Start to constraint the bone.
        /// </summary>
        /// <param name="skeleton"></param>
        void Apply(ISkeleton skeleton);

        /// <summary>
        /// Stop to constraint the bone.
        /// </summary>
        /// <param name="skeleton"></param>
        void EndApply(ISkeleton skeleton);

        /// <summary>
        /// Raised when constraint start to be applied.
        /// </summary>
        public event System.Action Applied;

        /// <summary>
        /// Raised when constraint stopped to be applied.
        /// </summary>
        public event System.Action Stopped;

        /// <summary>
        /// Produce the position and rotation of the bone based on the constraint.
        /// </summary>
        /// <returns>Position and rotation corrected by the constraint.</returns>
        (Vector3 position, Quaternion rotation) Resolve();
    }
}