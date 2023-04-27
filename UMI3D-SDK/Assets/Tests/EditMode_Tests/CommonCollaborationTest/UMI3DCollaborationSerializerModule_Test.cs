///*
//Copyright 2019 - 2023 Inetum

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
//*/

//using Moq;
//using NUnit.Framework;
//using System.Collections.Generic;
//using System.Linq;
//using umi3d.cdk;
//using umi3d.cdk.collaboration;
//using umi3d.common.collaboration;
//using umi3d.cdk.userCapture;
//using umi3d.common;
//using umi3d.common.userCapture;
//using UnityEngine;
//using System;
//using UnityEditor;

//namespace EditMode_Tests
//{
//    public class UMI3DCollaborationSerializerModule_Test
//    {
//        umi3d.common.collaboration.UMI3DCollaborationSerializerModule collabSerializerModule = null;
//        UMI3DSerializerBasicModules basicModules = null;
//        UMI3DSerializerVectorModules vectorModules = null;
//        UMI3DSerializerStringModules stringModules = null;
//        UMI3DSerializerShaderModules shaderModules = null;
//        UMI3DSerializerAnimationModules animationModules = null;

//        [OneTimeSetUp] public void InitSerializer()
//        {
//            collabSerializerModule = new umi3d.common.collaboration.UMI3DCollaborationSerializerModule();
//            basicModules = new UMI3DSerializerBasicModules();
//            vectorModules= new UMI3DSerializerVectorModules();
//            stringModules = new UMI3DSerializerStringModules();
//            shaderModules= new UMI3DSerializerShaderModules();
//            animationModules= new UMI3DSerializerAnimationModules();

//            UMI3DSerializer.AddModule(collabSerializerModule);
//            UMI3DSerializer.AddModule(basicModules);
//            UMI3DSerializer.AddModule(vectorModules);
//            UMI3DSerializer.AddModule(stringModules);
//            UMI3DSerializer.AddModule(shaderModules);
//            UMI3DSerializer.AddModule(animationModules);
//        }

//        [TearDown] public void Teardown()
//        {

//        }

//        #region Bindings
//        [Test] public void ReadBindingDTO_BindingDTOData()
//        {
//            BindingDataDto bindingDataDto = new BindingDataDto(
//                priority : 10,
//                partialFit : true
//            );

//            BindingDto bindingDto = new BindingDto(
//                objectId : 123,
//                active : true,
//                data : bindingDataDto
//            );

//            collabSerializerModule.Write<BindingDto>(bindingDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes()) ;

//            collabSerializerModule.Read(byteContainer, out bool readable, out BindingDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue(result.active == bindingDto.active);
//            Assert.IsTrue(result.bindingId == bindingDto.bindingId);
//            Assert.IsTrue(result.data.priority == bindingDto.data.priority);
//            Assert.IsTrue(result.data.partialFit == bindingDto.data.partialFit);
//        }

//        [Test]
//        public void ReadBindingDTO_SimpleBindingDto()
//        {
//            SimpleBindingDto simpleBindingDto = new SimpleBindingDto(
//                priority: 10,
//                partialFit: true,
//                syncRotation : true,
//                syncPosition : true,
//                syncScale : true,
//                offSetPosition : Vector3.one,
//                offSetRotation : Vector4.one,
//                offSetScale : Vector3.left
//            ) ;

//            BindingDto bindingDto = new BindingDto(
//                objectId: 123,
//                active: true,
//                data: simpleBindingDto
//            );

//            collabSerializerModule.Write<BindingDto>(bindingDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out BindingDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue(result.active == bindingDto.active);
//            Assert.IsTrue(result.bindingId == bindingDto.bindingId);
//            Assert.IsTrue(result.data.priority == bindingDto.data.priority);
//            Assert.IsTrue(result.data.partialFit == bindingDto.data.partialFit);

