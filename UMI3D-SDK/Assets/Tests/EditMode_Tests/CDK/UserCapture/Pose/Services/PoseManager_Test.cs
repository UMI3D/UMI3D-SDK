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

        UMI3DUserCaptureLoadingParameters userCaptureLoadingParameters = null;

        [SetUp]
        public void Setup()
        {
            environmentLoaderServiceMock = new Mock<IEnvironmentManager>();
            skeletonManagerServiceMock = new Mock<ISkeletonManager>();
            loadingManagerMock = new();
            userCaptureLoadingParameters = new UMI3DUserCaptureLoadingParameters();

            loadingManagerMock.Setup(x => x.LoadingParameters).Returns(userCaptureLoadingParameters);
            userCaptureLoadingParameters.clientPoses = new List<UMI3DPose_so>()
            {
                new UMI3DPose_so(),
                new UMI3DPose_so(),
                new UMI3DPose_so(),
                new UMI3DPose_so(),
                new UMI3DPose_so()
            };
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
            poseManager = new PoseManager(skeletonManagerServiceMock.Object, environmentLoaderServiceMock.Object, loadingManagerMock.Object);

            Assert.IsTrue(poseManager.localPoses.Length == userCaptureLoadingParameters.clientPoses.Count);
        }

        [Test]
        public void TestGetSetPose()
        {
            poseManager = new PoseManager(skeletonManagerServiceMock.Object, environmentLoaderServiceMock.Object, loadingManagerMock.Object);
            poseManager.SetPoses(PoseDictionary());

            Assert.IsTrue(poseManager.GetPose(0, 0).bones.Count == 2);
            Assert.IsTrue(poseManager.GetPose(1, 0).bones.Count == 2);
            Assert.IsTrue(poseManager.GetPose(1, 2).bones.Count == 4);
        }

        #region Utils
        private Dictionary<ulong, List<PoseDto>> PoseDictionary()
        {
            Dictionary<ulong, List<PoseDto>> allPoses = new Dictionary<ulong, List<PoseDto>>()
            {
                {0, new List<PoseDto>()
                    {
                        new PoseDto()
                        {
                            bones = new List<BoneDto>()
                            {
                                new BoneDto(),
                                new BoneDto()
                            }
                        },
                        new PoseDto()
                    }
                },
                {1, new List<PoseDto>()
                    {
                        new PoseDto()
                        {
                            bones = new List<BoneDto>()
                            {
                                new BoneDto(),
                                new BoneDto()
                            }
                        },
                        new PoseDto(),
                        new PoseDto()
                        {
                            bones = new List<BoneDto>()
                            {
                                new BoneDto(),
                                new BoneDto(),
                                new BoneDto(),
                                new BoneDto()
                            }
                        },
                    }
                },
            };

            return allPoses;
        }
        #endregion
    }
}
