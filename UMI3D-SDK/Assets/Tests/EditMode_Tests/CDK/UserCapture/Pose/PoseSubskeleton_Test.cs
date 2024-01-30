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
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.userCapture.tracking;
using UnityEditor;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(PoseSubskeleton))]
    public class PoseSubskeleton_Test
    {
        private PoseSubskeleton poseSubskeleton = null;

        private Mock<IEnvironmentManager> environmentServiceMock;
        private Mock<ISkeleton> parentSkeletonMock;

        #region SetUp

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ClearSingletons();
        }

        [SetUp]
        public void SetUp()
        {
            environmentServiceMock = new();
            parentSkeletonMock = new();
            poseSubskeleton = new PoseSubskeleton(0, parentSkeletonMock.Object, environmentServiceMock.Object);
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
            List<PoseClip> poses = null;

            // when
            TestDelegate action = () => poseSubskeleton.StartPose(poses, true);

            // then
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void StartPose_Empty()
        {
            // given
            List<PoseClip> poses = new();

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
            var pose = new PoseClip(new() { pose = new() });

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
            var poses = new List<PoseClip>()
            {
                new (new () { pose = new() }),
                new (new () { pose = new() }),
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
            PoseClip pose = null;

            // WHEN
            poseSubskeleton.StopPose(pose);

            // THEN
            Assert.AreEqual(0, poseSubskeleton.AppliedPoses.Count);
        }

        [Test]
        public void StopPose_Empty()
        {
            // GIVEN
            List<PoseClip> pose = new();

            // WHEN
            poseSubskeleton.StopPose(pose);

            // THEN
            Assert.AreEqual(0, poseSubskeleton.AppliedPoses.Count);
        }

        [Test]
        public void StopPose_One()
        {
            // GIVEN
            var pose = new PoseClip(new() { pose = new() });

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
            var poses = new List<PoseClip>()
            {
                new (new () { pose = new() }),
                new (new () { pose = new() }),
                new (new () { pose = new() }),
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
            var poses = new List<PoseClip>()
            {
                new (new () { pose = new() }),
                new (new () { pose = new() }),
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

        [Test, TestOf(nameof(PoseSubskeleton.GetPose))]
        public void GetPose()
        {
            // Given
            var poses = new List<PoseClip>()
            {
                new PoseClip(new ()
                {
                    pose = new()
                    {
                                            bones = new()
                    {
                        new() { boneType = BoneType.Chest, rotation = new() },
                        new() { boneType = BoneType.Spine, rotation = new() },
                    }
                    }
                }),
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
            var poses = new List<PoseClip>()
            {
                new PoseClip(new ()
                {
                    pose = new()
                    {
                                            bones = new()
                    {
                        new() { boneType = BoneType.Chest, rotation = new() },
                        new() { boneType = BoneType.Spine, rotation = new() },
                        new() { boneType = 120u, rotation = new() },
                    }
                    }
                }),
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

        [Test, TestOf(nameof(PoseSubskeleton.UpdateBones))]
        public void UpdateBones()
        {
            // Given
            List<PoseClip> poseClips = new ()
            {
                new (new () { id=1005uL, pose = new() { bones=new() { new() { boneType=BoneType.Chest } }, anchor=new()} }),
                new (new () { id=1006uL, pose = new() { bones=new() { new() { boneType=BoneType.Chest } }, anchor=new()} }),
            };

            Mock<ITrackedSubskeleton> trackedSubskeletonMock = new ();
            parentSkeletonMock.Setup(x => x.TrackedSubskeleton).Returns(trackedSubskeletonMock.Object);
            trackedSubskeletonMock.Setup(x => x.StartTrackerSimulation(It.IsAny<PoseAnchorDto>()));
            environmentServiceMock.Setup(x => x.TryGetEntityInstance(0, poseClips[0].Id)).Returns(new UMI3DEntityInstance(0, () => { }, 0) { Object=poseClips[0] });
            environmentServiceMock.Setup(x => x.TryGetEntityInstance(0, poseClips[1].Id)).Returns(new UMI3DEntityInstance(0, () => { }, 0) { Object = poseClips[1] });


            UserTrackingFrameDto frame = new()
            {
                poses = new() { poseClips[0].Id },
                userId = 1005uL
            };



            // When
            poseSubskeleton.UpdateBones(frame);

            // Then
            Assert.AreEqual(frame.poses.Count, poseSubskeleton.AppliedPoses.Count);
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

        [Test, TestOf(nameof(PoseSubskeleton.WriteTrackingFrame))]
        public void WriteTrackingFrame()
        {
            // given
            var poses = new List<PoseClip>()
            {
                new (new () { id=0, pose = new() { bones=new() { new() { boneType=BoneType.Chest } }, anchor=new()} }),
                new (new () { id=1, pose = new() { bones=new() { new() { boneType=BoneType.Chest } }, anchor=new()} }),
            };

            Mock<ITrackedSubskeleton> trackedSubskeletonMock = new();
            parentSkeletonMock.Setup(x => x.TrackedSubskeleton).Returns(trackedSubskeletonMock.Object);
            trackedSubskeletonMock.Setup(x => x.StartTrackerSimulation(It.IsAny<PoseAnchorDto>()));
            poseSubskeleton.StartPose(poses, true);

            UserTrackingFrameDto frame = new();

            // when
            poseSubskeleton.WriteTrackingFrame(frame, null);

            // then
            Assert.AreEqual(poses.Count(), frame.poses.Count);
        }

        #endregion WriteTrackingFrame
    }
}