//            Assert.IsTrue((result.data as SimpleBindingDto).offSetRotation
//                == (bindingDto.data as SimpleBindingDto).offSetRotation);
//            Assert.IsTrue((result.data as SimpleBindingDto).offSetPosition
//                == (bindingDto.data as SimpleBindingDto).offSetPosition);
//            Assert.IsTrue((result.data as SimpleBindingDto).offSetScale
//                == (bindingDto.data as SimpleBindingDto).offSetScale);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncScale
//                == (bindingDto.data as SimpleBindingDto).syncScale);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncPosition
//                == (bindingDto.data as SimpleBindingDto).syncPosition);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncRotation
//                == (bindingDto.data as SimpleBindingDto).syncRotation);
//        }

//        [Test]
//        public void ReadBindingDTO_NodeBindingDTO()
//        {
//            NodeBindingDto nodeBindingDto = new NodeBindingDto(
//                priority: 10,
//                partialFit: true,
//                syncRotation: true,
//                syncPosition: true,
//                syncScale: true,
//                offSetPosition: Vector3.one,
//                offSetRotation: Vector4.one,
//                offSetScale: Vector3.left,
//                objectID : 1523
//            );

//            BindingDto bindingDto = new BindingDto(
//                objectId: 123,
//                active: true,
//                data: nodeBindingDto
//            );

//            collabSerializerModule.Write<BindingDto>(bindingDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out BindingDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue(result.active == bindingDto.active);
//            Assert.IsTrue(result.bindingId == bindingDto.bindingId);
//            Assert.IsTrue(result.data.priority == bindingDto.data.priority);
//            Assert.IsTrue(result.data.partialFit == bindingDto.data.partialFit);

//            Assert.IsTrue((result.data as SimpleBindingDto).offSetRotation
//                == (bindingDto.data as SimpleBindingDto).offSetRotation);
//            Assert.IsTrue((result.data as SimpleBindingDto).offSetPosition
//                == (bindingDto.data as SimpleBindingDto).offSetPosition);
//            Assert.IsTrue((result.data as SimpleBindingDto).offSetScale
//                == (bindingDto.data as SimpleBindingDto).offSetScale);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncScale
//                == (bindingDto.data as SimpleBindingDto).syncScale);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncPosition
//                == (bindingDto.data as SimpleBindingDto).syncPosition);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncRotation
//                == (bindingDto.data as SimpleBindingDto).syncRotation);

//            Assert.IsTrue((result.data as NodeBindingDto).objectId
//                == (bindingDto.data as NodeBindingDto).objectId);
//        }

//        [Test]
//        public void ReadBindingDTO_SimpleBoneBindingDTO()
//        {
//            SimpleBoneBindingDto simpleBoneBindingDto = new SimpleBoneBindingDto(
//                priority: 10,
//                partialFit: true,
//                syncRotation: true,
//                syncPosition: true,
//                syncScale: true,
//                offSetPosition: Vector3.one,
//                offSetRotation: Vector4.one,
//                offSetScale: Vector3.left,
//                userId : 1,
//                boneType : 15            
//            );

//            BindingDto bindingDto = new BindingDto(
//                objectId: 123,
//                active: true,
//                data: simpleBoneBindingDto
//            );

//            collabSerializerModule.Write<BindingDto>(bindingDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out BindingDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue(result.active == bindingDto.active);
//            Assert.IsTrue(result.bindingId == bindingDto.bindingId);
//            Assert.IsTrue(result.data.priority == bindingDto.data.priority);
//            Assert.IsTrue(result.data.partialFit == bindingDto.data.partialFit);

//            Assert.IsTrue((result.data as SimpleBindingDto).offSetRotation
//                == (bindingDto.data as SimpleBindingDto).offSetRotation);
//            Assert.IsTrue((result.data as SimpleBindingDto).offSetPosition
//                == (bindingDto.data as SimpleBindingDto).offSetPosition);
//            Assert.IsTrue((result.data as SimpleBindingDto).offSetScale
//                == (bindingDto.data as SimpleBindingDto).offSetScale);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncScale
//                == (bindingDto.data as SimpleBindingDto).syncScale);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncPosition
//                == (bindingDto.data as SimpleBindingDto).syncPosition);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncRotation
//                == (bindingDto.data as SimpleBindingDto).syncRotation);

