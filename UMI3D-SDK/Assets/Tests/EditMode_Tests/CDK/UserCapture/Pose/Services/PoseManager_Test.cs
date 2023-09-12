using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(PoseManager))]
    public class PoseManager_Test
    {
        PoseManager poseManager;
        Mock<IEnvironmentManager> environmentLoaderServiceMock;
        Mock<ILoadingManager> loadingManagerMock;
        Mock<ISkeletonManager> skeletonManagerServiceMock;

        Mock<IUMI3DUserCaptureLoadingParameters> userCaptureLoadingParametersMock;

        [SetUp]
        public void Setup()
        {
            environmentLoaderServiceMock = new Mock<IEnvironmentManager>();
            skeletonManagerServiceMock = new Mock<ISkeletonManager>();
            loadingManagerMock = new();
            userCaptureLoadingParametersMock = new();

            loadingManagerMock.Setup(x => x.AbstractLoadingParameters).Returns(userCaptureLoadingParametersMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            poseManager = null;

            if (PoseManager.Exists)
                PoseManager.Destroy();
        }

        [Test]
        public void InitLocalPoses()
        {
            // GIVEN
            poseManager = new PoseManager(skeletonManagerServiceMock.Object, loadingManagerMock.Object);
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

        [Test]
        public void Poses()
        {
            // Given
            poseManager = new PoseManager(skeletonManagerServiceMock.Object, loadingManagerMock.Object)
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
        #endregion
    }
}
