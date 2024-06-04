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
using TestUtils;
using umi3d;
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.tracking.constraint;

namespace EditMode_Tests.UserCapture.Tracking.Common
{
    [TestFixture, TestOf(nameof(BoneConstraintSerializerModule))]
    public class BoneConstraintSerializerModule_Test
    {
        private BoneConstraintSerializerModule boneConstraintSerializerModule;
        protected List<UMI3DSerializerModule> serializationModules = new();

        #region Test SetUp

        [OneTimeSetUp]
        public virtual void InitSerializer()
        {
            boneConstraintSerializerModule = new();
            serializationModules = new()
            {
                 new UMI3DSerializerBasicModules(),
                 new UMI3DSerializerVectorModules(),
                 new UMI3DSerializerStringModules(),
                 boneConstraintSerializerModule
            };

            UMI3DSerializer.AddModule(serializationModules);
        }

        [OneTimeTearDown]
        public virtual void Teardown()
        {
            UMI3DSerializer.RemoveModule(serializationModules);
            serializationModules.Clear();
        }

        #endregion Test SetUp

        #region Read NodeBoneConstraint

        [Test]
        public void Read_NodeBoneConstraint()
        {
            // GIVEN
            NodeBoneConstraintDto dto = new NodeBoneConstraintDto()
            {
                id = 1005uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingNodeId = 16978uL,
                ShouldBeApplied = true,
                PositionOffset = Vector3Dto.one,
                RotationOffset = Vector4Dto.one
            };

            boneConstraintSerializerModule.Write(dto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes(), UMI3DVersion.ComputedVersion);

            // WHEN
            boneConstraintSerializerModule.Read(byteContainer, out bool readable, out NodeBoneConstraintDto result);

            // THEN
            Assert.IsTrue(readable);
            Assert.AreEqual(dto.id, result.id);
            Assert.AreEqual(dto.ConstrainedBone, result.ConstrainedBone);
            Assert.AreEqual(dto.ConstrainingNodeId, result.ConstrainingNodeId);
            Assert.AreEqual(dto.ShouldBeApplied, result.ShouldBeApplied);
            AssertUnityStruct.AreEqual(dto.PositionOffset.Struct(), result.PositionOffset.Struct());
            AssertUnityStruct.AreEqual(dto.RotationOffset.Struct(), result.RotationOffset.Struct());
        }

        #endregion Read NodeBoneConstraint

        #region Read BoneBoneConstraint

        [Test]
        public void Read_BoneBoneConstraint()
        {
            // GIVEN
            BoneBoneConstraintDto dto = new()
            {
                id = 1005uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingBone = BoneType.Head,
                ShouldBeApplied = true,
                PositionOffset = Vector3Dto.one,
                RotationOffset = Vector4Dto.one
            };

            boneConstraintSerializerModule.Write(dto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes(), UMI3DVersion.ComputedVersion);

            // WHEN
            boneConstraintSerializerModule.Read(byteContainer, out bool readable, out BoneBoneConstraintDto result);

            // THEN
            Assert.IsTrue(readable);
            Assert.AreEqual(dto.id, result.id);
            Assert.AreEqual(dto.ConstrainedBone, result.ConstrainedBone);
            Assert.AreEqual(dto.ConstrainingBone, result.ConstrainingBone);
            Assert.AreEqual(dto.ShouldBeApplied, result.ShouldBeApplied);
            AssertUnityStruct.AreEqual(dto.PositionOffset.Struct(), result.PositionOffset.Struct());
            AssertUnityStruct.AreEqual(dto.RotationOffset.Struct(), result.RotationOffset.Struct());
        }

        #endregion Read BoneBoneConstraint

        #region Read FloorBoneConstraint

        [Test]
        public void Read_FloorBoneConstraint()
        {
            // GIVEN
            FloorBoneConstraintDto dto = new()
            {
                id = 1005uL,
                ConstrainedBone = BoneType.Chest,
                ShouldBeApplied = true,
                PositionOffset = Vector3Dto.one,
                RotationOffset = Vector4Dto.one
            };

            boneConstraintSerializerModule.Write(dto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes(), UMI3DVersion.ComputedVersion);

            // WHEN
            boneConstraintSerializerModule.Read(byteContainer, out bool readable, out FloorBoneConstraintDto result);

            // THEN
            Assert.IsTrue(readable);
            Assert.AreEqual(dto.id, result.id);
            Assert.AreEqual(dto.ConstrainedBone, result.ConstrainedBone);
            Assert.AreEqual(dto.ShouldBeApplied, result.ShouldBeApplied);
            AssertUnityStruct.AreEqual(dto.PositionOffset.Struct(), result.PositionOffset.Struct());
            AssertUnityStruct.AreEqual(dto.RotationOffset.Struct(), result.RotationOffset.Struct());
        }

        #endregion Read FloorBoneConstraint
    }
}