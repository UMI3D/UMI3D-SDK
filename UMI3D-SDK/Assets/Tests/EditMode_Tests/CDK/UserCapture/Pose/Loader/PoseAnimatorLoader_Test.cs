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
using System.Threading.Tasks;
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
        private Mock<ILoadingManager> loadingServiceMock;
        private Mock<ISkeletonManager> skeletonServiceMock;
        private Mock<IPoseManager> poseServiceMock;

        private PoseAnimatorLoader PoseAnimatorLoader;

        #region SetUp

        [SetUp]
        public void Setup()
        {
            environmentServiceMock = new ();
            loadingServiceMock = new ();
            skeletonServiceMock = new ();
            poseServiceMock = new ();

            PoseAnimatorLoader = new PoseAnimatorLoader(environmentServiceMock.Object, loadingServiceMock.Object, skeletonServiceMock.Object);
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
            TestDelegate action = () => PoseAnimatorLoader.Load(0, overriderDto).Wait();

            // Then
            Assert.Throws<AggregateException>(() => action());
        }

        [Test, TestOf(nameof(PoseAnimatorLoader.Load))]
        public async void Load()
        {
            // Given
            PoseAnimatorDto dto = new()
            {
                id = 1009uL,
                poseClipId = 15593uL,
                relatedNodeId = 1005uL,
                activationMode = (ushort)PoseAnimatorActivationMode.ON_REQUEST,
                poseConditions = new AbstractPoseConditionDto[0],
            };

            PoseClipDto poseClipDto = new PoseClipDto() { id = 15593uL };
            PoseClip poseClip = new(poseClipDto);
            UMI3DEntityInstance instance = new UMI3DEntityInstance(0, () => { })
            {
                Object = poseClip
            };

            loadingServiceMock.Setup(x => x.WaitUntilEntityLoaded(0, poseClip.Id, null)).Returns(Task.FromResult(instance));


            Mock<ICoroutineService> coroutineServiceMock = new();
            Mock<IPoseManager> poseServiceMock = new();
            PoseAnimator container = new(dto, poseClip, new IPoseCondition[0], poseServiceMock.Object, coroutineServiceMock.Object);

            environmentServiceMock.Setup(x => x.RegisterEntity(0, dto.id, dto, It.IsAny<PoseAnimator>(), It.IsAny<Action>()))
                                  .Returns(new UMI3DEntityInstance(0, () => { }))
                                  .Verifiable();

            // When
            await PoseAnimatorLoader.Load(0, dto);

            // Then
            environmentServiceMock.Verify(x => x.RegisterEntity(0, dto.id, dto, It.IsAny<PoseAnimator>(), It.IsAny<Action>()), Times.Once());
        }

        [Test]
        public async void Load_Conditions()
        {
            // Given

            ulong nodeId = 1005uL;
            PoseClipDto poseClipDto = new PoseClipDto() { id = 15593uL };

            PoseAnimatorDto dto = new()
            {
                poseClipId = poseClipDto.id,
                relatedNodeId = nodeId,
                activationMode = (ushort)PoseAnimatorActivationMode.ON_REQUEST,
                poseConditions = new AbstractPoseConditionDto[]
                {
                    new MagnitudeConditionDto()
                    {
                        TargetNodeId = nodeId,
                    },
                    new BoneRotationConditionDto()
                    {
                        Rotation = new Vector4Dto()
                    },
                    new ScaleConditionDto()
                    {
                        TargetId = nodeId,
                        Scale = new Vector3Dto()
                    }
                },
            };


            Mock<UMI3DNodeInstance> nodeInstanceMock = new Mock<UMI3DNodeInstance>(new System.Action(() => { }));

            GameObject go = new ("magnitudePoseNode");
            loadingServiceMock.Setup(x => x.WaitUntilEntityLoaded(0, nodeId, null)).Returns(Task.FromResult(nodeInstanceMock.Object as UMI3DEntityInstance));
            loadingServiceMock.Setup(x => x.WaitUntilEntityLoaded(0, nodeId, null)).Returns(Task.FromResult(nodeInstanceMock.Object as UMI3DEntityInstance));

            nodeInstanceMock.Setup(x => x.transform).Returns(go.transform);
            environmentServiceMock.Setup(x => x.GetNodeInstance(0, It.IsAny<ulong>())).Returns(nodeInstanceMock.Object);

            var skeletonMock = new Mock<IPersonalSkeleton>();
            var trackedSubskeletonMock = new Mock<ITrackedSubskeleton>();
            skeletonServiceMock.Setup(x => x.PersonalSkeleton).Returns(skeletonMock.Object);
            skeletonMock.Setup(x => x.TrackedSubskeleton).Returns(trackedSubskeletonMock.Object);

            environmentServiceMock.Setup(x => x.RegisterEntity(0, dto.id, dto, It.IsAny<PoseAnimator>(), It.IsAny<Action>()))
                                 .Returns(new UMI3DEntityInstance(0, () => { }))
                                 .Verifiable();

            
            PoseClip poseClip = new(poseClipDto);
            UMI3DEntityInstance instance = new UMI3DEntityInstance(0, () => { })
            {
                Object = poseClip
            };

            loadingServiceMock.Setup(x => x.WaitUntilEntityLoaded(0, poseClip.Id, null)).Returns(Task.FromResult(instance));

            // When
            PoseAnimator poseAnimator = await PoseAnimatorLoader.Load(0, dto);

            // Then
            Assert.AreEqual(dto.id, poseAnimator.Id);
            Assert.AreEqual(dto.poseClipId, poseAnimator.PoseClip.Id);
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
            PoseAnimatorDto dto = new()
            {
                activationMode = (ushort)PoseAnimatorActivationMode.ON_REQUEST
            };
            ReadUMI3DExtensionData extensionData = new(0, dto);

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
            ReadUMI3DExtensionData extensionData = new(0, dto);

            // WHEN
            bool canReadResult = PoseAnimatorLoader.CanReadUMI3DExtension(extensionData);

            // THEN
            Assert.IsFalse(canReadResult);
        }

        #endregion CanReadUMI3DExtension
    }
}