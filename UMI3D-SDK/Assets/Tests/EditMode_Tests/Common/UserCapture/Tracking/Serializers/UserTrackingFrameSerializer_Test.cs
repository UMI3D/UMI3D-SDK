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
    public class UserTrackingFrameSerializer_Test
    {
        private UserTrackingFrameSerializer serializer;
        protected List<UMI3DSerializerModule> serializationModules = new();

        [OneTimeSetUp]
        public void InitSerializer()
        {
            serializer = new UserTrackingFrameSerializer();

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
        [TestCase(1005uL, 1006uL, 0, 0, 0)]
        [TestCase(1005uL, 1006uL, 1, 1, 1)]
        [TestCase(1005uL, 1006uL, 2, 3, 4)]
        public void WriteRead(ulong userId, ulong parentId, int nbTrackedbones, int nbServerPoses, int nbUserPoses)
        {
            // GIVEN
            var rng = new System.Random();

            var dto = new UserTrackingFrameDto()
            {
                userId = userId,
                parentId = parentId,
                position = Vector3.one.Dto(),
                rotation = Quaternion.identity.Dto()
            };
            dto.trackedBones = new(nbTrackedbones);
            for (int i = 0; i < nbTrackedbones; i++)
                dto.trackedBones.Add(new ControllerDto() { position = new() });

            dto.playerServerPoses = new(nbServerPoses);
            for (int i = 0; i < nbServerPoses; i++)
                dto.playerServerPoses.Add(rng.Next());

            dto.playerUserPoses = new(nbUserPoses);
            for (int i = 0; i < nbServerPoses; i++)
                dto.playerUserPoses.Add(rng.Next());

            serializer.Write(dto, out Bytable bytable);
            ByteContainer byteContainer = new ByteContainer(1, bytable.ToBytes());

            // WHEN
            serializer.Read(byteContainer, out bool readable, out UserTrackingFrameDto result);

            // THEN
            Assert.IsTrue(readable, "Bytable is not readable.");
            Assert.AreEqual(dto.userId, result.userId);
            Assert.AreEqual(dto.parentId, result.parentId);

            Assert.IsTrue(dto.position.Struct() == result.position.Struct(), "Position is not the same.");
            Assert.IsTrue(dto.rotation.Struct() == result.rotation.Struct(), "Rotation is not the same.");

            Assert.AreEqual(dto.trackedBones.Count, result.trackedBones.Count);
            for (var i = 0; i < dto.trackedBones.Count; i++)
                Assert.AreEqual(dto.trackedBones[i].boneType, result.trackedBones[i].boneType);

            Assert.AreEqual(dto.playerServerPoses.Count, result.playerServerPoses.Count);
            for (var i = 0; i < dto.playerServerPoses.Count; i++)
                Assert.AreEqual(dto.playerServerPoses[i], result.playerServerPoses[i]);

            Assert.AreEqual(dto.playerUserPoses.Count, result.playerUserPoses.Count);
            for (var i = 0; i < dto.playerUserPoses.Count; i++)
                Assert.AreEqual(dto.playerUserPoses[i], result.playerUserPoses[i]);
        }

        #endregion WriteRead
    }
}