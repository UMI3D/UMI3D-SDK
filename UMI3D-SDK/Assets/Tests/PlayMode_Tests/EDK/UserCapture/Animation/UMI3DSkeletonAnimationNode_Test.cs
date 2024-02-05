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

using NUnit.Framework;
using System.Collections.Generic;
using umi3d.edk;
using umi3d.edk.userCapture.animation;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.UserCapture.Animation.EDK
{
    [TestFixture, TestOf(typeof(UMI3DSkeletonAnimationNode))]
    public class UMI3DSkeletonAnimationNode_Test
    {
        protected UMI3DSkeletonAnimationNode skeletonAnimationNode;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            ClearSingletons();

            SceneManager.LoadScene(PlayModeTestHelper.TEST_SCENE_EDK_BASE);
            _ = UMI3DEnvironment.Instance;
            _ = UMI3DServer.Instance;
        }

        [SetUp]
        public void SetUp()
        {
            GameObject go = new GameObject("UMI3DSkeletonAnimationNode");
            UnityEngine.Object.Instantiate(go);
            skeletonAnimationNode = go.AddComponent<UMI3DSkeletonAnimationNode>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.Destroy(skeletonAnimationNode.gameObject);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (UMI3DEnvironment.Exists)
                UnityEngine.Object.Destroy(UMI3DEnvironment.Instance);

            if (UMI3DServer.Exists)
                UnityEngine.Object.Destroy(UMI3DServer.Instance);
        }

        #endregion Test SetUp


        #region GetLoadAnimations

        [Test]
        public void GetLoadAnimations_AnimationsNotGenerated()
        {
            // GIVEN
            // nothing

            // WHEN
            var result = skeletonAnimationNode.GetLoadAnimations();

            // THEN
            Assert.IsNull(result);
        }

        [Test]
        public void GetLoadAnimations_AnimationsGenerated()
        {
            // GIVEN
            // nothing
            skeletonAnimationNode.GenerateAnimations();

            // WHEN
            var result = skeletonAnimationNode.GetLoadAnimations();

            // THEN
            Assert.IsNull(result);
        }

        #endregion GetLoadAnimations

        #region GetDeleteAnimations

        [Test]
        public void GetDeleteAnimations_AnimationsNotGenerated()
        {
            // GIVEN
            // nothing

            // WHEN
            var result = skeletonAnimationNode.GetDeleteAnimations();

            // THEN
            Assert.IsNull(result);
        }

        [Test]
        public void GetDeleteAnimations_AnimationsGenerated([Values(1005uL)] ulong userId,
                                                            [Values(100)] int priority,
                                                            [Values(1, 10)] int nbStates)
        {
            // GIVEN
            List<string> states = new(nbStates);
            for (int i = 0; i < nbStates; i++)
            {
                states.Add(new($"state_{i}"));
            }

            skeletonAnimationNode.animationStates = states;
            skeletonAnimationNode.userId = userId;
            skeletonAnimationNode.priority = priority;
            skeletonAnimationNode.GenerateAnimations();

            // WHEN
            var result = skeletonAnimationNode.GetDeleteAnimations();

            // THEN
            Assert.IsNotNull(result);
        }

        #endregion GetDeleteAnimations

        #region GenerateAnimations

        [Test]
        public void GenerateAnimations_AnimationsNotGenerated([Values(1005uL)] ulong userId,
                                                              [Values(100)] int priority,
                                                              [Values(0,1,10)] int nbStates)
        {
            // GIVEN
            List<string> states = new(nbStates);
            for (int i = 0; i < nbStates; i++)
            {
                states.Add(new($"state_{i}"));
            }

            skeletonAnimationNode.animationStates = states;
            skeletonAnimationNode.userId = userId;
            skeletonAnimationNode.priority = priority;

            // WHEN
            var result = skeletonAnimationNode.GenerateAnimations();

            // THEN
            Assert.IsNotNull(result);
            Assert.AreEqual(states.Count, skeletonAnimationNode.relatedAnimationIds.Length);
        }

        [Test]
        public void GenerateAnimations_AnimationsGenerated()
        {
            // GIVEN
            skeletonAnimationNode.animationStates = new() { "state" };
            skeletonAnimationNode.userId = 1005uL;
            skeletonAnimationNode.priority = 10;
            skeletonAnimationNode.GenerateAnimations();

            // WHEN
            var result = skeletonAnimationNode.GenerateAnimations();

            // THEN
            Assert.IsNull(result);
        }

        #endregion GenerateAnimations
    }
}