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

using Moq;
using NUnit.Framework;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.tracking;
using umi3d.cdk.userCapture.tracking.constraint;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.UserCapture.Tracking.Constraint.CDK
{
    [TestFixture, TestOf(typeof(AbstractBoneConstraint))]
    public abstract class AbstractBoneConstraint_Test
    {
        protected AbstractBoneConstraint boneConstraint;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [OneTimeSetUp]
        public void OneTimeTeardown()
        {
            SceneManager.UnloadSceneAsync(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public virtual void SetUp()
        {
        }

        [TearDown]
        public virtual void TearDown()
        {
        }

        #endregion Test SetUp

        #region Apply

        [Test, TestOf(nameof(AbstractBoneConstraint.Apply))]
        public void Apply()
        {
            // given
            Mock<ISkeleton> skeleton = new();
            Mock<ITrackedSubskeleton> trackedSubskeleton = new();
            trackedSubskeleton.Setup(x => x.ReplaceController(It.IsAny<IController>(), true));
            skeleton.Setup(x => x.TrackedSubskeleton).Returns(trackedSubskeleton.Object);

            // when
            boneConstraint.Apply(skeleton.Object);

            // then
            Assert.IsTrue(boneConstraint.IsApplied);
            Assert.IsNotNull(boneConstraint.Tracker);
        }

        #endregion Apply

        #region EndApply

        [Test, TestOf(nameof(AbstractBoneConstraint.EndApply))]
        public void EndApply()
        {
            // given
            Mock<ISkeleton> skeleton = new();
            Mock<ITrackedSubskeleton> trackedSubskeleton = new();
            trackedSubskeleton.Setup(x => x.ReplaceController(It.IsAny<IController>(), true));
            trackedSubskeleton.Setup(x => x.RemoveController(It.IsAny<uint>()));
            skeleton.Setup(x => x.TrackedSubskeleton).Returns(trackedSubskeleton.Object);
            boneConstraint.Apply(skeleton.Object);

            // when
            boneConstraint.EndApply(skeleton.Object);

            // then
            Assert.IsFalse(boneConstraint.IsApplied);
            Assert.IsNull(boneConstraint.Tracker);
        }

        #endregion EndApply
    }
}