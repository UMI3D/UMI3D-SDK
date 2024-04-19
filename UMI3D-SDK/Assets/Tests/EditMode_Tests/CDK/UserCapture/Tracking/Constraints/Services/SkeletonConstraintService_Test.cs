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
using umi3d.cdk.userCapture.tracking.constraint;
using umi3d.common.userCapture;

namespace EditMode_Tests.UserCapture.Tracking.Constraint.CDK
{
    [TestFixture, TestOf(typeof(SkeletonConstraintService))]
    public class SkeletonConstraintService_Test
    {
        private SkeletonConstraintService skeletonConstraintService;
        private Mock<ISkeleton> skeletonMock;

        #region Test Setup

        [SetUp]
        public void Setup()
        {
            ClearSingletons();

            skeletonMock = new();
            skeletonConstraintService = new SkeletonConstraintService(skeletonMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (SkeletonConstraintService.Exists)
                SkeletonConstraintService.Destroy();
        }

        #endregion Test Setup

        #region RegisterConstraint

        [Test, TestOf(nameof(SkeletonConstraintService.RegisterConstraint))]
        public void RegisterConstraint()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(true);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            // when
            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            // then
            Assert.AreEqual(1, skeletonConstraintService.Constraints.Count);
        }

        [Test,]
        public void RegisterConstraint_SameBone()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(true);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            Mock<IBoneConstraint> boneConstraintMock2 = new();

            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(true);
            boneConstraintMock2.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            // when
            skeletonConstraintService.RegisterConstraint(boneConstraintMock2.Object);

            // then
            Assert.AreEqual(2, skeletonConstraintService.Constraints[boneConstraintMock.Object.ConstrainedBone].Count);
        }

        [Test]
        public void RegisterConstraint_Null()
        {
            // given
            IBoneConstraint boneConstraintMock = null;

            // when
            skeletonConstraintService.RegisterConstraint(boneConstraintMock);

            // then
            Assert.AreEqual(0, skeletonConstraintService.Constraints.Count);
        }

