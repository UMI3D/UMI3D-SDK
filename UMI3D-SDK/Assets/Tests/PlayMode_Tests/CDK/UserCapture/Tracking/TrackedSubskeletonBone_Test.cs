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
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayMode_Tests.UserCapture.Tracking.CDK
{
    [TestFixture, TestOf(typeof(TrackedSubskeletonBone))]
    public class TrackedSubskeletonBone_Test
    {
        protected GameObject boneGo;
        protected TrackedSubskeletonBone skeletonBone;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public void Setup()
        {
            boneGo = new GameObject("BoneGo");
            skeletonBone = boneGo.AddComponent<TrackedSubskeletonBone>();
            Object.Instantiate(boneGo);
            boneGo.transform.position = Vector3.one;
            boneGo.transform.rotation = Quaternion.identity;
            boneGo.transform.localScale = Vector3.one;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(boneGo);
        }

        #endregion Test SetUp

        #region ToBoneDto

        [Test]
        public void ToBoneDto_BoneTypeNone_Test()
        {
            // GIVEN
            skeletonBone.boneType = BoneType.None;

            // WHEN
            var dto = skeletonBone.ToBoneDto();

            // THEN
            Assert.IsTrue(dto == null);
        }

        [Test]
        public void ToBoneDto_BoneTypeNotNone_Test([Random(1, 58, 1)] int bonetype)
        {
            // GIVEN
            skeletonBone.boneType = (uint)bonetype;

            // WHEN
            var dto = skeletonBone.ToBoneDto();

            // THEN
            Assert.AreEqual(skeletonBone.boneType, dto.boneType);
            Assert.AreEqual(skeletonBone.transform.localRotation, dto.localRotation.Quaternion());
        }

        #endregion ToBoneDto

        #region ToControllerDto

        [Test]
        public void ToControllerDto_BoneTypeNone_Test()
        {
            // GIVEN
            skeletonBone.boneType = BoneType.None;

            // WHEN
            var dto = skeletonBone.ToControllerDto();

            // THEN
            Assert.IsTrue(dto == null);
        }

        [Test]
        public void ToControllerDto_BoneTypeNotNone_Test([Random(1, 58, 1)] int bonetype)
        {
            // GIVEN
            skeletonBone.boneType = (uint)bonetype;

            // WHEN
            var dto = skeletonBone.ToControllerDto();

            // THEN
            Assert.AreEqual(skeletonBone.boneType, dto.boneType);
            Assert.AreEqual(skeletonBone.transform.rotation, dto.rotation.Quaternion());
            Assert.AreEqual(skeletonBone.transform.position, dto.position.Struct());
            Assert.IsFalse(dto.isOverrider);
        }

        #endregion ToControllerDto
    }
}
