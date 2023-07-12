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
using umi3d.common.dto.binding;
using umi3d.common.userCapture;
using umi3d.common.userCapture.binding;
using umi3d.edk.userCapture.binding;

namespace EditMode_Tests.Core.Binding.EDK
{
    public class RigBoneBinding_Test
    {
        #region ToEntityDto

        [Test]
        [TestCase(1005uL, "myRig", 10058uL, BoneType.Chest)]
        [TestCase(10153uL, "",  10058uL, BoneType.Hips)]
        public void ToEntityDto(ulong boundNodeId, string rigName, ulong userId, uint boneType)
        {
            // GIVEN
            Mock<RigBoneBinding> rigboneBindingMock = new(boundNodeId, rigName, userId, boneType);
            rigboneBindingMock.CallBase = true;
            rigboneBindingMock.Setup(m => m.Id()).Returns(10036uL);
            var rigboneBinding = rigboneBindingMock.Object;

            // WHEN
            var resultDto = rigboneBinding.ToEntityDto(null) as BindingDto;

            // THEN
            Assert.AreEqual(rigboneBinding.Id(),        resultDto.id);
            Assert.AreEqual(rigboneBinding.boundNodeId, resultDto.boundNodeId);

            Assert.AreEqual(rigboneBinding.priority,   resultDto.data.priority);
            Assert.AreEqual(rigboneBinding.partialFit, resultDto.data.partialFit);

            var resultSimpleBindingDataDto = resultDto.data as AbstractSimpleBindingDataDto;
            Assert.AreEqual(rigboneBinding.syncPosition,   resultSimpleBindingDataDto.syncPosition);
            Assert.AreEqual(rigboneBinding.syncRotation,   resultSimpleBindingDataDto.syncRotation);
            Assert.AreEqual(rigboneBinding.syncScale,      resultSimpleBindingDataDto.syncScale);
            Assert.IsTrue(rigboneBinding.offsetPosition == resultSimpleBindingDataDto.offSetPosition.Struct());
            Assert.IsTrue(rigboneBinding.offsetRotation == resultSimpleBindingDataDto.offSetRotation.Quaternion());
            Assert.IsTrue(rigboneBinding.offsetScale    == resultSimpleBindingDataDto.offSetScale.Struct());

            var resultRigBoneBindingDataDto = resultDto.data as RigBoneBindingDataDto;
            Assert.AreEqual(rigboneBinding.userId,     resultRigBoneBindingDataDto.userId);
            Assert.AreEqual(rigboneBinding.boneType,   resultRigBoneBindingDataDto.boneType);
            Assert.AreEqual(rigboneBinding.rigName,    resultRigBoneBindingDataDto.rigName);
        }

        #endregion ToEntityDto
    }
}