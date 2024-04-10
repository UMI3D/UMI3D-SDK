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
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Description.Common
{
    public class BoneSerializer_Test
    {
        private BoneSerializer serializer;
        protected List<UMI3DSerializerModule> serializationModules = new();

        [OneTimeSetUp]
        public void InitSerializer()
        {
            serializer = new BoneSerializer();

            serializationModules = new()
            {
                 new UMI3DSerializerBasicModules(),
                 new UMI3DSerializerVectorModules(),
                 new UMI3DSerializerStringModules(),
                 serializer
            };

            UMI3DSerializer.AddModule(serializationModules);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            UMI3DSerializer.RemoveModule(serializationModules);
            serializationModules.Clear();
        }

        #region WriteRead

        [Test]
        [TestCase(BoneType.Chest)]
        [TestCase(BoneType.Hips)]
        public void WriteRead(uint boneType)
        {
            // GIVEN
            var dto = new BoneDto()
            {
                rotation = (Quaternion.identity).Dto(),
                boneType = boneType,
            };

            serializer.Write(dto, out Bytable bytable);
            ByteContainer byteContainer = new ByteContainer(0, 1, bytable.ToBytes());

            // WHEN
            serializer.Read(byteContainer, out bool readable, out BoneDto result);

            // THEN
            Assert.IsTrue(readable, "Bytable is not readable.");
            Assert.AreEqual(dto.boneType, result.boneType);
            Assert.IsTrue(dto.rotation.Struct() == result.rotation.Struct(), "Rotation is not the same.");
        }

        #endregion WriteRead
    }
}