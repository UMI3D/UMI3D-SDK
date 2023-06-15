using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.collaboration;
using umi3d.cdk.userCapture;
using umi3d.cdk;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PoseSkeletonTest
{
    private PoseSkeleton poseSkeleton = null;

    private Mock<PoseManager> poseManagerServiceMock;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {

    }

    [SetUp]
    public void SetUp()
    {
        poseManagerServiceMock = new Mock<PoseManager>();

        poseSkeleton = new PoseSkeleton(poseManagerServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        poseSkeleton = null;
    }

    [Test]
    public void TestSetPose_null()
    {
        poseSkeleton.SetPose(true, null, true);
    }

    [Test]
    public void TestSetServerPose()
    {
        poseSkeleton.SetPose(true, new List<PoseDto>() { new PoseDto(), new PoseDto()}, true);
        Assert.IsTrue(poseSkeleton.GetServerPoses().Count == 2);
    }

    [Test]
    public void TestSetClientPose()
    {
        poseSkeleton.SetPose(true, new List<PoseDto>() { new PoseDto(), new PoseDto() }, false);
        Assert.IsTrue(poseSkeleton.GetLocalPoses().Count == 2);
    }


    [Test]
    public void TestStopAllPose()
    {
        PoseDto pose_01 = new PoseDto();
        PoseDto pose_02 = new PoseDto();

        poseSkeleton.SetPose(false, new List<PoseDto>() { pose_01, pose_02 }, false);
        poseSkeleton.SetPose(false, new List<PoseDto>() { pose_01, pose_02 }, true);
        poseSkeleton.StopPose(true);

        Assert.IsTrue(poseSkeleton.GetLocalPoses().Count == 0);
        Assert.IsTrue(poseSkeleton.GetServerPoses().Count == 0);
    }

    [Test]
    public void TestStopASpecificPose()
    {
        PoseDto pose_01 = new PoseDto();
        PoseDto pose_02 = new PoseDto();

        poseSkeleton.SetPose(false, new List<PoseDto>() { pose_01, pose_02 }, false);
        poseSkeleton.SetPose(false, new List<PoseDto>() { pose_01, pose_02 }, true);
        poseSkeleton.StopPose(false, new List<PoseDto>() { pose_01}, false);

        Assert.IsTrue(poseSkeleton.GetLocalPoses().Count == 1);
        Assert.IsTrue(poseSkeleton.GetServerPoses().Count == 2);
        Assert.IsFalse(poseSkeleton.GetLocalPoses().Contains(pose_01));
        Assert.IsTrue(poseSkeleton.GetLocalPoses()[0] == pose_02);
    }
}
