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
using umi3d.edk.binding;

namespace EditMode_Tests.Core.Binding.EDK
{
    public class MultiBinding_Test
    {
        #region ToEntityDto

        [Test]
        [TestCase(1005uL)]
        [TestCase(10153uL)]
        public void ToEntityDto(ulong boundNodeId)
        {
            // GIVEN
            Mock<MultiBinding> multiBindingMock = new(boundNodeId)
            {
                CallBase = true
            };
            multiBindingMock.Setup(m => m.Id()).Returns(10036uL);
            var multiBinding = multiBindingMock.Object;

            // WHEN
            var resultDto = multiBinding.ToEntityDto(null) as BindingDto;

            // THEN
            Assert.AreEqual(multiBinding.Id(),          resultDto.id);
            Assert.AreEqual(multiBinding.boundNodeId,   resultDto.boundNodeId);

            Assert.AreEqual(multiBinding.priority,      resultDto.data.priority);
            Assert.AreEqual(multiBinding.partialFit,    resultDto.data.partialFit);

            var resultMultiBindingDataDto = resultDto.data as MultiBindingDataDto;
            Assert.AreEqual(multiBinding.bindings.Count, resultMultiBindingDataDto.Bindings.Length);
        }

        #endregion ToEntityDto
    }
}