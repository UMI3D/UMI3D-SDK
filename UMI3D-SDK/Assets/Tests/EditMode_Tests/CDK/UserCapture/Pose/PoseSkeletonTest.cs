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
        poseSkeleton.StopAllPoses();

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
        poseSkeleton.StopPose(new List<PoseDto>() { pose_01}, false);

        Assert.IsTrue(poseSkeleton.GetLocalPoses().Count == 1);
        Assert.IsTrue(poseSkeleton.GetServerPoses().Count == 2);
        Assert.IsFalse(poseSkeleton.GetLocalPoses().Contains(pose_01));
        Assert.IsTrue(poseSkeleton.GetLocalPoses()[0] == pose_02);
    }
}
