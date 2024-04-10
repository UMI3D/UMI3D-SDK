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
    [TestFixture, TestOf(nameof(NotPoseCondition))]
    public class NotPoseCondition_Test
    {
        [Test]
        public void Check_Null()
        {
            // GIVEN
            IPoseCondition condition = null;

            NotPoseCondition notPoseCondition = new(new(), condition);

            // WHEN
            bool success = notPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_False()
        {
            // GIVEN
            IPoseCondition condition = new DummyTestPoseCondition() { isValid = false };

            NotPoseCondition notPoseCondition = new(new(), condition);

            // WHEN
            bool success = notPoseCondition.Check();

            // THEN
            Assert.IsTrue(success);
        }

        [Test]
        public void Check_True()
        {
            // GIVEN
            IPoseCondition condition = new DummyTestPoseCondition() { isValid = true };

            NotPoseCondition notPoseCondition = new(new(), condition);

            // WHEN
            bool success = notPoseCondition.Check();

            // THEN
            Assert.IsFalse(success);
        }
    }
}