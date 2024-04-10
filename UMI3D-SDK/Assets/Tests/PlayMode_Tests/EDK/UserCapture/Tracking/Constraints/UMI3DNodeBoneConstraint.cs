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
using TestUtils;
using umi3d.common.userCapture;
using umi3d.common.userCapture.tracking.constraint;
using umi3d.edk;
using umi3d.edk.userCapture.tracking.constraint;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.UserCapture.Tracking.Constraint.EDK
{
    [TestFixture, TestOf(typeof(UMI3DNodeBoneConstraint))]
    public class UMI3DNodeBoneConstraint_Test
    {
        protected UMI3DNodeBoneConstraint boneConstraint;
        private UMI3DNode node;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SceneManager.LoadScene(PlayModeTestHelper.TEST_SCENE_EDK_BASE);
        }

        [OneTimeSetUp]
        public void OneTimeTeardown()
        {
            SceneManager.UnloadSceneAsync(PlayModeTestHelper.TEST_SCENE_EDK_BASE);
        }

        [SetUp]
        public virtual void SetUp()
        {
            GameObject go = new GameObject("NodeBoneBinding");
            boneConstraint = go.AddComponent<UMI3DNodeBoneConstraint>();
            node = go.AddComponent<UMI3DNode>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            UnityEngine.Object.Destroy(boneConstraint);
        }

        #endregion Test SetUp

        #region ToDto

        [Test, TestOf(nameof(UMI3DNodeBoneConstraint.ToDto))]
        public void ToDto()
        {
            // given
            boneConstraint.ShouldBeApplied.SetValue(true);
            boneConstraint.ConstrainedBone.SetValue(BoneType.Chest);
            boneConstraint.PositionOffset.SetValue(Vector3.one);
            boneConstraint.RotationOffset.SetValue(Quaternion.Euler(Vector3.one));
            boneConstraint.ConstrainingNode.SetValue(node);

            // when
            AbstractBoneConstraintDto constraintDto = boneConstraint.ToDto(null);

            // then
            Assert.AreEqual(boneConstraint.Id(), constraintDto.id);
            Assert.AreEqual(boneConstraint.ShouldBeApplied.GetValue(), constraintDto.ShouldBeApplied);
            Assert.AreEqual(boneConstraint.ConstrainedBone.GetValue(), constraintDto.ConstrainedBone);
            AssertUnityStruct.AreEqual(boneConstraint.PositionOffset.GetValue(), constraintDto.PositionOffset.Struct());
            AssertUnityStruct.AreEqual(boneConstraint.RotationOffset.GetValue(), constraintDto.RotationOffset.Quaternion());

            NodeBoneConstraintDto nodeBoneConstraintDto = (NodeBoneConstraintDto)constraintDto;
            Assert.AreEqual(boneConstraint.ConstrainingNode.GetValue().Id(), nodeBoneConstraintDto.ConstrainingNodeId);
        }

        #endregion ToDto
    }
}