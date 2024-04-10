/*
Copyright 2019 - 2024 Inetum

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
using TestUtils;
using umi3d.cdk.userCapture.tracking.constraint;
using UnityEngine;
using UnityEngine.TestTools;

namespace PlayMode_Tests.UserCapture.Tracking.Constraint.CDK
{
    [TestFixture, TestOf(typeof(ConstrainedSimulatedTracker))]
    public class ConstrainedSimulatedTracker_Test
    {
        private ConstrainedSimulatedTracker constrainedSimulatedTracker;
        private Mock<IBoneConstraint> boneConstraintMock;

        #region Test SetUp

        [SetUp]
        public void SetUp()
        {
            GameObject go = new GameObject();
            constrainedSimulatedTracker = go.AddComponent<ConstrainedSimulatedTracker>();

            boneConstraintMock = new();
            constrainedSimulatedTracker.Init(boneConstraintMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.Destroy(constrainedSimulatedTracker);
        }

        #endregion Test SetUp

        #region Update

        [UnityTest]
        public IEnumerator Update_Applied()
        {
            // given
            boneConstraintMock.Setup(x => x.IsApplied).Returns(true);

            (Vector3 position, Quaternion rotation) mouvement = (new Vector3(12, 13, 15), Quaternion.Euler(2, 3, 8));
            boneConstraintMock.Setup(x => x.Resolve()).Returns(mouvement);

            // when
            yield return null;

            // then
            AssertUnityStruct.AreEqual(mouvement.position, constrainedSimulatedTracker.transform.position);
            AssertUnityStruct.AreEqual(mouvement.rotation, constrainedSimulatedTracker.transform.rotation);
        }

        [UnityTest]
        public IEnumerator Update_NotApplied()
        {
            // given
            boneConstraintMock.Setup(x => x.IsApplied).Returns(false);

            (Vector3 position, Quaternion rotation) mouvement = (new Vector3(12, 13, 15), Quaternion.Euler(2, 3, 8));
            boneConstraintMock.Setup(x => x.Resolve()).Returns(mouvement);

            (Vector3 position, Quaternion rotation) initialPosition = (constrainedSimulatedTracker.transform.position, constrainedSimulatedTracker.transform.rotation);

            // when
            yield return null;

            // then
            AssertUnityStruct.AreEqual(initialPosition.position, constrainedSimulatedTracker.transform.position);
            AssertUnityStruct.AreEqual(initialPosition.rotation, constrainedSimulatedTracker.transform.rotation);
        }

        #endregion Update
    }
}