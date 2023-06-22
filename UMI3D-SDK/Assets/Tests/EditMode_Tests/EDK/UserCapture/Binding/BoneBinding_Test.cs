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
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.edk;
using umi3d.edk.userCapture;

namespace EditMode_Tests.Core.Bindings.EDK
{
    public class BoneBinding_Test
    {
        #region ToEntityDto

        [Test]
        [TestCase(1005uL, 10058uL, BoneType.Chest)]
        [TestCase(10153uL, 10058uL, BoneType.Hips)]
        public void ToEntityDto(ulong boundNodeId, ulong userId, uint boneType)
        {
            // GIVEN
            Mock<BoneBinding> boneBindingMock = new(boundNodeId, userId, boneType);
            boneBindingMock.CallBase = true;
            boneBindingMock.Setup(m => m.Id()).Returns(10036uL);
            var boneBinding = boneBindingMock.Object;

            // WHEN
            var resultDto = boneBinding.ToEntityDto(null) as BindingDto;

            // THEN
            Assert.AreEqual(boneBinding.Id(),        resultDto.id);
            Assert.AreEqual(boneBinding.boundNodeId, resultDto.boundNodeId);

            Assert.AreEqual(boneBinding.priority,   resultDto.data.priority);
            Assert.AreEqual(boneBinding.partialFit, resultDto.data.partialFit);

            var resultSimpleBindingDataDto = resultDto.data as AbstractSimpleBindingDataDto;
            Assert.AreEqual(boneBinding.syncPosition,   resultSimpleBindingDataDto.syncPosition);
            Assert.AreEqual(boneBinding.syncRotation,   resultSimpleBindingDataDto.syncRotation);
            Assert.AreEqual(boneBinding.syncScale,      resultSimpleBindingDataDto.syncScale);
            Assert.IsTrue(boneBinding.offsetPosition == resultSimpleBindingDataDto.offSetPosition.Struct());
            Assert.IsTrue(boneBinding.offsetRotation == resultSimpleBindingDataDto.offSetRotation.Quaternion());
            Assert.IsTrue(boneBinding.offsetScale    == resultSimpleBindingDataDto.offSetScale.Struct());

            var resultBoneBindingDataDto = resultDto.data as BoneBindingDataDto;
            Assert.AreEqual(boneBinding.userId,     resultBoneBindingDataDto.userId);
            Assert.AreEqual(boneBinding.boneType,   resultBoneBindingDataDto.boneType);
        }

        #endregion ToEntityDto
    }
}