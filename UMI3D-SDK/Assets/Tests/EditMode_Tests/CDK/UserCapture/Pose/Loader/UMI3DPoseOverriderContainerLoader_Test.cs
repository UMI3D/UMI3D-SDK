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
    [TestFixture, TestOf(nameof(UMI3DPoseOverriderContainerLoader))]
    public class UMI3DPoseOverriderContainerLoader_Test
    {
        private Mock<IEnvironmentManager> environmentServiceMock;
        private Mock<ISkeletonManager> skeletonServiceMock;
        private Mock<IPoseManager> poseServiceMock;

        private UMI3DPoseOverriderContainerLoader PoseOverriderContainerLoader => poseOverriderContainerLoaderMock.Object;
        private Mock<UMI3DPoseOverriderContainerLoader> poseOverriderContainerLoaderMock;

        #region SetUp

        [SetUp]
        public void Setup()
        {
            environmentServiceMock = new Mock<IEnvironmentManager>();
            skeletonServiceMock = new Mock<ISkeletonManager>();
            poseServiceMock = new Mock<IPoseManager>();

            poseOverriderContainerLoaderMock = new Mock<UMI3DPoseOverriderContainerLoader>(environmentServiceMock.Object,
                                                                                         skeletonServiceMock.Object,
                                                                                         poseServiceMock.Object);
            poseOverriderContainerLoaderMock.CallBase = true;
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
            UMI3DPoseOverridersContainerDto overriderDto = null;

            // When
            TestDelegate action = () => PoseOverriderContainerLoader.Load(overriderDto);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void Load()
        {
            // Given
            UMI3DPoseOverridersContainerDto dto = new()
            {
                id = 1009uL,
                relatedNodeId = 1005uL,
                poseOverriderDtos = new PoseOverriderDto[]
                {
                    new PoseOverriderDto()
                    {
                        poseConditions = new AbstractPoseConditionDto[0],
                        poseIndexInPoseManager = 1
                    }
                }
            };

            poseOverriderContainerLoaderMock.CallBase = false;
            poseOverriderContainerLoaderMock.Setup(x => x.Load(dto)).CallBase();
            var container = new PoseOverridersContainer(dto, new PoseOverrider[0]);
            poseOverriderContainerLoaderMock.Setup(x => x.LoadContainer(dto))
                                            .Returns(container)
                                            .Verifiable();

            environmentServiceMock.Setup(x => x.RegisterEntity(dto.id, dto, It.IsAny<PoseOverridersContainer>(), null))
                                  .Returns(new UMI3DEntityInstance(() => { }))
                                  .Verifiable();

            // When
            PoseOverriderContainerLoader.Load(dto);

            // Then
            poseOverriderContainerLoaderMock.Verify(x => x.LoadContainer(dto));
            environmentServiceMock.Verify(x => x.RegisterEntity(dto.id, dto, It.IsAny<PoseOverridersContainer>(), null), Times.Once());
        }



        #endregion Load

        #region LoadContainer

        [Test]
        public void LoadContainer_Null()
        {
            // Given
            UMI3DPoseOverridersContainerDto overriderDto = null;

            // When
            TestDelegate action = () => PoseOverriderContainerLoader.LoadContainer(overriderDto);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void LoadContainer()
        {
            // Given
            UMI3DPoseOverridersContainerDto dto = new()
            {
                id = 1009uL,
                relatedNodeId = 1005uL,
                poseOverriderDtos = new PoseOverriderDto[]
                {
                    new PoseOverriderDto()
                    {
                        poseConditions = new AbstractPoseConditionDto[0],
                        poseIndexInPoseManager = 1
                    }
                }
            };

            poseOverriderContainerLoaderMock.CallBase = false;
            poseOverriderContainerLoaderMock.Setup(x => x.LoadContainer(dto)).CallBase();
            var poseOverrider = new PoseOverrider(dto.poseOverriderDtos[0], new IPoseCondition[0]);
            poseOverriderContainerLoaderMock.Setup(x => x.LoadPoseOverrider(dto.poseOverriderDtos[0]))
                                            .Returns(poseOverrider)
                                            .Verifiable();

            poseServiceMock.Setup(x => x.AddPoseOverriders(It.IsAny<PoseOverridersContainer>())).Verifiable();

            // When
            PoseOverridersContainer container = PoseOverriderContainerLoader.LoadContainer(dto);

            // Then
            poseOverriderContainerLoaderMock.Verify(x => x.LoadPoseOverrider(dto.poseOverriderDtos[0]));
            poseServiceMock.Verify(x => x.AddPoseOverriders(It.IsAny<PoseOverridersContainer>()), Times.Once());
            Assert.AreEqual(dto.id, container.Id);
            Assert.AreEqual(dto.relatedNodeId, container.NodeId);
            Assert.AreEqual(dto.poseOverriderDtos.Length, container.PoseOverriders.Length);
        }

        #endregion LoadContainer

        #region LoadPoseOverrider

        [Test]
        public void LoadPoseOverrider_Null()
        {
            // Given
            PoseOverriderDto overriderDto = null;

            // When
            TestDelegate action = () => PoseOverriderContainerLoader.LoadPoseOverrider(overriderDto);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void LoadPoseOverrider_NoCondition()
        {
            // Given

            var dto = new PoseOverriderDto()
            {
                poseConditions = new AbstractPoseConditionDto[0],
                poseIndexInPoseManager = 1
            };

            // When
            PoseOverrider overrider = PoseOverriderContainerLoader.LoadPoseOverrider(dto);

            // Then
            Assert.AreEqual(dto.poseIndexInPoseManager, overrider.PoseIndexInPoseManager);
            Assert.AreEqual(dto.duration, overrider.Duration);
            Assert.AreEqual(dto.isComposable, overrider.IsComposable);
            Assert.AreEqual(dto.isInterpolable, overrider.IsInterpolable);
            Assert.AreEqual(dto.activationMode, overrider.ActivationMode);
            Assert.AreEqual(dto.poseConditions.Length, overrider.PoseConditions.Length);
        }

        [Test]
        public void LoadPoseOverrider_Conditions()
        {
            // Given

            var dto = new PoseOverriderDto()
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
                poseIndexInPoseManager = 1
            };
            var nodeInstance = new Mock<UMI3DNodeInstance>(new Action(() => { }));
            var go = new GameObject("magnitudePoseNode");
            nodeInstance.Setup(x => x.transform).Returns(go.transform);
            environmentServiceMock.Setup(x => x.GetNodeInstance(It.IsAny<ulong>())).Returns(nodeInstance.Object);

            var skeletonMock = new Mock<IPersonalSkeleton>();
            var trackedSubskeletonMock = new Mock<ITrackedSubskeleton>();
            skeletonServiceMock.Setup(x => x.PersonalSkeleton).Returns(skeletonMock.Object);
            skeletonMock.Setup(x => x.TrackedSubskeleton).Returns(trackedSubskeletonMock.Object);
            
            // When
            PoseOverrider overrider = PoseOverriderContainerLoader.LoadPoseOverrider(dto);

            // Then
            Assert.AreEqual(dto.poseIndexInPoseManager, overrider.PoseIndexInPoseManager);
            Assert.AreEqual(dto.duration, overrider.Duration);
            Assert.AreEqual(dto.isComposable, overrider.IsComposable);
            Assert.AreEqual(dto.isInterpolable, overrider.IsInterpolable);
            Assert.AreEqual(dto.activationMode, overrider.ActivationMode);
            Assert.AreEqual(dto.poseConditions.Length, overrider.PoseConditions.Length);
        }

        #endregion LoadPoseOverrider

        #region CanReadUMI3DExtension

        [Test]
        public void CanReadUMI3DExtension()
        {
            // Given
            UMI3DPoseOverridersContainerDto dto = new();
            ReadUMI3DExtensionData extensionData = new(dto);

            // When
            bool canReadResult = PoseOverriderContainerLoader.CanReadUMI3DExtension(extensionData);

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
            bool canReadResult = PoseOverriderContainerLoader.CanReadUMI3DExtension(extensionData);

            // THEN
            Assert.IsFalse(canReadResult);
        }

        #endregion CanReadUMI3DExtension

        #region ReadUMI3DExtension

        [Test]
        public async void ReadUMI3DExtension()
        {
            // given
            UMI3DPoseOverridersContainerDto dto = new();
            ReadUMI3DExtensionData extensionData = new(dto);
            poseOverriderContainerLoaderMock.CallBase = false;
            poseOverriderContainerLoaderMock.Setup(x => x.Load(dto)).Verifiable();
            poseOverriderContainerLoaderMock.Setup(x => x.ReadUMI3DExtension(extensionData)).CallBase();
            
            // when
            await PoseOverriderContainerLoader.ReadUMI3DExtension(extensionData);

            // then
            poseOverriderContainerLoaderMock.Verify(x => x.Load(dto));
            
        }

        #endregion ReadUMI3DExtension

        #region SetUMI3DProperty

        [Test]
        public async void SetUMI3DProperty()
        {
            // given
            var poseOverriderDto = new UMI3DPoseOverridersContainerDto();
            UMI3DEntityInstance entityInstance = new(() => { })
            {
                dto = poseOverriderDto
            };
            SetEntityPropertyDto propertyDto = new()
            {
                property = UMI3DPropertyKeys.ActivePoseOverrider,
            };

            SetUMI3DPropertyData setData = new(propertyDto, entityInstance);
            poseOverriderContainerLoaderMock.CallBase = false;
            poseOverriderContainerLoaderMock.Setup(x => x.Load(poseOverriderDto)).Verifiable();
            poseOverriderContainerLoaderMock.Setup(x => x.SetUMI3DProperty(setData)).CallBase();

            // when
            bool success = await PoseOverriderContainerLoader.SetUMI3DProperty(setData);

            // then
            Assert.IsTrue(success);
            poseOverriderContainerLoaderMock.Verify(x => x.Load(poseOverriderDto));
        }

        [Test]
        public async void SetUMI3DProperty_InvalidDto()
        {
            // given
            var poseOverriderDto = new UMI3DDto();
            UMI3DEntityInstance entityInstance = new(() => { })
            {
                dto = poseOverriderDto
            };
            SetEntityPropertyDto propertyDto = new()
            {
                property = 0uL,
            };

            SetUMI3DPropertyData setData = new(propertyDto, entityInstance);
            poseOverriderContainerLoaderMock.CallBase = false;
            poseOverriderContainerLoaderMock.Setup(x => x.SetUMI3DProperty(setData)).CallBase();

            // when
            bool success = await PoseOverriderContainerLoader.SetUMI3DProperty(setData);

            // then
            Assert.IsFalse(success);
        }

        #endregion SetUMI3DProperty
    }
}