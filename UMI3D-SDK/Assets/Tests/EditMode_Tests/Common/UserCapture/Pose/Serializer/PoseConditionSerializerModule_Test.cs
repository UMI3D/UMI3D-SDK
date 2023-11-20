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
using umi3d.common.userCapture.pose;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Pose.Common
{
    [TestFixture, TestOf(nameof(PoseConditionSerializerModule))]
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
        public void Read_MagnitudeCondition()
        {
            MagnitudeConditionDto magnitudeConditionDto = new MagnitudeConditionDto()
            {
                Magnitude = 1220,
                BoneOrigin = 12,
                TargetNodeId = 24
            };

            poseConditionSerializerModule.Write(magnitudeConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out AbstractPoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as MagnitudeConditionDto).Magnitude == magnitudeConditionDto.Magnitude);
        }

        [Test]
        public void Read_BoneRotationCondition()
        {
            BoneRotationConditionDto boneRotationConditionDto = new BoneRotationConditionDto()
            {
                BoneId = 8,
                Rotation = Vector4.one.Dto(),
                Threshold = 1f
            };

            poseConditionSerializerModule.Write(boneRotationConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out AbstractPoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as BoneRotationConditionDto).BoneId == boneRotationConditionDto.BoneId);
            Assert.IsTrue((result as BoneRotationConditionDto).Rotation.X == boneRotationConditionDto.Rotation.X);
            Assert.IsTrue((result as BoneRotationConditionDto).Rotation.Y == boneRotationConditionDto.Rotation.Y);
            Assert.IsTrue((result as BoneRotationConditionDto).Rotation.Z == boneRotationConditionDto.Rotation.Z);
            Assert.IsTrue((result as BoneRotationConditionDto).Rotation.W == boneRotationConditionDto.Rotation.W);
        }

        [Test]
        public void Read_DirectionCondition()
        {
            DirectionConditionDto directionConditionDto = new DirectionConditionDto()
            {
                Direction = Vector3.one.Dto(),
            };

            poseConditionSerializerModule.Write(directionConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out AbstractPoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as DirectionConditionDto).Direction.X == directionConditionDto.Direction.X);
            Assert.IsTrue((result as DirectionConditionDto).Direction.Y == directionConditionDto.Direction.Y);
            Assert.IsTrue((result as DirectionConditionDto).Direction.Z == directionConditionDto.Direction.Z);
        }

        [Test]
        public void Read_ScaleCondition()
        {
            ScaleConditionDto scaleConditionDto = new ScaleConditionDto()
            {
                Scale = Vector3.one.Dto()
            };

            poseConditionSerializerModule.Write(scaleConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out AbstractPoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue((result as ScaleConditionDto).Scale.X == scaleConditionDto.Scale.X);
            Assert.IsTrue((result as ScaleConditionDto).Scale.Y == scaleConditionDto.Scale.Y);
            Assert.IsTrue((result as ScaleConditionDto).Scale.Z == scaleConditionDto.Scale.Z);
        }

        [Test]
        public void Read_EnvironmentPoseCondition()
        {
            EnvironmentPoseConditionDto environmentConditionDto = new ()
            {
                Id = 10005uL,
                IsValidated = true,
            };

            poseConditionSerializerModule.Write(environmentConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out AbstractPoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.AreEqual(environmentConditionDto.Id, (result as EnvironmentPoseConditionDto).Id);
            Assert.AreEqual(environmentConditionDto.IsValidated, (result as EnvironmentPoseConditionDto).IsValidated);
        }

        #endregion Pose Conditions

        #region Multy Conditions

        [Test]
        public void Read_OrCondition()
        {
            OrConditionDto orConditionDto = new OrConditionDto()
            {
                ConditionsA = new AbstractPoseConditionDto[] { new MagnitudeConditionDto() { Magnitude = 12, BoneOrigin = 53, TargetNodeId = 15 } },
                ConditionsB = new AbstractPoseConditionDto[] { new ScaleConditionDto() { Scale = Vector3.one.Dto() } }
            };

            poseConditionSerializerModule.Write(orConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out AbstractPoseConditionDto result);
            Assert.IsTrue(readable);
            Assert.IsTrue(((result as OrConditionDto).ConditionsA[0] as MagnitudeConditionDto).Magnitude
                == (orConditionDto.ConditionsA[0] as MagnitudeConditionDto).Magnitude);
            Assert.IsTrue(((result as OrConditionDto).ConditionsB[0] as ScaleConditionDto).Scale.X
                == (orConditionDto.ConditionsB[0] as ScaleConditionDto).Scale.X);
        }

        [Test]
        public void Read_NotCondition()
        {
            NotConditionDto notConditionDto = new NotConditionDto()
            {
                Conditions = GetConditionsTestSet()
            };

            poseConditionSerializerModule.Write(notConditionDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

            poseConditionSerializerModule.Read(byteContainer, out bool readable, out AbstractPoseConditionDto result);
            Assert.IsTrue(readable);

            Assert.IsTrue(((result as NotConditionDto).Conditions[0] as ScaleConditionDto).Scale.X
                == (notConditionDto.Conditions[0] as ScaleConditionDto).Scale.X);
            Assert.IsTrue(((result as NotConditionDto).Conditions[1] as DirectionConditionDto).Direction.X
                == (notConditionDto.Conditions[1] as DirectionConditionDto).Direction.X);
        }

        private AbstractPoseConditionDto[] GetConditionsTestSet()
        {
            return new AbstractPoseConditionDto[]{
                new ScaleConditionDto() { Scale = Vector3.one.Dto() },
                new DirectionConditionDto() { Direction = Vector3.one.Dto() }
            };
        }

        #endregion Multy Conditions
    }
}