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
using umi3d.cdk.userCapture.pose;
using umi3d.cdk.userCapture.tracking;
using umi3d.common.userCapture;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace PlayMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(EnvironmentPoseCondition))]
    public class EnvironmentPoseCondition_Test
    {
        #region Check

        [Test, TestOf(nameof(EnvironmentPoseCondition.Validate))]
        public void Check_Validated()
        {
            // GIVEN
            EnvironmentPoseConditionDto dto = new()
            {
                Id = 1005uL,
                IsValidated = false
            };

            EnvironmentPoseCondition environmentPoseCondition = new (dto);
            environmentPoseCondition.Validate();

            // WHEN
            bool success = environmentPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test, TestOf(nameof(EnvironmentPoseCondition.Invalidate))]
        public void Check_Invalidated()
        {
            // GIVEN
            EnvironmentPoseConditionDto dto = new()
            {
                Id = 1005uL,
                IsValidated = true
            };

            EnvironmentPoseCondition environmentPoseCondition = new(dto);
            environmentPoseCondition.Invalidate();

            // WHEN
            bool success = environmentPoseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        #endregion Check
    }
}