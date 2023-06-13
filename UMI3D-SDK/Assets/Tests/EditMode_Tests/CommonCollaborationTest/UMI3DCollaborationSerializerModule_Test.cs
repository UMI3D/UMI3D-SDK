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
using System.Collections.Generic;
using System.Linq;
using umi3d.cdk;
using umi3d.cdk.collaboration;
using umi3d.common.collaboration;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.userCapture;
using UnityEngine;
using System;
using UnityEditor;

namespace EditMode_Tests
{
    public class UMI3DCollaborationSerializerModule_Test
    {
        umi3d.common.collaboration.UMI3DCollaborationSerializerModule collabSerializerModule = null;
        protected List<UMI3DSerializerModule> serializationModules = new();

        [OneTimeSetUp]
        public virtual void InitSerializer()
        {
            collabSerializerModule = new umi3d.common.collaboration.UMI3DCollaborationSerializerModule();
            serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();

            UMI3DSerializer.AddModule(serializationModules);
        }

        [OneTimeTearDown]
        public virtual void Teardown()
        {
            UMI3DSerializer.RemoveModule(serializationModules);
            serializationModules.Clear();
        }

        #region Pose Conditions
        [Test]
        public void ReadMagnitudeCondition()
        {
            MagnitudeConditionDto magnitudeConditionDto = new MagnitudeConditionDto(
                magnitude: 1220,
                boneOrigine : 12,
                targetObjectId : 24
            );

            collabSerializerModule.Write(magnitudeConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as MagnitudeConditionDto).magnitude == magnitudeConditionDto.magnitude);
        }
        [Test]
        public void ReadBoneRotationCondition()
        {
            BoneRotationConditionDto boneRotationConditionDto = new BoneRotationConditionDto(
                boneId: 8,
                rotation : Vector4Dto.one
            );

            collabSerializerModule.Write(boneRotationConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as BoneRotationConditionDto).boneId == boneRotationConditionDto.boneId);
            Assert.IsTrue((result as BoneRotationConditionDto).rotation.X == boneRotationConditionDto.rotation.X);
            Assert.IsTrue((result as BoneRotationConditionDto).rotation.Y == boneRotationConditionDto.rotation.Y);
            Assert.IsTrue((result as BoneRotationConditionDto).rotation.Z == boneRotationConditionDto.rotation.Z);
            Assert.IsTrue((result as BoneRotationConditionDto).rotation.W == boneRotationConditionDto.rotation.W);
        }
        [Test]
        public void ReadDirectionCondition()
        {
            DirectionConditionDto directionConditionDto = new DirectionConditionDto(
                direction : Vector3.one.Dto()
            );

            collabSerializerModule.Write(directionConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as DirectionConditionDto).direction.X == directionConditionDto.direction.X);
            Assert.IsTrue((result as DirectionConditionDto).direction.Y == directionConditionDto.direction.Y);
            Assert.IsTrue((result as DirectionConditionDto).direction.Z == directionConditionDto.direction.Z);
        }
        [Test]
        public void ReadUserScaleCondition()
        {
            UserScaleConditionDto userScaleConditinoDto = new UserScaleConditionDto(
                scale : Vector3.one.Dto()
            );

            collabSerializerModule.Write(userScaleConditinoDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as UserScaleConditionDto).scale.X == userScaleConditinoDto.scale.X);
        }
        [Test]
        public void ReadScaleCondition()
        {
            ScaleConditionDto scaleConditionDto = new ScaleConditionDto(
                scale: Vector3.one.Dto()
            );

            collabSerializerModule.Write(scaleConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as ScaleConditionDto).scale.X == scaleConditionDto.scale.X);
        }

        #endregion
        #region Multy Conditions
        [Test]
        public void ReadRangeCondition()
        {
            RangeConditionDto rangeConditionDto = new RangeConditionDto(
                conditionA : new MagnitudeConditionDto(12, 53, 15),
                conditionB : new ScaleConditionDto(Vector3Dto.one)
            );

            collabSerializerModule.Write(rangeConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue(((result as RangeConditionDto).conditionA as MagnitudeConditionDto).magnitude
                == (rangeConditionDto.conditionA as MagnitudeConditionDto).magnitude);
            Assert.IsTrue(((result as RangeConditionDto).conditionB as ScaleConditionDto).scale.X
                == (rangeConditionDto.conditionB as ScaleConditionDto).scale.X);
        }

        [Test]
        public void ReadNotCondition()
        {
            NotConditionDto notConditionDto = new NotConditionDto(
                conditions : GetCondditionsTestSet()
            );

            collabSerializerModule.Write(notConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue(((result as NotConditionDto).conditions[0] as UserScaleConditionDto).scale.X
                == (notConditionDto.conditions[0] as UserScaleConditionDto).scale.X);
            Assert.IsTrue(((result as NotConditionDto).conditions[1] as DirectionConditionDto).direction.X
                == (notConditionDto.conditions[1] as DirectionConditionDto).direction.X);
        }

        private PoseConditionDto[] GetCondditionsTestSet()
        {
            return new PoseConditionDto[]{
                new UserScaleConditionDto(Vector3Dto.one),
                new DirectionConditionDto(Vector3Dto.one)
            };
        }
        #endregion

        #region Bone Pose
        [Test]
        public void ReadBonePose()
        {
            BonePoseDto bonePoseDto = new BonePoseDto(
                bone : 2,
                position : Vector3Dto.one,
                rotation : Vector4Dto.one
            ) ;

            collabSerializerModule.Write(bonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue(((result as BonePoseDto).Bone
                == (bonePoseDto as BonePoseDto).Bone));
            Assert.IsTrue(((result as BonePoseDto).Position.Struct()
                 == (bonePoseDto as BonePoseDto).Position.Struct()));
            Assert.IsTrue((result as BonePoseDto).Rotation.Struct()
                == (bonePoseDto as BonePoseDto).Rotation.Struct());
        }

        [Test]
        public void ReadBonePose_AnchoredBonePose()
        {
            AnchoredBonePoseDto anchorBonePoseDto = new AnchoredBonePoseDto(
                bone: 2,
                position: Vector3Dto.one,
                rotation: Vector4Dto.one,
                otherBone : 17
            );

            collabSerializerModule.Write(anchorBonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue(((result as BonePoseDto).Bone
                == (anchorBonePoseDto as BonePoseDto).Bone));
            Assert.IsTrue(((result as BonePoseDto).Position.Struct()
                 == (anchorBonePoseDto as BonePoseDto).Position.Struct()));
            Assert.IsTrue(((result as BonePoseDto).Rotation.Struct()
                == (anchorBonePoseDto as BonePoseDto).Rotation.Struct()));

            Assert.IsTrue(((result as AnchoredBonePoseDto).otherBone
                == (anchorBonePoseDto as AnchoredBonePoseDto).otherBone));
        }

        [Test]
        public void ReadBonePose_NodeAnchoredBonePose()
        {
            NodeAnchoredBonePoseDto nodeAnchoredBonePoseDto = new NodeAnchoredBonePoseDto(
                bone: 2,
                position: Vector3Dto.one,
                rotation: Vector4Dto.one,
                node: 17
            );

            collabSerializerModule.Write(nodeAnchoredBonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue(((result as BonePoseDto).Bone
                == (nodeAnchoredBonePoseDto as BonePoseDto).Bone));
            Assert.IsTrue(((result as BonePoseDto).Position.Struct()
                 == (nodeAnchoredBonePoseDto as BonePoseDto).Position.Struct()));
            Assert.IsTrue(((result as BonePoseDto).Rotation.Struct()
                == (nodeAnchoredBonePoseDto as BonePoseDto).Rotation.Struct()));

            Assert.IsTrue(((result as NodeAnchoredBonePoseDto).node
                == (nodeAnchoredBonePoseDto as NodeAnchoredBonePoseDto).node));
        }

        [Test]
        public void ReadBonePose_FloorAnchoredBonePoseDto()
        {
            FloorAnchoredBonePoseDto floorAnchoredBonePoseDto = new FloorAnchoredBonePoseDto(
                bone: 2,
                position: Vector3Dto.one,
                rotation: Vector4Dto.one
            );

            collabSerializerModule.Write(floorAnchoredBonePoseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue(((result as BonePoseDto).Bone
                == (floorAnchoredBonePoseDto as BonePoseDto).Bone));
            Assert.IsTrue(((result as BonePoseDto).Position.Struct()
                 == (floorAnchoredBonePoseDto as BonePoseDto).Position.Struct()));
            Assert.IsTrue(((result as BonePoseDto).Rotation.Struct()
                == (floorAnchoredBonePoseDto as BonePoseDto).Rotation.Struct()));
        }
        #endregion

        #region Pose
        [Test]
        public void ReadPose()
        {
            PoseDto poseDto = new PoseDto(
                bones: GetTestBonePoseDtoSample(),
                boneAnchor : new BonePoseDto() { Bone = 24, Position = Vector3Dto.zero, Rotation = Vector4Dto.one }
            );

            collabSerializerModule.Write(poseDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out PoseDto result);
            Assert.IsTrue(readable);
            for (int i = 0; i < poseDto.bones.Count; i++)
            {
                Assert.IsTrue((result.bones[i]).boneType == poseDto.bones[i].boneType);
            }

            Assert.IsTrue(((result as PoseDto).boneAnchor.Bone
                == (poseDto as PoseDto).boneAnchor.Bone));
        }

        private List<BoneDto> GetTestBonePoseDtoSample()
        {
            return new List<BoneDto>
            {
                new BoneDto() {boneType = 1, rotation = Vector4Dto.one },
                new BoneDto() {boneType = 15, rotation = Vector4Dto.one }
            };
        }

        [Test]
        public void ReadPoseOverrider()
        {
            int poseIndexinPoseManager = 12 ;

            PoseOverriderDto poseOverriderDto = new PoseOverriderDto(
                poseIndexinPoseManager : poseIndexinPoseManager,
                poseConditionDtos: GetCondditionsTestSet(),
                duration: new DurationDto(24, 222, 13),
                interpolationable: true,
                composable: false
            ); ;

            collabSerializerModule.Write(poseOverriderDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out PoseOverriderDto result);
            Assert.IsTrue(readable);

            int poseIndex = result.poseIndexinPoseManager;

            Assert.IsTrue(poseIndex == poseIndexinPoseManager);
            Assert.IsTrue((result.poseConditions[0] as UserScaleConditionDto).scale.Struct()
                == (poseOverriderDto.poseConditions[0] as UserScaleConditionDto).scale.Struct());
            Assert.IsTrue((result.poseConditions[1] as DirectionConditionDto).direction.Struct()
                == (poseOverriderDto.poseConditions[1] as DirectionConditionDto).direction.Struct());

            Assert.IsTrue(poseOverriderDto.duration.duration == result.duration.duration);
            Assert.IsTrue(poseOverriderDto.interpolationable == result.interpolationable);
            Assert.IsTrue(poseOverriderDto.composable == result.composable);
        }

        #endregion

        #region Duration
        [Test]
        public void ReadDuration()
        {
            DurationDto duration = new DurationDto(
                duration : 159,
                min : 5,
                max : 89486
            );

            collabSerializerModule.Write(duration, out Bytable data);


            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            collabSerializerModule.Read(byteContainer, out bool readable, out DurationDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue(duration.duration == result.duration);    
            Assert.IsTrue(duration.max == result.max);
            Assert.IsTrue(duration.min == result.min);
        }
        #endregion
    }
}

