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
using System.Linq;
using umi3d.cdk.userCapture.pose;
using umi3d.common.userCapture.pose;

namespace EditMode_Tests.UserCapture.Pose.CDK
{
    [TestFixture, TestOf(nameof(PoseOverrider))]
    public class PoseOverrider_Test
    {
        #region CheckConditions

        [Test]
        public void CheckConditions_NoCondition()
        {
            // GIVEN
            PoseOverriderDto dto = new();

            PoseOverrider poseOverrider = new PoseOverrider(dto, new IPoseCondition[0]);

            // WHEN
            bool check = poseOverrider.CheckConditions();

            // THEN
            Assert.IsTrue(check);
        }

        [Test]
        public void CheckConditions_AllTrue()
        {
            // GIVEN
            PoseOverriderDto dto = new();

            Mock<IPoseCondition>[] conditions = new Mock<IPoseCondition>[]
            {
                new(),
                new(),
                new(),
            };

            conditions.ForEach(x => x.Setup(c => c.Check()).Returns(true).Verifiable());

            PoseOverrider poseOverrider = new PoseOverrider(dto, conditions.Select(x => x.Object).ToArray());

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
            PoseOverriderDto dto = new();

            Mock<IPoseCondition>[] conditions = new Mock<IPoseCondition>[]
            {
                new(),
                new(),
                new(),
            };

            conditions[0..2].ForEach(x => x.Setup(c => c.Check()).Returns(true).Verifiable());
            conditions[2].Setup(x => x.Check()).Returns(false);
            PoseOverrider poseOverrider = new PoseOverrider(dto, conditions.Select(x => x.Object).ToArray());

            // WHEN
            bool check = poseOverrider.CheckConditions();

            // THEN
            Assert.IsFalse(check);
            conditions.ForEach(x => x.Verify(c => c.Check(), Times.Once()));
        }

        #endregion CheckConditions
    }
}