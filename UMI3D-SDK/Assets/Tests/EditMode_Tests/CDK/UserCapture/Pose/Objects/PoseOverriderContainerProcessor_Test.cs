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
    [TestFixture, TestOf(nameof(PoseOverridersContainerProcessor))]
    public class PoseOverriderContainerProcessor_Test
    {
        private PoseOverridersContainerProcessor processor;
        private Mock<ICoroutineService> coroutineServiceMock;

        #region Test SetUp

        [SetUp]
        public void SetUp()
        {
            coroutineServiceMock = new Mock<ICoroutineService>();
            UMI3DPoseOverridersContainerDto dto = new()
            {
                id = 1009uL,
                relatedNodeId = 1005uL,
                poseOverriderDtos = new PoseOverriderDto[]
                {
                    new PoseOverriderDto()
                    {
                        poseConditions = new AbstractPoseConditionDto[0],
                        poseIndexInPoseManager = 1,
                        activationMode = (ushort)PoseActivationMode.AUTO
                    },
                    new PoseOverriderDto()
                    {
                        poseConditions = new AbstractPoseConditionDto[0],
                        poseIndexInPoseManager = 2,
                        activationMode = (ushort)PoseActivationMode.ON_REQUEST
                    }
                }
            };
            PoseOverrider[] overriders = dto.poseOverriderDtos
                                                .Select(x => new PoseOverrider(x, new IPoseCondition[0]))
                                                .ToArray();

            PoseOverridersContainer container = new PoseOverridersContainer(dto, overriders);
            processor = new(container, coroutineServiceMock.Object);
        }

        #endregion Test SetUp

        #region PoseOverridersContainerProcessor

        [Test]
        public void PoseOverridersContainerProcessor_Null()
        {
            // GIVEN
            PoseOverridersContainer container = null;

            // WHEN
            TestDelegate action = () => new PoseOverridersContainerProcessor(container);

            // THEN
            Assert.Throws<ArgumentNullException>(() => action());
        }

        #endregion PoseOverridersContainerProcessor

        #region StartWatchActivationConditions

        [Test]
        public void StartWatchActivationConditions()
        {
            // given
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false));

            // when
            processor.StartWatchActivationConditions();

            // then
            Assert.IsTrue(processor.IsWatching);
        }

        #endregion StartWatchActivationConditions

        #region StopWatchActivationConditions

        [Test]
        public void StopWatchActivationConditions()
        {
            // given
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false));
            coroutineServiceMock.Setup(x => x.DetachCoroutine(It.IsAny<Coroutine>()));
            processor.StartWatchActivationConditions();

            // when
            processor.StopWatchActivationConditions();

            // then
            Assert.IsFalse(processor.IsWatching);
        }

        #endregion StopWatchActivationConditions

        #region TryActivate

        [Test]
        public void TryActivate()
        {
            // given
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false));

            // when
            bool success = processor.Activate();

            //then
            Assert.IsTrue(success);
        }

        [Test]
        public void TryActivate_AlreadyActivated()
        {
            // given
            coroutineServiceMock.Setup(x => x.AttachCoroutine(It.IsAny<IEnumerator>(), false));
            processor.Activate();

            // when
            bool success = processor.Activate();

            //then
            Assert.IsFalse(success);
        }

        [Test]
        public void TryActivate_Environmental()
        {
            // given

            // when
            bool success = processor.Activate();

            //then
            Assert.IsFalse(success);
        }

        #endregion TryActivate
    }
}