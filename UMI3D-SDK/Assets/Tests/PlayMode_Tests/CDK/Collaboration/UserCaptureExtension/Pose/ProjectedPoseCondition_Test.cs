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
using umi3d.cdk.collaboration.userCapture.pose;
using umi3d.cdk.interaction;
using umi3d.common.collaboration.userCapture.pose.dto;
using umi3d.common.interaction;
using UnityEngine;

namespace PlayMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(ProjectedPoseCondition))]
    public class ProjectedPoseCondition_Test
    {
        private GameObject boneGo;

        #region Test SetUp

        [SetUp]
        public void SetUp()
        {
            boneGo = new GameObject("Bone");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(boneGo);
        }

        #endregion Test SetUp

        #region Check

        [Test]
        public void Check_Unprojected()
        {
            // GIVEN
            ulong interactableId = 1005uL;
            Mock<Interactable> interactableMock = new(new InteractableDto() { id = interactableId });
            ProjectedPoseConditionDto dto = new()
            {
                interactableId = interactableId
            };

            ProjectedPoseCondition poseCondition = new(dto, interactableMock.Object);

            // WHEN
            bool success = poseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public void Check_Projected()
        {
            // GIVEN
            ulong interactableId = 1005uL;
            Mock<Interactable> interactableMock = new(new InteractableDto() { id = interactableId });
            ProjectedPoseConditionDto dto = new()
            {
                interactableId = interactableId
            };

            ProjectedPoseCondition poseCondition = new(dto, interactableMock.Object);
            interactableMock.Object.onProject.Invoke();

            // WHEN
            bool success = poseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_Released()
        {
            // GIVEN
            ulong interactableId = 1005uL;
            Mock<Interactable> interactableMock = new(new InteractableDto() { id = interactableId });
            ProjectedPoseConditionDto dto = new()
            {
                interactableId = interactableId
            };

            ProjectedPoseCondition poseCondition = new(dto, interactableMock.Object);
            interactableMock.Object.onProject.Invoke();
            interactableMock.Object.onRelease.Invoke();

            // WHEN
            bool success = poseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        #endregion Check
    }
}