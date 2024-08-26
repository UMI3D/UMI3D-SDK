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

using NUnit.Framework;
using umi3d.cdk;
using umi3d.cdk.userCapture.tracking.constraint;
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.tracking.constraint;
using UnityEngine;

namespace PlayMode_Tests.UserCapture.Tracking.Constraint.CDK
{
    [TestFixture, TestOf(typeof(NodeBoneConstraint))]
    public class NodeBoneConstraint_Test : AbstractBoneConstraint_Test
    {
        private NodeBoneConstraint nodeBoneConstraint;
        private UMI3DNodeInstance node;

        #region Test SetUp

        public override void SetUp()
        {
            GameObject go = new();
            node = new UMI3DNodeInstance(0, () => { }, id: 1538uL)
            {
                GameObject = go
            };

            NodeBoneConstraintDto dto = new()
            {
                id = 1005uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingNodeId = node.Id,
                ShouldBeApplied = true,
                PositionOffset = Vector3Dto.one,
                RotationOffset = Vector4Dto.one
            };
            nodeBoneConstraint = new(dto, node);

            boneConstraint = nodeBoneConstraint;
        }

        public override void TearDown()
        {
            UnityEngine.Object.Destroy(node.GameObject);
        }

        #endregion Test SetUp

        #region Resolve

        [Test, TestOf(nameof(NodeBoneConstraint.Resolve))]
        public void Resolve()
        {
            // given
            (Vector3 position, Quaternion rotation) boneTransform = (new Vector3(125, 35, 16), Quaternion.Euler(94, 35, 16));

            node.GameObject.transform.SetPositionAndRotation(boneTransform.position, boneTransform.rotation);

            // when
            (Vector3 position, Quaternion rotation) result = nodeBoneConstraint.Resolve();

            // then
            TestUtils.AssertUnityStruct.AreEqual(boneTransform.position + boneTransform.rotation * nodeBoneConstraint.PositionOffset, result.position);
            TestUtils.AssertUnityStruct.AreEqual(boneTransform.rotation * nodeBoneConstraint.RotationOffset, result.rotation);
        }

        #endregion Resolve
    }
}