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

namespace PlayMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(AndPoseCondition))]
    public class AndPoseCondition_Test
    {
        [Test]
        public void Check_FalseFalse()
        {
            // GIVEN
            IPoseCondition conditionA = new DummyTestPoseCondition() { isValid = false };
            IPoseCondition conditionB = new DummyTestPoseCondition() { isValid = false };

            AndPoseCondition AndPoseCondition = new(new(), conditionA, conditionB);

            // WHEN
            bool success = AndPoseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public void Check_FalseTrue()
        {
            // GIVEN
            IPoseCondition conditionA = new DummyTestPoseCondition() { isValid = false };
            IPoseCondition conditionB = new DummyTestPoseCondition() { isValid = true };

            AndPoseCondition AndPoseCondition = new(new(), conditionA, conditionB);

            // WHEN
            bool success = AndPoseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public void Check_FalseNull()
        {
            // GIVEN
            IPoseCondition conditionA = new DummyTestPoseCondition() { isValid = false };
            IPoseCondition conditionB = null;

            AndPoseCondition AndPoseCondition = new(new(), conditionA, conditionB);

            // WHEN
            bool success = AndPoseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public void Check_TrueFalse()
        {
            // GIVEN
            IPoseCondition conditionA = new DummyTestPoseCondition() { isValid = true };
            IPoseCondition conditionB = new DummyTestPoseCondition() { isValid = false };

            AndPoseCondition AndPoseCondition = new(new(), conditionA, conditionB);

            // WHEN
            bool success = AndPoseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public void Check_TrueTrue()
        {
            // GIVEN
            IPoseCondition conditionA = new DummyTestPoseCondition() { isValid = true };
            IPoseCondition conditionB = new DummyTestPoseCondition() { isValid = true };

            AndPoseCondition AndPoseCondition = new(new(), conditionA, conditionB);

            // WHEN
            bool success = AndPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_TrueNull()
        {
            // GIVEN
            IPoseCondition conditionA = new DummyTestPoseCondition() { isValid = true };
            IPoseCondition conditionB = null;

            AndPoseCondition AndPoseCondition = new(new(), conditionA, conditionB);

            // WHEN
            bool success = AndPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_NullFalse()
        {
            // GIVEN
            IPoseCondition conditionA = null;
            IPoseCondition conditionB = new DummyTestPoseCondition() { isValid = false };

            AndPoseCondition AndPoseCondition = new(new(), conditionA, conditionB);

            // WHEN
            bool success = AndPoseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public void Check_NullTrue()
        {
            // GIVEN
            IPoseCondition conditionA = null;
            IPoseCondition conditionB = new DummyTestPoseCondition() { isValid = true };

            AndPoseCondition AndPoseCondition = new(new(), conditionA, conditionB);

            // WHEN
            bool success = AndPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_NullNull()
        {
            // GIVEN
            IPoseCondition conditionA = null;
            IPoseCondition conditionB = null;

            AndPoseCondition AndPoseCondition = new(new(), conditionA, conditionB);

            // WHEN
            bool success = AndPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }
    }
}