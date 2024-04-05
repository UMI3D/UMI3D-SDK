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
    public class NodeBoneConstraint : AbstractBoneConstraint
    {
        public NodeBoneConstraint(NodeBoneConstraintDto dto, UMI3DNodeInstance node) : base(dto)
        {
            ConstrainingNode = node;
        }

        public UMI3DNodeInstance ConstrainingNode { get; private set; }

        public override string TrackerLabel { get; protected set; } = "Node Constrained Tracker";

        public override (Vector3 position, Quaternion rotation) Resolve()
        {
            if (ConstrainingNode == null)
            {
                UMI3DLogger.LogWarning($"Anchoring reference node destroyed without destroying simulated tracker first. Destroying tracker.", DebugScope.CDK | DebugScope.UserCapture);
                DestroySimulatedTracker();
                return default;
            }
            Quaternion rotation = ConstrainingNode.transform.rotation * RotationOffset;
            Vector3 position = ConstrainingNode.transform.position + ConstrainingNode.transform.rotation * PositionOffset;
            return (position, rotation);
        }
    }
}