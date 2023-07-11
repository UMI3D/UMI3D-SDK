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
    [TestFixture, TestOf(typeof(LerpLink))]
    public class LerpLink_Test
    {
        private LerpLink link;

        #region Compute

        [Test]
        public void Compute([Values(0f, 0.25f, 0.5f, 1f)] float factor)
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
            Assert.IsTrue(Vector3.Lerp(Vector3.zero, Vector3.one, link.factor) == transform.position, "Positions are not correct.");
            Assert.IsTrue(Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(Vector3.one * 90f), link.factor) == transform.rotation, "Rotations are not correct.");
        }

        [Test]
        public void Compute_Clamped([Values(-2f, -1f, 1.5f, 2.5f)] float factor)
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
            Assert.IsTrue((factor > 0 ? Vector3.one : Vector3.zero) == transform.position, "Positions are not correct.");
            Assert.IsTrue((factor > 0 ? Quaternion.Euler(Vector3.one * 90f) : Quaternion.identity) == transform.rotation, "Rotations are not correct.");
        }

        #endregion Compute
    }
}