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
using System.Linq;
using TestUtils.UserCapture;
using umi3d.cdk.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;
using UnityEngine.TestTools;


namespace PlayMode_Tests.UserCapture.CDK
{
    [TestFixture, TestOf(nameof(SubskeletonDescriptionInterpolationPlayer))]
    public class SubskeletonDescriptionInterpolationPlayer_Test
    {
        protected SubskeletonDescriptionInterpolationPlayer player;

        protected Mock<ISkeleton> mockSkeleton;
        protected Mock<ISubskeletonDescriptor> mockDescriptor;

        #region Test SetUp

        [SetUp]
        public virtual void SetUp()
        {
            mockSkeleton = new Mock<ISkeleton>();

            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();
            List<SubSkeletonBoneDto> bones = new();

            foreach (var (boneType, node) in hierarchy.Relations)
                bones.Add(new() { boneType = boneType, localRotation = Quaternion.identity.Dto(), rotation = Quaternion.identity.Dto() });

            SubSkeletonPoseDto pose = new() { boneAnchor = new PoseAnchorDto(), bones = bones };

            mockDescriptor = new();
            mockDescriptor.Setup(x => x.GetPose(It.IsAny<UMI3DSkeletonHierarchy>())).Returns(pose);

            mockSkeleton.Setup(x => x.Bones).Returns(bones.ToDictionary(x => x.boneType, y => new ISkeleton.Transformation() { LocalRotation = Quaternion.identity, Position = Vector3.zero, Rotation = Quaternion.identity }));

            player = new(mockDescriptor.Object, true, mockSkeleton.Object);
        }

        #endregion Test SetUp

        #region Play

        [Test, TestOf(nameof(SubskeletonDescriptionInterpolationPlayer.Play))]
        public void Play()
        {
            // GIVEN
            // nothing

            // WHEN
            player.Play();

            // THEN
            Assert.IsTrue(player.IsPlaying);
            Assert.IsFalse(player.IsEnding);

        }

        [Test]
        public void Play_NullParameters()
        {
            // GIVEN
            // nothing

            // WHEN
            player.Play(null);

            // THEN
            Assert.IsTrue(player.IsPlaying);
            Assert.IsFalse(player.IsEnding);

        }

        [Test]
        public void Play_WithParameters()
        {
            // GIVEN
            ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = new()
            {
                startTransitionDuration = 0,
                endTransitionDuration = 0.5f
            };


            // WHEN

            // THEN

        }

        #endregion Play

        #region End

        [UnityTest, TestOf(nameof(SubskeletonDescriptionInterpolationPlayer.End))]
        public IEnumerator End()
        {
            // GIVEN
            player.Play();

            // WHEN
            player.End();

            // THEN
            Assert.IsTrue(player.IsPlaying);
            Assert.IsTrue(player.IsEnding);

            yield return new WaitForSeconds(ISubskeletonDescriptionInterpolationPlayer.DEFAULT_TRANSITION_DURATION);

            Assert.IsFalse(player.IsPlaying);
            Assert.IsFalse(player.IsEnding);
        }

        [UnityTest]
        public IEnumerator End_CustomTransitionDuration()
        {
            // GIVEN
            ISubskeletonDescriptionInterpolationPlayer.PlayingParameters parameters = new()
            {
                startTransitionDuration = 0,
                endTransitionDuration = 0.5f
            };

            player.Play(parameters);

            // WHEN
            player.End();

            // THEN
            Assert.IsTrue(player.IsPlaying);
            Assert.IsTrue(player.IsEnding);

            yield return new WaitForSeconds(parameters.endTransitionDuration / 2);

            Assert.IsTrue(player.IsPlaying);
            Assert.IsTrue(player.IsEnding);

            yield return new WaitForSeconds(parameters.endTransitionDuration / 2);

            Assert.IsFalse(player.IsPlaying);
            Assert.IsFalse(player.IsEnding);
        }

        [UnityTest]
        public IEnumerator End_WithoutPlay()
        {
            // GIVEN

            // WHEN
            player.End();

            // THEN
            Assert.IsFalse(player.IsPlaying);
            Assert.IsFalse(player.IsEnding);

            yield return new WaitForSeconds(ISubskeletonDescriptionInterpolationPlayer.DEFAULT_TRANSITION_DURATION);

            Assert.IsFalse(player.IsPlaying);
            Assert.IsFalse(player.IsEnding);
        }

        [Test]
        public void End_Immediately()
        {
            // GIVEN
            player.Play();

            // WHEN
            player.End(true);

            // THEN
            Assert.IsFalse(player.IsPlaying);
            Assert.IsFalse(player.IsEnding);
        }

        #endregion End

        #region GetPose

        [Test, TestOf(nameof(SubskeletonDescriptionInterpolationPlayer.GetPose))]
        public void GetPose()
        {
            // GIVEN
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();
            player.Play();

            // WHEN
            SubSkeletonPoseDto pose = player.GetPose(hierarchy);

            // THEN
            Assert.IsNotNull(pose);

        }

        [Test]
        public void GetPose_NotPlaying()
        {
            // GIVEN
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();

            // WHEN
            SubSkeletonPoseDto pose = player.GetPose(hierarchy);

            // THEN
            Assert.IsNull(pose);
        }

        [Test]
        public void GetPose_Ending()
        {
            // GIVEN
            UMI3DSkeletonHierarchy hierarchy = HierarchyTestHelper.CreateTestHierarchy();
            player.Play();
            player.End();

            // WHEN
            SubSkeletonPoseDto pose = player.GetPose(hierarchy);

            // THEN
            Assert.IsNotNull(pose);

        }

        #endregion GetPose
    }
}