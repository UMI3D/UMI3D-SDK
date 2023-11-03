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
using System;
using System.Collections;
using System.Linq;
using umi3d.cdk.userCapture.pose;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(PoseAnimator))]
    public class PoseAnimator_Test
    {
        #region CheckConditions

        [Test]
        public void CheckConditions_NoCondition()
        {
            // GIVEN
            PoseAnimatorDto dto = new();

            PoseAnimator poseOverrider = new PoseAnimator(dto, new IPoseCondition[0]);

            // WHEN
            bool check = poseOverrider.CheckConditions();

            // THEN
            Assert.IsTrue(check);
        }

        [Test]
        public void CheckConditions_AllTrue()
        {
            // GIVEN
            PoseAnimatorDto dto = new();

            Mock<IPoseCondition>[] conditions = new Mock<IPoseCondition>[]
            {
                new(),
                new(),
                new(),
            };

            conditions.ForEach(x => x.Setup(c => c.Check()).Returns(true).Verifiable());

            PoseAnimator poseOverrider = new PoseAnimator(dto, conditions.Select(x => x.Object).ToArray());

            // WHEN
            bool check = poseOverrider.CheckConditions();

            // THEN
            Assert.IsTrue(check);
            conditions.ForEach(x => x.Verify(c => c.Check(), Times.Once()));
        }

        [Test]
        public void CheckConditions_OneFalse()
        {
            // GIVEN
            PoseAnimatorDto dto = new();

            Mock<IPoseCondition>[] conditions = new Mock<IPoseCondition>[]
            {
                new(),
                new(),
                new(),
            };

            conditions[0..2].ForEach(x => x.Setup(c => c.Check()).Returns(true).Verifiable());
            conditions[2].Setup(x => x.Check()).Returns(false);
            PoseAnimator poseOverrider = new PoseAnimator(dto, conditions.Select(x => x.Object).ToArray());

            // WHEN
            bool check = poseOverrider.CheckConditions();

            // THEN
            Assert.IsFalse(check);
            conditions.ForEach(x => x.Verify(c => c.Check(), Times.Once()));
        }

        #endregion CheckConditions

        private PoseAnimator poseAnimator;
        private Mock<ICoroutineService> coroutineServiceMock;

        #region Test SetUp

        [SetUp]
        public void SetUp()
        {
            coroutineServiceMock = new Mock<ICoroutineService>();
            PoseAnimatorDto dto = new PoseAnimatorDto()
            {
            };
            poseAnimator = new PoseAnimator(dto, new IPoseCondition[0], coroutineServiceMock.Object);

        }

        #endregion Test SetUp

        #region PoseAnimator

        [Test]
        public void PoseAnimator_Null()
        {
            // GIVEN
            PoseAnimatorDto dto = null;

            // WHEN
            TestDelegate action = () => new PoseAnimator(dto, new IPoseCondition[0], coroutineServiceMock.Object);

            // THEN
            Assert.Throws<ArgumentNullException>(() => action());
        }

        #endregion PoseAnimator

        #region StartWatchActivationConditions

        [Test]
        public void StartWatchActivationConditions()
        {
            // given
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false));

            // when
            poseAnimator.StartWatchActivationConditions();

            // then
            Assert.IsTrue(poseAnimator.IsWatching);
        }

        #endregion StartWatchActivationConditions

        #region StopWatchActivationConditions

        [Test]
        public void StopWatchActivationConditions()
        {
            // given
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false));
            coroutineServiceMock.Setup(x => x.DetachCoroutine(It.IsAny<Coroutine>()));
            poseAnimator.StartWatchActivationConditions();

            // when
            poseAnimator.StopWatchActivationConditions();

            // then
            Assert.IsFalse(poseAnimator.IsWatching);
        }

        #endregion StopWatchActivationConditions

        #region TryActivate

        [Test]
        public void TryActivate()
        {
            // given
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false));
            PoseAnimatorDto dto = new PoseAnimatorDto()
            {
                activationMode = (ushort)PoseAnimatorActivationMode.ON_REQUEST
            };
            poseAnimator = new PoseAnimator(dto, new IPoseCondition[0], coroutineServiceMock.Object);

            // when
            bool success = poseAnimator.Activate();

            //then
            Assert.IsTrue(success);
        }

        [Test]
        public void TryActivate_AlreadyActivated()
        {
            // given
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false));
            poseAnimator.Activate();

            // when
            bool success = poseAnimator.Activate();

            //then
            Assert.IsFalse(success);
        }

        [Test]
        public void TryActivate_Environmental()
        {
            // given

            // when
            bool success = poseAnimator.Activate();

            //then
            Assert.IsFalse(success);
        }

        #endregion TryActivate
    }
}