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
using PlayMode_Tests.UserCapture.Binding.CDK;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using umi3d.cdk;
using umi3d.cdk.binding;
using umi3d.cdk.collaboration;
using umi3d.cdk.collaboration.userCapture;
using umi3d.cdk.collaboration.userCapture.binding;
using umi3d.cdk.userCapture;
using umi3d.common;
using umi3d.common.core;
using umi3d.common.dto.binding;
using umi3d.common.userCapture;
using umi3d.common.userCapture.binding;
using UnityEngine;

namespace PlayMode_Tests.Collaboration.UserCapture.Binding.CDK
{
    [TestFixture, TestOf(nameof(CollaborationBindingLoader))]
    public class CollaborationBindingLoader_Test : UserCaptureBindingLoader_Test
    {
        protected Mock<ICollaborationSkeletonsManager> collaborativeSkeletonManager;


        #region Test SetUp

        [OneTimeSetUp]
        public override void OneTimeSetup()
        {
            base.OneTimeSetup();
            ClearSingletons();
        }

        [SetUp]
        public override void SetUp()
        {
            bindingManagementServiceMock = new();
            skeletonServiceMock = new();
            loadingManagerMock = new();
            environmentManagerMock = new();
            collaborativeSkeletonManager = new();
            bindingLoader = new CollaborationBindingLoader(bindingManagementServiceMock.Object,
                                                           loadingManagerMock.Object,
                                                           environmentManagerMock.Object,
                                                           collaborativeSkeletonManager.Object);
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            ClearSingletons();
        }

        private void ClearSingletons()
        {
            if (UMI3DCollaborationEnvironmentLoader.Exists)
                UMI3DCollaborationEnvironmentLoader.Destroy();

            if (CollaborationSkeletonsManager.Exists)
                CollaborationSkeletonsManager.Destroy();
        }

        #endregion Test SetUp

        #region ReadUMI3DExtension

        [Test]
        public override void ReadUMI3DExtension_MultiBinding()
        {
            // GIVEN
            ulong environmentId = UMI3DGlobalID.EnvironmentId;
            uint targetBoneType = BoneType.Chest;
            ulong userId = 2005uL;
            var dto = new BindingDto()
            {
                id = 1005uL,
                boundNodeId = 1008uL,
                data = new MultiBindingDataDto()
                {
                    Bindings = new AbstractSimpleBindingDataDto[] { new NodeBindingDataDto() { parentNodeId = 1008uL },
                                                                                                   new BoneBindingDataDto() { userId = userId, boneType = targetBoneType } }
                }
            };

            var extensionData = new ReadUMI3DExtensionData(environmentId, dto);

            var entityFake = new UMI3DEntityInstance(environmentId, () => { }, dto.id);
            var nodeMock = new Mock<UMI3DNodeInstance>(environmentId, new System.Action(() => { }), dto.boundNodeId);
            var nodeGameObject = new GameObject("Node to bind");
            nodeMock.Setup(x => x.transform).Returns(nodeGameObject.transform);

            var skeletonMock = new Mock<ISkeleton>();
            skeletonMock.Setup(x => x.Bones).Returns(new Dictionary<uint, ITransformation>() { { targetBoneType, new PureTransformation() } });
            
            loadingManagerMock.Setup(x => x.WaitUntilNodeInstanceLoaded(environmentId, dto.boundNodeId, null)).Returns(Task.FromResult(nodeMock.Object));
            environmentManagerMock.Setup(x => x.RegisterEntity(environmentId, dto.id, dto, null, It.IsAny<System.Action>())).Returns(entityFake);
            environmentManagerMock.Setup(x => x.GetNodeInstance(environmentId, dto.boundNodeId)).Returns(nodeMock.Object);

            bindingManagementServiceMock.Setup(x => x.AddBinding(environmentId, dto.boundNodeId, It.IsAny<AbstractBinding>()));

            collaborativeSkeletonManager.Setup(x => x.WaitForSkeleton(environmentId, userId, It.IsAny<List<CancellationToken>>())).Returns(Task.FromResult(skeletonMock.Object));

            // WHEN
            Task.Run(() => bindingLoader.ReadUMI3DExtension(extensionData)).Wait();

            // THEN
            environmentManagerMock.Verify(x => x.RegisterEntity(environmentId, dto.id, dto, null, It.IsAny<System.Action>()));
            bindingManagementServiceMock.Verify(x => x.AddBinding(environmentId, dto.boundNodeId, It.IsAny<AbstractBinding>()));

            // teardown
            Object.Destroy(nodeGameObject);
        }

