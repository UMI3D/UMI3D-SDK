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
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.UserCapture.Description.Common
{
    public class SkeletonMapper_Test
    {
        private SkeletonMapper skeletonMapper;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
            GameObject go = new GameObject("SkeletonMapper");
            UnityEngine.Object.Instantiate(go);
            skeletonMapper = go.AddComponent<SkeletonMapper>();
        }

        #endregion Test SetUp

        #region GetPose

        [Test]
        public void GetPose()
        {
            var hierarchy = new UMI3DSkeletonHierarchy(null);
            // GIVEN
            Queue<SkeletonMapping> mappings = new();

            for (uint i = 0; i < 10; i++)
            {
                var mappingMock = new Mock<SkeletonMapping>(i, null);
                mappingMock.Setup(x => x.GetPose()).Returns(new BoneDto() { boneType = i });
                mappings.Enqueue(mappingMock.Object);
            }

            skeletonMapper.BoneAnchor = new BonePoseDto();
            skeletonMapper.Mappings = mappings.ToArray();

            // WHEN
            var result = skeletonMapper.GetPose(hierarchy);

            // THEN
            Assert.AreEqual(skeletonMapper.BoneAnchor.bone, result.boneAnchor.bone);
            Assert.AreEqual(skeletonMapper.Mappings.Length, result.bones.Count);
        }

        #endregion GetPose
    }
}