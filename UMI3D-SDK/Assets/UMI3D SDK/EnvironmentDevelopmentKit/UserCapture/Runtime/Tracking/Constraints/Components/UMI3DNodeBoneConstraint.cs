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

using inetum.unityUtils;
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.tracking.constraint;
using umi3d.edk.core;
using UnityEngine;

namespace umi3d.edk.userCapture.tracking.constraint
{
    /// <summary>
    /// Constrain a bone of user's skeleton to a node transform.
    /// </summary>
    public class UMI3DNodeBoneConstraint : AbstractLoadableComponentEntity, UMI3DLoadableEntity, IBoneConstraint
    {
        [SerializeField, Tooltip("If true, the constraint should be applied on the skeleton. Default value at start.")]
        private bool _isApplied = false;

        public UMI3DAsyncProperty<bool> ShouldBeApplied { get; private set; }

        [SerializeField, ConstEnum(typeof(BoneType), typeof(uint)), Tooltip("Bone that should have its movement based on another object. Default value at start.")]
        private uint _constrainedBone;

        public UMI3DAsyncProperty<uint> ConstrainedBone { get; private set; }

        [SerializeField, Tooltip("Position offset between the bone and the constraining object. Default value at start.")]
        private Vector3 _positionOffset = Vector3.zero;

        public UMI3DAsyncProperty<Vector3> PositionOffset { get; private set; }

        [SerializeField, Tooltip("Rotation offset between the bone and the constraining object. Default value at start.")]
        private Quaternion _rotationOffset = Quaternion.identity;

        public UMI3DAsyncProperty<Quaternion> RotationOffset { get; private set; }

        [SerializeField, Tooltip("Node constraining the skeleotn bone. Default value at start.")]
        private UMI3DNode _constrainingNode;

        public UMI3DAsyncProperty<UMI3DNode> ConstrainingNode { get; private set; }

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            _constrainingNode = _constrainingNode == null ? GetComponentInParent<UMI3DNode>() : _constrainingNode;
            ShouldBeApplied ??= new UMI3DAsyncProperty<bool>(Id(), UMI3DPropertyKeys.TrackingConstraintIsApplied, _isApplied);
            ConstrainedBone ??= new UMI3DAsyncProperty<uint>(Id(), UMI3DPropertyKeys.TrackingConstraintBoneType, _constrainedBone);
            PositionOffset ??= new UMI3DAsyncProperty<Vector3>(Id(), UMI3DPropertyKeys.TrackingConstraintPositionOffset, _positionOffset);
            RotationOffset ??= new UMI3DAsyncProperty<Quaternion>(Id(), UMI3DPropertyKeys.TrackingConstraintRotationOffset, _rotationOffset);
            ConstrainingNode ??= new UMI3DAsyncProperty<UMI3DNode>(Id(), UMI3DPropertyKeys.TrackingConstraintConstrainingNode, _constrainingNode);
        }

        public override IEntity ToEntityDto(UMI3DUser user)
        {
            return ToDto(user);
        }

        public AbstractBoneConstraintDto ToDto(UMI3DUser user)
        {
            return new NodeBoneConstraintDto()
            {
                id = Id(),
                ShouldBeApplied = ShouldBeApplied.GetValue(user),
                ConstrainedBone = ConstrainedBone.GetValue(user),
                PositionOffset = PositionOffset.GetValue(user).Dto(),
                RotationOffset = RotationOffset.GetValue(user).Dto(),
                ConstrainingNodeId = ConstrainingNode.GetValue(user).Id(),
            };
        }

        private void OnDrawGizmosSelected()
        {
            float scale = 0.3f;

            Quaternion anchorRotation = transform.rotation * _rotationOffset;
            Vector3 anchorPosition = transform.position + anchorRotation * _positionOffset;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(anchorPosition, anchorPosition + scale * (anchorRotation * Vector3.right));
            Gizmos.color = Color.green;
            Gizmos.DrawLine(anchorPosition, anchorPosition + scale * (anchorRotation * Vector3.up));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(anchorPosition, anchorPosition + scale * (anchorRotation * Vector3.forward));
        }
    }
}