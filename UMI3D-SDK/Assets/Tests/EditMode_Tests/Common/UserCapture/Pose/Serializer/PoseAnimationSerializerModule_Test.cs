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
using UnityEngine;

namespace EditMode_Tests.UserCapture.Pose.Common
{
    [TestFixture, TestOf(nameof(PoseAnimationSerializerModule))]
    public class PoseAnimationSerializerModule_Test
    {
        private PoseAnimationSerializerModule poseSerializerModule;
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

        #region Read Pose Clip

        [Test]
        public void Read_PoseClipDto()
        {
            // GIVEN
            PoseDto poseDto = new PoseDto()
            {
                bones = new List<BoneDto>
            {
                new BoneDto() {boneType = 1u, rotation = Vector4Dto.one },
                new BoneDto() {boneType = 15u, rotation = Vector4Dto.one }
            },
                anchor = new PoseAnchorDto() { bone = 24u, position = Vector3Dto.zero, rotation = Vector4Dto.one }
            };
            PoseClipDto poseClipDto = new PoseClipDto()
            {
                id = 1005uL,
                pose = poseDto
            };

            poseSerializerModule.Write(poseClipDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            // WHEN
            poseSerializerModule.Read(byteContainer, out bool readable, out PoseClipDto result);

            // THEN
            Assert.IsTrue(readable);
            Assert.AreEqual(poseClipDto.id, result.id);
            for (int i = 0; i < poseClipDto.pose.bones.Count; i++)
            {
                Assert.AreEqual(poseClipDto.pose.bones[i].boneType, (result.pose.bones[i]).boneType);
            }

            Assert.AreEqual(poseClipDto.pose.anchor.bone, result.pose.anchor.bone);
        }

        #endregion Read Pose Clip

        #region Read Pose Animator

        [Test]
        public void Read_PoseAnimatorDto()
        {
            PoseAnimatorDto poseAnimatorDto = new PoseAnimatorDto()
            {
                id = 10005uL,
                relatedNodeId = 1006uL,
                poseClipId = 11025uL,
                boneConstraintId = 15268uL,
                poseConditions = new AbstractPoseConditionDto[]
                {
                    new ScaleConditionDto() { Scale = Vector3.one.Dto() },
                    new DirectionConditionDto() { Direction = Vector3.one.Dto() },
                },
                duration = new DurationDto() { duration = 24, max = 12, min = 2 },
                activationMode = 0,
            };

            poseSerializerModule.Write(poseAnimatorDto, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            poseSerializerModule.Read(byteContainer, out bool readable, out PoseAnimatorDto result);
            Assert.IsTrue(readable);

            Assert.AreEqual(poseAnimatorDto.id, result.id);
            Assert.AreEqual(poseAnimatorDto.poseClipId, result.poseClipId);
            Assert.AreEqual(poseAnimatorDto.boneConstraintId, result.boneConstraintId);
            Assert.AreEqual(poseAnimatorDto.relatedNodeId, result.relatedNodeId);

            Assert.AreEqual(poseAnimatorDto.duration.duration, result.duration.duration);

            Assert.AreEqual(poseAnimatorDto.activationMode, result.activationMode);

            Assert.AreEqual(poseAnimatorDto.poseConditions.Length, result.poseConditions.Length);
        }

        #endregion Read Pose Animator

        #region Read Duration

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

        #endregion Read Duration

        #region Read Pose Request

        [Test]
        public void ReadPoseRequest()
        {
            // given
            PlayPoseClipDto playPoseClipRequest = new()
            {
                poseId = 1369uL,
                stopPose = true,
            };

            poseSerializerModule.Write(playPoseClipRequest, out Bytable data);

            ByteContainer byteContainer = new ByteContainer(0, 1, data.ToBytes());

            // when
            UMI3DSerializer.TryRead<uint>(byteContainer, out uint operationKey);
            poseSerializerModule.Read(byteContainer, out bool readable, out PlayPoseClipDto result);
            // then
            Assert.IsTrue(readable);

            Assert.AreEqual(UMI3DOperationKeys.PlayPoseRequest, operationKey);
            Assert.AreEqual(playPoseClipRequest.poseId, result.poseId);
            Assert.AreEqual(playPoseClipRequest.stopPose, result.stopPose);
        }

        #endregion Read Pose Request

    }
}