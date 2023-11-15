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
using System.Collections.Generic;
using umi3d.cdk.userCapture.pose;

namespace PlayMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(OrPoseCondition))]
    public class OrPoseCondition_Test
    {
        [Test]
        public void Check_Empty()
        {
            // GIVEN
            List<IPoseCondition> conditionsA = new();
            List<IPoseCondition> conditionsB = new();

            OrPoseCondition orPoseCondition = new(new(), conditionsA, conditionsB);

            // WHEN
            bool success = orPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_FalseFalse()
        {
            // GIVEN
            List<IPoseCondition> conditionsA = new() { new DummyTestPoseCondition() { isValid = false } };
            List<IPoseCondition> conditionsB = new() { new DummyTestPoseCondition() { isValid = true },
                                                                       new DummyTestPoseCondition() { isValid = false } };

            OrPoseCondition orPoseCondition = new(new(), conditionsA, conditionsB);

            // WHEN
            bool success = orPoseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public void Check_FalseTrue()
        {
            // GIVEN
            List<IPoseCondition> conditionsA = new() { new DummyTestPoseCondition() { isValid = false } };
            List<IPoseCondition> conditionsB = new() { new DummyTestPoseCondition() { isValid = true } };

            OrPoseCondition orPoseCondition = new(new(), conditionsA, conditionsB);

            // WHEN
            bool success = orPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_TrueFalse()
        {
            // GIVEN
            List<IPoseCondition> conditionsA = new() { new DummyTestPoseCondition() { isValid = true } };
            List<IPoseCondition> conditionsB = new() { new DummyTestPoseCondition() { isValid = false } };

            OrPoseCondition orPoseCondition = new(new(), conditionsA, conditionsB);

            // WHEN
            bool success = orPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_TrueTrue()
        {
            // GIVEN
            List<IPoseCondition> conditionsA = new() { new DummyTestPoseCondition() { isValid = true } };
            List<IPoseCondition> conditionsB = new() { new DummyTestPoseCondition() { isValid = true } };

            OrPoseCondition orPoseCondition = new(new(), conditionsA, conditionsB);

            // WHEN
            bool success = orPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_TrueTrueMany()
        {
            // GIVEN
            List<IPoseCondition> conditionsA = new() { new DummyTestPoseCondition() { isValid = true }, new DummyTestPoseCondition() { isValid = true } };
            List<IPoseCondition> conditionsB = new() { new DummyTestPoseCondition() { isValid = true }, new DummyTestPoseCondition() { isValid = true } };

            OrPoseCondition orPoseCondition = new(new(), conditionsA, conditionsB);

            // WHEN
            bool success = orPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }
    }

    internal class DummyTestPoseCondition : IPoseCondition
    {
        public bool isValid;

        public bool Check()
        {
            return isValid;
        }
    }
}