        [Test]
        public override void ReadUMI3DExtension_NodeBinding()
        {
            // GIVEN
            ulong environmentId = UMI3DGlobalID.EnvironmentId;
            var dto = new BindingDto()
            {
                id = 1005uL,
                boundNodeId = 1008uL,
                data = new NodeBindingDataDto() { parentNodeId = 1008uL }
            };

            var extensionData = new ReadUMI3DExtensionData(environmentId, dto);

            var entityFake = new UMI3DEntityInstance(environmentId, () => { }, dto.id);
            var nodeMock = new Mock<UMI3DNodeInstance>(environmentId, new System.Action(() => { }), dto.boundNodeId);
            var nodeGameObject = new GameObject("Node to bind");
            nodeMock.Setup(x => x.transform).Returns(nodeGameObject.transform);

            loadingManagerMock.Setup(x => x.WaitUntilNodeInstanceLoaded(environmentId, dto.boundNodeId, null)).Returns(Task.FromResult(nodeMock.Object));
            environmentManagerMock.Setup(x => x.RegisterEntity(environmentId, dto.id, dto, null, It.IsAny<System.Action>())).Returns(entityFake);
            environmentManagerMock.Setup(x => x.GetNodeInstance(environmentId, dto.boundNodeId)).Returns(nodeMock.Object);

            bindingManagementServiceMock.Setup(x => x.AddBinding(environmentId, dto.boundNodeId, It.IsAny<AbstractBinding>()));

            // WHEN
            Task.Run(() => bindingLoader.ReadUMI3DExtension(extensionData)).Wait();

            // THEN
            environmentManagerMock.Verify(x => x.RegisterEntity(environmentId, dto.id, dto, null, It.IsAny<System.Action>()));
            bindingManagementServiceMock.Verify(x => x.AddBinding(environmentId, dto.boundNodeId, It.IsAny<AbstractBinding>()));

            // teardown
            Object.Destroy(nodeGameObject);
        }

        [Test]
        public override void ReadUMI3DExtension_BoneBinding()
        {
            // GIVEN
            ulong environmentId = UMI3DGlobalID.EnvironmentId;
            uint targetBoneType = BoneType.Chest;
            ulong userId = 2005uL;
            var dto = new BindingDto()
            {
                id = 1005uL,
                boundNodeId = 1008uL,
                data = new BoneBindingDataDto() { userId = userId, boneType = targetBoneType }
            };

            var extensionData = new ReadUMI3DExtensionData(environmentId, dto);

            var entityFake = new UMI3DEntityInstance(environmentId, () => { }, dto.id);
            var nodeMock = new Mock<UMI3DNodeInstance>(MockBehavior.Default, environmentId, new System.Action(() => { }), dto.boundNodeId);
            var nodeGameObject = new GameObject("Node to bind");
            nodeMock.Setup(x => x.transform).Returns(nodeGameObject.transform);

            var skeletonMock = new Mock<ISkeleton>();
            skeletonMock.Setup(x => x.Bones).Returns(new Dictionary<uint, ITransformation>() { { targetBoneType, new PureTransformation() } });

            loadingManagerMock.Setup(x => x.WaitUntilNodeInstanceLoaded(environmentId, dto.boundNodeId, null)).Returns(Task.FromResult(nodeMock.Object));
            environmentManagerMock.Setup(x => x.RegisterEntity(environmentId, dto.id, dto, null, It.IsAny<System.Action>())).Returns(entityFake);
            environmentManagerMock.Setup(x => x.GetNodeInstance(environmentId, dto.boundNodeId)).Returns(nodeMock.Object); ;

            bindingManagementServiceMock.Setup(x => x.AddBinding(environmentId, dto.boundNodeId, It.IsAny<AbstractBinding>()));

            collaborativeSkeletonManager.Setup(x => x.WaitForSkeleton(environmentId, userId, It.IsAny<List<CancellationToken>>())).Returns(Task.FromResult(skeletonMock.Object));

            // WHEN
            Task.Run(() => bindingLoader.ReadUMI3DExtension(extensionData)).Wait();

            // THEN
            environmentManagerMock.Verify(x => x.RegisterEntity(environmentId, dto.id, dto, null, It.IsAny<System.Action>()));
            bindingManagementServiceMock.Verify(x => x.AddBinding(environmentId, dto.boundNodeId, It.IsAny<AbstractBinding>()));

            // teardown
            Object.Destroy(nodeGameObject);
        }

        #endregion ReadUMI3DExtension
    }
}