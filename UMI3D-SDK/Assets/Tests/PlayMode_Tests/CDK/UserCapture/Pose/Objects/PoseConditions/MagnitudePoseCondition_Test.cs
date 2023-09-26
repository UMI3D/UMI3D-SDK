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
    [TestFixture, TestOf(nameof(MagnitudePoseCondition))]
    public class MagnitudePoseCondition_Test
    {
        private GameObject nodeGo;
        private GameObject boneGo;

        #region Test SetUp

        [SetUp]
        public void SetUp()
        {
            nodeGo = new GameObject("Node");
            boneGo = new GameObject("Bone");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(nodeGo);
            Object.Destroy(boneGo);
        }

        #endregion Test SetUp

        #region Check

        [Test]
        public void Check_CloseEnough()
        {
            // GIVEN
            MagnitudeConditionDto dto = new()
            {
                BoneOrigin = BoneType.Chest,
                Magnitude = 1,
                TargetNodeId = 1005ul,
            };

            TrackedSubskeletonBone trackedBone = boneGo.AddComponent<TrackedSubskeletonBone>();
            trackedBone.boneType = BoneType.Chest;
            Dictionary<uint, TrackedSubskeletonBone> bones = new()
            {
                { BoneType.Chest,  trackedBone }
            };
            var trackedSkeletonMock = new Mock<ITrackedSubskeleton>();
            trackedSkeletonMock.Setup(x => x.TrackedBones).Returns(bones);

            MagnitudePoseCondition poseCondition = new(dto, nodeGo.transform, trackedSkeletonMock.Object);

            boneGo.transform.position = Vector3.one * 0.2f;
            nodeGo.transform.position = Vector3.zero;

            // WHEN
            bool success = poseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_NotCloseEnough()
        {
            // GIVEN
            MagnitudeConditionDto dto = new()
            {
                BoneOrigin = BoneType.Chest,
                Magnitude = 1,
                TargetNodeId = 1005ul,
            };

            TrackedSubskeletonBone trackedBone = boneGo.AddComponent<TrackedSubskeletonBone>();
            trackedBone.boneType = BoneType.Chest;
            Dictionary<uint, TrackedSubskeletonBone> bones = new()
            {
                { BoneType.Chest,  trackedBone }
            };
            var trackedSkeletonMock = new Mock<ITrackedSubskeleton>();
            trackedSkeletonMock.Setup(x => x.TrackedBones).Returns(bones);

            MagnitudePoseCondition poseCondition = new(dto, nodeGo.transform, trackedSkeletonMock.Object);

            boneGo.transform.position = Vector3.one * 3f;
            nodeGo.transform.position = Vector3.zero;

            // WHEN
            bool success = poseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        #endregion Check
    }
}