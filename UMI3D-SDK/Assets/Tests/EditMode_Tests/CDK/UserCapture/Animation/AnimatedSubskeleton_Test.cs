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
using UnityEngine;

namespace EditMode_Tests.UserCapture.Animation.CDK
{
    [TestFixture, TestOf(typeof(AnimatedSubskeleton))]
    public class AnimatedSubskeleton_Test
    {
        private Mock<SkeletonMapper> mockSkeletonMapper;
        private Mock<UMI3DEnvironmentLoader> environmentLoaderService;

        [SetUp]
        public void SetUp()
        {
            mockSkeletonMapper = new Mock<SkeletonMapper>();

            environmentLoaderService = new Mock<UMI3DEnvironmentLoader>();
        }

        [TearDown]
        public void TearDown()
        {
            if (UMI3DEnvironmentLoader.Exists)
                UMI3DEnvironmentLoader.Destroy();
        }

        #region GetCameraDto

        [Test]
        public void Test_GetCameraDto()
        {
            // GIVEN
            var animatedSkeleton = new AnimatedSubskeleton(null, null);

            // WHEN
            var cameraDto = animatedSkeleton.GetCameraDto();

            // THEN
            Assert.IsNull(cameraDto);
        }

        #endregion GetCameraDto

        #region GetPose

        [Test]
        public void Test_GetPose_NoAnimation()
        {
            // GIVEN
            var animatedSkeleton = new AnimatedSubskeleton(null, new UMI3DAnimatorAnimation[0]);

            // WHEN
            var pose = animatedSkeleton.GetPose();

            // THEN
            Assert.IsNull(pose);
        }

        [Test]
        public void Test_GetPose_NoAnimationPlaying()
        {
            // GIVEN
            var targetPose = new PoseDto();

            Dictionary<ulong, UMI3DAnimatorAnimationDto> animationsDtos = new()
            {
                {1L, new UMI3DAnimatorAnimationDto() { id = 1L, playing = false } },
                {2L, new UMI3DAnimatorAnimationDto() { id = 2L, playing = false } },
                {3L, new UMI3DAnimatorAnimationDto() { id = 3L, playing = false } },
            };

            Dictionary<ulong, Mock<UMI3DAnimatorAnimation>> mockAnimations = new();

            foreach (var anim in animationsDtos)
            {
                mockAnimations.Add(anim.Key, new Mock<UMI3DAnimatorAnimation>(anim.Value));
                mockAnimations[anim.Key].Setup(x => x.IsPlaying()).Returns(anim.Value.playing);
                environmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(anim.Key)).Returns(mockAnimations[anim.Key].Object);
            }

            mockSkeletonMapper.Setup(x => x.GetPose()).Returns(targetPose);

            var animatedSkeleton = new AnimatedSubskeleton(mockSkeletonMapper.Object, mockAnimations.Values.Select(x => x.Object).ToArray());

            // WHEN
            var pose = animatedSkeleton.GetPose();

            // THEN
            Assert.IsNull(pose);
        }

        [Test]
        public void Test_GetPose_OneAnimationPlaying()
        {
            // GIVEN
            var targetPose = new PoseDto();
            ulong animId = 1L;
            UMI3DAnimatorAnimationDto animDto = new UMI3DAnimatorAnimationDto()
            {
                id = animId,
                playing = true
            };
            var mockAnimation = new Mock<UMI3DAnimatorAnimation>(animDto);
            mockAnimation.Setup(x => x.IsPlaying()).Returns(true);

            mockSkeletonMapper.Setup(x => x.GetPose()).Returns(targetPose);

            environmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(animId)).Returns(mockAnimation.Object);

            var animatedSkeleton = new AnimatedSubskeleton(mockSkeletonMapper.Object, new UMI3DAnimatorAnimation[] { mockAnimation.Object });

