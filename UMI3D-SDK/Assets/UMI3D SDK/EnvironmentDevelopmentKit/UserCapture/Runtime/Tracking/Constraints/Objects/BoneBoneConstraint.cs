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

using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.tracking.constraint;
using umi3d.edk.core;
using UnityEngine;

namespace umi3d.edk.userCapture.tracking.constraint
{
    /// <summary>
    /// Constrain a bone of user's skeleton to another bone transform.
    /// </summary>
    public class BoneBoneConstraint : AbstractLoadableEntity, IBoneConstraint
    {
        public UMI3DAsyncProperty<bool> ShouldBeApplied { get; private set; }

        public UMI3DAsyncProperty<uint> ConstrainedBone { get; private set; }

        public UMI3DAsyncProperty<Vector3> PositionOffset { get; private set; }

        public UMI3DAsyncProperty<Quaternion> RotationOffset { get; private set; }

        public UMI3DAsyncProperty<uint> ConstrainingBone { get; private set; }

        public BoneBoneConstraint() : base()
        {
            ShouldBeApplied = new UMI3DAsyncProperty<bool>(Id(), UMI3DPropertyKeys.TrackingConstraintIsApplied, false);
            ConstrainedBone = new UMI3DAsyncProperty<uint>(Id(), UMI3DPropertyKeys.TrackingConstraintBoneType, BoneType.None);
            PositionOffset = new UMI3DAsyncProperty<Vector3>(Id(), UMI3DPropertyKeys.TrackingConstraintPositionOffset, Vector3.zero);
            RotationOffset = new UMI3DAsyncProperty<Quaternion>(Id(), UMI3DPropertyKeys.TrackingConstraintRotationOffset, Quaternion.identity);
            ConstrainingBone = new UMI3DAsyncProperty<uint>(Id(), UMI3DPropertyKeys.TrackingConstraintConstrainingBone, BoneType.None);
        }

        public AbstractBoneConstraintDto ToDto(UMI3DUser user)
        {
            return new BoneBoneConstraintDto()
            {
                id = Id(),
                ShouldBeApplied = ShouldBeApplied.GetValue(user),
                ConstrainedBone = ConstrainedBone.GetValue(user),
                PositionOffset = PositionOffset.GetValue(user).Dto(),
                RotationOffset = RotationOffset.GetValue(user).Dto(),
                ConstrainingBone = ConstrainingBone.GetValue(user),
            };
        }

        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto(user);
        }
    }
}