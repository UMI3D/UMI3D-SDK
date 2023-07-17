using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Pose.CDK
{
    public class PoseManager_Test : MonoBehaviour
    {
        PoseManager poseManager = null;
        Mock<IEnvironmentManager> environmentLoaderServiceMock = null;
        Mock<ILoadingManager> loadingManagerMock = null;
        Mock<ISkeletonManager> skeletonManagerServiceMock = null;

        Mock<IUMI3DUserCaptureLoadingParameters> userCaptureLoadingParameters = null;

        [SetUp]
        public void Setup()
        {
            environmentLoaderServiceMock = new Mock<IEnvironmentManager>();
            skeletonManagerServiceMock = new Mock<ISkeletonManager>();
            loadingManagerMock = new();
            userCaptureLoadingParameters = new();
            userCaptureLoadingParameters.Setup(x => x.ClientPoses).Returns(new List<UMI3DPose_so>()
            {
                new UMI3DPose_so(),
                new UMI3DPose_so(),
                new UMI3DPose_so(),
                new UMI3DPose_so(),
                new UMI3DPose_so()
            });
            loadingManagerMock.Setup(x => x.AbstractLoadingParameters).Returns(userCaptureLoadingParameters.Object);
        }

        [TearDown]
        public void TearDown()
        {
            poseManager = null;

            if (PoseManager.Exists)
                PoseManager.Destroy();
        }

        [Test]
        public void TestInitLocalPose()
        {
            // GIVEN
            poseManager = new PoseManager(skeletonManagerServiceMock.Object, loadingManagerMock.Object);

            // WHEN
            poseManager.InitLocalPoses();

            // THEN
            Assert.IsTrue(poseManager.localPoses.Length == userCaptureLoadingParameters.Object.ClientPoses.Count);
        }

        [Test]
        public void TestGetSetPose()
        {
            poseManager = new PoseManager(skeletonManagerServiceMock.Object, loadingManagerMock.Object)
            {
                Poses = PoseDictionary()
            };

            Assert.IsTrue(poseManager.Poses[0][0].Bones.Count == 2);
            Assert.IsTrue(poseManager.Poses[1][0].Bones.Count == 2);
            Assert.IsTrue(poseManager.Poses[1][2].Bones.Count == 4);
        }

        #region Utils
        private Dictionary<ulong, IList<SkeletonPose>> PoseDictionary()
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