            // WHEN
            var pose = animatedSkeleton.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreSame(targetPose, pose);
        }

        [Test]
        public void Test_GetPose_OneAnimationPlayingAmongSeveral()
        {
            // GIVEN
            var targetPose = new PoseDto();

            Dictionary<ulong, UMI3DAnimatorAnimationDto> animationsDtos = new()
            {
                {1L, new UMI3DAnimatorAnimationDto() { id = 1L, playing = false } },
                {2L, new UMI3DAnimatorAnimationDto() { id = 2L, playing = true } },
                {3L, new UMI3DAnimatorAnimationDto() { id = 3L, playing = false } },
            };

            Dictionary<ulong, Mock<UMI3DAnimatorAnimation>> mockAnimations = new();

            foreach (var anim in animationsDtos)
            {
                mockAnimations.Add(anim.Key, new Mock<UMI3DAnimatorAnimation>(anim.Value));
                mockAnimations[anim.Key].Setup(x => x.IsPlaying()).Returns(anim.Value.playing);
                environmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(anim.Key)).Returns(mockAnimations[anim.Key].Object);
            }

            mockSkeletonMapper.Setup(x => x.GetPose()).Returns(targetPose);

            var animatedSkeleton = new AnimatedSubskeleton(mockSkeletonMapper.Object, mockAnimations.Values.Select(x => x.Object).ToArray());

            // WHEN
            var pose = animatedSkeleton.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreSame(targetPose, pose);
        }

        [Test]
        public void Test_GetPose_SeveralPlaying()
        {
            // GIVEN
            var targetPose = new PoseDto();

            Dictionary<ulong, UMI3DAnimatorAnimationDto> animationsDtos = new()
            {
                {1L, new UMI3DAnimatorAnimationDto() { id = 1L, playing = false } },
                {2L, new UMI3DAnimatorAnimationDto() { id = 2L, playing = true } },
                {3L, new UMI3DAnimatorAnimationDto() { id = 3L, playing = true } },
            };

            Dictionary<ulong, Mock<UMI3DAnimatorAnimation>> mockAnimations = new();

            foreach (var anim in animationsDtos)
            {
                mockAnimations.Add(anim.Key, new Mock<UMI3DAnimatorAnimation>(anim.Value));
                mockAnimations[anim.Key].Setup(x => x.IsPlaying()).Returns(anim.Value.playing);
                environmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(anim.Key)).Returns(mockAnimations[anim.Key].Object);
            }

            mockSkeletonMapper.Setup(x => x.GetPose()).Returns(targetPose);

            var animatedSkeleton = new AnimatedSubskeleton(mockSkeletonMapper.Object, mockAnimations.Values.Select(x => x.Object).ToArray());

            // WHEN
            var pose = animatedSkeleton.GetPose();

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
            var animatedSkeleton = new AnimatedSubskeleton(null, new UMI3DAnimatorAnimation[0]);

            // WHEN
            Action action = () => animatedSkeleton.StartParameterSelfUpdate(null);

            // THEN
            Assert.Throws<ArgumentNullException>(() => action());
        }

        [Test]
        public void StartParameterSelfUpdate_NoParameter()
        {
            // GIVEN
            var mockSkeleton = new Mock<ISkeleton>();
            var mockCoroutineService = new Mock<ICoroutineService>();
            mockCoroutineService.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>()));
            var animatedSkeleton = new AnimatedSubskeleton(null, new UMI3DAnimatorAnimation[0], 0, new uint[0], mockCoroutineService.Object);

            // WHEN
            animatedSkeleton.StartParameterSelfUpdate(mockSkeleton.Object);

            // THEN
            mockCoroutineService.Verify(x => x.AttachCoroutine(It.IsAny<IEnumerator>()), Times.Never());
        }

        [Test]
        [TestCase(new uint[] { 1u })]
        [TestCase(new uint[] { 1u, 2u, 36u })]
        public void StartParameterSelfUpdate_Parameters(uint[] parameters)
        {
            // GIVEN
            var mockSkeleton = new Mock<ISkeleton>();
            var mockCoroutineService = new Mock<ICoroutineService>();
            mockCoroutineService.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>()));
            var animatedSkeleton = new AnimatedSubskeleton(null, new UMI3DAnimatorAnimation[0], 0, parameters, mockCoroutineService.Object);

            // WHEN
            animatedSkeleton.StartParameterSelfUpdate(mockSkeleton.Object);

            // THEN
            mockCoroutineService.Verify(x => x.AttachCoroutine(It.IsAny<IEnumerator>()), Times.Once());
        }

        #endregion StartParameterSelfUpdate

        #region StopParameterSelfUpdate

        [Test]
        public void StopParameterSelfUpdate_NotStarted()
        {
            // GIVEN
            var mockSkeleton = new Mock<ISkeleton>();
            var mockCoroutineService = new Mock<ICoroutineService>();
            mockCoroutineService.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>()));
            mockCoroutineService.Setup(x => x.DettachCoroutine(It.IsAny<Coroutine>()));
            var animatedSkeleton = new AnimatedSubskeleton(null, new UMI3DAnimatorAnimation[0], 0, new uint[0], mockCoroutineService.Object);

            // WHEN
            animatedSkeleton.StopParameterSelfUpdate();

            // THEN
            mockCoroutineService.Verify(x => x.DettachCoroutine(It.IsAny<Coroutine>()), Times.Never());
        }

        #endregion StopParameterSelfUpdate
    }
}