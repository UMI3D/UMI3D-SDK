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
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Description.Common
{
    [TestFixture, TestOf(typeof(SkeletonMapping))]
    public class SkeletonMapping_Test
    {
        private SkeletonMapping skeletonMapping;

        #region GetPose

        [Test]
        public void GetPose()
        {
            // GIVEN
            uint boneType = BoneType.Chest;
            var trasnform = (position: Vector3.zero, rotation: Quaternion.identity);
            Mock<ISkeletonMappingLink> linkMock = new();
            linkMock.Setup(x => x.Compute()).Returns(trasnform);
            skeletonMapping = new SkeletonMapping(boneType, linkMock.Object);

            // WHEN
            var pose = skeletonMapping.GetPose();

            // THEN
            Assert.IsNotNull(pose);
            Assert.AreEqual(boneType, pose.boneType);
            Assert.IsTrue(trasnform.rotation == pose.rotation.Quaternion());
        }

        #endregion GetPose
    }
}