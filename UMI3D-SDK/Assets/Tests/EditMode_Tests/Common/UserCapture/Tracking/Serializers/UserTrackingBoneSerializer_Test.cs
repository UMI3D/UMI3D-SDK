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

namespace EditMode_Tests.UserCapture.Tracking.Common
{
    [TestFixture]
    public class UserTrackingBoneSerializer_Test
    {
        private UserTrackingBoneSerializer serializer;
        protected List<UMI3DSerializerModule> serializationModules = new();

        [OneTimeSetUp]
        public void InitSerializer()
        {
            serializer = new UserTrackingBoneSerializer();

            serializationModules = new()
            {
                 new UMI3DSerializerBasicModules(),
                 new UMI3DSerializerVectorModules(),
                 new UMI3DSerializerStringModules(),
                 new ControllerSerializer(),
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
        [TestCase(1005uL)]
        [TestCase(1006uL)]
        public void WriteRead(ulong userId)
        {
            // GIVEN
            var dto = new UserTrackingBoneDto()
            {
                userId = userId,
                bone = new ControllerDto() { position = new() }
            };

            serializer.Write(dto, out Bytable bytable);
            ByteContainer byteContainer = new ByteContainer(1, bytable.ToBytes());

            // WHEN
            serializer.Read(byteContainer, out bool readable, out UserTrackingBoneDto result);

            // THEN
            Assert.IsTrue(readable, "Bytable is not readable.");
            Assert.AreEqual(dto.userId, result.userId);
            Assert.IsTrue(dto.bone.position.Struct() == result.bone.position.Struct());
            Assert.AreEqual(dto.bone.isOverrider, result.bone.isOverrider);
        }

        #endregion WriteRead
    }
}