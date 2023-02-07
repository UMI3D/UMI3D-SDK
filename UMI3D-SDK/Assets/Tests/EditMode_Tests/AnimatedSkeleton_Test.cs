﻿/*
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
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;

namespace EditMode_Tests // TODO: Complete and lowercase the namespace
{
    public class AnimatedSkeleton_Test
    {
        private AnimatedSkeleton animatedSkeleton;

        private Mock<SkeletonMapper> mockSkeletonMapper;
        private Mock<UMI3DEnvironmentLoader> environmentLoaderService;

        [SetUp]
        public void SetUp()
        {
            mockSkeletonMapper = new Mock<SkeletonMapper>();
            if (UMI3DEnvironmentLoader.Exists) //mocking create a new instance actually :/
                UMI3DEnvironmentLoader.Destroy();
            environmentLoaderService = new Mock<UMI3DEnvironmentLoader>();
            animatedSkeleton = new AnimatedSkeleton(mockSkeletonMapper.Object, environmentLoaderService.Object);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void Test_GetCameraDto()
        {
            // GIVEN

            // WHEN
            var cameraDto = animatedSkeleton.GetCameraDto();

            // THEN
            Assert.IsNull(cameraDto);
        }

        [Test]
        public void Test_GetPose_NoAnimation()
        {
            // GIVEN
            var targetPose = new PoseDto();
            mockSkeletonMapper.Setup(x => x.animations).Returns(new ulong[] { });

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
            mockSkeletonMapper.Setup(x => x.animations).Returns(animationsDtos.Keys.ToArray());

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
            mockSkeletonMapper.Setup(x => x.animations).Returns(new ulong[] { animId });

            environmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(animId)).Returns(mockAnimation.Object);

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
            mockSkeletonMapper.Setup(x => x.animations).Returns(animationsDtos.Keys.ToArray());

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
            mockSkeletonMapper.Setup(x => x.animations).Returns(animationsDtos.Keys.ToArray());

            // WHEN
            var pose = animatedSkeleton.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreSame(targetPose, pose);
        }

        [Test]
        public void Test_Update_NoAnimation()
        {
            // GIVEN
            var trackingFrame = new UserTrackingFrameDto()
            {
                animationsPlaying = new ulong[] { }
            };

            Dictionary<ulong, UMI3DAnimatorAnimationDto> animationsDtosInMapper = new()
            {
                {1L, new UMI3DAnimatorAnimationDto() { id = 1L, playing = false } },
                {2L, new UMI3DAnimatorAnimationDto() { id = 2L, playing = true } },
                {3L, new UMI3DAnimatorAnimationDto() { id = 3L, playing = true } },
            };
            mockSkeletonMapper.Setup(x => x.animations).Returns(animationsDtosInMapper.Keys.ToArray());

            Dictionary<ulong, Mock<UMI3DAnimatorAnimation>> mockAnimations = new();
            foreach (var anim in animationsDtosInMapper)
            {
                mockAnimations.Add(anim.Key, new Mock<UMI3DAnimatorAnimation>(anim.Value));
                mockAnimations[anim.Key].Setup(x => x.IsPlaying()).Returns(anim.Value.playing);
                mockAnimations[anim.Key].Setup(x => x.Start()).Callback(() => { anim.Value.playing = true; });
                mockAnimations[anim.Key].Setup(x => x.Stop()).Callback(() => { anim.Value.playing = false; });
                environmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(anim.Key)).Returns(mockAnimations[anim.Key].Object);
            }

            // WHEN
            animatedSkeleton.Update(trackingFrame);

            // THEN
            foreach (var anim in animationsDtosInMapper)
                Assert.IsFalse(anim.Value.playing);
        }

        [Test]
        public void Test_Update_OneAnimationPlaying()
        {
            // GIVEN
            ulong animIdPlaying = 2L;
            var trackingFrame = new UserTrackingFrameDto()
            {
                animationsPlaying = new ulong[1] { animIdPlaying }
            };

            Dictionary<ulong, UMI3DAnimatorAnimationDto> animationsDtosInMapper = new()
            {
                {1L, new UMI3DAnimatorAnimationDto() { id = 1L, playing = false } },
                {2L, new UMI3DAnimatorAnimationDto() { id = 2L, playing = true } },
                {3L, new UMI3DAnimatorAnimationDto() { id = 3L, playing = true } },
            };
            mockSkeletonMapper.Setup(x => x.animations).Returns(animationsDtosInMapper.Keys.ToArray());

            Dictionary<ulong, Mock<UMI3DAnimatorAnimation>> mockAnimations = new();
            foreach (var anim in animationsDtosInMapper)
            {
                mockAnimations.Add(anim.Key, new Mock<UMI3DAnimatorAnimation>(anim.Value));
                mockAnimations[anim.Key].Setup(x => x.IsPlaying()).Returns(anim.Value.playing);
                mockAnimations[anim.Key].Setup(x => x.Start()).Callback(() => { anim.Value.playing = true; });
                mockAnimations[anim.Key].Setup(x => x.Stop()).Callback(() => { anim.Value.playing = false; });
                environmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(anim.Key)).Returns(mockAnimations[anim.Key].Object);
            }

            // WHEN
            animatedSkeleton.Update(trackingFrame);

            // THEN
            foreach (var anim in animationsDtosInMapper)
            {
                if (anim.Key == animIdPlaying)
                    Assert.IsTrue(anim.Value.playing);
                else
                    Assert.IsFalse(anim.Value.playing);
            }
        }

        [Test]
        public void Test_Update_AnimationNotInMapper()
        {
            // GIVEN
            var trackingFrame = new UserTrackingFrameDto()
            {
                animationsPlaying = new ulong[3] { 2L, 3L, 4L }
            };

            Dictionary<ulong, UMI3DAnimatorAnimationDto> animationsDtosInMapper = new()
            {
                {1L, new UMI3DAnimatorAnimationDto() { id = 1L, playing = false } },
                {2L, new UMI3DAnimatorAnimationDto() { id = 2L, playing = true } },
                {3L, new UMI3DAnimatorAnimationDto() { id = 3L, playing = true } },
            };

            mockSkeletonMapper.Setup(x => x.animations).Returns(animationsDtosInMapper.Keys.ToArray());

            Dictionary<ulong, Mock<UMI3DAnimatorAnimation>> mockAnimations = new();
            foreach (var anim in animationsDtosInMapper)
            {
                mockAnimations.Add(anim.Key, new Mock<UMI3DAnimatorAnimation>(anim.Value));
                mockAnimations[anim.Key].Setup(x => x.IsPlaying()).Returns(anim.Value.playing);
                mockAnimations[anim.Key].Setup(x => x.Start()).Callback(() => { anim.Value.playing = true; });
                mockAnimations[anim.Key].Setup(x => x.Stop()).Callback(() => { anim.Value.playing = false; });
                environmentLoaderService.Setup(x => x.GetEntityObject<UMI3DAbstractAnimation>(anim.Key)).Returns(mockAnimations[anim.Key].Object);
            }

            // WHEN
            animatedSkeleton.Update(trackingFrame);

            // THEN
            Assert.IsFalse(animationsDtosInMapper[1].playing);
            Assert.IsTrue(animationsDtosInMapper[2].playing);
            Assert.IsTrue(animationsDtosInMapper[3].playing); //unchanged
        }
    }
}