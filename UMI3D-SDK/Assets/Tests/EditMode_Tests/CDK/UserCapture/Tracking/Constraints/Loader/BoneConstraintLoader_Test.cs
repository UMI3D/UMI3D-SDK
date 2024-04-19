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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestUtils;
using umi3d.cdk;
using umi3d.cdk.userCapture;
using umi3d.cdk.userCapture.tracking.constraint;
using umi3d.common;
using umi3d.common.userCapture;
using umi3d.common.userCapture.tracking.constraint;
using UnityEngine;

namespace EditMode_Tests.UserCapture.Tracking.Constraint.CDK
{
    [TestFixture, TestOf(nameof(BoneConstraintLoader))]
    public class BoneConstraintLoader_Test
    {
        private Mock<IEnvironmentManager> environmentServiceMock;
        private Mock<ILoadingManager> loadingServiceMock;
        private Mock<ISkeletonManager> skeletonServiceMock;
        private Mock<ISkeletonConstraintService> boneConstraintServiceMock;

        private BoneConstraintLoader boneConstraintLoader;

        #region SetUp

        [SetUp]
        public void Setup()
        {
            environmentServiceMock = new();
            loadingServiceMock = new();
            skeletonServiceMock = new();
            boneConstraintServiceMock = new();

            boneConstraintLoader = new BoneConstraintLoader(environmentServiceMock.Object, loadingServiceMock.Object, skeletonServiceMock.Object, boneConstraintServiceMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
        }

        #endregion SetUp

        #region Load

        [Test]
        public void Load_Null()
        {
            // Given
            AbstractBoneConstraintDto overriderDto = null;

            // When
            TestDelegate action = () => boneConstraintLoader.Load(UMI3DGlobalID.EnvironmentId, overriderDto).Wait();

            // Then
            Assert.Throws<AggregateException>(() => action());
        }

        [Test, TestOf(nameof(BoneConstraintLoader.Load))]
        public void Load_NodeBoneConstraint()
        {
            // Given
            ulong environmentId = 1523uL;
            NodeBoneConstraintDto dto = new NodeBoneConstraintDto()
            {
                id = 1589uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingNodeId = 1596uL,
                ShouldBeApplied = true,
                PositionOffset = Vector3.one.Dto(),
                RotationOffset = Quaternion.Euler(Vector3.one).Dto()
            };

            UMI3DNodeInstance instance = new UMI3DNodeInstance(environmentId, () => { }, dto.ConstrainingNodeId);

            loadingServiceMock.Setup(x => x.WaitUntilNodeInstanceLoaded(environmentId, dto.ConstrainingNodeId, null)).Returns(Task.FromResult(instance));

            boneConstraintServiceMock.Setup(x => x.RegisterConstraint(It.IsAny<NodeBoneConstraint>())).Verifiable();

            environmentServiceMock.Setup(x => x.RegisterEntity(environmentId, dto.id, dto, It.IsAny<NodeBoneConstraint>(), It.IsAny<Action>()))
                                  .Returns(new UMI3DEntityInstance(environmentId, () => { }, dto.id))
                                  .Verifiable();

            // When
            Task<AbstractBoneConstraint> task = boneConstraintLoader.Load(environmentId, dto);
            task.Wait();
            AbstractBoneConstraint constraint = task.Result;

            // Then
            boneConstraintServiceMock.Verify(x => x.RegisterConstraint(It.IsAny<AbstractBoneConstraint>()), Times.Once);
            environmentServiceMock.Verify(x => x.RegisterEntity(environmentId, dto.id, dto, It.IsAny<AbstractBoneConstraint>(), It.IsAny<Action>()), Times.Once);

            Assert.IsNotNull(constraint);
            Assert.AreEqual(dto.id, constraint.Id);
            Assert.AreEqual(dto.ConstrainedBone, constraint.ConstrainedBone);
            Assert.AreEqual(dto.ShouldBeApplied, constraint.ShouldBeApplied);
            AssertUnityStruct.AreEqual(dto.PositionOffset.Struct(), constraint.PositionOffset);
            AssertUnityStruct.AreEqual(dto.RotationOffset.Quaternion(), constraint.RotationOffset);

            NodeBoneConstraint nodeBoneConstraint = (NodeBoneConstraint)constraint;
            Assert.AreEqual(dto.ConstrainingNodeId, nodeBoneConstraint.ConstrainingNode.Id);
        }

        [Test]
        public void Load_BoneBoneConstraint()
        {
            // Given
            ulong environmentId = 1523uL;
            BoneBoneConstraintDto dto = new BoneBoneConstraintDto()
            {
                id = 1589uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingBone = BoneType.Head,
                ShouldBeApplied = true,
                PositionOffset = Vector3.one.Dto(),
                RotationOffset = Quaternion.Euler(Vector3.one).Dto()
            };

            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            personalSkeletonMock.Setup(x => x.Bones).Returns(new Dictionary<uint, ISkeleton.Transformation>() { { dto.ConstrainingBone, new() } });

            skeletonServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);

            boneConstraintServiceMock.Setup(x => x.RegisterConstraint(It.IsAny<BoneBoneConstraint>())).Verifiable();

            environmentServiceMock.Setup(x => x.RegisterEntity(environmentId, dto.id, dto, It.IsAny<BoneBoneConstraint>(), It.IsAny<Action>()))
                                  .Returns(new UMI3DEntityInstance(environmentId, () => { }, dto.id))
                                  .Verifiable();

            // When
            AbstractBoneConstraint constraint = Task.Run(() => boneConstraintLoader.Load(environmentId, dto)).Result;

            // Then
            boneConstraintServiceMock.Verify(x => x.RegisterConstraint(It.IsAny<BoneBoneConstraint>()), Times.Once);
            environmentServiceMock.Verify(x => x.RegisterEntity(environmentId, dto.id, dto, It.IsAny<BoneBoneConstraint>(), It.IsAny<Action>()), Times.Once);

            Assert.IsNotNull(constraint);
            Assert.AreEqual(dto.id, constraint.Id);
            Assert.AreEqual(dto.ConstrainedBone, constraint.ConstrainedBone);
            Assert.AreEqual(dto.ShouldBeApplied, constraint.ShouldBeApplied);
            AssertUnityStruct.AreEqual(dto.PositionOffset.Struct(), constraint.PositionOffset);
            AssertUnityStruct.AreEqual(dto.RotationOffset.Quaternion(), constraint.RotationOffset);

            BoneBoneConstraint boneBoneConstraint = (BoneBoneConstraint)constraint;
            Assert.AreEqual(dto.ConstrainingBone, boneBoneConstraint.ConstrainingBone);
        }

