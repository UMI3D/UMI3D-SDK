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
using umi3d.cdk.userCapture;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PlayMode_Tests.UserCapture.CDK
{
    public class PersonalSkeleton_Test
    {
        private List<AnimatedSkeleton> animatedSkeletons = new();
        private PersonalSkeleton personalSkeleton;

        private GameObject root;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public void SetUp()
        {
            root = new GameObject();
            animatedSkeletons = new();
            personalSkeleton = root.AddComponent<PersonalSkeleton>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.Destroy(root);
        }

        #region Compute

        /// <summary>
        /// test that we can handle an empty list of sub skeleton i the compute méthod
        /// </summary>
        [Test]
        public void Compute_EmptyList()
        {
            //Given
            // Empty by default

            //When
            ISkeleton results = personalSkeleton.Compute();

            //Then
            Assert.AreEqual(0, results.Bones.Count);
            Assert.AreSame(personalSkeleton, results);
        }

        /// <summary>
        /// Test that we handle a subskeleton that has a list of animated skeletons composed of not initialized animated skeletons
        /// </summary>
        [Test]
        public void Compute_ListWithEmpties()
        {
            //Given
            var a1 = new GameObject("SkeletonMapper_1");
            var a2 = new GameObject("SkeletonMapper_2");

            animatedSkeletons.Add(new AnimatedSkeleton(a1.AddComponent<SkeletonMapper>()));
            animatedSkeletons.Add(new AnimatedSkeleton(a2.AddComponent<SkeletonMapper>()));

            personalSkeleton.Skeletons.AddRange(animatedSkeletons);

            //When
            ISkeleton results = personalSkeleton.Compute();

            //Then
            Assert.AreEqual(1, results.Bones.Count);
            Assert.AreSame(personalSkeleton, results);
        }

        /// <summary>
        /// Test that we can compute correctly With one animated skelton with bones
        /// </summary>
        [Test]
        public void Compute_OneAnimatedSkeletonWithBones()
        {
            Mock<AnimatedSkeleton> mock = new Mock<AnimatedSkeleton>();
            PoseDto poseDto = new PoseDto();
            poseDto.SetBonePoseDtoArray(new List<BoneDto>
            {
            new BoneDto() {boneType = BoneType.CenterFeet, rotation = Vector4.one},
            });

            Mock<SkeletonMapper> mockSkeletonMapper = new Mock<SkeletonMapper>();
            animatedSkeletons.Add(new AnimatedSkeleton(mockSkeletonMapper.Object));
            animatedSkeletons.Add(new AnimatedSkeleton(mockSkeletonMapper.Object));

            //Lets mock the method of interest
            mock.Setup(x => x.GetPose()).Returns(poseDto);
            //Given
            animatedSkeletons.Add(mock.Object);

            personalSkeleton.Skeletons.AddRange(animatedSkeletons);

            //When
            ISkeleton results = personalSkeleton.Compute();

            //Then
            Assert.AreSame(personalSkeleton, results);
        }

        #endregion Compute
    }
}