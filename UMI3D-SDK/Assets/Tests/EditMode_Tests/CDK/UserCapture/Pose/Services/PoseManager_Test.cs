using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.common;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;

namespace EditMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(PoseManager))]
    public class PoseManager_Test
    {
        private PoseManager poseManager;
        private Mock<IEnvironmentManager> environmentLoaderServiceMock;
        private Mock<ILoadingManager> loadingManagerMock;
        private Mock<ISkeletonManager> skeletonManagerServiceMock;
        private Mock<IUMI3DClientServer> clientServerServiceMock;

        private Mock<IUMI3DUserCaptureLoadingParameters> userCaptureLoadingParametersMock;

        #region SetUp

        [SetUp]
        public void Setup()
        {
            environmentLoaderServiceMock = new Mock<IEnvironmentManager>();
            skeletonManagerServiceMock = new Mock<ISkeletonManager>();
            loadingManagerMock = new();
            userCaptureLoadingParametersMock = new();
            clientServerServiceMock = new();
            clientServerServiceMock.Setup(x => x.OnLeaving).Returns(new UnityEngine.Events.UnityEvent());
            clientServerServiceMock.Setup(x => x.OnRedirection).Returns(new UnityEngine.Events.UnityEvent());

            loadingManagerMock.Setup(x => x.AbstractLoadingParameters).Returns(userCaptureLoadingParametersMock.Object);
            poseManager = new PoseManager(skeletonManagerServiceMock.Object, loadingManagerMock.Object, clientServerServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            poseManager = null;

            if (PoseManager.Exists)
                PoseManager.Destroy();
        }

        #endregion SetUp

        #region InitLocalPoses

        [Test]
        public void InitLocalPoses()
        {
            // GIVEN
            var poseDataMock = new Mock<IUMI3DPoseData>();
            poseDataMock.Setup(x => x.ToDto()).Returns(new PoseDto());
            userCaptureLoadingParametersMock.Setup(x => x.ClientPoses).Returns(new List<IUMI3DPoseData>()
            {
                poseDataMock.Object,
                poseDataMock.Object,
                poseDataMock.Object,
                poseDataMock.Object,
                poseDataMock.Object,
                poseDataMock.Object,
            });

            // WHEN
            poseManager.InitLocalPoses();

            // THEN
            Assert.AreEqual(userCaptureLoadingParametersMock.Object.ClientPoses.Count, poseManager.localPoses.Length);
        }

        #endregion InitLocalPoses

        #region Poses

        [Test]
        public void Poses()
        {
            // Given
            if (PoseManager.Exists)
                PoseManager.Destroy();

            poseManager = new PoseManager(skeletonManagerServiceMock.Object, loadingManagerMock.Object, clientServerServiceMock.Object)
            {
                Poses = GeneratePoseDictionary()
            };

            // When
            var poses = poseManager.Poses;

            // Then
            Assert.AreEqual(2, poses[0][0].Bones.Count);
            Assert.AreEqual(2, poses[1][0].Bones.Count);
            Assert.AreEqual(4, poses[1][2].Bones.Count);
        }

        #endregion Poses

        #region AddPoseOverriders

        [Test]
        public void AddPoseOverriders_Null()
        {
            // Given
            PoseOverridersContainer overrider = null;

            // When
            TestDelegate action = () => poseManager.AddPoseOverriders(overrider);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void AddPoseOverriders()
        {
            // Given
            UMI3DPoseOverridersContainerDto dto = new();
            PoseOverridersContainer overrider = new(dto, new PoseOverrider[0]);

            // When
            TestDelegate action = () => poseManager.AddPoseOverriders(overrider);

            // Then
            Assert.DoesNotThrow(() => action());
        }

        #endregion AddPoseOverriders

        #region RemovePoseOverriders

        [Test]
        public void RemovePoseOverriders_Null()
        {
            // Given
            PoseOverridersContainer overrider = null;

            // When
            TestDelegate action = () => poseManager.RemovePoseOverriders(overrider);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void RemovePoseOverriders()
        {
            // Given
            UMI3DPoseOverridersContainerDto dto = new();
            PoseOverridersContainer overrider = new(dto, new PoseOverrider[0]);

            // When
            TestDelegate action = () => poseManager.RemovePoseOverriders(overrider);

            // Then
            Assert.DoesNotThrow(() => action());
        }

        #endregion RemovePoseOverriders

        #region TryActivatePoseOverriders

        [Test]
        public void TryActivatePoseOverriders()
        {
            // Given
            UMI3DPoseOverridersContainerDto dto = new()
            {
                poseOverriderDtos = new PoseOverriderDto[]
                {
                    new()
                    {
                        poseIndexInPoseManager = 1,
                        activationMode = (ushort)PoseActivationMode.NONE
                    },
                },
                relatedNodeId = 1006uL
            };
            PoseOverridersContainer overrider = new(dto, new PoseOverrider[0]);

            poseManager.AddPoseOverriders(overrider);
            poseManager.Poses.Add(dto.relatedNodeId, new List<SkeletonPose>() { new SkeletonPose(new PoseDto() { index = 1 }) });

            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            var poseSubskeletonMock = new Mock<IPoseSubskeleton>();
            skeletonManagerServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);
            personalSkeletonMock.Setup(x => x.PoseSubskeleton).Returns(poseSubskeletonMock.Object);
            poseSubskeletonMock.Setup(x => x.StartPose(It.IsAny<SkeletonPose>(), false));
            // When
            TestDelegate action = () => poseManager.TryActivatePoseOverriders(dto.relatedNodeId, PoseActivationMode.NONE);

            // Then
            Assert.DoesNotThrow(() => action());
        }

        #endregion TryActivatePoseOverriders

        #region ApplyPoseOverride

        [Test]
        public void ApplyPoseOverride_Null()
        {
            // Given
            PoseOverrider overrider = null;

            // When
            TestDelegate action = () => poseManager.ApplyPoseOverride(overrider);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void ApplyPoseOverride()
        {
            // Given
            var dto = new PoseOverriderDto()
            {
                poseIndexInPoseManager = 1,
                activationMode = (ushort)PoseActivationMode.NONE
            };
            PoseOverrider overrider = new PoseOverrider(dto, new IPoseCondition[0]);

            poseManager.Poses[UMI3DGlobalID.EnvironementId].Add(new SkeletonPose(new PoseDto() { index = 1 }));

            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            var poseSubskeletonMock = new Mock<IPoseSubskeleton>();
            skeletonManagerServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);
            personalSkeletonMock.Setup(x => x.PoseSubskeleton).Returns(poseSubskeletonMock.Object);
            poseSubskeletonMock.Setup(x => x.StartPose(It.IsAny<SkeletonPose>(), false));

            // When
            poseManager.ApplyPoseOverride(overrider);

            // Then
            poseSubskeletonMock.Verify(x => x.StartPose(It.IsAny<SkeletonPose>(), false), Times.Once());
        }

        #endregion ApplyPoseOverride

        #region StopPoseOverride

        [Test]
        public void StopPoseOverride_Null()
        {
            // Given
            PoseOverrider overrider = null;

            // When
            TestDelegate action = () => poseManager.StopPoseOverride(overrider);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void StopPoseOverride()
        {
            // Given
            var dto = new PoseOverriderDto()
            {
                poseIndexInPoseManager = 1,
                activationMode = (ushort)PoseActivationMode.NONE
            };
            PoseOverrider overrider = new PoseOverrider(dto, new IPoseCondition[0]);

            poseManager.Poses[UMI3DGlobalID.EnvironementId].Add(new SkeletonPose(new PoseDto() { index = 1 }));

            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            var poseSubskeletonMock = new Mock<IPoseSubskeleton>();
            skeletonManagerServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);
            personalSkeletonMock.Setup(x => x.PoseSubskeleton).Returns(poseSubskeletonMock.Object);
            poseSubskeletonMock.Setup(x => x.StopPose(It.IsAny<SkeletonPose>()));

            // When
            poseManager.StopPoseOverride(overrider);

            // Then
            poseSubskeletonMock.Verify(x => x.StopPose(It.IsAny<SkeletonPose>()), Times.Once());
        }

        #endregion StopPoseOverride

        #region Utils

        private Dictionary<ulong, IList<SkeletonPose>> GeneratePoseDictionary()
        {
            Dictionary<ulong, IList<SkeletonPose>> allPoses = new Dictionary<ulong, IList<SkeletonPose>>()
            {
                {0,
                    new List<SkeletonPose>()
                    {
                        new SkeletonPose(
                            new PoseDto()
                            {
                                bones = new List<BoneDto>()
                                {
                                    new BoneDto(),
                                    new BoneDto()
                                }
                            }),
                        new SkeletonPose(
                            new PoseDto()
                            {
                                bones = new List<BoneDto>()
                                {
                                    new BoneDto(),
                                    new BoneDto()
                                }
                            }),
                    }
                },
                {1,
                    new List<SkeletonPose>()
                    {
                        new SkeletonPose(
                            new PoseDto()
                            {
                                bones = new List<BoneDto>()
                                {
                                    new BoneDto(),
                                    new BoneDto()
                                }
                            }),
                        new SkeletonPose(
                            new PoseDto()
                            {
                                bones = new List<BoneDto>()
                                {
                                    new BoneDto(),
                                    new BoneDto()
                                }
                            }),
                        new SkeletonPose(
                            new PoseDto()
                            {
                                bones = new List<BoneDto>()
                                {
                                    new BoneDto(),
                                    new BoneDto(),
                                    new BoneDto(),
                                    new BoneDto()
                                }
                            }),
                    }
                },
            };

            return allPoses;
        }

        #endregion Utils
    }
}