﻿/*
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
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Pose.Common
{
    public class PoseConditionSerializerModule_Test
    {
        private PoseConditionSerializerModule poseConditionSerializerModule = null;
        protected List<UMI3DSerializerModule> serializationModules = new();

        #region Test SetUp

        [OneTimeSetUp]
        public virtual void InitSerializer()
        {
            poseConditionSerializerModule = new PoseConditionSerializerModule();
            serializationModules = UMI3DSerializerModuleUtils.GetModules().ToList();

            UMI3DSerializer.AddModule(serializationModules);
        }

        [OneTimeTearDown]
        public virtual void Teardown()
        {
            UMI3DSerializer.RemoveModule(serializationModules);
            serializationModules.Clear();
        }

        #endregion Test SetUp

        #region Pose Conditions

        [Test]
        public void ReadMagnitudeCondition()
        {
            MagnitudeConditionDto magnitudeConditionDto = new MagnitudeConditionDto(
                magnitude: 1220,
                boneOrigine: 12,
                targetObjectId: 24
            );

            poseConditionSerializerModule.Write(magnitudeConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as MagnitudeConditionDto).Magnitude == magnitudeConditionDto.Magnitude);
        }

        [Test]
        public void ReadBoneRotationCondition()
        {
            BoneRotationConditionDto boneRotationConditionDto = new BoneRotationConditionDto(
                boneId: 8,
                rotation: Vector4Dto.one
            );

            poseConditionSerializerModule.Write(boneRotationConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as BoneRotationConditionDto).BoneId == boneRotationConditionDto.BoneId);
            Assert.IsTrue((result as BoneRotationConditionDto).Rotation.X == boneRotationConditionDto.Rotation.X);
            Assert.IsTrue((result as BoneRotationConditionDto).Rotation.Y == boneRotationConditionDto.Rotation.Y);
            Assert.IsTrue((result as BoneRotationConditionDto).Rotation.Z == boneRotationConditionDto.Rotation.Z);
            Assert.IsTrue((result as BoneRotationConditionDto).Rotation.W == boneRotationConditionDto.Rotation.W);
        }

        [Test]
        public void ReadDirectionCondition()
        {
            DirectionConditionDto directionConditionDto = new DirectionConditionDto(
                direction: Vector3.one.Dto()
            );

            poseConditionSerializerModule.Write(directionConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as DirectionConditionDto).Direction.X == directionConditionDto.Direction.X);
            Assert.IsTrue((result as DirectionConditionDto).Direction.Y == directionConditionDto.Direction.Y);
            Assert.IsTrue((result as DirectionConditionDto).Direction.Z == directionConditionDto.Direction.Z);
        }

        [Test]
        public void ReadUserScaleCondition()
        {
            UserScaleConditionDto userScaleConditinoDto = new UserScaleConditionDto(
                scale: Vector3.one.Dto()
            );

            poseConditionSerializerModule.Write(userScaleConditinoDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as UserScaleConditionDto).Scale.X == userScaleConditinoDto.Scale.X);
        }

        [Test]
        public void ReadScaleCondition()
        {
            ScaleConditionDto scaleConditionDto = new ScaleConditionDto(
                scale: Vector3.one.Dto()
            );

            poseConditionSerializerModule.Write(scaleConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as ScaleConditionDto).Scale.X == scaleConditionDto.Scale.X);
        }

        #endregion Pose Conditions

        #region Multy Conditions

        [Test]
        public void ReadRangeCondition()
        {
            RangeConditionDto rangeConditionDto = new RangeConditionDto(
                conditionA: new MagnitudeConditionDto(12, 53, 15),
                conditionB: new ScaleConditionDto(Vector3Dto.one)
            );

            poseConditionSerializerModule.Write(rangeConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue(((result as RangeConditionDto).ConditionA as MagnitudeConditionDto).Magnitude
                == (rangeConditionDto.ConditionA as MagnitudeConditionDto).Magnitude);
            Assert.IsTrue(((result as RangeConditionDto).ConditionB as ScaleConditionDto).Scale.X
                == (rangeConditionDto.ConditionB as ScaleConditionDto).Scale.X);
        }

        [Test]
        public void ReadNotCondition()
        {
            NotConditionDto notConditionDto = new NotConditionDto(
                conditions: GetCondditionsTestSet()
            );

            poseConditionSerializerModule.Write(notConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue(((result as NotConditionDto).Conditions[0] as UserScaleConditionDto).Scale.X
                == (notConditionDto.Conditions[0] as UserScaleConditionDto).Scale.X);
            Assert.IsTrue(((result as NotConditionDto).Conditions[1] as DirectionConditionDto).Direction.X
                == (notConditionDto.Conditions[1] as DirectionConditionDto).Direction.X);
        }

        private PoseConditionDto[] GetCondditionsTestSet()
        {
            return new PoseConditionDto[]{
                new UserScaleConditionDto(Vector3Dto.one),
                new DirectionConditionDto(Vector3Dto.one)
            };
        }

        #endregion Multy Conditions
    }
}