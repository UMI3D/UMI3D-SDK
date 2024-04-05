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

using umi3d.common.userCapture.tracking.constraint;
using UnityEngine;

namespace umi3d.edk.userCapture.tracking.constraint
{
    /// <summary>
    /// Constrain a bone of user's skeleton to a another object's movement.
    /// </summary>
    public interface IBoneConstraint : UMI3DEntity
    {
        /// <summary>
        /// If true, the constraint should be applied on the skeleton.
        /// </summary>
        /// Not necessarily the case, e.g. another constraint is already there.
        UMI3DAsyncProperty<bool> ShouldBeApplied { get; }

        /// <summary>
        /// Bone that should have its movement based on another object.
        /// </summary>
        UMI3DAsyncProperty<uint> ConstrainedBone { get; }

        /// <summary>
        /// Position offset between the bone and the constraining object.
        /// </summary>
        UMI3DAsyncProperty<Vector3> PositionOffset { get; }

        /// <summary>
        /// Rotation offset between the bone and the constraining object.
        /// </summary>
        UMI3DAsyncProperty<Quaternion> RotationOffset { get; }

        /// <summary>
        /// Create a <see cref="AbstractBoneConstraintDto"/> to transmit the constraint.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public AbstractBoneConstraintDto ToDto(UMI3DUser user);
    }
}