//            Assert.IsTrue((result.data as SimpleBoneBindingDto).userId
//                == (bindingDto.data as SimpleBoneBindingDto).userId);
//            Assert.IsTrue((result.data as SimpleBoneBindingDto).boneType
//                == (bindingDto.data as SimpleBoneBindingDto).boneType);
//        }

//        [Test]
//        public void ReadBindingDTO_RigBindingDataDto()
//        {
//            RigBindingDataDto rigBindingDataDto = new RigBindingDataDto(
//                priority: 10,
//                partialFit: true,
//                syncRotation: true,
//                syncPosition: true,
//                syncScale: true,
//                offSetPosition: Vector3.one,
//                offSetRotation: Vector4.one,
//                offSetScale: Vector3.left,
//                userId: 1,
//                boneType: 15,
//                rigName : "Platipus"
//            );

//            BindingDto bindingDto = new BindingDto(
//                objectId: 123,
//                active: true,
//                data: rigBindingDataDto
//            );

//            collabSerializerModule.Write<BindingDto>(bindingDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out BindingDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue(result.active == bindingDto.active);
//            Assert.IsTrue(result.bindingId == bindingDto.bindingId);
//            Assert.IsTrue(result.data.priority == bindingDto.data.priority);
//            Assert.IsTrue(result.data.partialFit == bindingDto.data.partialFit);

//            Assert.IsTrue((result.data as SimpleBindingDto).offSetRotation
//                == (bindingDto.data as SimpleBindingDto).offSetRotation);
//            Assert.IsTrue((result.data as SimpleBindingDto).offSetPosition
//                == (bindingDto.data as SimpleBindingDto).offSetPosition);
//            Assert.IsTrue((result.data as SimpleBindingDto).offSetScale
//                == (bindingDto.data as SimpleBindingDto).offSetScale);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncScale
//                == (bindingDto.data as SimpleBindingDto).syncScale);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncPosition
//                == (bindingDto.data as SimpleBindingDto).syncPosition);
//            Assert.IsTrue((result.data as SimpleBindingDto).syncRotation
//                == (bindingDto.data as SimpleBindingDto).syncRotation);

//            Assert.IsTrue((result.data as SimpleBoneBindingDto).userId
//                == (bindingDto.data as SimpleBoneBindingDto).userId);
//            Assert.IsTrue((result.data as SimpleBoneBindingDto).boneType
//                == (bindingDto.data as SimpleBoneBindingDto).boneType);

//            Assert.IsTrue((result.data as RigBindingDataDto).rigName
//                 == (bindingDto.data as RigBindingDataDto).rigName);
//        }
//        #endregion

//        #region Multi binding
//        [Test]
//        public void ReadBindingDTO_MultiBinding()
//        {
//            MultiBindingDto multyBindingDto = new MultiBindingDto(
//                priority: 10,
//                partialFit: true,
//                Bindings :  GetTestBindingsArray()         
//            );

//            BindingDto bindingDto = new BindingDto(
//                objectId: 123,
//                active: true,
//                data: multyBindingDto
//            );

//            collabSerializerModule.Write<BindingDto>(bindingDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out BindingDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue(result.active == bindingDto.active);
//            Assert.IsTrue(result.bindingId == bindingDto.bindingId);
//            Assert.IsTrue(result.data.priority == bindingDto.data.priority);
//            Assert.IsTrue(result.data.partialFit == bindingDto.data.partialFit);

//            MultiBindingDto multiRes = result.data as MultiBindingDto;
//            for (int i = 0; i < multiRes.Bindings.Length; i++)
//            {
//                Assert.IsTrue(multiRes.Bindings[i].priority == multyBindingDto.Bindings[i].priority);
//            }
//        }

//        private BindingDataDto[] GetTestBindingsArray()
//        {
//            return new BindingDataDto[] {
//                new BindingDataDto(3, true),
//                new BindingDataDto(4, true),
//                new BindingDataDto(9, true),
//                new BindingDataDto(4, true),
//            };
//        }
//        #endregion