        #endregion Load

        #region SetUMI3DProperty

        [Test]
        public void SetUMI3DProperty_NotCorrectDto()
        {
            // GIVEN
            ulong environmentId = 1596uL;
            SetEntityPropertyDto setEntityDto = new()
            {
                property = UMI3DPropertyKeys.ActiveEmote,
                value = true
            };

            UMI3DEntityInstance entityInstance = new(environmentId, () => { }, 0)
            {
                dto = new()
            };

            SetUMI3DPropertyData data = new SetUMI3DPropertyData(environmentId, setEntityDto, entityInstance);

            // WHEN
            bool success = Task.Run(() => boneConstraintLoader.SetUMI3DProperty(data)).Result;

            // THEN
            Assert.IsFalse(success);
        }

        [Test]
        public void SetUMI3DProperty_TrackingConstraintIsApplied([Values(true, false)] bool value)
        {
            // GIVEN
            ulong environmentId = 1596uL;
            SetEntityPropertyDto setEntityDto = new()
            {
                property = UMI3DPropertyKeys.TrackingConstraintIsApplied,
                value = value
            };

            NodeBoneConstraintDto dto = new NodeBoneConstraintDto()
            {
                id = 1589uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingNodeId = 1596uL,
                ShouldBeApplied = !value,
                PositionOffset = Vector3.one.Dto(),
                RotationOffset = Quaternion.Euler(Vector3.one).Dto()
            };

            UMI3DNodeInstance nodeInstance = new UMI3DNodeInstance(environmentId, () => { }, dto.ConstrainingNodeId);

            AbstractBoneConstraint boneConstraint = new NodeBoneConstraint(dto, nodeInstance);

            UMI3DEntityInstance entityInstance = new(environmentId, () => { }, boneConstraint.Id)
            {
                dto = dto,
                Object = boneConstraint
            };

            SetUMI3DPropertyData data = new(environmentId, setEntityDto, entityInstance);

            environmentServiceMock.Setup(x => x.TryGetEntity(environmentId, boneConstraint.Id, out boneConstraint)).Returns(true);
            boneConstraintServiceMock.Setup(x => x.UpdateConstraints()).Verifiable();

            // WHEN
            bool success = Task.Run(() => boneConstraintLoader.SetUMI3DProperty(data)).Result;

            // THEN
            Assert.IsTrue(success);
            Mock.Verify();
            Assert.AreEqual(value, boneConstraint.ShouldBeApplied);
        }

