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
using umi3d.cdk.userCapture.pose;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace PlayMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(ScalePoseCondition))]
    public class ScalePoseCondition_Test
    {
        private GameObject nodeGo;

        #region Test SetUp

        [SetUp]
        public void SetUp()
        {
            nodeGo = new GameObject("Node");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(nodeGo);
        }

        #endregion Test SetUp

        #region Check

        [Test]
        public void Check_SmallEnough()
        {
            // GIVEN
            ScaleConditionDto dto = new()
            {
                Scale = Vector3.one.Dto(),
                TargetId = 1005uL,
            };

            ScalePoseCondition poseCondition = new(dto, nodeGo.transform);

            nodeGo.transform.localScale = Vector3.one * 0.2f;

            // WHEN
            bool success = poseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_NotSmallEnough()
        {
            // GIVEN
            ScaleConditionDto dto = new()
            {
                Scale = Vector3.one.Dto(),
                TargetId = 1005uL,
            };

            ScalePoseCondition poseCondition = new(dto, nodeGo.transform);

            nodeGo.transform.localScale = Vector3.one * 1.2f;

            // WHEN
            bool success = poseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public void Check_Equal()
        {
            // GIVEN
            ScaleConditionDto dto = new()
            {
                Scale = Vector3.one.Dto(),
                TargetId = 1005uL,
            };

            ScalePoseCondition poseCondition = new(dto, nodeGo.transform);

            nodeGo.transform.localScale = Vector3.one;

            // WHEN
            bool success = poseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        #endregion Check
    }
}