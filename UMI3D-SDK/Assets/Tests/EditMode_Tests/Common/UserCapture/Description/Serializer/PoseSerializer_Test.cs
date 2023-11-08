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
using System.Linq;
using umi3d.common;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;

namespace EditMode_Tests.UserCapture.Pose.Common
{
    public class PoseSerializerModule_Test
    {
        private PoseSerializerModule poseSerializerModule;
        protected List<UMI3DSerializerModule> serializationModules = new();

        [OneTimeSetUp]
        public virtual void InitSerializer()
        {
            poseSerializerModule = new();
            serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();

            UMI3DSerializer.AddModule(serializationModules);
        }

        [OneTimeTearDown]
        public virtual void Teardown()
        {
            UMI3DSerializer.RemoveModule(serializationModules);
            serializationModules.Clear();
        }

        #region Read Pose Anchor

        [Test]
        public void Read_PoseAnchorDto()
        {
            // GIVEN
            PoseAnchorDto bonePoseDto = new PoseAnchorDto()
            {
                bone = 2u,
                position = Vector3Dto.one,
                rotation = Vector4Dto.one
            };

            poseSerializerModule.Write(bonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            // WHEN
            poseSerializerModule.Read(byteContainer, out bool readable, out PoseAnchorDto result);

            // THEN
            Assert.IsTrue(readable);

            Assert.AreEqual(bonePoseDto.bone, result.bone);
            Assert.IsTrue(result.position.Struct() == bonePoseDto.position.Struct());
            Assert.IsTrue(result.rotation.Struct() == bonePoseDto.rotation.Struct());
        }

        [Test]
        public void Read_BonePoseAnchorDto()
        {
            BonePoseAnchorDto anchorBonePoseDto = new BonePoseAnchorDto()
            {
                bone = 2,
                position = Vector3Dto.one,
                rotation = Vector4Dto.one,
                otherBone = 17
            };

            poseSerializerModule.Write(anchorBonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseSerializerModule.Read(byteContainer, out bool readable, out PoseAnchorDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue((result.bone == anchorBonePoseDto.bone));
            Assert.IsTrue((result.position.Struct() == anchorBonePoseDto.position.Struct()));
            Assert.IsTrue((result.rotation.Struct() == anchorBonePoseDto.rotation.Struct()));

            Assert.IsTrue(((result as BonePoseAnchorDto).otherBone == anchorBonePoseDto.otherBone));
        }

        [Test]
        public void Read_NodePoseAnchorDto()
        {
            NodePoseAnchorDto nodeAnchoredBonePoseDto = new NodePoseAnchorDto()
            {
                bone = 2,
                position = Vector3Dto.one,
                rotation = Vector4Dto.one,
                node = 17
            };
            poseSerializerModule.Write(nodeAnchoredBonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseSerializerModule.Read(byteContainer, out bool readable, out PoseAnchorDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue((result.bone == nodeAnchoredBonePoseDto.bone));
            Assert.IsTrue((result.position.Struct() == nodeAnchoredBonePoseDto.position.Struct()));
            Assert.IsTrue((result.rotation.Struct() == nodeAnchoredBonePoseDto.rotation.Struct()));

            Assert.IsTrue(((result as NodePoseAnchorDto).node == nodeAnchoredBonePoseDto.node));
        }

        [Test]
        public void Read_FloorPoseAnchorDto()
        {
            FloorPoseAnchorDto floorAnchoredBonePoseDto = new FloorPoseAnchorDto()
            {
                bone = 2,
                position = Vector3Dto.one,
                rotation = Vector4Dto.one
            };

            poseSerializerModule.Write(floorAnchoredBonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseSerializerModule.Read(byteContainer, out bool readable, out PoseAnchorDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue((result.bone == floorAnchoredBonePoseDto.bone));
            Assert.IsTrue((result.position.Struct() == floorAnchoredBonePoseDto.position.Struct()));
            Assert.IsTrue((result.rotation.Struct() == floorAnchoredBonePoseDto.rotation.Struct()));
        }

        #endregion Read Pose Anchor

        #region Read Pose

        [Test]
        public void Read_PoseDto()
        {
            // GIVEN
            PoseDto poseDto = new PoseDto()
            {
                bones = GetTestBonePoseDtoSample(),
                anchor = new PoseAnchorDto() { bone = 24u, position = Vector3Dto.zero, rotation = Vector4Dto.one }
            };

            poseSerializerModule.Write(poseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            // WHEN
            poseSerializerModule.Read(byteContainer, out bool readable, out PoseDto result);

            // THEN
            Assert.IsTrue(readable);
            for (int i = 0; i < poseDto.bones.Count; i++)
            {
                Assert.AreEqual(poseDto.bones[i].boneType, (result.bones[i]).boneType);
            }

            Assert.AreEqual(poseDto.anchor.bone, result.anchor.bone);
        }

        private List<BoneDto> GetTestBonePoseDtoSample()
        {
            return new List<BoneDto>
            {
                new BoneDto() {boneType = 1u, rotation = Vector4Dto.one },
                new BoneDto() {boneType = 15u, rotation = Vector4Dto.one }
            };
        }

        #endregion Read Pose
    }
}