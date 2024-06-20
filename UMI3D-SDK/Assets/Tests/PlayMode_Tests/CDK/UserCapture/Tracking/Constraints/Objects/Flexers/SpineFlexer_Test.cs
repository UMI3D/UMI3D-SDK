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
using System.Collections;
using System.Collections.Generic;
using TestUtils;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.tracking;
using umi3d.cdk.userCapture.tracking.constraint;
using umi3d.common.core;
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace PlayMode_Tests.UserCapture.Tracking.Constraint.CDK
{
    [TestFixture, TestOf(nameof(SpineFlexer))]
    public class SpineFlexer_Test
    {
        private Mock<ISkeleton> skeletonMock;
        private Mock<ICoroutineService> coroutineServiceMock;

        private SpineFlexer spineFlexer;

        #region SetUp

        [SetUp]
        public void Setup()
        {
            skeletonMock = new();
            coroutineServiceMock = new();
            spineFlexer = new(skeletonMock.Object, coroutineServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion SetUp

        #region Enable

        [Test, TestOf(nameof(SpineFlexer.Enable))]
        public void Enable()
        {
            // given
            SetupCorrectSkeleton();

            Coroutine flexCoroutine = null; // mock
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false)).Returns(flexCoroutine);

            bool eventCalled = false;
            SpineFlexer.Enabled += () => { eventCalled = true; };

            // when
            spineFlexer.Enable();

            // then
            Assert.IsTrue(spineFlexer.IsEnabled);
            Assert.IsTrue(eventCalled);
        }

        [Test]
        public void Enable_AlreadyEnabled()
        {
            // given
            SetupCorrectSkeleton();

            Coroutine flexCoroutine = null; // mock
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false)).Returns(flexCoroutine);

            spineFlexer.Enable();

            bool eventCalled = false;
            SpineFlexer.Enabled += () => { eventCalled = true; };

            // when
            spineFlexer.Enable();

            // then
            Assert.IsTrue(spineFlexer.IsEnabled);
            Assert.IsFalse(eventCalled);
        }

        #endregion Enable

        #region Disable

        [Test, TestOf(nameof(SpineFlexer.Disable))]
        public void Disable()
        {
            // given
            SetupCorrectSkeleton();

            Coroutine flexCoroutine = null; // mock
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false)).Returns(flexCoroutine);
            coroutineServiceMock.Setup(x => x.DetachCoroutine(It.IsAny<Coroutine>()));

            spineFlexer.Enable();

            bool eventCalled = false;
            SpineFlexer.Disabled += () => { eventCalled = true; };

            // when
            spineFlexer.Disable();

            // then
            Assert.IsFalse(spineFlexer.IsEnabled);
            Assert.IsTrue(eventCalled);
        }

        [Test]
        public void Disable_AlreadyDisabled()
        {
            // given
            SetupCorrectSkeleton();

            Coroutine flexCoroutine = null; // mock
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false)).Returns(flexCoroutine);
            coroutineServiceMock.Setup(x => x.DetachCoroutine(It.IsAny<Coroutine>()));

            spineFlexer.Enable();
            spineFlexer.Disable();

            bool eventCalled = false;
            SpineFlexer.Disabled += () => { eventCalled = true; };

            // when
            spineFlexer.Disable();

            // then
            Assert.IsFalse(spineFlexer.IsEnabled);
            Assert.IsFalse(eventCalled);
        }

        #endregion Disable

        #region Flex

        [Test, TestOf(nameof(SpineFlexer.Flex))]
        public void Flex()
        {
            // given
            var trackedSubskeletonMock = SetupCorrectSkeleton();

            Coroutine flexCoroutine = null; // mock
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false)).Returns(flexCoroutine);

            spineFlexer.Enable();

            Mock<IController> controllerHeadMock = new Mock<IController>();
            controllerHeadMock.Setup(x => x.rotation).Returns(Quaternion.Euler(0, 90, 0));
            controllers.Add(BoneType.Head, controllerHeadMock.Object);

            Mock<IController> controllerHipsMock = new Mock<IController>();
            controllerHipsMock.Setup(x => x.rotation).Returns(Quaternion.Euler(0, 0, 0));
            controllers.Add(BoneType.Hips, controllerHipsMock.Object);

            // when
            spineFlexer.Flex();

            // then
            AssertUnityStruct.AreEqual(Quaternion.Euler(0, 30, 0), skeletonMock.Object.TrackedSubskeleton.Controllers[BoneType.Spine].transformation.LocalRotation);
            AssertUnityStruct.AreEqual(Quaternion.Euler(0, 30, 0), skeletonMock.Object.TrackedSubskeleton.Controllers[BoneType.Chest].transformation.LocalRotation);
            AssertUnityStruct.AreEqual(Quaternion.Euler(0, 30, 0), skeletonMock.Object.TrackedSubskeleton.Controllers[BoneType.UpperChest].transformation.LocalRotation);
        }

        [Test]
        public void Flex_NoHead()
        {
            // given
            // not enabled

            // when
            TestDelegate action = () => spineFlexer.Flex();

            // then
            Assert.DoesNotThrow(action);
        }

        [Test]
        public void Flex_Disabled()
        {
            // given
            // not enabled

            // when
            TestDelegate action = () => spineFlexer.Flex();

            // then
            Assert.DoesNotThrow(action);
        }

        #endregion Flex

        private Dictionary<uint, IController> controllers = new();

        private Mock<ITrackedSubskeleton> SetupCorrectSkeleton()
        {
            Dictionary<uint, UnityTransformation> bones = CreateHierarchy();
            skeletonMock.Setup(x => x.Bones).Returns(bones);

            Mock<ITrackedSubskeleton> trackedSubkeletonMock = new();
            controllers = new();

            trackedSubkeletonMock.Setup(x => x.ReplaceController(It.IsAny<IController>(), It.IsAny<bool>()))
                                  .Callback<IController, bool>((c, b) => controllers.Add(c.boneType, c));

            trackedSubkeletonMock.Setup(x => x.Controllers).Returns(controllers);

            skeletonMock.Setup(x => x.TrackedSubskeleton).Returns(trackedSubkeletonMock.Object);

            return trackedSubkeletonMock;
        }

        private static Dictionary<uint, UnityTransformation> CreateHierarchy()
        {
            GameObject rootGo = new GameObject(BoneTypeHelper.GetBoneName(BoneType.Hips));

            Dictionary<uint, UnityTransformation> bones = new()
            {
                [BoneType.Hips] = new(rootGo.transform)
            };

            GameObject CreateChild(GameObject parent, uint boneType, out GameObject go)
            {
                go = new GameObject(BoneTypeHelper.GetBoneName(boneType));
                go.transform.SetParent(parent.transform);
                bones[boneType] = new(go.transform);
                return go;
            }

            CreateChild(rootGo, BoneType.Spine, out var spineGo);
            CreateChild(spineGo, BoneType.Chest, out var chestGo);
            CreateChild(chestGo, BoneType.UpperChest, out var upperChestGo);
            CreateChild(upperChestGo, BoneType.Neck, out var neckGo);

            return bones;
        }
    }
}