//        #region Pose Conditions
//        [Test]
//        public void ReadMagnitudeCondition()
//        {
//            MagnitudeConditionDto magnitudeConditionDto = new MagnitudeConditionDto(
//                magnitude: 1220
//            );

//            collabSerializerModule.Write(magnitudeConditionDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue((result as MagnitudeConditionDto).magnitude == magnitudeConditionDto.magnitude);
//        }
//        [Test]
//        public void ReadBoneRotationCondition()
//        {
//            BoneRotationConditionDto boneRotationConditionDto = new BoneRotationConditionDto(
//                boneId: 8,
//                rotation : Vector4.one
//            );

//            collabSerializerModule.Write(boneRotationConditionDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue((result as BoneRotationConditionDto).boneId == boneRotationConditionDto.boneId);
//            Assert.IsTrue((result as BoneRotationConditionDto).rotation == boneRotationConditionDto.rotation);
//        }
//        [Test]
//        public void ReadDirectionCondition()
//        {
//            DirectionConditionDto directionConditionDto = new DirectionConditionDto(
//                direction : Vector3.one
//            );

//            collabSerializerModule.Write(directionConditionDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue((result as DirectionConditionDto).direction == directionConditionDto.direction);
//        }
//        [Test]
//        public void ReadUserScaleCondition()
//        {
//            UserScaleConditinoDto userScaleConditinoDto = new UserScaleConditinoDto(
//                scale : Vector3.one
//            );

//            collabSerializerModule.Write(userScaleConditinoDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue((result as UserScaleConditinoDto).scale == userScaleConditinoDto.scale);
//        }
//        [Test]
//        public void ReadScaleCondition()
//        {
//            ScaleConditionDto scaleConditionDto = new ScaleConditionDto(
//                scale: Vector3.one
//            );

//            collabSerializerModule.Write(scaleConditionDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue((result as ScaleConditionDto).scale == scaleConditionDto.scale);
//        }

//        #endregion
//        #region Multy Conditions
//        [Test]
//        public void ReadRangeCondition()
//        {
//            RangeConditionDto rangeConditionDto = new RangeConditionDto(
//                conditionA : new MagnitudeConditionDto(12),
//                conditionB : new ScaleConditionDto(Vector3.one)
//            );

//            collabSerializerModule.Write(rangeConditionDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
//            Assert.IsTrue(readable);
//            Assert.IsTrue(((result as RangeConditionDto).conditionA as MagnitudeConditionDto).magnitude
//                == (rangeConditionDto.conditionA as MagnitudeConditionDto).magnitude);
//            Assert.IsTrue(((result as RangeConditionDto).conditionB as ScaleConditionDto).scale
//                == (rangeConditionDto.conditionB as ScaleConditionDto).scale);
//        }

//        [Test]
//        public void ReadNotCondition()
//        {
//            NotConditionDto notConditionDto = new NotConditionDto(
//                conditions : GetCondditionsTestSet()
//            );

//            collabSerializerModule.Write(notConditionDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out PoseConditionDto result);
//            Assert.IsTrue(readable);

//            Assert.IsTrue(((result as NotConditionDto).conditions[0] as UserScaleConditinoDto).scale
//                == (notConditionDto.conditions[0] as UserScaleConditinoDto).scale);
//            Assert.IsTrue(((result as NotConditionDto).conditions[1] as DirectionConditionDto).direction
//                == (notConditionDto.conditions[1] as DirectionConditionDto).direction);
//        }

//        private PoseConditionDto[] GetCondditionsTestSet()
//        {
//            return new PoseConditionDto[]{
//                new UserScaleConditinoDto(Vector3.one),
//                new DirectionConditionDto(Vector3.one)
//            };
//        }
//        #endregion

//        #region Bone Pose
//        [Test]
//        public void ReadBonePose()
//        {
//            BonePoseDto bonePoseDto = new BonePoseDto(
//                bone : 2,
//                position : Vector3.one,
//                rotation : Vector4.one
//            ) ;

