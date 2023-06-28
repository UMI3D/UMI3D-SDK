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
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.animation;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.UserCapture.Skeletons.CDK
{
    public abstract class AbstractSkeleton_Test
    {
        protected AbstractSkeleton abstractSkeleton;
        protected GameObject skeletonGo;

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            ClearSingletons();
        }

        [SetUp]
        public virtual void SetUp()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);

            skeletonGo = new GameObject("Susbkeleton");
            UnityEngine.Object.Instantiate(skeletonGo);
        }

        [TearDown]
        public virtual void TearDown()
        {
            ClearSingletons();
        }

        private void ClearSingletons()
        {
        }

        #region Compute

        /// <summary>
        /// test that we can handle an empty list of sub skeleton i the compute méthod
        /// </summary>
        [Test]
        public virtual void Compute_EmptyList()
        {
            //Given
            // Empty by default

            //When
            ISkeleton results = abstractSkeleton.Compute();

            //Then
            Assert.AreEqual(0, results.Bones.Count);
            Assert.AreSame(abstractSkeleton, results);
        }

        /// <summary>
        /// Test that we handle a subskeleton that has a list of animated skeletons composed of not initialized animated skeletons
        /// </summary>
        [Test]
        public virtual void Compute_ListWithEmpties()
        {
            //Given
            GameObject subskeletonGo = new GameObject("Subskeleton");
            UnityEngine.Object.Instantiate(subskeletonGo);
            var mapper = subskeletonGo.AddComponent<SkeletonMapper>();

            Mock<AnimatedSkeleton> animatedSkeletonMock = new(mapper, new UMI3DAnimatorAnimation[0], 0u, null);
            animatedSkeletonMock.Setup(x => x.GetPose()).Returns(new PoseDto());

            List<AnimatedSkeleton> animatedSubskeletons = new()
            {
                animatedSkeletonMock.Object,
                animatedSkeletonMock.Object
            };

            abstractSkeleton.Skeletons.AddRange(animatedSubskeletons);

            //When
            ISkeleton results = abstractSkeleton.Compute();

            //Then
            Assert.AreEqual(1, results.Bones.Count);
            Assert.AreSame(abstractSkeleton, results);
        }

        /// <summary>
        /// Test that we can compute correctly With one animated skelton with bones
        /// </summary>
        [Test]
        public virtual void Compute_OneAnimatedSkeletonWithBones()
        {
            //Given
            GameObject subskeletonGo = new GameObject("Subskeleton");
            UnityEngine.Object.Instantiate(subskeletonGo);
            var mapper = subskeletonGo.AddComponent<SkeletonMapper>();

            Mock<AnimatedSkeleton> animatedSkeletonMock = new(mapper, new UMI3DAnimatorAnimation[0], 0u, null);
            PoseDto poseDto = new PoseDto();
            poseDto.SetBonePoseDtoArray(new List<BoneDto>
            {
                new BoneDto()
                {
                    boneType = BoneType.CenterFeet,
                    rotation = Vector4.one.Dto()
                },
            });

            animatedSkeletonMock.Setup(x => x.GetPose()).Returns(poseDto);

            List<AnimatedSkeleton> animatedSubskeletons = new()
            {
                animatedSkeletonMock.Object,
                animatedSkeletonMock.Object
            };

            abstractSkeleton.Skeletons.AddRange(animatedSubskeletons);

            //When
            ISkeleton results = abstractSkeleton.Compute();

            //Then
            Assert.AreSame(abstractSkeleton, results);
        }

        #endregion Compute
    }
}