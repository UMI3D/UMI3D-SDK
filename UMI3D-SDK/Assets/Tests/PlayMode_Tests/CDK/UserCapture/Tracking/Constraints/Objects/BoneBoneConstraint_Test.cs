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
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.tracking.constraint;
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.tracking.constraint;
using UnityEngine;

namespace PlayMode_Tests.UserCapture.Tracking.Constraint.CDK
{
    [TestFixture, TestOf(typeof(BoneBoneConstraint))]
    public class BoneBoneConstraint_Test : AbstractBoneConstraint_Test
    {
        private BoneBoneConstraint boneBoneConstraint;
        private ISkeleton.Transformation boneTransformation;

        #region Test SetUp

        [SetUp]
        public override void SetUp()
        {
            boneTransformation = new();
            BoneBoneConstraintDto dto = new()
            {
                id = 1005uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingBone = BoneType.Head,
                ShouldBeApplied = true,
                PositionOffset = Vector3Dto.one,
                RotationOffset = Vector4Dto.one
            };
            boneBoneConstraint = new(dto, boneTransformation);

            boneConstraint = boneBoneConstraint;
        }

        #endregion Test SetUp

        #region Resolve

        [Test, TestOf(nameof(BoneBoneConstraint.Resolve))]
        public void Resolve()
        {
            // given
            (Vector3 position, Quaternion rotation) boneTransform = (new Vector3(125, 35, 16), Quaternion.Euler(94, 35, 16));

            boneTransformation.Position = boneTransform.position;
            boneTransformation.Rotation = boneTransform.rotation;

            // when
            (Vector3 position, Quaternion rotation) result = boneBoneConstraint.Resolve();

            // then
            TestUtils.AssertUnityStruct.AreEqual(boneTransform.position + boneTransform.rotation * boneBoneConstraint.PositionOffset, result.position);
            TestUtils.AssertUnityStruct.AreEqual(boneTransform.rotation * boneBoneConstraint.RotationOffset, result.rotation);
        }

        #endregion Resolve
    }
}