//            collabSerializerModule.Write(bonePoseDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
//            Assert.IsTrue(readable);

//            Assert.IsTrue(((result as BonePoseDto).bone
//                == (bonePoseDto as BonePoseDto).bone));
//            Assert.IsTrue(((result as BonePoseDto).position
//                 == (bonePoseDto as BonePoseDto).position));
//            Assert.IsTrue(((result as BonePoseDto).rotation
//                == (bonePoseDto as BonePoseDto).rotation));
//        }

//        [Test]
//        public void ReadBonePose_AnchoredBonePose()
//        {
//            AnchorBonePoseDto anchorBonePoseDto = new AnchorBonePoseDto(
//                bone: 2,
//                position: Vector3.one,
//                rotation: Vector4.one,
//                otherBone : 17
//            );

//            collabSerializerModule.Write(anchorBonePoseDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
//            Assert.IsTrue(readable);

//            Assert.IsTrue(((result as BonePoseDto).bone
//                == (anchorBonePoseDto as BonePoseDto).bone));
//            Assert.IsTrue(((result as BonePoseDto).position
//                 == (anchorBonePoseDto as BonePoseDto).position));
//            Assert.IsTrue(((result as BonePoseDto).rotation
//                == (anchorBonePoseDto as BonePoseDto).rotation));

//            Assert.IsTrue(((result as AnchorBonePoseDto).otherBone
//                == (anchorBonePoseDto as AnchorBonePoseDto).otherBone));
//        }

//        [Test]
//        public void ReadBonePose_NodePositionAnchoredBonePose()
//        {
//            NodePositionAnchoredBonePoseDto nodePositionAnchoredBonePoseDto = new NodePositionAnchoredBonePoseDto(
//                bone: 2,
//                position: Vector3.one,
//                rotation: Vector4.one,
//                node: 17
//            );

//            collabSerializerModule.Write(nodePositionAnchoredBonePoseDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
//            Assert.IsTrue(readable);

//            Assert.IsTrue(((result as BonePoseDto).bone
//                == (nodePositionAnchoredBonePoseDto as BonePoseDto).bone));
//            Assert.IsTrue(((result as BonePoseDto).position
//                 == (nodePositionAnchoredBonePoseDto as BonePoseDto).position));
//            Assert.IsTrue(((result as BonePoseDto).rotation
//                == (nodePositionAnchoredBonePoseDto as BonePoseDto).rotation));

//            Assert.IsTrue(((result as NodePositionAnchoredBonePoseDto).node
//                == (nodePositionAnchoredBonePoseDto as NodePositionAnchoredBonePoseDto).node));
//        }

//        [Test]
//        public void ReadBonePose_NodeRotationAnchoredBonePose()
//        {
//            NodeRotationAnchoredBonePoseDto nodeRotationAnchoredBonePoseDto = new NodeRotationAnchoredBonePoseDto(
//                bone: 2,
//                position: Vector3.one,
//                rotation: Vector4.one,
//                node: 17
//            );

//            collabSerializerModule.Write(nodeRotationAnchoredBonePoseDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
//            Assert.IsTrue(readable);

//            Assert.IsTrue(((result as BonePoseDto).bone
//                == (nodeRotationAnchoredBonePoseDto as BonePoseDto).bone));
//            Assert.IsTrue(((result as BonePoseDto).position
//                 == (nodeRotationAnchoredBonePoseDto as BonePoseDto).position));
//            Assert.IsTrue(((result as BonePoseDto).rotation
//                == (nodeRotationAnchoredBonePoseDto as BonePoseDto).rotation));

//            Assert.IsTrue(((result as NodeRotationAnchoredBonePoseDto).node
//                == (nodeRotationAnchoredBonePoseDto as NodeRotationAnchoredBonePoseDto).node));
//        }

