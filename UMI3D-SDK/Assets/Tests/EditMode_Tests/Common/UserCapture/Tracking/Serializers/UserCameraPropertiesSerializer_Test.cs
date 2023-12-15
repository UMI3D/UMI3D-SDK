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
using umi3d.common.userCapture.tracking;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Tracking.Common
{
    [TestFixture]
    public class UserCameraPropertiesSerializer_Test
    {
        private UserCameraPropertiesSerializer serializer;
        protected List<UMI3DSerializerModule> serializationModules = new();

        [OneTimeSetUp]
        public void InitSerializer()
        {
            serializer = new UserCameraPropertiesSerializer();

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
        [TestCase(0f, BoneType.Chest)]
        [TestCase(1f, BoneType.Hips)]
        [TestCase(float.MinValue, BoneType.Head)]
        public void WriteRead(float scale, uint boneType)
        {
            // GIVEN
            var dto = new UserCameraPropertiesDto()
            {
                scale = scale,
                boneType = boneType,
                projectionMatrix = new Matrix4x4().Dto()
            };

            serializer.Write(dto, out Bytable bytable);
            ByteContainer byteContainer = new ByteContainer(0, 1, bytable.ToBytes());

            // WHEN
            serializer.Read(byteContainer, out bool readable, out UserCameraPropertiesDto result);

            // THEN
            Assert.IsTrue(readable, "Bytable is not readable.");
            Assert.AreEqual(dto.scale, result.scale, 1e-6f);
            Assert.AreEqual(dto.boneType, result.boneType);
            Assert.IsTrue(dto.projectionMatrix.Struct() == result.projectionMatrix.Struct(), "Projection matrix is not the same.");
        }

        #endregion WriteRead
    }
}