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
using umi3d.edk;

namespace EditMode_Tests.Core.Bindings.EDK
{
    public class NodeBinding_Test
    {
        #region ToEntityDto

        [Test]
        [TestCase(1005uL, 1008uL)]
        [TestCase(10153uL, 1008uL)]
        public void ToEntityDto(ulong boundNodeId, ulong nodeId)
        {
            // GIVEN
            Mock<NodeBinding> nodeBindingMock = new(boundNodeId, nodeId);
            nodeBindingMock.CallBase = true;
            nodeBindingMock.Setup(m => m.Id()).Returns(10036uL);

            var nodeBinding = nodeBindingMock.Object;
            // WHEN
            var resultDto = nodeBinding.ToEntityDto(null) as BindingDto;

            // THEN
            Assert.AreEqual(nodeBinding.Id(),           resultDto.id);
            Assert.AreEqual(nodeBinding.boundNodeId,    resultDto.boundNodeId);

            Assert.AreEqual(nodeBinding.priority,       resultDto.data.priority);
            Assert.AreEqual(nodeBinding.partialFit,     resultDto.data.partialFit);

            var resultSimpleBindingDataDto = resultDto.data as AbstractSimpleBindingDataDto;
            Assert.AreEqual(nodeBinding.syncPosition,   resultSimpleBindingDataDto.syncPosition);
            Assert.AreEqual(nodeBinding.syncRotation,   resultSimpleBindingDataDto.syncRotation);
            Assert.AreEqual(nodeBinding.syncScale,      resultSimpleBindingDataDto.syncScale);
            Assert.IsTrue(nodeBinding.offsetPosition == resultSimpleBindingDataDto.offSetPosition.Struct());
            Assert.IsTrue(nodeBinding.offsetRotation == resultSimpleBindingDataDto.offSetRotation.Quaternion());
            Assert.IsTrue(nodeBinding.offsetScale    == resultSimpleBindingDataDto.offSetScale.Struct());

            var resultNodeBindingDataDto = resultDto.data as NodeBindingDataDto;
            Assert.AreEqual(nodeBinding.nodeId,         resultNodeBindingDataDto.nodeId);
        }

        #endregion ToEntityDto
    }
}