        [Test]
        public void SetUMI3DProperty_TrackingConstraintBoneType()
        {
            // GIVEN
            uint value = BoneType.Neck;

            ulong environmentId = 1596uL;
            SetEntityPropertyDto setEntityDto = new()
            {
                property = UMI3DPropertyKeys.TrackingConstraintBoneType,
                value = value
            };

            NodeBoneConstraintDto dto = new NodeBoneConstraintDto()
            {
                id = 1589uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingNodeId = 1596uL,
                ShouldBeApplied = true,
                PositionOffset = Vector3.one.Dto(),
                RotationOffset = Quaternion.Euler(Vector3.one).Dto()
            };

            UMI3DNodeInstance nodeInstance = new UMI3DNodeInstance(environmentId, () => { }, dto.ConstrainingNodeId);

            AbstractBoneConstraint boneConstraint = new NodeBoneConstraint(dto, nodeInstance);

            UMI3DEntityInstance entityInstance = new(environmentId, () => { }, boneConstraint.Id)
            {
                dto = dto,
                Object = boneConstraint
            };

            SetUMI3DPropertyData data = new(environmentId, setEntityDto, entityInstance);

            environmentServiceMock.Setup(x => x.TryGetEntity(environmentId, boneConstraint.Id, out boneConstraint)).Returns(true);
            boneConstraintServiceMock.Setup(x => x.UpdateConstraints()).Verifiable();

            // WHEN
            bool success = Task.Run(() => boneConstraintLoader.SetUMI3DProperty(data)).Result;

            // THEN
            Assert.IsTrue(success);
            Mock.Verify();
            Assert.AreEqual(value, boneConstraint.ConstrainedBone);
        }

        [Test]
        public void SetUMI3DProperty_TrackingConstraintConstrainingNode()
        {
            // GIVEN
            UMI3DNodeDto value = new() { id = 15615uL };

            ulong environmentId = 1596uL;
            SetEntityPropertyDto setEntityDto = new()
            {
                property = UMI3DPropertyKeys.TrackingConstraintConstrainingNode,
                value = value
            };

            NodeBoneConstraintDto dto = new NodeBoneConstraintDto()
            {
                id = 1589uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingNodeId = 1596uL,
                ShouldBeApplied = true,
                PositionOffset = Vector3.one.Dto(),
                RotationOffset = Quaternion.Euler(Vector3.one).Dto()
            };

            UMI3DNodeInstance nodeInstance = new UMI3DNodeInstance(environmentId, () => { }, dto.ConstrainingNodeId);

            NodeBoneConstraint nodeBoneConstraint = new(dto, nodeInstance);

            UMI3DEntityInstance entityInstance = new(environmentId, () => { }, nodeBoneConstraint.Id)
            {
                dto = dto,
                Object = nodeBoneConstraint
            };

            SetUMI3DPropertyData data = new(environmentId, setEntityDto, entityInstance);

            environmentServiceMock.Setup(x => x.TryGetEntity(environmentId, nodeBoneConstraint.Id, out nodeBoneConstraint)).Returns(true);

            UMI3DNodeInstance newNodeInstance = new UMI3DNodeInstance(environmentId, () => { }, value.id);
            environmentServiceMock.Setup(x => x.GetNodeInstance(environmentId, value.id)).Returns(newNodeInstance);

            // WHEN
            bool success = Task.Run(() => boneConstraintLoader.SetUMI3DProperty(data)).Result;

            // THEN
            Assert.IsTrue(success);
            Assert.AreEqual(value.id, nodeBoneConstraint.ConstrainingNode.Id);
        }

