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
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.animation;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using umi3d.common.utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.UIElements.VisualElement;

namespace PlayMode_Tests.UserCapture.Skeletons.CDK
{
    public abstract class AbstractSkeleton_Test
    {
        protected AbstractSkeleton abstractSkeleton;
        protected GameObject skeletonGo;
        private Mock<IUnityMainThreadDispatcher> unityMainThreadDispatcherMock;

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
            ClearSingletons();

            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public virtual void SetUp()
        {
            skeletonGo = new GameObject("Susbkeleton");
            UnityEngine.Object.Instantiate(skeletonGo);

            unityMainThreadDispatcherMock = new Mock<IUnityMainThreadDispatcher>();
        }

        [TearDown]
        public virtual void TearDown()
        {
            UnityEngine.Object.Destroy(skeletonGo.gameObject);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
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
            var hierarchy = new UMI3DSkeletonHierarchy(null);
            //Given
            GameObject subskeletonGo = new GameObject("Subskeleton");
            UnityEngine.Object.Instantiate(subskeletonGo);
            var mapper = subskeletonGo.AddComponent<SkeletonMapper>();

            Mock<AnimatedSubskeleton> animatedSkeletonMock = new(mapper, new UMI3DAnimatorAnimation[0], 0u, null, null, unityMainThreadDispatcherMock.Object);
            animatedSkeletonMock.Setup(x => x.GetPose(hierarchy)).Returns(new SubSkeletonPoseDto());

            List<AnimatedSubskeleton> animatedSubskeletons = new()
            {
                animatedSkeletonMock.Object,
                animatedSkeletonMock.Object
            };

            foreach (var animatedSubskeleton in animatedSubskeletons)
                abstractSkeleton.AddSubskeleton(animatedSubskeleton);

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
            var hierarchy = new UMI3DSkeletonHierarchy(null);
            //Given
            GameObject subskeletonGo = new GameObject("Subskeleton");
            UnityEngine.Object.Instantiate(subskeletonGo);
            var mapper = subskeletonGo.AddComponent<SkeletonMapper>();

            Mock<AnimatedSubskeleton> animatedSkeletonMock = new(mapper, new UMI3DAnimatorAnimation[0], 0u, null, null, unityMainThreadDispatcherMock.Object);
            var poseDto = new SubSkeletonPoseDto
            {
                bones = new List<SubSkeletonBoneDto>
                {
                    new SubSkeletonBoneDto()
                    {
                        boneType = BoneType.CenterFeet,
                        rotation = Vector4.one.Dto()
                    },
                }
            };

            animatedSkeletonMock.Setup(x => x.GetPose(hierarchy)).Returns(poseDto);

            List<AnimatedSubskeleton> animatedSubskeletons = new()
            {
                animatedSkeletonMock.Object,
                animatedSkeletonMock.Object
            };

            foreach (var animatedSubskeleton in animatedSubskeletons)
                abstractSkeleton.AddSubskeleton(animatedSubskeleton);

            //When
            ISkeleton results = abstractSkeleton.Compute();

            //Then
            Assert.AreSame(abstractSkeleton, results);
        }

        #endregion Compute
    }
}