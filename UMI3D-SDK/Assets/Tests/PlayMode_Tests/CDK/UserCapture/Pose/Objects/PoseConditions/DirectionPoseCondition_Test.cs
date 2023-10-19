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
using System.Collections.Generic;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace PlayMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(DirectionPoseCondition))]
    public class DirectionPoseCondition_Test
    {
        private GameObject boneGo;
        private GameObject nodeGo;

        #region Test SetUp

        [SetUp]
        public void SetUp()
        {
            boneGo = new GameObject("Bone");
            nodeGo = new GameObject("Node");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(boneGo);
            Object.Destroy(nodeGo);
        }

        #endregion Test SetUp

        #region Check

        [Test]
        public void Check_GoodDirection()
        {
            // GIVEN
            DirectionConditionDto dto = new()
            {
                BoneId = BoneType.Chest,
                Direction = new Vector3(0f, 0f, 1f).Dto(),
                Threshold = 0.1f,
                TargetNodeId = 10005uL
            };

            TrackedSubskeletonBone trackedBone = boneGo.AddComponent<TrackedSubskeletonBone>();
            trackedBone.boneType = BoneType.Chest;
            Dictionary<uint, TrackedSubskeletonBone> bones = new()
            {
                { BoneType.Chest,  trackedBone }
            };
            var trackedSkeletonMock = new Mock<ITrackedSubskeleton>();
            trackedSkeletonMock.Setup(x => x.TrackedBones).Returns(bones);

            DirectionPoseCondition poseCondition = new(dto, nodeGo.transform, trackedSkeletonMock.Object);

            nodeGo.transform.position = new Vector3(1f, 0, 1f);

            boneGo.transform.position = new Vector3(1f, 0, 2f);

            // WHEN
            bool success = poseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_WrongDirection()
        {
            // GIVEN
            DirectionConditionDto dto = new()
            {
                BoneId = BoneType.Chest,
                Direction = new Vector3(0f, 0f, 1f).Dto(),
                Threshold = 0.1f,
                TargetNodeId = 10005uL
            };

            TrackedSubskeletonBone trackedBone = boneGo.AddComponent<TrackedSubskeletonBone>();
            trackedBone.boneType = BoneType.Chest;
            Dictionary<uint, TrackedSubskeletonBone> bones = new()
            {
                { BoneType.Chest,  trackedBone }
            };
            var trackedSkeletonMock = new Mock<ITrackedSubskeleton>();
            trackedSkeletonMock.Setup(x => x.TrackedBones).Returns(bones);

            DirectionPoseCondition poseCondition = new(dto, nodeGo.transform, trackedSkeletonMock.Object);

            nodeGo.transform.position = new Vector3(1f, 0, 1f);

            boneGo.transform.position = new Vector3(1f, 0, 0f);

            // WHEN
            bool success = poseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public void Check_GoodDirectionWithinTolerance()
        {
            // GIVEN
            DirectionConditionDto dto = new()
            {
                BoneId = BoneType.Chest,
                Direction = new Vector3(0f, 0f, 1f).Dto(),
                Threshold = 50f,
                TargetNodeId = 10005uL
            };

            TrackedSubskeletonBone trackedBone = boneGo.AddComponent<TrackedSubskeletonBone>();
            trackedBone.boneType = BoneType.Chest;
            Dictionary<uint, TrackedSubskeletonBone> bones = new()
            {
                { BoneType.Chest,  trackedBone }
            };
            var trackedSkeletonMock = new Mock<ITrackedSubskeleton>();
            trackedSkeletonMock.Setup(x => x.TrackedBones).Returns(bones);

            DirectionPoseCondition poseCondition = new(dto, nodeGo.transform, trackedSkeletonMock.Object);

            nodeGo.transform.position = new Vector3(1f, 0, 1f);

            boneGo.transform.position = new Vector3(2f, 0, 2f);

            // WHEN
            bool success = poseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        #endregion Check
    }
}