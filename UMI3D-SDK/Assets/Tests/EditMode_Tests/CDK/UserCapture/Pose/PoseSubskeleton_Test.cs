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
using umi3d.common.userCapture.pose;

namespace EditMode_Tests.UserCapture.Pose.CDK
{
    using SkeletonPose = umi3d.cdk.userCapture.pose.SkeletonPose;

    public class PoseSubskeleton_Test
    {
        private PoseSubskeleton poseSubskeleton = null;

        private Mock<IPoseManager> poseManagerServiceMock;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ClearSingletons();
        }

        [SetUp]
        public void SetUp()
        {
            poseManagerServiceMock = new Mock<IPoseManager>();

            poseSubskeleton = new PoseSubskeleton(poseManagerServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (PoseManager.Exists)
                PoseManager.Destroy();
        }

        #region SetPose

        [Test]
        public void SetPose_Null()
        {
            poseSubskeleton.StartPose(new List<SkeletonPose>(), true);
        }

        [Test]
        public void SetPoses()
        {
            // GIVEN
            var poses = new List<SkeletonPose>()
            {
                new SkeletonPose(new PoseDto(), true),
                new SkeletonPose(new PoseDto()),
            };

            // WHEN
            poseSubskeleton.StartPose(poses, true);
            
            // THEN
            Assert.IsTrue(poseSubskeleton.AppliedPoses.Count == 2);
        }

        #endregion SetPose

        #region StopAllPoses

        [Test]
        public void StopAllPoses_All()
        {
            // GIVEN
            var poses = new List<SkeletonPose>()
            {
                new SkeletonPose(new PoseDto(), true),
                new SkeletonPose(new PoseDto()),
            };

            poseSubskeleton.StartPose(poses, false);

            // WHEN
            poseSubskeleton.StopAllPoses();

            // THEN
            Assert.IsTrue(poseSubskeleton.AppliedPoses.Count == 0);
        }

        #endregion StopAllPoses
    }
}