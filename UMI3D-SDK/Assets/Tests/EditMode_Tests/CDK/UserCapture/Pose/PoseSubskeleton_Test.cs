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
using System.Collections.Generic;
using System.Linq;
using TestUtils.UserCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(PoseSubskeleton))]
    public class PoseSubskeleton_Test
    {
        private PoseSubskeleton poseSubskeleton = null;

        private Mock<IPoseManager> poseManagerServiceMock;

        #region SetUp

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

        #endregion SetUp

        #region StartPose

        [Test]
        public void StartPose_Null()
        {
            // given
            List<SkeletonPose> poses = null;

            // when
            TestDelegate action = () => poseSubskeleton.StartPose(poses, true);

            // then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void StartPose_Empty()
        {
            // given
            List<SkeletonPose> poses = new();

            // when
            TestDelegate action = () => poseSubskeleton.StartPose(poses, true);

            // then
            Assert.DoesNotThrow(() => action());
            Assert.AreEqual(poses.Count, poseSubskeleton.AppliedPoses.Count);
        }

        [Test]
        public void StartPose_One()
        {
            // GIVEN
            var pose = new SkeletonPose(new PoseDto(), true);

            // WHEN
            poseSubskeleton.StartPose(pose, true);

            // THEN
            Assert.AreEqual(1, poseSubskeleton.AppliedPoses.Count);
            Assert.AreEqual(pose, poseSubskeleton.AppliedPoses[0]);
        }

        [Test]
        public void StartPose_Many()
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
            Assert.AreEqual(poses.Count, poseSubskeleton.AppliedPoses.Count);
        }

        #endregion StartPose

        #region StopPose

        [Test]
        public void StopPose_Null()
        {
            // GIVEN
            SkeletonPose pose = null;

            // WHEN
            poseSubskeleton.StopPose(pose);

            // THEN
            Assert.AreEqual(0, poseSubskeleton.AppliedPoses.Count);
        }

        [Test]
        public void StopPose_Empty()
        {
            // GIVEN
            List<SkeletonPose> pose = new();

            // WHEN
            poseSubskeleton.StopPose(pose);

            // THEN
            Assert.AreEqual(0, poseSubskeleton.AppliedPoses.Count);
        }

        [Test]
        public void StopPose_One()
        {
            // GIVEN
            var pose = new SkeletonPose(new PoseDto(), true);

            poseSubskeleton.StartPose(pose, true);

            // WHEN
            poseSubskeleton.StopPose(pose);

            // THEN
            Assert.AreEqual(0, poseSubskeleton.AppliedPoses.Count);
        }

        [Test]
        public void StopPose_Many()
        {
            // GIVEN
            var poses = new List<SkeletonPose>()
            {
                new SkeletonPose(new PoseDto(), true),
                new SkeletonPose(new PoseDto()),
                new SkeletonPose(new PoseDto()),
            };

            var posesToStop = poses.ToArray()[0..2];

            poseSubskeleton.StartPose(poses, true);

            // WHEN
            poseSubskeleton.StopPose(posesToStop);

            // THEN
            Assert.AreEqual(1, poseSubskeleton.AppliedPoses.Count);
        }

        #endregion StopPose

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
            Assert.AreEqual(0, poseSubskeleton.AppliedPoses.Count);
        }

        #endregion StopAllPoses

        #region GetPose

        [Test]
        public void GetPose_NoPoseSet()
        {
            // Given
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            // When
            var result = poseSubskeleton.GetPose(hierarchy); //hierarchy parameter is not important

            // Then
            Assert.AreEqual(0, result.bones.Count);
        }

        [Test]
        public void GetPose()
        {
            // Given
            var poses = new List<SkeletonPose>()
            {
                new SkeletonPose(new PoseDto()
                {
                    bones = new()
                    {
                        new() { boneType = BoneType.Chest, rotation = new() },
                        new() { boneType = BoneType.Spine, rotation = new() },
                    }
                }, true),
            };

            poseSubskeleton.StartPose(poses, false);

            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            // When
            var result = poseSubskeleton.GetPose(hierarchy);

            // Then
            Assert.AreEqual(2, result.bones.Count);
            var resultBoneTypes = poses[0].Bones.Select(x => x.boneType);
            var expectedBoneTypes = result.bones.Select(x => x.boneType);
            Assert.AreEqual(2, expectedBoneTypes.Intersect(resultBoneTypes).Count());
        }

        [Test]
        public void GetPose_InvalidHierarchy()
        {
            // Given
            var poses = new List<SkeletonPose>()
            {
                new SkeletonPose(new PoseDto()
                {
                    bones = new()
                    {
                        new() { boneType = BoneType.Chest, rotation = new() },
                        new() { boneType = BoneType.Spine, rotation = new() },
                        new() { boneType = 120u, rotation = new() },
                    }
                }, true),
            };

            poseSubskeleton.StartPose(poses, false);

            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            // When
            TestDelegate action = () => poseSubskeleton.GetPose(hierarchy);

            // Then
            Assert.Throws<ArgumentException>(() => action());
        }

        [Test]
        public void GetPose_Null()
        {
            // Given
            UMI3DSkeletonHierarchy hierarchy = null;

            // When
            TestDelegate action = () => poseSubskeleton.GetPose(hierarchy);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        #endregion GetPose

        #region UpdateBones

        [Test]
        public void UpdateBones_Null()
        {
            // Given
            UserTrackingFrameDto frame = null;

            // When
            TestDelegate action = () => poseSubskeleton.UpdateBones(frame);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void UpdateBones()
        {
            // Given
            var poses = new List<SkeletonPose>()
            {
                new SkeletonPose(new PoseDto() { index=0, bones=new() { new() { boneType=BoneType.Chest } }, boneAnchor=new()}, true),
                new SkeletonPose(new PoseDto() { index=1, bones=new() { new() { boneType=BoneType.Chest } }, boneAnchor=new()}, true),
            };

            UserTrackingFrameDto frame = new()
            {
                customPosesIndexes = new() { 1, 2 },
                userId = 1005uL
            };

            poseManagerServiceMock.Setup(x => x.Poses).Returns(new Dictionary<ulong, IList<SkeletonPose>>() { { frame.userId, poses } });

            // When
            poseSubskeleton.UpdateBones(frame);

            // Then
            Assert.AreEqual(1, poseSubskeleton.AppliedPoses.Count);
        }

        #endregion UpdateBones

        #region WriteTrackingFrame

        [Test]
        public void WriteTrackingFrame_Null()
        {
            // Given
            UserTrackingFrameDto frame = null;

            // When
            TestDelegate action = () => poseSubskeleton.WriteTrackingFrame(frame, null);

            // Then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void WriteTrackingFrame()
        {
            // given
            var poses = new List<SkeletonPose>()
            {
                new SkeletonPose(new PoseDto() { index=0, bones=new() { new() { boneType=BoneType.Chest } }, boneAnchor=new()}, true),
                new SkeletonPose(new PoseDto() { index=1, bones=new() { new() { boneType=BoneType.Chest } }, boneAnchor=new()}, false),
            };

            poseSubskeleton.StartPose(poses, true);

            UserTrackingFrameDto frame = new();

            // when
            poseSubskeleton.WriteTrackingFrame(frame, null);

            // then
            Assert.AreEqual(poses.Where(x => x.IsCustom).Count(), frame.customPosesIndexes.Count);
            Assert.AreEqual(poses.Where(x => !x.IsCustom).Count(), frame.environmentPosesIndexes.Count);
        }

        #endregion WriteTrackingFrame
    }
}