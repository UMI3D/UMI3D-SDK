/*
Copyright 2019 - 2023 Inetum

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
using Moq;
using NUnit.Framework;
using System;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(umi3d.cdk.userCapture.pose.PoseAnimatorLoader))]
    public class PoseAnimatorLoader_Test
    {
        private Mock<IEnvironmentManager> environmentServiceMock;
        private Mock<ISkeletonManager> skeletonServiceMock;
        private Mock<IPoseManager> poseServiceMock;

        private PoseAnimatorLoader PoseAnimatorLoader;

        #region SetUp

        [SetUp]
        public void Setup()
        {
            environmentServiceMock = new Mock<IEnvironmentManager>();
            skeletonServiceMock = new Mock<ISkeletonManager>();
            poseServiceMock = new Mock<IPoseManager>();

            PoseAnimatorLoader = new PoseAnimatorLoader(environmentServiceMock.Object, skeletonServiceMock.Object, poseServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion SetUp

        #region Load

        [Test]
        public void Load_Null()
        {
            // Given
            PoseAnimatorDto overriderDto = null;

            // When
            TestDelegate action = () => PoseAnimatorLoader.Load(overriderDto);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test, TestOf(nameof(PoseAnimatorLoader.Load))]
        public void Load()
        {
            // Given
            PoseAnimatorDto dto = new()
            {
                id = 1009uL,
                relatedNodeId = 1005uL,

                poseConditions = new AbstractPoseConditionDto[0],
            };

            Mock<ICoroutineService> coroutineServiceMock = new();
            PoseAnimator container = new(dto, new IPoseCondition[0], coroutineServiceMock.Object);

            environmentServiceMock.Setup(x => x.RegisterEntity(dto.id, dto, It.IsAny<PoseAnimator>(), It.IsAny<Action>()))
                                  .Returns(new UMI3DEntityInstance(() => { }))
                                  .Verifiable();

            // When
            PoseAnimatorLoader.Load(dto);

            // Then
            environmentServiceMock.Verify(x => x.RegisterEntity(dto.id, dto, It.IsAny<PoseAnimator>(), It.IsAny<Action>()), Times.Once());
        }

        [Test]
        public void Load_Conditions()
        {
            // Given

            PoseAnimatorDto dto = new()
            {
                poseConditions = new AbstractPoseConditionDto[]
                {
                    new MagnitudeConditionDto(),
                    new BoneRotationConditionDto()
                    {
                        Rotation = new Vector4Dto()
                    },
                    new ScaleConditionDto()
                    {
                        Scale = new Vector3Dto()
                    }
                },
            };

            var nodeInstance = new Mock<UMI3DNodeInstance>(new Action(() => { }));
            environmentServiceMock.Setup(x => x.RegisterEntity(dto.id, dto, It.IsAny<PoseAnimator>(), It.IsAny<Action>()))
                                  .Returns(new UMI3DEntityInstance(() => { }))
                                  .Verifiable();

            var go = new GameObject("magnitudePoseNode");
            nodeInstance.Setup(x => x.transform).Returns(go.transform);
            environmentServiceMock.Setup(x => x.GetNodeInstance(It.IsAny<ulong>())).Returns(nodeInstance.Object);

            var skeletonMock = new Mock<IPersonalSkeleton>();
            var trackedSubskeletonMock = new Mock<ITrackedSubskeleton>();
            skeletonServiceMock.Setup(x => x.PersonalSkeleton).Returns(skeletonMock.Object);
            skeletonMock.Setup(x => x.TrackedSubskeleton).Returns(trackedSubskeletonMock.Object);

            // When
            PoseAnimator poseAnimator = PoseAnimatorLoader.Load(dto);

            // Then
            Assert.AreEqual(dto.id, poseAnimator.Id);
            Assert.AreEqual(dto.poseId, poseAnimator.PoseId);
            Assert.AreEqual(dto.relatedNodeId, poseAnimator.RelativeNodeId);
            Assert.AreEqual(dto.duration, poseAnimator.Duration);
            Assert.AreEqual(dto.isComposable, poseAnimator.IsComposable);
            Assert.AreEqual(dto.isInterpolable, poseAnimator.IsInterpolable);
            Assert.AreEqual(dto.activationMode, poseAnimator.ActivationMode);
            Assert.AreEqual(dto.poseConditions.Length, poseAnimator.PoseConditions.Length);
        }

        #endregion Load

        #region CanReadUMI3DExtension

        [Test, TestOf(nameof(PoseAnimatorLoader.CanReadUMI3DExtension))]
        public void CanReadUMI3DExtension()
        {
            // Given
            PoseAnimatorDto dto = new();
            ReadUMI3DExtensionData extensionData = new(dto);

            // When
            bool canReadResult = PoseAnimatorLoader.CanReadUMI3DExtension(extensionData);

            // Then
            Assert.IsTrue(canReadResult);
        }

        [Test]
        public void CanReadUMI3DExtension_InvalidDto()
        {
            // GIVEN
            UMI3DDto dto = new();
            ReadUMI3DExtensionData extensionData = new(dto);

            // WHEN
            bool canReadResult = PoseAnimatorLoader.CanReadUMI3DExtension(extensionData);

            // THEN
            Assert.IsFalse(canReadResult);
        }

        #endregion CanReadUMI3DExtension

        #region ReadUMI3DExtension

        [Test, TestOf(nameof(PoseAnimatorLoader.ReadUMI3DExtension))]
        public async void ReadUMI3DExtension()
        {
            // given
            PoseAnimatorDto dto = new()
            {
                id = 1009uL,
                relatedNodeId = 1005uL,

                poseConditions = new AbstractPoseConditionDto[0],
            };

            ReadUMI3DExtensionData extensionData = new(dto);

            environmentServiceMock.Setup(x => x.RegisterEntity(dto.id, dto, It.IsAny<PoseAnimator>(), It.IsAny<Action>()))
                                  .Returns(new UMI3DEntityInstance(() => { }))
                                  .Verifiable();

            // when
            await PoseAnimatorLoader.ReadUMI3DExtension(extensionData);

            // then
            environmentServiceMock.Verify(x => x.RegisterEntity(dto.id, dto, It.IsAny<PoseAnimator>(), It.IsAny<Action>()), Times.Once());
        }

        #endregion ReadUMI3DExtension
    }
}