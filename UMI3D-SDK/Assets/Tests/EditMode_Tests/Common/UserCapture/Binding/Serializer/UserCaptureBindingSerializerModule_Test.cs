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

using EditMode_Tests.Core.Binding.Common;
using NUnit.Framework;
using umi3d.common;
using umi3d.common.binding;
using umi3d.common.userCapture;
using umi3d.common.userCapture.binding;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Binding.Common
{
    [TestFixture]
    public class UserCaptureBindingSerializerModule_Test : BindingSerializerModule_Test
    {
        [OneTimeSetUp]
        public override void InitSerializer()
        {
            serializationModules = new()
            {
                 new UMI3DSerializerBasicModules(),
                 new UMI3DSerializerVectorModules(),
                 new UMI3DSerializerStringModules(),
                 new UserCaptureBindingSerializerModule()
            };

            UMI3DSerializer.AddModule(serializationModules);
        }

        [OneTimeTearDown]
        public override void Teardown()
        {
            UMI3DSerializer.RemoveModule(serializationModules);
            serializationModules.Clear();
        }

        #region Bone bindings

        [Test]
        [TestCase(159uL, true, true, true, 10, true)]
        [TestCase(1982uL, false, false, false, 1, false)]
        public void WriteRead_BoneBinding(ulong userId, bool syncPos, bool syncRot, bool syncScale, int prio, bool partial)
        {
            BoneBindingDataDto bindingDataDto = new BoneBindingDataDto()
            {
                boneType = BoneType.LeftEye,
                userId = userId,
                syncPosition = syncPos,
                syncRotation = syncRot,
                syncScale = syncScale,
                offSetPosition = Vector3.zero.Dto(),
                offSetRotation = Quaternion.identity.Dto(),
                offSetScale = Vector3.one.Dto(),
                anchorPosition = Vector3.zero.Dto(),
                priority = prio,
                partialFit = partial
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

            Assert.AreEqual(bindingDto.data.priority,   result.data.priority);
            Assert.AreEqual(bindingDto.data.partialFit, result.data.partialFit);

            var simpleBindingDataDto = bindingDto.data as AbstractSimpleBindingDataDto;
            var resultSimpleBindingDataDto = result.data as AbstractSimpleBindingDataDto;
            Assert.AreEqual(simpleBindingDataDto.syncPosition,      resultSimpleBindingDataDto.syncPosition);
            Assert.AreEqual(simpleBindingDataDto.syncRotation,      resultSimpleBindingDataDto.syncRotation);
            Assert.AreEqual(simpleBindingDataDto.syncScale,         resultSimpleBindingDataDto.syncScale);
            Assert.AreEqual(simpleBindingDataDto.offSetPosition.Struct(),    resultSimpleBindingDataDto.offSetPosition.Struct());
            Assert.AreEqual(simpleBindingDataDto.offSetRotation.Struct(), resultSimpleBindingDataDto.offSetRotation.Struct());
            Assert.AreEqual(simpleBindingDataDto.offSetScale.Struct(),       resultSimpleBindingDataDto.offSetScale.Struct());

            var boneBindingDataDto = bindingDto.data as BoneBindingDataDto;
            var resultBoneBindingDataDto = result.data as BoneBindingDataDto;
            Assert.AreEqual(boneBindingDataDto.boneType,    resultBoneBindingDataDto.boneType);
            Assert.AreEqual(boneBindingDataDto.userId,      resultBoneBindingDataDto.userId);
        }

        [Test]
        [TestCase(159uL, "Rig1", true, true, true, 10, true)]
        [TestCase(1982uL, "Rig2", false, false, false, 1, false)]
        public void WriteRead_RiggedBoneBinding(ulong userId, string rigName, bool syncPos, bool syncRot, bool syncScale, int prio, bool partial)
        {
            RigBoneBindingDataDto bindingDataDto = new RigBoneBindingDataDto()
            {
                boneType = BoneType.LeftEye,
                userId = userId,
                rigName = rigName,
                syncPosition = syncPos,
                syncRotation = syncRot,
                syncScale = syncScale,
                offSetPosition = Vector3.zero.Dto(),
                offSetRotation = Quaternion.identity.Dto(),
                offSetScale = Vector3.one.Dto(),
                anchorPosition = Vector3.zero.Dto(),
                priority = prio,
                partialFit = partial
            };

            BindingDto bindingDto = new BindingDto() { 
                boundNodeId = 123, 
                data = bindingDataDto 
            };

            Bytable data = UMI3DSerializer.Write(bindingDto);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            bool readable = UMI3DSerializer.TryRead(byteContainer, out BindingDto result);

            Assert.IsTrue(readable, "Object deserialization failed.");
            Assert.AreEqual(bindingDto.boundNodeId,     result.boundNodeId);

            Assert.AreEqual(bindingDto.data.priority,   result.data.priority);
            Assert.AreEqual(bindingDto.data.partialFit, result.data.partialFit);

            var simpleBindingDataDto = bindingDto.data as AbstractSimpleBindingDataDto;
            var resultSimpleBindingDataDto = result.data as AbstractSimpleBindingDataDto;
            Assert.AreEqual(simpleBindingDataDto.syncPosition,      resultSimpleBindingDataDto.syncPosition);
            Assert.AreEqual(simpleBindingDataDto.syncRotation,      resultSimpleBindingDataDto.syncRotation);
            Assert.AreEqual(simpleBindingDataDto.syncScale,         resultSimpleBindingDataDto.syncScale);
            Assert.AreEqual(simpleBindingDataDto.offSetPosition.Struct(),    resultSimpleBindingDataDto.offSetPosition.Struct());
            Assert.AreEqual(simpleBindingDataDto.offSetRotation.Struct(), resultSimpleBindingDataDto.offSetRotation.Struct());
            Assert.AreEqual(simpleBindingDataDto.offSetScale.Struct(),       resultSimpleBindingDataDto.offSetScale.Struct());

            var boneBindingDataDto = bindingDto.data as BoneBindingDataDto;
            var resultBoneBindingDataDto = result.data as BoneBindingDataDto;
            Assert.AreEqual(boneBindingDataDto.boneType,    resultBoneBindingDataDto.boneType);
            Assert.AreEqual(boneBindingDataDto.userId,      resultBoneBindingDataDto.userId);

            var rigBindingDataDto = bindingDto.data as RigBoneBindingDataDto;
            var resultRigBindingDataDto = result.data as RigBoneBindingDataDto;
            Assert.AreEqual(rigBindingDataDto.rigName,      resultRigBindingDataDto.rigName);
        }

        #endregion Bone bindings

        #region Multi binding

        [Test]
        public override void WriteRead_MultiBinding()
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
                    parentNodeId = id,
                    syncPosition = false,
                    syncRotation = false,
                    syncScale = false,
                    offSetPosition = Vector3.zero.Dto(),
                    offSetRotation = Quaternion.identity.Dto(),
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