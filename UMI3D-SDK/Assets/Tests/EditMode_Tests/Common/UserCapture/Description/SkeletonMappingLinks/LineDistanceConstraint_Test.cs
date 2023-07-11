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
using umi3d.common.userCapture.description;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Description.Common
{
    [TestFixture, TestOf(typeof(LineDistanceConstraintLink))]
    public class LineDistanceConstraint_Test
    {
        private LineDistanceConstraintLink link;

        #region Compute

        [Test]
        public void Compute([Values(1f, 0f, -1f)] float distance)
        {
            // GIVEN
            Mock<ISkeletonMappingLink> nodeMock = new();
            nodeMock.Setup(x => x.Compute()).Returns((Vector3.zero, Quaternion.identity));
            link = new(nodeMock.Object, distance, Vector3.one);

            // WHEN
            var transform = link.Compute();

            // THEN
            Assert.IsNotNull(transform);
            Assert.IsTrue(link.direction.normalized * distance == transform.position, "Positions are not correct.");
        }

        #endregion Compute
    }
}