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
    [TestFixture, TestOf(typeof(BezierLerpLink))]
    public class BezierLerpLink_Test
    {
        private BezierLerpLink link;

        #region Compute

        [Test]
        public void Compute([Values(0f, 0.25f, 0.75f, 1f)] float factor)
        {
            // GIVEN
            Mock<ISkeletonMappingLink> nodeAMock = new();
            nodeAMock.Setup(x => x.Compute()).Returns((Vector3.zero, Quaternion.identity));

            Mock<ISkeletonMappingLink> nodeBMock = new();
            nodeBMock.Setup(x => x.Compute()).Returns((Vector3.one, Quaternion.Euler(Vector3.one * 90f)));

            link = new(nodeAMock.Object, nodeBMock.Object) { factor = factor };

            // WHEN
            var transform = link.Compute();

            // THEN
            Assert.IsNotNull(transform);
            var positionExpected = Mathf.Pow((factor), 3) * Vector3.one;

            Assert.IsTrue(positionExpected == transform.position, "Positions are not correct.");
            Assert.IsTrue(Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(Vector3.one * 90f), link.factor) == transform.rotation, "Rotations are not correct.");
        }

        #endregion Compute
    }
}