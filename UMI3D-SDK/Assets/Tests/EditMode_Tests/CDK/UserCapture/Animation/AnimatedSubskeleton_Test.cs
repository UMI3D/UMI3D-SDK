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

using inetum.unityUtils;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.animation;
using umi3d.common;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.utils;
using umi3d.common.userCapture.animation;
using UnityEngine;
using TestUtils.UserCapture;

namespace EditMode_Tests.UserCapture.Animation.CDK
{
    [TestFixture, TestOf(typeof(AnimatedSubskeleton))]
    public class AnimatedSubskeleton_Test
    {
        private Mock<ISkeletonMapper> mockSkeletonMapper;
        private Mock<IEnvironmentManager> mockEnvironmentLoaderService;
        private Mock<ISubskeletonDescriptionInterpolationPlayer> mockSkeletonAnimationPlayer;
        private Mock<ICoroutineService> mockCoroutineService;
        private Mock<IUnityMainThreadDispatcher> mockUnityMainThreadDispatcher;
        private AnimatedSubskeleton animatedSubskeleton;

        [SetUp]
        public void SetUp()
        {
            mockSkeletonMapper = new Mock<ISkeletonMapper>();

            mockEnvironmentLoaderService = new Mock<IEnvironmentManager>();

            mockCoroutineService = new Mock<ICoroutineService>();
            mockUnityMainThreadDispatcher = new Mock<IUnityMainThreadDispatcher>();

            mockSkeletonAnimationPlayer = new Mock<ISubskeletonDescriptionInterpolationPlayer>();

            SkeletonAnimationNodeDto dto = new();

            animatedSubskeleton = new AnimatedSubskeleton(dto, mockSkeletonAnimationPlayer.Object, mockSkeletonMapper.Object, new UMI3DAnimatorAnimation[0], new SkeletonAnimationParameterDto[0],
                                                        mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object);
        }

        #region GetPose

        [Test]
        public void Test_GetPose_NoAnimation()
        {
            // GIVEN
            // Nothing

            // WHEN
            var pose = animatedSubskeleton.GetPose(HierarchyTestHelper.CreateTestHierarchy());

            // THEN
            Assert.IsNull(pose);
        }

        [Test]
        public void Test_GetPose_NoAnimationPlaying()
        {
            var hierarchy = HierarchyTestHelper.CreateTestHierarchy();
            // GIVEN
            var targetPose = new SubSkeletonPoseDto();

            Dictionary<ulong, UMI3DAnimatorAnimationDto> animationsDtos = new()
            {
                {1L, new UMI3DAnimatorAnimationDto() { id = 1L, playing = false } },
                {2L, new UMI3DAnimatorAnimationDto() { id = 2L, playing = false } },
                {3L, new UMI3DAnimatorAnimationDto() { id = 3L, playing = false } },
            };

            Dictionary<ulong, Mock<UMI3DAnimatorAnimation>> mockAnimations = new();

            foreach (var anim in animationsDtos)
            {
                mockAnimations.Add(anim.Key, new Mock<UMI3DAnimatorAnimation>(MockBehavior.Default, 0UL, anim.Value, mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object));
                mockAnimations[anim.Key].Setup(x => x.IsPlaying()).Returns(anim.Value.playing);
                mockEnvironmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(0, anim.Key)).Returns(mockAnimations[anim.Key].Object);
            }

            mockSkeletonMapper.Setup(x => x.GetPose(hierarchy)).Returns(targetPose);

            SkeletonAnimationNodeDto dto = new();
            animatedSubskeleton = new AnimatedSubskeleton(dto, mockSkeletonAnimationPlayer.Object, mockSkeletonMapper.Object, mockAnimations.Values.Select(x => x.Object).ToArray(), new SkeletonAnimationParameterDto[0],
                                                        mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object);

            // WHEN
            var pose = animatedSubskeleton.GetPose(hierarchy);

            // THEN
            Assert.IsNull(pose);
        }

        [Test]
        public void Test_GetPose_OneAnimationPlaying()
        {
            var hierarchy = HierarchyTestHelper.CreateTestHierarchy();
            // GIVEN
            var targetPose = new SubSkeletonPoseDto();
            ulong animId = 1L;
            UMI3DAnimatorAnimationDto animDto = new UMI3DAnimatorAnimationDto()
            {
                id = animId,
                playing = true
            };
            var mockAnimation = new Mock<UMI3DAnimatorAnimation>(MockBehavior.Default, 0UL,animDto, mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object);
            mockAnimation.Setup(x => x.IsPlaying()).Returns(true);

            mockSkeletonMapper.Setup(x => x.GetPose(hierarchy)).Returns(targetPose);
            mockSkeletonAnimationPlayer.Setup(x => x.GetPose(hierarchy)).Returns(targetPose);

            mockEnvironmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(0, animId)).Returns(mockAnimation.Object);

