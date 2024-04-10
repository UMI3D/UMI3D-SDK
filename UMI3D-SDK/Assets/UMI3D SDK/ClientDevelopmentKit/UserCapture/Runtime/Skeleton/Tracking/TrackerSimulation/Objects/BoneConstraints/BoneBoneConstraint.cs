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
using umi3d.common.userCapture.tracking.constraint;
using UnityEngine;

namespace umi3d.cdk.userCapture.tracking.constraint
{
    public class BoneBoneConstraint : AbstractBoneConstraint
    {
        public ISkeleton.Transformation ConstrainingBoneTransform { get; private set; }

        public BoneBoneConstraint(BoneBoneConstraintDto dto, ISkeleton.Transformation bone) : base(dto)
        {
            ConstrainingBoneTransform = bone;
        }

        public override string TrackerLabel { get; protected set; } = "Bone Constrained Tracker";

        public override (Vector3 position, Quaternion rotation) Resolve()
        {
            if (ConstrainingBoneTransform == null)
            {
                UMI3DLogger.LogWarning($"Anchoring reference bone destroyed without destroying simulated tracker first. Destroying tracker.", DebugScope.CDK | DebugScope.UserCapture);
                DestroySimulatedTracker();
                return default;
            }
            Vector3 position = ConstrainingBoneTransform.Position + ConstrainingBoneTransform.Rotation * PositionOffset;
            Quaternion rotation = ConstrainingBoneTransform.Rotation * RotationOffset;
            return (position, rotation);
        }
    }
}