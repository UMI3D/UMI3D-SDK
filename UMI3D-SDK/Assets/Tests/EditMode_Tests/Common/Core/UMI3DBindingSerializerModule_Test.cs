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
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;

namespace EditMode_Tests
{
    public class UMI3DBindingSerializerModule_Test
    {
        protected List<UMI3DSerializerModule> serializationModules = new();

        [OneTimeSetUp]
        public virtual void InitSerializer()
        {
            serializationModules = new()
            {
                 new UMI3DSerializerBasicModules(),
                 new UMI3DSerializerVectorModules(),
                 new UMI3DSerializerStringModules(),
                 new UMI3DBindingSerializerModule()
            };

            UMI3DSerializer.AddModule(serializationModules);
        }

        [OneTimeTearDown]
        public virtual void Teardown()
        {
            UMI3DSerializer.RemoveModule(serializationModules);
            serializationModules.Clear();
        }

        [Test]
        public void WriteRead_Binding()
        {
            NodeBindingDataDto bindingDataDto = new NodeBindingDataDto() {
                nodeId=         12,
                syncPosition= false,
                syncRotation= false,
                syncScale= false,
                offSetPosition= Vector3.zero.Dto(),
                offSetRotation= Quaternion.identity,
                offSetScale= Vector3.one.Dto(),
                anchorPosition= Vector3.zero.Dto(),
                priority= 10,
                partialFit= true
            };

            BindingDto bindingDto = new BindingDto()
            {
                boundNodeId = 123,
                data = bindingDataDto
            };

            Bytable data = UMI3DSerializer.Write(bindingDto);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            bool readable = UMI3DSerializer.TryRead(byteContainer, out BindingDto result);

            Assert.IsTrue(readable, "Object deserialization failed.");
            Assert.AreEqual(bindingDto.boundNodeId, result.boundNodeId);
        }

        #region Simple Bindings

        [Test]
        public void WriteRead_SimpleBinding()
        {
            NodeBindingDataDto bindingDataDto = new NodeBindingDataDto()
            {
                nodeId = 12,
                syncPosition = false,
                syncRotation = false,
                syncScale = false,
                offSetPosition = Vector3.zero.Dto(),
                offSetRotation = Quaternion.identity,
                offSetScale = Vector3.one.Dto(),
                anchorPosition = Vector3.zero.Dto(),
                priority = 10,
                partialFit = true
            };

            BindingDto bindingDto = new BindingDto()
            {
                boundNodeId = 123,
                data = bindingDataDto
            };

            Bytable data = UMI3DSerializer.Write(bindingDto);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            bool readable = UMI3DSerializer.TryRead(byteContainer, out BindingDto result);

            Assert.IsTrue(readable, "Object deserialization failed.");

            Assert.AreEqual(bindingDto.boundNodeId, result.boundNodeId);

            Assert.AreEqual(bindingDto.data.priority, result.data.priority);
            Assert.AreEqual(bindingDto.data.partialFit, result.data.partialFit);

            var simpleBindingDataDto = bindingDto.data as AbstractSimpleBindingDataDto;
            var resultSimpleBindingDataDto = result.data as AbstractSimpleBindingDataDto;
            Assert.AreEqual(simpleBindingDataDto.syncPosition,      resultSimpleBindingDataDto.syncPosition);
            Assert.AreEqual(simpleBindingDataDto.syncRotation,      resultSimpleBindingDataDto.syncRotation);
            Assert.AreEqual(simpleBindingDataDto.syncScale,         resultSimpleBindingDataDto.syncScale);
            Assert.AreEqual(simpleBindingDataDto.offSetPosition.Struct(),    resultSimpleBindingDataDto.offSetPosition.Struct());
            Assert.AreEqual(simpleBindingDataDto.offSetRotation,    resultSimpleBindingDataDto.offSetRotation);
            Assert.AreEqual(simpleBindingDataDto.offSetScale.Struct(),       resultSimpleBindingDataDto.offSetScale.Struct());
        }