        [Test]
        public void SetUMI3DProperty_TrackingConstraintConstrainingBone()
        {
            // GIVEN
            uint value = BoneType.LeftAnkle;

            ulong environmentId = 1596uL;
            SetEntityPropertyDto setEntityDto = new()
            {
                property = UMI3DPropertyKeys.TrackingConstraintConstrainingBone,
                value = value
            };

            BoneBoneConstraintDto dto = new BoneBoneConstraintDto()
            {
                id = 1589uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingBone = BoneType.Head,
                ShouldBeApplied = true,
                PositionOffset = Vector3.one.Dto(),
                RotationOffset = Quaternion.Euler(Vector3.one).Dto()
            };

            BoneBoneConstraint boneBoneConstraint = new(dto, new());

            UMI3DEntityInstance entityInstance = new(environmentId, () => { }, boneBoneConstraint.Id)
            {
                dto = dto,
                Object = boneBoneConstraint
            };

            SetUMI3DPropertyData data = new(environmentId, setEntityDto, entityInstance);

            environmentServiceMock.Setup(x => x.TryGetEntity(environmentId, boneBoneConstraint.Id, out boneBoneConstraint)).Returns(true);

            var personalSkeletonMock = new Mock<IPersonalSkeleton>();
            personalSkeletonMock.Setup(x => x.Bones).Returns(new Dictionary<uint, ISkeleton.Transformation>() { { value, new() } });

            skeletonServiceMock.Setup(x => x.PersonalSkeleton).Returns(personalSkeletonMock.Object);

            // WHEN
            bool success = Task.Run(() => boneConstraintLoader.SetUMI3DProperty(data)).Result;

            // THEN
            Assert.IsTrue(success);
            Assert.AreEqual(value, boneBoneConstraint.ConstrainingBone);
        }

        [Test]
        public void SetUMI3DProperty_TrackingConstraintPositionOffset()
        {
            // GIVEN
            Vector3Dto value = (2 * Vector3.one).Dto();

            ulong environmentId = 1596uL;
            SetEntityPropertyDto setEntityDto = new()
            {
                property = UMI3DPropertyKeys.TrackingConstraintPositionOffset,
                value = value
            };

            NodeBoneConstraintDto dto = new NodeBoneConstraintDto()
            {
                id = 1589uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingNodeId = 1596uL,
                ShouldBeApplied = true,
                PositionOffset = Vector3.one.Dto(),
                RotationOffset = Quaternion.Euler(Vector3.one).Dto()
            };

            UMI3DNodeInstance nodeInstance = new UMI3DNodeInstance(environmentId, () => { }, dto.ConstrainingNodeId);

            AbstractBoneConstraint boneConstraint = new NodeBoneConstraint(dto, nodeInstance);

            UMI3DEntityInstance entityInstance = new(environmentId, () => { }, boneConstraint.Id)
            {
                dto = dto,
                Object = boneConstraint
            };

            SetUMI3DPropertyData data = new(environmentId, setEntityDto, entityInstance);

            environmentServiceMock.Setup(x => x.TryGetEntity(environmentId, boneConstraint.Id, out boneConstraint)).Returns(true);

            // WHEN
            bool success = Task.Run(() => boneConstraintLoader.SetUMI3DProperty(data)).Result;

            // THEN
            Assert.IsTrue(success);
            AssertUnityStruct.AreEqual(value.Struct(), boneConstraint.PositionOffset);
        }

        [Test]
        public void SetUMI3DProperty_TrackingConstraintRotationOffset()
        {
            // GIVEN
            Vector4Dto value = Quaternion.Euler(2 * Vector3.one).Dto();

            ulong environmentId = 1596uL;
            SetEntityPropertyDto setEntityDto = new()
            {
                property = UMI3DPropertyKeys.TrackingConstraintRotationOffset,
                value = value
            };

            NodeBoneConstraintDto dto = new NodeBoneConstraintDto()
            {
                id = 1589uL,
                ConstrainedBone = BoneType.Chest,
                ConstrainingNodeId = 1596uL,
                ShouldBeApplied = true,
                PositionOffset = Vector3.one.Dto(),
                RotationOffset = Quaternion.Euler(Vector3.one).Dto()
            };

            UMI3DNodeInstance nodeInstance = new UMI3DNodeInstance(environmentId, () => { }, dto.ConstrainingNodeId);

            AbstractBoneConstraint boneConstraint = new NodeBoneConstraint(dto, nodeInstance);

            UMI3DEntityInstance entityInstance = new(environmentId, () => { }, boneConstraint.Id)
            {
                dto = dto,
                Object = boneConstraint
            };

            SetUMI3DPropertyData data = new(environmentId, setEntityDto, entityInstance);

            environmentServiceMock.Setup(x => x.TryGetEntity(environmentId, boneConstraint.Id, out boneConstraint)).Returns(true);

            // WHEN
            bool success = Task.Run(() => boneConstraintLoader.SetUMI3DProperty(data)).Result;

            // THEN
            Assert.IsTrue(success);
            AssertUnityStruct.AreEqual(value.Quaternion(), boneConstraint.RotationOffset);
        }

        #endregion SetUMI3DProperty
    }
}