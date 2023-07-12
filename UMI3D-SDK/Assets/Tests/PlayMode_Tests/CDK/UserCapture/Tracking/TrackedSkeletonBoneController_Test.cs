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
using umi3d.common.userCapture;
using UnityEngine.SceneManagement;
using UnityEngine;
using umi3d.cdk.userCapture.tracking;

namespace PlayMode_Tests.UserCapture.Tracking.CDK
{

    public class TrackedSkeletonBoneController_Test
    {
        protected GameObject controllerGo;
        protected TrackedSkeletonBoneController skeletonBoneController;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public void Setup()
        {
            controllerGo = new GameObject("ControllerGo");
            skeletonBoneController = controllerGo.AddComponent<TrackedSkeletonBoneController>();
            Object.Instantiate(controllerGo);
            controllerGo.transform.position = Vector3.one;
            controllerGo.transform.rotation = Quaternion.identity;
            controllerGo.transform.localScale = Vector3.one;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(controllerGo);
        }

        #endregion Test SetUp

        #region ToBoneDto

        [Test]
        public void ToBoneDto_BoneTypeNone_Test()
        {
            // GIVEN
            skeletonBoneController.boneType = BoneType.None;

            // WHEN
            var dto = skeletonBoneController.ToBoneDto();

            // THEN
            Assert.IsTrue(dto == null);
        }

        [Test]
        public void ToBoneDto_BoneTypeNotNone_Test([Random(1, 58, 1)] int bonetype)
        {
            // GIVEN
            skeletonBoneController.boneType = (uint)bonetype;

            // WHEN
            var dto = skeletonBoneController.ToBoneDto();

            // THEN
            Assert.AreEqual(skeletonBoneController.boneType, dto.boneType);
            Assert.AreEqual(skeletonBoneController.transform.rotation, dto.rotation.Quaternion());
        }

        #endregion ToBoneDto

        #region ToControllerDto

        [Test]
        public void ToControllerDto_BoneTypeNone_Test()
        {
            // GIVEN
            skeletonBoneController.boneType = BoneType.None;

            // WHEN
            var dto = skeletonBoneController.ToControllerDto();

            // THEN
            Assert.IsTrue(dto == null);
        }

        [Test]
        public void ToControllerDto_BoneTypeNotNone_Test([Random(1, 58, 1)] int bonetype)
        {
            // GIVEN
            skeletonBoneController.boneType = (uint)bonetype;

            // WHEN
            var dto = skeletonBoneController.ToControllerDto();

            // THEN
            Assert.AreEqual(skeletonBoneController.boneType, dto.boneType);
            Assert.AreEqual(skeletonBoneController.transform.rotation, dto.rotation.Quaternion());
            Assert.AreEqual(skeletonBoneController.transform.position, dto.position.Struct());
            Assert.IsTrue(dto.isOverrider);
        }

        #endregion ToControllerDto
    }
}