        [Test]
        public void WriteRead_NodeBinding()
        {
            NodeBindingDataDto bindingDataDto = new NodeBindingDataDto()
            {
                nodeId = 12,
                syncPosition = false,
                syncRotation = false,
                syncScale = false,
                offSetPosition = Vector3.zero.Dto(),
                offSetRotation = Quaternion.identity,
                offSetScale = Vector3.one.Dto(),
                anchorPosition = Vector3.zero.Dto(),
                priority = 10,
                partialFit = true
            };

            BindingDto bindingDto = new BindingDto()
            {
                boundNodeId = 123,
                data = bindingDataDto
            };

            Bytable data = UMI3DSerializer.Write(bindingDto);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            bool readable = UMI3DSerializer.TryRead(byteContainer, out BindingDto result);

            Assert.IsTrue(readable, "Object deserialization failed.");
            Assert.AreEqual(bindingDto.boundNodeId, result.boundNodeId);

            Assert.AreEqual(bindingDto.data.priority, result.data.priority);
            Assert.AreEqual(bindingDto.data.partialFit, result.data.partialFit);

            var simpleBindingDataDto = bindingDto.data as AbstractSimpleBindingDataDto;
            var resultSimpleBindingDataDto = result.data as AbstractSimpleBindingDataDto;
            Assert.AreEqual(simpleBindingDataDto.syncPosition,      resultSimpleBindingDataDto.syncPosition);
            Assert.AreEqual(simpleBindingDataDto.syncRotation,      resultSimpleBindingDataDto.syncRotation);
            Assert.AreEqual(simpleBindingDataDto.syncScale,         resultSimpleBindingDataDto.syncScale);
            Assert.AreEqual(simpleBindingDataDto.offSetPosition.Struct(),    resultSimpleBindingDataDto.offSetPosition.Struct());
            Assert.AreEqual(simpleBindingDataDto.offSetRotation,    resultSimpleBindingDataDto.offSetRotation);
            Assert.AreEqual(simpleBindingDataDto.offSetScale.Struct(),       resultSimpleBindingDataDto.offSetScale.Struct());

            var nodeBindingDataDto = bindingDto.data as NodeBindingDataDto;
            var resultNodeBindingDataDto = result.data as NodeBindingDataDto;
            Assert.AreEqual(nodeBindingDataDto.nodeId, resultNodeBindingDataDto.nodeId);
        }

        #endregion Simple Bindings

        #region Multi binding

        [Test]
        public virtual void WriteRead_MultiBinding()
        {
            MultiBindingDataDto multiBindingDto = new MultiBindingDataDto()
            {
                priority = 10,
                partialFit = true,
                Bindings = GetTestBindingsArray()
            };

            BindingDto bindingDto = new BindingDto()
            {
                boundNodeId = 123,
                data = multiBindingDto
            };

            Bytable data = UMI3DSerializer.Write(bindingDto);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            bool readable = UMI3DSerializer.TryRead(byteContainer, out BindingDto result);

            Assert.IsTrue(readable, "Object deserialization failed.");
            Assert.AreEqual(bindingDto.boundNodeId, result.boundNodeId);

            Assert.AreEqual(bindingDto.data.priority, result.data.priority);
            Assert.AreEqual(bindingDto.data.partialFit, result.data.partialFit);

            MultiBindingDataDto multiRes = result.data as MultiBindingDataDto;
            for (int i = 0; i < multiRes.Bindings.Length; i++)
            {
                Assert.AreNotSame(multiBindingDto.Bindings[i].priority, multiRes.Bindings[i].priority);
            }
        }

        private AbstractSimpleBindingDataDto[] GetTestBindingsArray()
        {
            static AbstractSimpleBindingDataDto GetMockBindingData(ulong id)
                => new NodeBindingDataDto()
                {
                    nodeId = id,
                    syncPosition = false,
                    syncRotation = false,
                    syncScale = false,
                    offSetPosition = Vector3.zero.Dto(),
                    offSetRotation = Quaternion.identity,
                    offSetScale = Vector3.one.Dto(),
                    anchorPosition = Vector3.zero.Dto(),
                    priority = 10,
                    partialFit = true
                };

            return new AbstractSimpleBindingDataDto[] {
                GetMockBindingData(3),
                GetMockBindingData(4),
                GetMockBindingData(9),
                GetMockBindingData(5)
            };
        }

        #endregion Multi binding
    }
}