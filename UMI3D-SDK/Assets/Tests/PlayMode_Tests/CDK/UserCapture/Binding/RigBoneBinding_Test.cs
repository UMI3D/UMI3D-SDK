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
using System.Collections;
using System.Collections.Generic;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.binding;
using umi3d.common.userCapture;
using umi3d.common.userCapture.binding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PlayMode_Tests.UserCapture.Binding.CDK
{
    public class RigBoneBinding_Test
    {
        protected GameObject parentGo;
        protected GameObject rigGo;
        protected GameObject go;

        #region Test SetUp

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            SceneManager.LoadScene(PlayModeTestHelper.EMPTY_TEST_SCENE_NAME);
        }

        [SetUp]
        public void SetUp()
        {
            parentGo = new GameObject("Parent Node");
            Object.Instantiate(parentGo);
            parentGo.transform.position = Vector3.zero;
            parentGo.transform.rotation = Quaternion.identity;
            parentGo.transform.localScale = Vector3.one;

            go = new GameObject("Bound Node");
            Object.Instantiate(go);
            go.transform.position = Vector3.one;
            go.transform.rotation = Quaternion.Euler(90, 90, 90);
            go.transform.localScale = Vector3.one * 0.5f;

            rigGo = new GameObject("Rig Bound Node");
            Object.Instantiate(rigGo);
            rigGo.transform.SetParent(go.transform);
            rigGo.transform.localPosition = Vector3.one;
            rigGo.transform.localRotation = Quaternion.Euler(90, 90, 90);
            rigGo.transform.localScale = Vector3.one * 0.5f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(parentGo);
            Object.Destroy(go);
        }

        #endregion Test SetUp

        #region Apply

        private static Vector3[] positionOffsets = new Vector3[] { Vector3.zero, Vector3.one, new Vector3(1, -1, 25) };
        private static Quaternion[] rotationOffsets = new Quaternion[] { Quaternion.identity, Quaternion.Euler(10, 10, 10), Quaternion.Euler(-10, 10, -10) };
        private static Vector3[] scaleOffsets = new Vector3[] { Vector3.zero, Vector3.one, new Vector3(1, -1, 25) };

        #region Position

        [Test]
        public void Apply_SyncPosition([ValueSource("positionOffsets")] Vector3 offSetPosition)
        {
            // GIVEN
            uint boneType = BoneType.Chest;
            var dto = new RigBoneBindingDataDto()
            {
                boneType = boneType,
                syncPosition = true,
                offSetPosition = offSetPosition.Dto()
            };

            var skeletonBoneMock = new Mock<ISkeleton>();

            var skeletonBones = new Dictionary<uint, ISkeleton.s_Transform>()
            {
                { boneType, new() { s_Position = parentGo.transform.position, s_Rotation= parentGo.transform.rotation } }
            };

            skeletonBoneMock.Setup(x => x.Bones).Returns(skeletonBones);

            RigBoneBinding binding = new(dto, rigGo.transform, skeletonBoneMock.Object);

            var previousPosition = rigGo.transform.position;
            var previousRotation = rigGo.transform.rotation;
            var previousScale = rigGo.transform.localScale;

            // WHEN
            binding.Apply(out bool succes);

            // THEN
            Assert.IsTrue(succes);
            Assert.IsTrue(parentGo.transform.position + offSetPosition == rigGo.transform.position);
            Assert.IsTrue(previousRotation == rigGo.transform.rotation);
            Assert.IsTrue(previousScale == rigGo.transform.localScale);
        }

        [UnityTest]
        public IEnumerator Apply_SyncPosition_SeveralFrames([ValueSource("positionOffsets")] Vector3 offSetPosition)
        {
            // GIVEN
            uint boneType = BoneType.Chest;
            var dto = new RigBoneBindingDataDto()
            {
                boneType = boneType,
                syncPosition = true,
                offSetPosition = offSetPosition.Dto()
            };

            var skeletonBoneMock = new Mock<ISkeleton>();

            var skeletonBones = new Dictionary<uint, ISkeleton.s_Transform>()
            {
                { boneType, new() { s_Position = parentGo.transform.position, s_Rotation= parentGo.transform.rotation } }
            };

            skeletonBoneMock.Setup(x => x.Bones).Returns(skeletonBones);

            RigBoneBinding binding = new(dto, rigGo.transform, skeletonBoneMock.Object);

            var previousPosition = rigGo.transform.position;
            var previousRotation = rigGo.transform.rotation;
            var previousScale = rigGo.transform.localScale;

            int numberOfFrames = 10;

            for (int i = 0; i < numberOfFrames; i++)
            {
                // GIVEN
                parentGo.transform.Translate(1, 1, 1);
                parentGo.transform.Rotate(1, 1, 1);

                skeletonBones[boneType].s_Position = parentGo.transform.position;
                skeletonBones[boneType].s_Rotation = parentGo.transform.rotation;

                // WHEN
                binding.Apply(out bool succes);

                // THEN
                Assert.IsTrue(succes);
                Assert.IsTrue(parentGo.transform.position + offSetPosition == rigGo.transform.position);
                Assert.IsTrue(previousRotation == rigGo.transform.rotation);
                Assert.IsTrue(previousScale == rigGo.transform.localScale);

                yield return null;
            }
        }

        #endregion Position

        #region Rotation

        [Test]
        public void Apply_SyncRotation([ValueSource("rotationOffsets")] Quaternion offsetRotation)
        {
            // GIVEN
            uint boneType = BoneType.Chest;
            var dto = new RigBoneBindingDataDto()
            {
                boneType = boneType,
                syncRotation = true,
                offSetRotation = offsetRotation.Dto()
            };

            var skeletonBoneMock = new Mock<ISkeleton>();

            var skeletonBones = new Dictionary<uint, ISkeleton.s_Transform>()
            {
                { boneType, new() { s_Position = parentGo.transform.position, s_Rotation= parentGo.transform.rotation } }
            };

            skeletonBoneMock.Setup(x => x.Bones).Returns(skeletonBones);

            RigBoneBinding binding = new(dto, rigGo.transform, skeletonBoneMock.Object);

            var previousPosition = rigGo.transform.position;
            var previousRotation = rigGo.transform.rotation;
            var previousScale = rigGo.transform.localScale;

            // WHEN
            binding.Apply(out bool success);

            // THEN
            Assert.IsTrue(success);
            Assert.IsTrue(previousPosition == rigGo.transform.position);
            Assert.IsTrue(parentGo.transform.rotation * offsetRotation == rigGo.transform.rotation);
            Assert.IsTrue(previousScale == rigGo.transform.localScale);
        }

        [UnityTest]
        public IEnumerator Apply_SyncRotation_SeveralFrames([ValueSource("rotationOffsets")] Quaternion offsetRotation)
        {
            // GIVEN
            uint boneType = BoneType.Chest;
            var dto = new RigBoneBindingDataDto()
            {
                boneType = boneType,
                syncRotation = true,
                offSetRotation = offsetRotation.Dto()
            };

            var skeletonBoneMock = new Mock<ISkeleton>();

            var skeletonBones = new Dictionary<uint, ISkeleton.s_Transform>()
            {
                { boneType, new() { s_Position = parentGo.transform.position, s_Rotation= parentGo.transform.rotation } }
            };

            skeletonBoneMock.Setup(x => x.Bones).Returns(skeletonBones);

            RigBoneBinding binding = new(dto, rigGo.transform, skeletonBoneMock.Object);

            var previousPosition = rigGo.transform.position;
            var previousRotation = rigGo.transform.rotation;
            var previousScale = rigGo.transform.localScale;

            int numberOfFrames = 10;

            for (int i = 0; i < numberOfFrames; i++)
            {
                // GIVEN
                parentGo.transform.Translate(1, 1, 1);
                parentGo.transform.Rotate(1, 1, 1);
                skeletonBones[boneType].s_Position = parentGo.transform.position;
                skeletonBones[boneType].s_Rotation = parentGo.transform.rotation;

                // WHEN
                binding.Apply(out bool success);

                // THEN
                Assert.IsTrue(success);
                Assert.IsTrue(previousPosition == rigGo.transform.position);
                Assert.IsTrue(parentGo.transform.rotation * offsetRotation == rigGo.transform.rotation);
                Assert.IsTrue(previousScale == rigGo.transform.localScale);

                yield return null;
            }
        }

        #endregion Rotation

        #region Scale

        [Test]
        public void Apply_SyncScale([ValueSource("scaleOffsets")] Vector3 offSetScale)
        {
            // GIVEN
            uint boneType = BoneType.Chest;
            var dto = new RigBoneBindingDataDto()
            {
                boneType = boneType,
                syncScale = true,
                offSetScale = offSetScale.Dto()
            };

            var skeletonBoneMock = new Mock<ISkeleton>();

            var skeletonBones = new Dictionary<uint, ISkeleton.s_Transform>()
            {
                { boneType, new() { s_Position = parentGo.transform.position, s_Rotation= parentGo.transform.rotation } }
            };

            skeletonBoneMock.Setup(x => x.Bones).Returns(skeletonBones);

            RigBoneBinding binding = new(dto, rigGo.transform, skeletonBoneMock.Object);

            var previousPosition = rigGo.transform.position;
            var previousRotation = rigGo.transform.rotation;
            var previousScale = rigGo.transform.localScale;

            // WHEN
            binding.Apply(out bool succes);

            // THEN
            Assert.IsTrue(succes);
            Assert.IsTrue(previousPosition == rigGo.transform.position);
            Assert.IsTrue(previousRotation == rigGo.transform.rotation);
            Assert.IsTrue(Vector3.Scale(parentGo.transform.localScale, rigGo.transform.localScale) == rigGo.transform.localScale);
        }

        [UnityTest]
        public IEnumerator Apply_SyncScale_SeveralFrames([ValueSource("scaleOffsets")] Vector3 offSetScale)
        {
            // GIVEN
            uint boneType = BoneType.Chest;
            var dto = new RigBoneBindingDataDto()
            {
                boneType = boneType,
                syncScale = true,
                offSetScale = offSetScale.Dto()
            };

            var skeletonBoneMock = new Mock<ISkeleton>();

            var skeletonBones = new Dictionary<uint, ISkeleton.s_Transform>()
            {
                { boneType, new() { s_Position = parentGo.transform.position, s_Rotation= parentGo.transform.rotation } }
            };

            skeletonBoneMock.Setup(x => x.Bones).Returns(skeletonBones);

            RigBoneBinding binding = new(dto, rigGo.transform, skeletonBoneMock.Object);

            var previousPosition = rigGo.transform.position;
            var previousRotation = rigGo.transform.rotation;
            var previousScale = rigGo.transform.localScale;

            int numberOfFrames = 10;

            for (int i = 0; i < numberOfFrames; i++)
            {
                // GIVEN
                parentGo.transform.Translate(1, 1, 1);
                parentGo.transform.Rotate(1, 1, 1);
                skeletonBones[boneType].s_Position = parentGo.transform.position;
                skeletonBones[boneType].s_Rotation = parentGo.transform.rotation;

                // WHEN
                binding.Apply(out bool success);

                // THEN
                Assert.IsTrue(success);
                Assert.IsTrue(previousPosition == rigGo.transform.position);
                Assert.IsTrue(previousRotation == rigGo.transform.rotation);
                Assert.IsTrue(Vector3.Scale(parentGo.transform.localScale, rigGo.transform.localScale) == rigGo.transform.localScale);

                yield return null;
            }
        }

        #endregion Scale

        #endregion Apply
    }
}