        [Test]
        public void RegisterConstraint_AlreadyRegistered()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(true);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            // when
            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            // then
            Assert.AreEqual(1, skeletonConstraintService.Constraints.Count);
        }

        #endregion RegisterConstraint

        #region RegisterConstraint

        [Test, TestOf(nameof(SkeletonConstraintService.UnregisterConstraint))]
        public void UnregisterConstraint()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(true);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);
            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            boneConstraintMock.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));

            // when
            skeletonConstraintService.UnregisterConstraint(boneConstraintMock.Object);

            // then
            Assert.AreEqual(0, skeletonConstraintService.Constraints[boneConstraintMock.Object.ConstrainedBone].Count);
        }

        [Test]
        public void UnregisterConstraint_NotRegistered()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(true);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            // when
            skeletonConstraintService.UnregisterConstraint(boneConstraintMock.Object);

            // then
            Assert.AreEqual(0, skeletonConstraintService.Constraints.Count);
        }

        [Test]
        public void UnregisterConstraint_Null()
        {
            Mock<IBoneConstraint> boneConstraintMockRegistered = new();

            boneConstraintMockRegistered.Setup(x => x.ShouldBeApplied).Returns(true);
            boneConstraintMockRegistered.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMockRegistered.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMockRegistered.Object);

            // given
            IBoneConstraint boneConstraintMock = null;

            // when
            skeletonConstraintService.RegisterConstraint(boneConstraintMock);

            // then
            Assert.AreEqual(1, skeletonConstraintService.Constraints.Count);
        }

        #endregion RegisterConstraint

        #region UpdateConstraints

        [Test, TestOf(nameof(SkeletonConstraintService.UpdateConstraints))]
        public void UpdateConstraints_DifferentBone_BothNotApplied()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            Mock<IBoneConstraint> boneConstraintMock2 = new();

            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.ConstrainedBone).Returns(BoneType.Head);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock2.Object);

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(true);
            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(true);

            // when
            skeletonConstraintService.UpdateConstraints();

            // then
            boneConstraintMock.Verify(x => x.Apply(It.IsAny<ISkeleton>()), Times.Once());
            boneConstraintMock2.Verify(x => x.Apply(It.IsAny<ISkeleton>()), Times.Once());
        }

        [Test]
        public void UpdateConstraints_SameBone_BothNotApplied()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            Mock<IBoneConstraint> boneConstraintMock2 = new();

            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock2.Object);

            // when
            skeletonConstraintService.UpdateConstraints();

            // then
            boneConstraintMock.Verify(x => x.Apply(It.IsAny<ISkeleton>()), Times.Never());
            boneConstraintMock2.Verify(x => x.Apply(It.IsAny<ISkeleton>()), Times.Never());
        }

        [Test]
        public void UpdateConstraints_SameBone_SecondNotApplied()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            Mock<IBoneConstraint> boneConstraintMock2 = new();

            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock2.Object);

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(true);
            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(false);

            // when
            skeletonConstraintService.UpdateConstraints();

            // then
            boneConstraintMock.Verify(x => x.Apply(It.IsAny<ISkeleton>()), Times.Once());
            boneConstraintMock2.Verify(x => x.Apply(It.IsAny<ISkeleton>()), Times.Never());
        }

        [Test]
        public void UpdateConstraints_SameBone_FirstNotAppliedSecondApplied()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            Mock<IBoneConstraint> boneConstraintMock2 = new();

            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock2.Object);

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(true);

            // when
            skeletonConstraintService.UpdateConstraints();

            // then
            boneConstraintMock.Verify(x => x.Apply(It.IsAny<ISkeleton>()), Times.Never());
            boneConstraintMock2.Verify(x => x.Apply(It.IsAny<ISkeleton>()), Times.Once());
        }

        [Test]
        public void UpdateConstraints_SameBone_BothApplied()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            Mock<IBoneConstraint> boneConstraintMock2 = new();

            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock2.Object);

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(true);
            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(true);

            // when
            skeletonConstraintService.UpdateConstraints();

            // then
            boneConstraintMock.Verify(x => x.Apply(It.IsAny<ISkeleton>()), Times.Once());
            boneConstraintMock2.Verify(x => x.Apply(It.IsAny<ISkeleton>()), Times.Never());
        }

        #endregion UpdateConstraints

        #region ForceActivateConstraint

        [Test, TestOf(nameof(SkeletonConstraintService.ForceActivateConstraint))]
        public void ForceActivateConstraint()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            Mock<IBoneConstraint> boneConstraintMock2 = new();

            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock2.Object);

            bool value = false;
            boneConstraintMock.SetupSet(x => x.ShouldBeApplied = It.IsAny<bool>()).Callback<bool>((a) => { value = a; });

            // when
            skeletonConstraintService.ForceActivateConstraint(boneConstraintMock.Object);

            // then
            Assert.IsTrue(value);
        }

        #endregion ForceActivateConstraint

        #region ForceDeactivateConstraint

        [Test, TestOf(nameof(SkeletonConstraintService.ForceDeactivateConstraint))]
        public void ForceDeactivateConstraint()
        {
            // given
            Mock<IBoneConstraint> boneConstraintMock = new();

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock.Object);

            boneConstraintMock.Setup(x => x.ShouldBeApplied).Returns(true);
            boneConstraintMock.Setup(x => x.IsApplied).Returns(true);

            Mock<IBoneConstraint> boneConstraintMock2 = new();

            boneConstraintMock2.Setup(x => x.ShouldBeApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.IsApplied).Returns(false);
            boneConstraintMock2.Setup(x => x.Apply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.EndApply(It.IsAny<ISkeleton>()));
            boneConstraintMock2.Setup(x => x.ConstrainedBone).Returns(BoneType.Chest);

            skeletonConstraintService.RegisterConstraint(boneConstraintMock2.Object);

            // when
            skeletonConstraintService.ForceDeactivateConstraint(boneConstraintMock.Object);

            // then
            boneConstraintMock.Verify(x => x.EndApply(It.IsAny<ISkeleton>()), Times.Once());
        }

        #endregion ForceDeactivateConstraint
    }
}