            SkeletonAnimationNodeDto dto = new();
            animatedSubskeleton = new AnimatedSubskeleton(dto, mockSkeletonAnimationPlayer.Object, mockSkeletonMapper.Object, new UMI3DAnimatorAnimation[] { mockAnimation.Object }, new SkeletonAnimationParameterDto[0],
                                                        mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object);

            // WHEN
            var pose = animatedSubskeleton.GetPose(hierarchy);

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreSame(targetPose, pose);
        }

        [Test]
        public void Test_GetPose_OneAnimationPlayingAmongSeveral()
        {
            var hierarchy = HierarchyTestHelper.CreateTestHierarchy();
            // GIVEN
            var targetPose = new SubSkeletonPoseDto();

            Dictionary<ulong, UMI3DAnimatorAnimationDto> animationsDtos = new()
            {
                {1L, new UMI3DAnimatorAnimationDto() { id = 1L, playing = false } },
                {2L, new UMI3DAnimatorAnimationDto() { id = 2L, playing = true } },
                {3L, new UMI3DAnimatorAnimationDto() { id = 3L, playing = false } },
            };

            Dictionary<ulong, Mock<UMI3DAnimatorAnimation>> mockAnimations = new();

            foreach (var anim in animationsDtos)
            {
                mockAnimations.Add(anim.Key, new Mock<UMI3DAnimatorAnimation>(MockBehavior.Default, 0UL, anim.Value, mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object));
                mockAnimations[anim.Key].Setup(x => x.IsPlaying()).Returns(anim.Value.playing);
                mockEnvironmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(0, anim.Key)).Returns(mockAnimations[anim.Key].Object);
            }

            mockSkeletonMapper.Setup(x => x.GetPose(hierarchy)).Returns(targetPose);
            mockSkeletonAnimationPlayer.Setup(x => x.GetPose(hierarchy)).Returns(targetPose);

            SkeletonAnimationNodeDto dto = new();
            animatedSubskeleton = new AnimatedSubskeleton(dto, mockSkeletonAnimationPlayer.Object, mockSkeletonMapper.Object, mockAnimations.Values.Select(x => x.Object).ToArray(), new SkeletonAnimationParameterDto[0],
                                                        mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object);

            // WHEN
            var pose = animatedSubskeleton.GetPose(hierarchy);

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreSame(targetPose, pose);
        }

        [Test]
        public void Test_GetPose_SeveralPlaying()
        {
            var hierarchy = HierarchyTestHelper.CreateTestHierarchy();
            // GIVEN
            var targetPose = new SubSkeletonPoseDto();

            Dictionary<ulong, UMI3DAnimatorAnimationDto> animationsDtos = new()
            {
                {1L, new UMI3DAnimatorAnimationDto() { id = 1L, playing = false } },
                {2L, new UMI3DAnimatorAnimationDto() { id = 2L, playing = true } },
                {3L, new UMI3DAnimatorAnimationDto() { id = 3L, playing = true } },
            };

            Dictionary<ulong, Mock<UMI3DAnimatorAnimation>> mockAnimations = new();

            foreach (var anim in animationsDtos)
            {
                mockAnimations.Add(anim.Key, new Mock<UMI3DAnimatorAnimation>(MockBehavior.Default, 0UL, anim.Value, mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object));
                mockAnimations[anim.Key].Setup(x => x.IsPlaying()).Returns(anim.Value.playing);
                mockEnvironmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(0, anim.Key)).Returns(mockAnimations[anim.Key].Object);
            }

            mockSkeletonMapper.Setup(x => x.GetPose(hierarchy)).Returns(targetPose);
            mockSkeletonAnimationPlayer.Setup(x => x.GetPose(hierarchy)).Returns(targetPose);

            SkeletonAnimationNodeDto dto = new();
            animatedSubskeleton = new AnimatedSubskeleton(dto, mockSkeletonAnimationPlayer.Object, mockSkeletonMapper.Object, mockAnimations.Values.Select(x => x.Object).ToArray(), new SkeletonAnimationParameterDto[0],
                                                        mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object);

            // WHEN
            var pose = animatedSubskeleton.GetPose(hierarchy);

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreSame(targetPose, pose);
        }

        #endregion GetPose

        #region StartParameterSelfUpdate

        [Test]
        public void StartParameterSelfUpdate_Null()
        {
            // GIVEN
            SkeletonAnimationNodeDto dto = new();
            animatedSubskeleton = new AnimatedSubskeleton(dto, mockSkeletonAnimationPlayer.Object, mockSkeletonMapper.Object, new UMI3DAnimatorAnimation[0], new SkeletonAnimationParameterDto[0],
                                                        mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object);

            // WHEN
            Action action = () => animatedSubskeleton.StartParameterSelfUpdate(null);

            // THEN
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void StartParameterSelfUpdate_NoParameter()
        {
            // GIVEN
            var mockSkeleton = new Mock<ISkeleton>();

            
            mockCoroutineService.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false));
            mockUnityMainThreadDispatcher.Setup(x => x.Enqueue(It.IsAny<Action>()));

            SkeletonAnimationNodeDto dto = new();
            animatedSubskeleton = new AnimatedSubskeleton(dto, mockSkeletonAnimationPlayer.Object, mockSkeletonMapper.Object, new UMI3DAnimatorAnimation[0], new SkeletonAnimationParameterDto[0],
                                                        mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object);

            // WHEN
            animatedSubskeleton.StartParameterSelfUpdate(mockSkeleton.Object);

            // THEN
            mockCoroutineService.Verify(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false), Times.Never());
            mockUnityMainThreadDispatcher.Verify(x => x.Enqueue(It.IsAny<Action>()), Times.Never());
        }

        [Test]
        [TestCase(new uint[] { 1u })]
        [TestCase(new uint[] { 1u, 2u, 36u })]
        public void StartParameterSelfUpdate_Parameters(uint[] parametersKey)
        {
            // GIVEN
            var mockSkeleton = new Mock<ISkeleton>();
            var parameters = parametersKey.Select(key => new SkeletonAnimationParameterDto() { parameterKey = key, ranges = new SkeletonAnimationParameterDto.RangeDto[0] }).ToArray();

            mockUnityMainThreadDispatcher.Setup(x => x.Enqueue(It.IsAny<Action>())).Callback<Action>(r => r()); // callback allow the nested code to be run also
            mockCoroutineService.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false));

            SkeletonAnimationNodeDto dto = new();
            animatedSubskeleton = new AnimatedSubskeleton(dto, mockSkeletonAnimationPlayer.Object, mockSkeletonMapper.Object, new UMI3DAnimatorAnimation[0], parameters,
                                                        mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object);

            // WHEN
            animatedSubskeleton.StartParameterSelfUpdate(mockSkeleton.Object);

            // THEN
            mockCoroutineService.Verify(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false), Times.Once());
            mockUnityMainThreadDispatcher.Verify(x => x.Enqueue(It.IsAny<Action>()), Times.Once());
        }

            #endregion StartParameterSelfUpdate

            #region StopParameterSelfUpdate

        [Test]
        public void StopParameterSelfUpdate_NotStarted()
        {
            // GIVEN
            var mockSkeleton = new Mock<ISkeleton>();

            mockCoroutineService.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false));
            mockCoroutineService.Setup(x => x.DetachCoroutine(It.IsAny<Coroutine>()));
;
            mockUnityMainThreadDispatcher.Setup(x => x.Enqueue(It.IsAny<Action>())).Callback<Action>(r=> r());

            SkeletonAnimationNodeDto dto = new();
            animatedSubskeleton = new AnimatedSubskeleton(dto, mockSkeletonAnimationPlayer.Object, mockSkeletonMapper.Object, new UMI3DAnimatorAnimation[0], new SkeletonAnimationParameterDto[0],
                                                        mockCoroutineService.Object, mockUnityMainThreadDispatcher.Object);

            // WHEN
            animatedSubskeleton.StopParameterSelfUpdate();

            // THEN
            mockCoroutineService.Verify(x => x.DetachCoroutine(It.IsAny<Coroutine>()), Times.Never());
            mockUnityMainThreadDispatcher.Verify(x => x.Enqueue(It.IsAny<Action>()), Times.Once());
            }

        #endregion StopParameterSelfUpdate
    }
}