//        [Test]
//        public void ReadBonePose_FloorAnchoredBonePoseDto()
//        {
//            FloorAnchoredBonePoseDto floorAnchoredBonePoseDto = new FloorAnchoredBonePoseDto(
//                bone: 2,
//                position: Vector3.one,
//                rotation: Vector4.one
//            );

//            collabSerializerModule.Write(floorAnchoredBonePoseDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out BonePoseDto result);
//            Assert.IsTrue(readable);

//            Assert.IsTrue(((result as BonePoseDto).bone
//                == (floorAnchoredBonePoseDto as BonePoseDto).bone));
//            Assert.IsTrue(((result as BonePoseDto).position
//                 == (floorAnchoredBonePoseDto as BonePoseDto).position));
//            Assert.IsTrue(((result as BonePoseDto).rotation
//                == (floorAnchoredBonePoseDto as BonePoseDto).rotation));
//        }
//        #endregion

//        #region Pose
//        [Test]
//        public void ReadPose()
//        {
//            PoseDto poseDto = new PoseDto(
//                bones: GetTestBonePoseDtoSample(),
//                boneAnchor : 24
//            );

//            collabSerializerModule.Write(poseDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out PoseDto result);
//            Assert.IsTrue(readable);
//            for (int i = 0; i < poseDto.bones.Length; i++)
//            {
//                Assert.IsTrue((result.bones[i]).bone == poseDto.bones[i].bone);
//            }

//            Assert.IsTrue(((result as PoseDto).boneAnchor
//                == (poseDto as PoseDto).boneAnchor));
//        }

//        private BonePoseDto[] GetTestBonePoseDtoSample()
//        {
//            return new BonePoseDto[]
//            {
//                new BonePoseDto(1, Vector3.one, Vector4.one),
//                new BonePoseDto(15, Vector3.one, Vector4.one)
//            };
//        }

//        [Test]
//        public void ReadPoseOverrider()
//        {
//            PoseOverriderDto poseOverriderDto = new PoseOverriderDto(
//                pose : new PoseDto(
//                        bones: GetTestBonePoseDtoSample(),
//                        boneAnchor: 24
//                        ),
//                poseConditionDtos : GetCondditionsTestSet(),
//                duration : new DurationDto(24,222,13),
//                interpolationable : true,
//                composable : false
//            );;

//            collabSerializerModule.Write(poseOverriderDto, out Bytable data);

//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out PoseOverriderDto result);
//            Assert.IsTrue(readable);

//            PoseDto poseDto = result.pose;
//            for (int i = 0; i < poseDto.bones.Length; i++)
//            {
//                Assert.IsTrue((poseDto.bones[i]).bone == poseOverriderDto.pose.bones[i].bone);
//            }
//            Assert.IsTrue(((poseDto).boneAnchor == poseOverriderDto.pose.boneAnchor));

//            Assert.IsTrue((result.poseConditions[0] as UserScaleConditinoDto).scale
//                == (poseOverriderDto.poseConditions[0] as UserScaleConditinoDto).scale);
//            Assert.IsTrue((result.poseConditions[1] as DirectionConditionDto).direction
//                == (poseOverriderDto.poseConditions[1] as DirectionConditionDto).direction);

//            Assert.IsTrue(poseOverriderDto.duration.duration == result.duration.duration);
//            Assert.IsTrue(poseOverriderDto.interpolationable == result.interpolationable);  
//            Assert.IsTrue(poseOverriderDto.composable == result.composable);
//        }

//        #endregion

//        #region Duration
//        [Test]
//        public void ReadDuration()
//        {
//            DurationDto duration = new DurationDto(
//                duration : 159,
//                min : 5,
//                max : 89486
//            );

//            collabSerializerModule.Write(duration, out Bytable data);


//            ByteContainer byteContainer = new ByteContainer(1, data.ToBytes());

//            collabSerializerModule.Read(byteContainer, out bool readable, out DurationDto result);
//            Assert.IsTrue(readable);

//            Assert.IsTrue(duration.duration == result.duration);    
//            Assert.IsTrue(duration.max == result.max);
//            Assert.IsTrue(duration.min == result.min);
//        }
//        #endregion
//    }
//}

