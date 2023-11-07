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
using umi3d.common.userCapture;
using umi3d.common.userCapture.description;
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Pose.Common
{
    public class PoseSerializerModule_Test
    {
        private PoseSerializerModule poseSerializerModule = null;
        protected List<UMI3DSerializerModule> serializationModules = new();

        [OneTimeSetUp]
        public virtual void InitSerializer()
        {
            poseSerializerModule = new PoseSerializerModule();
            serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();

            UMI3DSerializer.AddModule(serializationModules);
        }

        [OneTimeTearDown]
        public virtual void Teardown()
        {
            UMI3DSerializer.RemoveModule(serializationModules);
            serializationModules.Clear();
        }

        #region Read Bone Pose

        [Test]
        public void ReadBonePose()
        {
            // GIVEN
            BonePoseDto bonePoseDto = new BonePoseDto()
            {
                bone = 2u,
                position = Vector3Dto.one,
                rotation = Vector4Dto.one
            };

            poseSerializerModule.Write(bonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            // WHEN
            poseSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);

            // THEN
            Assert.IsTrue(readable);

            Assert.AreEqual(bonePoseDto.bone, result.bone);
            Assert.IsTrue(result.position.Struct() == bonePoseDto.position.Struct());
            Assert.IsTrue(result.rotation.Struct() == bonePoseDto.rotation.Struct());
        }

        [Test]
        public void ReadBonePose_AnchoredBonePose()
        {
            AnchoredBonePoseDto anchorBonePoseDto = new AnchoredBonePoseDto()
            {
                bone = 2,
                position = Vector3Dto.one,
                rotation = Vector4Dto.one,
                otherBone = 17
            };

            poseSerializerModule.Write(anchorBonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            poseSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
            Assert.IsTrue(readable);

            Assert.AreEqual((anchorBonePoseDto as BonePoseDto).bone, (result as BonePoseDto).bone);
            Assert.IsTrue(((result as BonePoseDto).position.Struct()
                 == (anchorBonePoseDto as BonePoseDto).position.Struct()));
            Assert.IsTrue(((result as BonePoseDto).rotation.Struct()
                == (anchorBonePoseDto as BonePoseDto).rotation.Struct()));

            Assert.IsTrue(((result as AnchoredBonePoseDto).otherBone
                == (anchorBonePoseDto as AnchoredBonePoseDto).otherBone));
        }

        [Test]
        public void ReadBonePose_NodeAnchoredBonePose()
        {
            NodeAnchoredBonePoseDto nodeAnchoredBonePoseDto = new NodeAnchoredBonePoseDto()
            {
                bone = 2,
                position = Vector3Dto.one,
                rotation = Vector4Dto.one,
                node = 17
            };
            poseSerializerModule.Write(nodeAnchoredBonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            poseSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue(((result as BonePoseDto).bone
                == (nodeAnchoredBonePoseDto as BonePoseDto).bone));
            Assert.IsTrue(((result as BonePoseDto).position.Struct()
                 == (nodeAnchoredBonePoseDto as BonePoseDto).position.Struct()));
            Assert.IsTrue(((result as BonePoseDto).rotation.Struct()
                == (nodeAnchoredBonePoseDto as BonePoseDto).rotation.Struct()));

            Assert.IsTrue(((result as NodeAnchoredBonePoseDto).node
                == (nodeAnchoredBonePoseDto as NodeAnchoredBonePoseDto).node));
        }

        [Test]
        public void ReadBonePose_FloorAnchoredBonePoseDto()
        {
            FloorAnchoredBonePoseDto floorAnchoredBonePoseDto = new FloorAnchoredBonePoseDto()
            {
                bone = 2,
                position = Vector3Dto.one,
                rotation = Vector4Dto.one
            };

            poseSerializerModule.Write(floorAnchoredBonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            poseSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue(((result as BonePoseDto).bone
                == (floorAnchoredBonePoseDto as BonePoseDto).bone));
            Assert.IsTrue(((result as BonePoseDto).position.Struct()
                 == (floorAnchoredBonePoseDto as BonePoseDto).position.Struct()));
            Assert.IsTrue(((result as BonePoseDto).rotation.Struct()
                == (floorAnchoredBonePoseDto as BonePoseDto).rotation.Struct()));
        }

        #endregion Read Bone Pose

        #region ReadPose

        [Test]
        public void ReadPose()
        {
            // GIVEN
            PoseDto poseDto = new PoseDto()
            {
                bones = GetTestBonePoseDtoSample(),
                boneAnchor = new BonePoseDto() { bone = 24u, position = Vector3Dto.zero, rotation = Vector4Dto.one }
            };

            poseSerializerModule.Write(poseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            // WHEN
            poseSerializerModule.Read(byteContainer, out bool readable, out PoseDto result);

            // THEN
            Assert.IsTrue(readable);
            for (int i = 0; i < poseDto.bones.Count; i++)
            {
                Assert.AreEqual(poseDto.bones[i].boneType, (result.bones[i]).boneType);
            }

            Assert.AreEqual(poseDto.boneAnchor.bone, result.boneAnchor.bone);
        }

        private List<BoneDto> GetTestBonePoseDtoSample()
        {
            return new List<BoneDto>
            {
                new BoneDto() {boneType = 1u, rotation = Vector4Dto.one },
                new BoneDto() {boneType = 15u, rotation = Vector4Dto.one }
            };
        }

        [Test]
        public void ReadPoseOverrider()
        {
            int poseIndexinPoseManager = 12;

            PoseOverriderDto poseOverriderDto = new PoseOverriderDto()
            {
                poseIndexInPoseManager =  poseIndexinPoseManager,
                poseConditions = GetConditionsTestSet(),
                duration = new DurationDto() { duration=24, max=12, min=2},
                isInterpolable = true,
                isComposable = false,
                activationMode = 0,
            };

            poseSerializerModule.Write(poseOverriderDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            poseSerializerModule.Read(byteContainer, out bool readable, out PoseOverriderDto result);
            Assert.IsTrue(readable);

            int poseIndex = result.poseIndexInPoseManager;

            Assert.IsTrue(poseIndex == poseIndexinPoseManager);
            Assert.IsTrue((result.poseConditions[0] as UserScaleConditionDto).Scale.Struct()
                == (poseOverriderDto.poseConditions[0] as UserScaleConditionDto).Scale.Struct());
            Assert.IsTrue((result.poseConditions[1] as DirectionConditionDto).Direction.Struct()
                == (poseOverriderDto.poseConditions[1] as DirectionConditionDto).Direction.Struct());

            Assert.AreEqual(poseOverriderDto.duration.duration, result.duration.duration);
            Assert.AreEqual(poseOverriderDto.isInterpolable, result.isInterpolable);
            Assert.AreEqual(poseOverriderDto.isComposable, result.isComposable);
        }

        private AbstractPoseConditionDto[] GetConditionsTestSet()
        {
            return new AbstractPoseConditionDto[]{
                new UserScaleConditionDto() { Scale = Vector3.one.Dto() },
                new DirectionConditionDto() { Direction = Vector3.one.Dto() },
            };
        }

        #endregion ReadPose

        #region ReadDuration

        [Test]
        public void ReadDuration()
        {
            DurationDto duration = new DurationDto()
            {
                duration = 159,
                min = 5,
                max = 89486
            };

            poseSerializerModule.Write(duration, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            poseSerializerModule.Read(byteContainer, out bool readable, out DurationDto result);
            Assert.IsTrue(readable);

            Assert.AreEqual(duration.duration, result.duration);
            Assert.AreEqual(duration.max, result.max);
            Assert.AreEqual(duration.min, result.min);
        }

        #endregion ReadDuration
    }
}